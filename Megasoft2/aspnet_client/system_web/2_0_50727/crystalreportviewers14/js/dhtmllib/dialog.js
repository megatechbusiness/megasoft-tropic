// <script>

/*
=============================================================
WebIntelligence(r) Report Panel
Copyright(c) 2001-2003 Business Objects S.A.
All rights reserved

Use and support of this software is governed by the terms
and conditions of the software license agreement and support
policy of Business Objects S.A. and/or its subsidiaries. 
The Business Objects products and technology are protected
by the US patent number 5,555,403 and 6,247,008

File: dialog.js
Dialog boxes
=============================================================
*/

var DialogBoxWidget_instances = new Array ();
var DialogBoxWidget_current=null
var DialogBoxWidget_zindex=1000;

var _promptDlgInfo=0
var _promptDlgWarning=1
var _promptDlgCritical=2
    
var _dlgTitleLBorderToTxt=20
    
var _dlgTitleHeight=25
var _dlgTitleMarginBottom=4
var _dlgTitleRBorderToClose=10
    
    //close button in the title
var _dlgTitleCloseBtnImgFile='dialogtitle.gif'
var _dlgTitleCloseBtnW=11
var _dlgTitleCloseBtnH=10
var _dlgTitleCloseBtnDy=26
var _dlgTitleCloseBtnHoverDy=37
    
var _dlgBottomMargin=14



// ================================================================================
// ================================================================================
//
// OBJECT newDialogBoxWidget (Constructor)
//
// Class for dialog box
//
// ================================================================================
// ================================================================================

function newDialogBoxWidget(id,title,width,height,defaultCB,cancelCB,noCloseButton,isAlert)
// CONSTRUCTOR
// id        [String]            the id for DHTML processing
// title     [String]            the caption dialog text
// width     [int optional]      dialog box width
// height    [int optional]      dialog box height
// defaultCB [Function optional] callback when enter key is hit
// cancelCB  [Function optional] callback when esc key is hit or close button pushed
// noCloseButton  [Boolean optional] if true no close (on top right) button is displayed
// isAlert   [Boolean optional]  if true JAWS will read dialog as an "alert" and read everything
{
	var o=newWidget(id)

	o.title=title
	o.width=width
	o.height=height
	o.defaultCB=defaultCB
	o.cancelCB=cancelCB
	o.noCloseButton=noCloseButton?noCloseButton:false
	o.isAlert=isAlert
	o.closeCB=null
	o.resizeable=false
	o.oldMouseDown=null
	o.oldCurrent=null
	o.modal=null
	o.hiddenVis=new Array
	o.lastLink=null
	o.firstLink=null
	o.titleLayer = null
	o.defaultBtn=null
	o.divLayer=null
	
	o.oldInit=o.init
	o.oldShow=o.show
	o.init=DialogBoxWidget_init
	o.setResize=DialogBoxWidget_setResize
	o.beginHTML=DialogBoxWidget_beginHTML
	o.endHTML=DialogBoxWidget_endHTML
	o.show=DialogBoxWidget_Show
	o.center=DialogBoxWidget_center
	o.focus=DialogBoxWidget_focus
	o.setTitle=DialogBoxWidget_setTitle
	o.getContainerWidth=DialogBoxWidget_getContainerWidth
	o.getContainerHeight=DialogBoxWidget_getContainerHeight
	DialogBoxWidget_instances[id]=o
	o.modal=newWidget('modal_'+id)
	o.placeIframe=DialogBoxWidget_placeIframe	
	o.oldResize=o.resize
	o.resize=DialogBoxWidget_resize
	o.attachDefaultButton=DialogBoxWidget_attachDefaultButton
	o.unload=DialogBoxWidget_unload
	o.close = DialogBoxWidget_close;
	o.setCloseCB = DialogBoxWidget_setCloseCB;
	o.setNoCloseButton = DialogBoxWidget_setNoCloseButton;
	o.resizeModal = DialogBoxWidget_ResizeModal;
	o.setParent = DialogBoxWidget_setParent;
	o._onWindowResize = function () {
	    o.resizeModal();
	}
	o.respectPageDirection = false;
	if (!_ie)
	{
		if (o.width!=null) o.width=Math.max(0,width+4)
		if (o.height!=null) o.height=Math.max(0,height+4)
	}
		
	return o
}

