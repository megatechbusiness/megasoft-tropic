/* Copyright (c) Business Objects 2006. All rights reserved. */

var L_bobj_crv_MainReport = "Main Report";
// Viewer Toolbar tooltips
var L_bobj_crv_Back = "Go Back";
var L_bobj_crv_Forward = "Go Forward";
var L_bobj_crv_Page = "Page";
var L_bobj_crv_PageNum = "Page {0}";
var L_bobj_crv_FirstPage = "Go to First Page";
var L_bobj_crv_PrevPage = "Go to Previous Page";
var L_bobj_crv_NextPage = "Go to Next Page";
var L_bobj_crv_LastPage = "Go to Last Page";
var L_bobj_crv_InvalidPageNumber = "Please specify a page number greater than 0.";
var L_bobj_crv_ParamPanel = "Prompt Panel";
var L_bobj_crv_Parameters = "Parameters";
var L_bobj_crv_GroupTree = "Group Tree";
var L_bobj_crv_Search = "Find";
var L_bobj_crv_SearchResults = "Find Results";
var L_bobj_crv_DrillUp = "Drill Up";
var L_bobj_crv_DrillTo = "Drill to %1";

var L_bobj_crv_Refresh = "Refresh Report";
var L_bobj_crv_Zoom = "Zoom";
var L_bobj_crv_PageNav = "Page Navigation";
var L_bobj_crv_SelectPage = "Go to Page";
var L_bobj_crv_SearchText = "Find";
var L_bobj_crv_SearchOptions = "Find Options";
var L_bobj_crv_Export = "Export this report";
var L_bobj_crv_Print = "Print this report";
var L_bobj_crv_History = "History";
var L_bobj_crv_ClearHistory = "Clear History";
var L_bobj_crv_TabList = "Tab List";
var L_bobj_crv_Minimize = "Minimize";
var L_bobj_crv_Logo=  "Business Objects Logo";
var L_bobj_crv_FileMenu = "File Menu";
var L_bobj_crv_ResultsFound = "{0} Results Found";
var L_bobj_crv_ResultsFoundInSubreport = "{0} Results Found in {1}";
var L_bobj_crv_MatchWholeWordsOnly = "Match Whole Word Only";
var L_bobj_crv_MatchCase = "Match Case";
var L_bobj_crv_FindMore = "Find More";
var L_bobj_crv_Completed = "Completed";
var L_bobj_crv_Breadcrumb= "Breadcrumb";
var L_bobj_crv_File = "File";

var L_bobj_crv_Show = "Show";
var L_bobj_crv_Hide = "Hide";

var L_bobj_crv_Find = "Find...";
var L_bobj_crv_of = "%1 of %2"; // Example: Page "1 of 3"

var L_bobj_crv_submitBtnLbl = "Export";
var L_bobj_crv_ActiveXPrintDialogTitle = "Print";
var L_bobj_crv_PDFPrintDialogTitle = "Print to PDF";
var L_bobj_crv_PrintRangeLbl = "Page Range:";
var L_bobj_crv_PrintAllLbl = "All Pages";
var L_bobj_crv_PrintPagesLbl = "Select Pages";
var L_bobj_crv_PrintFromLbl = "From:";
var L_bobj_crv_PrintToLbl = "To:";
var L_bobj_crv_PrintInfoTitle = "Print to PDF:";
var L_bobj_crv_PrintInfo1 = 'The viewer must export to PDF to print. Choose the Print option from the PDF reader application once the document is opened.';
var L_bobj_crv_PrintInfo2 = 'Note: You must have a PDF reader installed to print. (eg. Adobe Reader)';
var L_bobj_crv_PrintPageRangeError = "Enter a valid page range.";

var L_bobj_crv_ExportBtnLbl = "Export";
var L_bobj_crv_ExportDialogTitle = "Export";
var L_bobj_crv_ExportFormatLbl = "File Format:";
var L_bobj_crv_ExportInfoTitle = "To export:";

var L_bobj_crv_ParamsApply = "Apply";
var L_bobj_crv_ParamsAdvDlg = "Edit parameter value";
var L_bobj_crv_ParamsDeleteTooltip = "Delete parameter value";
var L_bobj_crv_ParamsAddValue = "Click to Add...";
var L_bobj_crv_ParamsApplyTip = "Apply Parameter Values";
var L_bobj_crv_ParamsDlgTitle = "Enter Values";
var L_bobj_crv_ParamsCalBtn = "Calendar";
var L_bobj_crv_Reset= "Reset";
var L_bobj_crv_ResetTip = "Reset Parameter Values";
var L_bobj_crv_ParamsDirtyTip = "Parameter value has changed. Click the Apply button to apply changes.";
var L_bobj_crv_ParamsDataTip = "This is a data-fetching parameter";
var L_bobj_crv_ParamsMaxNumDefaultValues = "Click here for more items...";
var L_bobj_crv_paramsOpenAdvance = "Advanced prompt button for \'%1\'";

