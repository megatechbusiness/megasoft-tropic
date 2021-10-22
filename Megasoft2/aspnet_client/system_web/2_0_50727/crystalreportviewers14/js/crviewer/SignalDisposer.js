/*
 *  Disposes signals synchronously and asynchronously. 
 */
if (typeof(bobj.crv.SignalDisposer) == 'undefined') {
    bobj.crv.SignalDisposer = new function() {
        var signals = []; //private variable
        var timerID = null; //private variable
        var CLEAN_SIGNALS_PER_TASK = 20;
        var disconnect = MochiKit.Signal.disconnect;
    
        function cleanTask() {
            var count = CLEAN_SIGNALS_PER_TASK;
            while (signals.length > 0 && count > 0) {
                disconnect (signals.pop ());
                count--;
            }
    
            if (signals.length == 0 && timerID != null) {
                clearInterval (timerID);
                timerID = null;
            }
        }
        
        this.dispose = function(signal, sync) {
            if(signal != null) {
                if(sync) {
                    disconnect(signal);
                }
                else {
                    signals.push (signal);
                    if (timerID == null)
                        timerID = setInterval (cleanTask, 100);
                }
            }
        };
    };
}