function DialogBoxWidget_setParent(parent) {
    this.parent = parent;
}

// ================================================================================

function DialogBoxWidget_setResize(resizeCB,minWidth,minHeight,noResizeW,noResizeH)
// Allow the dialog box to be resized
// resizeCB  [Function] callback for resize
// minWidth  [int]      minimum dialog width
// minHeight [int]      minimum dialog height
// noResizeW [boolean]  disable vertical resizing
// noResizeH [boolean]  disable horizontal resizing
{
	var o=this;
	o.resizeable=true
	o.resizeCB=resizeCB
	o.minWidth=minWidth?minWidth:50
	o.minHeight=minHeight?minHeight:50
	
	o.noResizeW=noResizeW
	o.noResizeH=noResizeH
}

// ================================================================================

function DialogBoxWidget_setTitle(title)
// Change the dialog title
// title [String] the new title
{
	var o=this	
	o.title=title
	if (o.titleLayer == null)
		o.titleLayer = getLayer('titledialog_'+this.id);
	o.titleLayer.innerHTML=convStr(title)	
}

// ================================================================================

function DialogBoxWidget_setCloseIcon(lyr,isActive)
// PRIVATE
{
	changeOffset(lyr,0,(isActive==1?0:18))
}

// ================================================================================

function DialogBoxWidget_beginHTML()
// Get the widget beginning HTML
// Returns : [String] the HTML
{
    with (this) {
        var moveableCb = ' onselectstart="return false" ondragstart="return false" onmousedown="' + _dhtmlLib
                + '.DialogBoxWidget_down(event,\'' + id + '\',this,false);return false;" '
        var titleBG = "background-image:url(" + _skin + "dialogtitle.gif)"

        var mdl = '<div onselectstart="return false" onmouseup="' + _dhtmlLib + '.DialogBoxWidget_keepFocus(\'' + this.id
                + '\');" onmousedown="' + _dhtmlLib + '.eventCancelBubble(event)" border="0" hspace="0" vspace="0" src="' + _skin
                + '../transp.gif" id="modal_' + id + '" type="modal" style="background-color:#888888;opacity:0.3;filter:alpha(opacity:30);position:absolute;top:0px;left:0px;width:1px;height:1px">'
                + (_ie ? img (_skin + '../transp.gif', '100%', '100%', null) : '') + '</div>'

        var btn = '';

        if (_dtd4) {
            btn = '<td style="padding-'+(_rtl?"left":"right")+':' + _dlgTitleRBorderToClose + 'px"><div id="dialogClose_' + id
                    + '" class="dlgCloseBtn" title="' + L_DHTMLLIB_closeDialog + '"></div></td>';
        } else {
            btn = '<td style="padding-'+(_rtl?"left":"right")+':'
                    + _dlgTitleRBorderToClose
                    + 'px">'
                    + simpleImgOffset (_skin + _dlgTitleCloseBtnImgFile, _dlgTitleCloseBtnW, _dlgTitleCloseBtnH, 0, _dlgTitleCloseBtnDy,
                            'dialogClose_' + id, null, L_DHTMLLIB_closeDialog) + '</td>';
        }

        var closeBtn = '<td class="dlgCloseArea" align="left" valign="middle">'
                + '<table border="0" cellspacing="0" cellpadding="0"><tbody><tr style="height:' + _dlgTitleHeight + 'px">' + btn
                + '</tr></tbody></table>' + '</td>';

        var dlgtitle = '<table dir="'+(_rtl?"left":"right")+'" style="height:' + _dlgTitleHeight
                + '" class="dlgTitle" width="100%"  border="0" cellspacing="0" cellpadding="0">' + '<tr valign="top" style="height:'
                + _dlgTitleHeight + 'px">' + '<td ' + moveableCb + ' style="cursor:move;padding-'+(_rtl?'right':'left')+':' + _dlgTitleLBorderToTxt
                + 'px;" width="100%" valign="middle" align="'+(_rtl?'right':'left')+'">' + '<nobr><span id="titledialog_' + id
                + '" tabIndex="0" class="titlezone">' + convStr (title) + '</span></nobr></td>' + closeBtn
                + '</tr></table>';

        var s = '';

        s = mdl;

        var dims = sty ("width", width ? ("" + width + "px") : null)
                + sty ("height", height ? ("" + Math.max (0, height + (_moz ? -2 : 0)) + "px") : null)

        s += '<button style="position:absolute;left:-30px;top:-30px; visibility:hidden" id="firstLink_'+this.id+'" onfocus="'+_dhtmlLib+'.DialogBoxWidget_keepFocus(\''+this.id+'\', true);return false;" ></button>';
        s += '<table border="0" cellspacing="0" cellpadding="0" id="' + id + '" style="display:none;padding:0px;visibility:' + _hide
                + ';position:absolute;top:-2000px;left:-2000px;' + dims + '" ' + (isAlert?'role="alertdialog"':'role="dialog"') + '>';
        s += '<tr><td class="dialogbox" id="td_dialog_' + id + '" onresize="' + _dhtmlLib
                + '.DialogBoxWidget_resizeIframeCB(\'' + id + '\',this)"  valign="top">';
        s += '<table dir='+(_rtl?"rtl":"ltr")+' class="dlgBox2" border="0" cellspacing="0" cellpadding="0" style="width:100%;"><tbody>'
        s += '<tr><td height="' + _dlgTitleHeight +'" valign="top">' + dlgtitle + '<button id="webKitFocusElem' + id + '" style="height:1px;width:1px;position:absolute;left:-10000em;opacity:0;tabIndex:-1"/></td></tr>';
        s += '<tr><td class="dlgBody" valign="top" id="div_dialog_' + id + '">'
        return s;

    }
}
/*
function DialogBoxWidget_beginHTML()
// Get the widget beginning HTML
// Returns : [String] the HTML
{
	with (this)
	{
		var moveableCb=' onselectstart="return false" ondragstart="return false" onmousedown="'+_codeWinName+'.DialogBoxWidget_down(event,\''+id+'\',this,false);return false;" '
		var mdl='<div onselectstart="return false" ondragstart="return false" onmousedown="'+_codeWinName+'.eventCancelBubble(event)" border="0" hspace="0" vspace="0" src="'+_skin+'../transp.gif" id="modal_'+id+'" style="position:absolute;top:0px;left:0px;width:1px;height:1px">'+(_ie?img(_skin+'../transp.gif','100%','100%',null):'')+'</div>'
		var titleBG="background-image:url("+_skin+"dialogtitle.gif)"		
		
		return mdl+
			'<a style="position:absolute;left:-30px;top:-30px; visibility:hidden" id="firstLink_'+this.id+'" href="javascript:void(0)" onfocus="'+_codeWinName+'.DialogBoxWidget_keepFocus(\''+this.id+'\');return false;" ></a>'+
			'<table border="0" cellspacing="0" cellpadding="2" id="'+id+'" style="display:none;padding:0px;visibility:'+_hide+';position:absolute;top:-2000px;left:-2000px;'+sty("width",width?(""+width+"px"):null)+sty("height",height?(""+height+"px"):null)+'"><tr><td id="td_dialog_'+id+'" onresize="'+_codeWinName+'.DialogBoxWidget_resizeIframeCB(\''+id+'\',this)" class="dialogbox" valign="top">'+
			'<table width="100%" style="margin-bottom:4px" border="0" cellspacing="0" cellpadding="0"><tr valign="top">'+
			'<td '+moveableCb+' style="cursor:move;'+titleBG+'" class="titlezone">'+getSpace(5,18)+'</td>'+
			'<td '+moveableCb+' style="cursor:move;'+titleBG+'" class="titlezone" width="100%" valign="middle" align="left"><nobr><span id="titledialog_'+id+'" tabIndex="0" class="titlezone">'+convStr(title)+'</span></nobr></td>'+
			'<td class="titlezone" style="'+titleBG+'">'+
			(noCloseButton?'':'<a href="javascript:void(0)" onclick="'+_codeWinName+'.DialogBoxWidget_close(\''+id+'\');return false;" title="'+ L_DHTMLLIB_closeDialog +'">'+imgOffset(_skin+'dialogelements.gif',18,18,0,18,'dialogClose_'+this.id,'onmouseover="'+_codeWinName+'.DialogBoxWidget_setCloseIcon(this,1)" onmouseout="'+_codeWinName+'.DialogBoxWidget_setCloseIcon(this,0)" ',L_DHTMLLIB_closeDialog,'cursor:'+_hand)+'</a>')+
			'</td>'+
			'</tr></table>'
	}
}
*/

