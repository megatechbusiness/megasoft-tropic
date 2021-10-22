bobj.crv.SearchPanelState = {
    Searching : "Searching",
    SearchCancelled : "SearchCancelled",
    Idle : "Idle",
    FoundFirstN : "FoundFirstN"
};

bobj.crv.newSearchPanel = function(kwArgs) {
    kwArgs = MochiKit.Base.update( {
        id : bobj.uniqueId() + '_searchPanel',
        searchText : ""
    }, kwArgs);

    return new bobj.crv.SearchPanel(kwArgs.id, kwArgs.searchText);
};

bobj.crv.SearchPanel = function(id, searchText) {
    this.id = id;
    this.searchText = searchText;
    this.status = bobj.crv.SearchPanelState.Idle;
    this.itemCount = 0;
    this.widgetType = "Search";
    this.isMatchCase = false;
    this.isMatchWholeWordOnly = false;
    this.FIRST_N_SIZE = 100;
    this.subreportName = null;

    var bind = bobj.bindFunctionToObject;

    var startSearchCB = bind(this.startSearch, this);
    var cancelSearchCB = bind(this.cancelSearch, this);
    var showOptionsCB = bind(this._showOptionsMenu, this);
    var hideMenuCB = bind(this._onHideMenu, this);
    var toggleMatchCaseCB = bind(this._toggleMatchCase, this);
    var toggleMatchWholeWordOnlyCB = bind(this._toggleMatchWholeWordOnly, this);

    this._resultPanel = new bobj.crv.SelectList(null, L_bobj_crv_SearchResults);

    this._optionsMenu = newMenuWidget(this.id + "_menu", hideMenuCB);
    this._optionsMenu.setAccelEnabled(false);

    this._optionsMenu.insertCheck(1, "case", L_bobj_crv_MatchCase, toggleMatchCaseCB);
    this._optionsMenu.insertCheck(2, "wholeWord", L_bobj_crv_MatchWholeWordsOnly , toggleMatchWholeWordOnlyCB);

    this._searchButton = newIconWidget(this.id + "_button", bobj.crv.allInOne.uri, startSearchCB, null, L_bobj_crv_SearchText, 16, 14, 0,
            bobj.crv.allInOne.toolbarSearchDy, bobj.crv.allInOne.toolbarSearchDy, 3, true);
    this._searchButton.margin = 0; 
    
    this._cancelSearchButton = newIconWidget(this.id + "_cancelButton", bobj.crv.allInOne.uri, cancelSearchCB, null, L_bobj_crv_Cancel, 16,
            14, 0, bobj.crv.allInOne.cancelDy, bobj.crv.allInOne.cancelDy, 0, true);
    this._cancelSearchButton.margin = 0; 
    
    this._optionsArrowButton = newIconWidget(this.id + "_optionsArrowButton", bobj.skinUri("menus.gif"), showOptionsCB, null,
            L_bobj_crv_SearchOptions, 7, 12, 0, 83, 0, 83, true, true);

    bobj.crv.setAllClasses(this._searchButton, 'button');
    bobj.crv.setAllClasses(this._cancelSearchButton, 'button');
    bobj.crv.setAllClasses(this._optionsArrowButton, 'button');

    this._cancelSearchButton.setDisplay(false);

    this.totalNumOfGroupPaths = 0;
    this.currentSearchText = '';
    this.continueSearchGroupPath = null;
    this.currentSearchSubrptReqCxt = '';
    this.currentSearchDrilldownGroupPath = '';
    this.widx=_widgets.length;
    this.findMoreItem = null;
    _widgets[this.widx]=this;
};

