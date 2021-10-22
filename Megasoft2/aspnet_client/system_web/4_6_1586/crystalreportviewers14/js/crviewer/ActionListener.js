bobj.crv.ActionEvents = {
    EXPORT : 'export',
    PRINT : 'print',
    DRILL : 'drill',
    GROUP_TREE_NAVIGATE : 'navigate',
    ERROR : 'error'
};

bobj.crv.BaseEvent = function() {
    var isHandled = false;

    this.setHandled = function(isH) {
        isHandled = isH;
    };

    this.isHandled = function() {
        return isHandled;
    };
};

bobj.crv.ExportEvent = bobj.crv.BaseEvent;
bobj.crv.PrintEvent = bobj.crv.BaseEvent;

bobj.crv.DrillEvent = function(eventArgs) {
    bobj.crv.BaseEvent.apply(this);
    this.groupName = eventArgs[0];
    this.groupPath = eventArgs[1];
    this.groupNamePath = eventArgs[2];
};

bobj.crv.GroupTreeNavigateEvent = function(eventArgs) {
    bobj.crv.BaseEvent.apply(this);
    this.groupName = eventArgs[0];
    this.groupPath = eventArgs[1];
    this.groupNamePath = eventArgs[2];
};

bobj.crv.ErrorEvent = function(eventArgs) {
    bobj.crv.BaseEvent.apply(this);
    this.text = eventArgs[0];
    this.details = eventArgs[1];
    this.code = eventArgs[2] || 0;
    this.RCI = eventArgs[3] || "";
};

bobj.crv.ActionEventFactory = new function() {
    this.create = function(eventName, eventArgs) {
        var eventCons = null;
        switch (eventName) {
        case bobj.crv.ActionEvents.EXPORT:
            eventCons = bobj.crv.ExportEvent;
            break;
        case bobj.crv.ActionEvents.PRINT:
            eventCons = bobj.crv.PrintEvent;
            break;
        case bobj.crv.ActionEvents.DRILL:
            eventCons = bobj.crv.DrillEvent;
            break;
        case bobj.crv.ActionEvents.GROUP_TREE_NAVIGATE:
            eventCons = bobj.crv.GroupTreeNavigateEvent;
            break;
        case  bobj.crv.ActionEvents.ERROR:
            eventCons = bobj.crv.ErrorEvent;
            break;
        default:
            eventCons = bobj.crv.BaseEvent;
        }
        
        if(eventCons != null)
            return new eventCons(eventArgs);
    };
};

bobj.crv.ActionListener = function() {
    function isValidEvent(name) {
        if (!name)
            return false;

        for ( var key in bobj.crv.ActionEvents) {
            if (bobj.crv.ActionEvents[key] == name)
                return true;
        }

        return false;
    }

    bobj.crv.BaseListener.apply(this, [ isValidEvent ]);
};