// ================================================================================

function DialogBoxWidget_endHTML()
// Get the widget end HTML
// Returns : [String] the HTML
{
    var s='</td></tr>'
        
        s+='<tr><td style="height:'+_dlgBottomMargin+'px;"></td></tr>';
        s+='</tbody></table>';
        s+='</td></tr></table>';
        s+='<button style="position:absolute;left:-30px;top:-30px; visibility:hidden" id="lastLink_'+this.id+'" onfocus="'+_dhtmlLib+'.DialogBoxWidget_keepFocus(\''+this.id+'\', false);return false;" ></button>' 
        
        return s;}

// ================================================================================

function DialogBoxWidget_getContainerWidth()
// Get the width of the container
// Returns : [int] the width
{
	var o=this
	return o.width-(2+2)
}

// ================================================================================

function DialogBoxWidget_getContainerHeight() 
// Get the height of the container
// Returns : [int] the height
{
	var o=this
	return o.height-(2+18+2+2+2)
}


// ================================================================================

function DialogBoxWidget_close(id) 
// PRIVATE
{
	var o=DialogBoxWidget_instances[id]
	if (o)
	{
		o.show(false)
		if(o.cancelCB!=null) o.cancelCB()
	}
}

//================================================================================

function DialogBoxWidget_setCloseCB(closeCB)
{
	this.closeCB = closeCB;
}

