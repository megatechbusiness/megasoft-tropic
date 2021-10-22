// allInOne.gif - icon images with different width and heights - disabled states are on the right
bobj.crv.allInOne = (function () {
	var o = new Object();
	o.uri = bobj.crvUri('images/allInOne' + (bobj.crv.config.isRTL ? '_rtl' : '')+ '.gif');
	o.css = 'crvAllInOne' + (bobj.crv.config.isRTL ? 'RTL' : '');
	
	var iconHeight22 = 22;
	var offset = 0;
	
	// Toolbar icons are 22 pixels height and have 3 pixels place holder
	offset += 3;
	offset = o.toolbarBackDy = offset;
	offset = o.toolbarForwardDy = offset + iconHeight22;
	offset = o.toolbarExportDy = offset + iconHeight22;
	offset = o.toolbarPrintDy = offset + iconHeight22;
	
	offset = o.toolbarRefreshDy = offset + iconHeight22;
	offset = o.toolbarSearchDy = offset + 16; // 16x16 
	o.toolbarSearchDy += 3; // Extra place holder because image smaller
	offset = o.toolbarUpDy = offset + iconHeight22;
	// Rest of the images don't need the place holder 
	offset -= 3;

	// These are 22 pixels height
	offset = o.groupTreeToggleDy = offset + iconHeight22;
	offset = o.paramPanelToggleDy = offset + iconHeight22;
	offset = o.searchPanelToggleDy = offset + 24; // 24x24 
	
	// These two are 20 pixels height
	offset = o.toolbarPrevPageDy = offset + iconHeight22; // 22x20
	offset = o.toolbarNextPageDy = offset + 20; // 22x20
	
	offset = o.paramRunDy = offset + 20; // 22x22
	offset = o.paramDataFetchingDy = offset + 22; // 16x16
	offset = o.closePanelDy = offset + 16;	// 8x7
	offset = o.openParameterArrowDy = offset + 7;	// 15x15
	offset = o.plusDy = offset + 15; // 13x12
	offset = o.minusDy = offset + 12;  // 13x12
	offset = o.undoDy = offset + 12;  // 16x16
	offset = o.cancelDy = offset + 16;  // 16x16

	offset = o.scrollGrabHDy = offset + 16;
	offset = o.scrollGrabVDy = offset + 17;
	offset = o.scrollThumbHDy = offset + 10;
	offset = o.scrollThumbVDy = offset + 17;
	offset = o.scrollLeftDy = offset + 4;
	offset = o.scrollRightDy = offset + 34;
	offset = o.scrollUpDy = offset + 34;
	offset = o.scrollDownDy = offset + 17;
	
	offset = o.menuArrowsDy = offset + 16; // 32x8
    
	offset = o.breadcrumbReportDy = offset + 8; // 16x16
	offset = o.breadcrumbSubreportDy = offset + 16; // 16x16
	offset = o.breadcrumbDrillDy = offset + 16; // 16x16
	
	return o;
})();
