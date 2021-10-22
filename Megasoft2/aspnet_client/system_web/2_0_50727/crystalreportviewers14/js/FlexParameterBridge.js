/* Copyright (c) Business Objects 2006. All rights reserved. */

if (typeof (bobj) == 'undefined') {
    bobj = {};
}

if (typeof (bobj.crv) == 'undefined') {
    bobj.crv = {};
}

if (typeof (bobj.crv.params) == 'undefined') {
    bobj.crv.params = {};
}

/*
 ================================================================================
 FlexParameterBridge

 Base functionality for flex prompting UI
 ================================================================================
 */

bobj.crv.params.FlexParameterBridge = {
    _swfID : [],
    _swf : [],
    _cb : [],
    _isRTL : false,
    /*  You can think of this as a flag to tell us "I want to see the viewer.  It is used by parallel loading */
    _isInitializing : undefined,
    
    // TODO: Ryan - clean this up when the CAF actions are also cleaned up
    _promptData : [],
    
    setPromptData : function(id, d) {
        this._promptData[id] = d;
    },

    setMasterCallBack : function(viewerName, callBack) {
        this._cb[viewerName] = callBack;
    },

    getSWF : function(viewerName) {
        if (this._swf[viewerName]) {
            return this._swf[viewerName];
        } else {
            var swf = document.getElementById(this._swfID[viewerName]);
            this._swf[viewerName] = swf;
            return swf;
        }
    },
    
    clearSWF : function(viewerName) {
        if (this._swf[viewerName]) {
            this._swfID[viewerName] = null;
            this._swf[viewerName] = null;
        }
    },
    
    getInstallHTML : function() {
        return L_bobj_crv_FlashRequired.replace("{0}", "<br><a href='http://www.adobe.com/go/getflash/' target='_blank'>") + "</a>";
    },

    checkFlashPlayer : function() {
        return swfobject.hasFlashPlayerVersion("11.0.0");
    },

    /**
     * Creates the swf and replaces the div specified with the flash object.
     */
    createSWF : function(viewerName, divID, servletURL, showMinUI, locale, rptSrcKey, isRTL, isInitializing) {
        this._isRTL = isRTL;
        this._isInitializing = isInitializing;
        var cb = this._cb[viewerName];
        if (!cb) {
            return;
        }
        
        if (cb.logger) {
            cb.logger('Create the SWF');
        }
        
        if (this.checkFlashPlayer()) {
        
            var swfBaseURL = cb.getSWFBaseURL();
            var swfPath = swfBaseURL + "prompting.swf";
            var swfID = cb.getSWFID();
            var useSavedData = cb.getUseSavedData ? cb.getUseSavedData(viewerName) : false;
            var useOKCancelButtons = cb.getUseOKCancelButtons ? cb.getUseOKCancelButtons(viewerName) : false;
            var isDialog = cb.getIsDialog ? cb.getIsDialog (viewerName) : false;
            var allowFullScreen = cb.getAllowFullScreen ? cb.getAllowFullScreen (viewerName) : false;
            var enforceRequiredPrompt = cb.getEnforceRequiredPrompt ? cb.getEnforceRequiredPrompt () : true;
            var shouldAutoResize = cb.getShouldAutoResize ? cb.getShouldAutoResize(viewerName) : false;
            
            var flashvars = {
                "eventTarget" : viewerName,
                "locale" : locale,
                "showMinUI" : showMinUI,
                "baseURL" : swfBaseURL,
                "servletURL" : servletURL, // The SWF will use javascript to handle all async requests if this is null or empty
                "reportSourceKey" : rptSrcKey,
                "useSavedData" : useSavedData,
                "useOKCancelButtons" : useOKCancelButtons,
                "isDialog" : isDialog,
                "allowFullScreen" : allowFullScreen,
                "enforceRequiredPrompt" : enforceRequiredPrompt,
                "shouldAutoResize" : shouldAutoResize,
                "isRTL" : isRTL
            };
        
            // Important: Do not specify play=true as one of the params. If this
            // is set to true we could end up in an infinite loop reloading
            // the swf when viewing using the embedded browser in eclipse.
            var params = {
                menu : "false",
                wmode : "window",
                AllowScriptAccess : "always"
            };
            
            var attributes = {
                id : swfID,
                name : swfID,
                style : 'z-index:' + cb.getZIndex()
            };
        
            if (cb.processingDelayedShow) {
                cb.processingDelayedShow('hidden', divID);
            }
        
            var h = cb.getSWFHeight ? cb.getSWFHeight(viewerName) + "" : "600"; 
            var w = cb.getSWFWidth ? cb.getSWFWidth(viewerName) + "" : "800"; 
            
            swfobject.embedSWF(swfPath, divID, w, h, "11.0.0", "", flashvars, params, attributes);
            this._swfID[viewerName] = swfID;
        
            if (cb.processingDelayedShow) {
                cb.processingDelayedShow();
            }
            //We need to resize here for performance reasons.  Waiting until the init() to do a resize has some very bad performance problems.            
            this.resize(viewerName, 1, 1, false);
            //Need to move the top left as the progress dialog will block the rendering of the flash component making it slower.
            this.move(viewerName, 1, 1);


        } else {
            document.getElementById(divID).innerHTML = "<p>" + cb.getInstallHTML() + "</p>";
        }
    },
    
    initViewer : function(viewerName) {
        this._isInitializing = false;
        this.init(viewerName);
    },
    /**
     * This function will initialize the data in the flex swf with the
     * current state of the parameter ui. The Flex swf will call back to 
     * this method when it has first been created and all external interface
     * connections have been setup. If the swf has already been created this will 
     * be called when showing the parameter UI.
     */
    
    init : function(viewerName) {
        if (!viewerName) {
            return;
        }
        
        var cb = this._cb[viewerName];
        var swf = this.getSWF(viewerName);
        if (!swf || !cb) {
            return;
        }
        
        if (cb.logger) {
            cb.logger('Init the SWF');
        }
        
        if (swf.setIsInitializing) {
            swf.setIsInitializing(this._isInitializing);
        }

        
        if(swf.setShowMinUI && cb.getShowMinUI) {
            swf.setShowMinUI(cb.getShowMinUI(viewerName));
        }
        
        if(swf.setUseSavedData && cb.getUseSavedData) {
            swf.setUseSavedData(cb.getUseSavedData(viewerName));
        }
        
        if(swf.setUseOKCancelButtons && cb.getUseOKCancelButtons) {
            swf.setUseOKCancelButtons(cb.getUseOKCancelButtons(viewerName));
        }
        
        if(swf.setAllowFullScreen && cb.getAllowFullScreen) {
            swf.setAllowFullScreen(cb.getAllowFullScreen(viewerName));
        }

        if (swf.setReportStateInfo && cb.getReportStateInfo) {
            swf.setReportStateInfo(cb.getReportStateInfo(viewerName));
        }
        
        if (swf.setPromptData) {
            if (cb.getPromptData && cb.getPromptData(viewerName)) {
                swf.setPromptData(cb.getPromptData(viewerName));
            } else {
                swf.setPromptData(this._promptData[viewerName]);
            }
        }
        
        if (cb.getShouldAutoResize && cb.getShouldAutoResize(viewerName))
        {
            if (this._isInitializing == false)
            {
                //If we are not initializing, this should already have been taken care of in the creaetSWF command 
                this.resize(viewerName, 1, 1, false);
                this.move(viewerName, 1, 1);
            }

        }
        else if (cb.getSWFHeight && cb.getSWFWidth)
            this.resize(viewerName, cb.getSWFHeight(viewerName), cb.getSWFWidth(viewerName), true);
    },
    
    /**
     * Flex callback for closing the current dialog window.
     */ 
    closeDialog : function (viewerName){
        var cb = this._cb[viewerName];
        if (cb && cb.closeDialog) {
            cb.closeDialog(viewerName);
        }
    },

    /**
     * Flex callback for adjusting the size of the swf to fit the number of
     * prompts being displayed.
     */

    resize : function(viewerName, height, width, shouldCenter, fitToScreen) {
        var swf = this.getSWF(viewerName);
        var cb = this._cb[viewerName];
        if (swf && cb) {
            cb.logger('Resizing the SWF h:' + height + ' w:' + width);
            
            if (cb.getScreenHeight && cb.getScreenWidth)
            {
                var screenHeight = cb.getScreenHeight(viewerName);
                var screenWidth = cb.getScreenWidth(viewerName);
                var p = MochiKit.Style.getElementPosition(swf.parentNode);
                
                // Do not allow resizing beyond the screen size
                if (width > screenWidth) {
                    width = screenWidth;
                }
                if (!shouldCenter && !fitToScreen) { // resizing by dragging the corner?
                    if (this._isRTL) {
                        // Since element is positioned using left property width change affects the right edge of the element
                        // In RTL mode in order to visually show that the element is positioned on right we need to move the 
                        // the element on the left using the change in width value. 
                        var widthInc = width - swf.parentNode.offsetWidth;
                        var x = p.x - widthInc;
                        var vX = 0;
                        // If viewer is not own the page calculate the left offset of the viewer
                        if (getLayer) {
                            var l = getLayer(viewerName);
                            if (l) vX = l.offsetLeft;
                        }
                        // If resizing trying to exceed the left edge limit the resize and stay as it is
                        if (x < vX) {
                            width += (x - vX);
                            x = vX;
                        }
                        // Only update the left property is necessary
                        if (x != p.x) {
                            swf.parentNode.style.left = x + 'px';
                        }
                    }
                    else {
                        if ((p.x >= 0) && ((p.x + width) >= screenWidth)) {
                            width = screenWidth - p.x;
                        }
                    }
                }
                
                if ((p.y >= 0) && ((p.y + height) >= screenHeight) && !shouldCenter) {
                    height = screenHeight - p.y;
                }
                else if (height > screenHeight) {
                    height = screenHeight;
                }
                if (height < 0)
                {
                    height = 1;
                }
            }
            if (shouldCenter) {
                this.move(viewerName, ((screenWidth - width) / 2), ((screenHeight - height) / 2));
            }          

            if(swf.setWidth && swf.setHeight) {
                swf.setWidth(width);
                swf.setHeight(height);
            }
            
            swf.style.width = width + 'px';
            swf.style.height = height + 'px';
                      
            cb.setVisibility(viewerName);
            
            swf._isMaximized = false;
            
            if (cb.resize) {
                cb.resize();
            }
        }
    },

    fitScreen : function (viewerName){
        var swf = this.getSWF(viewerName);
        var cb = this._cb[viewerName];
        if (swf && cb && cb.getScreenHeight && cb.getScreenWidth && swf.setHeight && swf.setWidth) {
            cb.logger('Fitting SWF to the screen');
            var h = cb.getScreenHeight(viewerName);
            var w = cb.getScreenWidth(viewerName);
            
            // Resize the html object
            // We must call move before resize so that we can calculate the width/height appropriately when resizing
            this.move(viewerName, 0, 0);
            this.resize(viewerName, h, w, false, true);
            
            swf._isMaximized = true;
        }
    },
    
    startDrag : function(viewerName) {
        var cb = this._cb[viewerName];
        if (cb && cb.startDrag) {
            cb.startDrag(viewerName);
        }
    },

    stopDrag : function(viewerName) {
        var cb = this._cb[viewerName];
        if (cb && cb.stopDrag) {
            cb.stopDrag(viewerName);
        }
    },

    drag : function(viewerName, x, y) {
        var cb = this._cb[viewerName];
        if (cb && cb.drag) {
            cb.drag(viewerName, x, y);
        }
    },

    move : function(viewerName, x, y) {
        var cb = this._cb[viewerName];
        if (cb && cb.move) {
            cb.move(viewerName, x, y);
        }
    },
    
    setParamValues : function(viewerName, paramData) {
        var cb = this._cb[viewerName];
        if (cb && cb.setParamValues) {
            cb.setParamValues(viewerName, paramData);
        }
    },

    logon : function(viewerName, logonData) {
        var cb = this._cb[viewerName];
        if (cb && cb.logon) {
            cb.logon(viewerName, logonData);
        }
    },

    setReportStateInfo : function(viewerName, rsInfo) {
        var cb = this._cb[viewerName];
        if (cb && cb.setReportStateInfo) {
            cb.setReportStateInfo(viewerName, rsInfo);
        }
    },

    sendAsyncRequest : function(viewerName, args) {
        var cb = this._cb[viewerName];
        if (cb && cb.sendAsyncRequest) {
            cb.sendAsyncRequest(viewerName, args);
        }
    },
    
    handleAsyncResponse : function(viewerName, args) {
        var swf = this.getSWF(viewerName);
        if (swf && swf.handleAsyncResponse){
            swf.handleAsyncResponse(args);
        }
    },

    readyToShow: function(viewerName) {
        var cb = this._cb[viewerName];
        if (cb && cb.readyToShow) {
            cb.readyToShow(viewerName);
        }
        this._isInitializing = false;
    }
    
};
