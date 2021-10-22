SAP.common.defineNS("SAP.CR.Viewer", {
    /**
     * Loads viewer with name[viewerName] placed in container[divID]
     * 
     * @param containerID
     *            [String], id of element that will hold the viewer
     * @param viewerName
     *            [String], name of viewer
     * @param initCB[function(instance)]
     *            a function that initializes the viewer instance
     * @param failCB[function(instance,error)],
     *            a function that gets executed when something fails
     */
    create : function(viewerName, containerID, initCB, failCB) {

        var failCBWrapper = function(instance, error) {
            try {
                if (failCB)
                    failCB(instance, error);
                else
                    throw error;
            } catch (err) {
                throw err;
            }
        };
        // validate the viewer name, must begin with a letter and may be followed by any number of letters or digits
        if (!viewerName.match (/^[0-9a-zA-Z]+$/) || !viewerName.charAt(0).match (/^[a-zA-Z]+$/))
        {
            failCBWrapper (null, SAP.CR.Viewer.Exceptions.ViewerInitException.create(L_bobj_crv_API_ViewerInitFailed, L_bobj_crv_API_InvalidViewerName.replace("{0}", viewerName)));
        }

        var instance = new SAP.CR.Viewer.Instance(viewerName, containerID);
        SAP.common.instances[viewerName] = instance;

        try {
            if (initCB)
                initCB(instance)
        } catch (err) {
            failCBWrapper(instance, SAP.CR.Viewer.Exceptions.ViewerInitException.create(L_bobj_crv_API_ViewerInitFailed, err));
        }

        var ViewerLoadException = SAP.CR.Viewer.Exceptions.ViewerLoadException;

        var loadInstance = function(instance) {
            var args = {
                'ServletTask' : 'CreateViewer',
                'pendingEvents' : MochiKit.Base.serializeJSON(instance.getPendingEvents()),
                'viewerName' : instance.getName(),
                'containerID' : instance.getContainerID(),
                'servletUri' : SAP.common.getServletUri(),
                'resourcePrefix' : SAP.common.resourceUri,
                'productLocale' : SAP.common.productLocale
            };

            function onSucess(data, textStatus, XMLHttpRequest) {
                var html = bobj.html.extractScripts(data);
                var scripts = html.scripts;
                if (scripts.length > 0) {
                    for ( var iScripts = 0, scriptsLen = scripts.length; iScripts < scriptsLen; ++iScripts) {
                        var script = scripts[iScripts];
                        if (!script)
                            continue;

                        if (script.text) {
                            var text = script.text.replace("<!--", "").replace("//-->", ""); // IE cannot evaluate script with <!--
                            try {
                                bobj.evalScript (text);
                            } catch (err) {
                                failCBWrapper(instance, ViewerLoadException.create(L_bobj_crv_API_ViewerLoadFailed, err));
                            }
                        }
                    }
                }
                //If there are no scripts to parse, there must be an error. Display the error message.
                else {
                    var containerId = instance.getContainerID();
                    var viewerElem = document.getElementById(containerId);
                    if (viewerElem)
                        viewerElem.innerHTML = data;
                }
            }

            var onFail = function(err) {
                failCBWrapper(instance, ViewerLoadException.create(L_bobj_crv_API_ViewerLoadFailed, err));
            };

            SAP.common.ajax(args, onSucess, onFail);
        }

        try {
            loadInstance(instance);
        } catch (err) {
            failCBWrapper(instance, ViewerLoadException.create(L_bobj_crv_API_ViewerLoadFailed, err));
        }
    },

    getInstance : function(name) {
        return SAP.common.instances[name];
    }
});

