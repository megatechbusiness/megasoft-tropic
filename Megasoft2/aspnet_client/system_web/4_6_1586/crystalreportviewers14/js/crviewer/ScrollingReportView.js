/* Copyright (c) Business Objects 2006. All rights reserved. */

/**
 * ScrollingReportView constructor
 */

bobj.crv.newScrollingReportView = function(kwArgs) {
    kwArgs = MochiKit.Base.update( {
        id : bobj.uniqueId() + "_scrollingReportView",
        viewStateId : null,
        printLayout : true,
        isMainReport : false,
        isLastPageNumberKnown : false,
        lastPageNumber : 1,
        groupNamePath : "",
        zoomLevel : 100
    }, kwArgs);

    var o = new bobj.crv.ScrollingReportView(kwArgs.id, kwArgs.printLayout, kwArgs.isMainReport, kwArgs.lastPageNumber, kwArgs.isLastPageNumberKnown, kwArgs.groupNamePath, kwArgs.zoomLevel);
    
    return o;
};


bobj.crv.ScrollingReportView = function(id, isPrintLayout, isMainReport, lastPageNumber, isLastPageNumberKnown, groupNamePath, zoomLevel) {
    this.isMainReportFlag = isMainReport;
    this.printLayout = isPrintLayout;
    this.id = id;
    
    this.widgetType = 'ScrollingReportView';
    this.pages = [];
    this.initChildren = [];
    this.lastPageNumber = lastPageNumber;
    this.isLastPageNumberKnown = isLastPageNumberKnown;
    this.groupNamePath = groupNamePath;
    this.zoomLevel = zoomLevel;
    this.zoomChangeFactor = 1; // ChangeFactor varies between 40 and 1/40 for current minZoom and maxZoom levels
    this.topOffset = -1;
    this.leftOffset = -1;
    this.vsbar = new bobj.crv.VerticalScrollBar();
    this.hsbar = new bobj.crv.HorizontalScrollBar();
    this.displayHsbar = false;
    this.displayVsbar = false;

    this.SCROLL_DOWN_PER_UNIT = 16; // move by 16 pixel for up/down arrow in scrollbar or one tick of a mouse wheel
    this.LOAD_MARGIN = 500; // load a new page if no pages exist in the panel's load margin
    this.UNLOAD_MARGIN = 2500; // unload any pages that goes beyond panel's unload margin
    this.SCROLLBAR_SIZE = 16; // width/height of the scrollbars and the arrow buttons
    this.PAGES_BELOW_LAST = 1; // assume 'this' many pages exists below the last known page number when last page is unknown
    this.MAX_ABSOLUTE_DELTA = 12;
    this.containerOffsetTop = 0;
    this.panelHeight = 0;
    this.panelWidth = 0;
    this.loadPageIntervalMap = {}; /*number -> interval id*/
    this.isPageLoadInProgress = false;  // prevent incomplete page updates if loading other pages
    this.sizeOfLastLoadedPage = {w : 720, h : 984};
    this.scrollingStartPage = -1;
    
    var bind = bobj.bindFunctionToObject;
    this._canvasListeners = [];
    this._mouseWheelCallback = bind(this._onScrollListener, this);
    this._onKeypressCB = bind(this._onKeyPress, this);
    this._panelScrollListener = bind(this._panelScrollListener, this);
    this._viewListener = new bobj.crv.ReportViewListener(this);
    this.widx = _widgets.length;
    this._hasRequestedLastPageNumber = false;
    _widgets.push(this);
};


