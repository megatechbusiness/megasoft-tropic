bobj.crv.VerticalScrollBar = function() {
    this.id = bobj.uniqueId();
    this.widx = _widgets.length;
    _widgets[this.widx] = this;

    this._track = new bobj.crv.ScrollTrack(true);
    this._toolTip = new bobj.crv.ScrollBarToolTip();

    this._moveScrollUpCB = null;
    this._moveScrollDownCB = null;
    this._thumbMoveCB = null;
    this._scrollBtnCBIntervalID = null;
    this._tooltipGeneratorFn = null;
    this._scrollableElement = null;
    this._scrollbarHeight = 0; // cache offsetHeight
    this._topOffset = 0;

    this.SCROLLBAR_SIZE = 16;
    this.SCROLL_DELTA = 8;    
};

bobj.crv.VerticalScrollBar.prototype = {
    init : function() {
        var bind = bobj.bindFunctionToObject;
        
        this._track.init();
        this._track.setThumbMoveListener(bind(this._onThumbMoveListener, this));
        this._toolTip.init();
        this.layer = getLayer(this.id);

        this._mouseWheelCB = bind(this._onScrollListener, this);
    },

    setCallBacks : function(upCB, downCB, thumbMoveCB) {
        this._moveScrollDownCB = downCB;
        this._moveScrollUpCB = upCB;
        this._thumbMoveCB = thumbMoveCB;
    },

    getHTML : function() {
        var h = bobj.html;
        return h.DIV( {
            id : this.id,
            'class' : 'crvScrollBar crvVertical ' + (bobj.crv.config.isRTL ? 'crvRTL' : 'crvLTR'),
            oncontextmenu : 'return false'
        }, h.TABLE( {
            cellpadding : 0,
            cellspacing : 0
        }, h.TR( {
            'class' : 'crvScrollButton crvup'
        }, h.TD( {
            onmousedown : bobj.getExecuteDOMCallbackHTML(this.widx, 'onMouseDown_ScrollUpBtn'),
            onmouseup : bobj.getExecuteDOMCallbackHTML(this.widx, 'cancelCallBack'),
            onmouseout : bobj.getExecuteDOMCallbackHTML(this.widx, 'cancelCallBack')
        }, h.A( {
            'class' : bobj.crv.allInOne.css,
            href : 'javascript:void(0);',
            tabIndex : '-1'
        }))), h.TR(null, h.TD(null, this._track.getHTML())), h.TR( {
            'class' : 'crvScrollButton crvdown'
        }, h.TD( {
            onmousedown : bobj.getExecuteDOMCallbackHTML(this.widx, 'onMouseDown_ScrollDownBtn'),
            onmouseup : bobj.getExecuteDOMCallbackHTML(this.widx, 'cancelCallBack'),
            onmouseout : bobj.getExecuteDOMCallbackHTML(this.widx, 'cancelCallBack')
        }, h.A( {
            'class' : bobj.crv.allInOne.css,
            href : 'javascript:void(0);',
            tabIndex : '-1'
        })))), this._toolTip.getHTML());
    },
    
    isDisplayed : function () {
        if(this.layer.style.display == "none")
            return false;
        else    
            return true;
    },
    
    /**
     * @param, height in pixels
     */
    resize : function (height) {
    	this._track.resize(Math.max(0, height - 34)); //17 x 17 for buttons
    },

    onMouseDown_ScrollUpBtn : function(ev) {
        if (this._scrollableElement) {
            this.scrollUp ();
            var scrollCB = function(scroll, fn) {
                return function () {
                    scroll.setIntervalID(window.setInterval(fn, 50));
                };
            };
            
            this._scrollBtnCBIntervalID = window.setTimeout(scrollCB(this, bobj.bindFunctionToObject(this.scrollUp, this)), 200);
        }
        else if (this._moveScrollUpCB) {
            this._moveScrollUpCB();
            this._scrollBtnCBIntervalID = window.setInterval(this._moveScrollUpCB, 100);
        }
        
        eventCancelBubble(ev);
        if (ev.preventDefault)
            ev.preventDefault();
        
        return false;
    },

    onMouseDown_ScrollDownBtn : function(ev) {
        if (this._scrollableElement) {
        	this.scrollDown ();
            var scrollCB = function(scroll, fn) {
                return function () {
                    scroll.setIntervalID(window.setInterval(fn, 50));
                };
            };
            
            this._scrollBtnCBIntervalID = window.setTimeout(scrollCB(this, bobj.bindFunctionToObject(this.scrollDown, this)), 200);
        }
        else if (this._moveScrollDownCB) {
            this._moveScrollDownCB();
            this._scrollBtnCBIntervalID = window.setInterval(this._moveScrollDownCB, 100);
        }
        
        eventCancelBubble(ev);
        if (ev.preventDefault)
            ev.preventDefault();
        
        return false;
    },
    
    scrollUp : function () {
        if (this._scrollableElement) {
        	this._moveByDelta(-this.SCROLL_DELTA, true);
        }
    },
    
    scrollDown : function () {
        if (this._scrollableElement) {
        	this._moveByDelta(this.SCROLL_DELTA, true);
        }
    },
    
    pageUp : function () {
        if (this._scrollableElement) {
            var delta = Math.max(0, this._scrollbarHeight - 16);
            this._moveByDelta (-delta, true);
        }
    },
    
    pageDown : function () {
        if (this._scrollableElement) {
            var delta = Math.max(0, this._scrollbarHeight - 16);
            this._moveByDelta (delta, true);
        }
    },

    cancelCallBack : function() {
        if (this._scrollBtnCBIntervalID)
            clearInterval(this._scrollBtnCBIntervalID);

        this._scrollBtnCBIntervalID = null;
    },

    /**
     * 
     * @param fn a function that converts percent to a tooltip
     * @return
     */
    setTooltipGenerator : function(fn) {
        this._tooltipGeneratorFn = fn;
    },

    adjustForResize : function(height) {
        if (height <= 0)
            return;
        
        // We need to do this twice because for some reason IE doesn't
        // calculate it right the first time
        var scrHeight = this._scrollableElement.scrollHeight;
        scrHeight = this._scrollableElement.scrollHeight;
        
        var displayVsbar = scrHeight > height;

        this.layer.style.display = displayVsbar ? 'block' : 'none';
        this.layer.style.height = height + 'px';

        if (displayVsbar) {
            if (_ie && this.offsetLeft < 0) { // fix _ie rendering issue
                this.layer.style.visibility = 'hidden';
                this.layer.style.visibility = 'visible';
            }
        }
        
        this.resize (height);
    	
        if (this._scrollableElement) {
            this._scrollbarHeight = height;
            if (scrHeight > 0) {
                var lInPercent = (this._scrollbarHeight / scrHeight) * 100.0;
                this.setThumbLength (lInPercent);
                
                this._moveByDelta(0);
            }
        }
    },

    _onThumbMoveListener : function(newY, moveByCode) {
        if (this._thumbMoveCB)
            this._thumbMoveCB(newY);
        
        if (this._tooltipGeneratorFn) {
            this._toolTip.setToolTip(this._tooltipGeneratorFn(newY));
            var thumbClientPos = this.getThumbPosition();
            thumbClientPos += this.getThumbLength() / 2;
            this._toolTip.setPosition(thumbClientPos);
        }
        
        if (this._scrollableElement && !isNaN(newY)) {
            var elmtH = this._scrollableElement.scrollHeight;
            if(elmtH >= this._scrollbarHeight) {
                var newTop = Math.floor((newY * (elmtH - this._scrollbarHeight)) / 100);
                var delta = newTop - this._topOffset;
                this._moveByDelta (delta, true);
            }
        }
    },
    
    /**
     * This method moves the scroll top of the scrollable element by delta pixels
     * 
     * @param delta, pixels to move the scrollable element
     * @param moveByThumb, true if this method was called due to the movement of the thumb;
     *                     false otherwise
     */
    _moveByDelta : function (delta, moveByThumb) {
        var isMoved = false;
        var orginalOffsetTop = this._scrollableElement.scrollTop;
        if (this._scrollableElement) {
            var elmtH = this._scrollableElement.scrollHeight;
            var diff = this._scrollbarHeight - elmtH;
            
            this._topOffset += delta;
            this._topOffset = Math.max(this._topOffset, 0);
            this._topOffset = Math.min(this._topOffset, elmtH - this._scrollbarHeight);
            
            // Update the thumb when the scrollable element has been scrolled
            if (!moveByThumb && diff != 0) {
                var pInPercent = -this._topOffset / diff * 100;
                this.setThumbPosition(pInPercent);
            }
            // Update the scrollable element when the thumb has been moved
            else if (this._scrollableElement.scrollTop != this._topOffset) {
                this._scrollableElement.scrollTop = this._topOffset;
            }
            
            isMoved = orginalOffsetTop != this._scrollableElement.scrollTop;
        }
        return isMoved;
    },
    
    _onScrollListener : function (e) {
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

        if (delta) {/* nonzero? handle it */
            this._onScroll(delta, e);
        }
    },

    _onScroll : function(value, event) {
        if (value == 0)
            return;

        var moveBy = value * this.SCROLL_DELTA;
        var isMoved = this._moveByDelta(-moveBy, true);
        if(isMoved)
            (new MochiKit.Signal.Event(this, event)).stop(); //cancel window scrolling when bestfitpage is true
    },
    
    _onElementScroll : function () {
        var delta = this._scrollableElement.scrollTop - this._topOffset;
        this._moveByDelta (delta);
    },
    
    getThumbLength : function() {
    	return this._track.getThumb().getLength();
    },

    getThumbPosition : function() {
        return this._track.getThumb().getPosition();
    },

    setThumbLength : function(lInPercent) {
        this._track.getThumb().setLength(lInPercent);
    },

    setThumbPosition : function(pInPercent) {
        this._track.getThumb().setPosition(pInPercent);
    },
        
    setIntervalID : function (id) {
        this._scrollBtnCBIntervalID = id;
    },
    
    setScrollableElement : function(element) {
        this._scrollableElement = element;
        
        if (this._scrollableElement) {
            var bind = bobj.bindFunctionToObject;
            
            bobj.connectMouseWheelListener (this.layer, this._mouseWheelCB);
            bobj.connectMouseWheelListener (this._scrollableElement, this._mouseWheelCB);
            
            MochiKit.Signal.connect(this._scrollableElement, "onscroll", bind(this._onElementScroll, this));

            this._track.setMoveDownCB(bind(this.pageDown, this));
            this._track.setMoveUpCB(bind(this.pageUp, this));
        }
    }
};

