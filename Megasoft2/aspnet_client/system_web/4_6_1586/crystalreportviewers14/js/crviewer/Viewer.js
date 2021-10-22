/* Copyright (c) Business Objects 2006. All rights reserved. */

/**
 * Viewer Constructor
 *
 * kwArgs.layoutType [String]  Tells the viewer how to size itself. Can be 
 *                             "client" (fill window), "fitReport", or "fixed"
 * kwArgs.width      [Int]          Width in pixels when layoutType=fixed
 * kwArgs.height     [Int]          Height in pixels when layoutType=fixed
 */
bobj.crv.newViewer = function(kwArgs) {
    kwArgs = MochiKit.Base.update({
        id: bobj.uniqueId(),
        isDisplayModalBG : false,
        isLoadContentOnInit : false,
        layoutType  : bobj.crv.Viewer.LayoutTypes.FIXED, 
        visualStyle : {
            className       : null,
            backgroundColor : null,
            borderWidth     : null,
            borderStyle     : null,
            borderColor     : null,
            fontFamily      : null,
            fontWeight      : null,
            textDecoration  : null,
            color           : null,
            width           : '800px',
            height          : '600px',
            fontStyle       : null,
            fontSize        : null,
            top             : "0px", /* passed by Java DHTML viewer */
            left            : "0px"  /* passed by Java DHTML viewer */
       }        
    }, kwArgs);
    var o = newWidget(kwArgs.id);

    bobj.fillIn(o, kwArgs);  
    o.widgetType = 'Viewer';
    
    o._topToolbar = null;
    o._reportAlbum = null;
    o._leftPanel = null;
    o._separator = null;
    o._print = null;
    o._export = null;
    o._promptDlg = null;
    o._reportProcessing = null;
    o._eventListeners = [];
    o._statusbar = null;
    o._leftPanelResizeGrabber = newGrabberWidget(
        o.id + '_leftPanelResizeGrabber', 
        bobj.bindFunctionToObject(bobj.crv.Viewer.onGrabberMove, o),
        0, // intial left
        0, // intial top
        4, // width
        1, // intial height (has to be pixels so we'll figure it out later)
        true); // Moves on the horizontal axis        
    
    // Attach member functions 
    o.initOld = o.init;
    o._boundaryControl = new bobj.crv.BoundaryControl(kwArgs.id + "_bc");
    o._modalBackground = new bobj.crv.ModalBackground(
            kwArgs.id + "_mb",
            bobj.bindFunctionToObject(bobj.crv.Viewer.keepFocus, o));
    MochiKit.Base.update(o, bobj.crv.Viewer);
    window[o.id] = o;
    o._actionListeners = [];
    
    return o;    
};

