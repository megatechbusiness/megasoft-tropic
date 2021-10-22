/**
 * LeftPanel is a container class managing PanelNavigator and ToolPanel classes
 */

bobj.crv.newLeftPanel = function(kwArgs) {
    kwArgs = MochiKit.Base.update ( {
        id : bobj.uniqueId () + "_leftPanel",
        hasToggleSearchButton : true,
        hasToggleGroupTreeButton : true,
        hasToggleParameterPanelButton : true,
        paramIconImg : null,
        treeIconImg : null,
        isVisible : true
    }, kwArgs);

    return new bobj.crv.LeftPanel (kwArgs.id, kwArgs.hasToggleSearchButton, kwArgs.hasToggleGroupTreeButton, kwArgs.hasToggleParameterPanelButton, kwArgs.isVisible, kwArgs.paramIconImg, kwArgs.treeIconImg);
};

bobj.crv.LeftPanel = function(id, hasToggleSearchButton, hasToggleGroupTreeButton, hasToggleParameterPanelButton, isVisible, paramIconImg, treeIconImg) {
    this._panelNavigator = null;
    this._panelHeader = null;
    this._toolPanel = null;
    this.id = id;
    this.isVisible = isVisible;
    this.widgetType = 'LeftPanel';
    this.hasToggleSearchButton = hasToggleSearchButton;
    this.hasToggleParameterPanelButton = hasToggleParameterPanelButton;
    this.hasToggleGroupTreeButton = hasToggleGroupTreeButton;
    this.paramIconImg = paramIconImg;
    this.treeIconImg = treeIconImg;
    this._lastViewedPanel = null;
    this.respectPageDirection = true;
};