bobj.crv.SearchPanel.prototype = {
    getHTML : function() {
        var divH = bobj.isBorderBoxModel() ? 20 : 18;
        var h = bobj.html;
        return h.DIV( {
            id : this.id,
            'class' : 'searchPanel'
        }, h.TABLE( {
            style : {
                width : '100%',
                height : '100%'
            }
        }, h.TR( {
            style : {
                height : '25px'
            }
        }, h.TD(null, h.TABLE( {
            style : {
                width : '100%'
            }
        }, h.TR(null, h.TD(null, h.DIV( {
            id : this.id + "_searchInputContainer",
            'class' : 'searchPanelTextContainer',
            style : {height : divH + 'px'}
        }, h.TABLE( {
            cellpadding : '0',
            cellspacing : '0',
            style : {
                width : '100%',
                height : '100%',
                'table-layout' : 'fixed'
            }
    	}, h.TR(null, h.TD( {
            style : {
                width : '100%',
                'padding-left' : '5px',
                'padding-right' : '2px'
            }
    	}, h.INPUT( {
            onkeydown : bobj.getExecuteDOMCallbackHTML(this.widx, 'onTextFieldKeyDown'),
            'class' : 'searchPanel searchPanelInput',
            id : this.id + "_searchTextinput",
            title : L_bobj_crv_Search
        } )), h.TD( {
            style : {
                'width' : '20px',
                'vertical-align' : 'middle'
        }}, this._searchButton.getHTML(), this._cancelSearchButton.getHTML()), h.TD( {
            style : {
                width : '14px',
                'vertical-align' : 'top'
        }}, h.DIV( {
            'class' : 'arrowContainer',
            'style' : {
                'float' : 'left'
        }}, this._optionsArrowButton.getHTML())))))))))), h.TR(null, h.TD( {
            'style' : {
                'padding' : '0px'
        }}, h.DIV( {
            id : this.id + "_info",
            'class' : 'searchPanel info'
        }, h.TABLE( {
            'style' : {
                'width' : '100%',
                'height' : '24px'
        }}, h.TR(null, h.TD( {
            'style' : {
            'width' : '100%',
            'height' : '100%'
        }}, h.DIV( {
            id : this.id + "_numOfResultsFound",
            'tabIndex' : '0',
            'class' : 'searchPanel info resultsFoundText',
            'style' : bobj.crv.config.isRTL?{'right' : '5px', 'left':'0px'}:{},
            "aria-live" : 'polite',
            "aria-atomic" : "true"
        })), h.TD( {
            'style' : {
            'width' : '45px'
        }}, h.DIV( {
            'id' : this.id + "_loadingPercentText",
            'class' : 'searchPanel loadingInfo',
            'tabIndex' : '0',
            'title' : L_bobj_crv_Completed,
            'style' : {
                'display' : 'none',
                'text-align': 'right',
                'padding' : '0px',
                'margin-right' : bobj.crv.config.isRTL?'-5px' : '0px'
        }})), h.TD( {
            'style' : {
                'width' : '20px',
                'height' : '20px'
        }}, h.DIV( {
            'id' : this.id + "_loadingImage",
            'style' : {
                'margin-right' : '4px',
                'display' : 'none',
                'vertical-align' : 'top'
        }}, simpleImgOffset(bobj.skinUri("wait01.gif"), 20, 20, 90, 10, null, null, null, "float:right")))))
        ))), h.TR(null, h.TD(null, this._resultPanel.getHTML()))));
    },
    
    setSelected : function(isSelected) {
        if(isSelected)
            this.layer.setAttribute("role", "search");
        else
            this.layer.removeAttribute("role");
    },
    
    focusFirstChild : function () {
        this._searchTextInput.focus();
        return true;
    },
    
    onTextFieldKeyDown : function(e) {
        if(eventGetKey(e)==13) {
            this.startSearch();
            return false;
        }
        
        return true;
    },

    init : function() {
        this.layer = getLayer(this.id);
        this._searchButton.init();
        this._cancelSearchButton.init();
        this._optionsArrowButton.init();
        this._resultPanel.init();
        this._infoLayer = getLayer(this.id + "_info");

        this._numOfResultsFoundLayer = getLayer(this.id + "_numOfResultsFound");
        this._loadingPercentText = getLayer(this.id + "_loadingPercentText");
        this._loadingImage = getLayer(this.id + "_loadingImage");
        this._searchTextInput = getLayer(this.id + "_searchTextinput");
        this._searchInputContainer = getLayer(this.id + "_searchInputContainer");

        MochiKit.Signal.connect(this._resultPanel, "itemSelected", this, '_OnSelectSearchItem');
    },
    
    _OnSelectSearchItem : function (item) {
        if(item == this.findMoreItem) {
            this.continueSearch();
        }
        else {
            MochiKit.Signal.signal( this, 'selectSearchItem', item.groupPath);
        }
       
    },

    _toggleMatchCase : function() {
        this.isMatchCase = !this.isMatchCase
    },

    _toggleMatchWholeWordOnly : function() {
        this.isMatchWholeWordOnly = !this.isMatchWholeWordOnly
    },


    update : function(update) {
        if (update && update.cons == "bobj.crv.newSearchPanel" && update.args != null) {
            if (update.args.state) {
                var state = update.args.state;
                if (state.clearResults) {
                    this.reset();
                }
                else if (state.searchText == this.currentSearchText) {
                    if (state.subreportName != null) {
                        this.subreportName = state.subreportName;
                    }
                    if (state.searchSubreportRequestContext != null) {
                        this.currentSearchSubrptReqCxt = state.searchSubreportRequestContext;
                    }
                    if (state.searchDrilldownGroupPath != null) {
                        this.currentSearchDrilldownGroupPath = state.searchDrilldownGroupPath;
                    }

                    if (state.isFinished) {
                        this.updateSearchStatus(bobj.crv.SearchPanelState.Idle);
                        var foundItems = update.args.foundItems;
                        this.batchAddSearchItems(foundItems);
                        
                    } else if (this.status == bobj.crv.SearchPanelState.Searching) {
                        if (state.totalNumOfGroupPaths) {
                            this.totalNumOfGroupPaths = state.totalNumOfGroupPaths;
                        }
			
                        if (state.numberOfProcessedGroupPath == undefined ) // ipoint does not have this fix, keep compatible for ipoint
                            this.updateLoadingPercent(state.remainingGroupPaths.length);
			else
                            this.updateLoadingPercent(state.remainingGroupPaths.length + this.totalNumOfGroupPaths - state.numberOfProcessedGroupPath);

                        var foundItems = update.args.foundItems;
                        var isFetchNextBatch = true;
                        if (foundItems != null && foundItems.length > 0) {
                            for ( var i = 0, len = foundItems.length; i < len; i++) {
                                var item = foundItems[i];

                                if (state.isFindFirstN && this.itemCount == this.FIRST_N_SIZE) {
                                    if (this.continueSearchGroupPath == null) {
                                        this.continueSearchGroupPath = {
                                            remainingItems : [],
                                            state : state
                                        };
                                    }

                                    this.continueSearchGroupPath.remainingItems.push(item);
                                    isFetchNextBatch = false;
                                } else {
                                    this.addSearchItem(item.title, item.desc, item.drillGroupPath, item.isRTL);
                                }
                            }
                            
                            this.updateItemsCount();
                        }

                        if (isFetchNextBatch) {
                            MochiKit.Signal.signal(this, 'searchAll', state.searchText, false, state.isMatchCase,
                                    state.isMatchWholeWordOnly, state.isFindFirstN, state.lastPageNumber, state.remainingGroupPaths,
                                    state.numberOfProcessedGroupPath, this.currentSearchSubrptReqCxt, this.currentSearchDrilldownGroupPath);
                        } else {
                            this.updateSearchStatus(bobj.crv.SearchPanelState.FoundFirstN);
                        }
                    }
                }
            }
        }
    },

    updateLoadingPercent : function(numOfRemainingGroupPaths) {
        if (this.totalNumOfGroupPaths == 0) {
            this._loadingPercentText.style.display = "none";
        } else {
            var percent = parseInt(((this.totalNumOfGroupPaths - numOfRemainingGroupPaths) / this.totalNumOfGroupPaths) * 100);
            this._loadingPercentText.innerHTML = "[" + percent + "%]";
            this._loadingPercentText.style.display = "inline";
        }
    },

    resetLoadingPercent : function() {
        this._loadingPercentText.innerHTML = "";
        this._loadingPercentText.style.display = "none";
    },

    addSearchItem : function(title, desc, groupPath, isRTL) {
        this._resultPanel.addChild(title, desc, groupPath, isRTL);
        this.incrementItemCount();
    },

    resetItemCount : function() {
        this.itemCount = 0;
        this.updateItemsCount();
    },

    updateItemsCount : function() {
        var itemsFound = this.itemCount;
        if(this.status == bobj.crv.SearchPanelState.FoundFirstN) {
            itemsFound = itemsFound + "+";
        }
        if(this.subreportName == null) {
            this._numOfResultsFoundLayer.innerHTML = L_bobj_crv_ResultsFound.replace("{0}", bobj.html.B(null, itemsFound));
        }
        else {
            this._numOfResultsFoundLayer.innerHTML = L_bobj_crv_ResultsFoundInSubreport.replace("{0}", bobj.html.B(null, itemsFound)).replace(
                    "{1}", this.subreportName);
        }
    },

    incrementItemCount : function() {
        this.itemCount++;
    },

    getBestFitHeight : function() {
        return bobj.getHiddenElementDimensions(this.layer).h;
    },

    resize : function(w, h) {
        bobj.setOuterSize(this.layer, w, h);
        if (bobj.isNumber(h)) {
            this._resultPanel.resize(w - 6, h - 63);
        }
    },

    setDisplayResultsFound : function(isDisplay) {
        this._numOfResultsFoundLayer.style.display = isDisplay ? "block" : "none";
    },

    setDisplayLoadingInfo : function(isDisplay) {
        this._loadingPercentText.style.display = isDisplay ? "block" : "none";
        this._loadingImage.style.display = isDisplay ? "block" : "none";
    },

    setDisplayFindMoreItem : function(isDisplay) {
        if(isDisplay) {
            if(!this.findMoreItem) {
                var h = bobj.html;
                this.findMoreItem = this._resultPanel.addChild(h.B(null, h.A(null, L_bobj_crv_FindMore)), "&nbsp;", "FindMore");
            }
        }
        else {
            if(this.findMoreItem) {
                this._resultPanel.removeChild(this.findMoreItem);
            }
            
            this.findMoreItem = null;
        }
    },
    
    
    setDisableTextInput : function (isDisable) {
        this._searchTextInput.disabled = isDisable;
        this._optionsArrowButton.setDisabled(isDisable);
        this._searchInputContainer.className = isDisable ? 'searchPanelTextContainer searchPanelTextContainerDisabled' : 'searchPanelTextContainer';
        if(_firefox && !isDisable && document.activeElement == this._searchTextInput) {
            /* work around a bug in firefox where blinking cursor on textinput disappears*/
            this._searchTextInput.blur();
            this._searchTextInput.focus();
        }
    },

    startSearch : function() {
        if(this._searchTextInput.value.length > 0) {
            this.reset();
            this.currentSearchText = this._searchTextInput.value;
            this.updateSearchStatus(bobj.crv.SearchPanelState.Searching);
            MochiKit.Signal.signal(this, 'searchAll', this.currentSearchText, true, this.isMatchCase, this.isMatchWholeWordOnly,
                    true, null, null, null, null, null);
        }
    },

    reset : function () {
        this.continueSearchGroupPath = null;
        this._resultPanel.reset();
        this.resetItemCount();
        this.resetLoadingPercent();
        this.totalNumOfGroupPaths = 0;
        this.updateSearchStatus(bobj.crv.SearchPanelState.Idle);
        this.setDisplayResultsFound(false);
        this.subreportName = null;
	this.currentSearchSubrptReqCxt = '';
	this.currentSearchDrilldownGroupPath = '';
    },
    
    batchAddSearchItems : function (items) {
        if(items != null && items.length > 0) {
            MochiKit.Iter.forEach(items, function(item) {
                this.addSearchItem(item.title, item.desc, item.drillGroupPath, item.isRTL);
            }, this);
            
            this.updateItemsCount();
        }
    },
    
    continueSearch : function() {
        if (this.continueSearchGroupPath != null) {
            this.updateSearchStatus(bobj.crv.SearchPanelState.Searching);
            var remainingItems = this.continueSearchGroupPath.remainingItems;
            this.batchAddSearchItems(remainingItems);

            var state = this.continueSearchGroupPath.state;
            MochiKit.Signal.signal(this, 'searchAll', state.searchText, false, state.isMatchCase, state.isMatchWholeWordOnly, false,
                    state.lastPageNumber, state.remainingGroupPaths, state.numberOfProcessedGroupPath, this.currentSearchSubrptReqCxt,
                    this.currentSearchDrilldownGroupPath);
            
        }
        this.continueSearchGroupPath = null;
    },

    cancelSearch : function() {
        this.updateSearchStatus(bobj.crv.SearchPanelState.SearchCancelled);
    },


    updateSearchStatus : function(status) {
        this.status = status;
        switch (this.status) {
        case bobj.crv.SearchPanelState.SearchCancelled:
            this.setDisplayLoadingInfo(false);
            this.setDisplayResultsFound(true);
            this.setDisplayFindMoreItem(false);
            this._searchButton.setDisplay(true);
            this._cancelSearchButton.setDisplay(false);
            this.setDisableTextInput(false);
            this._searchTextInput.select();
            
            break;
        case bobj.crv.SearchPanelState.Searching:
            this.setDisplayResultsFound(true);
            this.setDisplayLoadingInfo(true);
            this.setDisplayFindMoreItem(false);
            this._searchButton.setDisplay(false);
            this._cancelSearchButton.setDisplay(true);
            this.setDisableTextInput(true);
            break;
        case bobj.crv.SearchPanelState.Idle:
            this.setDisplayResultsFound(true);
            this.setDisplayLoadingInfo(false);
            this.setDisplayFindMoreItem(false);
            this._searchButton.setDisplay(true);
            this._cancelSearchButton.setDisplay(false);
            this.setDisableTextInput(false);
            break;
        case bobj.crv.SearchPanelState.FoundFirstN:
            this.setDisplayResultsFound(true);
            this.setDisplayLoadingInfo(false);
            this._cancelSearchButton.setDisplay(false);
            this._searchButton.setDisplay(true);
            this.setDisplayFindMoreItem(true);
            this.setDisableTextInput(false);
            this.updateItemsCount();
        }
    },

    _onHideMenu : function() {
        this._optionsArrowButton.focus();
    },

    _showOptionsMenu : function() {
        var layer = this._optionsArrowButton.layer;
        var position = getPosScrolled(layer);
        var dimension = MochiKit.Style.getElementDimensions(layer)
        
        
        if (this._optionsMenu.layer==null)
            this._optionsMenu.justInTimeInit();
        
        var menuDimension = MochiKit.Style.getElementDimensions(this._optionsMenu.layer);
        var xPos = position.x - menuDimension.w + dimension.w;
        var yPos = position.y + dimension.h + 3;
        if (MochiKit.Base.isIE()) {
            if (bobj.isQuirksMode()) {
                yPos += 2;
            }
        }

        xPos = Math.max(xPos, 0);
        yPos = Math.max(yPos, 0);
        
        //RTL position for options menu with no parent
        if(_rtl)
        	xPos += 2*menuDimension.w - layer.offsetWidth;
        
        this._optionsMenu.show(true, xPos, yPos);
    },

    getWidth : Widget_getWidth,
    setDisplay : Widget_setDisplay
};