bobj.crv.Viewer = {
    LayoutTypes : {
        FIXED : 'fixed',
        CLIENT : 'client',
        FITREPORT : 'fitreport'
    },

    PromptingTypes : {
        HTML : 'html',
        FLEX : 'flex'
    },
    
    Components : {
        Toolbar : 'Toolbar',
        LeftPanel : 'LeftPanel',
        Statusbar : 'Statusbar',
        Breadcrumb : 'Breadcrumb'
    },
    
    onGrabberMove : function(x) {
        if (this._leftPanel) {
            this._leftPanel.resize (x, null);
            this._doLayout ();
        }
    },
    
    keepFocus : function () {
        var swf = bobj.crv.params.FlexParameterBridge.getSWF(this.id);
        if (swf)
            swf.focus();
    },
    
    addChild : function(widget) {
        if (widget.widgetType == 'ReportAlbum') {
            this._reportAlbum = widget;
        } else if (widget.widgetType == 'Toolbar') {
            this._topToolbar = widget;
            this._separator = newSeparatorWidget(this.id+'_sep',null,0,4,2,4);
        } else if (widget.widgetType == 'Statusbar') {
            this._statusbar = widget;
        } else if (widget.widgetType == 'PrintUI') {
            this._print = widget;
            this._print.setParent(this);
        } else if (widget.widgetType == 'ExportUI') {
            this._export = widget;
            this._export.setParent(this);
        } else if (widget.widgetType == 'ReportProcessingUI') {
            this._reportProcessing = widget;
            this._reportProcessing.setParent(this);
        } else if (widget.widgetType == 'LeftPanel') {
            this._leftPanel = widget;
        }
    },

    getHTML : function() {
        var h = bobj.html;
        
        var layerStyle = {
            overflow: 'hidden',
            position: 'relative',
            left : this.visualStyle.left,
            top  : this.visualStyle.top
        };
        
        var innerStyle = {
            width: _ie && _isQuirksMode ? '100%' : '',
            height: _ie && _isQuirksMode ? '100%' : ''
        };
        
        var dir = bobj.crv.config.isRTL ? 'rtl' : 'ltr';
        var html = h.DIV({dir: dir, id:this.id, style:layerStyle, 'class':'dialogzone'}, h.DIV({'class':'crviewer', style:innerStyle},
            this._topToolbar ? this._topToolbar.getHTML() : '',
            this._separator ? this._separator.getHTML() : '',
            this._leftPanel ? this._leftPanel.getHTML() : '',
            this._reportAlbum ? this._reportAlbum.getHTML() : '',
            this._leftPanelResizeGrabber ? this._leftPanelResizeGrabber.getHTML() : '',
            this._statusbar ? this._statusbar.getHTML(): ''));

        return html;
    },
    
    _onWindowResize : function() {
        if (this._currWinSize.w != winWidth () || this._currWinSize.h != winHeight ()) {
            this._doLayout ();
            this._currWinSize.w = winWidth ();
            this._currWinSize.h = winHeight ();
        }
    },
    
    init : function() {
        this.initOld ();
        this._initSignals ();
        
        if (this._reportAlbum)
            this._reportAlbum.init ();

        if (this._topToolbar)
            this._topToolbar.init ();

        if (this._separator)
            this._separator.init ();

        if (this._leftPanel)
            this._leftPanel.init ();

        if (this._statusbar)
            this._statusbar.init ();

        if (this._leftPanelResizeGrabber) {
            this._leftPanelResizeGrabber.init ();

            if (!this._leftPanel || !this._leftPanel.isToolPanelDisplayed ())
                this._leftPanelResizeGrabber.setDisplay (false);
        }
        
        this.setDisplayModalBackground(this.isDisplayModalBG);

        bobj.setVisualStyle (this.layer, this.visualStyle);

        this._currWinSize = {
            w : winWidth (),
            h : winHeight ()
        };
        var connect = MochiKit.Signal.connect;
        var signal = MochiKit.Signal.signal;

        if (this.layoutType.toLowerCase () == bobj.crv.Viewer.LayoutTypes.CLIENT) {
            connect (window, 'onresize', this, '_onWindowResize');
        }

        if (!this._topToolbar && !this._statusbar && this._reportAlbum && !this._reportAlbum.isDisplayDrilldownTab()) {
            this.layer.className += ' hideFrame';
            this._reportAlbum.setHideFrame(true);
        }

        if (this.layer && _ie && bobj.checkParent (this.layer, "TABLE")) {
            /*
             * delays the call to doLayout to ensure all dom elemment's width and
             * height are set beforehand.
             */
            connect (window, 'onload', this, '_doLayoutOnLoad');
            this._oldCssVisibility = this.css.visibility;
            this.css.visibility = "hidden";
        } else {
            this._doLayout ();
        }

        this.scrollToHighlighted ();
        signal (this, 'initialized', this.isLoadContentOnInit);
        bobj.crv.ViewerManager.onViewerInit(this);
    },
    
    
    resetSearch : function () {
        if(this._leftPanel) {
            this._leftPanel.resetSearch();
        }
    },
    
    /**
     * Connects all the signals during initialization
     */
    _initSignals : function() {
        var partial = MochiKit.Base.partial;
        var signal = MochiKit.Signal.signal;
        var connect = MochiKit.Signal.connect;
        var fe = MochiKit.Iter.forEach;

        if (this._topToolbar) {
            fe ( [ 'zoom', 'drillUp','refresh', 'search', 'export', 'print', 'selectHistory', 'clearHistory' ], function(sigName) {
                connect (this._topToolbar, sigName,  this, partial(this.fireAndSignalActionEvent, sigName));
            }, this);
            
            fe ( ['firstPage', 'prevPage', 'nextPage', 'lastPage', 'selectPage'], function(sigName) {
                /*ReportView must decide what to do about these events*/
                connect (this._topToolbar, sigName, this, partial(this._onNavigatePage, sigName));
            }, this);
            
            connect(this._topToolbar, 'onSetDisplay', this, '_onToolbarSetDisplayListener');
            
        }

        this._initLeftPanelSignals ();

        if (this._reportAlbum) {
            fe ( [ 'selectView', 'removeView', 'viewChanged', 'getPage', 'firstPage', 'prevPage', 'nextPage', 'lastPage', 'selectPage', 'findLastPageNumber', 'breadcrumbNavigate' ], function(sigName) {
                connect (this._reportAlbum, sigName, partial (signal, this, sigName));
            }, this);
            
            connect (this._reportAlbum, 'updateLastPage' , this, partial(this._onUpdateLastPageNumber));
            connect (this._reportAlbum, 'updateCurrentPage' , this, partial(this._onUpdatePageNumber));
            connect (this._reportAlbum, 'moveFocusGroup' , this, partial (this.moveGroupFocus, this._reportAlbum));
        }

        if (this._print) {
            connect (this._print, 'printSubmitted', partial (signal, this, 'printSubmitted'));
        }

        if (this._export) {
            connect (this._export, 'exportSubmitted', partial (signal, this, 'exportSubmitted'));
        }
        
        fe([this._export, this._print, this._topToolbar], function(obj) {
            if(obj) {
                connect (obj, 'showError', this, this.showError); 
            }
        }, this);
        
        bobj.connectDOMEvent(this.layer, _ie || _saf || _ie9Up ? 'onkeydown' : 'onkeypress', bobj.bindFunctionToObject(this._onKeyDownListener, this));
    },
    
    _onKeyDownListener : function (e) {
        if(e.keyCode == 117) {
            var target =  e.originalTarget || e.srcElement;
            var comp = this.findComponent(target);
            if(!comp)
                return false;
            
            this.moveGroupFocus(comp, !e.shiftKey);
            
            (new MochiKit.Signal.Event(this, e)).stop();
            
            return false;
        }
    },
    
    /**
     * @param isNextGroup [boolean] true for clockwise and false for counterclockwise
     * @param currentFocusedElement [DOM e] current focused element
     */
    moveGroupFocus: function (component, isNextGroup) {
        if(component != null) {
            function isNotNull (o){ 
                return o != null;
            }
            var tabOrderList = MochiKit.Base.filter(isNotNull, [this._topToolbar, this._leftPanel, this._reportAlbum]);
           
            if(tabOrderList.length <= 1)
                return;
           
            var index = MochiKit.Base.findIdentical(tabOrderList, component);
            
            if(index < 0)
                throw "PE, Failed to find focused component";
            else {
                var newIndex = 0;
                
                if(isNextGroup) {
                    newIndex = (index + 1) % tabOrderList.length;
                }
                else {
                    newIndex = index - 1;
                    if(newIndex < 0)
                        newIndex = tabOrderList.length - 1;
                }
                
                var nextComponent = tabOrderList[newIndex];                
                var isNextChildFocused = nextComponent.focusFirstChild();
                if(!isNextChildFocused)
                    this.moveGroupFocus(nextComponent, isNextGroup);
            }
            
        }
    },
    
    /**
     * Find's the group that element belongs to or null if not found.
     */
    findComponent : function (element) {
        if(element == null || element == this.layer)
            return null;
        else if (element == this._topToolbar.layer)
            return this._topToolbar;
        else if( element == this._leftPanel.layer)
            return this._leftPanel;
        else if (element == this._reportAlbum.layer)
            return this._reportAlbum;
        else if (element == this._statusbar.layer)
            return this._statusbar;
        else 
            return this.findComponent(element.parentNode);
    },
    
    addCanvasListener : function(canvasListener) {
        if( this._reportAlbum)
            this._reportAlbum.addCanvasListener(canvasListener);
    },
    
    
    removeCanvasListener : function(canvasListener) {
        if( this._reportAlbum)
            this._reportAlbum.removeCanvasListener(canvasListener);
    },
    
    fireActionEvent : function (signalName/* signal arguments*/) {
        var isHandled = false;
        if( this._actionListeners.length > 0) {
            var actionEvent = bobj.crv.ActionEventFactory.create(signalName, MochiKit.Base.extend(null, arguments, 1));
            if(actionEvent) {
                for (var i = 0; i < this._actionListeners.length; i++)
                    this._actionListeners[i].fire(signalName, actionEvent);
                
                isHandled = actionEvent.isHandled();
            }
        }
        
        return isHandled;
       
    },
    
    fireAndSignalActionEvent : function(signalName) {
        var isHandled = this.fireActionEvent.apply(this, arguments);
        if(!isHandled)
            MochiKit.Signal.signal.apply(null, MochiKit.Base.extend([this], arguments));
    },

    addActionListener : function(listener) {
        var hasListener = MochiKit.Base.findIdentical(this._actionListeners, listener) >= 0;
        if(!hasListener)
            this._actionListeners.push(listener);
    },
    
    removeActionListener : function(listener) {
        var index = MochiKit.Base.findIdentical(this._actionListeners, listener);
        if(index >= 0)
            this._actionListeners.splice(index,1);
    },

    _onUpdateLastPageNumber : function (groupPath, lastPageNumber, lastPageNumberKnown) {
        var numPagesString = lastPageNumber;
        if (!lastPageNumberKnown)
            numPagesString += '+';
        
        if(this._topToolbar)
            this._topToolbar.setPageNumber(null, numPagesString);
        
        MochiKit.Signal.signal(this, 'updateLastPage', groupPath, lastPageNumber, lastPageNumberKnown);
    },
    _onUpdatePageNumber : function (currentPage, totalPages) {
        if(this._topToolbar)
            this._topToolbar.setPageNumber(currentPage, totalPages)
        
        MochiKit.Signal.signal(this, 'updateCurrentPage', currentPage);
    },
    
    _onNavigatePage: function() {
        if(this._reportAlbum) {
            var activeView = this._reportAlbum.getSelectedView();
            if(activeView) {
                activeView.handlePageNavigationEvent.apply(activeView, arguments);
            }
        }
    },
    
    /**
     * DO NOT REMOVE. USED BY WEB ELEMENTS
     */
    getLeftPanel : function () {
        return this._leftPanel;
    },
    
    _initLeftPanelSignals : function () {
        var partial = MochiKit.Base.partial;
        var signal = MochiKit.Signal.signal;
        var connect = MochiKit.Signal.connect;
        var fe = MochiKit.Iter.forEach;
        
        if (this._leftPanel) {
            fe ( [ 'grpDrilldown', 'grpNodeRetrieveChildren', 'grpNodeCollapse', 'grpNodeExpand', 'resetParamPanel', 'resizeToolPanel', 'searchAll', 'selectSearchItem' ], function(sigName) {
                connect (this._leftPanel, sigName, this, partial(this.fireAndSignalActionEvent, sigName));
            }, this);

            connect (this._leftPanel, 'switchPanel', this, '_onSwitchPanel');
            connect(this._leftPanel, 'onSetDisplay', this, '_onLeftPanelSetDisplayListener');
        }
    },
    
    /**
     * returns true when main report view is selected in report album
     */
    _isMainReportViewSelected : function() {
        var currentView = this._reportAlbum.getSelectedView();
        return currentView && currentView.isMainReport();
    },
    
    _doLayoutOnLoad : function() {
        this.css.visibility = this._oldCssVisibility;
        this._doLayout();
    },
    
    _doLayout : function() {
        var topToolbarH = this._topToolbar ? this._topToolbar.getHeight() : 0;
        var topToolbarW = this._topToolbar ? this._topToolbar.getWidth() : 0;
        var separatorH = this._separator ? this._separator.getHeight() : 0;
        var statusbarH = this._statusbar ? this._statusbar.getHeight() : 0;
        var leftPanelW = this._leftPanel  ? this._leftPanel.getBestFitWidth() : 0;
        
        var leftPanelGrabberW = this._leftPanelResizeGrabber && this._leftPanelResizeGrabber.isDisplayed() ? 
                this._leftPanelResizeGrabber.getWidth() : 0;
        
        var layout = this.layoutType.toLowerCase();
        
        var toolPanel = this._leftPanel ? this._leftPanel.getToolPanel() : null;
        var hasPercentWidth = (toolPanel && toolPanel.isDisplayed() && toolPanel.hasPercentWidth());
        
        if (bobj.crv.Viewer.LayoutTypes.CLIENT == layout) {
            this.css.width = '100%';
            this.css.height = '100%';
            
            if (hasPercentWidth)
                leftPanelW = Math.max(leftPanelW, (this.getWidth() * toolPanel.getPercentWidth()) - leftPanelGrabberW);
        }
        else if (bobj.crv.Viewer.LayoutTypes.FITREPORT == layout) {
            var viewerWidth = 0;
            var viewerHeight = 0;
            
            if (hasPercentWidth)
                leftPanelW += 200;
            
            if(this._reportAlbum) {
                var albumSize = this._reportAlbum.getBestFitSize();
                viewerWidth = (albumSize.width + leftPanelW + leftPanelGrabberW < topToolbarW) ? topToolbarW  : albumSize.width + leftPanelW + leftPanelGrabberW + 2;
                viewerHeight = (albumSize.height + topToolbarH + separatorH + statusbarH + 2);  //2px for borders
            }
            else if (this._leftPanel) { /* If DisplayPage = false in webformviewer */
                viewerWidth = leftPanelW + 2;
                viewerHeight = (this._leftPanel.getBestFitHeight() + topToolbarH + separatorH + statusbarH + 2); //2px for borders
            }
            
            this.css.height = viewerHeight + 'px';
            this.css.width  = viewerWidth + 'px';
        }
        else { /* fixed layout */
            this.css.width = this.visualStyle.width;
            this.css.height = this.visualStyle.height;
            
            if (hasPercentWidth)
                leftPanelW = Math.max(leftPanelW, (this.getWidth() * toolPanel.getPercentWidth()) - leftPanelGrabberW);
        }
        
        if (this._topToolbar) {
            this._topToolbar.doLayout();
        }
        
        var albumW = Math.max(0, this.getWidth() - leftPanelW - leftPanelGrabberW - 2); // subtract 2 for the outer viewer border
        var albumH = Math.max(0, this.getHeight() - topToolbarH - separatorH - statusbarH - 2); // subtract 2 for the outer viewer border
        
        if (this._reportAlbum) {
            this._reportAlbum.resizeOuter(albumW, albumH);
            this._reportAlbum.move(leftPanelW + leftPanelGrabberW + ((bobj.crv.config.isRTL && _webKit)? 10 : 0), topToolbarH + separatorH);
        }
        
        if(this._leftPanel) {
            this._leftPanel.resize(leftPanelW, albumH);
            this._leftPanel.move(0, topToolbarH + separatorH);
        }
        
        if(this._leftPanelResizeGrabber && this._leftPanelResizeGrabber.isDisplayed()) {
            this._leftPanelResizeGrabber.resize(null, albumH);
            this._leftPanelResizeGrabber.move(leftPanelW, topToolbarH + separatorH)
        }
        
        if(this._statusbar) {
            this._statusbar.doLayout();
            this._statusbar.move(0, topToolbarH + separatorH + albumH)
        }

        if (this._print && this._print.layer) {
            this._print.center();
        }
        
        if (this._export && this._export.layer) {
            this._export.center();
        }

        if (this._reportProcessing && this._reportProcessing.layer) {
            this._reportProcessing.center();
        }
        
        
        var viewerP = MochiKit.Style.getElementPosition(this.layer);
        var viewerD = MochiKit.Style.getElementDimensions(this.layer);
        
        if (this._modalBackground)
            this._modalBackground.updateBoundary(viewerD.w, viewerD.h, viewerP.x, viewerP.y);
        
        var bodyD = bobj.getBodyScrollDimension();

        var isViewerCutOff = ((viewerP.x + viewerD.w) >= bodyD.w) || ((viewerP.y + viewerD.h) >= bodyD.h);

        if(isViewerCutOff && (layout !=  bobj.crv.Viewer.LayoutTypes.CLIENT)) {

            /* BoundaryControl adds a hidden div with the same dimension and position as current viewer to body
               to fix the problem of IE regarding scrollbar that are hidden when left + viewer's width > body's width
            */

            this._boundaryControl.updateBoundary(viewerD.w, viewerD.h, viewerP.x, viewerP.y);
        }
        else {
            this._boundaryControl.updateBoundary(0, 0, 0, 0);
        }

        var FLEXUI = bobj.crv.params.FlexParameterBridge;
        var swf = FLEXUI.getSWF(this.id);
        if(swf) {
            if (this._promptDlg && this._promptDlg.style.visibility != 'hidden') {
                if(swf._isMaximized) {
                    FLEXUI.fitScreen(this.id);
                }
                else {
                    FLEXUI.resize(this.id, swf.offsetHeight, swf.offsetWidth, true);
                }
            }
        }
        
        this._adjustWindowScrollBars();
    },
    
    _onSwitchPanel : function(panelType) {
        var Type = bobj.crv.ToolPanelType;

        if (Type.Search == panelType) {
            MochiKit.Signal.signal (this, 'showSearch');
        }else if (Type.GroupTree == panelType) {
            MochiKit.Signal.signal (this, 'showGroupTree');
        } else if (Type.ParameterPanel == panelType) {
            MochiKit.Signal.signal (this, 'showParamPanel');
        } else if (Type.None == panelType) {
            MochiKit.Signal.signal (this, 'hideToolPanel');
        }
        this._leftPanelResizeGrabber.setDisplay (!(Type.None == panelType));
        this._doLayout ();
    },
    
    resize : function(w, h) {
        if (bobj.isNumber (w)) {
            w = w + 'px';
        }

        if (bobj.isNumber (h)) {
            h = h + 'px';
        }

        this.visualStyle.width = w;
        this.visualStyle.height = h;
        this._doLayout ();
    },
    
    /** 
     * Set the page number. Updates toolbars with current page and number of pages
     * info.
     *
     * @param curPageNum [String]  
     * @param numPages   [String] (eg. "1" or "1+");
     */
    setPageNumber : function(curPageNum, numPages) {
        if (this._topToolbar) {
            this._topToolbar.setPageNumber (curPageNum, numPages);
        }
    },
    
    /**
     * Display the prompt dialog.
     *
     * @param html [string] HTML fragment to display inside the dialog's form.
     */
    showPromptDialog : function(html, closeCB) {
        if (!this._promptDlg) {
            var promptDialog_ShowCB = MochiKit.Base.bind (this._onShowPromptDialog, this);
            var promptDialog_HideCB = MochiKit.Base.bind (this._onHidePromptDialog, this);
            this._promptDlg = bobj.crv.params.newParameterDialog ( {
                id : this.id + '_promptDlg',
                showCB : promptDialog_ShowCB,
                hideCB : promptDialog_HideCB
            });
            this._promptDlg.setParent(this);
        }
        
        this._promptDlg.setCloseCB (closeCB);
        this._promptDlg.setNoCloseButton(!closeCB);

        /* The reason for saving document.onkeypress is that prompt dialog steals the document.onkeypress and never sets it back */
        this._originalDocumentOnKeyPress = document.onkeypress; /*
                                                                 * Must be set before .updateHtmlAndDisplay(html) as this function call modifies
                                                                 * document.onkeypress;
                                                                 */
        this.updatePromptDialog(html);
    },
    
    /**
     * Update the prompt dialog - with CR2010 DCP prompting this allow the dialog to be kept open during dependent prompt submits
     * 
     * @param html [string] HTML fragment to display inside the dialog's form
     */
    updatePromptDialog : function(html) {
        html = html || '';

        var callback = function(prompt, prompthtml) {
            return function() {
                prompt.updateHtmlAndDisplay (prompthtml);
            }
        };

        /**
         * AllInOne.js lacks prompting javascript files (to reduce amount of data transmitted) therefore, prompting data must be loaded on demand
         */

        bobj.loadJSResourceAndExecCallBack(bobj.crv.config.resources.HTMLPromptingSDK, callback(this._promptDlg, html));

        if (bobj.isParentWindowTestRunner ()) {
            /* for testing purposes only*/
            setTimeout (MochiKit.Base.partial (MochiKit.Signal.signal, this, "promptDialogIsVisible"), 5);
        }
    },
        
    showFlexPromptDialog : function(servletURL, closeCB, isInitializing) {
        
        var FLEXUI = bobj.crv.params.FlexParameterBridge;
        var VIEWERFLEX = bobj.crv.params.ViewerFlexParameterAdapter;
        isInitializing = typeof (isInitializing) !== 'undefined' ? isInitializing : false;

        if (!FLEXUI.checkFlashPlayer ()) {
            var msg = L_bobj_crv_FlashRequired;
            this.showError (msg.substr (0, msg.indexOf ('{0}')), FLEXUI.getInstallHTML ());
            return;
        }

        VIEWERFLEX.setViewerLayoutType (this.id, this.layoutType);
        
        if (!this._promptDlg) {
            this._promptDlg = document.createElement ('div');
            this._promptDlg.id = this.id + '_promptDlg';
            this._promptDlg.closeCB = closeCB;

            var PROMPT_STYLE = this._promptDlg.style;
            PROMPT_STYLE.border = '1px';
            PROMPT_STYLE.borderStyle = 'solid';
            PROMPT_STYLE.borderColor = '#000000';
            PROMPT_STYLE.position = 'absolute';
            PROMPT_STYLE.zIndex = bobj.constants.modalLayerIndex;

            var divID = bobj.uniqueId ();
            this._promptDlg.innerHTML = "<div id=\"" + divID + "\" name=\"" + divID + "\"></div>";
            
            // generate hidden buttons to prevent tabbing into the viewer
            var onfocusCB = bobj.bindFunctionToObject(bobj.crv.Viewer.keepFocus, this);
            
            var firstLink = MochiKit.DOM.createDOM('BUTTON', {
                id : this._promptDlg.id + '_firstLink',
                onfocus : onfocusCB,
                style : {
                    width : '0px',
                    height : '0px',
                    position : 'absolute',
                    left : '-30px',
                    top : '-30px'
                }
            });

            var lastLink = MochiKit.DOM.createDOM('BUTTON', {
                id : this._promptDlg.id + '_lastLink',
                onfocus : onfocusCB,
                style : {
                    width : '0px',
                    height : '0px',
                    position : 'absolute',
                    left : '-30px',
                    top : '-30px'
                }
            });
            
            document.body.appendChild (firstLink);
            document.body.appendChild (this._promptDlg);
            document.body.appendChild (lastLink);

            var state = bobj.crv.stateManager.getComponentState (this.id);
            var sessionID = state.common.reportSourceSessionID;
            var lang = bobj.crv.getLangCode ();
            var isRTL = bobj.crv.config.isRTL;

            FLEXUI.setMasterCallBack (this.id, VIEWERFLEX);
            FLEXUI.createSWF (this.id, divID, servletURL, true, lang, sessionID, isRTL, isInitializing);
        } else {
            this._promptDlg.closeCB = closeCB;
            this._promptDlg.style.display = '';
            FLEXUI.initViewer (this.id);
        }
        
        this.setDisplayModalBackground (true);
    },
    
    sendPromptingAsyncRequest : function (evArgs){
        MochiKit.Signal.signal(this, 'crprompt_asyncrequest', evArgs);
    },
    setDisplayModalBackground : function (isDisplay, showWaitCursor, opacity, filterOpacity) {
        isDisplay = this.isDisplayModalBG || isDisplay; //viewer.isDisplayModalBG has higher priority        
        if(this._modalBackground)
            this._modalBackground.show(isDisplay, showWaitCursor, opacity, filterOpacity);
    },
    
    _onShowPromptDialog : function() {
        this._adjustWindowScrollBars ();
        this.setDisplayModalBackground (true);
    },
    
    _onHidePromptDialog : function() {
        this._adjustWindowScrollBars ();
        document.onkeypress = this._originalDocumentOnKeyPress;
        this.setDisplayModalBackground (false);
    },
    
    isPromptDialogVisible: function () {
        return this._promptDlg && this._promptDlg.isVisible && this._promptDlg.isVisible (); 
    },
    
    hidePromptDialog : function() {
        if (this.isPromptDialogVisible()) {
            this._promptDlg.show (false);
        }
    },
    
    /**
     * Hide the flex prompt dialog
     */ 
    hideFlexPromptDialog : function() {
        if (this._promptDlg) {
            if (_ie || _ie9Up)
            {
                /* IE has an issue where if a user calls back from a swf
                 and closes the containing div then when the div is shown
                 again the swf will lose any external interface calls. To get around
                 this we must set the focus to something other than the swf first
                 before hiding the window. */

                // IE will throw error when focus on an invisible element.
                // Therefore, check if _promptDlg is visible before calling focus.
                if (this._promptDlg.style.display != 'none') 
                    this._promptDlg.focus();
            } 

            this._promptDlg.style.visibility = 'hidden';
            this._promptDlg.style.display = 'none';
            this.setDisplayModalBackground (false);
            
            if (this._promptDlg.closeCB)
                this._promptDlg.closeCB();
        }
    },
    
    _adjustWindowScrollBars : function() {
        if (_ie && this.layoutType == bobj.crv.Viewer.LayoutTypes.CLIENT && this._promptDlg && this._promptDlg.layer && MochiKit.DOM.currentDocument ().body) {
            var bodyOverFlow, pageOverFlow;
            var body = MochiKit.DOM.currentDocument ().body;
            var promptDlgLayer = this._promptDlg.layer;

            if (this.getReportPage () && this.getReportPage ().layer) {
                var reportPageLayer = this.getReportPage ().layer;
            }

            if (!window["bodyOverFlow"]) {
                window["bodyOverFlow"] = MochiKit.DOM.getStyle (body, 'overflow');
            }

            if (body.offsetHeight < (promptDlgLayer.offsetTop + promptDlgLayer.offsetHeight)) {
                if (window["bodyOverFlow"] == "hidden") {
                    bodyOverFlow = "scroll";
                }
                pageOverFlow = "hidden";
            } else {
                bodyOverFlow = window["bodyOverFlow"];
                pageOverFlow = "auto";
            }

            if(bodyOverFlow) {
                body.style.overflow = bodyOverFlow;
            }
            if (reportPageLayer) {
                reportPageLayer.style.overflow = pageOverFlow;
            }
        }
    },
    
    setComponentVisibility : function (componentName, isVisible) {
        var Components = bobj.crv.Viewer.Components;
        switch(componentName) {
        case Components.Toolbar:
            if(this._topToolbar) {
                this._topToolbar.setVisible(isVisible);
            }
            break;
        case Components.Statusbar:
            if(this._statusbar)
                this._statusbar.setVisible(isVisible);
            break;
        case Components.LeftPanel:
            if(this._leftPanel)
                this._leftPanel.setVisible(isVisible);
            break;
        case Components.Breadcrumb:
            if(this._reportAlbum && this._reportAlbum.getBreadcrumb())
                this._reportAlbum.getBreadcrumb().setVisible(isVisible);
        }
        
        this._doLayout();
    },
    
    /**
     * Display an error message dialog.
     *
     * @param text [String]    Short, user-friendly error message
     * @param details [String] Technical info that's hidden unless the user chooses to see it  
     */
    showError : function(text, details, errorCode, RCI) {
        var divDecode = document.createElement("div");
        divDecode.innerHTML = text;
        text = divDecode.innerHTML;
        if (details) {
        	divDecode.innerHTML = details;
        	details = divDecode.innerHTML;
        }
        
        var isDisplayErrorHandled = this.fireActionEvent("error", text, details, errorCode, RCI);
        if(!isDisplayErrorHandled) {
            var parent = this;
            setTimeout(function() {
                /**
                 * executing after 0 second daly to workaround an issue in IE where error dialog pops up on browser refresh and focus is not passed to 
                 * title of dialog
                 */
                var dlg = bobj.crv.ErrorDialog.getInstance ();
                dlg.setParent(parent);
                dlg.setText (text, details);
                dlg.setTitle (L_bobj_crv_Error);
                dlg.show (true);
            }, 0);
            
        }
    },
    
    /**
     * Update the UI using the given properties
     *
     * @param update [Object] Component properties 
     */
    update : function(update) {
        if (!update || update.cons != "bobj.crv.newViewer")
            return;
        
        if(update.args)
            this.isDisplayModalBG = update.args.isDisplayModalBG;
        
        /*
         * With CR2010 DCP prompting we want to keep open the prompt dialog until all parameters 
         * are resolved (ADAPT01346079). Unfortunately, as soon as the parameters resolved, server
         * calls the getPage and returns the page content, so we don't know when to close the dialog. 
         */
        this.hidePromptDialog();

        for ( var childNum in update.children) {
            var child = update.children[childNum];
            if (child) {
                switch (child.cons) {
                    case "bobj.crv.newReportAlbum":
                        if (this._reportAlbum) {
                            this._reportAlbum.update (child);
                        }
                        break;
                    case "bobj.crv.newToolbar":
                        if (this._topToolbar) {
                            this._topToolbar.update (child);
                        }
                        break;
                    case "bobj.crv.newStatusbar":
                        if (this._statusbar) {
                            this._statusbar.update (child);
                        }
                        break;
                    case "bobj.crv.newLeftPanel":
                        if (this._leftPanel)
                            this._leftPanel.update (child);
                        else {
                            this._leftPanel = bobj.crv.createWidget(child);
                            if(this.layer) {
                                var parentNode = this.layer.firstChild;
                                if(this._reportAlbum)
                                    parentNode.insertBefore(this._leftPanel.getDOM(),this._reportAlbum.layer);
                                else if(this._statusbar)
                                    parentNode.insertBefore(this._leftPanel.getDOM(), this._statusbar.layer);
                                else 
                                    parentNode.appendChild(this._leftPanel.getDOM());
                                    
                                this._initLeftPanelSignals ();
                                this._leftPanel.init();
                            }
                        }
                        break;
                    case "bobj.crv.newExportUI":
                        if (this._export) {
                            this._export.update (child);
                        }
                        break;
                }
            }
        }

        this._doLayout ();
        this.scrollToHighlighted ();
        this.setDisplayModalBackground (this.isDisplayModalBG);
    },
    
    pushLastPageNumberUpdate : function(json) {
        // Make sure we have everything we need to update the last page number.
        if ((typeof(json.lastPageNumber) === "undefined") || (typeof(json.groupNamePath) === "undefined"))
            return;
        
        if (this._reportAlbum)
            this._reportAlbum.pushLastPageNumber(json.lastPageNumber, json.groupNamePath);
    },
    
    getToolPanel : function() {
        if(this._leftPanel)
            return this._leftPanel.getToolPanel();
        
        return null;
    },
    
    getParameterPanel : function() {
        var toolPanel = this.getToolPanel ();
        if (toolPanel)
            return toolPanel.getParameterPanel ();

        return null;
    },
    
    getReportPage : function() {
        if (this._reportAlbum) {
            var view = this._reportAlbum.getSelectedView();
            if (view) { 
                return view.reportPage;
            }
        }  
        
        return null;
    },
    
    scrollToHighlighted : function() {
        if(!this._reportAlbum)
            return;
        
        var currentView = this._reportAlbum.getSelectedView ();
        // if the current view is not a SRV, highlight the target.
        if (currentView.scrollToHighlighted) {
            currentView.scrollToHighlighted(this.layoutType.toLowerCase () == bobj.crv.Viewer.LayoutTypes.FITREPORT);
        }
    },

    addViewerEventListener : function (e, l) {
        var ls = this._eventListeners[e];
        if (!ls) {
            this._eventListeners[e] = [l];
            return;
        }
    
        ls[ls.length] = l;
    },
    
    removeViewerEventListener : function (e, l) {
        var ls = this._eventListeners[e];
        if (ls) {
            for (var i = 0, lsLen = ls.length; i < lsLen; i++) {
                if (ls[i] == l){
                    ls.splice(i, 1);
                    return;
                }
            }
        }
    },
    
    getEventListeners : function (e) {
        return this._eventListeners[e];
    },
    
    recycle : function () {
        if(this._promptDlg) {
            bobj.crv.params.FlexParameterBridge.clearSWF(this.id); //in case its flex prompting
            MochiKit.DOM.removeElement(this._promptDlg);
            delete this._promptDlg;
        }
    },
    
    _onToolbarSetDisplayListener : function(isVisible) {
        if(this._separator)
            this._separator.setDisplay(isVisible);
    },
    
    _onLeftPanelSetDisplayListener : function(isVisible) {
        if(this._leftPanelResizeGrabber) {
            var isDisplayGrabber = isVisible;
            
            if(!this._leftPanel || !this.getToolPanel() || !this._leftPanel.isToolPanelDisplayed () || this.getToolPanel().getPanelType() == bobj.crv.ToolPanelType.None)
                isDisplayGrabber = false;
            
            this._leftPanelResizeGrabber.setDisplay(isDisplayGrabber);
        }
    },
    
    addActionsMenu : function(json) {
        if (json) {
            var widget = bobj.crv.createWidget(json);
            this._topToolbar.delayedAddChild(widget, 0, true);
        }
    }
};