//================================================================================

function DialogBoxWidget_setNoCloseButton(noCloseButton)
{
    if (this.noCloseButton !== noCloseButton){
        this.noCloseButton = noCloseButton;
        if (this.initialized()) {
            this.closeButton.style.visibility = this.noCloseButton?_hide:_show;
        }
    }
}

// ================================================================================

function DialogBoxWidget_resizeIframeCB(id,lyr)
{
	DialogBoxWidget_instances[id].placeIframe()
}

// ================================================================================

function DialogBoxWidget_placeIframe()
{
	var o=this
	if (o.iframe)
	{
		var lyr=o.td_lyr
		if (lyr==null)
		{
			o.td_lyr=lyr=getLayer("td_dialog_"+o.id)
		}
		o.iframe.resize(lyr.offsetWidth,lyr.offsetHeight)
		o.iframe.move(o.layer.offsetLeft,o.layer.offsetTop)
	}
}

// ================================================================================
//use when resize dialogbox by code (exemple: variableEditorDialog.html)
function DialogBoxWidget_resize(w,h)
{
	var o=this;
	o.oldResize(w,h);
	if (o.iframe)
	{
		o.iframe.resize(w,h);
	}	
}

// ================================================================================

function DialogBoxWidget_init()
// Init the widget layers
// Return [void]
{
    if (this.layer != null)
        return

    var o = this
    o.oldInit ();
    o.modal.init ();
    o.lastLink = newWidget ("lastLink_" + o.id)
    o.firstLink = newWidget ("firstLink_" + o.id)
    o.lastLink.init ()
    o.firstLink.init ()
    if (_saf)
        o.webKitFocusElem = getLayer ("webKitFocusElem" + o.id);
        
    o.closeButton = getLayer ('dialogClose_' + o.id)
    o.closeButton.style.visibility = o.noCloseButton?_hide:_show;
    o.closeButton.onmouseover = DialogBoxWidget_moverCloseBtn;
    o.closeButton.onmouseout = DialogBoxWidget_moverCloseBtn;
    o.closeButton.onclick = function() {
        o.close (o.id);
    }
}

function DialogBoxWidget_moverCloseBtn(evt)
{
    var evt=getEvent(evt);
    var over=(evt && evt.type=="mouseover")?true:false;
    
    if (_dtd4)
        this.className=over?'dlgCloseBtnHover':'dlgCloseBtn';
    else
        changeOffset(this,0, over?_dlgTitleCloseBtnHoverDy:_dlgTitleCloseBtnDy);
}
// ================================================================================

function DialogBoxWidget_attachDefaultButton(btn)
// Attach a default button to the dialogbox
//the callback of this button will be call by Enter action
{	
	this.defaultBtn=btn;	
	this.defaultBtn.setDefaultButton();
}

