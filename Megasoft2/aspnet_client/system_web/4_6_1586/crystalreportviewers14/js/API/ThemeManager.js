SAP.common.defineNS("SAP.CR.Viewer.ThemeManager", new function() {
    this.setThemeColor = function(color, hideGradient) {
        if (bobj.isValidHex(color))
            bobj.crv.themeManager.setThemeColor(color, hideGradient);
        else
            throw SAP.CR.Viewer.Exceptions.IllegalArgumentException.create(L_bobj_crv_API_InvalidColor.replace("{0}", color));
    };
    
    this.setThemeFont = function(family) {
        bobj.crv.themeManager.setThemeFont(family);
    };
});
