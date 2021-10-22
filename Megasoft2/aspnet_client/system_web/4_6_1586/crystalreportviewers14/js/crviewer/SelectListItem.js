bobj.crv.SelectListItem = function(title, desc, groupPath, isRTL, itemIndex) {
    this.id = bobj.uniqueId();
    this.title = title;
    this.desc = desc;
    this.isRTL = isRTL && true;
    this.groupPath = groupPath;
    this.widx=_widgets.length;
    _widgets[this.widx]=this;
    this.isSelected = false;
}

bobj.crv.SelectListItem.prototype = {

    getHTML : function() {
        var h = bobj.html;
        return h.DIV({id : this.id, widx : this.widx, 'class' : 'selectListItem', role : 'option'},
                h.DIV({ 'class' : 'selectListItemTitle', dir : this.isRTL?'rtl':'ltr',
                		style : {'text-align' : bobj.crv.config.isRTL?'right':'left'}}, this.title),
                h.DIV({ 'class' : 'selectListItemDesc'}, this.desc));
    },
    
    init : function() {
        this.layer = getLayer(this.id);
        this.layer.onmouseover = bobj.getExecuteDOMCallback(this.widx, 'onMouseOver');
        this.layer.onclick = bobj.getExecuteDOMCallback(this.widx, 'onClick');
        this.layer.onkeydown = bobj.getExecuteDOMCallback(this.widx, 'onKeyDown');
        this.layer.onmouseout = bobj.getExecuteDOMCallback(this.widx, 'onMouseOut');
        this.layer.onfocus = bobj.getExecuteDOMCallback(this.widx, 'onFocus');
        this.layer.onblur = bobj.getExecuteDOMCallback(this.widx, 'onBlur');
    },  
    
    getID : function() {
        return this.id;
    },
    
    getGroupPath : function () {
    	return this.groupPath;
    },
    
    dispose : function () {
        bobj.deleteWidget(this);
    },
    

    onFocus : function (widx) {
        this.highlight();
        MochiKit.Signal.signal (this, "focused", this);
    },
    
    onBlur : function (widx) {
        this.unhighlight();
    },
    
    onMouseOver : function (widx) {
        this.highlight();
    },
    
    onMouseOut : function (widx) {
    	this.unhighlight();
    },
    
    highlight : function () {
   	MochiKit.DOM.addElementClass (this.layer, "selectListItemHighlighted");
    },
    
    unhighlight : function () {
    	MochiKit.DOM.removeElementClass (this.layer, "selectListItemHighlighted");
    },

    onKeyDown : function(ev) {
        if(eventGetKey(ev)==13 || eventGetKey(ev)==32) // On Enter or spacebar, signal switch panel
            this.select();
    },

    onClick : function() {
        this.select();
    },
    
    select : function () {
        MochiKit.DOM.addElementClass (this.layer, "selectListItemSelected");
        MochiKit.Signal.signal (this, "selected", this);
        this.focus();
        this.isSelected = true;
    	
    },
    
    focus : function () {
        if(this.layer)
            this.layer.focus();
    },
    
    deselect : function () {
    	this.isSelected = false;
        MochiKit.DOM.removeElementClass (this.layer, "selectListItemSelected");
    },
    
    getHeight : Widget_getHeight
};