// ================================================================================

var _theLYR=null
var _dlgResize=null

function DialogBoxWidget_down(e,id,obj,isResize)
// PRIVATE
{
	_dlgResize=isResize
	var o=DialogBoxWidget_instances[id],lyr=o.layer,mod=o.modal.layer

	lyr.onmousemove=mod.onmousemove=eval('_curWin.'+_dhtmlLib+'.DialogBoxWidget_move')
	lyr.onmouseup=mod.onmouseup=eval('_curWin.'+_dhtmlLib+'.DialogBoxWidget_up')

	lyr.dlgStartPosx=mod.dlgStartPosx=parseInt(lyr.style.left)
	lyr.dlgStartPosy=mod.dlgStartPosy=parseInt(lyr.style.top)
	
	lyr.dlgStartx=mod.dlgStartx=eventGetX(e)
	lyr.dlgStarty=mod.dlgStarty=eventGetY(e)
	
	lyr.dlgStartw=mod.dlgStartw=o.getWidth()
	lyr.dlgStarth=mod.dlgStarth=o.getHeight()

	lyr._widget=mod._widget=o.widx
	
	_theLYR=lyr
	
	eventCancelBubble(e)
	
	if (lyr.setCapture)
		lyr.setCapture(true)
}

// ================================================================================

function DialogBoxWidget_move(e)
// PRIVATE
{
	var o=_theLYR,dlg=getWidget(o)

	if (_dlgResize)
	{
		var newW=Math.max(dlg.minWidth,o.dlgStartw+eventGetX(e)-o.dlgStartx)
		var newH=Math.max(dlg.minHeight,o.dlgStarth+eventGetY(e)-o.dlgStarty)
		
		dlg.resize(dlg.noResizeW?null:newW,dlg.noResizeH?null:newH)
		
		if (dlg.firstTR)
		{
			if (!dlg.noResizeW)
				dlg.firstTR.style.width=newW-4

			if (!dlg.noResizeH)
				dlg.secondTR.style.height=newH-44
		}
		
		if (dlg.resizeCB)
			dlg.resizeCB(newW,newH)
	}
	else
	{
		var x=Math.max(0,o.dlgStartPosx-o.dlgStartx+eventGetX(e))
		var y=Math.max(0,o.dlgStartPosy-o.dlgStarty+eventGetY(e))
		
		//x = Math.min( Math.max(10,winWidth()-10), x)
		//y = Math.min( Math.max(10,winHeight()-18), y)
		
		
		dlg.iframe.move(x,y)
		dlg.move(x,y)
	}
	eventCancelBubble(e)
	return false
}

// ================================================================================

function DialogBoxWidget_up(e)
// PRIVATE
{
	var o=getWidget(_theLYR),lyr=o.layer,mod=o.modal.layer;
	lyr.onmousemove=mod.onmousemove=null;
	lyr.onmouseup=mod.onmouseup=null;
	
	if (lyr.releaseCapture)
		lyr.releaseCapture();
		
	_theLYR=null
}

// ================================================================================

function DialogBoxWidget_keypress(e)
// PRIVATE
{		
	eventCancelBubble(e)
	var dlg=DialogBoxWidget_current
	if (dlg!=null)
	{			
		switch( eventGetKey(e))
		{			
			case 13://enter
				if(dlg.yes && !dlg.no) //old code ???
				{ 
					if (dlg.defaultCB) dlg.defaultCB();return false; 
				} 
				
				//Behavior is different from textArea and textField with the Enter action
				if(isTextArea(_ie?_curWin.event:e)) return true;
				
				if(dlg.defaultBtn!=null && !dlg.defaultBtn.isDisabled())
				{
					dlg.defaultBtn.executeCB(); return false;
				}				
				break;				
			case 27://cancel
				if (!dlg.noCloseButton) // accept 'esc' only if there is a close button in the dialog
				{
					dlg.show(false)
					hideBlockWhileWaitWidget()
					if (dlg.cancelCB!=null) dlg.cancelCB()
				}
				return false;
				break;
			case 8: //back space				
				return isTextInput(_ie?_curWin.event:e);
				break;
		}
	}
}

// ================================================================================

