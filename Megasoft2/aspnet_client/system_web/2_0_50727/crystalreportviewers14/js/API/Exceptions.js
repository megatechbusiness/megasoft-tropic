SAP.common.defineNS("SAP.CR.Viewer.Exceptions.BaseException", function(name, message, cause) {
    this.name = name;
    this.message = (!message) ? '' : message;
    this.cause = (cause === undefined) ? null : cause;

    this.toString = function() {
        var ret = this.name + " ['" + this.message + "']";
        return (this.cause ? this.cause.toString() + "\r\n" : '') + ret;
    };
});

(function() {
    MochiKit.Iter.forEach( [ "ViewerInitException", "ViewerLoadException", "IllegalOperationException", "IllegalArgumentException",
            "InvalidParamValueException", "InvalidValueType", "InvalidValue", "InvalidRangeBound", "InvalidRangeValue", "MissingArgumentException" ], function(
            exceptionName) {
        var name = "SAP.CR.Viewer.Exceptions." + exceptionName;
        SAP.common.defineNS(name + ".create", function(message, rootcause) {
            return new SAP.CR.Viewer.Exceptions.BaseException(name, message, rootcause);
        });
    });
})();
