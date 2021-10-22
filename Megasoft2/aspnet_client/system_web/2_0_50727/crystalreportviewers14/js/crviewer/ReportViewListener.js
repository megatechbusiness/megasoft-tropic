/**
 * Listener for Report view mouse enter and mouse leave events
 */
bobj.crv.ReportViewListener = function(view) {
    if (!(this instanceof bobj.crv.ReportViewListener))
        throw "PE. Must instantiate a new instance of bobj.crv.ReportViewListener";

    if (view == null)
        throw "view cannot be null";

    var currentPageNumber = null;
    var isMouseInReportPage = false;
    var isMouseInReportView = false;

    function onDocumentMouseMove(e) {
        var target = bobj.getEventTarget(e);
        if (target.getAttribute && target.getAttribute("type") !== "modal") {
            /*
             * prevent unneccsary firing of events when "loading" dialog with
             * transparent modal background is shown
             */
            var isInView = bobj.isParentOf(target, view.layer);
            setMouseInReportView(isInView, e);
        }
    }

    bobj.connectDOMEvent(document, "onmousemove", onDocumentMouseMove);

    function setMouseInReportPage(isInPage, originalEvent) {
        isMouseInReportPage = isInPage;
        if (isInPage) {
            /*
             * If mouse is in reportpage, then it is definitely in view
             */
            setMouseInReportView(true, originalEvent);
        }
    }

    function setMouseInReportView(isInView, originalEvent) {
        if (isMouseInReportView && !isInView) {
            setTimeout(function() {
                /*
                 * Wait 0 msec to prevent unneccessary firing when mouse moves
                 * around the page boundary
                 */
                if (!isMouseInReportView) {
                    fireEvent(bobj.crv.InternalCanvasEvents.REPORT_VIEW_MOUSE_LEAVE, new bobj.crv.ReportViewEvent(originalEvent));
                }

            }, 0);
        } else if (!isMouseInReportView && isInView) {
            setTimeout(function() {
                /*
                 * Wait 0 msec to prevent unneccessary firing when mouse moves
                 * around the page boundary
                 */
                if (isMouseInReportView) {
                    fireEvent(bobj.crv.InternalCanvasEvents.REPORT_VIEW_MOUSE_ENTER, new bobj.crv.ReportViewEvent(originalEvent));
                }
            }, 0);
        }

        isMouseInReportView = isInView;
    }

    view.addCanvasListener((function() {
        var listener = new bobj.crv.CanvasListener();
        listener.onEvent(bobj.crv.InternalCanvasEvents.REPORT_CANVAS_MOUSE_ENTER, function(e) {
            if (currentPageNumber != null && currentPageNumber != e.pageNumber) {
                var page = view.getPage(currentPageNumber);
                if(!page) {
                    /**
                     * Page must have been removed in context switch
                     */
                    page = new function () {
                        this.getNumber = function () {
                            return currentPageNumber;
                        };
                        
                        this.getIframe = function () {
                            return null;
                        };
                    };
                }
                fireEvent(bobj.crv.CanvasEvents.REPORT_CANVAS_MOUSE_LEAVE, new bobj.crv.CanvasEventArgs(page, e.event));
            }

            if (currentPageNumber != e.pageNumber) {
                var page = view.getPage(e.pageNumber);
                fireEvent(bobj.crv.CanvasEvents.REPORT_CANVAS_MOUSE_ENTER, new bobj.crv.CanvasEventArgs(page, e.event));
            }
            currentPageNumber = e.pageNumber;
            setMouseInReportPage(true);
        });

        listener.onEvent(bobj.crv.InternalCanvasEvents.REPORT_CANVAS_MOUSE_LEAVE, function(e) {
            if (currentPageNumber == e.pageNumber) {
                var page = view.getPage(currentPageNumber);
                fireEvent(bobj.crv.CanvasEvents.REPORT_CANVAS_MOUSE_LEAVE, new bobj.crv.CanvasEventArgs(page, e.event));
                currentPageNumber = null;
                setMouseInReportPage(false);
            }
        });

        return listener;
    })());

    function fireEvent(eventName, eventArgs) {
        if (view.getCanvasListeners().length > 0) {
            MochiKit.Iter.forEach(view.getCanvasListeners(), function(l) {
                l.fire(eventName, eventArgs);
            });
        }
    }

    bobj.crv.ReportViewEvent = function(originalEvent) {
        this.event = new MochiKit.Signal.Event(null, originalEvent);
    };
};