function DialogBoxWidget_ResizeModal()
// PRIVATE
{
	if(this.parent && this.parent.layer) {
		var parentLayer = this.parent.layer;
		var coord = getPos2(parentLayer);
		var dimension = {w : parentLayer.offsetWidth, h : parentLayer.offsetHeight};
		var m_sty = this.modal.css;
		m_sty.top = coord.y + "px";
		m_sty.left = coord.x + "px";
		m_sty.width = dimension.w + "px";
		m_sty.height = dimension.h + "px";
	}
}

// ================================================================================

function DialogBoxWidget_center()
{
	var o=this;
	var defaults = {modalDisplay : o.modal.css.display, layerDisplay : o.css.display};
	var height = o.layer.offsetHeight, width = o.layer.offsetWidth;
	var yOffset = 0, xOffset = 0;
	
	if(this.parent && this.parent.layer) {
		var parentLayer = this.parent.layer;
		var coord = getPos2(parentLayer);
		var dimension = {w : parentLayer.offsetWidth, h : parentLayer.offsetHeight};
		
		yOffset = coord.y + Math.max(0, (dimension.h - height)/ 2);
		xOffset = coord.x + Math.max(0, (dimension.w - width)/ 2);
	}
	
	o.move(Math.max(0,xOffset),Math.max(0, yOffset));
	o.placeIframe()
}

function DialogBoxWidget_Show(sh)
// Show or Hide the dialog
// sh [boolean] if true show
{
	with (this)
	{
		var m_sty=modal.css
		var l_sty=css
				
		if (sh)
		{
			if (!this.iframe)
			{				
				this.iframe=newWidget(getDynamicBGIFrameLayer().id)			
				this.iframe.init()
				this.iframe.respectPageDirection = this.respectPageDirection;
			}
			
			oldCurrent=DialogBoxWidget_current
			DialogBoxWidget_current=this
						
			if (_ie)
			{		
				layer.onkeydown=eval('_curWin.'+_dhtmlLib+'.DialogBoxWidget_keypress')
				modal.layer.onkeydown=eval('_curWin.'+_dhtmlLib+'.DialogBoxWidget_keypress')
				window.attachEvent("onresize", this._onWindowResize);
			}
			else
			{
				_curDoc.addEventListener("keydown", eval('_curWin.'+_dhtmlLib+'.DialogBoxWidget_keypress'), false)
				window.addEventListener("resize", this._onWindowResize , false);
			}
			
			oldMouseDown=_curDoc.onmousedown
			_curDoc.onmousedown=null
						
			hideBlockWhileWaitWidget()
		}
		else		
		{
			DialogBoxWidget_current=oldCurrent
			oldCurrent=null

			if (_ie)
			{
				layer.onkeydown=null
				modal.layer.onkeydown=null
				window.detachEvent("onresize", this._onWindowResize);
			}
			else
			{
				_curDoc.removeEventListener("keydown", eval('_curWin.'+_dhtmlLib+'.DialogBoxWidget_keypress'), false)
				window.removeEventListener("resize", this._onWindowResize, false);
			}
			
			_curDoc.onmousedown=oldMouseDown
		}
		
		var sameState=(layer.isShown==sh)

		if (sameState)
			return
		layer.isShown=sh

		if (sh)
		{			
			this.iframe.css.zIndex=DialogBoxWidget_zindex++;
			m_sty.zIndex=DialogBoxWidget_zindex++;
			l_sty.zIndex=DialogBoxWidget_zindex++;

			m_sty.display=''
			l_sty.display='block'
			this.iframe.setDisplay(true)
			holdBGIFrame(this.iframe.id);
			this.resizeModal()
			
			this.height=layer.offsetHeight;
			this.width=layer.offsetWidth;
			
			//add scroll bar for small resolution 800*600
			//do the resize action before the center
			if(_small && height)
			{									
				if(divLayer == null)
					divLayer=getLayer("div_dialog_"+id);
				if(divLayer)
				{
					divLayer.style.overflow="auto";
					divLayer.style.height = (winHeight()< height)? (winHeight()-40) : getContainerHeight();
					divLayer.style.width = (_moz?width+20:getContainerWidth());
				}
				resize(null,((winHeight()< height)? (winHeight()-10):null));			
			}
			
			if (isHidden(layer))
			{
				this.center()
			}
			
			if (!_small && this.resizeCB)
				this.resizeCB(width,height)				
										
		}
		else
		{
			m_sty.width='1px'
			m_sty.height='1px'

			m_sty.display='none'
			l_sty.display='none'
			

			if (this.iframe != null) {
				this.iframe.setDisplay(false)
				releaseBGIFrame(this.iframe.id)
			}	
		}	

		modal.show(sh);
		
		firstLink.show(sh)
		lastLink.show(sh)

		oldShow(sh);		
		
		if (DialogBoxWidget_current!=null && sh==true)
			DialogBoxWidget_current.focus();
		
		if (!sh && closeCB!=null) closeCB();
	}
}
	
