SAP.common.defineNS("SAP.CR.Viewer.ActionListener", function() {
    bobj.crv.ActionListener.apply(this, arguments);
    var baseOnEvent = this.onEvent;

    this.onEvent = function(name, listener) {
        var isAdded = baseOnEvent.apply(this, arguments);
        if (!isAdded) {
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidEventName.replace("{0}", name).replace(
                    "{1}", "SAP.CR.Viewer.ActionEvents"));

        }
    };

});
SAP.common.defineNS("SAP.CR.Viewer.ActionEvents", bobj.crv.ActionEvents);