bobj.crv.HorizontalScrollBar = function() {
    this.id = bobj.uniqueId();
    this.widx = _widgets.length;
    _widgets[this.widx] = this;

    this._track = new bobj.crv.ScrollTrack(false);
    this._scrollBtnCBIntervalID = null;
    this._scrollableElement = null;
    this._scrollbarWidth = 0; // cache offsetWidth
    this._leftOffset = 0;
};

bobj.crv.HorizontalScrollBar.prototype = {
    getHTML : function() {
        var h = bobj.html;
        
        return h.DIV( {
            id : this.id,
            'class' : 'crvScrollBar crvHorizontal',
            oncontextmenu : 'return false'
        }, h.TABLE( {
            cellpadding : 0,
            cellspacing : 0,
            'dir' : 'ltr',
            style : {'table-layout' : 'fixed'}
        }, h.TR(null, h.TD( {
            'class' : 'crvScrollButton crvLeft',
            onmousedown : bobj.getExecuteDOMCallbackHTML(this.widx, 'onMouseDown_ScrollLeftBtn'),
            onmouseup : bobj.getExecuteDOMCallbackHTML(this.widx, 'cancelCallBack'),
            onmouseout : bobj.getExecuteDOMCallbackHTML(this.widx, 'cancelCallBack')
        }, h.A( {
            'class' : bobj.crv.allInOne.css,
            href : 'javascript:void(0);',
            tabIndex : '-1'
        })), h.TD({style :{width : '100%', 'vertical-align': 'top'}}, this._track.getHTML()), h.TD( {
            'class' : 'crvScrollButton crvRight',
            onmousedown : bobj.getExecuteDOMCallbackHTML(this.widx, 'onMouseDown_ScrollRightBtn'),
            onmouseup : bobj.getExecuteDOMCallbackHTML(this.widx, 'cancelCallBack'),
            onmouseout : bobj.getExecuteDOMCallbackHTML(this.widx, 'cancelCallBack')
        }, h.A( {
            'class' : bobj.crv.allInOne.css,
            href : 'javascript:void(0);',
            tabIndex : '-1'
        })))));
    },
    
    init : function() {
        this.layer = getLayer(this.id);
        this._track.init();
        this._track.setThumbMoveListener(bobj.bindFunctionToObject(this._onThumbMoveListener, this));
    },

    onMouseDown_ScrollRightBtn : function(ev) {
        this.scrollRight();
        var scrollCB = function(scroll, fn) {
            return function () {
                scroll.setIntervalID(window.setInterval(fn, 50));
            };
        };
        
        this._scrollBtnCBIntervalID = window.setTimeout(scrollCB(this, bobj.bindFunctionToObject(this.scrollRight, this)), 200);
        
        eventCancelBubble(ev);
        if (ev.preventDefault)
            ev.preventDefault();
        
        return false;
    },

    onMouseDown_ScrollLeftBtn : function(ev ) {
        this.scrollLeft();

        var scrollCB = function(scroll, fn) {
            return function () {
                scroll.setIntervalID(window.setInterval(fn, 50));
            };
        };
        
        this._scrollBtnCBIntervalID = window.setTimeout(scrollCB(this, bobj.bindFunctionToObject(this.scrollLeft, this)), 200);
        
        eventCancelBubble(ev);
        if (ev.preventDefault)
            ev.preventDefault();
        
        return false;
    },
    
    scrollLeft : function () {
        if (this._scrollableElement) {
            this._track.getThumb().moveXByDelta(-0.01);
        }
    },
    
    scrollRight : function () {
        if (this._scrollableElement) {
            this._track.getThumb().moveXByDelta(0.01);
        }
    },
        
    setIntervalID : function (id) {
        this._scrollBtnCBIntervalID = id;
    },
    
    cancelCallBack : function() {
        if (this._scrollBtnCBIntervalID) 
            clearInterval(this._scrollBtnCBIntervalID);
        
        this._scrollBtnCBIntervalID = null;
    },
    
    setScrollableElement : function(element) {
        this._scrollableElement = element;
    },
    
    adjustForResize : function() {
    	if(this._scrollbarWidth == 0 && bobj.crv.config.isRTL)
    		this._track.getThumb().moveXByDelta(1);
        this._scrollbarWidth = (this.layer) ? this.layer.offsetWidth : 0;
        if (this._scrollbarWidth == 0)
            return;
        var elemOffsetWidth = this._scrollableElement.offsetWidth;
        var lInPercent = (this._scrollbarWidth / elemOffsetWidth) * 100.0;
        this._track.getThumb().setLength(lInPercent);
        this._track.getThumb().moveX((this._leftOffset / elemOffsetWidth) * this._scrollbarWidth, true);
    },
    
    /*
     * Set position in pixel
     */
    setThumbPosition : function(pInPercent) {
        this._track.getThumb().setPosition(pInPercent);
    },
    
    _onThumbMoveListener : function(newX, moveByCode) {
        if (this._scrollableElement) {
            var elmtW = this._scrollableElement.offsetWidth;
            var sbarW = this._scrollbarWidth;
            if(elmtW > sbarW) {
                var newLeft = Math.floor((newX * (elmtW - sbarW)) / 100);
                // second condition is when we resize panel to a smaller size. in this case we should calculate newLeft by above formula
                if(moveByCode && this._leftOffset < newLeft)
                    newLeft = this._leftOffset;
                this._scrollableElement.style.left = -newLeft + 'px';
                this._leftOffset = newLeft;
            }
        }

    }
};

