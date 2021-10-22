bobj.crv.BaseListener = function(validator) {
    var listeners = {};
    var isValidEvent = validator;

    /**
     * adds native event listener to current canvas listener
     * @return true if event was sucessfully registered
     */
    this.onEvent = function(eventName, listener) {
        if (!isValidEvent || isValidEvent(eventName)) {
            if (listeners[eventName] == null)
                listeners[eventName] = new Array();

            listeners[eventName].push(listener);
            
            return true;
        }
        else {
            return false;
        }
    };

    this.removeEvent = function(eventName, listener) {
        if (listeners[eventName] != null) {
            if (arguments.length == 1)
                listeners[eventName] = [];
            else {
                var list = listeners[eventName];
                for ( var i = list.length - 1; i >= 0; i--) {
                    if (list[i] == listener) {
                        list.splice(i, 1);
                    }
                }
            }
        }
    };

    this.fire = function(eventName, eventArgs) {
        if (listeners[eventName] != null) {
            var list = listeners[eventName];
            for ( var i = 0; i < list.length; i++)
                list[i].call(null, eventArgs);
        }
    };
};