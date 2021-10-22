SAP.common.defineNS("SAP.CR.InProcReportSource", function (reportId, ebisId, productLocale, docLocale) {
	
	this.getReportId = function() {
		return reportId;
	};
	this.getEBISId = function() {
		return ebisId;
	};
	this.getProductLocale = function() {
		return productLocale;
	};
	this.getDocLocale = function() {
		return docLocale;
	};
});

SAP.common.defineNS("SAP.CR.HANAReportSource", function (packageId, objectName, productLocale, docLocale) {
	var reportId = MochiKit.Base.serializeJSON({ packageId: packageId, objectName: objectName });
	SAP.CR.InProcReportSource.call(this, reportId, "true", productLocale, docLocale);
});

SAP.CR.HANAReportSource.prototype = new SAP.CR.InProcReportSource();