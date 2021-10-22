bobj.crv.ReportPageListener = function(page) {
    if (!(this instanceof bobj.crv.ReportPageListener))
        throw "PE. Must instantiate a new instance of bobj.crv.ReportPageListener";

    if (page == null)
        throw "page cannot be null";

    page.setOnUpdateHTMLListener(function() {
        var iframeDoc = getCanvasDocument(page);
        if (iframeDoc) {
            var connect = bobj.connectDOMEvent;
            /**
             * Must disconnect signal on dispose. see dispose function
             */
            connect(document, "onmousemove", onParentDocumentMouseMove);
            connect(iframeDoc, "onmousemove", onCanvasMouseMove);
            connect(iframeDoc, "onclick", onClick);
            connect(iframeDoc, "oncontextmenu", onContextMenu);
        }
    });

    var VALID_REPORT_TYPES = {
        'text' : true,
        'textInGrid' : true,
        'textfield' : true,
        'textfieldInGrid' : true,
        'field' : true,
        'numberField' : true,
        'fieldInGrid' : true,
        'numberFieldInGrid' : true
    };

    var page = page;
    var lastHoveredElem = null;
    var canvasListeners = [];
    var isMouseInCanvas = false;

    function onParentDocumentMouseMove(e) {
        try {
            if (isMouseInCanvas) {
                isMouseInCanvas = false;
                fireEvent(bobj.crv.InternalCanvasEvents.REPORT_CANVAS_MOUSE_LEAVE, new bobj.crv.CanvasEventArgs(page, e));
            }
        }
        catch(e) {
            bobj.logToConsole(e);   
        }
    }

    /*
     * private functions
     */
    function onCanvasMouseMove(e) {
        if (canvasListeners.length == 0) /* avoid unnecessary computation */
            return;
        
        try {
            if (!isMouseInCanvas) {
                isMouseInCanvas = true;
                fireEvent(bobj.crv.InternalCanvasEvents.REPORT_CANVAS_MOUSE_ENTER, new bobj.crv.CanvasEventArgs(page, e));
            }
            
            var newTarget = bobj.getEventTarget(e);
            
            if (lastHoveredElem == newTarget || bobj.isParentOf(newTarget, lastHoveredElem))
                return;
            else
                onMouseIn(newTarget, e);
        }
        catch(e) {
            bobj.logToConsole(e);
        }
    }

    function onContextMenu(e) {
        if (canvasListeners.length == 0) /* avoid unnecessary computation */
            return;
        
        try {
            var isRightClick = e.which == 3;
            var target = bobj.getEventTarget(e);
            var reportElem = findReportElement(target);
            if (reportElem != null) {
                /* fire event for report element */
                fireEvent(bobj.crv.CanvasEvents.REPORT_ELEMENT_RIGHT_CLICK, new bobj.crv.ReportElementCanvasEventArgs(page, reportElem, e));
            }
    
            fireEvent(bobj.crv.CanvasEvents.REPORT_CANVAS_RIGHT_CLICK, new bobj.crv.CanvasEventArgs(page, e));
        }
        catch(e) {
            bobj.logToConsole(e);
        }
    }

    function onClick(e) {
        if (canvasListeners.length == 0) /* avoid unnecessary computation */
            return;
        
        try {
            var isRightClick = e.which == 3;
            var target = bobj.getEventTarget(e);
            var reportElem = findReportElement(target);
            if (reportElem != null) {
                /* fire event for report element */
                fireEvent(isRightClick /* firefox fix */? bobj.crv.CanvasEvents.REPORT_ELEMENT_RIGHT_CLICK
                        : bobj.crv.CanvasEvents.REPORT_ELEMENT_CLICK, new bobj.crv.ReportElementCanvasEventArgs(page, reportElem, e));
            }
    
            fireEvent(isRightClick ? bobj.crv.CanvasEvents.REPORT_CANVAS_RIGHT_CLICK : bobj.crv.CanvasEvents.REPORT_CANVAS_CLICK,
                    new bobj.crv.CanvasEventArgs(page, e));
        }
        catch(e) {
            bobj.logToConsole(e);
        }
    }

    function fireEvent(eventName, eventArgs) {
        if (canvasListeners.length > 0) {
            MochiKit.Iter.forEach(canvasListeners, function(l) {
                l.fire(eventName, eventArgs);
            });
        }
    }

    function onMouseIn(elem, originalEvent) {
        var reportElem = findReportElement(elem);

        if (reportElem != lastHoveredElem)
            onMouseOut(lastHoveredElem, originalEvent);

        if (reportElem != null) {
            lastHoveredElem = reportElem;
            fireEvent(bobj.crv.CanvasEvents.REPORT_ELEMENT_MOUSE_ENTER, new bobj.crv.ReportElementCanvasEventArgs(page, lastHoveredElem,
                    originalEvent));
        }
    }

    function onMouseOut(elem, originalEvent) {
        if (lastHoveredElem != null)
            fireEvent(bobj.crv.CanvasEvents.REPORT_ELEMENT_MOUSE_LEAVE, new bobj.crv.ReportElementCanvasEventArgs(page, lastHoveredElem,
                    originalEvent));

        lastHoveredElem = null;
    }

    function isReportElement(domE) {
        if (domE != null && domE.getAttribute != null) {
            var type = domE.getAttribute("type");
            return type != null && VALID_REPORT_TYPES[type] == true;
        }

        return false;
    }

    function findReportElement(domElem) {
        if (domElem == null)
            return null;

        if (isReportElement(domElem))
            return domElem;

        if (domElem != domElem.parentNode)
            return arguments.callee(domElem.parentNode); // Traverse up teh
        // hierarchy.

        return null;
    }


    /*
     * public functions
     */

    this.addCanvasListener = function(listener) {
        var hasIdentical = MochiKit.Base.findIdentical(canvasListeners, listener) >= 0;
        if (!hasIdentical)
            canvasListeners.push(listener);
    };

    this.removeCanvasListener = function(listener) {
        var index = MochiKit.Base.findIdentical(canvasListeners, listener);
        if (index >= 0)
            canvasListeners.splice(index, 1);
    };

    this.getCanvasListeners = function () {
        return canvasListeners;
    };
    
    this.dispose = function() {
        var iframeDoc = getCanvasDocument(page);
        if (iframeDoc) {
            var disconnect = bobj.connectDOMEvent;
            disconnect(document, "onmousemove", onParentDocumentMouseMove);
            disconnect(iframeDoc, "onmousemove", onCanvasMouseMove);
            disconnect(iframeDoc, "onclick", onClick);
            disconnect(iframeDoc, "oncontextmenu", onContextMenu);
        }
    };

    function getCanvasDocument(page) {
        var pageFrame = getCanvasFrame(page);
        if (pageFrame != null) {
            var iframeDoc = _ie ? pageFrame.contentWindow.document : pageFrame.contentDocument;
            return iframeDoc;
        }

        return null;
    }

    function getCanvasWindow(page) {
        var frame = getCanvasFrame(page);
        if (frame != null)
            return frame.contentWindow;
        else
            return null;
    }

    function getCanvasFrame(page) {
        if(page)
            return page.getIframe();
        
        return null;
    }

    bobj.crv.CanvasEventArgs = function(page, originalEvent) {
        if (!page || !originalEvent)
            throw "programming error";

        this.getCanvasDocument = function() {
            return getCanvasDocument(page);
        };

        this.getCanvasWindow = function() {
            return getCanvasWindow(page);
        };

        this.getCanvasFrame = function() {
            return getCanvasFrame(page);
        };

        this.event = new MochiKit.Signal.Event(this.getCanvasDocument(), originalEvent);
        this.pageNumber = page.getNumber();
    };

    bobj.crv.ReportElementCanvasEventArgs = function(page, reportElem, originalEvent) {
        if (!page || !reportElem || !originalEvent)
            throw "programming error";

        this.event = new MochiKit.Signal.Event(reportElem, originalEvent);
        this.target = reportElem;
        this.text = reportElem.textContent || reportElem.innerText || "";
        this.type = reportElem.getAttribute("type") || "";
        this.url = reportElem.getAttribute("url") || "";
        this.getCanvasDocument = function() {
            return getCanvasDocument(page);
        };

        this.getCanvasWindow = function() {
            return getCanvasWindow(page);
        };

        this.getCanvasFrame = function() {
            return getCanvasFrame(page);
        };

        this.getCanvasOffset = function() {
            return MochiKit.Style.getElementPosition(reportElem, null, reportElem.ownerDocument);
        };

        this.getWindowOffset = function() {
            var pageOffset = this.getCanvasOffset();
            var iframe = this.getCanvasFrame();
            var iframeOffset = MochiKit.Style.getElementPosition(iframe);
            /*
             * IE Quirks mode bug elem.getBoundingClientRect is off by 2px in
             * both x,y co-ordinates
             */
            return {
                x : parseInt(pageOffset.x + iframeOffset.x - (_ie && bobj.isQuirksMode() ? 2 : 0)),
                y : parseInt(pageOffset.y + iframeOffset.y - (_ie && bobj.isQuirksMode() ? 2 : 0))
            };
        };
    };
};