// must be called to release bg iframe
function DialogBoxWidget_unload()
{
	if (this.iframe) {
		releaseBGIFrame(this.iframe.id)
	}	
}


// ================================================================================

function DialogBoxWidget_keepFocus(id, isFocusLastChild)
// PRIVATE
{					
	var o=DialogBoxWidget_instances[id];
	if (o) o.focus(isFocusLastChild);
}

// ================================================================================

function DialogBoxWidget_focus(isFocusLastChild)
// apply focus on the dialog box
{
with (this)
{
    if(!isFocusLastChild || !layer.querySelectorAll) {
        if (titleLayer == null)
            titleLayer = getLayer('titledialog_'+id);
    
        if(_saf && webKitFocusElem && webKitFocusElem.focus)
            webKitFocusElem.focus();
    
        else if (titleLayer.focus) {
            try { 
                titleLayer.focus();
            } catch (e) {
                if (_ie9Up) throw e;
            }
        }
    }
    else {
        /*
         * Only supported in IE8+/Firefox 3.5+/Safari 3.2
         */
        var focusableElems = layer.querySelectorAll("span[tabindex='0'], div[tabindex='0'], a")
        if(focusableElems.length > 0) {
            focusableElems[focusableElems.length -1].focus();
        }
    }
}
}


// ================================================================================
// ================================================================================
//
// OBJECT newAlertDialog (Constructor)
//
// Class for alert dialog box
//
// ================================================================================
// ================================================================================
/*
function newAlertDialog(id,title,text,okLabel,promptType,yesCB)
// id         [String]            the id for DHTML processing
// title      [String]            the caption dialog text
// text       [String]            the alert text
// okLabel    [String]            label for the OK button
// promptType [_promptDlgInfo | _promptDlgWarning=1 | _promptDlgCritical]
// yesCB      [Function optional] callback when clicking on OK
{
	return newPromptDialog(id,title,text,okLabel,null,promptType,yesCB,null,true)
}
*/
// ================================================================================
// ================================================================================
//
// OBJECT newPromptDialog (Constructor)
//
// Class for prompt dialog box
//
// ================================================================================
// ================================================================================

function newPromptDialog(id,title,text,okLabel,cancelLabel,promptType,yesCB,noCB,noCloseButton,isAlert)
// id          [String]            the id for DHTML processing
// title       [String]            the caption dialog text
// text        [String]            the alert text
// okLabel     [String]            label for the OK button
// cancelLabel [String]            label for the Cancel button
// promptType  [_promptDlgInfo | _promptDlgWarning=1 | _promptDlgCritical]
// yesCB       [String or Function optional] callback when clicking on Yes 
// noCB        [String or Function optional] callback when clicking on No
// isAlert     [Boolean optional]  if true JAWS will read dialog as an "alert" and read everything
{
	var o=newDialogBoxWidget(id,title,300,null,PromptDialog_defaultCB,PromptDialog_cancelCB,noCloseButton,isAlert)
	o.text=text
	o.getHTML=PromptDialog_getHTML
	o.yes=okLabel?newButtonWidget(id+"_yesBtn",okLabel,'PromptDialog_yesCB(\"'+o.id+'\")',70):null
	o.no=cancelLabel?newButtonWidget(id+"_noBtn",cancelLabel,'PromptDialog_noCB(\"'+o.id+'\")',70):null
	o.yesCB=yesCB
	o.noCB=noCB
	o.promptType=promptType
	o.txtLayer = null
	o.imgLayer = null	
	
	o.setPromptType=PromptDialog_setPromptType
	o.setText=PromptDialog_setText
	
	//attach default button for prompt dialog
	if(o.yes) o.attachDefaultButton(o.yes)
	else if(o.no) o.attachDefaultButton(o.no)
	
	return o
}