var L_bobj_crv_ParamsInvalidTitle = "The parameter value is not valid";
var L_bobj_crv_ParamsTooLong = "Parameter value can be no more than %1 characters long";
var L_bobj_crv_ParamsTooShort = "Parameter value must be at least %1 characters long";
var L_bobj_crv_ParamsBadNumber = "This parameter is of type \"Number\" and can only contain a negative sign symbol, digits (\"0-9\"), digit grouping symbols or a decimal symbol.";
var L_bobj_crv_ParamsBadCurrency = "This parameter is of type \"Currency\" and can only contain a negative sign symbol, digits (\"0-9\"), digit grouping symbols or a decimal symbol.";
var L_bobj_crv_ParamsBadDate = "This parameter is of type \"Date\" and the correct format is \"%1\" where \"yyyy\" is the four digit year, \"mm\" is the month (e.g. January = 1), and \"dd\" is the day of the month.";
var L_bobj_crv_ParamsBadTime = "This parameter is of type \"Time\" and the correct format is \"hh:mm:ss\" where \"hh\" is hours in a 24 hour clock, \"mm\" is minutes, and \"ss\" is seconds.";
var L_bobj_crv_ParamsBadDateTime = "This parameter is of type \"DateTime\" and the correct format is \"%1 hh:mm:ss\". \"yyyy\" is the four digit year, \"mm\" is the month (e.g. January = 1), \"dd\" is the day of the month, \"hh\" is hours in a 24 hour clock, \"mm\" is minutes, and \"ss\" is seconds.";
var L_bobj_crv_ParamsMinTooltip = "Please specify a %1 value greater than or equal to %2.";
var L_bobj_crv_ParamsMaxTooltip = "Please specify a %1 value less than or equal to %2.";
var L_bobj_crv_ParamsMinAndMaxTooltip = "Please specify a %1 value between %2 and %3.";
var L_bobj_crv_ParamsStringMinOrMaxTooltip = "The %1 length for this field is %2.";
var L_bobj_crv_ParamsStringMinAndMaxTooltip = "The value must be between %1 and %2 characters long.";
var L_bobj_crv_ParamsYearToken = "yyyy";
var L_bobj_crv_ParamsMonthToken = "mm";
var L_bobj_crv_ParamsDayToken = "dd";
var L_bobj_crv_ParamsReadOnly = "This parameter is of type \"Read Only\".";
var L_bobj_crv_ParamsNoValue = "No Value";
var L_bobj_crv_ParamsDuplicateValue = "Duplicate values are not allowed.";
var L_bobj_crv_ParamsEnterOptional = "Enter %1 (Optional)";
var L_bobj_crv_ParamsNoneSelected= "(None Selected)";
var L_bobj_crv_ParamsClearValues= "Clear Values";
var L_bobj_crv_ParamsMoreValues= "%1 more values...";
var L_bobj_crv_ParamsMoreValue= "%1 more value...";
var L_bobj_crv_Error = "Error";
var L_bobj_crv_OK = "OK";
var L_bobj_crv_Cancel = "Cancel";
var L_bobj_crv_showDetails = "Show Details";
var L_bobj_crv_hideDetails = "Hide Details";
var L_bobj_crv_RequestError = "Unable to process your request";
var L_bobj_crv_ServletMissing = "The viewer is unable to connect with the CrystalReportViewerServlet that handles asynchronous requests.\nPlease ensure that the Servlet and Servlet-Mapping have been properly declared in the application\'s web.xml file.";
var L_bobj_crv_FlashRequired = "This content requires Adobe Flash Player 11 or higher. {0}Please click here to install.";
var L_bobj_crv_ReadOnlyInPanel= "This parameter is not editable in the panel. Open advanced prompt dialog to modify its value";

var L_bobj_crv_Tree_Drilldown_Node = "Drilldown node %1";

var L_bobj_crv_ReportProcessingMessage = "Please wait while the document is being processed.";
var L_bobj_crv_PrintControlProcessingMessage = "Please wait while the Crystal Reports Print Control is loaded.";

var L_bobj_crv_SundayShort = "S";
var L_bobj_crv_MondayShort = "M";
var L_bobj_crv_TuesdayShort = "T";
var L_bobj_crv_WednesdayShort = "W";
var L_bobj_crv_ThursdayShort = "T";
var L_bobj_crv_FridayShort = "F";
var L_bobj_crv_SaturdayShort = "S";

