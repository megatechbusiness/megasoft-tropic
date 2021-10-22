/*
 ================================================================================
 ThemeManager
 ================================================================================
 */

/**
 * Constructor.
 */
bobj.crv.ThemeManager = function() {
    this.id = bobj.uniqueId();
    
    var defaultThemeColor = '#CDD9E9';
    var defaultFontFamily = 'tahoma, sans-serif';
    var scheme = createScheme(defaultThemeColor);
    var hideGradient = false;
    
    // constants
    var BACKGROUND = "background";
    var BGCOLOR = "background-color";
    var BDCOLOR = "border-color";
    var BDTCOLOR = "border-top-color";
    var BDRCOLOR = "border-right-color";
    var BDBCOLOR = "border-bottom-color";
    var BDLCOLOR = "border-left-color";
    var COLOR = "color";
    var FILTER = "filter";
    var FONTFAMILY = "font-family";
    
    /**
     * Sets the font family of the viewer
     * @param family [string] name of the font family (ie. arial)
     */
    this.setThemeFont = function(family) {
        var fontFamily = family ? family + ', ' + defaultFontFamily : defaultFontFamily;
    
        var sheet = addStyleSheet(this.id + '_font');
        
        // add the css rules to the style sheet
        var selectors = [
            // crviewer style selectors go here
            '.crviewer',
            '.crviewer input',

            // dhtmllib style selectors go here
            '.dialogbox',
            '.dialogzone',
            '.menuFrame',
            '.menuFrame select',
            '.menuFrame input',
            '.dialogbox input',
            '.calendarTextPart'
        ];
        
        for (var i = 0; i < selectors.length; i++) {
            changeCSS(sheet, selectors[i], [FONTFAMILY], [fontFamily]);
        }
    };
    
    /**
     * Sets the theme color of the viewer
     * @param color [string] hex representation of the theme color (ie. #ffffff)
     * @param gradient [boolean] false to show a gradient color, true to show a solid color
     */
    this.setThemeColor = function(color, gradient) {
        scheme = createScheme(bobj.isValidHex(color) ? color : defaultThemeColor);
        hideGradient = gradient;
        
        var sheet = addStyleSheet(this.id + '_color');
        
        // add the css rules to the style sheet
        addRules(sheet);
    };
    
    function addStyleSheet(id) {
        // remove old style element
        var style = getLayer(id);
        if(style)
            MochiKit.DOM.removeElement (style);
        
        // create and add new style element to the document
        bobj.addStyleSheet(null, id);
        style = getLayer(id);

        var sheet = null;
        if (style.sheet)
            sheet = style.sheet;
        else if (style.styleSheet)
            sheet = style.styleSheet;
        
        return sheet;
    };
    var rtl = (bobj.crv.config.isRTL ?'RTL':'');
    function addRules(sheet) {
        var regularRules = [
            // crviewer style selectors go here
            [ '.crviewer', [BDCOLOR], [scheme.dark] ],
            [ '.crtoolbarbottom', [BGCOLOR], [scheme.theme] ],
            [ '.toolbar_buttongroup', [BGCOLOR, BDCOLOR], [scheme.faint, scheme.dark] ],
            [ '.filemenu_hover', [BDCOLOR], [scheme.medium] ],
            [ '.filemenu_depressed', [BDCOLOR], [scheme.medium] ],
            [ '.button_hover .button_image', [BDCOLOR], [scheme.medium] ],
            [ '.button_depressed .button_image', [BDCOLOR], [scheme.medium] ],
            [ '.horizontalSeparator', [BDTCOLOR], [scheme.dark] ],
            [ '.verticalSeparator', [BDLCOLOR], [scheme.dark] ],
            [ '.panelHeaderBottom', [BGCOLOR], [scheme.dark] ],
            [ '.panelHeader' + rtl, [BDTCOLOR, (bobj.crv.config.isRTL?BDLCOLOR:BDRCOLOR)], [scheme.dark, scheme.dark] ],
            [ '.panelNavigator'+rtl, [BGCOLOR, BDCOLOR], [scheme.light, scheme.dark] ],
            [ '.panelNavigatorInnerBorder', [BDCOLOR], [scheme.faint] ],
            [ '.toolPanel'+ rtl, [BGCOLOR, (bobj.crv.config.isRTL?BDLCOLOR:BDRCOLOR), BDBCOLOR], [scheme.faint, scheme.dark, scheme.dark] ],
            [ '.searchPanelTextContainer', [BGCOLOR, BDCOLOR], ['#FFFFFF', scheme.dark] ],
            [ '.searchPanelTextContainer.searchPanelTextContainerDisabled', [BGCOLOR, BDCOLOR], ['#DDDDDD', '#999999'] ],
            [ '.selectListControl', [BDCOLOR], [scheme.dark] ],
            [ '.stackedTabTitleDirty', [BGCOLOR], [scheme.selBottom] ],
            [ '.stackedPanel', [BGCOLOR], [scheme.faint] ],
            [ '.stackedTab', [BGCOLOR, BDCOLOR], [scheme.faint, scheme.dark] ],
            [ '.iactTextField', [BGCOLOR], [scheme.faint] ],
            [ '.iactParamRowEditable', [BDCOLOR], [scheme.dark] ],
            [ '.scrollingMenuArrow', [BDCOLOR], [scheme.dark] ],
            [ '.breadcrumbLink', [COLOR], [scheme.link] ],
            [ '.breadcrumbText', [COLOR], [scheme.visitedLink] ],
            [ '.loadingMessageBar', [BGCOLOR], [scheme.selBottom] ],
            
            // dhtmllib style selectors go here
            [ '.tabbedFrame', [BGCOLOR], [scheme.light] ],
            [ '.dialogzone', [BGCOLOR], [scheme.medium] ],
            [ '.dialogbox', [BGCOLOR, BDCOLOR], [scheme.light, scheme.dark] ],
            [ '.dlgFrame', [BDCOLOR], [scheme.dark] ],
            [ '.dlgBody', [BGCOLOR], [scheme.light] ],
            [ '.menuFrame', [BDCOLOR], [scheme.dark] ],
            [ '.menuIconCheck', [BDCOLOR], [scheme.dark] ],
            [ '.menuLeftPart', [BGCOLOR, BDCOLOR], [scheme.light, scheme.light] ],
            [ '.menuTextPart', [BDCOLOR], [scheme.faint] ],
            [ '.menuRightPart', [BGCOLOR, BDCOLOR], [scheme.faint, scheme.faint] ],
            [ '.menuLeftPartSel', [BDCOLOR], [scheme.dark] ],
            [ '.menuTextPartSel', [BDCOLOR], [scheme.dark] ],
            [ '.menuRightPartSel', [BDCOLOR], [scheme.dark] ],
            [ '.menuTextPartDisabled', [BGCOLOR, BDCOLOR], [scheme.faint, scheme.faint] ],
            [ '.menuCalendarSel', [BGCOLOR, BDCOLOR], [scheme.selBottom, scheme.dark] ],
            [ '.menuCalendar', [BDCOLOR], [scheme.faint] ],
            [ '.treeSelected', [BGCOLOR], [scheme.dark] ],
            [ '.wizbuttonBorder', [BDCOLOR], [scheme.dark] ],
            [ '.textinputsBorder', [BDCOLOR], [scheme.dark] ],
            [ '.textDisabled .textinputsBorder', [BGCOLOR, BDCOLOR], ['#DDDDDD', '#999999'] ],
            [ '.iconnocheck', [BGCOLOR, BDCOLOR], [scheme.light, scheme.light] ],
            [ '.iconcheckhover', [BGCOLOR, BDTCOLOR, BDLCOLOR], [scheme.theme, scheme.dark, scheme.dark] ],
            [ '.iconhover', [BGCOLOR, BDRCOLOR, BDBCOLOR], [scheme.light, scheme.dark, scheme.dark] ],
            [ '.infozone', [BDCOLOR], [scheme.dark] ],
            [ '.combonocheck', [BDCOLOR], [scheme.medium] ],
            [ '.combohover', [BDCOLOR], [scheme.dark] ],
            [ '.comboEditable', [BDCOLOR], [scheme.dark] ],
            [ '.combobtnhover', [BGCOLOR, BDCOLOR], [scheme.dark, scheme.dark] ]
        ];
        
        for (var i = 0; i < regularRules.length; i++) {
            changeCSS(sheet, regularRules[i][0], regularRules[i][1], regularRules[i][2]);
        }

        var gradientRules = [
            // crviewer style selectors go here
            [ '.crtoolbartop', null, scheme.theme, scheme.theme, false ],
            [ '.filemenu_hover', scheme.selBottom, null, scheme.light, false ],
            [ '.filemenu_depressed', scheme.selTop, scheme.selBottom, scheme.selTop, false ],
            [ '.button_hover .button_image', scheme.selBottom, null, scheme.light, false ],
            [ '.button_depressed .button_image', scheme.selTop, scheme.selBottom, scheme.selTop, false ],
            [ '.panelNavigatorItemSelected', scheme.selTop, scheme.selBottom, scheme.selTop, false ],
            [ '.panelNavigatorItemHighlighted', null, scheme.selBottom, scheme.selBottom, false ],
            [ '.panelHeaderTop', scheme.medium, scheme.dark, scheme.dark, false ],
            [ '.selectListItem', '#FFFFFF', '#FFFFFF', '#FFFFFF', false ],
            [ '.selectListItemHighlighted', null, scheme.selBottom, scheme.selBottom, false ],
            [ '.selectListItemSelected', scheme.selTop, scheme.selBottom, scheme.selTop, false ],
            
            // dhtmllib style selectors go here
            [ '.dlgTitle', scheme.dark, scheme.theme, scheme.dark, false ],
            [ '.menuIconCheck', scheme.selTop, scheme.selBottom, scheme.selTop, false ],
            [ '.menuItemBG', null, scheme.selBottom, scheme.selBottom, false ],
            [ '.menuTextPart', scheme.faint, scheme.faint, scheme.faint, false ],
            [ '.wizbuttonBG', null, scheme.medium, scheme.medium, true ]
        ];
        
        for (var i = 0; i < gradientRules.length; i++) {
            changeGradientColor(sheet, gradientRules[i][0], gradientRules[i][1], gradientRules[i][2], gradientRules[i][3], gradientRules[i][4]);
        }
    };
    
    /**
     * Updates the gradient color
     * @param sheet [obj] DOM style sheet to update
     * @param selector [string] css rule's selector text
     * @param startColor [string] hex representation of the gradient start color (ie. #ffffff)
     * @param endColor [string] hex representation of the gradient end color (ie. #ffffff)
     * @param solidColor [string] hex representation of the background color when gradients are turned off (ie. #ffffff)
     */
    function changeGradientColor(sheet, selector, startColor, endColor, solidColor, keepGradient) {
        if (!startColor)
            startColor = '#FAFCFD';
        if (!endColor)
            endColor = '#FAFCFD';
        if (!solidColor)
            solidColor = '#FAFCFD';
        
        if (hideGradient && !keepGradient) {
            startColor = solidColor;
            endColor = solidColor;
        }
    	
        var newStyle = null;
        if (_webKit) {
            newStyle = "-webkit-gradient(linear, left top, left bottom, from("+startColor+"), to("+endColor+"))";
            changeCSS(sheet, selector, [BACKGROUND], [newStyle]);
        }
        else if (_moz) {
            newStyle = "-moz-linear-gradient(top, "+startColor+", "+endColor+")";
            changeCSS(sheet, selector, [BACKGROUND], [newStyle]);
        }
        else if (_ie) {
            newStyle = "progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr='"+startColor+"', EndColorStr='"+endColor+"')";
            changeCSS(sheet, selector, [FILTER], [newStyle]);
        }
    };
    
    /**
     * Adds rules to the style sheet
     * @param sheet [obj] DOM style sheet to update
     * @param selector [string] css rule's selector text
     * @param elements [array] array of string properties
     * @param values [array] array of string values
     */
    function changeCSS(sheet, selector, elements, values) {
        var numElements = elements.length;
        if (numElements != values.length)
            return;
        
        var cssRules = null;
        if (sheet.rules)
            cssRules = sheet.rules;
        else if (sheet.cssRules)
            cssRules = sheet.cssRules;
        else
            return;
        
        // construct the style declaration
        var newValue = '';
        for (var i = 0; i < numElements; i++)
            newValue += elements[i] + ': ' + values[i] + ' !important;';
        
        // add the rule to the style sheet
        if (sheet.insertRule)
            sheet.insertRule(selector + ' { ' + newValue + ' }', cssRules.length);
        else if (sheet.addRule)
            sheet.addRule(selector, newValue);
    };
    
    /**
     * Creates a scheme of colors based on the given color
     * @param color [string] hex representation of the color (ie. #ffffff)
     * @return color scheme
     */
    function createScheme(color) {
        var palette = new bobj.crv.ColorPalette(color);
        return {
            theme: color,
            faint : palette.getSpecifiedColor(2, 100),
            light : palette.getModifiedColor(0, -0.5, 0.5),
            medium : palette.getModifiedColor(0, 0.01, -0.06),
            dark : palette.getModifiedColor(0, 0.25, -0.15),
            selTop : palette.getModifiedColor(-19, 0.54, 0.22),
            selBottom : palette.getModifiedColor(-18, 0.02, 0.55),
            link : palette.getModifiedColor(-14, 1, 0),
            visitedLink : palette.getModifiedColor(-14, 1, -0.31)
        };
    };
};

if (typeof bobj.crv.themeManager == 'undefined') {
    bobj.crv.themeManager = new bobj.crv.ThemeManager();
    
    bobj.crv.themeManager.setThemeColor();
    bobj.crv.themeManager.setThemeFont();
};