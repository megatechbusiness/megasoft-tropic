/* Copyright (c) Business Objects 2006. All rights reserved. */

/**
 * ReportAlbum Constructor
 */
bobj.crv.newReportAlbum = function(kwArgs) {
    var mb = MochiKit.Base;
    var UPDATE = mb.update;
    var BIND = mb.bind;
    var ALBUM = bobj.crv.ReportAlbum;
    
    kwArgs = UPDATE({
        id: bobj.uniqueId(),
        initTabIdx: 0, // Index of tab to select when initializing
        width: 800,
        height: 500,
        displayDrilldownTab : true
    }, kwArgs);
    
    
    // Override TabbedZone's TabBarWidget
    var tabbar = newNaviBarWidget(bobj.uniqueId(), _HorizTabTopWithClose, null, null, kwArgs.width, null, null, null, false, true, false);
    var o = newTabbedZone(kwArgs.id, tabbar, kwArgs.width, kwArgs.height);
    
    tabbar.cb = BIND(ALBUM._onSelectTab, o);
    tabbar.closeTab = BIND(ALBUM._removeView, o, true);
    bobj.fillIn(o, kwArgs);  
    o.widgetType = 'ReportAlbum';
    o._views = [];
    o._hideFrame = false;
    
    // Attach member functions 
    o.selectOld = o.select;
    UPDATE(o, ALBUM);

    o.getNewRowBelowTabBar = bobj.crv.ReportAlbum.addBreadcrumb;
    o.displayBreadcrumb = undefined;
    o._signalMapping = {}; /* view -> [Signals]*/
    
    return o;    
};

