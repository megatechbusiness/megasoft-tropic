bobj.crv.SelectList = function(id, ariaLabel) {
    this.id = id ? id : bobj.uniqueId() + "_selectList";
    this.ariaLabel = ariaLabel ? ariaLabel : "";
    this._prevSelectedItem = null;
    this._prevFocusedItem = null;
    this._children = new Array();
    this._curSigs = new Array();
    this.widx=_widgets.length;
    _widgets[this.widx]=this;
    
    this.vsbar = new bobj.crv.VerticalScrollBar();
}

bobj.crv.SelectList.prototype = {
    addChild: function(title, desc, groupPath, isRTL) {
        var newItem = new bobj.crv.SelectListItem(title, desc, groupPath, isRTL);
        append(this._itemsNode, newItem.getHTML());
        newItem.init();
        this._children.push(newItem);
        this._curSigs.push(MochiKit.Signal.connect(newItem, 'selected', this, this.onSelectItem));
        this._curSigs.push(MochiKit.Signal.connect(newItem, 'focused', this, this.onFocusItem));
        
        return newItem;
    },
    
    removeChild : function (child) {
        var childIndex = -1;
        for(var i = this._children.length -1; i >= 0; i--) {
            if(this._children[i] == child) {
                childIndex = i;
                break;
            }
        }
        
        if(childIndex > -1) {
            this._children.splice(childIndex, 1);
            var focusPrevChild = false;
            
            if(child == this._prevFocusedItem) {
                focusPrevChild = true;
                this._prevFocusedItem = null;
            }
            
            if(child == this._prevSelectedItem) {
                focusPrevChild = true;
                this._prevSelectedItem = null;
            }
            
            if(focusPrevChild) {
                var prevChild = this._children[childIndex - 1];
                if(prevChild && prevChild.layer) 
                    prevChild.layer.onfocus();
            }
            
            child.dispose();
        }
        
        
    },
    
   
    onSelectItem : function (item) {
        if(this._prevSelectedItem && this._prevSelectedItem != item)
            this._prevSelectedItem.deselect();
        
        this._prevSelectedItem = item;
        this._prevFocusedItem = item;
        
        MochiKit.Signal.signal(this, 'itemSelected', item);
        
        this._scrollToItem(item);
    },
    
    onFocusItem : function (item) {
         if(this._prevFocusedItem && this._prevFocusedItem != item)
             this._prevFocusedItem.unhighlight();
         
         this._prevFocusedItem = item;
         
         this._scrollToItem(item);
         this._itemsNode.setAttribute("aria-activedescendant", item.getID());
    },
    
    _scrollToItem : function (item) {
        var scrTop = this._itemsNode.scrollTop;
        var ctrHeight = this.layer.offsetHeight;
        
        if (item.layer) {
            var itemTop = item.layer.offsetTop;
            var itemHeight = item.layer.offsetHeight;
            
            // If the item is above the visible area, move the scroll top to the top of the item
            if (itemTop < scrTop)
                this._itemsNode.scrollTop = itemTop;
            // If the item is below the visible area, move the scroll top so that the bottom of
            // the item is visible
            else if (itemTop + itemHeight > scrTop + ctrHeight)
                this._itemsNode.scrollTop = itemTop + itemHeight - ctrHeight;
        }
    },

    reset : function() {
        while(this._children.length > 0) {
            var child = this._children.pop();
            child.dispose();
        }
        
        
        /* removes all the signals */
        while (this._curSigs.length > 0) {
            bobj.crv.SignalDisposer.dispose (this._curSigs.pop ());
        }
        
        bobj.removeAllChildElements(this._itemsNode);
        this._prevSelectedItem = null;
        this._prevFocusedItem = null;

        this.vsbar.layer.style.display = 'none';
    },
    
    getHTML : function () {
        var h = bobj.html;

        return h.DIV({id : this.id,
            onkeydown : bobj.getExecuteDOMCallbackHTML(this.widx, 'onKeyDown'), 
            onblur : bobj.getExecuteDOMCallbackHTML(this.widx, 'onBlur'),
            'class' : 'selectListControl'},
            h.LABEL({'class' : 'crvHidden', id : this.id + "_label"}, this.ariaLabel),
            h.DIV({
                id : this.id + '_items',
                style : {
                    overflow : 'hidden',
                    padding: '2px'
                },
                'tabIndex' : '0',
                role : 'listbox',
                'aria-labelledby' : this.id + "_label"
            }), this.vsbar.getHTML());
    },
    
    onBlur : function () {
        if(this._prevFocusedItem) {
            this._prevFocusedItem.unhighlight();
            this._prevFocusedItem = null;
        }
    },
    
    init : function () {
        this.layer = getLayer(this.id);
        
        this._itemsNode = getLayer(this.id + '_items');
        
        this.vsbar.init();
        this.vsbar.setScrollableElement (this._itemsNode);
    },
    
    onKeyDown : function (e) {
        if(eventGetKey(e)==38) //arrow up
        {
            if(this._prevFocusedItem != null) {
                var layer = this._prevFocusedItem.layer.previousSibling;
                if(layer != null) {
                    layer.onfocus();
                    return false; //prevent scrolling on widget
                }
            }
            else if(this._prevSelectedItem != null) {
                var layer = this._prevSelectedItem.layer.previousSibling;
                if(layer != null) {
                    layer.onfocus();
                    return false; //prevent scrolling on widget
                }
            }
        }
        else if(eventGetKey(e)==40) //arrow down
        {
            if(this._prevFocusedItem != null) {
                var layer = this._prevFocusedItem.layer.nextSibling;
                if(layer != null) {
                    layer.onfocus();
                    return false; //prevent scrolling on widget
                }
            }
            else if(this._prevSelectedItem != null) {
                var layer = this._prevSelectedItem.layer.nextSibling;
                if(layer != null) {
                    layer.onfocus();
                    return false; //prevent scrolling on widget
                }
            }
            else {
                if(this._children.length > 0) {
                    var firstChild = this._children[0];
                    if(firstChild && firstChild.layer) {
                        firstChild.layer.onfocus();
                        return false;
                    }
                }
            }
        }
        else if(eventGetKey(e) == KEY_PAGEUP || eventGetKey(e) == KEY_PAGEDOWN)
        {
            if(this._children.length == 0)
                return;
            
            var isPageDown = (eventGetKey(e) == KEY_PAGEDOWN);
            var ADJUSTMENT = 5; //To workaround an issue in IE
            var isOnLastPage =  this._itemsNode.scrollTop + ADJUSTMENT > (this._itemsNode.scrollHeight - this._itemsNode.offsetHeight);
            /*
             *  if last page has smaller height than other pages, make sure current page reflects that so when user presses Page_UP button,
             *  Page at index (length-2) is not skipped
             */
            var pageAdjuster = isOnLastPage ? Math.ceil : Math.round;
            var currentPage = pageAdjuster(this._itemsNode.scrollTop / this._itemsNode.offsetHeight);
                       
            var newPage = Math.max(0, currentPage + ( isPageDown ? 1 : -1));
            this.focusFirstNodeOnPage(newPage);
        }
        else if(eventGetKey(e)==36) /*home*/
        {
            if(this._itemsNode.children.length > 0) {
                this._itemsNode.firstChild.onfocus();
                return false;
            }
        }
        else if(eventGetKey(e)==35) { /*end*/
            if(this._itemsNode.children.length > 0) {
                this._itemsNode.lastChild.onfocus();
                return false;
            }
        }
        else if(eventGetKey(e)==13 || eventGetKey(e)==32) // On Enter or spacebar, signal switch panel
        {
            if(this._prevFocusedItem != null) {
                this._prevFocusedItem.select();
            }
            return false;
        }
        
        return true;        
    },
    
    focusFirstNodeOnPage : function (pageNum) {
        var itemHeight = this._children[0].getHeight();
        var itemsPerPage =  this._itemsNode.offsetHeight / itemHeight;
        var newNodeNum = Math.ceil(itemsPerPage * pageNum);
        
        if(newNodeNum < this._itemsNode.children.length) {
            var node = this._itemsNode.children[newNodeNum];
            node.onfocus();
            this._itemsNode.scrollTop = node.offsetTop;
        }
    },
    
    _doLayout : function(w, h) {
        bobj.setOuterSize(this.layer, w, h);
        
        // we need to subract 1 for ie because of a gradient bug which causes it to overlap the border
        bobj.setOuterSize(this._itemsNode, Math.max(0, w - 2), Math.max(0, h - 2 - (_ie ? 1 : 0)), true);
        if (bobj.isNumber(h)) {
            this.vsbar.adjustForResize (Math.max(0, h - 2));
            
            if (this.vsbar.isDisplayed()) {
                bobj.setOuterSize(this._itemsNode, Math.max(0, w - this.vsbar.SCROLLBAR_SIZE - 2), null);
            }
        }
    },
    
    resize : function (w, h) {
        this._doLayout(w, h);
    }
};
