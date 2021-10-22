/* Copyright (c) Business Objects 2006. All rights reserved. */

/**
 * Statusbar widget constructor ({id: String})
 */
bobj.crv.newReportBreadcrumb = function(kwArgs) {
    var UPDATE = MochiKit.Base.update;
    kwArgs = UPDATE({
        id: bobj.uniqueId(),
        isVisible : kwArgs.isVisible,
        values : kwArgs.values,
        visualStyle : {
            className       : null,
            backgroundColor : null,
            borderWidth     : null,
            borderStyle     : null,
            borderColor     : null,
            fontFamily      : null,
            fontWeight      : null,
            textDecoration  : null,
            color           : null,
            width           : null,
            height          : null,
            fontStyle       : null,
            fontSize        : null         
       }
   }, kwArgs);
       
    var o = newWidget(kwArgs.id);

    o.margin = 0;
    bobj.fillIn(o, kwArgs);  
    o.widgetType = 'ReportBreadcrumb';
    
    // Attach member functions (since we can't use prototypes)
    o.initOld = o.init;
    UPDATE(o, bobj.crv.ReportBreadcrumb);
    /*
    // Do we need this? Can't seem to find it in the HTML
    o.palette = newPaletteWidget(o.id + "_palette");
    o.palette.isLeftTableFixed = true;
    o.add(o.palette);
    */
    o._newWidth = '';
    
    return o;    
};

