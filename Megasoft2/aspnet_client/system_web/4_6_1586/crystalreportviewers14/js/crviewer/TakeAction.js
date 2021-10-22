if (typeof bobj == 'undefined') {
    bobj = {};
}
if (typeof bobj.crv == 'undefined') {
    bobj.crv = {};
}
if (typeof bobj.crv.TakeAction == 'undefined') {
    bobj.crv.TakeAction = {};
}

bobj.crv.TakeAction.getInstance = function(frameElement) 
{
	if (!bobj.crv.TakeAction._widgetInstance) 
	{
		bobj.crv.TakeAction._widgetInstance = new bobj.crv.TakeAction._widget (frameElement);
	}
	else if (frameElement)
	{
		bobj.crv.TakeAction._widgetInstance.update(frameElement);
	}

	return bobj.crv.TakeAction._widgetInstance;
};

bobj.crv.TakeAction._widget = function (frameElement) {
	// member variables
	this._currentFrameElement = null;
	this._contentDocumentElement = null;
	this._processingActions = null;
	this._button = null;
	this._menu = null;
	this._menuOverlay = null;
	this._timeoutId = null;
	this._associatedReportObject = null;
	this._associatedReportObjectHasKeyFocus = null;
	this._currentViewerID = null;
	this._noActionsMenuId = null;
	
	// store the frame element to be used to remove dynamically
	this._currentFrameElement = frameElement;
	this._contentDocumentElement = _ie ? frameElement.contentWindow.document : frameElement.contentDocument;
	
	// div used to place the take action feedback on so the feedback is only
	// shown and clipped within the page view only
	var frameContainingDiv = frameElement.parentNode;

	// create the img button
	var imgId = bobj.uniqueId();
	var imgTooltip = L_bobj_crv_TakeActionMenuTip;
	var imgHTML = bobj.html.IMG({id : imgId,
		src : bobj.crvUri('../../images/takeaction/action_oncanvas_normal.gif'),
		alt : imgTooltip,
		title : imgTooltip,
		onmouseover : 'bobj.crv.TakeAction.getInstance()._dropDownMenuMouseover(event,this)',
		onmouseout : 'bobj.crv.TakeAction.getInstance()._dropDownMenuMouseout(event,this)',
		onclick : 'bobj.crv.TakeAction.getInstance()._dropDownMenuOnClick(event,this)',
		'class':'takeaction_menu_button'});
	
	append(frameContainingDiv, imgHTML);
	
    this._button = getLayer(imgId);

    // Create the borders that associated the drop down menu with the report object.
    // Decided to create 4 individual lines and build a border with it because using
    // a single global DIV creates a flicker problem on the mouse events where the
    // report object div is receiving those events and thinks its doing a true mouse
    // in and out thus introduces the drop down menu flicker. The 4 lines should
    // reduce that flicker since the middle empty space is free to use.
    this._borders = {};
    MochiKit.Iter.forEach([['left', true] , ['right', true], ['top', false], ['bottom', false]],
	    MochiKit.Base.bind(function (item) {
		    var borderId = bobj.uniqueId();
			    	
			var borderClass;
			if (item[1])
			   	borderClass = 'takeaction_vertical_borders';
			else
			  	borderClass = 'takeaction_horizontal_borders';
			   	
			var borderHTML = bobj.html.DIV({id : borderId,
			   	'class': borderClass});
			        
			append(frameContainingDiv, borderHTML);
			    
			this._borders[item[0]] = getLayer(borderId);
		}, this)
	);

	// create the drop down menu
    this._menu = bobj.crv.newScrollingMenu (bobj.uniqueId(), MochiKit.Base.bind(this._hideMenuAfterCallback, this));
    
    // create the menu overlay used to disable all events from the viewer
	var menuOverlayId = bobj.uniqueId();
	var menuOverlayHTML = bobj.html.DIV({id : menuOverlayId, 'class': 'takeaction_menu_overlay'});
	targetApp(menuOverlayHTML);
	this._menuOverlay = getLayer(menuOverlayId);
	
	// IE does not support fixed position which will make the entire overlay
	// expand 100% therefore we need to constantly update the position on a
	// scroll
	if (_ie)
	{
		this._menuOverlay.style.position = 'absolute';
		bobj.connectMouseWheelListener (this._menuOverlay, function(){setTimeout (MochiKit.Base.bind(this._updateMenuOverlayPosition, this), 1);});
	}
	
	// flag to indicate whether request sent to the server is being processed
	this._processingActions = false;
};

