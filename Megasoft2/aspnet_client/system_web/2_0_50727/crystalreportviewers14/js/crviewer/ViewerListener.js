/* Copyright (c) Business Objects 2006. All rights reserved. */

/**
 * ViewerListener Constructor. Handles Viewer UI events.
 */
 
if(typeof(bobj.crv.Async) == 'undefined') {
    bobj.crv.Async = {};
}

bobj.crv.ViewerListener = function(viewerName, ioHandler) {
    this._name = viewerName;
    this._viewer = null;
    this._promptPage = null; 
    this._paramCtrl = null;
    this._ioHandler = ioHandler;
    this._reportProcessing = null;
    this._prevCtxt = null;
    this._prevSubCtxt = null;
    this._requestDispatchCount = 0;
    
    var connect = MochiKit.Signal.connect;
    var subscribe = bobj.event.subscribe;
    var bind = MochiKit.Base.bind;
    
    var widget = window[viewerName];
    if (widget) {
        if (widget.widgetType == 'Viewer') {
            this._viewer = widget;
            this._reportProcessing = this._viewer._reportProcessing;
        }
        else if (widget.widgetType == 'PromptPage') {
            this._promptPage = widget;    
            this._reportProcessing = this._promptPage._reportProcessing;
        }
    }
    
    if (this._viewer) {
        // ReportAlbum events
        connect(this._viewer, 'selectView', this, '_onSelectView');
        connect(this._viewer, 'removeView', this, '_onRemoveView');
        connect(this._viewer, 'getPage', this, '_onGetPage');
        connect(this._viewer, 'breadcrumbNavigate', this, '_onBreadcrumbNavigate');

        // Toolbar events
        connect(this._viewer, 'selectHistory', this, '_onSelectHistory');
        connect(this._viewer, 'clearHistory', this, '_onClearHistory');
        connect(this._viewer, 'firstPage', this, '_onFirstPage');
        connect(this._viewer, 'prevPage', this, '_onPrevPage');
        connect(this._viewer, 'nextPage', this, '_onNextPage');
        connect(this._viewer, 'lastPage', this, '_onLastPage');
        connect(this._viewer, 'selectPage', this, '_onSelectPage');
        connect(this._viewer, 'findLastPageNumber', this, '_onfindLastPageNumber');
        connect(this._viewer, 'zoom', this, '_onZoom');
        connect(this._viewer, 'drillUp', this, '_onDrillUp');
        connect(this._viewer, 'refresh', this, '_onRefresh');  
        connect(this._viewer, 'export', this, '_onExport');
        connect(this._viewer, 'print', this, '_onPrint');
        connect(this._viewer, 'updateCurrentPage', this, '_onUpdateCurrentPage');
        connect(this._viewer, 'updateLastPage', this, '_onUpdateLastPage');
        
        // Tool Panel events
        connect(this._viewer, 'resizeToolPanel', this, '_onResizeToolPanel');
        connect(this._viewer, 'hideToolPanel', this, '_onHideToolPanel');
        connect(this._viewer, 'grpDrilldown', this, '_onDrilldownGroupTree');
        connect(this._viewer, 'grpNodeRetrieveChildren', this, '_onRetrieveGroupTreeNodeChildren');
        connect(this._viewer, 'grpNodeCollapse', this, '_onCollapseGroupTreeNode');
        connect(this._viewer, 'grpNodeExpand', this, '_onExpandGroupTreeNode');
        connect(this._viewer, 'showSearch', this, '_onShowSearch');
        connect(this._viewer, 'showParamPanel', this, '_onShowParamPanel');
        connect(this._viewer, 'showGroupTree', this, '_onShowGroupTree');
        connect(this._viewer, 'viewChanged',  this, '_onChangeView');
        connect(this._viewer, 'resetParamPanel',  this, '_onResetParamPanel');
        connect(this._viewer, 'printSubmitted', this, '_onSubmitPrintPdf');
        connect(this._viewer, 'exportSubmitted', this, '_onSubmitExport');
        connect(this._viewer, 'initialized', this, '_onViewerInitialization');
        connect(this._viewer, 'searchAll', this, '_onSearchAll');
        connect(this._viewer, 'selectSearchItem', this, '_onSelectSearchItem');
    }
    
    // Report Page Events
    subscribe('drilldown', this._forwardTo('_onDrilldown')); 
    subscribe('drilldownGraph', this._forwardTo('_onDrilldownGraph'));
    subscribe('drilldownSubreport', this._forwardTo('_onDrilldownSubreport'));
    subscribe('sort', this._forwardTo('_onSort'));
    subscribe('hyperlinkClicked', this._forwardTo('_onHyperlinkClicked'));
    subscribe('displayError', this._forwardTo('_displayError'));
    subscribe('refresh', this._forwardTo('_onRefresh')); 
    subscribe('zoom', this._forwardTo('_onZoom')); 
    subscribe('setParameters', this._forwardTo('applyParams')); 
    subscribe('setReportSource', this._forwardTo('setReportSource')); 
    subscribe('closeReportSource', this._forwardTo('closeReportSource')); 
    subscribe('setPageNumber', this._forwardTo('setPageNumber')); 
    subscribe('setComponentVisibility', this._forwardTo('setComponentVisibility'));
    
    // Prompt events
    subscribe('crprompt_param', this._forwardTo('_onSubmitStaticPrompts'));
    subscribe('crprompt_pmtEngine', this._forwardTo('_onSubmitPromptEnginePrompts'));
    subscribe('crprompt_logon', this._forwardTo('_onSubmitDBLogon'));
    subscribe('crprompt_cancel',this._forwardTo('_onCancelParamDlg'));
    subscribe('crprompt_flexparam', this._forwardTo('_onFlexParam'));
    subscribe('crprompt_flexlogon', this._forwardTo('_onFlexLogon'));
    subscribe('crprompt_asyncrequest', this._forwardTo('_onPromptingAsyncRequest'));

    
    // Report Part Events
    subscribe('pnav', this._forwardTo('_onNavigateReportPart'));
    subscribe ('navbookmark', this._forwardTo('_onNavigateBookmark'));
    subscribe ('updatePromptDlg', this._forwardTo('_onPromptDialogUpdate'));

    // Universal Events, No Target Checks
    subscribe('saveViewState', bind(this._onSaveViewState, this));
    subscribe('waitingModalBackground', this._forwardTo('_showWaitingModalBackground'));

    // Take Action Events
    subscribe('takeaction_showMenuButton', this._forwardTo('_onTakeActionShowMenuButton'));
    subscribe('takeaction_hideMenuButton', this._forwardTo('_onTakeActionHideMenuButton')); 
    subscribe('takeaction_accessibleKeyUp', this._forwardTo('_onTakeActionAccessibleKeyUp')); 
    subscribe('takeaction_asyncRequest', this._forwardTo('_onTakeActionAsyncRequest')); 
    subscribe('batchExecuteEvent', this._forwardTo('_onBatchExecuteEvent'));

    if(widget) {
        widget.init(); //must initialize widget after connecting event listeners as signals might be thrown during initialization
    }
        
    if(bobj.isObject(window.jsUnit) && !window.jsUnit.testSuite) {
        window.jsUnit.testSuite = new jsUnit.crViewerTestSuite(this);
    } 
    //This is a performance improvement -- we load the prompting component in parallel.
    //We have to run the initialization on the load event or else the swf object embedding
    //has performance problems.
    var doLoad = function()
    {
        if (widget.initialPromptData != null)
        {
            var FLEXUI = bobj.crv.params.FlexParameterBridge;
            if (typeof(FLEXUI._isInitializing) == 'undefined') {
                bobj.crv.params.ViewerFlexParameterAdapter.setPromptData(
                        widget.id, widget.initialPromptData, false);
                var closeCB = this._getPromptDialogCloseCB();
                widget.showFlexPromptDialog(this.getServletURI(), closeCB, true);
                widget.initialPromptData = null;
            }
        }                
    };
    doLoad = MochiKit.Base.bind(doLoad, this);
    //We have to run the on the load event to properly embed the swf in the foreground.
    swfobject.addLoadEvent(doLoad);
};