// ================================================================================

function PromptDialog_getimgPath(promptType)
// PRIVATE
{	
	var imgPath=_skin
	switch(promptType)
	{
		case _promptDlgInfo: imgPath+='information_icon.gif';break
		case _promptDlgWarning: imgPath+='warning_icon.gif';break
		default: imgPath+='critical_icon.gif';break
	}	
	return imgPath
}

// ================================================================================

function PromptDialog_getimgAlt(promptType)
// PRIVATE
{
	var imgAlt=''

	/*
	switch(promptType)
	{
		case _promptDlgInfo: imgAlt=_topfs._labImgInfo;break
		case _promptDlgWarning: imgAlt=_topfs._labImgWarning;break
		default: imgAlt=_topfs._labImgError;break
	}
	*/
	return imgAlt
}

// ================================================================================

function PromptDialog_setPromptType(promptType)
// Change the prompt icon
// promptType [_promptDlgInfo | _promptDlgWarning=1 | _promptDlgCritical]
{
	var o=this
	
	if (o.imgLayer == null)
		o.imgLayer = getLayer("dlg_img_"+o.id)
	
	o.imgLayer.src=PromptDialog_getimgPath(promptType)
	o.imgLayer.alt=PromptDialog_getimgAlt(promptType)		
}

// ================================================================================

function PromptDialog_setText(text)
// Change the prompt text
// text [String] the prompt text
{
	var o=this	
	o.text=text
	
	if (o.txtLayer == null)
		o.txtLayer = getLayer("dlg_txt_"+o.id)

	o.txtLayer.innerHTML='<div tabindex="0">'+convStr(text,false,true)+'</div>'
}

// ================================================================================

function PromptDialog_getHTML()
// Get the widget HTML
// Returns : [String] the HTML
{
	var o=this
	var imgPath=PromptDialog_getimgPath(o.promptType)
	var imgAlt=PromptDialog_getimgAlt(o.promptType)			
	
	return o.beginHTML()+
		'<table class="dialogzone" width="290" cellpadding="0" cellspacing="5" border="0">'+
			'<tr><td>'+
				'<table class="dialogzone" cellpadding="5" cellspacing="0" border="0">'+
					'<tr><td align="right" width="32" >'+img(imgPath,32,32,null,'id="dlg_img_'+o.id+'"',imgAlt)+'</td><td></td><td id="dlg_txt_'+o.id+'" align="left" tabindex="0">'+convStr(o.text,false,true)+'</td></tr>'+
				'</table>'+
			'</td></tr>'+
			'<tr><td>'+getSep()+'</td></tr>'+
			'<tr><td align="right">'+
				'<table cellpadding="5" cellspacing="0" border="0">'+
					'<tr>'+
						(o.yes?'<td>'+o.yes.getHTML()+'</td>':'')+
						(o.no?'<td>'+o.no.getHTML()+'</td>':'')+
					'</tr>'+
				'</table>'+
			'</td></tr>'+
		'</table>'+
	o.endHTML()
}

// ================================================================================

function PromptDialog_defaultCB()
// PRIVATE
{
	var o=this
	if (o.yesCB)
	{
		if (typeof o.yesCB!="string")
			o.yesCB()
		else
			eval(o.yesCB)
	}
/*	
	if (this.yesCB)
		this.yesCB()
*/		
	this.show(false)
	
}

// ================================================================================

function PromptDialog_cancelCB()
// PRIVATE
{
	var o=this
	if (o.noCB)
	{
		if (typeof o.noCB!="string")
			o.noCB()
		else
			eval(o.noCB)
	}
	/*
	if (this.noCB)
		this.noCB()
	*/			
	this.show(false)
}

// ================================================================================

function PromptDialog_yesCB(id)
// PRIVATE
{
	DialogBoxWidget_instances[id].defaultCB()
}

// ================================================================================

function PromptDialog_noCB(id) 
// PRIVATE
{
	DialogBoxWidget_instances[id].cancelCB()
}


DHTMLLIB.add(DialogBoxWidget_down, DialogBoxWidget_keepFocus, DialogBoxWidget_resizeIframeCB, DialogBoxWidget_move, DialogBoxWidget_up, DialogBoxWidget_keypress);