bobj.crv.BoundaryControl = function(id) {    
    this.id = id;  
};

bobj.crv.BoundaryControl.prototype = {
    updateBoundary : function(width,height,left,top) {
        if(!this.layer) {
            this._init();
        }
        if(this.layer) {
            this.layer.style.width = width + "px";
            this.layer.style.height = height + "px";
            this.layer.style.left = left + "px";
            this.layer.style.top = top + "px";
        }
        
    },
    
    _getStyle : function () {
        return {
            display:'block',
            visibility:'hidden',
            position:'absolute'
        }; 
    },
    
    _getHTML : function () {
        return bobj.html.DIV({id : this.id, style : this._getStyle()});
    },
    
    _init: function() {
        if(!this.layer){
            append2(_curDoc.body,this._getHTML ());
            this.layer = getLayer(this.id);
            this.layer.onselectstart = function () {return false;};
            this.layer.onmousedown = eventCancelBubble;
            if (this.mouseupCB)
                this.layer.onmouseup = this.mouseupCB;
        }
    }
};

bobj.crv.ModalBackground = function (id, mouseupCB) {
    this.id = id;
    this.mouseupCB = mouseupCB;
};

bobj.crv.ModalBackground.prototype = new bobj.crv.BoundaryControl();
MochiKit.Base.update(bobj.crv.ModalBackground.prototype, {
    _getStyle : function () {
        return {
            'background-color' : '#888888',
            position : 'absolute',
            opacity : 0.30,
            display : 'block',
            filter : 'alpha(opacity=30);',
            'z-index' : bobj.constants.modalLayerIndex - 2,
            visibility : 'hidden',
            cursor : 'auto'
        }; 
    },
    
    show : function (show, showWaitCursor, opacity, filterOpacity) {
        if(!this.layer) {
            this._init();
        }
        
        this.layer.style.visibility = show ? "visible" : "hidden";
        this.layer.style.cursor = show && showWaitCursor ? "wait" : "auto";
        this.layer.style.opacity = show && opacity != undefined ? opacity : 0.30;
        
        if (this.layer.style.filter)
            this.layer.style.filter = show && filterOpacity ? filterOpacity : 'alpha(opacity=30);';
    }

});

/**
 * ViewerManager is a singleton that enables registering of listeners for viewer init event
 */
bobj.crv.ViewerManager = new function () {
    var initListeners = {/*viewerID, [listener]*/};
    this.addOnViewerInitListener = function(viewerID, listener) {
        if(initListeners[viewerID] == null) {
            initListeners[viewerID] = [];
        }
        
        initListeners[viewerID].push(listener);
    };
    
    this.onViewerInit = function(viewer) {
        if(viewer != null) {
            var id = viewer.id;
            if(initListeners[id] != null) {
                var listeners = initListeners[id];
                for(var i = 0 ; i < listeners.length; i++)
                    listeners[i].call(window, viewer);
            }
        }
    }
};