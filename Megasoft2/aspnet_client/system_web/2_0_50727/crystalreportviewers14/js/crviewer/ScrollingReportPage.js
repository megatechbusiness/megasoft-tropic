bobj.crv.newScrollingReportPage = function(kwArgs) {
    var o = bobj.crv.newReportPage(kwArgs);
    
    o.widgetType = 'ScrollingReportPage';
    o.updateHtmlOld = o.updateHTML;
    o.updateSizeOld = o.updateSize;
    o.updateOld = o.update;
    o.margin = 10;
    MochiKit.Base.update(o, bobj.crv.ScrollingReportPage);
    
    o.highlightedCoord = null;

    return o;
};

bobj.crv.ScrollingReportPage = {
    addMouseWheelListener : function(callback) {
        if(this._iframe) {
            var iframeDoc = _ie ? this._iframe.contentWindow.document : this._iframe.contentDocument;
            bobj.connectMouseWheelListener (iframeDoc, callback);
        }
    },
    
    removeMouseWheelListener : function(callback) {
        if(this._iframe) {
            var iframeDoc = _ie ? this._iframe.contentWindow.document : this._iframe.contentDocument;
            bobj.disconnectMouseWheelListener (iframeDoc, callback);
        }
    },
    
    addKeyPressListener : function(callback) {
        if(this._iframe) {
            var e = _ie ? this._iframe.contentWindow.document : this._iframe.contentWindow;
            var eventName = (_ie || _ie11Up || _saf) ? 'onkeydown' : 'onkeypress';
            bobj.connectDOMEvent(e, eventName, callback);
        }
    },
    
    removeKeyPressListener : function(callback) {
        if(this._iframe) {
            var iframeDoc = _ie ? this._iframe.contentWindow.document : this._iframe.contentDocument;
            bobj.disconnectDOMEvent(iframeDoc, 'onkeypress', callback);
        }
    },
    
    getOffsetTop : function () {
        return this.layer.offsetTop;
    },

    
    updateHTML : function (content) {
        if(content) {
            if(!this._iframe) 
                bobj.removeAllChildElements (this._pageNode); /* clear loading span*/
            
            this.updateHtmlOld(content);
        }
        else {
            
            var imageTD = MochiKit.DOM.TD();
            imageTD.innerHTML = simpleImgOffset(bobj.skinUri("wait01.gif"), 20, 20, 90, 10, null, null, null, "float:none");
            
            var anchorTD = MochiKit.DOM.TD(null, MochiKit.DOM.A({
                href : "javascript:void(0);",
                'class' : 'loadingMessage'
            }, L_bobj_crv_LoadingPage.replace("{0}", this.getNumber())));

            var cells = [];
            cells.push(imageTD);
            cells.push(anchorTD);
            var table = MochiKit.DOM.TABLE({align: 'center', cellSpacing: '0', cellPadding: '4'}, MochiKit.DOM.TBODY(null, MochiKit.DOM.TR(null, cells)));
            
            var messageBar = MochiKit.DOM.DIV({
                'class' : 'loadingMessageBar',
                style : {
                    width: this.width - 12 + 'px'
                }
            }, table);
            
            this._pageNode.appendChild(messageBar);
            bobj.connectDOMEvent(this._pageNode.lastChild, 'onclick', this.screenReaderHandler);
        }
    },
    
    updateSize : function(sizeObject) {
        this.updateSizeOld(sizeObject);
        if(this.layer) {
            var isBBM = bobj.isBorderBoxModel ();
            this.layer.style.width = ((isBBM ? this.width + 2: this.width) + this.margin) + 'px';
            this.layer.style.height = ((isBBM ? this.height + 2: this.height) + this.margin) + 'px';
        }
    },
    
    update : function(update) {
        if (update) {
            this.updateOld(update);
            this.highlightedCoord = null;
        }
    },
    
    getHighlightedElementCoordinates : function () {
        if(this.highlightedCoord)
            return this.highlightedCoord;
        
        if(this._iframe) {
            var iframeDoc = _ie ? this._iframe.contentWindow.document  : this._iframe.contentDocument;
            var e = iframeDoc.getElementById("CrystalHighLighted");
            if(e) {
                this.highlightedCoord = MochiKit.Style.getElementPosition (e, null, iframeDoc);
            }
            
            return this.highlightedCoord;
        }
    },
    
    getHeight : function () {
        var height = this.height +  this.margin;
        if(bobj.isBorderBoxModel ())
            height += 2;
        return height;
    },
    
    getWidth : function () {
        var width =  this.width + this.margin;
        if(bobj.isBorderBoxModel ())
            width += 2;
        return width;
    },

    /**
     * @return Returns an object with width and height properties such that there 
     * would be no scroll bars around the page if they were applied to the widget. 
     */
    getBestFitSize : function() {
        var page = this.layer;
        return {
            width: page.offsetWidth, 
            height: page.offsetHeight 
        };
    },

    getHTML : function() {
        var h = bobj.html;
        var isBBM = bobj.isBorderBoxModel();

        var layerStyle = {
            position : 'relative',
            width : (this.width + 10) + 'px',
            height : (this.height + 10) + 'px',
            overflow : 'hidden'
        };

        var pageStyle = {
            position : 'absolute',
            width : (isBBM ? this.width + 2 : this.width) + 'px',
            height : (isBBM ? this.height + 2 : this.height) + 'px',
            'z-index' : 1,
            'border-width' : '1px',
            'border-style' : 'solid',
            'background-color' : this.bgColor,
            overflow : 'hidden',
            top : '0px',
            left : '0px'
        };

        var shadowStyle = {
            position : 'absolute',
            'z-index' : 0,
            width :  this.width + 'px',
            height : this.height + 'px',
            top : '10px',
            left : '10px'
        };

        var shadowHTML = '';
        if (this.documentView.toLowerCase() == bobj.crv.ReportPage.DocumentView.PRINT_LAYOUT) {
            layerStyle['background-color'] = 'transparent';
            pageStyle['border-color'] = '#000000';
            pageStyle['top'] = '4px';
            pageStyle['left'] = '4px';
            shadowStyle['background-color'] = '#737373';
            shadowHTML = h.DIV( {
                id : this.id + '_shadow',
                'class' : 'menuShadow',
                style : shadowStyle
            });
            /* page should appear in the center for print layouts */
            layerStyle['text-align'] = 'center'; /*
                                                     * For page centering in IE
                                                     * quirks mode
                                                     */
            layerStyle['margin'] = '0 auto'; /*
                                                 * center the page horizontally -
                                                 * CSS2
                                                 */
        } else {
            /* Web Layout*/
            pageStyle['border-color'] = '#FFFFFF';
            /* page should appear in left for web layouts */
            pageStyle['margin'] = '0';
        }

        var html = h.DIV( {
            id : this.id,
            style : layerStyle
        }, h.DIV( {
            id : this.id + '_page',
            style : pageStyle
        }), shadowHTML);

        return html;
    },

    addDashedBottomBorder : function() {
        if (this._pageNode)
            this._pageNode.style.borderBottom = '1px dashed #737373';
    },

    isInFocus : function() {
        return this._iframe == bobj.getActiveElement();
    }

};