bobj.crv.ScrollTrack = function(isVertical) {
    this.id = bobj.uniqueId();
    this._isVertical = isVertical;
    this._thumb = new bobj.crv.ScrollThumb(isVertical);

    var bind = bobj.bindFunctionToObject;
    this._mouseDownCB = bind(this._onMouseDown, this);
    this._mouseUpCB = bind(this._onMouseUp, this);
    this._moveDownCB = null;
    this._moveUpCB = null;
    this._moveIntervalID = null;
};

bobj.crv.ScrollTrack.prototype = {
    getHTML : function() {
        var h = bobj.html;
        return h.DIV( {
            id : this.id,
            'class' : 'crvScrollTrack',
            style : {height : !this._isVertical ? '17px' : '100%'}
        }, this._thumb.getHTML());
    },

    getOffsetTop : function() {
        return this.layer.offsetTop;
    },

    getLength : function() {
        if (this._isVertical)
            return this.layer.offsetHeight;
        else
            return this.layer.offsetWidth;
    },

    init : function() {
        this.layer = getLayer(this.id);

        this._thumb.init();
        bobj.connectDOMEvent(this.layer, 'onmousedown', this._mouseDownCB);
        bobj.connectDOMEvent(this.layer, 'onmouseup', this._mouseUpCB);
    },
    
    /**
     * Resizes vertically if vertical track and horizontally if horizontal track
     * @param newSize pixel
     * @return
     */
    resize : function (newSize) {
        var style = this.layer.style;
        newSize += "px";

        if(this._isVertical)
            style.height = newSize;
        else
            style.width = newSize;
    },

    setThumbMoveListener : function(listener) {
        this._thumb.setMoveListener(listener);
    },
    
    setMoveDownCB : function (moveDownCB) {
        this._moveDownCB = moveDownCB;
    },
    
    setMoveUpCB : function (moveUpCB) {
        this._moveUpCB = moveUpCB;
    },

    getThumb : function() {
        return this._thumb;
    },

    _onMouseDown : function(ev) {
        if(ev.which == 1 || !!(ev.button & 1)) {        
            this._clearMoveTimer();
            
            var clickOffset = null;
            if(this._isVertical)
                clickOffset = ev.layerY || ev.y;
            else
                clickOffset = ev.layerX || ev.x;
    
            this._moveThumb(clickOffset, 500);
        }
        else {
            return false;
        }
    },

    _moveThumb : function(clickOffset, timeout) {
        var thumbPos = this._thumb.getPosition();
        var thumbLen = this._thumb.getLength();
        
        if (clickOffset > thumbPos + thumbLen) {
            if(this._isVertical) {
                if (this._moveDownCB != null)
                    this._moveDownCB();
                else
                    this._thumb.moveDown();
            }
            else {
                this._thumb.moveRight();
            }
        } else if (clickOffset < thumbPos) {
            if(this._isVertical) {
                if (this._moveUpCB != null)
                    this._moveUpCB();
                else
                    this._thumb.moveUp();
            }
            else {
                this._thumb.moveLeft();
            }
        }
        else {
            return;
        }

        this._moveTimerID = window.setTimeout(MochiKit.Base.bind(this._moveThumb, this, clickOffset, 50), timeout);
    },

    _onMouseUp : function(ev) {
        this._clearMoveTimer();
    },

    _clearMoveTimer : function() {
        if (this._moveTimerID != null) {
            clearTimeout(this._moveTimerID);
            this._moveTimerID = null;
        }
    }
};

