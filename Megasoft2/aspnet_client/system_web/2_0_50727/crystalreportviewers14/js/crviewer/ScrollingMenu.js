/*
 ================================================================================
 ScrollingMenu
 ================================================================================
 */

/**
 * ScrollingMenu constructor.
 */
bobj.crv.newScrollingMenu = function(id, hideCB, beforeShowCB) {
    var UPDATE = MochiKit.Base.update;

    var o = newMenuWidget(id, hideCB, beforeShowCB);

    o.initOld = o.init;
    o.justInTimeInitOld = o.justInTimeInit;

    o._hasOverflow = false;
    o._scrollCBIntervalID = null;
    
    o.SCROLL_DELTA = 4;
    o.ARROW_HEIGHT = 12;

    UPDATE(o, bobj.crv.ScrollingMenu);

    return o;
};

bobj.crv.ScrollingMenu = {
    justInTimeInit : function() {
        var o = this;
        var bind = MochiKit.Base.bind;
        var connect = MochiKit.Signal.connect;
        
        // call the menu.js justInTimeInit()
        o.justInTimeInitOld();
        
        // store the layers
        o.scrollLayer = getLayer(o.id + '_scroll');
        o.scrollUpLayer = getLayer(o.id + '_scrollUp');
        o.scrollDownLayer = getLayer(o.id + '_scrollDown');
        o.scrollUpImageLayer = getLayer(o.id + '_scrollUpArrow');
        o.scrollDownImageLayer = getLayer(o.id + '_scrollDownArrow');

        // connect the event listeners
        connect(o.scrollUpLayer, "onmouseout", o, o._cancelCallback);
        connect(o.scrollDownLayer, "onmouseout", o, o._cancelCallback);
        connect(o.scrollUpLayer, "onmouseover", bind(o._onScroll, o, true));
        connect(o.scrollDownLayer, "onmouseover", bind(o._onScroll, o, false));
        connect(o.scrollUpLayer, "onmousedown", o, o._onArrowClick);
        connect(o.scrollDownLayer, "onmousedown", o, o._onArrowClick);
        connect(o.scrollLayer, "onscroll", o, o._updateScrolls);
        connect(window, 'onresize', o, o.overflowCB);
    },

    beginHTML : function() {
        var o = this;
        var imgHTML = imgOffset(bobj.crv.allInOne.uri, 8, 8, 0, bobj.crv.allInOne.menuArrowsDy, this.id+'_scrollUpArrow', null, null, "float:none;");
        return '<table cellspacing="0" cellpadding="0"><tr id="'+o.id+'_scrollUp" style="height:'+o.ARROW_HEIGHT+'px;">' +
               '<td class="scrollingMenuArrow" align="center">'+imgHTML+'</td></tr><tr><td>' +
               '<div id="'+o.id+'_scroll" style="position:relative;overflow:hidden;">';
    },

    endHTML : function() {
        var o = this;
        var imgHTML = imgOffset(bobj.crv.allInOne.uri, 8, 8, 16, bobj.crv.allInOne.menuArrowsDy, this.id+'_scrollDownArrow', null, null, "float:none;");
        return '</div></td></tr><tr id="'+o.id+'_scrollDown" style="height:'+o.ARROW_HEIGHT+'px;">' +
               '<td class="scrollingMenuArrow" align="center">'+imgHTML+'</td></tr></table>';
    },
    
    /**
     * Calculates an appropriate size for the menu based on the window height and
     * shows the scrolling arrows if the menu needs scrolling.
     */
    overflowCB : function() {
        var o = this;
        o.scrollLayer.style.height = '0px';
        if (o.isShown()) {
            // calculate the new height based on the window height and the menu's offsetTop
            var top = o.layer.offsetTop;
            
            // adjust the top to take into account document scrolling
            var scrollTop = document.body.scrollTop || document.documentElement.scrollTop;
            top = top - scrollTop;

            var regHeight = o.scrollLayer.scrollHeight;
            var newHeight = Math.max(0, winHeight() - top - (2 * o.ARROW_HEIGHT) - (bobj.isBorderBoxModel() ? 2 : 6));
            
            // set the new height of the menu
            o.scrollLayer.style.height = Math.min(regHeight, newHeight) + 'px';
            
            // display the scrolling arrows if there is overflow
            o._hasOverflow = newHeight < regHeight;
            o.scrollLayer.style.marginTop = o._hasOverflow ? '1px' : '0px';
            o.scrollLayer.style.marginBottom = o._hasOverflow ? '1px' : '0px';
            o.scrollUpLayer.style.display = o._hasOverflow ? '' : 'none';
            o.scrollDownLayer.style.display = o._hasOverflow ? '' : 'none';
            
            // update the size of the iframe
            if (o.iframeLyr) {
                var w = o.getWidth();
                var h = o.getHeight();
                var iCss = o.iframeCss;
                iCss.width = ""+w+"px";
                iCss.height = ""+h+"px";
                iCss.top = o.css.top;
                iCss.left = o.css.left;
            }
            
            if (!o._hasOverflow) {
                // reset the scrollTop if the menu doesn't need scrolling
                o.scrollLayer.scrollTop = 0;
            }
            else {
                // update the scrolling arrows based on the scroll position
                o._updateScrolls();
            }
        }
    },
    
    scroll : function(up) {
        var o = this;
        if (up)
            o.scrollLayer.scrollTop -= o.SCROLL_DELTA;
        else
            o.scrollLayer.scrollTop += o.SCROLL_DELTA;
    },
    
    /**
     * Updates the images of the scrolling arrows
     */
    _updateScrolls : function() {
        var o = this;
        if (o._hasOverflow) {
            var scrollTop = o.scrollLayer.scrollTop;
            if (scrollTop == 0)
                changeOffset(o.scrollUpImageLayer, 8, bobj.crv.allInOne.menuArrowsDy);
            else
                changeOffset(o.scrollUpImageLayer, 0, bobj.crv.allInOne.menuArrowsDy);
            
            if (scrollTop + o.scrollLayer.offsetHeight >= o.scrollLayer.scrollHeight)
                changeOffset(o.scrollDownImageLayer, 24, bobj.crv.allInOne.menuArrowsDy);
            else
                changeOffset(o.scrollDownImageLayer, 16, bobj.crv.allInOne.menuArrowsDy);
        }
    },
    
    _onArrowClick : function(event) {
        // prevent MenuWidget_globalClick() in menu.js from being called
        event.stop();
    },
    
    _onScroll : function(up) {
        var o = this;
        o._scrollCBIntervalID = window.setInterval(MochiKit.Base.bind(o.scroll, o, up), 100);
    },
    
    _cancelCallback : function() {
        var o = this;
        if (o._scrollCBIntervalID) {
            clearInterval(o._scrollCBIntervalID);
        }
        o._scrollCBIntervalID = null;
    }
};