bobj.crv.TakeAction._widget.prototype = {
		showDropDownButton : function(event, element, viewerName, isFocusIn) 
		{
			if (!this._processingActions && !this._menu.isShown() && this._helper_isMouseLeaveOrEnter(event, element)) 
			{
				// clear the timeout since we're going to show it again
				if (this._timeoutId)
				{
					clearTimeout (this._timeoutId);
					this._timeoutId = null;
				}

				// show the menu button
				var css = this._button.style;
				css.visibility="visible";
				
				// place the menu button directly next to the report object top right
				var position = this._helper_calculateAbsoluteXY (element);
				var xPosOffset = 1;
				var yPosOffset = -1;
				if (_ie && bobj.isQuirksMode())
				{
					if(bobj.crv.config.isRTL)
					{
						xPosOffset = +2;
					}
					else
					{
						xPosOffset = -1;
					}
					
				}
				if(bobj.crv.config.isRTL)
				{
					css.left=(position.x - xPosOffset - this._button.width) + "px";
				}
				else
				{
					css.left=(position.x + element.offsetWidth + xPosOffset) + "px";
				}
				
				css.top=(position.y + yPosOffset) + "px";

				// show the borders on the report object
				var position = this._helper_calculateAbsoluteXY (element);
				this._updateBorders (position, element);
				
				MochiKit.Iter.forEach(['left', 'right', 'top', 'bottom' ],
					MochiKit.Base.bind(function (name) {
						this._borders[name].style.visibility = "visible";
					}, this)
				);

				// store the associated report object used to show the drop down menu,
				// used for the global button widget mouse over
				this._associatedReportObject = element;
				this._associatedReportObjectHasKeyFocus = false;
				
				this._currentViewerID = viewerName;
				
				// for keyboard accessibility
				if (isFocusIn)
				{
					this._button.setAttribute ("src", bobj.crvUri('../../images/takeaction/action_oncanvas_hover.gif'));
					css.outlineColor="invert";
					css.outlineStyle="dotted";
					css.outlineWidth="thin";
				}
			}
		},

		hideDropDownButton : function(event, element, delay) 
		{
			if (!this._processingActions && !this._menu.isShown() && this._helper_isMouseLeaveOrEnter(event, element)) 
			{
				if (delay)
				{
					// set a timeout when hiding the button widget, used to help with
					// the transition between report object to the global button widget
					this._timeoutId = setTimeout (MochiKit.Base.bind(this._hideDropDownButton, this), 300);
				}
				else
				{
					this._hideDropDownButton ();
				}
			}
		},
		
		_hideDropDownButton : function () 
		{
			// make sure the menu button image is reset
			this._button.setAttribute ("src", bobj.crvUri('../../images/takeaction/action_oncanvas_normal.gif'));

			// hide the button
			this._button.style.visibility="hidden";
			this._button.style.outlineColor="";
			this._button.style.outlineStyle="";
			this._button.style.outlineWidth="";

			// hide the borders
			MochiKit.Iter.forEach(['left', 'right', 'top', 'bottom' ],
				MochiKit.Base.bind(function (name) {
					this._borders[name].style.visibility = "hidden";
				}, this)
			);
		},
		
		accessibleKeyUp : function(event) 
		{
			// check whether enter and spacebar key is pressed
			if (event.keyCode && (event.keyCode == 13 || event.keyCode == 32))
			{
				var css = this._button.style;
				if (css.outlineStyle != "" || css.outlineStyle != "" || css.outlineWidth != "")
				{
					css.outlineColor="";
					css.outlineStyle="";
					css.outlineWidth="";
					
					this._associatedReportObjectHasKeyFocus = true;
					this._dropDownMenuOnClick(event, this._button);
				}
			}
		},
		
		update : function (frameElement)
		{
			var oldFrameContainingDiv = this._currentFrameElement.parentNode;
			var newFrameContainingDiv = frameElement.parentNode;
		
			if (oldFrameContainingDiv != newFrameContainingDiv)
			{
				// reset the current frame element
				this._currentFrameElement = frameElement;
				this._contentDocumentElement = _ie ? frameElement.contentWindow.document : frameElement.contentDocument;
				
				// move the button feedback into new frame element
				if (oldFrameContainingDiv != null)
					oldFrameContainingDiv.removeChild (this._button);
				
				if (newFrameContainingDiv != null)
					newFrameContainingDiv.appendChild (this._button);
		
				// move the borders feedback into new frame element
				MochiKit.Iter.forEach([['left', true] , ['right', true], ['top', false], ['bottom', false]],
					MochiKit.Base.bind(function (item) {
						var border = this._borders[item[0]];
						if (oldFrameContainingDiv != null)
							oldFrameContainingDiv.removeChild (border);
						
						if (newFrameContainingDiv != null)
							newFrameContainingDiv.appendChild (border);
					}, this)
				);
			}
		},
		
		_updateBorders : function (position, reportObjectElement)
		{
			// use magic number offsets to make sure the border is drawn inside to
			// outside, note that the border is 2px thickness
			
			var reportObjectHeight = reportObjectElement.offsetHeight;
			var reportObjectWidth = reportObjectElement.offsetWidth;
			
			// create the border positions
			var leftPos = {x:(position.x-1), y:position.y, h:reportObjectHeight};
			var rightPos = {x:(position.x+reportObjectWidth-1), y:position.y, h:leftPos.h};
			var topPos = {x:leftPos.x, y:(position.y-1), w:(reportObjectWidth+2)};
			var bottomPos = {x:leftPos.x, y:(position.y+reportObjectHeight-1), w:topPos.w};

			if (_ie && bobj.isQuirksMode())
			{
				// in quirks mode the borders is not calculated as part of the
				// width/height so we need to adjust
				reportObjectHeight += (reportObjectElement.clientTop * 2);
				reportObjectWidth += (reportObjectElement.clientLeft * 2);
				
				leftPos = {x:(position.x-3), y:(position.y-1), h:reportObjectHeight};
				rightPos = {x:(position.x+reportObjectElement.offsetWidth-3), y:(position.y-1), h:leftPos.h};
				topPos = {x:leftPos.x, y:(position.y-1), w:reportObjectWidth};
				bottomPos = {x:leftPos.x, y:(position.y+reportObjectElement.offsetHeight-1), w:topPos.w};
				
				// adjust the right border height if our report object doesn't have borders
				if (reportObjectElement.clientLeft <= 0)
					rightPos.h += 2;
			}

			// adjust the left
			this._borders.left.style.left=leftPos.x + "px";
			this._borders.left.style.top=leftPos.y + "px";
			this._borders.left.style.height=leftPos.h + "px";
			
			// adjust the right
			this._borders.right.style.left=rightPos.x + "px";
			this._borders.right.style.top=rightPos.y + "px";
			this._borders.right.style.height=rightPos.h + "px";
				
			// adjust the top
			this._borders.top.style.left=topPos.x + "px";
			this._borders.top.style.top=topPos.y + "px";
			this._borders.top.style.width=topPos.w + "px";
			
			// adjust the bottom
			this._borders.bottom.style.left=bottomPos.x + "px";
			this._borders.bottom.style.top=bottomPos.y + "px";
			this._borders.bottom.style.width=bottomPos.w + "px";
		},

		_updateMenuOverlayPosition : function ()
		{
			// only update the menu overlay position for IE since it uses position absolute
			if (_ie)
			{
				this._menuOverlay.style.left = document.body.scrollLeft;
				this._menuOverlay.style.top = document.body.scrollTop;
			}
		},
		
		_getMultiValueContextInfo : function (assoRptObjElement) 
		{
		    var contextId = assoRptObjElement.getAttribute("contextid");
		    var docElement = this._contentDocumentElement;
		    
		    if(contextId != null && contextId.length > 0 && docElement != null) 
		    {
		        var contextInput =  docElement.getElementById(contextId);
		        if(contextInput)
		            return contextInput.value;
		    }
		    
		    return "";
		},

		_hideMenuAfterCallback : function (event)
		{
			// hide the menu overlay
			this._menuOverlay.style.visibility = "hidden";

			if (event && event.keyCode && event.keyCode == 27 && this._associatedReportObjectHasKeyFocus)
			{
				// if we esc from the menu and the report object is currently focused then keep the focus
				this._associatedReportObject.focus();
			}
			else
			{
				// hide the drop down menu button
				this._hideDropDownButton ();
			}
			
			// reset the key focus
			this._associatedReportObjectHasKeyFocus = false;
		},
		
		_invokeAction : function(targetId, assoRptObjElement)
		{
			// setup arguments for action servlet
			var state = bobj.crv.stateManager.getComponentState (this._currentViewerID);
			var drillState = state[state.curViewId];
			
			// page context for .NET only, TODO Java and .NET should use the same code path to 
			// get the subreport name
			var pageContext = drillState.vCtxt;
			
			var subreportName = null;
			if (drillState.srptRqtCtxt)
				subreportName = drillState.srptRqtCtxt.srptNm;

			var requestArgs = {
					'ReportSourceKey' : state.common.reportSourceSessionID,
					'RequestEvent' : 'EventActionInvoke',
					'RequestRptObjName' : assoRptObjElement.id,
					'RequestTargetId' : targetId,
					'RequestContext' : pageContext,
					'RequestSubreportName' : subreportName,
					"RequestContextInfo" : this._getMultiValueContextInfo(assoRptObjElement)
			};

			this._processingActions = true;
			bobj.event.publish('waitingModalBackground', this._currentViewerID, true);

			var onsuccess = MochiKit.Base.bind (function onSuccess(jsonObj) {
				this._processingActions = false;
				bobj.event.publish('waitingModalBackground', this._currentViewerID, false);
				
				if (jsonObj.actionURL)
				{
					window.open (jsonObj.actionURL);
				}	
			}, this);

			var onfail = MochiKit.Base.bind (function onFail(jsonObj) {
				this._processingActions = false;
				bobj.event.publish('waitingModalBackground', this._currentViewerID, false);
				
				var errorMsg = L_bobj_crv_TakeActionExecuteActionError;
				if (jsonObj.actionError)
					errorMsg = jsonObj.actionError;

				bobj.event.publish('displayError', this._currentViewerID, 'errorMessage=' + errorMsg);	
			}, this);
			
			bobj.event.publish('takeaction_asyncRequest', this._currentViewerID, requestArgs, onsuccess, onfail);
		},
		
		_helper_showMenu : function(element, menu, menuOverlay)
		{
			var mockOffset = {x:0, y:0};
			
			// calculate the menu button offset to the main document, this is required
			// because the menu is included under the main document body
			
			var elementPos = MochiKit.Style.getElementPosition(element, null, null);
			mockOffset.x += elementPos.x;
			mockOffset.y += elementPos.y;

			if (_ie && bobj.isQuirksMode())
			{
				mockOffset.x -= 2;
				mockOffset.y -= 2;
			}
			
			// FIXME Nick - if the menu shown does not fit the screen then must position
			// the menu at the top of the button widget
			if (this._menu.layer==null)
	            this._menu.justInTimeInit();
			var menuDimension = MochiKit.Style.getElementDimensions(this._menu.layer);
			var frameWidth = MochiKit.Style.getElementDimensions(this._currentFrameElement).w;
			
			if(bobj.crv.config.isRTL)
			{
				
				if(_ie)
				{
					mockOffset.x = frameWidth + mockOffset.x+182; 		
				}
				mockOffset.x += element.offsetWidth;
			}
			
			menu.show (true, mockOffset.x, mockOffset.y + element.height, null, null, null, null, element.offsetHeight);
			
			// show the menu overlay
			menuOverlay.style.visibility = "visible";
			this._updateMenuOverlayPosition ();
		},

		_helper_addDisabledNoActionsMenuItem : function()
		{
			// add a disabled no actions available menu item if no actions found
			if (!this._noActionsMenuId)
				this._noActionsMenuId = bobj.uniqueId();
			this._menu.internalAdd(this._noActionsMenuId,  L_bobj_crv_TakeActionNoActionsMenuLabel, null, null, null, null, true);
		},

		/*
		 * Calculate the absolute offset position of the given element
		 * 
		 * @param element the element to calculate the absolute offset position
		 */
		_helper_calculateAbsoluteXY : function (element)
		{
			var mockOffset = {x:0, y:0};

			// calculate the elements offset to its frame
			var elementPos = MochiKit.Style.getElementPosition(element, null, this._contentDocumentElement);
			mockOffset.x += elementPos.x;
			mockOffset.y += elementPos.y;

			if (_ie && bobj.isQuirksMode())
			{
				// in quirks mode adjust for borders not included in calculation
				mockOffset.x += 2;
			}

			return mockOffset;
		},

		/*
		 * Determine whether a mouse enter or leave event occurred. To be used only with
		 * the mouseover and mouseout events. This helper method ignores all other
		 * onmouseover and onmouseout events to take into account the event bubble
		 * effect
		 * 
		 * @param event the current event @param element the element that the event
		 * occurred on
		 */
		_helper_isMouseLeaveOrEnter : function(event, element) 
		{
			var relatedTarget;

			if (event.relatedTarget) // Firefox
				relatedTarget = event.relatedTarget;
			else if (event.type == 'mouseout') // IE
				relatedTarget = event.toElement;
			else
				relatedTarget = event.FromElement;

			// get the root, will either be null or this element itself
			while (relatedTarget && relatedTarget != element)
				relatedTarget = relatedTarget.parentNode;

			return (relatedTarget != element);
		},
		
		_dropDownMenuMouseover : function(event, element) 
		{
			if (!this._processingActions && !this._menu.isShown() && this._helper_isMouseLeaveOrEnter(event, element))
			{
				this._button.setAttribute ("src", bobj.crvUri('../../images/takeaction/action_oncanvas_hover.gif'));

				// clear the timeout since we transitioned between report object to global button widget
				// do not need to show the button widget again because we're depending on the timeout
				if (this._timeoutId)
				{
					clearTimeout (this._timeoutId);
					this._timeoutId = null;
				}
			}
		},

		_dropDownMenuMouseout : function(event, element) 
		{
			if (!this._processingActions && !this._menu.isShown() && this._helper_isMouseLeaveOrEnter(event, element))
			{
				this._button.setAttribute ("src", bobj.crvUri('../../images/takeaction/action_oncanvas_normal.gif'));
				this._hideDropDownButton(event, this._associatedReportObject, false);
			}
		},
		
		_dropDownMenuOnClick : function(event, element)
		{
			// update the button image for a mouse down
			this._button.setAttribute ("src", bobj.crvUri('../../images/takeaction/action_oncanvas_press.gif'));
			
			// setup arguments for action servlet
			var state = bobj.crv.stateManager.getComponentState (this._currentViewerID);
			var viewState = state[state.curViewId];
			
			var sessionID = state.common.reportSourceSessionID;
			var rptObjName = this._associatedReportObject.id;
			var rptFieldType = this._associatedReportObject.getAttribute ('fieldtype');
			
			// page context for .NET only, TODO Java and .NET should use the same code path to 
			// get the subreport name
			var pageContext = viewState.vCtxt;
			
			var subreportName = null;
			if (viewState.srptRqtCtxt)
				subreportName = viewState.srptRqtCtxt.srptNm;
			
			var requestArgs = {
				'ReportSourceKey' : sessionID,
				'RequestEvent' : 'EventActionGet',
				'RequestRptObjName' : rptObjName,
				'RequestRptFieldType' : rptFieldType,
				'RequestContext' : pageContext,
				'RequestSubreportName' : subreportName,
				"RequestContextInfo" : this._getMultiValueContextInfo(this._associatedReportObject)
			};
			
			this._menu.removeAll();

			this._processingActions = true;
			bobj.event.publish('waitingModalBackground', this._currentViewerID, true);

			var onsuccess = MochiKit.Base.bind (function onSuccess(jsonObj) {
				this._processingActions = false;
				bobj.event.publish('waitingModalBackground', this._currentViewerID, false);
				
				var actions = jsonObj.actions;
		    	
				if (actions && actions.length > 0)
				{
					for (var i=0; i < actions.length; i++)
					{
						var actionObj = actions[i];
						
						this._menu.internalAdd(actionObj.actionId, actionObj.actionName, 
							MochiKit.Base.bind (function(actionId) {
								this._invokeAction(actionId, this._associatedReportObject);
							}, this, actionObj.actionId)
						);
					}
				}
				else
				{
					this._helper_addDisabledNoActionsMenuItem ();
				}
				
				this._helper_showMenu (element, this._menu, this._menuOverlay);
			}, this);

			var onfail = MochiKit.Base.bind (function onFail(jsonObj) {
				this._processingActions = false;
				bobj.event.publish('waitingModalBackground', this._currentViewerID, false);
				this._helper_addDisabledNoActionsMenuItem ();
				this._helper_showMenu (element, this._menu, this._menuOverlay);
			}, this);
			
			bobj.event.publish('takeaction_asyncRequest', this._currentViewerID, requestArgs, onsuccess, onfail);
		}
};