bobj.crv.ScrollThumb = function(isVertical) {
    this._clickOffset = 0;
    this._isVertical = isVertical;
    this.id = bobj.uniqueId() + "_thumb";
    this._onMoveListnerCB = null;

    var bind = bobj.bindFunctionToObject;

    this._mouseDownCB = bind(this._onMouseDown, this);
    this._mouseMoveCB = bind(this._onMouseMove, this);
    this._mouseUpCB = bind(this._onMouseUp, this);
    
    this._pageOverlay = null; /*overlay div that prevents mouse from entering iframe of the page*/
    this._length = null;
    this._position = null;
};

bobj.crv.ScrollThumb.prototype = {
    getHTML : function() {
        var h = bobj.html;

        if (this._isVertical) {
            return h.A( {
                'class' : 'crvScrollThumb crvThumbVertical',
                id : this.id,
                tabindex : -1,
                href : "javascript:void(0)"
            }, h.TABLE( {
            	style : {'table-layout' : 'fixed'},
                cellspacing : 0,
                cellpadding : 0
            }, h.TBODY(null, h.TR( {
                'class' : "crvThumbL1"
            }, h.TD()), h.TR( {
                'class' : "crvThumbL2"
            }, h.TD(null, h.DIV({'class' : 'crvThumbImage'}))), h.TR( {
                'class' : "crvThumbL3"
            }, h.TD()))));
        } else {
            return h.A( {
                'class' : 'crvScrollThumb crvThumbHorizontal',
                id : this.id,
                tabindex : -1,
                href : "javascript:void(0)"
            }, h.TABLE( {
                cellspacing : 0,
                cellpadding : 0
            }, h.TR(null, h.TD( {
                'class' : "crvThumbL1"
            }), h.TD( {
                'class' : "crvThumbL2"
            }, h.DIV({'class' : 'crvThumbImage'})), h.TD( {
                'class' : "crvThumbL3"
            }))));
        }

    },

    init : function() {
        this.layer = getLayer(this.id);
        this.css = this.layer.style;
        bobj.connectDOMEvent(this.layer, 'onmousedown', this._mouseDownCB);
    },

    _getScrollTrackLength : function() {
        var par = this.layer.offsetParent;
        if (!par) return 0;
        
        if (this._isVertical)
            return par.offsetHeight;
        else
            return par.offsetWidth;
    },
    
    _setThumbWidth : function(w) {
        this.css.width = w + "%";
        this._length = null;
    },
    
    _setThumbHeight : function(h) {
        this.css.height = h + "%";
        var height = this.layer.offsetHeight;
        if(_ie && document.documentMode == 7) {
            /**
             * IE7 and IE8 Compatibility view allocate less space to middle TR causing the top/below TR to display
             * more images than what they are supposed to.
             */
            this.layer.firstChild.rows[1].style.height = Math.max(0, height - 4) + "px";
        }
        this._length = null;
    },    
    
    _setThumbLeft : function(l) {
        this.css.left = l + "%";
        this._position = null;
    },
    
    _setThumbTop : function(t) {
        this.css.top = t + "%";
        this._position = null;
    },
    
    /*
     * Returns the length of the thumb in pixels
     */
    getLength : function() {
        if (!this._length) {
            if(this._isVertical)
                this._length = Math.max(14, this.layer.offsetHeight);
            else
                this._length = Math.max(14, this.layer.offsetWidth);
        }
        return this._length;
    },

    /*
     * Resize thumb to new length
     * @param newL new length in %
     */
    setLength : function(newL) {
        if (this._isVertical) 
            this._setThumbHeight(newL);
        else 
            this._setThumbWidth(newL);
    },
    
    /*
     * Returns the position of the thumb in pixels
     */
    getPosition : function() {
        if (!this._position) {
            if(this._isVertical)
                this._position = this.layer.offsetTop;
            else
                this._position = this.layer.offsetLeft;
        }
        return this._position;
    },

    /*
     * Moves thumb to new position
     * @param newP new position in %
     */
    setPosition : function(newP) {
        var trackSpace = this._getScrollTrackLength() - this.getLength();
        var pos = newP * trackSpace / 100;

        if(this._isVertical) {
            this.moveY(pos, false);
        }
        else {
            this.moveX(pos, false);
        }
    },

    /*
     * Moves Thumb to new y offset (in twips)
     * @param newTop , new top position in pixels
     */
    moveY : function(newTop, moveByCode) {
        if(this._isVertical) {
            var oldTop = this.getPosition();
            var scrollTrackH = this._getScrollTrackLength();
            if (newTop < 0)
                newTop = 0;
            else if (newTop + this.getLength() > scrollTrackH)
                newTop = Math.max (0, scrollTrackH - this.getLength());
            
            if (scrollTrackH != 0)
            {
                var newTopPercent = (newTop / scrollTrackH) * 100;
                this._setThumbTop(newTopPercent);
            }
            
            if (oldTop != Math.round(newTop)) {
                var thumbPos = (newTop * 100) / (scrollTrackH - this.getLength());
                if (this._onMoveListnerCB) {
                    this._onMoveListnerCB(thumbPos, moveByCode);
                }
            }
        }
    },

    /*
    * Moves Thumb to new x offset (in twips)
    * @param newLeft , new left position in pixels
    */
   moveX : function(newLeft, moveByCode) {
       if(!this._isVertical) {
           var scrollTrackW = this._getScrollTrackLength();
           if (scrollTrackW == 0)
               return;
           
           if (newLeft < 0)
               newLeft = 0;
           else if (newLeft + this.getLength() > scrollTrackW)
               newLeft = scrollTrackW - this.getLength();
           
           if (newLeft == this.getPosition() && !moveByCode)
               return;
           
           var newLeftPercent = (newLeft / scrollTrackW) * 100;
           this._setThumbLeft(newLeftPercent);
   
           var thumbOffset = (newLeft * 100) / (scrollTrackW - this.getLength());
           if (this._onMoveListnerCB) {
               this._onMoveListnerCB(thumbOffset, moveByCode);
           }
       }
   },
   
   /*
    * Moves Thumb to new x offset (in twips)
    * @param delta , moves thumb by delta in percent (0 to 1)
    */
   moveXByDelta : function(delta) {
       if(!this._isVertical) {
           var scrollTrackW = this._getScrollTrackLength();
           var scrollableW = scrollTrackW - this.getLength();
           
           // Can't move less than one pixel.
           while (Math.abs(delta * scrollableW) > 0 && Math.abs(delta * scrollableW) < 1) {
               delta *= 2;
           }
           
           var newLeft = this.getPosition() + (delta * scrollableW);
           
           if (newLeft < 0)
               newLeft = 0;
           else if (newLeft + this.getLength() > scrollTrackW)
               newLeft = scrollTrackW - this.getLength();
   
           var newLeftPercent = (newLeft / scrollTrackW) * 100;
           this._setThumbLeft(newLeftPercent);
           
           var thumbOffset = (newLeft * 100) / (scrollTrackW - this.getLength());
           if (this._onMoveListnerCB) {
               this._onMoveListnerCB(thumbOffset, false);
           }
       }
   },
   
   /*
    * Moves thumb left by its width
    */
   moveLeft : function() {
       if(!this._isVertical) {
           var newLeft = this.getPosition() - this.getLength();
           this.moveX(newLeft, false);
       }
   },
   
   /*
   * Moves thumb left by its width
   */
  moveRight : function() {
      if(!this._isVertical) {
          var newLeft = this.getPosition() + this.getLength(); 
          this.moveX(newLeft, false);
      }
  },
   
    /*
     * Moves thumb down by its height
     */
    moveDown : function() {
        if(this._isVertical) {
            var newTop = this.getPosition() + this.getLength();    
            this.moveY(newTop);
        }
    },

    /*
     * Moves thumb up by its height
     */
    moveUp : function() {
        if(this._isVertical) {
            var newTop = this.getPosition() - this.getLength();
            this.moveY(newTop);
        }
    },    

    setMoveListener : function(listener) {
        this._onMoveListnerCB = listener;
    },
    
    
    _onMouseMove : function(ev) {
        if(this._isVertical) {
            var newTop = ev.clientY - this._clickOffset;
            this.moveY(newTop);
        }
        else {
            var newLeft = ev.clientX - this._clickOffset;
            this.moveX(newLeft, false);
        }

        return false;
    },

    setDisplayPageOverLay : function (isDisplay) {
        if(!this._pageOverlay && isDisplay) {
            this._pageOverlay = MochiKit.DOM.DIV({'class' : 'crvScrollThumbMoveOverlay', type : 'modal'});
            document.body.appendChild(this._pageOverlay);
        }
        
        if(this._pageOverlay)
            this._pageOverlay.style.display = isDisplay ? "block" : "none";
    },
    
    _onMouseDown : function(ev) {
        if(ev.which == 1 || !!(ev.button & 1)) {    /* mouse left button down */     
            var clientPos = (this._isVertical) ? ev.clientY : ev.clientX;
            this._clickOffset = clientPos - this.getPosition();
            
            this.setDisplayPageOverLay(true);
            bobj.connectDOMEvent(document, 'onmousemove', this._mouseMoveCB);
            bobj.connectDOMEvent(document, 'onmouseup', this._mouseUpCB);
            
            MochiKit.DOM.addElementClass(this.layer, "thumbselected");
            eventCancelBubble(ev);
            if (ev.preventDefault)
                ev.preventDefault();
        }
        else {
            return false;
        }

    },

    _onMouseUp : function(ev) {
        this.setDisplayPageOverLay(false);
        bobj.disconnectDOMEvent(document, 'onmousemove', this._mouseMoveCB);
        bobj.disconnectDOMEvent(document, 'onmouseup', this._mouseUpCB);
        MochiKit.DOM.removeElementClass(this.layer, "thumbselected");
        return true;
    }
};