bobj.crv.LeftPanel.prototype = {
    getHTML : function() {
        return bobj.html.DIV ( {
            'class' : 'leftPanel ' + (bobj.crv.config.isRTL? 'crvRTL' : 'crvLTR'),
            id : this.id
        }, this._getInnerHTML());
    },
    
    _getInnerHTML : function () {
        var toolPanelHTML = this._toolPanel ? this._toolPanel.getHTML () : '';
        var panelHeaderHTML = this._panelHeader ? this._panelHeader.getHTML () : '';
        var navigatorHTML = this._panelNavigator ? this._panelNavigator.getHTML () : '';
        
        return navigatorHTML +  panelHeaderHTML + toolPanelHTML;
    },
    
    resetSearch : function () {
        if(this._toolPanel) {
            this._toolPanel.resetSearch();
        }
    },
    
    focusFirstChild : function () {
        if(!this.isVisible)
            return false;
        
        var isFocused = false;
        if (this._panelNavigator) {
            isFocused = this._panelNavigator.focusFirstChild();
        }
        
        if (!isFocused && this._toolPanel) {
            isFocused = this._toolPanel.focusFirstChild();
        }
        
        return isFocused;
    },
    
    /**
     * Since insertBefore takes in DOM object and not html string, must create DOM element and append html of leftpanel's children.
     * @return
     */
    getDOM : function () {
        if(this.layer)
            return this.layer;

        var div = MochiKit.DOM.DIV({'class' : 'leftPanel', id : this.id});
        div.innerHTML = this._getInnerHTML();
        
        return div;
    },

    /**
     * @return width that would best fit both navigator and toolpanel
     */
    getBestFitWidth : function() {
        var w = 0;
        if (this._panelNavigator)
            w += this._panelNavigator.getWidth ();
        
        if (this._toolPanel && this._toolPanel.isDisplayed())
            w += this._toolPanel.getWidth();
        else if(this.isDisplayed())
            w += 5; //The padding between navigator and reportAlbum when no toolpanel exist
        
        return w;
    },
    
    getBestFitHeight : function () {
        var toolPanelHeight = 0;
        var panelNavigatorHeight = 0;
        
        if(this._panelHeader)
            toolPanelHeight += this._panelHeader.getHeight()
        if(this._toolPanel)
            toolPanelHeight += this._toolPanel.getBestFitHeight();
        if(this._panelNavigator)
            panelNavigatorHeight = this._panelNavigator.getBestFitHeight();
        
        return Math.max (toolPanelHeight, panelNavigatorHeight);
    },

    /**
     * AJAX update flow
     * @param update
     * @return
     */
    update : function(update) {
        if (!update || update.cons != "bobj.crv.newLeftPanel") {
            return;
        }

        for ( var childNum in update.children) {
            var child = update.children[childNum];
            if (child && child.cons == "bobj.crv.newToolPanel") {
                if (this._toolPanel)
                    this._toolPanel.update (child);
                break;
            }
        }
        
        if(update.args) {
            this.setVisible(update.args.isVisible);
        }
    },
    
    setVisible : function (isVisible) {
        this.isVisible = isVisible;
        this.setDisplay(this.isVisible);
    },

    /**
     * Initializes widgets and connects signals
     */
    init : function() {
        this.layer = getLayer (this.id);
        this.css = this.layer.style;
        
        if (this._toolPanel)
            this._toolPanel.init ();

        if (this._panelHeader) {
            this._panelHeader.init ();
            if(!this.isToolPanelDisplayed())
                this._panelHeader.setDisplay(false);
        }

        if (this._panelNavigator)
            this._panelNavigator.init ();
        
        this.setDisplay(this.isVisible);
            
    },

    /**
     * Connects signals
     * @return
     */
    _initSignals : function() {
        var partial = MochiKit.Base.partial;
        var signal = MochiKit.Signal.signal;
        var connect = MochiKit.Signal.connect;

        if (this._toolPanel) {
            MochiKit.Iter.forEach ( [ 'grpDrilldown', 'grpNodeRetrieveChildren', 'grpNodeCollapse', 'grpNodeExpand', 'resetParamPanel',
                    'resizeToolPanel', 'searchAll', 'selectSearchItem' ], function(sigName) {
                connect (this._toolPanel, sigName, partial (signal, this, sigName));
            }, this);
            
            connect (this._toolPanel, 'delayAddChild', this, '_onToolPanelDelayAddChild');
            
        }

        if (this._panelNavigator) 
            connect (this._panelNavigator, "switchPanel", this, '_switchPanel');
        
        if(this._panelHeader)
            connect (this._panelHeader, "switchPanel", this, '_switchPanel');
    },
    
    _onToolPanelDelayAddChild : function(childName) {
        var newChild = null;
        
        var ToolPanelType = bobj.crv.ToolPanelType;
        var ToolPanelTypeDetails = bobj.crv.ToolPanelTypeDetails;
        
        if(this._panelNavigator != null) {
            if (this.hasToggleParameterPanelButton && childName == ToolPanelType.ParameterPanel && !this._panelNavigator.hasPanelItem(ToolPanelType.ParameterPanel)) {
                newChild = {
                    name : ToolPanelType.ParameterPanel,
                    img : this.paramIconImg ? this.paramIconImg : ToolPanelTypeDetails.ParameterPanel.img,
                    title : ToolPanelTypeDetails.ParameterPanel.title
                };
            }
            else if (this.hasToggleGroupTreeButton && childName == ToolPanelType.GroupTree && !this._panelNavigator.hasPanelItem(ToolPanelType.GroupTree)) {
                newChild = {
                    name : ToolPanelType.GroupTree,
                    img : this.treeIconImg ? this.treeIconImg : ToolPanelTypeDetails.GroupTree.img,
                    title : ToolPanelTypeDetails.GroupTree.title
                };
            }
            else if (this.hasToggleSearchButton && childName == ToolPanelType.Search  && !this._panelNavigator.hasPanelItem(ToolPanelType.Search)) {
                newChild = {
                    name : ToolPanelType.Search,
                    img : ToolPanelTypeDetails.Search.img,
                    title : ToolPanelTypeDetails.Search.title
                };
            }
        
            if(newChild != null)
                this._panelNavigator.delayAddChild (newChild);
        }
    },

    /**
     * @return true if toolpanel is displayed
     */
    isToolPanelDisplayed : function() {
        return this._toolPanel && this._toolPanel.isDisplayed ();
    },
    
    /**
     * Do not Remove, Used by WebElements Public API
     */
    displayLastViewedPanel : function () {
        if(this._toolPanel) {
            switch(this._lastViewedPanel)
            {
                case bobj.crv.ToolPanelType.Search:
                    this._switchPanel (bobj.crv.ToolPanelType.Search);
                    break;    
                case bobj.crv.ToolPanelType.GroupTree:
                    this._switchPanel (bobj.crv.ToolPanelType.GroupTree);
                    break;
                case bobj.crv.ToolPanelType.ParameterPanel:
                    this._switchPanel (bobj.crv.ToolPanelType.ParameterPanel);
                    break;
                default :
                    this._switchPanel (bobj.crv.ToolPanelType.GroupTree);
            }
        }
    },
    
    /**
     * Do not Remove, Used by WebElements Public API
     */
    hideToolPanel : function () {
        this._switchPanel (bobj.crv.ToolPanelType.None);
    },
    
    /**
     * 
     * @param panelType [bobj.crv.ToolPanelType]
     * @return
     */
    _switchPanel : function(panelType) {
        if (this._toolPanel) {
            this._toolPanel.setView (panelType);
            if(panelType == bobj.crv.ToolPanelType.None) {
                this._toolPanel.setDisplay(false);
                this._panelHeader.setDisplay(false);
                this.focusFirstChild();
            }
            else {
                this._toolPanel.setDisplay(true);
                this._panelHeader.setDisplay(true);
                this._lastViewedPanel = panelType;
            }
        }

        if (this._panelHeader)
            var title = bobj.crv.ToolPanelTypeDetails[panelType].title;
            this._panelHeader.setTitle (title);

        if (this._panelNavigator)
            this._panelNavigator.selectChild (panelType);
        
        MochiKit.Signal.signal (this, 'switchPanel', panelType);
    },
    
    /**
     * Do not Remove, Used by WebElements Public API
     */
    getPanelNavigator : function () {
        return this._panelNavigator;
    },

    getToolPanel : function() {
        return this._toolPanel;
    },

    addChild : function(child) {
        if (child.widgetType == "ToolPanel") {
            this._toolPanel = child;
            this.updateChildren ();
            this._initSignals ();
        }
    },

    /**
     * Update Navigator and Header with toolPanel's children
     * @return
     */
    updateChildren : function() {
        if (this._toolPanel) {

            this._panelNavigator = new bobj.crv.PanelNavigator ();
            this._panelHeader = new bobj.crv.PanelHeader ();
            var newChild = null;

            if (this._toolPanel.hasSearch () && this.hasToggleSearchButton) {
                newChild = {
                    name : bobj.crv.ToolPanelType.Search,
                    img : bobj.crv.ToolPanelTypeDetails.Search.img,
                    title : bobj.crv.ToolPanelTypeDetails.Search.title
                };
                this._panelNavigator.addChild (newChild);
            }
            if (this._toolPanel.hasParameterPanel () && this.hasToggleParameterPanelButton) {
                newChild = {
                    name : bobj.crv.ToolPanelType.ParameterPanel,
                    img : this.paramIconImg ? this.paramIconImg : bobj.crv.ToolPanelTypeDetails.ParameterPanel.img,
                    title : bobj.crv.ToolPanelTypeDetails.ParameterPanel.title
                };
                this._panelNavigator.addChild (newChild);
            }
            if (this._toolPanel.hasGroupTree () && this.hasToggleGroupTreeButton) {
                newChild = {
                    name : bobj.crv.ToolPanelType.GroupTree,
                    img : this.treeIconImg ? this.treeIconImg : bobj.crv.ToolPanelTypeDetails.GroupTree.img,
                    title : bobj.crv.ToolPanelTypeDetails.GroupTree.title
                };
                this._panelNavigator.addChild (newChild);
            }
            
            this._lastViewedPanel = this._toolPanel.initialViewType;
            this._panelNavigator.selectChild (this._toolPanel.initialViewType);
            this._panelHeader.setTitle (bobj.crv.ToolPanelTypeDetails[this._toolPanel.initialViewType].title);
            
            if (!this._panelNavigator.hasChildren())
            {
            	this._panelHeader.hideCloseButton();
            	this._toolPanel.addLeftBorder();
            }
        }
    },
    
    resize : function(w, h) {
        if(this.isDisplayed()) {
            bobj.setOuterSize (this.layer, w, h);
            this._doLayout ();
        }
    },
    
    /**
    * Layouts children where PanelHeader appears on top of toolPanel and panelNavigator to left of both header and toolpanel
    * @return
    */
   _doLayout : function() {
       if(!this._toolPanel || !this._panelNavigator || !this._panelHeader || !this.isDisplayed())
           return;
       
       var w = this.getWidth ();
       var h = this.getHeight ();
       var navigatorW = this._panelNavigator.getWidth ();
       var newToolPanelWidth = w - navigatorW;
       var newToolPanelHeight = h - this._panelHeader.getHeight ();
       
       if (this._toolPanel.isDisplayed()) {
           this._toolPanel.resize (newToolPanelWidth, newToolPanelHeight);
           this._toolPanel.move (navigatorW, this._panelHeader.getHeight ());
       }
       
       this._panelHeader.resize (newToolPanelWidth, null);
       this._panelHeader.move (navigatorW, 0);
       this._panelNavigator.resize(navigatorW, h);
   },
   
   setDisplay : function (isDisplay) {
       Widget_setDisplay.call(this, isDisplay);
       MochiKit.Signal.signal(this, 'onSetDisplay', isDisplay);
   },

    move : Widget_move,
    getWidth : Widget_getWidth,
    getHeight : Widget_getHeight,
    isDisplayed : Widget_isDisplayed
};
