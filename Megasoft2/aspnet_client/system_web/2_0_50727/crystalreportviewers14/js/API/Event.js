SAP.common.defineNS("SAP.CR.Events.BaseEvent", function() {
    return new function() {
        var args = {};

        this.json = function() {
            return args;
        },

        this.addArgument = function(key, value) {
            args[key] = value;
        };
    };
});

SAP.common.defineNS("SAP.CR.Events.SetEnterpriseReportSourceEvent", function(reportIdType, reportId, boeLogonType, boeLogonString, docLocale) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setReportSource", "fromBOE");
    // when only 1 argument is passed in, we are passing in the report source factory session id
    if (arguments.length == 1) {
        e.addArgument("factorySessionID", arguments[0]);
    }
    else {
        e.addArgument("reportId", reportId);
        e.addArgument("reportIdType", reportIdType);
        e.addArgument("boeLogonType", boeLogonType);
        e.addArgument("boeLogonString", boeLogonString);
        e.addArgument("locale", docLocale ? docLocale : "en-US");
    }
    
    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetInProcReportSourceEvent", function(inProcReportSource) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setReportSource", "InProc");
   
    e.addArgument("reportId", inProcReportSource.getReportId());
    e.addArgument("ebisId", inProcReportSource.getEBISId());
    
    var productLocale = inProcReportSource.getProductLocale();
    e.addArgument("productLocale", productLocale ? productLocale : "en-US");
    
    var docLocale = inProcReportSource.getDocLocale();
    e.addArgument("locale", docLocale ? docLocale : "en-US");
    
    
    return e;
});



SAP.common.defineNS("SAP.CR.Events.RefreshReportEvent", function() {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("tb", "refresh");

    return e;
});

SAP.common.defineNS("SAP.CR.Events.DrillDownEvent", function(groupPath) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("brch", groupPath);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetUserDefinedParamsEvent", function(params) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("crprompt", "paramPanel");
    e.addArgument("paramList", params);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.GetPageEvent", function(pageNumber) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("getPage", pageNumber.toString());
    e.addArgument("isTopPage", true);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetResolution", function(dpi) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setResolution", dpi);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetPrintMode", function(mode) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setPrintMode", mode);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetPdfOneClick", function(oneClick) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setPdfOneClick", oneClick);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetTimeZone", function(timeZone) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setTimeZone", timeZone);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetHasRefreshButton", function(isVisible) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setHasRefreshButton", isVisible);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetPromptOnRefresh", function(promptOnRefresh) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setPromptOnRefresh", promptOnRefresh);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetReportMode", function(reportMode) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setReportMode", reportMode);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetZoom", function(zoom) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("tb", "zoom");
    e.addArgument("value", zoom);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetHyperlinkTarget", function(target) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setHyperlinkTarget", target);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetSelectionFormula", function(sf) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setSelectionFormula", sf);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetComponentVisibility", function(component, isVisible) {
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setComponentVisibility", component);
    e.addArgument("isVisible", isVisible);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetHasLogo", function(logo){
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setHasLogo", logo);

    return e;
});

SAP.common.defineNS("SAP.CR.Events.SetLogoURI", function(uri,link,tooltip){
    var e = SAP.CR.Events.BaseEvent();
    e.addArgument("setLogoURI", uri);
    e.addArgument("setLogoLink", link);
    e.addArgument("setLogoToolTip", tooltip);
    
    return e;
});