bobj.crv.ScrollingReportView.prototype = {
    init : function() {
        Widget_init.apply(this);

        this._panelNode = getLayer(this.id + '_panel');
        this._hsbarNode = getLayer(this.id + '_hsbar');
        this._ctrNode = getLayer(this.id + '_ctr');

        this.vsbar.init();
        var bind = MochiKit.Base.bind;
        
        this.vsbar.setCallBacks(bind(this._moveDown, this, this.SCROLL_DOWN_PER_UNIT), bind(this._moveUp, this, this.SCROLL_DOWN_PER_UNIT),
                bind(this._onThumbMoveListener, this));
        this.vsbar.setTooltipGenerator(bind(this._getPageNumForScrollPos, this));

        this.hsbar.init();
        this.hsbar.setScrollableElement(this._ctrNode);

        bobj.connectMouseWheelListener (this.layer, this._mouseWheelCallback);
        
        
        while(this.initChildren.length > 0)
            this._addPage(this.initChildren.pop(), false);

        this._fillEmptySpaceWithPages();
        
        this._findLastPageNumberIfNecessary();
    },
        
    getCanvasListeners : function () {
        return this._canvasListeners;
    },
    
    _findLastPageNumberIfNecessary : function () {
        var firstVisible = this._getFirstVisiblePage();
        if(firstVisible != null && firstVisible.isMissingTotalPageCount() && !this._hasRequestedLastPageNumber) {
            this._hasRequestedLastPageNumber = true;
            MochiKit.Signal.signal(this, 'findLastPageNumber');
        }
    },
    
    addCanvasListener : function (listener) {
        var hasIdentical = MochiKit.Base.findIdentical(this._canvasListeners, listener) >= 0;
        if(!hasIdentical) {
            this._canvasListeners.push(listener);
            for (var i = 0, len = this.pages.length; i < len; i++)
                this.pages[i].addCanvasListener(listener);
        }
    },
    
    removeCanvasListener : function (listener) {
        var index = MochiKit.Base.findIdentical(this._canvasListeners, listener);
        if(index >= 0) {
            this._canvasListeners.splice(index, 1);
            for (var i = 0, len = this.pages.length; i < len; i++)
                this.pages[i].removeCanvasListener(listener);
        }
    },
    
    _panelScrollListener : function(ev) {
        if(this._panelNode.scrollTop > 0) {
        	var scrollBy = Math.ceil(this._panelNode.scrollTop / this.SCROLL_DOWN_PER_UNIT);
        	this._moveUp(scrollBy * this.SCROLL_DOWN_PER_UNIT, false);
            this._panelNode.scrollTop = 0;
        }
    },
    
    addChild : function(widget) {
        if (widget.widgetType == 'ScrollingReportPage') {
            this.initChildren.push(widget);
            this.sizeOfLastLoadedPage = {w : widget.width, h : widget.height};  
        }
    },
    
    _onKeyPress : function(ev) {
        var stopEventPropagation = false;

        switch(ev.keyCode) {
        case KEY_PAGEUP:
            this.gotoPrevPage();
            stopEventPropagation = true;
            break;
        case KEY_PAGEDOWN:
            this.gotoNextPage();
            stopEventPropagation = true;
            break;
        case KEY_END:
            this.gotoLastPage();
            stopEventPropagation = true;
            break;
        case KEY_HOME:
            this.gotoFirstPage();  
            stopEventPropagation = true;
            break;
        case KEY_LEFT:
            if(this.displayHsbar) {
                this.hsbar.scrollLeft();
            }
            stopEventPropagation = true;
            break;
        case KEY_UP:
            this._moveDown(this.SCROLL_DOWN_PER_UNIT, false);
            stopEventPropagation = true;
            break;
        case KEY_RIGHT:
            if(this.displayHsbar) {
                this.hsbar.scrollRight();
            }
            stopEventPropagation = true;
            break;
        case KEY_DOWN:
            this._moveUp(this.SCROLL_DOWN_PER_UNIT, false);
            stopEventPropagation = true;
            break;
        case 117:
            MochiKit.Signal.signal(this, 'moveFocusGroup', !ev.shiftKey);
            stopEventPropagation = true;
            break;
        }
        
        if(stopEventPropagation) {
            (new MochiKit.Signal.Event(this, ev)).stop();
        }
    },
    
    focusFirstChild : function () {
        var page = this._getFirstVisiblePage() || this._getFirstPageInCache();
        if(page) {
            return page.focusFirstChild();
        }
        
        return false;
    },

    isMainReport : function() {
        return this.isMainReportFlag;
    },
    
    handlePageNavigationEvent: function() {
        var eventName = arguments[0];
        if (!eventName)
            return;
        
        switch(eventName) {
        case 'nextPage':
            this.gotoNextPage();
            break;
        case 'prevPage':
            this.gotoPrevPage();
            break;
        case 'firstPage':
            this.gotoFirstPage();
            break;
        case 'lastPage':
            this.gotoLastPage();
            break;
        case 'selectPage':
            this.gotoSelectPage(arguments[1]);
            break;
        }
    },
    
    gotoNextPage : function () {
        var firstVisiblePage = this._getFirstVisiblePage();
        if (firstVisiblePage) {
            var nextPageNumber = firstVisiblePage.getNumber() + 1;
            this.gotoPage(nextPageNumber);
        }
    },
    
    gotoPrevPage : function () {
        var firstVisiblePage = this._getFirstVisiblePage();
        if (firstVisiblePage) {
            var prevPage = firstVisiblePage.getNumber() - 1;
            if (prevPage > 0)
                this.gotoPage(prevPage);
        }
    },
    
    gotoFirstPage : function () {
        this.gotoPage(1);
    },
    
    gotoLastPage : function () {
        this._clearAllLoadPageSignals();
        if (this.isLastPageNumberKnown)
            this.gotoPage(this.lastPageNumber);
        else
            MochiKit.Signal.signal(this, 'lastPage', true, true);
    },
    
    gotoSelectPage : function (page) {
        this.gotoPage(page);
    },
    
    gotoPage : function(pageNumber) {
        if (this.isLastPageNumberKnown && pageNumber > this.lastPageNumber)
            this.gotoPage(this.lastPageNumber);
        
        var currentPage = this._getCurrentPageNumber();
        if (currentPage > 0) {
            if (pageNumber > currentPage)
                this._updateCache(true);
            else
                this._updateCache(false);
        }
        
        if (this._isPageInCacheAndLoaded(pageNumber)) {
            this._displayPageAtTop(pageNumber);
        }
        else {
            this._signalLoadPage(pageNumber, true, this._isAllowIncompletePageRequest());
        }
        
        this._signalUpdateCurrentPage(pageNumber);
    },
    
    
    _isAllowIncompletePageRequest : function () {
        var lastLoadedPageInCache = this._getLastLoadedPageInCache();
        /**
         * if last loaded page in browser cache is not missing total page count, then we should ask for complete page in future requests
         * such that cacheserver has correct information about cached pages
         */
        if(lastLoadedPageInCache != null && !lastLoadedPageInCache.isMissingTotalPageCount())
            return false;
        /*allow incomplete pages requests when last page is not known for Java Reports*/
        return !this.isLastPageNumberKnown;
    },
    

    _getCurrentPageNumber : function () {
        var firstVisiblePage = this._getFirstVisiblePage();
        if (firstVisiblePage)
            return firstVisiblePage.getNumber();
        
        return -1;
    },
    
    _isPageInCacheAndLoaded : function(pageNumber) {
        return this._isPageInCache(pageNumber) && this.loadPageIntervalMap[pageNumber] == undefined;
    },
    
    /*
     * Add a dashed border to every page except the last page, when in web layout
     */
    _addPageBorder : function(page) {
        if (!this.printLayout
                && !(this.isLastPageNumberKnown && page.pageNum == this.lastPageNumber))
            page.addDashedBottomBorder();
    },
    
    update : function(update) {
        if (update && update.cons == "bobj.crv.newScrollingReportView") {
            if (update.args) {
                if (update.args.isClearCachedPages) {
                    this._unloadAllPages();
                    this.lastPageNumber = 1;
                    this.isLastPageNumberKnown = false;
                    this.viewStateId = update.args.viewStateId;
                    this._hasRequestedLastPageNumber = false; //context changed or new Processing Job was creating
                }
                else {
                    if(this.groupNamePath != update.args.groupNamePath || this.zoomLevel != update.args.zoomLevel) {
                        /* if update contains a page from another groupNamepath or has different zoom level and has isClearCachedPages as false, then
                         * then it contains an outdated response.
                         * It happens when response for fetching complete page (with totalpage count) takes long time and user navigates to another view beforehand.
                         */
                        return;
                     }
                }
                
                
                if (!this.isLastPageNumberKnown) {
                    var oldLastPage = this.lastPageNumber;
                    var pagination = update.args.separatePages;
                    this.lastPageNumber = pagination ? update.args.lastPageNumber : 1;
                    this.isLastPageNumberKnown = pagination ? update.args.isLastPageNumberKnown : true;
                    if (oldLastPage != this.lastPageNumber && this.displayVsbar)
                        this._adjustThumbSize();
                }
                
                this.groupNamePath = update.args.groupNamePath;
                var oldZoom = this.zoomLevel;
                var newZoom = update.args.zoomLevel;      
                this.zoomChangeFactor = newZoom / oldZoom;
                this.zoomLevel = newZoom;
                
                this._signalUpdateLastPage(this.lastPageNumber, this.isLastPageNumberKnown);
            }

            if (update.children) {
                var child = update.children[0];
                this.sizeOfLastLoadedPage = {w : child.args.width, h : child.args.height};
                if (child && child.cons == "bobj.crv.newScrollingReportPage") {
                    this.isPageLoadInProgress = false;  // allow any other incomplete pages to reload
                    var pageNum = child.args.pageNum;
                    var page = this._getPageFromCache(pageNum);
                    this._clearLoadPageSignal(pageNum);
                    if (page == null) {
                        page = bobj.crv.createWidget(child);
                        this._addPage(page, true);
                        this._logPages("Added page " + page.getNumber());
                        if(pageNum == this.lastPageNumber)
                            this._loadPage(pageNum - 1, false); /*pre load lastPage-1*/
                    } else {
                        page.update(child);
                        this._logPages("Updated page " + page.getNumber());                        
                        
                        var firstVisiblePage = this._getFirstVisiblePage();
                        if(firstVisiblePage == null) {
                            this._displayPageAtTop(page.getNumber());
                            bobj.logToConsole("HACK2 _displayPageAtTop " + page.getNumber());
                        }

                        this._addPageListeners(page);
                        
                    }
                    this._addPageBorder(page);
                }
            }
            
            if (update.args) {
                var pageAtTop = update.args.pageDisplayedAtTop;
                if (pageAtTop > 0) {
                    var currentPageNumber = this._getCurrentPageNumber();
                    this._displayPageAtTop(pageAtTop);
                    if (this._getCurrentPageNumber() < pageAtTop)
                        this._updateCache(true);
                    
                   var page = this._getPageFromCache(pageAtTop);
                   if (page)
                        var hlCoord = page.getHighlightedElementCoordinates();
                   if (hlCoord) {
                       // align top highlighted object with panel
                       // vertical alignment
                       this._setContainerOffsetTop(this._getContainerOffsetTop() - hlCoord.y);
                       // horizontal alignment: just set the leftOffset, actual movement will be done by doLayout() function 
                       if(this.displayHsbar)
                           this.hsbar._leftOffset = hlCoord.x;
                   }
                }
            }

            this._fillEmptySpaceWithPages();

            if (this.isLastPageNumberKnown) {
                var isAdjustScrollbar = false;
                /** Can be removed once advance page loading algorithm is added*/
                for (var i = this.pages.length -1; i >= 0; i--) {
                    if (this.pages[i].getNumber() > this.lastPageNumber)  {
                        this._unloadPage(this.pages[i]);
                        isAdjustScrollbar = true;
                    }
                }
                
                if (isAdjustScrollbar && this.displayVsbar) {
                    this._adjustThumbSize();
                    this._adjustThumbPos();
                }
                
                // If we know the total page count, we can start updating any incomplete pages.
                this._signalUpdateIncompletePages();
            }
            
            this._findLastPageNumberIfNecessary();
        }        
    },

    pushLastPageNumber : function(lastPageNumber, groupNamePath) {
        // Make sure the update is for this particular view.
        if (this.groupNamePath != groupNamePath)
            return;
        
        // Update the last page number.
        this.lastPageNumber = lastPageNumber;
        this.isLastPageNumberKnown = true;
          
        // Update other parts of the viewer with the known last page number.
        this._signalUpdateLastPage(this.lastPageNumber, this.isLastPageNumberKnown);
        
        // Now that we know the total page count, we can start updating any incomplete pages.
        this._signalUpdateIncompletePages();
    },
    
    _addPageListeners : function (page) {
        page.addMouseWheelListener(this._mouseWheelCallback);
        page.addKeyPressListener(this._onKeypressCB);
        MochiKit.Iter.forEach(this._canvasListeners, function(l) {
            page.addCanvasListener(l);
        })
    },
    
    _displayPageAtTop : function (pageNumber) {
        var page = this._getPageFromCache(pageNumber);
        if (page) {
            var offset = this._getPageTopOffset(page);
            this._setContainerOffsetTop(-offset);
            this._signalUpdateCurrentPage(pageNumber);
            this._adjustThumbPos();
        }
    },

    /*
     * Returns page number that must be loaded to fill empty space in view.
     * If there is no empty space null is returned
     */
    _getPageToLoadToFillEmptySpace : function() {
        var firstPageVisible = this._getFirstVisiblePage();

        if (firstPageVisible && firstPageVisible.getNumber() > 1
                && this._getPageTopOffset(firstPageVisible) + this._getContainerOffsetTop() > this.LOAD_MARGIN)
            return firstPageVisible.getNumber() - 1;

        var lastVisiblePage = this._getLastVisiblePage();
        var lastPageInCache = this._getLastPageInCache();

        if (this.isLastPageNumberKnown && lastPageInCache && lastPageInCache.getNumber() >= this.lastPageNumber)
            return null;
        else if (lastVisiblePage && lastPageInCache.getNumber() == lastVisiblePage.getNumber()
                && this._getPageTopOffset(lastVisiblePage) + lastVisiblePage.getHeight()
                        - (this._getContainerOffsetTop() - this._getContainerHeight()) > (-1) * this.LOAD_MARGIN)
            return lastVisiblePage.getNumber() + 1;

        return null;
    },

    _getContainerOffsetTop : function() {
        return this.containerOffsetTop;
    },

    _setContainerOffsetTop : function (top) {
        /*this._logPages("Old offset : [" + this.containerOffsetTop + "] new Offset : [" + top + "]");*/
        var newTop = Math.round(top);
        this._ctrNode.style.top = newTop + "px";
        this.containerOffsetTop = newTop;
        /*bobj.logToConsole("At offset [" + top + "], first visible child is " + this._getFirstVisiblePage());*/
    },
    
    _getContainerHeight : function() {
        var height = 0;
        for (var i = 0, len = this.pages.length; i < len; i++)
            height += this.pages[i].getHeight();
        
        return height;
    },
    
    _getPanelHeight : function () {
        return this.panelHeight;
    },
    
    _setPanelHeight : function(height) {
        this._panelNode.style.height = height + 'px';
        this.panelHeight = height;
    },
    
    _getPanelWidth: function () {
        return this.panelWidth;
    },
    
    _setPanelWidth : function(width) {
        this._panelNode.style.width = width + 'px';
        this.panelWidth = width;
    },

    _fillEmptySpaceWithPages : function() {
        var nextPageToLoad = this._getPageToLoadToFillEmptySpace();
        while (nextPageToLoad != null) {
            this._loadPage(nextPageToLoad, false);
            nextPageToLoad = this._getPageToLoadToFillEmptySpace();
        }
    },

    /*
     * ScrollingReportView contains three main DIVs - panel, vertical scrollbar and horizontal scrollbar
     * Panel contains the container DIV which contains the report pages  
     */
    getHTML : function() {
        var h = bobj.html;

        var layerStyle = {
            position : 'relative',
            width : '100%',
            height : '100%',
            overflow : 'hidden',
            'background-color' : '#F0F0F0'
        };

        var panelStyle = {
            position : 'absolute',
            top : '0px',
            overflow : 'hidden'
        };
        
        var ctrStyle = {
            position : 'absolute',
            top : '0px',
            left : '0px',
            display : 'block'
        };

        if (this.printLayout) {
            panelStyle['background-color'] = '#8E8E8E';
            panelStyle['text-align'] = 'center'; /* For page centering in IE quirks mode */
            ctrStyle['margin'] = '0 auto'; /* center the page horizontally - CSS2 */
        }
        else {
            /* Web Layout*/
            panelStyle['background-color'] = '#FFFFFF';
            /* page should appear in left for web layouts */
            ctrStyle['margin'] = '0';
        }

        var hsbarStyle = {
            position : 'absolute',
            bottom : '0px',
            height : this.SCROLLBAR_SIZE + 'px',
            display : 'none'
        };
        
        if (bobj.crv.config.isRTL){
            panelStyle['right'] = '0px';
            hsbarStyle['right'] = '0px';
        }else{
            panelStyle['left'] = '0px';
            hsbarStyle['left'] = '0px';
        }
        
        var html = h.DIV( {
            id : this.id,
            style : layerStyle,
            'class' : 'insetBorder',
            tabIndex : '-1',
            onkeydown : bobj.getExecuteDOMCallbackHTML(this.widx, '_onKeyPress')
        }, h.DIV( {
            id : this.id + '_panel',
            style : panelStyle,
            onscroll : bobj.getExecuteDOMCallbackHTML(this.widx, '_panelScrollListener')
        }, h.DIV( {
            id : this.id + '_ctr',
            style : ctrStyle,
            role : 'main'
        })), this.vsbar.getHTML(), h.DIV( {
            id : this.id + '_hsbar',
            style : hsbarStyle
        }, this.hsbar.getHTML()));

        return html;
    },

    _doLayout : function(w, h) {
        var ctrHeight = this._getContainerHeight();
        var sbarSize = this.SCROLLBAR_SIZE;

        var isBBM = bobj.isBorderBoxModel();
        if (!isBBM) {
            w -= 4;
            h -= 4;
            h = Math.max(0, h);
            w = Math.max(0, w);
            this.layer.style.width = w + 'px';
            this.layer.style.height = h + 'px';
        } else {
            w -= 4;
            h -= 4;
        }

        var ctrWidth = this._ctrNode.offsetWidth;
        this.displayHsbar = (ctrWidth > w);
        this.displayVsbar = (ctrHeight > h) || (this._getOverallCtrTopAndHeight().height > h);

        if (this.displayHsbar)
            h -= sbarSize;

        if (this.displayVsbar)
            w -= sbarSize;

        h = Math.max(0, h);
        w = Math.max(0, w);
        
        this._setPanelHeight(h);
        this._setPanelWidth(w);
        
        this.vsbar.layer.style.display = this.displayVsbar ? 'block' : 'none';
        this.vsbar.layer.style.height = h + 'px';
        
        this._hsbarNode.style.display = this.displayHsbar ? 'block' : 'none';
        this._hsbarNode.style.width = w + 'px';
        
        if (ctrHeight == 0)
            return; // no pages to layout further
        
        if (this.displayHsbar) {
            this.hsbar.adjustForResize();
        }
        else {
            var ctrNewLeft = 0;
            if (this.printLayout) {
                ctrNewLeft = ((w - ctrWidth) / 2);
            }
            this._ctrNode.style.left = ctrNewLeft + 'px';
            //this.hsbar._leftOffset = 0;
        }        

        if (this.displayVsbar) {
            if (_ie && this.vsbar.layer.offsetLeft < 0) { // fix _ie rendering issue
                this.vsbar.layer.style.visibility = 'hidden';
                this.vsbar.layer.style.visibility = 'visible';
            }
            this.vsbar.resize(h);
            this._adjustThumbSize();
            this._adjustThumbPos();

            // fix for IE
            this.vsbar.layer.style.display = 'none';
            this.vsbar.layer.style.display = this.displayVsbar ? 'block' : 'none';
        }
        else {
            var newCtrTop = 0;
            if (this.printLayout) {
                newCtrTop = (h - ctrHeight) / 2;
            }            
            this._setContainerOffsetTop(newCtrTop);
        }
    },

    resize : function(w, h) {
        this._doLayout(w, h);
    },
    
    getBestFitSize : function() {
        var w = 4; //left/right border width
        var h = 4; //top/bottom border width
        
        var pages = this.pages;
        if(pages.length > 1)
            w += this.SCROLLBAR_SIZE;

        var maxPageWidth = 0;
        if(pages.length > 0) {
            for(var i = 0; i < pages.length; i++) {
                maxPageWidth = Math.max(maxPageWidth, pages[i].getBestFitSize ().width);
            }
        }
        
        var pageSize = pages.length > 0 ? pages[0].getBestFitSize () : null;
        if (pageSize) {
            w += maxPageWidth;
            h += pageSize.height;
        }
        else {
            w += this.sizeOfLastLoadedPage.w;
            h += this.sizeOfLastLoadedPage.h;
        }

        return {
            width : w,
            height : h
        };
    },

    /* TODO Delete this function and its references */
    _logPages : function(msg) {
        var s = null;
        for (var i = 0; i < this.pages.length; i++) {
            if (s != null)
                s += ',';
            s += this.pages[i].getNumber();
        }
        
       if (this.pages.length == 0)
            return;
       bobj.logToConsole(msg + ' - Current pages : ' + s);
    },

    scrollTimeStampArray : [],
    
    /**
     * @param e [Scroll Event]
     * Scrolling events are executed very rapidly in Safari causing viewer to crash. To prevent this behavior, timestamp of all events are stored in an array,
     * and if time difference between current event and third event from last is less than 100 msec, event is ignored. 
     */
    _isExecuteScrollEvent : function (e) {
        var currTimeStamp = e.timeStamp;
        var array = this.scrollTimeStampArray;
        
        if(array.length > 0 && currTimeStamp - array[array.length -1] > 1000) {
            this.scrollTimeStampArray = []; //empty array;
        }
       
        if(array.length > 3 && currTimeStamp - array[array.length - 3] < 100 ) {
            return false;
        }
        
        array.push(currTimeStamp);
        
        return true;
    },
    
    _onScrollListener : function(e) {
        if (!this.displayVsbar)
            return;
        
        
        if(!this._isExecuteScrollEvent(e)) {
            (new MochiKit.Signal.Event(this, e)).stop(); //cancel window scrolling when bestfitpage is true
            return;
        }
                
        var delta = 0;
        if (!e) /* For IE */
            e = window.event;
        if (e.wheelDelta) { /* IE / Opera */
            delta = e.wheelDelta / 40;
            if (window.opera)
                delta -= delta;
        } else if (e.detail) { /* Mozilla*/
            delta = -e.detail;
        }
        
        if(Math.abs(delta) > this.MAX_ABSOLUTE_DELTA) {
            //delta is too large in Safari
            delta = delta > 0 ? this.MAX_ABSOLUTE_DELTA : -this.MAX_ABSOLUTE_DELTA;
        }

        if (delta) /* nonzero? handle it */
            this._onScroll(delta, e);
    },

    _onScroll : function(value, event) {
        if (value == 0)
            return;

        var isMoved = false;
        var moveBy = value * this.SCROLL_DOWN_PER_UNIT;
        if (value < 0)
            isMoved = this._moveUp(-moveBy);
        else if (value > 0)
            isMoved = this._moveDown(moveBy);
        
        if(isMoved) 
            (new MochiKit.Signal.Event(this, event)).stop(); //cancel window scrolling when bestfitpage is true
    },

    _updatePagesArray : function(page) {
        var pageNum = page.getNumber();
        var isAdded = false;
        for (var i = 0, len = this.pages.length; i < len; i++) {
            if (pageNum < this.pages[i].getNumber()) {
                this.pages.splice(i, 0, page);
                isAdded = true;
                break;
            }
        }
        
        if (!isAdded)
            this.pages.push(page);
    },

    /*
     * Return true if all pages of this report is cached in browser.
     */
    _isAllPagesCached : function() {
        return (this.isLastPageNumberKnown && this.pages.length == this.lastPageNumber);
    },

    /**
     * Provides estimate of overal height of container ( from page 1 to x) and top from page 1 to to of first page cached
     */
    _getOverallCtrTopAndHeight : function() {
        if (this._isAllPagesCached())
            return {height : this._getContainerHeight(), top : this._getContainerOffsetTop() };
        else  {
            var avgPageHeight = this._getAverageHeightOfCachedPages();
            
            var lastPageNumber = this.lastPageNumber;
            var lastPageInCache = this._getLastPageInCache();
            if(lastPageInCache)
                lastPageNumber = Math.max(this.lastPageNumber, lastPageInCache.getNumber());
            
            var height = this._getContainerHeight();
            if(lastPageNumber > this.pages.length)
                height +=(lastPageNumber - this.pages.length) * avgPageHeight;
            
            
            var firstVisiblePage = this._getFirstVisiblePage();
            if (!firstVisiblePage)
                bobj.logToConsole("Error!!!!! No first visible page");
                
            var missingPagesCount = 0; /** get count of missing pages in cache from first visible page to the first cached page*/
            for (var i = 1,len = this.pages.length; i < len; i++) {
                if (firstVisiblePage && this.pages[i].getNumber() <= firstVisiblePage.getNumber())
                    missingPagesCount += (this.pages[i].getNumber() - this.pages[i - 1].getNumber() - 1)
            }
            
            var firstCachedPage = this._getFirstPageInCache();
            
            var top = 0;
            
            if (firstCachedPage) 
                top = this._getContainerOffsetTop() + (-1 * ((missingPagesCount * avgPageHeight) + ((firstCachedPage.getNumber() -1) * avgPageHeight)));
            
            return {height : height, top : top};
        }
    },
    
    _getAverageHeightOfCachedPages : function () {
        var height = this._getContainerHeight();
        var pagesCount = this.pages.length;
        if (pagesCount == 0)
            return this.sizeOfLastLoadedPage.h;
        else
            return height/pagesCount;
    },

    _adjustThumbSize : function() {
        var ctrTH = this._getOverallCtrTopAndHeight();
        if (ctrTH && ctrTH.height > 0) {
            var lInPercent = (this._getPanelHeight() / ctrTH.height) * 100.0;
            this.vsbar.setThumbLength(lInPercent);
            /* bobj.logToConsole("Thumb length changed to " + lInPercent); */
        }
    },

    _adjustThumbPos : function() {
        var ctrTH = this._getOverallCtrTopAndHeight();
        if (ctrTH) {
            var diff = ctrTH.height - this._getPanelHeight();
            if (diff != 0) {
                var pInPercent = -ctrTH.top / diff * 100;
                this.vsbar.setThumbPosition(pInPercent);
            }
            this.topOffset = -this._getPageTop(this._getFirstVisiblePage());
            // topOffset should be zero (case of single page in middle of screen)
            if (this.topOffset < 0)
                this.topOffset = 0;
        }
    },

    _onThumbMoveListener : function(pos) {       
        var moveBy = this._getMoveByOffsetY(pos);
        if (moveBy < 0)
            this._moveUp(-moveBy, true);
        else if (moveBy > 0)
            this._moveDown(moveBy, true);
    },
    
    _getMoveByOffsetY : function (pos) {
        var ctrTH = this._getOverallCtrTopAndHeight();
        if (ctrTH) {
            var curTop = ctrTH.top;
            var newTop = -pos * (ctrTH.height - this._getPanelHeight()) / 100;
            var moveBy = newTop - curTop;
            return moveBy;
        }
        
        return 0;
    },

    _getPageNumForScrollPos : function(pos) {
        var first = this._getFirstVisiblePage();
        if (first)
            return L_bobj_crv_PageNum.replace("{0}", this._getFirstVisiblePage().getNumber());
        else
            return '';
    },

    _addPage : function(page, isTopPage) {
        bobj.logToConsole("START ScrollingReportView_addPage" + page.getNumber());
        var html = page.getHTML();
        
        var currentPageNumber = this._getCurrentPageNumber();
        var isAdded = false;
        var isAdjustOffset = false;
        for (var i = 0; i < this.pages.length; i++) {
            if (page.getNumber() < this.pages[i].getNumber()) {
                if (currentPageNumber > 0 && page.getNumber() < currentPageNumber) {
                    isAdjustOffset = true;
                }
                
                insBefore2(this.pages[i].layer, html);
                isAdded = true;
                break;
            }
        }
        if (!isAdded) {
            append(this._ctrNode, html); /*new page is after all the cached pages*/
        }
        
        this._updatePagesArray(page);
        
        page.init();
        
        if (isAdjustOffset) {
            var newTop = (this._getContainerOffsetTop() - page.getHeight());
            this._setContainerOffsetTop(newTop);
        }
        
        this._addPageListeners(page);
        
        if(!this._getFirstVisiblePage()) {
            this._displayPageAtTop(page.getNumber());
            bobj.logToConsole("HACK1 _displayPageAtTop " + page.getNumber());
        }
        
        if (isTopPage && this.zoomChangeFactor != 1)
            this._moveUp(this.zoomChangeFactor * this.topOffset);
        
        this._adjustThumbPos();
        this._adjustThumbSize();
        
        bobj.logToConsole("END ScrollingReportView_addPage" + page.getNumber());
    },
    
    /*
     * Returns the first visible page on the top of the view from the cached pages
     */
    _getFirstVisiblePage : function() {
        for ( var i = 0, len = this.pages.length; i < len; i++) {
            var page = this.pages[i];
            if (this._isPageVisible(page))
                return page;
        }

        return null;
    },
    
    _getLastLoadedPageInCache : function () {
        if(this.pages.length > 0) {
            for(var i = this.pages.length-1; i >= 0; i--) {
                if(this.pages[i] != null && this.pages[i].hasContent()) {
                    return this.pages[i];
                }
            }
        }
        return null;
    },

    /*
     * Returns the last visible page on the bottom of the view from the cached pages
     */
    _getLastVisiblePage : function() {
        var p = this.pages;
        var len = p.length;

        for ( var i = len - 1; i > -1; i--) {
            var page = p[i];
            if (this._isPageVisible(page))
                return page;
        }

        return null;
    },

    /*
     * Returns the given page's top offset relative to panel
     */
    _getPageTop : function(page) {
        return this._getContainerOffsetTop() + this._getPageTopOffset(page);
    },
    
    /*
     * Returns the given page's top offset relative to container
     */
    _getPageTopOffset : function(page) {
        var offset = 0;
        for (var i = 0 ; i < this.pages.length; i++) {
            if (this.pages[i].getNumber() < page.getNumber())
                offset += this.pages[i].getHeight();
            else
                break;
        }
        
        return offset;
    },

    /*
     * Returns true if given page is visible in the report view
     */
    _isPageVisible : function(page) {
        var pageTop = this._getPageTop(page);
        if (pageTop > this._getPanelHeight())
            return false;

        var pageBottom = pageTop + page.getHeight();
        if (pageBottom <= 0)
            return false;

        return true;
    },
    
    /*
     * Returns true if given page is about to visible in the report view
     */
    _isPageAboutToVisible : function(page) {
        var pageTop = this._getPageTop(page);
        if (pageTop > this._getPanelHeight() + 50)
            return false;

        var pageBottom = pageTop + page.getHeight();
        if (pageBottom < -50)
            return false;

        return true;
    },

    /*
     * Returns true if the given page can be unload from cache
     */
    _canUnloadPage : function(page) {
        if (this.pages.length == 1) /*TODO, remove this line?*/
            return false; /* Don't unload the last page in cache as it's coordinate is necessary for loading next pages*/
        
        var pageTop = this._getPageTop(page);
        if (pageTop > (this._getPanelHeight() + this.UNLOAD_MARGIN))
            return true;

        var pageBottom = pageTop + page.getHeight();
        if (pageBottom < -this.UNLOAD_MARGIN)
            return true;

        return false;
    },
    
    _findIndexOfPageInCache : function (pageNum) {
        for (var i = 0, len = this.pages.length; i < len; i++)
            if (this.pages[i].getNumber() == pageNum)
                return i;
        
        return -1;
    },
    
    _isPageInCache : function(pageNum) {
        return this._findIndexOfPageInCache(pageNum) >= 0;
    },

    getPage : function (pageNum) {
        return this._getPageFromCache(pageNum);
    },
    
    _getPageFromCache : function(pageNum) {
        var index = this._findIndexOfPageInCache(pageNum);
        if (index >= 0)
            return this.pages[index];
        else 
            return null;
    },

    /*
     * Unload the given page from cache and cleanup memory
     */
    _unloadPage : function(page) {
        bobj.logToConsole("START ScrollingReportView_unloadPage [page:" + page.getNumber() + "]");
        var cacheIndex = this._findIndexOfPageInCache(page.getNumber());
        var currentPageNumber = this._getCurrentPageNumber();
        this.pages.splice(cacheIndex, 1);
        
        if (page.isInFocus()) {
            this.layer.focus();
        }

        var height = page.getHeight();
        page.dispose();
        page.removeKeyPressListener(this._onKeypressCB);
        page.removeMouseWheelListener(this._mouseWheelCallback);
        bobj.deleteWidget(page);
        
        if (currentPageNumber > page.getNumber()) {
            this._setContainerOffsetTop(this._getContainerOffsetTop() + height);
        }
        
        if(currentPageNumber == -1) {
            this._setContainerOffsetTop(0);
            bobj.logToConsole("HACK3");
        }
        
        bobj.logToConsole("END ScrollingReportView_unloadPage [page:" + page.getNumber() + "]");
    },

    /*
     * Dispose this report view and cleanup memory
     */
    dispose : function() {
        this._unloadAllPages();
        /* TODO dispose scrollbar widgets */
        bobj.removeAllChildElements(this.layer);
    },
    
    
    _unloadAllPages : function() {
        while (this.pages[0]) {
            this._unloadPage(this.pages[0]);
        }
        
        this._setContainerOffsetTop(0);
    },

    /* 
     * Move the container up by pixel value or by segment value - scroll down
     * MoveBy is relative to top of container. returns true of container offsetop changes
     */
    _moveUp : function(moveBy, byThumbMove) {
        var isMoved = false;
        var originalTop = this._getContainerOffsetTop();
        if (this.pages.length == 0 || this._getContainerOffsetTop() > 0)
            return isMoved; /* don't attempt to move when no pages visible? */
        
        var oldPageNumber = this._getCurrentPageNumber();
        var pageToLoad = this._getPageToLoadFromMoveByOffset(moveBy, false);
        
        if(pageToLoad < oldPageNumber)
            return isMoved;
        
        var newTop = this._getContainerOffsetTop() - moveBy;
        var isLastPageLoaded = this.isLastPageNumberKnown && (this.lastPageNumber == this._getLastPageInCache().getNumber());
        
        if (isLastPageLoaded) {
            if ((this._getContainerOffsetTop() + this._getContainerHeight()) != this._getPanelHeight()) {
                var newBottom = newTop + this._getContainerHeight();
                var diff = this._getPanelHeight() - newBottom;
                if (diff > 0) 
                    newTop += diff; /* don't let user to scroll below end of last page*/
            }
            else {
                newTop = this._getContainerOffsetTop();
            }
        }

        this._setContainerOffsetTop(newTop);
        
        if (!byThumbMove) {
            this._adjustThumbPos();
        }
        
        this._loadPage(pageToLoad, false);
        
        if (this._getFirstVisiblePage() == null)
            this._displayPageAtTop(pageToLoad);
        
        this._updateCache(true);
        
        var newPageNumber = this._getCurrentPageNumber();
        
        if (newPageNumber != -1 && newPageNumber != oldPageNumber)
            this._signalUpdateCurrentPage(newPageNumber);
        
        this._loadPage(pageToLoad + 1, false); /*Page should be already in cache in normal circumstances*/
        this.vsbar.resize(this.panelHeight);
        
        isMoved = (originalTop != this._getContainerOffsetTop());
        return isMoved;
    },
    
    /**
     * Clears unnecessary cached pages based on direction of scrolling
     * @param isMoveUp [boolean] indicating scroll down
     */
    _updateCache : function(isMoveUp) {
        if (isMoveUp) {
            while (this.pages[0] && this._canUnloadPage(this.pages[0])) {
                this._unloadPage(this.pages[0]);
            }
        }
        else {
            while (this.pages[this.pages.length - 1]) {
                var currentPage = this.pages[this.pages.length - 1];
                if (this._canUnloadPage(currentPage)) {
                    this._unloadPage(currentPage);
                } else
                    break;
            }
        }
    },
    
   /**
    * @param moveBy, positive decimal number indicating the amount of movement
    * @param isUpward, boolean indicating the direction of movement. true means scroll up (movedown)
    */
   _getPageToLoadFromMoveByOffset: function(moveBy, isDownward) {
       var firstVisiblePageNumber = this._getCurrentPageNumber();
       if (firstVisiblePageNumber == -1) {
           var firstCachedPage =  this._getFirstPageInCache();
           if (firstCachedPage)
               firstVisiblePageNumber = firstCachedPage.getNumber();
       }
       
       if (isDownward)
           moveBy *= -1;
       
       var pageToLoad =  Math.floor(firstVisiblePageNumber + ( moveBy / this._getAverageHeightOfCachedPages()));
       if (pageToLoad < 1)
           pageToLoad = 1;
       
       return pageToLoad;
       
   },

    /* 
     * Move the container down by pixel value or by segment value - scroll up
     */
    _moveDown : function(moveBy, byThumbMove) {
        var isMoved = false;
        var originalTop = this._getContainerOffsetTop();
       
        if (this.pages.length == 0 || this._getContainerOffsetTop() > 0)
            return isMoved; /* don't attempt to move when no pages visible? */

        var oldPageNumber = this._getCurrentPageNumber();
        var pageToLoad = this._getPageToLoadFromMoveByOffset(moveBy, true);
        
        if(pageToLoad > oldPageNumber)
            return isMoved;
        
        var newTop = this._getContainerOffsetTop() + moveBy;
        this._setContainerOffsetTop(newTop);
        
        if (oldPageNumber && oldPageNumber == 1 && this._getContainerOffsetTop() > 0) {
            this._setContainerOffsetTop(0);
            isMoved = (originalTop != this._getContainerOffsetTop());
            return isMoved;
        }
            
        
        if (!byThumbMove)
            this._adjustThumbPos();

        this._updateCache(false);

        if (pageToLoad >= 1) { /* time to load a page above */
            this._loadPage(pageToLoad, false);
            if (this._getContainerOffsetTop() > 0) {
                this._displayPageAtTop(pageToLoad);
            }
        }
        
        if (pageToLoad > 1)
            this._loadPage(pageToLoad - 1, false); /*Page should be already in cache in normal circumstances*/
        
        var newPageNumber = this._getCurrentPageNumber();
        if (newPageNumber != oldPageNumber && newPageNumber != -1)
            this._signalUpdateCurrentPage(newPageNumber);
        this.vsbar.resize(this.panelHeight);
        
        isMoved = (originalTop != this._getContainerOffsetTop());
        return isMoved;
    },

    /*
     * Signal the viewer listener to load a page with the given page number if appropriate
     */
    _loadPage : function(pageNum, isTopPage) {
        if (pageNum < 1 || (this.isLastPageNumberKnown && pageNum > this.lastPageNumber) || this._isPageInCache(pageNum))
            return;
        var loadingPage = bobj.crv.newScrollingReportPage( {
            pageNum :pageNum,
            width :this.sizeOfLastLoadedPage.w,
            height :this.sizeOfLastLoadedPage.h,
            screenReaderHandler : MochiKit.Base.bind(this._signalLoadPage, this, pageNum, false, this._isAllowIncompletePageRequest(), true),
            documentView :(this.printLayout) ? bobj.crv.ReportPage.DocumentView.PRINT_LAYOUT
                            : bobj.crv.ReportPage.DocumentView.WEB_LAYOUT
        });

        this._addPage(loadingPage, isTopPage);
        this._logPages("Added dummy page " + loadingPage.getNumber());
        
        this._clearLoadPageSignal(pageNum);
        
        var loadPageCB = function(me, pageNumber, isTopPage, isAllowIncompletePage) {
            return function() {
                me._signalLoadPage(pageNumber,isTopPage, isAllowIncompletePage, false);
            }
        }(this, pageNum, false, this._isAllowIncompletePageRequest());
        
        this.loadPageIntervalMap[pageNum] = setInterval(loadPageCB, 200);
    },
    
    loadCompletePage : function (pageNum) {
        this._signalLoadPage(pageNum, false, false, false);
    },
    
    _signalLoadPage : function (pageNum, isTopPage, isAllowIncompletePage, byScreenReaderLink) {
        var page = this._getPageFromCache(pageNum);
        var cancelInterval = false;
        if ((page && this._isPageAboutToVisible(page)) || isTopPage || byScreenReaderLink) {
            var isShowProcessingIndicator = isTopPage; /*When top page is true, we must also display processingIndicator*/
            this._signalLoadPageCommon(pageNum, isTopPage, isAllowIncompletePage, isShowProcessingIndicator);
            cancelInterval = true;
            
            /*load page before and after the current page*/
            this._loadPage(pageNum - 1, false);
            this._loadPage(pageNum + 1, false);
        }
        else if (page == null) {
            cancelInterval = true; //Page is no longer visible;
        }
        
        if (cancelInterval) {
            this._clearLoadPageSignal(pageNum);
        }
            
    },
    
    // Find the next page that needs an update from incomplete to complete.  Reload it.
    // The update() funtion will trigger the loading of subsequent next incomplete pages.
    // While iterating through, unload any non-visible cached pages so we re-load them later.
    _signalUpdateIncompletePages : function () {
        if (!this.isLastPageNumberKnown)
            return;  // no need to load anything
        if (this.isPageLoadInProgress)
            return;  // wait for the currently-loading page to re-trigger an update
        
        // Generally firstPageInCache <= firstVisiblePage <= lastVisiblePage <= lastPageInCache.
        var firstPageInCache = this._getFirstPageInCache();
        var firstVisiblePage = this._getFirstVisiblePage();
        var lastVisiblePage = this._getLastVisiblePage(); 
        var lastPageInCache = this._getLastPageInCache();
        
        var firstN = this.lastPageNumber;
        if (firstVisiblePage && firstVisiblePage.getNumber() < firstN)
            firstN = firstVisiblePage.getNumber();
        if (firstPageInCache && firstPageInCache.getNumber() < firstN)
            firstN = firstPageInCache.getNumber();

        var lastN = 1;
        if (lastPageInCache && lastPageInCache.getNumber() > lastN)
            lastN = lastPageInCache.getNumber();
        if (lastVisiblePage && lastVisiblePage.getNumber() > lastN)
            lastN = lastVisiblePage.getNumber();
        
        // Loop over all pages and either skip, unload from cache, or reload.
        for (var pageNum = firstN; pageNum <= lastN; ++pageNum) {
            var page = this._getPageFromCache(pageNum);
            if (page && page.hasContent() && page.isMissingTotalPageCount()) {
                if (this._canUnloadPage(page) && (firstVisiblePage && pageNum < firstVisiblePage.getNumber()) ||
                                                 (lastVisiblePage && pageNum > lastVisiblePage.getNumber())) {
                    this._unloadPage(page);
                }
                else if (!this.isPageLoadInProgress) {
                    this._signalLoadPageCommon(pageNum, false, false, false);
                }
            }
        }
    },
    
    _signalLoadPageCommon : function (pageNum, isTopPage, isAllowIncompletePage, isShowProcessingIndicator) {
        MochiKit.Signal.signal(this, 'getPage', pageNum.toString(), isTopPage, isAllowIncompletePage, isShowProcessingIndicator);
        bobj.logToConsole("Getting page " + pageNum);
        this.isPageLoadInProgress = true;
    },
    
    _clearAllLoadPageSignals : function () {
        for (var pageNum in this.loadPageIntervalMap) {
            this._clearLoadPageSignal(pageNum);
            this._unloadPage(this._getPageFromCache(pageNum));
        }
    },
    
    _clearLoadPageSignal : function(pageNum) {
        clearInterval(this.loadPageIntervalMap[pageNum]);
        delete this.loadPageIntervalMap[pageNum];
    },

    _getLastPageInCache : function() {
        if (this.pages)
            return this.pages[this.pages.length - 1];
        return null;
    },

    _getFirstPageInCache : function() {
        if (this.pages)
            return this.pages[0];
        return null;
    },

    _signalUpdateCurrentPage : function(pageNum) {
        var numPagesString = this.lastPageNumber;
        if (!this.isLastPageNumberKnown)
            numPagesString += '+';
        
        MochiKit.Signal.signal(this, 'updateCurrentPage', pageNum, numPagesString);
        var page = this.getPage(pageNum);
        
        if(this.isLastPageNumberKnown && page!= null && page.isMissingTotalPageCount()) 
            this.loadCompletePage(pageNum);
    },

    _signalUpdateLastPage : function(lastPageNum, lastPageNumKnown) {
        MochiKit.Signal.signal(this, 'updateLastPage', this.groupNamePath, lastPageNum, lastPageNumKnown);
    }
};