bobj.crv.ScrollBarToolTip = function() {
    this.id = bobj.uniqueId();
    this._timeoutID = null;
};

bobj.crv.ScrollBarToolTip.prototype = {
    init : function() {
        this.layer = getLayer(this.id);
        this.css = this.layer.style;
        this._animationObj = null;
    },

    getHTML : function() {
        var h = bobj.html;
        return h.DIV( {
            'class' : 'crvScrollThumbToolTip',
            id : this.id
        });
    },

    setPosition : function(thumbClientPos) {
        var newX =  (bobj.crv.config.isRTL?(+(this.getWidth() - 20)) :(-(this.getWidth() + 2))) ; // on the left side of the scroll bar
        this.css.top = (thumbClientPos + this.getHeight() / 2) + "px"; // middle of the thumb
        this.css.left = newX + "px";
    },

    getHeight : function() {
        return this.layer.offsetHeight;
    },

    getWidth : function() {
        return this.layer.offsetWidth;
    },

    setToolTip : function(tooltip) {
        this.setDisplay(true);

        this.layer.innerHTML = tooltip;
        if (this._timeoutID) {
            clearTimeout(this._timeoutID);
        }

        this._timeoutID = setTimeout(MochiKit.Base.bind(this.setDisplay, this, false), _ie ? 1200 : 800);
    },

    setDisplay : function(isDisplay) {
        if (isDisplay) {
            if (this._animationObj) {
                this._animationObj.cancel();
                this._animationObj = null;
            }
            this.css.display = "inline";
            MochiKit.Style.setOpacity(this.layer, 1);
        } else if (!_ie) {
            this._animationObj = new MochiKit.Visual.fade(this.layer, {
                duration : 0.4});
        } else {
            this.css.display = "none";
        }
    }

}
