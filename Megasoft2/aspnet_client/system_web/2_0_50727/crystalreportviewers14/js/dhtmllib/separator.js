/**
 * Separator Constructor.
 */
function newSeparatorWidget(id, h, marginTop, marginRight, marginBottom, marginLeft)
{
    var o=newWidget(id);
    o.height=(h==null)?null:h+'px';
    o.marginTop=marginTop;
    o.marginRight=marginRight;
    o.marginBottom=marginBottom;
    o.marginLeft=marginLeft;
    
    o.getHTML=Separator_getHTML;
    o.getHeight=Separator_getHeight;
    
    return o;    
}

function Separator_getHTML()
{
    var o=this, s='';
    var className = o.height ? 'verticalSeparator' : 'horizontalSeparator';
    var heightStyle = o.height ? 'height:' + o.height + 'px;' : '';
    var widthStyle = (_ie && _isQuirksMode && o.height) ? 'width:2px;' : '';
    
    s += '<div id="'+o.id+'" class="'+className+'" style="margin-left:'+o.marginLeft+'px;margin-right:'+o.marginRight+'px;';
    s += 'margin-top:'+o.marginTop+'px;margin-bottom:'+o.marginBottom+'px;'+heightStyle+widthStyle+'"></div>';
    
    return s;
}

/**
 * @return Returns the outer height of the widget, including margins
 */
function Separator_getHeight() {
    if(this.isDisplayed())
        return this.layer.offsetHeight + this.marginTop + this.marginBottom;
    else
        return 0;
};