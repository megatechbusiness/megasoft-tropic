bobj.crv.CanvasEvents = {
    REPORT_ELEMENT_MOUSE_ENTER : 'reportElementMouseEnter',
    REPORT_ELEMENT_MOUSE_LEAVE : 'reportElementMouseLeave',
    REPORT_ELEMENT_CLICK : 'reportElementClick',
    REPORT_ELEMENT_RIGHT_CLICK : 'reportElementRightClick',
    REPORT_CANVAS_MOUSE_ENTER : 'reportCanvasMouseEnter',
    REPORT_CANVAS_MOUSE_LEAVE : 'reportCanvasMouseLeave',
    REPORT_CANVAS_CLICK : 'reportCanvasClick',
    REPORT_CANVAS_RIGHT_CLICK : 'reportCanvasRightClick'
};

bobj.crv.InternalCanvasEvents = {
    REPORT_CANVAS_MOUSE_ENTER : 'internalReportCanvasMouseEnter',
    REPORT_CANVAS_MOUSE_LEAVE : 'internalReportCanvasMouseLeave',
    REPORT_VIEW_MOUSE_ENTER : 'internalReportViewMouseEnter',
    REPORT_VIEW_MOUSE_LEAVE : 'internalReportViewMouseLeave'
};

bobj.crv.CanvasListener = function() {
    function isValidEvent(name) {
        if (!name)
            return false;

        for ( var key in bobj.crv.CanvasEvents) {
            if (bobj.crv.CanvasEvents[key] == name)
                return true;
        }

        for ( var key in bobj.crv.InternalCanvasEvents) {
            if (bobj.crv.InternalCanvasEvents[key] == name)
                return true;
        }

        return false;
    }

    bobj.crv.BaseListener.apply(this, [ isValidEvent ]);
};