var L_bobj_crv_Minimum = "minimum";
var L_bobj_crv_Maximum = "maximum";

var L_bobj_crv_Date = "Date";
var L_bobj_crv_Time = "Time";
var L_bobj_crv_DateTime = "DateTime";
var L_bobj_crv_Boolean = "Boolean";
var L_bobj_crv_Number = "Number";
var L_bobj_crv_Text = "Text";

var L_bobj_crv_InteractiveParam_NoAjax = "The web browser you are using is not configured to show the Parameter Panel.";
var L_bobj_crv_AdvancedDialog_NoAjax= "The viewer is unable to open an advanced prompt dialog.";

var L_bobj_crv_EnableAjax= "Please contact your administrator to enable asynchronous requests.";

var L_bobj_crv_LastRefreshed = "Last Refreshed : {0}";

var L_bobj_crv_Collapse = "Collapse";

var L_bobj_crv_CatalystTip = "Online Resources";
var L_bobj_crv_LoadingPage = "Loading Page {0}...";
var L_bobj_crv_TakeActionMenuTip = "Select a bound action";
var L_bobj_crv_TakeActionExecuteActionError = "A problem has occurred executing the action; please contact your Administrator.";

// # Note to translators, this string is used under a context menu item where no items are available so we show this disabled menu item instead
var L_bobj_crv_TakeActionNoActionsMenuLabel = "(No actions available)";

var L_bobj_crv_API_InvalidArray = "\'{0}\' is of type Array";
var L_bobj_crv_API_InvalidBoolean = "\'{0}\' is of type Boolean";
var L_bobj_crv_API_InvalidPageNumber = "\'pageNumber\' is of type Integer and greater than 0";
var L_bobj_crv_API_InstantiationFailed = "New instance of {0} must be created.";
var L_bobj_crv_API_InvalidParamValue = "The value of parameter field \'{0}\' is not valid.";
var L_bobj_crv_API_InvalidParamType = "The type of parameter field \'{0}\' is not valid. Please select a valid type from SAP.CR.Parameter.DataTypes";
var L_bobj_crv_API_InvalidParamName = "The name of the parameter field must be a string of length greater than 0.";

var L_bobj_crv_API_InvalidNumOfArguments = "{0} requires {1} argument(s).";
var L_bobj_crv_API_InvalidValueType = "Expected value type for this parameter is \'{0}\'.";
var L_bobj_crv_API_InvalidCanvasListener= "Listener must be an instance of SAP.CR.Viewer.CanvasListener";
var L_bobj_crv_API_InvalidActionListener= "Listener must be an instance of SAP.CR.Viewer.ActionListener";
var L_bobj_crv_API_ValueTypeUndefined= "The paramType property of the parameter field is not valid. Only values from SAP.CR.Parameter.DataTypes.* are supported.";
var L_bobj_crv_API_InvalidRangeBound = "The upper bound and lower bound can only use values from SAP.CR.Parameter.RangeBoundTypes";
var L_bobj_crv_API_InvalidBeginValue = "The begin value is not valid";
var L_bobj_crv_API_InvalidEndValue = "The end value is not valid";
var L_bobj_crv_API_RangeBeginValueGreaterThanEndValue = "The begin value must be less than or equal to the end value";
var L_bobj_crv_API_InvalidLowerAndUpperRangeBound = "The value of both lower bound and upper bound cannot be equal to SAP.CR.Parameter.DataTypes.UNBOUNDED";
var L_bobj_crv_API_InvalidBeginAndEndValue = "The begin and end value cannot be equal.";
var L_bobj_crv_API_InvalidColor = "The color \'{0}\' is not valid.";
var L_bobj_crv_API_InvalidResolution = "The resolution {0} is not valid";
var L_bobj_crv_API_InvalidEventName = "\'{0}\' is not a valid event. Please choose an event from {1}";
var L_bobj_crv_API_InvalidViewerName = "The viewer name \'{0}\' is malformed. It must begin with a letter and may be followed by any number of letters or digits.";
var L_bobj_crv_API_ViewerInitFailed = "Failed to initialize viewer";
var L_bobj_crv_API_ViewerLoadFailed = "Failed to load viewer";

var L_bobj_crv_API_InvalidPrintMode = "\'{0}\' is not a valid print mode. Please choose a print mode from {1}";
var L_bobj_crv_API_InvalidReportMode = "\'{0}\' is not a valid report mode. Please choose a report mode from {1}";
var L_bobj_crv_API_InvalidZoom = "\'{0}\' is not valid a zoom level";
var L_bobj_crv_API_InvalidFunction = "First argument should be a function";
var L_bobj_crv_API_InvalidFunctionAfterInit = "\'{0}\' cannot be called after viewer has been initialized.";