SAP.common.defineNS("SAP.CR.Viewer.Instance", function(viewerName, parentID) {
    var container = parentID;
    var name = viewerName;
    var isInitialized = false;
    /**
     * Forces functions to queue their events even if the viewer is initialized during batchExecute().
     */
    var isForceQueue = false;           
    var pendingEvents = new Array();
    var viewer = null;
    var initCallbackQueue = [];

    /**
     * Once actual viewer is loaded in the page, the callback function below is called
     */
    bobj.crv.ViewerManager.addOnViewerInitListener(name, function(viewerRef) {
        viewer = viewerRef;
        isInitialized = true;
        pendingEvents = [];
        while (initCallbackQueue.length > 0)
            initCallbackQueue.pop().call();
    });

    /**
     *  Will return true if events should be queued. isForceQueue is true during batchExecute().
     *  Functions that cannot operate after viewer has been initialized should not call this function.
     *  
     */
    function isQueueEvents() {
        return (!isInitialized) || isForceQueue;
    }
    
    this.setReportSource = function(infoObjectId, token, locale) {
        this.Internal.setReportSource(infoObjectId, this.Internal.BOELogonTypeEnum.LogonToken, token, locale);
    };
    
    this.setHyperlinkTarget = function(target) {
        this.Internal.setHyperlinkTarget(target);
    };
    
    this.closeReportSource = function()
    {
        bobj.event.publish('closeReportSource', name);
    };
    
    /**
     * 
     * @param parameters
     *            [SAP.Viewer.Parameter], array of parameter objects
     * @return
     */
    this.setParameters = function(params) {
        if (!bobj.isArray(params))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidArray.replace("{0}", "params"));
        if (isQueueEvents())
            pendingEvents.push(SAP.CR.Events.SetUserDefinedParamsEvent(params));
        else
            bobj.event.publish('setParameters', name, params);

    };

    this.drillDown = function(groupPath) {
        if (!bobj.isArray(groupPath))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidArray.replace("{0}", "groupPath"));
        if (!MochiKit.Iter.every(groupPath, function(value) { return /^[0-9]+$/.test(value); }))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidArray.replace("{0}", "groupPath"));
        
        var stringPath = groupPath.join("-");
        if (isQueueEvents())
            pendingEvents.push(SAP.CR.Events.DrillDownEvent(stringPath));
        else
            bobj.event.publish('drilldown', name, MochiKit.Base.queryString( {
                brch : stringPath
            }));
    };

    this.refresh = function() {
        if (isQueueEvents())
            pendingEvents.push(SAP.CR.Events.RefreshReportEvent());
        else
            bobj.event.publish('refresh', name);
    };

    this.getPendingEvents = function() {
        return pendingEvents;
    };

    this.getName = function() {
        return name;
    };

    this.getContainerID = function() {
        return container;
    };

    this.setPageNumber = function(pageNumber) {
        if (bobj.isNumber(pageNumber) && pageNumber > 0)
            pageNum = pageNumber;
        else
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidPageNumber);

        if (isQueueEvents())
            pendingEvents.push(SAP.CR.Events.GetPageEvent(pageNumber));
        else
            bobj.event.publish('setPageNumber', name, pageNumber);
    };

    function setComponentDisplay(component, isDisplay) {
        if (!bobj.isBoolean(isDisplay))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidBoolean.replace("{0}", "isDisplay"));

        if (isQueueEvents())
            pendingEvents.push(SAP.CR.Events.SetComponentVisibility(component, isDisplay));
        else
            bobj.event.publish('setComponentVisibility', name, component, isDisplay);
    }

    this.setDisplayToolbar = function(isDisplay) {
        setComponentDisplay(bobj.crv.Viewer.Components.Toolbar, isDisplay);
    };

    this.setDisplayLeftPanel = function(isDisplay) {
        setComponentDisplay(bobj.crv.Viewer.Components.LeftPanel, isDisplay);
    };

    this.setDisplayStatusbar = function(isDisplay) {
        setComponentDisplay(bobj.crv.Viewer.Components.Statusbar, isDisplay);
    };

    this.setDisplayBreadcrumb = function(isDisplay) {
        setComponentDisplay(bobj.crv.Viewer.Components.Breadcrumb, isDisplay);
    };
    
    this.setPrintMode = function(mode) {
        if (!isInitialized) {
            if (isValidPrintMode(mode))
                pendingEvents.push(SAP.CR.Events.SetPrintMode(mode));
            else
                throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidPrintMode.replace("{0}", mode).replace(
                        "{1}", "SAP.CR.Viewer.PrintMode"));
        }else
            throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.setPrintMode"));
    };

    function isValidPrintMode(mode) {
        if (!mode)
            return false;

        for (var key in SAP.CR.Viewer.PrintMode) {
            if (SAP.CR.Viewer.PrintMode[key] == mode)
                return true;
        }

        return false;
    }

    this.setPromptOnRefresh = function(promptOnRefresh) {
        if (!bobj.isBoolean(promptOnRefresh))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidBoolean.replace("{0}", promptOnRefresh));

        if (!isInitialized)
            pendingEvents.push(SAP.CR.Events.SetPromptOnRefresh(promptOnRefresh));
        else
            throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.setPromptOnRefresh"));
    };

    this.setReportMode = function(reportMode) {
        if (!isInitialized) {
            if (isValidReportMode(reportMode))
                pendingEvents.push(SAP.CR.Events.SetReportMode(reportMode));
            else
                throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidReportMode.replace("{0}", reportMode).replace(
                        "{1}", "SAP.CR.Viewer.ReportMode"));
        } else
            throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.setReportMode"));
    };

    function isValidReportMode(mode) {
        if (!mode)
            return false;

        for (var key in SAP.CR.Viewer.ReportMode) {
            if (SAP.CR.Viewer.ReportMode[key] == mode)
                return true;
        }

        return false;
    }

    this.setZoom = function(zoom) {
        if (zoom != null && (!bobj.isNumber(zoom) || zoom <= 0))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidZoom.replace("{0}", zoom));
        
        if (isQueueEvents())
            pendingEvents.push(SAP.CR.Events.SetZoom(zoom));
        else
            bobj.event.publish('zoom', name, zoom);
    };

    function addOnViewerInit(callback) {
        if (!isInitialized)
            initCallbackQueue.push(callback);
        else
            callback.apply();
    }

    this.addCanvasListener = function(listener) {
        if (listener == null || !(listener instanceof SAP.CR.Viewer.CanvasListener))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidCanvasListener);

        addOnViewerInit(function() {
            viewer.addCanvasListener(listener);
        });
    };
    
    this.removeCanvasListener = function(listener) {
        if (listener == null || !(listener instanceof SAP.CR.Viewer.CanvasListener))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidCanvasListener);

        addOnViewerInit(function() {
            viewer.removeCanvasListener(listener);
        });
    };

    this.addActionListener = function(listener) {
        if (listener == null || !(listener instanceof SAP.CR.Viewer.ActionListener))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidActionListener);

        addOnViewerInit(function() {
            viewer.addActionListener(listener);
        });
    };
    
    this.removeActionListener = function(listener) {
        if (listener == null || !(listener instanceof SAP.CR.Viewer.ActionListener))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidActionListener);

        addOnViewerInit(function() {
            viewer.removeActionListener(listener);
        });
    };
    
    this.batchExecute = function (fn) {
        if(!bobj.isFunction(fn))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidFunction);
        
        if(!isInitialized) {
            fn();
        }
        else {
            var exception = null;
            isForceQueue = true;
            try {
                fn();
                if(pendingEvents.length > 0) {
                    /**
                     * Only executed if no exception is thrown
                     */
                    bobj.event.publish('batchExecuteEvent', name, MochiKit.Base.serializeJSON(pendingEvents));
                }
            }
            catch (e) {
                exception = e;
            }
            
            isForceQueue = false;
            pendingEvents = [];
            if(exception != null)
                throw exception;
        }
    };
    
    this.setHasRefreshButton = function(isVisible) {
        if (!bobj.isBoolean(isVisible))
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidBoolean.replace("{0}", isVisible));

        if (!isInitialized)
            pendingEvents.push(SAP.CR.Events.SetHasRefreshButton(isVisible));
        else
            throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.setHasRefreshButton"));
    };
    
    this.setHasLogo = function(logo) {
        if(!bobj.isBoolean(logo))
            throw SAP.CR.Viewer.Exceptions.IllegalArugmentException.create(L_bobj_crv_API_InvalidBoolean.replace("{0}", logo));

        if(!isInitialized)
            pendingEvents.push(SAP.CR.Events.SetHasLogo(logo));
        else
            throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.setHasLogo"));
    };

    this.setLogo = function(uri,link,tooltip){
        if(!bobj.isString(uri))
            throw SAP.CR.Viewer.Exceptions.IllegalArugmentException.create(L_bobj_crv_API_InvalidParamType.replace("{0}", uri));
        
        if(!bobj.isString(link))
            link = "";
        
        if(!bobj.isString(tooltip))
            tooltip = "";
        
        if(!isInitialized){
            uri = encodeURI(uri);
            link = encodeURI(link);
            tooltip = encodeURIComponent(tooltip);
            pendingEvents.push(SAP.CR.Events.SetLogoURI(uri,link,tooltip));
        } else
            throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.setLogo"));
            
    };
    
    this.Internal = {
        BOELogonTypeEnum : { LogonToken : "LogonToken", SerializedSession : "SerializedSession" },
        ReportIdTypeEnum : {InfoObjectId : "InfoObjectId" , TransientId : "TransientId"},
        setReportSource : function(reportId, boeLogonType, boeLogonString, docLocale) {
        
            if(arguments[0] instanceof SAP.CR.InProcReportSource) //InProc report source
            {
            	if (isQueueEvents()) {
                        pendingEvents.push(SAP.CR.Events.SetInProcReportSourceEvent(arguments[0]));
                }
                else {
                        bobj.event.publish('setReportSource', name, arguments[0]);
                }
            	
            }
            else //managed report source
            {
                var reportIdType = null;
                reportId = reportId + ''; //convert reportid to string in case it's a number
                var regNumber = /^[0-9]+$/; 
                
            	if(regNumber.test(reportId)) {
            		reportIdType = this.ReportIdTypeEnum.InfoObjectId;
            	}
            	else {
            		reportIdType = this.ReportIdTypeEnum.TransientId;
            	}
            
            	if (isQueueEvents()) {
            		// when only 1 argument is passed in, we are passing in the report source factory session id
            		if (arguments.length == 1)
            			pendingEvents.push(SAP.CR.Events.SetEnterpriseReportSourceEvent(arguments[0]));
            		else
            			pendingEvents.push(SAP.CR.Events.SetEnterpriseReportSourceEvent(reportIdType, reportId, boeLogonType, boeLogonString, docLocale));
            	}
            	else {
            		// when only 1 argument is passed in, we are passing in the report source factory session id
            		if (arguments.length == 1)
            			bobj.event.publish('setReportSource', name, arguments[0]);
            		else
            			bobj.event.publish('setReportSource', name, reportIdType, reportId, boeLogonType, boeLogonString, docLocale);
            	}
            }
        },
        
        addActionsMenu : function(json) {
            addOnViewerInit(function() {
                viewer.addActionsMenu(json);
            });
        },
        
        setPdfOneClick : function(isOneClick) {
            if (!isInitialized)
                pendingEvents.push(SAP.CR.Events.SetPdfOneClick(isOneClick));
            else
                throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.Internal.setPdfOneClick"));
        },
    
        setResolution : function(dpi) {
            if (dpi != null && (!bobj.isNumber(dpi) || dpi <= 0))
                throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidResolution.replace("{0}", dpi));
    
            if (!isInitialized)
                pendingEvents.push(SAP.CR.Events.SetResolution(dpi));
            else
                throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.Internal.setResolution"));
        },
    
        setTimeZone : function(timeZone) {
            if (!isInitialized)
                pendingEvents.push(SAP.CR.Events.SetTimeZone(timeZone));
            else
                throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.Internal.setTimeZone"));
        },        
        
        setHyperlinkTarget : function(target) {
            if (!isInitialized)
                pendingEvents.push(SAP.CR.Events.SetHyperlinkTarget(target));
            else
                throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.Internal.setHyperlinkTarget"));
        },
        
        setSelectionFormula : function(target) {
            if (!isInitialized)
                pendingEvents.push(SAP.CR.Events.SetSelectionFormula(target));
            else
                throw SAP.CR.Viewer.Exceptions.IllegalOperationException.create(L_bobj_crv_API_InvalidFunctionAfterInit.replace("{0}", "SAP.CR.Viewer.Instance.Internal.setSelectionFormula"));
        }
    };
});

SAP.common.defineNS("SAP.CR.Viewer.PrintMode", {
    PDF : "PDF",
    ACTIVEX : "ActiveX"
});

SAP.common.defineNS("SAP.CR.Viewer.ReportMode", {
    PRINT : "PrintLayout",
    WEB : "WebLayout"
});