bobj.crv.ReportBreadcrumb = {
    /**
     * Overrides parent
     */
    init : function() {
        this.initOld ();
        bobj.setVisualStyle (this.layer, this.visualStyle);
        this.updateUI();
    },
    
    /**
     * Overrides parent
     */
    getHTML : function() {
        var h = bobj.html;
        return h.LABEL({'class' : 'crvHidden', id : this.id + "_label"}, L_bobj_crv_Breadcrumb) +
            h.DIV({id : this.id,
                'class' : 'dialogzone',
                'aria-labelledby' : this.id + "_label",
                style : {
                    width : '100%',
                    overflow : 'hidden',
                    position : 'relative'
                }});
    },
    
    /**
     * Update child widgets - if they are exists in the breadcrumb
     */
    update : function(update) {
        if(update != null && update.cons == "bobj.crv.newReportBreadcrumb") {
            this.values = update.args.values;
            this.setVisible(update.args.isVisible);
        }
    },
    
    updateUI : function () {
        var signal = MochiKit.Signal.signal;
        
        if(this.isVisible && this.values != null && this.values.length > 0) {
            this.refreshHTML();
            this.layer.setAttribute("role", "navigation");
            signal (this, "showBreadcrumb");
        }
        else {
            signal (this, "hideBreadcrumb");
            this.layer.removeAttribute("role");
        }
    },
    
    setVisible : function (isVisible) {
        this.isVisible = isVisible;
        this.updateUI();
    },

    navigate : function(args)
    {
        MochiKit.Signal.signal (this, "breadcrumbNavigate", args);
        return false;
    },
    
    /**
     * Render the breadcrumbs as table cells separated by an image - called by getHTML and update
     */
    refreshHTML : function() {
        var values = this.values;
        bobj.removeAllChildElements (this.layer);
        if(values && values.length > 1) {
            var iconURI = bobj.crv.allInOne.uri;
            
            var SPAN = MochiKit.DOM.SPAN;
            var A = MochiKit.DOM.A;
            var TABLE = MochiKit.DOM.TABLE;
            var TBODY = MochiKit.DOM.TBODY;
            var TR = MochiKit.DOM.TR;
            var TD = MochiKit.DOM.TD;
            var bind = MochiKit.Base.bind;
            var cells = [];

            var first = values[0];
            var last = values[values.length - 1];
            
            // add the report icon and first crumb
            var drillName = first.drillname;
            var icon = newIconWidget(o.id + '_icon_' + 0, iconURI, null, null, null, 16, 16, 0, bobj.crv.allInOne.breadcrumbReportDy);
            bobj.crv.setAllClasses(icon, 'breadcrumb');
            var crumb = this._createBreadcrumbItem(this.id + '_a_' + 0, icon, drillName, first);
            cells.push( TD(null, crumb) );
            
            // add separator
            var sep = MochiKit.DOM.IMG({height: "9", width: "14", src: bobj.crvUri('images/breadcrumbSep' + (bobj.crv.config.isRTL ? '_rtl' : '')+ '.gif')}, null);
            sep.style.whiteSpace = 'nowrap';
            cells.push( TD(null, sep) );
            
            // add the drill icon or the subreport icon and the last crumb
            drillName = last.drillname;
            var isSub = last.issub;
            var dy = isSub ? bobj.crv.allInOne.breadcrumbSubreportDy : bobj.crv.allInOne.breadcrumbDrillDy;
            icon = newIconWidget(o.id + '_icon_' + (values.length - 1), iconURI, null, null, null, 16, 16, 0, dy);
            bobj.crv.setAllClasses(icon, 'breadcrumb');
            crumb = this._createBreadcrumbItem(this.id + '_a_' + (values.length - 1), icon, drillName);
            cells.push( TD(null, crumb) );
            
            // add crumbs to a row in a table
            var tbl = TABLE({cellSpacing: '0', cellPadding: '0'}, TBODY(null, MochiKit.DOM.TR(null, cells)));
            this.layer.appendChild(tbl);
            
            // insert crumbs between first and last crumb. parentElement is the row,
            // and refElement is the last separator in the row. New crumbs are added before refElement
            var parentElement = this.layer.lastChild.lastChild.lastChild;
            var refElement = parentElement.lastChild.previousSibling;
            
            // keep adding crumbs until overflow or we run out of crumbs
            for(i = 1; i < values.length-1; i++) {
                drillName = values[i].drillname;
                
                // add the subreport icon if crumb is for the subreport
                isSub = values[i].issub;
                icon = (isSub && i == 1) ? newIconWidget(o.id + '_icon_' + i, iconURI, null, null, null, 16, 16, 0, bobj.crv.allInOne.breadcrumbSubreportDy) : null;
                bobj.crv.setAllClasses(icon, 'breadcrumb');
                crumb = this._createBreadcrumbItem(this.id + '_a_' + i, icon, drillName, values[i]);

                parentElement.insertBefore(TD(null, sep.cloneNode(true)), refElement);
                parentElement.insertBefore(TD(null, crumb), refElement);
                
                // check for overflow and factor in the width of the ellipses 
                if(this.layer.firstChild.clientWidth + 27 > this._newWidth) {
                    // remove previously inserted crumb
                    parentElement.removeChild(refElement.previousSibling);
                    
                    // get remaining drillnames and show as a title for ellipses
                    var tooltip = '';
                    for(j = i; j < values.length-1; j++) {
                        tooltip += values[j].drillname;
                        if(j < values.length-2) tooltip += ' -> ';
                    }

                    // replace last added crumb with ellipses
                    var span = SPAN( {title: tooltip}, '. . .' );
                    span.style.whiteSpace = 'nowrap';
                    parentElement.insertBefore(TD(null, span), refElement);
                    break;
                }
            }
        }
    },
    
    resize : function(width) {
        if(this.values && this._newWidth != width) {
            this._newWidth = width;
            this.refreshHTML();
        }
    },
    
    _createBreadcrumbItem : function(id, icon, drillName, drillValue) {
        var cells = [];
        var text = null;
        if (drillValue) {
            text = MochiKit.DOM.A({
                id : id,
                "class" : "breadcrumbLink",
                title : L_bobj_crv_DrillTo.replace("%1", drillName),
                href : "javascript:void(0)",
                onclick : MochiKit.Base.bind(this.navigate, this, drillValue)
            }, null);
        }
        else {
            text = MochiKit.DOM.SPAN({
                id : id,
                "class" : "breadcrumbText"
            }, null);
        }
        text.style.whiteSpace = 'nowrap';
        var tmp = convStr(drillName, true);
        
        if (bobj.crv.config.isRTL && hasNoRTLCharacters (tmp))
        {
        	text.style.direction = 'ltr';
        	text.style.unicodeBidi = 'embed'; 	
        }          
        text.innerHTML = tmp;
        
        var td = null;
        if (icon) {
            td = MochiKit.DOM.TD(null, null);
            td.innerHTML = icon.getHTML();
            cells.push(td);
        }
        
        td = MochiKit.DOM.TD(null, text);
        cells.push(td);
        
        return MochiKit.DOM.TABLE({'class': 'iconText', style : {'fontSize' : '10px'}, cellSpacing: '0', cellPadding: '0'}, MochiKit.DOM.TBODY(null, MochiKit.DOM.TR(null, cells)));
    }
};