bobj.crv.ViewerListener.prototype = {

    /**
     * Public
     *
     * @return The current report view 
     */
     getCurrentView: function() {
         if(this._viewer && this._viewer._reportAlbum) {
             return this._viewer._reportAlbum.getSelectedView();
         }
         
         return null;
     },
     
     getPromptingType : function () {
         return this._getCommonProperty('promptingType');
     },
     
     _displayError : function(args) {
         args = MochiKit.Base.parseQueryString(args);
         
         if (args && this._viewer ) { 
             var errorMessage = args.errorMessage || L_bobj_crv_RequestError;
             var debug = args.debug || "";
             this.showError(errorMessage, debug);
         }  
     },
     
    /**
     * Private. Wraps an event handler function with an event target check.
     *
     * @param handlerName [String]  Name of the event handler method
     *
     * @return Function that can be used as a callback for subscriptions
     */
    _forwardTo: function(handlerName) {
        return MochiKit.Base.bind(function(target) {
            if (target == this._name) {
                var args = bobj.slice(arguments, 1);
                this[handlerName].apply(this, args);
            }
        }, this);    
    },
    
    _onViewerInitialization: function (isLoadContentOnInit) {
        if(isLoadContentOnInit) {
            this._initialLoadViewerContent ();
        }
    },

    _onSaveViewState: function() {
        this._saveViewState();
    },

    _onSelectView: function(view) {
        if (view) {
            bobj.crv.logger.info('UIAction View.Select');
            
            // Since "restore state" happens before events, we need to 
            // change the curViewId before making the server request
            var state = bobj.crv.stateManager.getComponentState(this._name);
            if (state) {
                state.curViewId = view.viewStateId;
                this._request({selectView: view.viewStateId}, bobj.crv.config.useAsync, true);
            }
        }
    },
    
    _onRemoveView: function(view) {
        if (view) {
            bobj.crv.logger.info('UIAction View.Remove');
            var viewerState = bobj.crv.stateManager.getComponentState(this._name);
            if (viewerState) {
                // Remove view from viewer state
                delete viewerState[view.viewStateId];
            }
            
            var commonState = this._getCommonState();
            if (commonState) {
                // Remove view from taborder
                var idx = MochiKit.Base.findValue(commonState.rptAlbumOrder, view.viewStateId);
                if (idx != -1) {
                    arrayRemove(commonState, 'rptAlbumOrder', idx);   
                }
            }
        }
    },
    
    _onBreadcrumbNavigate: function(breadcrumbNavigateArgs) {
        bobj.crv.logger.info('UIAction Breadcrumb.Navigate');
        this._request(breadcrumbNavigateArgs, bobj.crv.config.useAsync, true, null, null, bobj.crv.ActionEvents.DRILL);    
    },
    
    _initialLoadViewerContent: function() {
        bobj.crv.logger.info('UIAction InitLoad');
        this._request({ajaxInitLoad : true}, bobj.crv.config.useAsync, true);
    },
    
    _onSelectHistory: function(dir) {
        bobj.crv.logger.info('UIAction Toolbar.SelectHistory');
        this._request({tb:'selectHistory', histPos: dir}, bobj.crv.config.useAsync, true);
    },
    
    _onClearHistory: function() {
        bobj.crv.logger.info('UIAction Toolbar.ClearHistory');

        // clear the history in the common state
        this._setCommonProperty('backHist', []);
        this._setCommonProperty('fwdHist', []);
        
        // clear the history in the toolbar
        this._viewer._topToolbar.clearHistory();
    },
    
    _showWaitingModalBackground: function(show) {
        if (this._viewer)
            this._viewer.setDisplayModalBackground (show, true, 0, 'alpha(opacity=0)');
    },
    
    _onTakeActionShowMenuButton : function(event, element, window, isFocusIn) {
    	var instance = bobj.crv.TakeAction.getInstance(window.frameElement);
    	instance.showDropDownButton(event, element, this._name, isFocusIn);
    },
    
    _onTakeActionHideMenuButton : function(event, element, delay) {
    	var instance = bobj.crv.TakeAction.getInstance();
    	instance.hideDropDownButton(event, element, delay);
    },
    
    _onTakeActionAccessibleKeyUp : function(event) {
    	var instance = bobj.crv.TakeAction.getInstance();
    	instance.accessibleKeyUp(event);
    },
    
    /**
     * Sends asynchronous request through ioadapter. 
     * @param eventArgs
     * @param onSuccess
     * @param onFailure
     * @return
     */
    _onTakeActionAsyncRequest : function(eventArgs, onSuccess, onFailure) {
        var ACTION_SERVICE_STATE_KEY = "actionServiceState";
        this._ioHandler.addRequestField ('ServletTask', 'TakeActionEvent');
        for(var key in eventArgs) {
            this._ioHandler.addRequestField (key, eventArgs[key]);
            //Adding eventArgs to the form instead of CREventArguments as request is not handled by ViewerContainer
        }

        var actionServiceState = this._getCommonProperty(ACTION_SERVICE_STATE_KEY);
        if(actionServiceState)	
            this._ioHandler.addRequestField(ACTION_SERVICE_STATE_KEY, this._getCommonProperty(ACTION_SERVICE_STATE_KEY));
        
        function getJsonResponse(res) {
            var json = null;
            if (bobj.isString(res)) {
                json = MochiKit.Base.evalJSON(res);
            } else {
                if(res instanceof  MochiKit.Async.XMLHttpRequestError)/*when error is thrown on async failure*/
                    json = MochiKit.Async.evalJSONRequest(res.req);
                else
                    json = MochiKit.Async.evalJSONRequest(res);
            }
            
            return json;
        }

        var viewerListener = this;
        
        var successCB = function (res) {
            viewerListener._requestDispatchCount--;

            // Used for .NET only as a workaround for error callbacks. We're suppose to throw an exception in
            // the method RaiseCallbackEvent from CrystalReportViewer.cs where .NET will take care of doing the error
            // redirect but it still calls GetCallbackResult to form the response text which looks something like this.
            //     e<Exception.message>0|<GetCallbackResult>
            // so instead of parsing that response decided to handle the error as a success instead and check for the
            // status code.
            if(onSuccess) 
            {
            	var jsonResponse = getJsonResponse(res);
                if(jsonResponse[ACTION_SERVICE_STATE_KEY] != null) {
                    viewerListener._setCommonProperty(ACTION_SERVICE_STATE_KEY, jsonResponse[ACTION_SERVICE_STATE_KEY]);
                }
            	if (jsonResponse.StatusCode == 500)
            	{
            	    onFailure(jsonResponse.CallbackResult);
            	}
            	else
            	{
            	    onSuccess(jsonResponse);
            	}
            }
        };
        
        var failureCB = function (res) {
            viewerListener._requestDispatchCount--;
            if(onFailure) 
                onFailure(getJsonResponse(res));
        };
                
        var state = {};
        state[this._name] = {}; //emty state
        var deferred = this._ioHandler.request(state, this._name, {/*evArgs*/}, true, false, successCB, failureCB); //Adds CB for .Net
        this._requestDispatchCount++;
        if (deferred) {  
            //add CBs for Java DHTML Viewer
            deferred.addCallback(successCB);
            deferred.addErrback(failureCB);
        }
    },
        
    /**
     * Used by JS API
     */
    setPageNumber : function (pageNumber) {
        this._onGetPage(pageNumber, true, true, true);
    },
    
    _onFirstPage: function() {
        this._onGetPage(1, true, true, true);
    },
    
    _onPrevPage: function() {
        var targetPage = this._getViewProperty('pageNum') - 1;
        this._onGetPage(targetPage, true, true, true);
    },
    
    _onNextPage: function() {
        var targetPage = this._getViewProperty('pageNum') + 1;
        this._onGetPage(targetPage, true, true, true);
    },
    
    _onLastPage: function() {
        this._onGetPage(-1, true, true, true);
    },
    
    _onSelectPage: function(pgNum) {
        if (pgNum == -1)
            pgNum = NaN;
        this._onGetPage(pgNum, true, true, true);     
    },
    
    _onGetPage: function(pgNum, isTopPage, isAllowIncompletePageCount, isDisplayProgressIndicator) {
        bobj.logToConsole("getPage" + pgNum);
        bobj.crv.logger.info('Get Page' + pgNum);
        this._request({'getPage': pgNum.toString(), 'isTopPage': isTopPage, isAllowIncompletePageCount : isAllowIncompletePageCount}, bobj.crv.config.useAsync, isDisplayProgressIndicator);   
    },

    _onDrillUp: function() {
        bobj.crv.logger.info('UIAction Toolbar.DrillUp');
        this._request({tb:'drillUp'}, bobj.crv.config.useAsync, true);
    },
    
    _onfindLastPageNumber : function () {
        bobj.crv.logger.info('Find last page number');
        this._request({'findLastPageNumber' : 1}, true, false, null, MochiKit.Base.bind(this._onFindLastPageNumberResponse, this));
    },
    
    _onChangeView: function() {
        if(this._paramCtrl) {
            this._paramCtrl.onChangeView();
        }
    },
    
    _onResetParamPanel: function() {
        if(this._isResettingParamPanel) {
            return;
        }
        
        this._isResettingParamPanel = true;
        
        if(this._paramCtrl) {
            this._paramCtrl.setParameters([]);
            delete this._paramCtrl;
        }  
        
        this.clearAdvancedPromptData();
        
        var onFinishCB = bobj.bindFunctionToObject( function () { 
            this._isResettingParamPanel = false; 
        }, this);
        
        this._setInteractiveParams(onFinishCB);
    },
        
    _onZoom: function(zoomTxt) {
        bobj.crv.logger.info('UIAction Toolbar.Zoom ' + zoomTxt);
        this._request({tb:'zoom', value:zoomTxt}, bobj.crv.config.useAsync, true);    
    },
    
    _onExport: function(closeCB) {
        var exportComponent = this._viewer._export;
        if (exportComponent) {
            if (closeCB) exportComponent.setCloseCB (closeCB);
            
            bobj.crv.logger.info('UIAction Toolbar.Export');
            exportComponent.show(true);
        }
    },
    
    _onBatchExecuteEvent : function(events) {
        this._request({'batchExecuteEvent' : events}, bobj.crv.config.useAsync, true);  
        bobj.crv.logger.info('Batch Execute events');
    },
        
    _onPrint: function(closeCB) {
        var printComponent = this._viewer._print;
        if (printComponent) {
            if (closeCB) printComponent.setCloseCB (closeCB);
            
            if (printComponent.isActxPrinting) {
                bobj.crv.logger.info('UIAction Toolbar.Print ActiveX');
                var pageState = bobj.crv.stateManager.getCompositeState();
                var postBackData = this._ioHandler.getPostDataForPrinting(pageState, this._name);
                this._viewer._print.show(true, postBackData);
            }
            else {
                var isOneClickPrint = this._getCommonProperty('pdfOCP') && bobj.hasPDFReaderWithJSFunctionality(); 
                if (isOneClickPrint) {
                    this._onSubmitPrintPdf(0, 0, isOneClickPrint);
                }
                else {
                    bobj.crv.logger.info('UIAction Toolbar.Print PDF');
                    this._viewer._print.show(true);
                }
            }
        }
    },

    _onResizeToolPanel: function(width) {
        this._setCommonProperty('toolPanelWidth', width);
        this._setCommonProperty('toolPanelWidthUnit', 'px');
    },
    
    _onHideToolPanel: function() {
        bobj.crv.logger.info('UIAction Toolbar.HideToolPanel');
        this._setCommonProperty('toolPanelType', bobj.crv.ToolPanelType.None);
    },
    
    _onShowSearch: function() {
        bobj.crv.logger.info('UIAction Toolbar.ShowSearch');
        this._setCommonProperty('toolPanelType', bobj.crv.ToolPanelType.Search);
    },
    
    _onShowParamPanel: function() {
        bobj.crv.logger.info('UIAction Toolbar.ShowParamPanel');
        this._setCommonProperty('toolPanelType', bobj.crv.ToolPanelType.ParameterPanel);
    },
    
    _onShowGroupTree: function() {
        bobj.crv.logger.info('UIAction Toolbar.ShowGroupTree');
        this._setCommonProperty('toolPanelType', bobj.crv.ToolPanelType.GroupTree);
    },
    
    _onDrilldown: function(drillargs) {
        bobj.crv.logger.info('UIAction Report.Drilldown');
        this._request(drillargs, bobj.crv.config.useAsync, true, null, null, bobj.crv.ActionEvents.DRILL);
    },
    
    _onDrilldownSubreport: function(drillargs) {
        bobj.crv.logger.info('UIAction Report.DrilldownSubreport');
        this._request(drillargs, bobj.crv.config.useAsync, true, null, null, bobj.crv.ActionEvents.DRILL);
    },
    
    _onDrilldownGraph: function(event, graphName, branch, offsetX, offsetY, pageNumber, nextpart, twipsPerPixel) {
        if (event) {
            bobj.crv.logger.info('UIAction Report.DrilldownGraph');
            var mouseX, mouseY;
            
            if(_ie || _saf || _ie11Up) {
                mouseX = event.offsetX;
                mouseY = event.offsetY;                
            }
            else {
                mouseX = event.layerX;
                mouseY = event.layerY;                
            }
            
            var zoomFactor = parseInt(this._getCommonProperty('zoom'), 10);
            zoomFactor = (isNaN(zoomFactor) ? 1 : zoomFactor/100);
            
            this._request({ name:encodeURIComponent(graphName),
                            brch:branch,
                            coord:(mouseX*twipsPerPixel/zoomFactor + parseInt(offsetX, 10)) + '-' + (mouseY*twipsPerPixel/zoomFactor +parseInt(offsetY, 10)),
                            pageNumber:pageNumber,
                            nextpart:encodeURIComponent(nextpart)}, 
                            bobj.crv.config.useAsync, 
                            true,
                            null,
                            null,
                            bobj.crv.ActionEvents.DRILL);
        }
    },
    
    _onDrilldownGroupTree: function(groupName, groupPath, isVisible, groupNamePath, nodeType) {
        bobj.crv.logger.info('UIAction GroupTree.Drilldown');
        var encodedGroupName = encodeURIComponent(groupName);
        var evtArgs = {drillname: encodedGroupName, gnpath: groupNamePath, type:nodeType};
        var actionEventType = null;
        if(isVisible) {
            evtArgs.grp = groupPath;
            actionEventType = bobj.crv.ActionEvents.GROUP_TREE_NAVIGATE;
        }
        else {
            evtArgs.brch = groupPath;
            actionEventType = bobj.crv.ActionEvents.DRILL;
        }
         
        this._request(evtArgs, bobj.crv.config.useAsync, true, null, null, actionEventType);
    },
    
    _onRetrieveGroupTreeNodeChildren: function(groupPath) {
        this._request({grow:groupPath}, bobj.crv.config.useAsync, true);
    },
    
    /*
     * Removes groupPath from currentExpandedPaths in current view's state
     */
    _onCollapseGroupTreeNode: function(groupPath) {
        bobj.crv.logger.info('UIAction GroupTree.CollapseNode');
        var expPathPointer = this.getCurrentExpandedPaths(); // expPathPointer is assigned to expanded paths in memory
        var groupPathArray = groupPath.split('-');

        for(var i = 0 , end = groupPathArray.length - 1; i <= end ; i++) {
            var nodeID = groupPathArray[i];
            if(expPathPointer[nodeID]) {
                if(i == end) {
                    delete expPathPointer[nodeID];
                    return;
                }
                expPathPointer = expPathPointer[nodeID];
            }
            else {
                return;
            }
        }         

    },
    
    showError: function(message,detailText, errorCode, RCI) {
        if(this._viewer) {
            this._viewer.showError(message, detailText, errorCode, RCI);
        }
    },
    
    _onExpandGroupTreeNode: function(groupPath) {
        bobj.crv.logger.info('UIAction GroupTree.ExpandNode');
        var expPathPointer = this.getCurrentExpandedPaths();
        var groupPathArray = groupPath.split('-');
        // Iterates through groupPath, and adds indxes to currentExpandedPaths if not existing
        for(var i = 0 , end = groupPathArray.length; i < end; i++) {
            var nodeID = groupPathArray[i];
            if(!expPathPointer[nodeID]) {
                expPathPointer[nodeID] = {};
            }
            expPathPointer = expPathPointer[nodeID];
        }   
        
    },
    
    _onRefresh: function() {
        bobj.crv.logger.info('UIAction Toolbar.Refresh');

        var commonState = this._getCommonState();
        var useAsyncForRefresh = true;
        if (commonState && commonState.useAsyncForRefresh !== undefined) {
            useAsyncForRefresh = commonState.useAsyncForRefresh;
        }
        
        //Since refresh creates a new processingjob on server, we must cancel current search as it's searching on current job
        this._viewer.resetSearch();
        
        this._request({tb:'refresh'}, bobj.crv.config.useAsync && useAsyncForRefresh, true, false, null, null, true);
    },

    _onSearchAll: function(searchText, isNewSearch, isCaseSensitive, isMatchWholeWordOnly, isFindFirstN, lastPageNumber, remainingGroupPaths, numberOfProcessedGroupPath, searchSubreportRequestContext, searchDrilldownGroupPath) {
        bobj.crv.logger.info('UIAction Toolbar.Search');
        
        var request = MochiKit.Base.bind(this._request, this, {'searchAll':encodeURIComponent(searchText), 'isNewSearch':isNewSearch, 'isMatchCase':isCaseSensitive , 
            'isMatchWholeWordOnly':isMatchWholeWordOnly, 'isFindFirstN':isFindFirstN,  'lastPageNumber':lastPageNumber, 'remainingGroupPaths':remainingGroupPaths, 
            'numberOfProcessedGroupPath':numberOfProcessedGroupPath, 'searchSubreportRequestContext':searchSubreportRequestContext,
            'searchDrilldownGroupPath':searchDrilldownGroupPath}, true, false)
        if(this._requestDispatchCount > 0 )
                setTimeout(request, 500);
        else
            request();
            
    },
    
    
    _onSelectSearchItem: function(drillContext) {
        this._request({'selectSearchItem': drillContext} , bobj.crv.config.useAsync, true);
    },
    
    
    /**
     * Full Prompt page should use post back request even if ajax is enabled as it is unable to process the ajax response
     */
    _canUseAsync : function () {
        return this._viewer != null && bobj.crv.config.useAsync;
    },
    
    _onFlexParam: function(paramData){
        this._request({'crprompt':'flexPromptingSetValues', 'paramList':paramData, 'isFullPrompt': true}, this._canUseAsync(), true);
    },
    
    _onFlexLogon: function(logonData) {
        for (var i = 0, len = logonData.length; i < len; i++) {
            this._addRequestField(logonData[i].field, logonData[i].value);
        }

        this._request({'crprompt':'logon'}, this._canUseAsync(), true);
    },
    
    /**
     * 
     * @param isFullPrompt [String] a flag indicating whether the current prompt is full prompt which refreshes report content
     * @return
     */
    _onSubmitPromptEnginePrompts: function(isFullPrompt) {

        isFullPrompt = eval(isFullPrompt); //converting it to boolean type
        var useAjax = this._viewer && this._viewer.isPromptDialogVisible(); //use ajax request when prompt dialog is visible
        /*
         * this._name is used to get a unique id for prompt variables
         * For dhtml and JSF viewer it will be the name of the viewer
         * For webform viewer it will be the 'unique UI id'
         */
        var valueIDKey = 'ValueID' + this._name;
        var contextIDKey = 'ContextID' + this._name;
        var contextHandleIDKey = 'ContextHandleID' + this._name;
        
        var _valueId = null;
        var _contextId = null;
        var _contextHandleId = null;
        

        var valueID = document.getElementById(valueIDKey);
        if (valueID) {
            _valueId = encodeURIComponent(valueID.value);
        }
        
        var contextID = document.getElementById(contextIDKey);
        if (contextID) {
            _contextId = encodeURIComponent(contextID.value);
        }
        
        var contextHandleID = document.getElementById(contextHandleIDKey);
        if (contextHandleID) {
            _contextHandleId =  encodeURIComponent(contextHandleID.value);
        }
        
        // When not doing ajax, _reportProcessing don't have a parent to position the processing dialog to the 
        // center (refer to DialogBoxWidget_center in dialog.js) thus always being show top left.
        // The Solution is to provide a parent just for the purpose of centering with the width and height of the window. 
        if (!useAjax) {
            var parentObj = { layer : { offsetWidth : winWidth(), offsetHeight : winHeight() } };
            this._reportProcessing.setParent(parentObj);
        }
        
        this._request({'crprompt':'pmtEngine', 'isFullPrompt': isFullPrompt, "ValueID" : _valueId, "ContextHandleID" : _contextHandleId, "ContextID" : _contextId, "isAjaxEnabled" : bobj.crv.config.useAsync}, useAjax, true);
        
        // These elements are dynamically created - we should delete them as soon after their job is done.
        this._removeRequestField(valueIDKey);
        this._removeRequestField(contextIDKey);
        this._removeRequestField(contextHandleIDKey);
        
    },
      
    _onSubmitStaticPrompts: function(formName) {
        this._addRequestFields(formName);
        this._request({'crprompt':'param'}, false, true);
    },
    
    _onSubmitDBLogon: function(formName) {
        var useAjax = this._viewer && this._viewer.isPromptDialogVisible(); //use ajax request when prompt dialog is visible
        
        if(this._viewer)
            this._viewer.hidePromptDialog(); 
        this._addRequestFieldsFromContent(formName);
        /**
         * use async when viewer is loaded (instead of promptpage) and it's allowed to make async requests
         */

        // When not doing ajax, _reportProcessing don't have a parent to position the processing dialog to the 
        // center (refer to DialogBoxWidget_center in dialog.js) thus always being show top left.
        // The Solution is to provide a parent just for the purpose of centering with the width and height of the window. 
        if (!useAjax) {
            var parentObj = { layer : { offsetWidth : winWidth(), offsetHeight : winHeight() } };
            this._reportProcessing.setParent(parentObj);
        }
        
        this._request({'crprompt':'logon'}, useAjax, true);
    },
    
    _onSubmitPrintPdf: function (start, end, isOneClickPrint) {
        this._handlePrintOrExport (start, end, 'PDF', isOneClickPrint);
    },
    
    _onSubmitExport: function (start, end, format) {
        this._handlePrintOrExport (start, end, format);
    },
    
    _handlePrintOrExport: function (start, end, format, isOneClickPrint) {
        var isRange = true;
        var useIframe = false;
        if (!start && !end) {
            isRange = false;
        }
        
        if (!format) {
            format = 'PDF';
        }
        
        var isOneClickPDFPrinting = (format == 'PDF' && isOneClickPrint);
        var reqObj = {text:format, range:isRange+''};
        reqObj.tb = isOneClickPDFPrinting ? 'crpdfprint' : 'crexport';
        if (isRange) {
            reqObj['from'] = start + '';
            reqObj['to'] = end + '';
        }

         bobj.crv.logger.info('UIAction Export.Submit ' + format);
        // we want to redirect export requests to a Servlet (ASP should do nothing different)
        if (this._ioHandler instanceof bobj.crv.ServletAdapter || this._ioHandler instanceof bobj.crv.FacesAdapter) {
            useIframe = true; //always use iframe for exporting/printing in Java DHTML Viewer
            this._ioHandler.redirectToServlet ();
            this._ioHandler.addRequestField ('ServletTask', 'Export');
            this._ioHandler.addRequestField ('LoadInIFrame', useIframe);
        }
        else if(this._ioHandler instanceof bobj.crv.AspDotNetAdapter){
            useIframe = true;//always use iframe for exporting/printing in ASP
        }
        else {
            useIframe = isOneClickPDFPrinting;
        }
        this._request(reqObj, false, false, useIframe);
    },
    
    _onCancelParamDlg: function() {
        bobj.crv.logger.info('UIAction PromptDialog.Cancel');
        this._viewer.hidePromptDialog();
    },
    
    _onReceiveParamDlg: function(html) {
        this._viewer.showPromptDialog(html);
    },
    
    _onSort: function(sortArgs) {
        bobj.crv.logger.info('UIAction Report.Sort');
        this._request(sortArgs, bobj.crv.config.useAsync, true);  
    },
    
    _onNavigateReportPart: function(navArgs) {
        bobj.crv.logger.info('UIAction ReportPart.Navigate');
        this._request(navArgs, false, true);
    },
    
    _onNavigateBookmark: function(navArgs) {
        bobj.crv.logger.info('UIAction Report.Navigate');
        this._request(navArgs, bobj.crv.config.useAsync, true);        
    },
    
    getCurrentExpandedPaths: function() {
        var viewState = this._getViewState();
        if(viewState) {
            return viewState.gpTreeCurrentExpandedPaths;
        }

        //This return shouldn't get exectued
        return {};
    },
    
    /**
     * Clears viewer's state and UI in case report engine type changes.
     * 
     */
    recycle : function () {
        this._setCommonProperty('parameterFields', []); /* Must clear parameter fields if new reportsource does not have any parameters(Will not render paramPanel*/
        this._clearPendingEvents();
        this._onResetParamPanel();
        this._viewer.recycle();
    },
    
    setReportSource: function(reportIdType, reportId, boeLogonType, boeLogonString, locale) {
        this.recycle();
        
        if (arguments[0] instanceof SAP.CR.InProcReportSource) //InProc reportsource
        {
        	var inProcReportSourceVar = arguments[0];
        	reportId = inProcReportSourceVar.getReportId();
        	reportId = reportId + ''; //convert reportid to string in case it's a number
        	
        	var ebisId = inProcReportSourceVar.getEBISId();
        	var productLocale = inProcReportSourceVar.getProductLocale();
        	var docLocale = inProcReportSourceVar.getDocLocale();
        	
        	this._request({ setReportSource: 'InProc', reportId : reportId, ebisId : ebisId, 
        		productLocale : productLocale ? productLocale : "en-US", 
    			docLocale : docLocale ? docLocale : "en-US"}, bobj.crv.config.useAsync, true); 
        }
        else //managed report source
        {
        	// when only 1 argument is passed in, we are passing in the report source factory session id
        	if (arguments.length == 1)
        		this._request({ setReportSource: 'fromBOE', factorySessionID : arguments[0]}, bobj.crv.config.useAsync, true);  
        	else
        		this._request({ setReportSource: 'fromBOE', reportIdType : reportIdType, reportId : reportId, 
        			boeLogonType : boeLogonType, boeLogonString : boeLogonString, 
        			locale : locale ? locale : "en-US"}, bobj.crv.config.useAsync, true); 
        }
    },
    
    closeReportSource: function() {
        this._request({ closeReportSource: true }, bobj.crv.config.useAsync);
    },
    
    setComponentVisibility : function (component, isVisible) {
        var Components = bobj.crv.Viewer.Components;
        var stateName = null;
        switch(component) {
        case Components.Toolbar:
            stateName = "isDisplayToolbar";
            break;
        case Components.Statusbar:
            stateName = "isDisplayStatusBar";
            break;
        case Components.LeftPanel:
            stateName = "isDisplayLeftPanel";
            break;
        case Components.Breadcrumb:
            stateName = "isDisplayBreadCrumb";
            break;
        }
        
        if(stateName)
            this._setCommonProperty(stateName, isVisible);
        
        if(this._viewer)
            this._viewer.setComponentVisibility(component,isVisible);
    },
    
    applyParams: function(params) {
        // TODO Dave can we just set the parsm into state since they've 
        // gone through client side validation?
        if (params) {
            bobj.crv.logger.info('UIAction ParameterPanel.Apply');
            var clonedParams = [];
            var cloneFunc = MochiKit.Base.clone;
            for(var i = 0, len = params.length; i < len; i++) {
                var clonedParam = cloneFunc(params[i]);
                clonedParam.modifiedValue = null;
                clonedParam.value = cloneFunc(params[i].value); /* _encodeParameter modifes deep copy of value so I have to make a copy of it before encoding it */
                if (this._ioHandler instanceof bobj.crv.ServletAdapter || this._ioHandler instanceof bobj.crv.FacesAdapter) {
                    this._encodeParameter(clonedParam);
                }
                clonedParams.push(clonedParam);
            }         
            

            //Since param apply creates a new processingjob on server, we must cancel current search as it's searching on current job
            this._viewer.resetSearch();
            
            this._request({crprompt: 'paramPanel', paramList: clonedParams}, bobj.crv.config.useAsync, true, false, null, null, true);            
        }
    },
    
    getServletURI : function () {
        var servletURL = "";
        if (this._ioHandler instanceof bobj.crv.ServletAdapter
                || this._ioHandler instanceof bobj.crv.FacesAdapter) {
            servletURL = this._ioHandler._servletUrl;
        }
        
        return servletURL;
    },
    
    /**
    * The loading time of HTML prompting resources is changed from initialization to on demand to reduce the initial loading time.
    * When a request for advance dialog is triggered, resources are loaded prior to sending the request to server
    * @param callBack
    * @return
    */
    showAdvancedParamDialog : function(param) {
        var paramOpts = this._getCommonProperty('paramOpts');
        if(!paramOpts.canOpenAdvancedDialog) {
            this.showError(L_bobj_crv_AdvancedDialog_NoAjax, L_bobj_crv_EnableAjax);
        }
        else {
            this._focusedParamName = param.paramName;
            
            if (this._isPromptingTypeFlex()) {    
                var flexAdapter = bobj.crv.params.ViewerFlexParameterAdapter;
                flexAdapter.setCurrentIParamInfo (this._name, this._paramCtrl, param);
                    
                if (!flexAdapter.hasIParamPromptUnitData(this._name)) {
                    /* Need to fetch the interactive prompt data*/
                    this._request({promptDlg: this._cloneParameter(param)}, true, true); 
                } else {
                    /* Already have enough data to prompt*/
                    /* Creating 5+ range controls can take a long time */
                    if (param.allowMultiValue && param.allowRangeValue &&
                        param.modifiedValue.length > 5) {
                        if (this._reportProcessing) {
                            this._reportProcessing.Show();
                        }
                    }
                    var closeCB = this._getPromptDialogCloseCB();
                    this._viewer.showFlexPromptDialog(this.getServletURI(), closeCB, false);
                }
            } else {
                 /* Need to fetch the interactive parameter HTML*/
                this._request({promptDlg: this._cloneParameter(param)}, true, true);    
            }
        }
    },
    
    _cloneParameter : function(param) {
        var clonedParam = MochiKit.Base.clone(param);
        clonedParam.defaultValues = null; // ADAPT00776482
        clonedParam.modifiedValue = null;
        if (this._ioHandler instanceof bobj.crv.ServletAdapter || this._ioHandler instanceof bobj.crv.FacesAdapter) {
            // we need to clone the value again
            clonedParam.value = MochiKit.Base.clone(param.value);
        }
        return clonedParam;
    },
    
    _encodeParameter: function(p) {
        // we only worry about the "%" sign in the "string" values, param name and the report name.
        // ignore the other properties of the parameter because they are not used on the server side
        if (p) {
            if (p.value && (p.valueDataType == bobj.crv.params.DataTypes.STRING || p.valueDataType == bobj.crv.params.DataTypes.MEMBER)) {
                for (var i = 0, valuesLen = p.value.length; i < valuesLen; i++) {
                    var paramValue = p.value[i];
                    if (bobj.isString(paramValue)) {    // single discrete parameter
                        paramValue = encodeURIComponent(paramValue);
                    }
                    else if (bobj.isObject(paramValue)) {
                        var isRangeValue = paramValue.beginValue || paramValue.endValue;
                        if (isRangeValue) {    // single/multi range parameter
                            if (paramValue.beginValue) {
                                paramValue.beginValue = this._encodeParameterValue (paramValue.beginValue);
                                paramValue.beginDisplay = this._encodeParameterValue (paramValue.beginDisplay);
                            }
                            if (paramValue.endValue) {
                                paramValue.endValue = this._encodeParameterValue (paramValue.endValue);
                                paramValue.endDisplay = this._encodeParameterValue (paramValue.endDisplay);
                            }
                        }
                        else {    // multi discrete parameter
                            if (paramValue.value) {
                                paramValue.value = this._encodeParameterValue (paramValue.value);
                            }
                            if (paramValue.displayValue) {
                                paramValue.displayValue = this._encodeParameterValue (paramValue.displayValue);
                            }
                            if (paramValue.desc) {
                                paramValue.desc = this._encodeParameterValue (paramValue.desc);
                            }
                        }
                    }
                    p.value[i] = paramValue;
                }
            }
            
            if (p.paramName) {
                p.paramName = encodeURIComponent(p.paramName);
            }
            
            if (p.reportName) {
                p.reportName = encodeURIComponent(p.reportName);
            }
        }
        
        return p;
    },
    
    _encodeParameterValue : function (paramValue) {
        var actualValue = bobj.crv.params.getValue(paramValue);
        return encodeURIComponent (actualValue);
    },
    
    /**
     * Set a property in the state associated with the current report view
     *
     * @param propName [String]  The name of the property to set
     * @param propValue [Any]    The value of the property to set
     */
    _setViewProperty: function(propName, propValue) {
        var viewState = this._getViewState();
        if (viewState) {
            viewState[propName] = propValue;    
        }    
    },
    
    /**
     * Get a property in the state associated with the current report view
     *
     * @param propName [String]  The name of the property to retrieve
     */
    _getViewProperty: function(propName) {
        var viewState = this._getViewState();
        if (viewState) {
            return viewState[propName];
        }
        return null;
    },
    
    /**
     * Set a property that's shared by all report views from the state 
     *
     * @param propName [String]  The name of the property to set
     * @param propValue [String]  The value to set
     */
    _setCommonProperty: function(propName, propValue) {
        var state = this._getCommonState();
        if (state) {
            state[propName] = propValue;
        }
    },
    
    /**
     * Get a property that's shared by all report views from the state 
     *
     * @param propName [String]  The name of the property to retrieve
     */
    _getCommonProperty: function(propName) {
        var state = this._getCommonState();
        if (state) {
            return state[propName];
        }
        return null;
    },
    
    /**
     * Set the UI properties to match the state associated with viewId
     *
     * @param viewId [String - optional]  
     */ 
    _updateUIState: function(viewId) {
        
    },
    
    /**
     * Get the state associated with the current report view
     *
     * @return State object or null 
     */
    _getViewState: function() {
        var compState = bobj.crv.stateManager.getComponentState(this._name);
        if (compState && compState.curViewId !== undefined) {
            return compState[compState.curViewId];
        }
        return null;
    },
    
    /**
     * Get the state that's common to all report views
     *
     * @return State object or null
     */ 
    _getCommonState: function() {
        var compState = bobj.crv.stateManager.getComponentState(this._name);
        if (compState) {
            return compState.common;
        }
        return null;
    },
        
    /**
     * Create CRPrompt instances from interactive parameters in state and pass 
     * them to the Viewer widget so it can display them in the parameter panel.
     */
     _setInteractiveParams : function(onFinishCB) {
         if (!this._ioHandler.canUseAjax()) {
             var paramPanel = this._viewer.getParameterPanel();
             if (paramPanel) {
                 paramPanel.showError(L_bobj_crv_InteractiveParam_NoAjax);
             }
             onFinishCB();
             return;
         }

         var unusedParamList = [];
         var usedParamList = [];
         var paramList = this._getCommonProperty('parameterFields');

         if (paramList) {
             var Parameter = bobj.crv.params.Parameter;
             for ( var i = 0; i < paramList.length; i++) {
                 var param = new Parameter(paramList[i]);
                 if (param.isInteractive())
                     usedParamList.push(param);
                 else
                     unusedParamList.push(param);
             }
         }
         
         if (usedParamList && usedParamList.length) {
             var callback = function (viewerListener, paramList, unusedParamList) {
                 return function () {
                     var paramPanel = viewerListener._viewer.getParameterPanel();
                     if (paramPanel) {
                         var paramOpts = viewerListener._getCommonProperty('paramOpts');
                         var controller = new bobj.crv.params.ParameterController(paramPanel, viewerListener, paramOpts);
                         controller.setParameters(paramList, onFinishCB);
                         controller.setUnusedParameters(unusedParamList);
                         viewerListener.setParameterController(controller);
                     }
                 }
             }
             
             bobj.loadJSResourceAndExecCallBack(bobj.crv.config.resources.ParameterControllerAndDeps, callback(this, usedParamList, unusedParamList));
         }
         else {
             onFinishCB();
         }
     },
    
     
    _isPromptingTypeFlex: function() {
        var type = this.getPromptingType();
        return (type && type.toLowerCase() == bobj.crv.Viewer.PromptingTypes.FLEX);
    },
    
    setParameterController : function (controller) {
        this._paramCtrl = controller;
    },
    
    clearAdvancedPromptData: function() {
        if (this._isPromptingTypeFlex()){
            bobj.crv.params.ViewerFlexParameterAdapter.clearIParamPromptUnitData(this._name);
        }
    },
    
    _onPromptDialogUpdate: function(update) {
        if (update.resolvedFields) {
            this._viewer.hidePromptDialog(); 

            if(this._paramCtrl) {
                for (var i = 0; i < update.resolvedFields.length; i++) {
                    var param = new bobj.crv.params.Parameter(update.resolvedFields[i]);
                    this._paramCtrl.updateParameter(param.paramName, param.getValue());
                }
                this._paramCtrl._updateToolbar();
            }
        }
        else {
            if (this._isPromptingTypeFlex()) {
                if(update.script) {
                    /* update.script contains JavaScript code for updating ViewerFlexParameterAdapter class*/
                    bobj.evalInWindow(update.script); 
                    var closeCB = this._getPromptDialogCloseCB();
                    this._viewer.showFlexPromptDialog(this.getServletURI(), closeCB, false);
                }
            }
            else 
            {
                if(update.html) {
                    if (this._viewer.isPromptDialogVisible()) {
                        this._viewer.updatePromptDialog(update.html);
                    }
                    else {
                        var closeCB = this._getPromptDialogCloseCB();
                        this._viewer.showPromptDialog(update.html, closeCB);
                    }
                }
            }
            
        }    
    },

    _getPromptDialogCloseCB: function () {
        var closeCB = null;
        if (this._paramCtrl && this._focusedParamName) {
            closeCB = this._paramCtrl.getFocusAdvButtonCB(this._focusedParamName);
            
            /* Clear the name so that if the dialog is used for a full prompt */
            /* it won't have the focus callback */
            this._focusedParamName = null;
        }
        return closeCB;
    },
    
    _onPromptingAsyncRequest: function(evArgs) {
        this._request(evArgs, true, false, false);
    },
    
    /**
     * Add evArgs to pending events of viewer
     * 
     * @param evArgs, JSON representation of event 
     * @return
     */
    _addToPendingEvents : function(evArgs) {
        var pendingEvents = this._getCommonProperty('pendingEvents');
        if(!pendingEvents)
            pendingEvents = {};
        
        var index = 0;
        while(index in pendingEvents)
            index++;
        
        pendingEvents[index] = ("\"" + MochiKit.Base.queryString(evArgs) + "\"");
        
        this._setCommonProperty('pendingEvents', pendingEvents);
    },
    
    _clearPendingEvents : function () {
        this._setCommonProperty('pendingEvents', {});
    },
    
    _request: function(evArgs, allowAsynch, showIndicator, useIframe, callback, actionEventType, isWaitForPendingRequests) {
        var pageState = bobj.crv.stateManager.getCompositeState();
        var bind = MochiKit.Base.bind;
        var defaultCallback = callback ? callback : bind(this._onResponse, this, arguments, actionEventType);
        var defaultErrCallback = bind(this._onIOError, this);
        
        var ctxt = this._getViewProperty('vCtxt');
        this._prevCtxt = ctxt ? {gpNme:ctxt.gpNme, gpPath:ctxt.gpPath, gpNamePath:ctxt.gpNamePath} : null;
        var subCtxt = this._getViewProperty('srptRqtCtxt');
        this._prevSubCtxt = subCtxt ? {pX:subCtxt.pX, pY:subCtxt.pY, srptNm:subCtxt.srptNm} : null;

        if (this._reportProcessing && showIndicator) {
            this._reportProcessing.delayedShow ();
        }
        
        function sendRequest(listener, pageState, evArgs, allowAsynch, useIframe, defaultCallback, defaultErrCallback, isWaitForPendingRequests) {
            if(isWaitForPendingRequests && listener._requestDispatchCount > 0) {
                /**
                 * Waiting for all other requests to complete before proceding with current one. Requests that can potentially change the procJob should wait for all pending requests
                 * to avoid inconsistent data being displayed in viewer's report page (data from different job).
                 */
                setTimeout(function () {
                    sendRequest(listener, pageState, evArgs, allowAsynch, useIframe, defaultCallback, defaultErrCallback, isWaitForPendingRequests);
                }, 500);
                
                return;
            }
            
            if(!useIframe) {
                //default callbacks are not executed in case of printing/exporting when requesting through an iframe
                listener._requestDispatchCount++;
            }
            var deferred = listener._ioHandler.request(pageState, listener._name, evArgs, allowAsynch, useIframe, defaultCallback, defaultErrCallback);

            if (deferred) {            
                if (listener._reportProcessing && showIndicator) {
                    listener._reportProcessing.setDeferred (deferred);
                }
            
                deferred.addCallback(defaultCallback);
                deferred.addErrback(defaultErrCallback);
            }    
        }
        
        sendRequest(this, pageState, evArgs, allowAsynch, useIframe, defaultCallback, defaultErrCallback, isWaitForPendingRequests);
    },
    
    _onResponse: function(requestArgs, actionEventType, response) {
        this._requestDispatchCount--;
        var json = null;
        if (bobj.isString(response)) {
            json = MochiKit.Base.evalJSON(response);
        } else {
            try {
                json = MochiKit.Async.evalJSONRequest(response);
            }
            catch (e) {
                if (typeof (JSON) != "undefined") {
                    json = MochiKit.Async.parseJSONRequest(response);
                }
                else {
                    json = bobj.external.json_parse.parseJSONRequest(response);
                }
            }
        }

        if (json) {
            var isActionHandled = actionEventType != null;
            var jsonState = json.state;
            if (jsonState) {
                if (bobj.isString(jsonState)) {
                    jsonState = MochiKit.Base.evalJSON(jsonState);
                }
                
                isActionHandled = this._handleActionEvent(actionEventType, jsonState, requestArgs);
            }
            
            if (json.isRepeatRequest) {
                this._addToPendingEvents({ajaxInitLoad : true, isRepeatRequest : true}); /*forces rerendering of all components*/
                this._request.apply(this, requestArgs);
                return;
            }

            if (json.needsReload) {
                this._setCommonProperty('isReloadRequest', true);
                var evArgs = requestArgs[0];
                this._request(evArgs, false, true);
                return;
            }
            
            if(json.redirect) {
                window.location = json.redirect;
                return;
            }
            
            if (json.status && this._viewer && (json.status.errorMessage || json.status.debug) ) { 
                var errorMessage = json.status.errorMessage || L_bobj_crv_RequestError;
                this.showError(errorMessage, json.status.debug, json.status.code, json.status.RCI);
            }
                
            if (!isActionHandled) {
                if (jsonState) {
                    bobj.crv.stateManager.setComponentState(this._name, jsonState);
                }
                
                if (json.update) {
                    if (json.update.promptDlg) {
                        this._onPromptDialogUpdate(json.update.promptDlg);
                        bobj.crv.logger.info('Update InteractiveParams');    
                    }
                    else if (this._viewer) {
                        if (bobj.crv.params.FlexParameterBridge._isInitializing == true && this._isPromptingTypeFlex())
                        {
                            //In the case of parallel flash loading, the flash dialog will be active -- we should remove it.  See BOERPTA-1631
                            this._viewer.hideFlexPromptDialog();    
                        }
                         
                        this._viewer.update(json.update);
                        bobj.crv.logger.info('Update Viewer');
                    }
                }
            }
            
            if (json.script && json.script.length > 0) {                
                bobj.evalInWindow(json.script);
                bobj.crv.logger.info('Execute Script');
            }
        }
        
        var isShowIndicator = requestArgs[2];
        if (this._reportProcessing && isShowIndicator) {
            this._reportProcessing.cancelShow ();
        }
        
        if(bobj.isParentWindowTestRunner())
            MochiKit.Signal.signal(this._viewer, "updated");
    },    
    _onIOError: function(response) { 
        this._requestDispatchCount--;
        if (this._reportProcessing.wasCancelled () == true) {
            return;
        }
        
        if (this._reportProcessing) {
            this._reportProcessing.cancelShow ();
        }
        
        if (this._viewer) {
            var error = this._ioHandler.processError (response);
            var detailText = '';
            if (bobj.isString(error)) {
              detailText = error;
            }
            else {
                for (var i in error) {
                    if (bobj.isString(error[i]) || bobj.isNumber(error[i])) {
                        detailText += i + ': ' + error[i] + '\n';     
                    }
                }
            }

            this.showError(L_bobj_crv_RequestError, detailText);    
        }
    },
    
    _onFindLastPageNumberResponse: function(response) {
        this._requestDispatchCount--;
        
        var json = MochiKit.Async.evalJSONRequest(response);
        if (json && json.nonRenderUpdate && json.nonRenderUpdate.lastPageNumberUpdate) {
            if (this._viewer) {
                this._viewer.pushLastPageNumberUpdate(json.nonRenderUpdate.lastPageNumberUpdate);
                bobj.crv.logger.info('Update Last Page Number');
            }
        }
    },    
    
    _handleActionEvent: function(actionEventType, jsonState, requestArgs) {
        var isActionHandled = false;
        var vCtxtChanged = true;
        var srptRqtCtxtChanged = true;

        var groupName, groupPath, groupNamePath;
        if (actionEventType == bobj.crv.ActionEvents.DRILL) {
            var view = jsonState[jsonState.curViewId];
            var srptRqtCtxt = view.srptRqtCtxt;
            var vCtxt = view.vCtxt;
            vCtxtChanged = view.vCtxtChanged;
            srptRqtCtxtChanged = view.srptRqtCtxtChanged;
            
            if(vCtxtChanged == undefined || srptRqtCtxtChanged == undefined)
                return false;
            else if (srptRqtCtxt && srptRqtCtxtChanged) {
                isActionHandled = this._viewer.fireActionEvent(actionEventType, null, null, null);
            }
            else {
                if (vCtxt && vCtxtChanged) {
                    groupName = vCtxt.gpNme ? decodeURIComponent(vCtxt.gpNme) : null;
                    groupPath = vCtxt.gpPath ? vCtxt.gpPath : null;
                    groupNamePath = vCtxt.gpNamePath ? vCtxt.gpNamePath : null;
                    isActionHandled = this._viewer.fireActionEvent(actionEventType, groupName, groupPath, groupNamePath);
                }
            }
        }
        else if (actionEventType == bobj.crv.ActionEvents.GROUP_TREE_NAVIGATE) {
            var args = requestArgs[0];
            groupName = args.drillname ? decodeURIComponent(args.drillname) : null;
            groupPath = args.grp ? args.grp : null;
            groupNamePath = args.gnpath ? args.gnpath : null;
            isActionHandled = this._viewer.fireActionEvent(actionEventType, groupName, groupPath, groupNamePath);
        }
        
        return isActionHandled || (!srptRqtCtxtChanged && !vCtxtChanged);
    },
    
    _saveViewState: function() {
        var pageState = bobj.crv.stateManager.getCompositeState();
        this._ioHandler.saveViewState(pageState, this._name);
    },
    
    /**
     * Private. Retrieve all children of the given form and add them to the request.
     *
     * @param formName [String]  Name of the form
     */
    _addRequestFields: function(formName) {
        var frm = document.getElementById(formName);
        if (frm) {
            for (var i in frm) {
                var frmElem = frm[i];
                if (frmElem && frmElem.name && frmElem.value) {
                    this._addRequestField(frmElem.name, frmElem.value);
                }
            }
        }
    },

    /**
     * Private. Retrieve all input fields inside the content div element and add them to the request.
     *
     * @param contentId [String]  Id of the containing div element
     */
    _addRequestFieldsFromContent: function(contentId) {
        var parent = document.getElementById(contentId);
        if (!parent)
            return;

        var elements = MochiKit.DOM.getElementsByTagAndClassName("input", null, parent);
            
        for (var i in elements) {
            var inputElement = elements[i];
            if(inputElement.type && inputElement.type.toLowerCase() == "checkbox" && inputElement.name) {
                if(inputElement.checked)
                    this._addRequestField(inputElement.name, inputElement.value );
            }
            else if (inputElement && inputElement.name && inputElement.value) {
                this._addRequestField(inputElement.name, inputElement.value);
            }
        }
    },
    
    /**
     * Private. Add the given name and value as a request variable.
     *
     * @param fldName [String]  Name of the field
     * @param fldValue [String] Value of the field
     */
    _addRequestField: function(fldName, fldValue) {
        this._ioHandler.addRequestField(fldName, fldValue);
    },
    
    /**
     * Private. Add the given name and value as a request variable.
     *
     * @param fldName [String]  Name of the field
     */
    _removeRequestField: function(fldName) {
        this._ioHandler.removeRequestField(fldName);
    },
    
    _onHyperlinkClicked: function(args) {
        args = MochiKit.Base.parseQueryString(args);
        var ls = this._viewer.getEventListeners('hyperlinkClicked');
        var handled = false;
        if (ls) {
            for (var i = 0, lsLen = ls.length; i < lsLen; i++) {
                if (ls[i](args) == true){
                    handled = true;
                }
            }
        }
        
        if (handled) {return;}

        var w = window;
        if (args.target && args.target != '_self'){
            w.open(args.url, args.target);
        } else {
            w.location = args.url;
        }
        
    },
    
    _onUpdateCurrentPage : function (pageNum) {
        this._setViewProperty('pageNum', pageNum);
        
        // FIXME 4.2 - Note we need to create a single entry point such as an adapter
        // that knows how to change context values. The adapters supported will
        // be Java and .NET
        
        // Update the page number for .NET json objects
        var ctxt = this._getViewProperty('vCtxt');
        if (ctxt != null && bobj.isString(ctxt))
        {
            // update the page number
            var jsonCtxt = MochiKit.Base.evalJSON(ctxt);
            if (jsonCtxt.prc != null && jsonCtxt.prc.pn != null)
            {
                jsonCtxt.prc.pn = pageNum;

                // set and serialize back the json vCtxt
                this._setViewProperty('vCtxt', MochiKit.Base.serializeJSON(jsonCtxt));
            }
        }
    },
    
    _onUpdateLastPage : function (groupNamePath, lastPageNum, lastPageNumKnown) {
        var lastPages = this._getCommonProperty('lastPageNumbers');
        if (lastPages)
            lastPages[groupNamePath] = lastPageNum;
        var lastPageKnowns = this._getCommonProperty('lastPageNumberKnowns');
        if (lastPageKnowns)
            lastPageKnowns[groupNamePath] = lastPageNumKnown;
    }
};