bobj.crv.ReportAlbum = {
    getTabBarHeight : function() {
        return this.tabs.getHeight ();
    },
    
    init : function() {
        /*
         * This is not nice... Copied super class' init code here so that select will be called only once.
         */
        var partial = MochiKit.Base.partial;
        var signal = MochiKit.Signal.signal;
        var connect = MochiKit.Signal.connect;
        this.tzOldInit ();
        this.tabs.init ();
        if (this.breadcrumb) {
            connect (this.breadcrumb, 'showBreadcrumb', this, this.showBreadcrumb);
            connect (this.breadcrumb, 'hideBreadcrumb', this, this.hideBreadcrumb);
            connect (this.breadcrumb, 'breadcrumbNavigate',  partial (signal, this, 'breadcrumbNavigate'));
            this.breadcrumb.init();
	}
        this.showDrilldownTab (this.displayDrilldownTab);

        var views = this._views;
        if (views.length > 0) {
            for ( var i = 0, len = views.length; i < len; i++) {
                /* Since views are not initializing themself (as before through
                 adding bobj.crv.getInitHtml to their HTML)
                 it is report album's responsibility to initialize its children */
                views[i].init ();

            }

            if (this.initTabIdx < 0 || this.initTabIdx >= views.length) {
                this.initTabIdx = 0;
            }

            this.select (this.initTabIdx);
        }
    },
    
    addCanvasListener : function(canvasListener) {
        for ( var i = 0, len = this._views.length; i < len; i++) {
            this._views[i].addCanvasListener(canvasListener);
        }
    },
    
    removeCanvasListener : function(canvasListener) {
        for ( var i = 0, len = this._views.length; i < len; i++) {
            this._views[i].removeCanvasListener(canvasListener);
        }
    },

    showBreadcrumb : function ()
    {
    	if (this.displayBreadcrumb != true)
    	{
            this.displayBreadcrumb = true;
            getLayer(this.id + '_breadcrumbtr').style.display = (!_ie8Up && _ie) ? 'block' : 'table-row';/* IE supports table-row since IE8. Other browsers support table-row*/
            this.resizeOuter (this.getWidth(), this.getHeight());
    	}
    },
    
    getBreadcrumb : function () {
        return this.breadcrumb;
    },
    
    hideBreadcrumb : function ()
    {
    	if (this.displayBreadcrumb != false)
    	{
            this.displayBreadcrumb = false;
            getLayer(this.id + '_breadcrumbtr').style.display = 'none';
            this.resizeOuter (this.getWidth(), this.getHeight());
    	}
    },
    
    showDrilldownTab  : function (isDisplay) {
        this.displayDrilldownTab = isDisplay;
        try 
        {
        	/* IE supports table-row since IE8. Other browsers support table-row*/
        	 var displayStyle = (!_ie8Up && _ie) ? 'block' : 'table-row';
            this.tabs.layer.parentNode.parentNode.style.display = isDisplay ? displayStyle : "none";
        }
        catch(e){}
    },
    
    isDisplayDrilldownTab : function () {
        return this.displayDrilldownTab;
    },
    
    setHideFrame : function(hide) {
        this._hideFrame = hide;
        var selectedView = this.getSelectedView ();
        if (selectedView && this._hideFrame)
        	selectedView.hideFrame();
    },

    addBreadcrumb : function () {
        if (this.breadcrumb) {
            var align = bobj.crv.config.isRTL ? 'right' : 'left';
            return '<tr id="' + this.id + '_breadcrumbtr" style="display:none;height:23px" class="hideableFrame" valign="bottom">'+
                '<td></td>'+
                '<td style="vertical-align: middle" align="'+ align + '">'+ this.breadcrumb.getHTML()+'</td>'+
                '<td></td></tr>';
        } else {
            return '';
        }
    },
    
    update : function(update) {
        if (!update || update.cons !== "bobj.crv.newReportAlbum") {
            return;
        }

        /* Updates all views */
        var nNewViews = 0;
        for (var i = 0, len = update.children.length; i < len; i++) {
            var child = update.children[i];
            if (child) {
                switch (child.cons) {
                case "bobj.crv.newReportView":
                case "bobj.crv.newScrollingReportView":
                    var view = this._views[nNewViews++];
                    if (view) {
                        view.update (child); /* updates current view */
                    }
                    break;
                case "bobj.crv.newReportBreadcrumb":
                    if (this.breadcrumb) {
                        this.breadcrumb.update (child);
                    }
                    break;
                }
            }
        }

        var updateChildrenLength = nNewViews;
        var currentViewsLength = this._views.length;

        /* Adds any new view created by drill down action */
        if (updateChildrenLength > currentViewsLength) {
            for ( var i = currentViewsLength, len = updateChildrenLength; i < len; i++) {
                this.delayedAddChild (bobj.crv.createWidget (update.children[i]));
            }
        }
        /* Removes views that are have been removed from viewstate */
        else if (updateChildrenLength < currentViewsLength) {
            for ( var i = currentViewsLength -1, len = updateChildrenLength; i >= len; i--) {
                this._removeView (false, i); /* removes non-existing views */
            }
        }

        this.initTabIdx = update.args.initTabIdx;
        this.select (this.initTabIdx);
    },

    pushLastPageNumber : function(lastPageNumber, groupNamePath) {
        for (var i in this._views) {
            var view = this._views[i];
            if (view.pushLastPageNumber && typeof(view.pushLastPageNumber) == "function") {
                view.pushLastPageNumber(lastPageNumber, groupNamePath);
            }
        }
    },
    
    /**
     * @return index of the tab specified by viewstateid
     */
    findTabNumber : function(viewStateId) {
        var views = this._views;
        for ( var i = 0, len = views.length; i < len; i++)
            if (views[i].viewStateId == viewStateId)
                return i;

        return -1;
    },
    
    /**
     * Adds view to its list of views. Since this method can be called after initialization time, its HTML
     * is first added to report album, and then widget is intialized.
     */
    delayedAddChild : function(view) {
        this._views.push (view);
        var tab = this.tabs.add(view.label, view.tooltip);
        var tabHTML = this.getTabHTML (this._views.length - 1);
        append (getLayer (this.id + "_container"), tabHTML);
        view.init ();
    },
    
    addChild : function(child) {
        if (child) {
            switch (child.widgetType) {
            case 'ReportView':
            case 'ScrollingReportView':
                this._views.push (child);
                this.add (child.label, child.tooltip);
                break;
            case 'ReportBreadcrumb':
                this.breadcrumb = child;
                break;
            }
        }
    },
    
    focusFirstChild : function () {
        var view = this.getSelectedView ();
        if(view) {
            return view.focusFirstChild();
        }
        
        return false;
    },
    
        
    connectViewSignals: function(view) {
        var connect = MochiKit.Signal.connect ;
        var signal = MochiKit.Signal.signal;
        var partial = MochiKit.Base.partial ;
        
        if(this._signalMapping[view] != null) {
            this.disconnectViewSignals(view);
        }
        
        this._signalMapping[view] = new Array();
        var arr = this._signalMapping[view];
        
        arr.push(connect (view, 'getPage', partial(signal, this, 'getPage')));
        arr.push(connect (view, 'moveFocusGroup', partial(signal, this, 'moveFocusGroup')));
        arr.push(connect (view, 'updateCurrentPage', partial (signal, this, 'updateCurrentPage')));
        arr.push(connect (view, 'updateLastPage', partial (signal, this, 'updateLastPage')));
        arr.push(connect (view, 'nextPage', partial(signal, this, 'nextPage')));
        arr.push(connect (view, 'prevPage', partial(signal, this, 'prevPage')));
        arr.push(connect (view, 'firstPage', partial(signal, this, 'firstPage')));
        arr.push(connect (view, 'lastPage', partial(signal, this, 'lastPage')));
        arr.push(connect (view, 'selectPage', partial(signal, this, 'selectPage')));
        arr.push(connect (view, 'findLastPageNumber', partial(signal, this, 'findLastPageNumber')));
    },
    
    
    disconnectViewSignals: function(view) {
        if(this._signalMapping[view] != null) {
            var arr = this._signalMapping[view];
            while(arr.length > 0) {
                bobj.crv.SignalDisposer.dispose(arr.pop(), false);
            }
            
            delete this._signalMapping[view];
        }
    },
    
    getHTML : function() {
        var html = this.beginHTML ();
        var children = this._views;
        for ( var i = 0, len = children.length; i < len; ++i) {
            html += this.getTabHTML (i);
        }

        html += this.endHTML ();
        return html;
    },
    

    /**
     * @param index,
     *            index of tab to create HTML for
     * @return creates HTML for tab specified by index
     */
    getTabHTML : function(index) {
        var tab = this.tabs.items[index];
        var view = this._views[index];
        var html = '';

        if (tab && view) {
            html += this.beginTabHTML (tab);
            html += view.getHTML ();
            html += this.endTabHTML ();
        }

        return html;
    },
    
    /**
     * Resize the outer dimensions of the ReportAlbum. The standard resize method, inherited from TabbedZoneWidget, resizes the container. We
     * can't override it without breaking things.
     */
    resizeOuter : function(w, h) {
    	var tabHeight = 33;
        var frameWidth = 10;
        
        if(bobj.isNumber (h)) {
            if(this.displayDrilldownTab || this.displayBreadcrumb)
                h -= tabHeight;
            else
            	h -= 10;
            h = Math.max(h, 0);
        }

        if(bobj.isNumber (w)) {
            if(!this._hideFrame) 
                w -= frameWidth;
            
            w = Math.max(w, 0);
        }
        
        if(this.breadcrumb)
        	this.breadcrumb.resize(w, h);

        this.resize (w, h);
        this.tabs.resize (w);

        var selectedView = this.getSelectedView ();
        if (selectedView) {
            selectedView.resize (w, h); /* Notify ReportView of resize */
        }
    },
    
    
    
    /**
     * @return Returns a suggested size for the widget as an object with width and height integer properties that specify the dimensions in
     *         pixels.
     */
    getBestFitSize : function() {
        var w = this._hideFrame ? 0 : 10;
        var h = (this.displayDrilldownTab || this.displayBreadcrumb) ? 33 : 10;
        var selectedView = this.getSelectedView ();
        if (selectedView) {
            var viewSize = selectedView.getBestFitSize ();
            w += viewSize.width;
            h += viewSize.height;
        }

        return {
            width : w,
            height : h
        };
    },
    
    /**
     * Overrides parent. Opens a tab with a positioned div. The positioning prevents
     * the ReportView from disappearing in IE.
     */
    beginTabHTML : function(tab) {
        return bobj.html.openTag ('div', {
            id : tab.zoneId,
            style : {
                display : 'none',
                width : this.w + 'px',
                height : this.h + 'px',
                position : 'relative'
            }
        });
    },
    
    /**
     * @return Returns the select ReportView or null if no view is selected
     */
    getSelectedView : function() {
        return this._views[this.oldIndex];
    },
    

    /**
     * Overrides parent. Selects a report view to display.
     *
     * @param index [int]  Index of the report view
     */
    select : function(index) {

        if (index >= 0 && index < this._views.length && index != this.oldIndex) {

            /* Disposes currently selected view */
            var selectedView = this.getSelectedView ();
            if (selectedView) {
                this.disconnectViewSignals(selectedView);
                selectedView.dispose ();
            }

            /* Creates new signals for newly selected view */
            var selectedView = this._views[index];
            this.connectViewSignals(selectedView);

            /* Does the dirty DOM manipulation to actually change the view */
            this.selectOld (index);

            /*
             * Notifies parameter controller that view has changes so it can enable/disable panel based on view's type
             */
            MochiKit.Signal.signal (this, "viewChanged");
        }
    },
    
    /**
     * Removes view specified by index from view list
     * 
     * @param autoSelectNext
     *            [boolean] indicates whether another view should be auto selected after view specified by index is removed
     * @param index
     *            [Number} index of view that would be removed
     */
    _removeView : function(autoSelectNext, index) {
        var removedView = this._views[index];
        var removedTab = this.tabs.items[index];

        autoSelectNext = (removedTab != null && removedTab.isSelected && autoSelectNext);
        
        if (removedView) {
            removedView.dispose ();
            bobj.deleteWidget (removedView);
            MochiKit.Signal.signal (this, 'removeView', removedView);
        }
  
        if (removedTab.isSelected) {
            /* oldIndex (index of currently selected view) needs to be reset so when
             response for the next selected view arrives
             viewer think that view has changed */
             this.oldIndex = -1;
        }

        arrayRemove (this, '_views', index);
        this.tabs.remove (index, autoSelectNext);
        bobj.deleteWidget (removedTab);
    },
    
    _onSelectTab : function(index) {
        if(index != this.oldIndex)
            MochiKit.Signal.signal (this, 'selectView', this._views[index]);
    }
};

