function UltimateScrollingTable(tabId, height, width, sortId) {
    var __version__ = '14.7.17';

    /* Create the variables for the closure */
    var windowWidth;
    var windowHeight;
    var scrollWidth;
    var scrollHeight;
    var naturalWidth;
    var naturalHeight;

    var tHeadMC;
    var tBodyMC;
    var tFootMC;
    var thMC;
    var divHR; /* only if there is a header */
    var thHR;
    var tabHR;
    var divFR; /* only if there is a footer */
    var thFR;
    var tabFR;

    var withinWindow = 0;
    var overX = 'scroll'; // unused
    var overY = 'scroll';
    var widthToUse;
    var tabWidthToUse;
    var hdrWidthToUse;
    var heightToUse;
    var scrollHeightToUse;
    var headerHeight = 0;
    var footerHeight = 0;

    var divRoot = document.createElement('div');

    /* other variables */
    var tabRoot = document.getElementById(tabId);
    var parent = tabRoot.parentNode;
    var divMC = document.createElement('div');
    var maxWidth = 10000;
    var tHeadHR;
    var tHeadFR;
    var convertMCIds;
    /* temporary usage */
    var UST,
	ix,
	iy;

    /* must do this for each of the cloned tables */
    function getGrandChildThList(headTag) {
        var t1 = headTag.getElementsByTagName('th');
        t1 = Array.prototype.slice.call(t1);
        return t1;
    }

    /* calculate the parameters */
    UST = tabRoot.getAttribute('data-ustoverflow-x');
    if (UST !== null) {
        if (UST === "within-window") {
            withinWindow = 1;
        } else {
            overX = UST;
        }
    }
    UST = tabRoot.getAttribute('data-ustoverflow-y');
    if (UST !== null) {
        overY = UST;
    }
    convertMCIds = 0;
    UST = tabRoot.getAttribute('data-ustmcids');
    if (UST !== null) {
        convertMCIds = 1;
    }
    if (sortId === undefined) {
        sortId = tabRoot.getAttribute('data-ustsort');
        if (sortId) {
            ix = Number(sortId, 10);
            if (!isNaN(ix)) {
                sortId = ix;
            }
        }
    }
    if (height === undefined) {
        UST = tabRoot.getAttribute('data-ustheight');
    } else {
        UST = height;
    }
    UST = Number(UST);
    if (isNaN(UST)) {
        height = 0;
    } else {
        height = UST;
    }
    if (width === undefined) {
        UST = tabRoot.getAttribute('data-ustwidth');
    } else {
        UST = width;
    }
    UST = Number(UST);
    if (isNaN(UST)) {
        width = 0;
    } else {
        width = UST;
    }
    UST = window.getComputedStyle(tabRoot, null).maxWidth;
    if (UST.substring(UST.length - 2) === "px") {
        UST = Number(UST.substring(0, UST.length - 2), 10);
        if (!isNaN(UST)) {
            maxWidth = UST;
        }
    }

    /* divRoot will be the container for all 3 copies of the table. */
    parent.insertBefore(divRoot, tabRoot);
    /* divMC will be the container for the scrollable table but first we use
	it as a scratch pad to learn some things about the browser (scrollbar
	sizes) and table (the natural width & height; called maximum width for
	the two-pass auto-sizing algorithm for tables in HTML 4.01). */
    divRoot.appendChild(divMC);
    divMC.style.overflowX = 'scroll';
    divMC.style.overflowY = 'scroll';
    divMC.style.position = 'absolute';
    divMC.appendChild(tabRoot);
    tabRoot.style.width = "";
    tabRoot.style.height = "";

    /* calculate the size of scrollbars */
    divRoot.style.overflow = 'scroll';

    divMC.style.width = maxWidth + "px";

    divRoot.style.width = "100px";
    divRoot.style.height = "100px";
    scrollWidth = divRoot.offsetWidth - divRoot.clientWidth;
    scrollHeight = divRoot.offsetHeight - divRoot.clientHeight;
    divRoot.style.width = "";
    divRoot.style.height = "";
    divRoot.style.overflow = 'hidden';
    divRoot.style.position = 'relative';
    divRoot.style.align = 'left';

    /* precalculate size of table: divMC, presently being given a huge size,
	allows the table to format to its natural size.
	 */
    naturalWidth = tabRoot.offsetWidth;
    naturalHeight = tabRoot.offsetHeight;
    if (sortId) {
        naturalWidth += 25;
    }

    /* find all the interesting pieces of the main content,
	and make <div>s for the header and footer, if needed. */
    tHeadMC = tabRoot.getElementsByTagName('thead')[0];
    if (tHeadMC) {
        thMC = getGrandChildThList(tHeadMC);
    }
    tBodyMC = tabRoot.getElementsByTagName('tbody')[0];
    tFootMC = tabRoot.getElementsByTagName('tfoot')[0];
    if (tHeadMC !== undefined) {
        iy = document.createElement('div');
        iy.style.position = 'absolute';
        divRoot.appendChild(iy);
        divHR = document.createElement('div');
        divHR.style.position = 'static';
        divHR.style.overflow = 'hidden';
        divHR.style.zIndex = '10';
        iy.appendChild(divHR);

        tabHR = tabRoot.cloneNode(true);
        tabHR.style.position = 'relative';
        divHR.appendChild(tabHR);
        iy = divHR.getElementsByTagName('table')[0];
        iy.removeAttribute('id');
        tHeadHR = divHR.getElementsByTagName('thead')[0];
        thHR = getGrandChildThList(tHeadHR);
        thMC = getGrandChildThList(tHeadMC);
        /* zap or convert all the thMC th ids so there is only one copy in thHR */
        for (ix = 0; ix < thMC.length; ix++) {
            iy = thMC[ix];
            if (convertMCIds) {
                if (iy.id) {
                    iy.id = iy.id + "MC";
                }
            } else {
                iy.removeAttribute('id');
            }
        }
    }
    if (tFootMC !== undefined) {
        iy = document.createElement('div');
        iy.style.position = 'absolute';
        divRoot.appendChild(iy);
        divFR = document.createElement('div');
        divFR.style.position = 'static';
        divFR.style.overflow = 'hidden';
        divFR.style.zIndex = '10';
        iy.appendChild(divFR);

        tabFR = tabRoot.cloneNode(true);
        tabFR.style.position = 'relative';
        divFR.appendChild(tabFR);
        iy = divFR.getElementsByTagName('table')[0];
        iy.removeAttribute('id');
        if (sortId && divHR !== undefined) { /* zap all the thFR th ids so there is only one copy in thHR */
            if (divHR !== undefined) {
                tHeadFR = divFR.getElementsByTagName('thead')[0];
                thFR = getGrandChildThList(tHeadFR);
                for (ix = 0; ix < thFR.length; ix++) {
                    iy = thFR[ix];
                    iy.removeAttribute('id');
                }
            }
        }
    }
    /* Finish styling the Main Content */
    divMC.onscroll = function () {
        var far = (-this.scrollLeft) + 'px';
        if (tabHR) {
            tabHR.style.left = far;
        }
        if (tabFR) {
            tabFR.style.left = far;
        }
    };

    function calculate() {
        /* calculate window width & height, width & height to use,
		header & footer heights
		 */
        windowWidth = document.documentElement.clientWidth;
        windowHeight = document.documentElement.clientHeight;
        if (window.innerWidth < windowWidth) {
            windowWidth = window.innerWidth;
        }
        iy = document.getElementsByTagName('body')[0];
        if (iy.clientWidth < windowWidth) {
            windowWidth = iy.clientWidth;
        }
        if (window.innerHeight < windowHeight) {
            windowHeight = window.innerHeight;
        }
        if (width === -2) {
            widthToUse = windowWidth - scrollWidth;
        } else if (width === -1) {
            widthToUse = naturalWidth + scrollWidth;
        } else if (width <= 0) {
            if (naturalWidth + scrollWidth < windowWidth - scrollWidth) {
                widthToUse = naturalWidth + scrollWidth;
            } else {
                widthToUse = windowWidth - scrollWidth;
            }
        } else {
            widthToUse = width;
        }
        tabWidthToUse = widthToUse - scrollWidth;
        if (withinWindow && widthToUse > windowWidth - scrollWidth) {
            widthToUse = windowWidth - scrollWidth;
        }
        hdrWidthToUse = Math.min(tabWidthToUse, widthToUse - scrollWidth);
        divRoot.style.width = (hdrWidthToUse + scrollWidth) + 'px';

        tabRoot.style.width = tabWidthToUse + 'px';
        divMC.style.width = widthToUse + 'px';
        if (divHR) {
            divHR.firstChild.style.width = tabWidthToUse + 'px';
            divHR.style.width = hdrWidthToUse + 'px';
        }
        if (divFR) {
            divFR.firstChild.style.width = tabWidthToUse + 'px';
            divFR.style.width = hdrWidthToUse + 'px';
        }

        /* Obtain the calculated header & footer heights */
        UST = tabRoot.getBoundingClientRect();
        iy = tBodyMC.getBoundingClientRect();
        if (tHeadMC !== undefined) {
            headerHeight = iy.top - UST.top + 1;
        }
        if (tFootMC !== undefined) {
            footerHeight = UST.bottom - iy.bottom + 1;
        }

        /* Calculate the height */
        ix = UST.bottom - UST.top;
        if (height <= 0) {
            heightToUse = windowHeight - scrollHeight;
            if (windowHeight - UST.top >= 100 + headerHeight + footerHeight + scrollHeight) {
                /* Heuristic: if there are at least 100px left on the screen, for the
				scrolling area, use the remaining screen height as the table height.
				If there is less, use the full screen height, assuming the user must
				scroll down anyway, so then might as well have a full screen of data.
				 */
                heightToUse -= UST.top;
            }
            if (heightToUse >= ix) {
                heightToUse = ix + scrollHeight - 10;
                if (overY === "scroll") {
                    divMC.style.overflowY = "hidden";
                }
            }
        } else {
            if (ix < height) {
                heightToUse = ix + scrollHeight;
            } else {
                heightToUse = height;
            }
        }
        scrollHeightToUse = scrollHeight;
        if (tabRoot.clientWidth > hdrWidthToUse) {
            divMC.style.overflowX = 'scroll';
        } else {
            divMC.style.overflowX = 'hidden';
            heightToUse -= scrollHeight;
            scrollHeightToUse = 0;
        }
        if (tabRoot.offsetHeight > heightToUse) {
            divMC.style.overflowY = 'scroll';
        } else {
            divMC.style.overflowY = 'hidden';
        }
        divRoot.style.height = heightToUse + 'px';
        divMC.style.height = heightToUse + 'px';
        if (divHR !== undefined) {
            divHR.style.height = headerHeight + 'px';
        }
        if (divFR !== undefined) {
            divFR.style.height = footerHeight + 'px';
            divFR.parentNode.style.top = (heightToUse - footerHeight
				 - scrollHeightToUse) + 'px';
            divFR.scrollTop = divFR.scrollHeight - footerHeight;
            divFR.style.top = (heightToUse - footerHeight) + 'px';
        }

        if (withinWindow && width > 0) {
            if (widthToUse < windowWidth - scrollWidth) {
                divMC.style.height = heightToUse + 'px';
            } else {
                ix = (windowWidth - scrollWidth) + 'px';
                divRoot.style.width = ix;
                divMC.style.width = ix;
                ix -= scrollWidth;
            }
        }
    }
    calculate();

    if (sortId && divHR !== undefined) {
        /* Add an onClick handler to the header rows in thHR that clicks the
		corresponding header in thMC. */
        for (iy = 0; iy < thHR.length; iy++) {
            ix = thHR[iy];
            ix.onclick = function () {
                var inner;
                var iz = thHR.indexOf(this);
                thMC[iz].click();

                /* copy headers from MC to HR and FR as needed */
                for (iz = 0; iz < thMC.length; iz++) {
                    inner = thMC[iz].innerHTML;
                    thHR[iz].innerHTML = inner;
                    if (thFR !== undefined) {
                        thFR[iz].innerHTML = inner;
                    }
                }
                calculate();
                divMC.onscroll();
            };
        }
        /* call init before making this table sortable, to deal with any tables
		that are not also UltimateScrollingTable tables, and to prevent future
		initializations from occurring. */
        //sorttable.init();
        //tabRoot.classList.add("sortable"); // is this needed?
        //sorttable.makeSortable(tabRoot);

        if (sortId["constructor"] === String) {
            //alert(sortId);
            document.getElementById(sortId).click(); //JR
        }
    }

}
