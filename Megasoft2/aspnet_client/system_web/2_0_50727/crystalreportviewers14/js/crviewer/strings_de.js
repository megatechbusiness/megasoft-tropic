/* Copyright (c) Business Objects 2006. All rights reserved. */

var L_bobj_crv_MainReport = "Hauptbericht";
// Viewer Toolbar tooltips
var L_bobj_crv_Back = "Zur\u00FCck";
var L_bobj_crv_Forward = "Vorw\u00E4rts";
var L_bobj_crv_Page = "Seite";
var L_bobj_crv_PageNum = "Seite {0}";
var L_bobj_crv_FirstPage = "Zur ersten Seite";
var L_bobj_crv_PrevPage = "Zur vorherigen Seite";
var L_bobj_crv_NextPage = "Zur n\u00E4chsten Seite";
var L_bobj_crv_LastPage = "Zur letzten Seite";
var L_bobj_crv_InvalidPageNumber = "Geben Sie eine Seitenzahl gr\u00F6\u00DFer als 0 an.";
var L_bobj_crv_ParamPanel = "Eingabeaufforderungsfenster";
var L_bobj_crv_Parameters = "Parameter";
var L_bobj_crv_GroupTree = "Gruppenstruktur";
var L_bobj_crv_Search = "Suchen";
var L_bobj_crv_SearchResults = "Suchergebnisse";
var L_bobj_crv_DrillUp = "Drillup";
var L_bobj_crv_DrillTo = "Drill auf %1";

var L_bobj_crv_Refresh = "Bericht regenerieren";
var L_bobj_crv_Zoom = "Zoom";
var L_bobj_crv_PageNav = "Seitennavigation";
var L_bobj_crv_SelectPage = "Gehe zu Seite";
var L_bobj_crv_SearchText = "Suchen";
var L_bobj_crv_SearchOptions = "Suchoptionen";
var L_bobj_crv_Export = "Diesen Bericht exportieren";
var L_bobj_crv_Print = "Diesen Bericht drucken";
var L_bobj_crv_History = "Verlauf";
var L_bobj_crv_ClearHistory = "Verlauf l\u00F6schen";
var L_bobj_crv_TabList = "Tabulatorliste";
var L_bobj_crv_Minimize = "Minimiert";
var L_bobj_crv_Logo=  "Business Objects-Logo";
var L_bobj_crv_FileMenu = "Men\u00FC \"Datei\"";
var L_bobj_crv_ResultsFound = "{0} Ergebnisse gefunden";
var L_bobj_crv_ResultsFoundInSubreport = "{0} Ergebnisse gefunden in {1}";
var L_bobj_crv_MatchWholeWordsOnly = "Nur ganzes Wort";
var L_bobj_crv_MatchCase = "Gro\u00DF-/Kleinschreibung beachten";
var L_bobj_crv_FindMore = "Weiter suchen";
var L_bobj_crv_Completed = "Abgeschlossen";
var L_bobj_crv_Breadcrumb= "Klickpfad";
var L_bobj_crv_File = "Datei";

var L_bobj_crv_Show = "Anzeigen";
var L_bobj_crv_Hide = "Ausblenden";

var L_bobj_crv_Find = "Suchen...";
var L_bobj_crv_of = "%1 von %2"; // Example: Page "1 of 3"

var L_bobj_crv_submitBtnLbl = "Exportieren";
var L_bobj_crv_ActiveXPrintDialogTitle = "Drucken";
var L_bobj_crv_PDFPrintDialogTitle = "Als PDF drucken";
var L_bobj_crv_PrintRangeLbl = "Seitenbereich:";
var L_bobj_crv_PrintAllLbl = "Alle Seiten";
var L_bobj_crv_PrintPagesLbl = "Seiten ausw\u00E4hlen";
var L_bobj_crv_PrintFromLbl = "Von:";
var L_bobj_crv_PrintToLbl = "Bis:";
var L_bobj_crv_PrintInfoTitle = "Als PDF drucken:";
var L_bobj_crv_PrintInfo1 = 'Der Viewer muss f\u00FCr den Druck einen Export in PDF ausf\u00FChren. W\u00E4hlen Sie in der PDF Reader-Anwendung die Druckoption aus, nachdem das Dokument ge\u00F6ffnet wurde.';
var L_bobj_crv_PrintInfo2 = 'Hinweis: F\u00FCr den Druck muss ein PDF Reader installiert sein. (z.B. Adobe Reader)';
var L_bobj_crv_PrintPageRangeError = "Geben Sie einen g\u00FCltigen Seitenbereich ein.";

var L_bobj_crv_ExportBtnLbl = "Exportieren";
var L_bobj_crv_ExportDialogTitle = "Exportieren";
var L_bobj_crv_ExportFormatLbl = "Dateiformat:";
var L_bobj_crv_ExportInfoTitle = "Zu exportieren:";

var L_bobj_crv_ParamsApply = "Anwenden";
var L_bobj_crv_ParamsAdvDlg = "Parameterwert bearbeiten";
var L_bobj_crv_ParamsDeleteTooltip = "Parameterwert l\u00F6schen";
var L_bobj_crv_ParamsAddValue = "Zum Hinzuf\u00FCgen klicken...";
var L_bobj_crv_ParamsApplyTip = "Parameterwerte anwenden";
var L_bobj_crv_ParamsDlgTitle = "Werte eingeben";
var L_bobj_crv_ParamsCalBtn = "Kalender";
var L_bobj_crv_Reset= "Zur\u00FCcksetzen";
var L_bobj_crv_ResetTip = "Parameterwerte zur\u00FCcksetzen";
var L_bobj_crv_ParamsDirtyTip = "Der Parameterwert hat sich ge\u00E4ndert. Klicken Sie auf die Schaltfl\u00E4che \"Anwenden\", um die \u00C4nderungen zu speichern.";
var L_bobj_crv_ParamsDataTip = "Dies ist ein Parameter zum Abrufen von Daten";
var L_bobj_crv_ParamsMaxNumDefaultValues = "Klicken Sie hier, um weitere Elemente zu erhalten...";
var L_bobj_crv_paramsOpenAdvance = "Erweiterte Eingabeaufforderungs-Schaltfl\u00E4che f\u00FCr \'%1\'";

var L_bobj_crv_ParamsInvalidTitle = "Der Parameterwert ist ung\u00FCltig.";
var L_bobj_crv_ParamsTooLong = "Parameterwert darf nicht mehr als %1 Zeichen lang sein";
var L_bobj_crv_ParamsTooShort = "Parameterwert muss mindestens %1 Zeichen lang sein";
var L_bobj_crv_ParamsBadNumber = "Dies ist ein Parameter vom Typ \"Zahl\", der nur ein vorangestelltes Minuszeichen, Ziffern (\"0-9\"), Zeichen zur Zifferngruppierung oder ein Dezimalsymbol enthalten darf.";
var L_bobj_crv_ParamsBadCurrency = "Dies ist ein Parameter vom Typ \"W\u00E4hrung\", der nur ein vorangestelltes Minuszeichen, Ziffern (\"0-9\"), Zeichen zur Zifferngruppierung oder ein Dezimalsymbol enthalten darf.";
var L_bobj_crv_ParamsBadDate = "Dies ist ein Parameter vom Typ \"Datum\", der das Format \"%1\" aufweisen sollte, wobei \"yyyy\" eine vierstellige Jahreszahl, \"mm\" der Monat (z. B. Januar = 1) und \"dd\" der Tag des angegebenen Monats ist.";
var L_bobj_crv_ParamsBadTime = "Dies ist ein Parameter vom Typ \"Uhrzeit\", der das Format \"hh:mm:ss\"  aufweisen sollte, wobei \"hh\" die Stunden im 24-Stunden-Format darstellt, \"mm\" die Minuten und \"ss\" die Sekunden.";
var L_bobj_crv_ParamsBadDateTime = "Dies ist ein Parameter vom Typ \"DatumUhrzeit\", der das Format \"%1 hh:mm:ss\" aufweisen sollte, wobei \"yyyy\" eine vierstellige Jahreszahl, \"mm\" den Monat (z. B. Januar = 1), \"dd\" den Tag, \"hh\" die Stunden im 24-Stunden-Format, \"mm\" die Minuten und \"ss\" die Sekunden darstellt.";
var L_bobj_crv_ParamsMinTooltip = "Geben Sie einen %1-Wert gr\u00F6\u00DFer oder gleich %2 ein.";
var L_bobj_crv_ParamsMaxTooltip = "Geben Sie einen %1-Wert kleiner oder gleich %2 ein.";
var L_bobj_crv_ParamsMinAndMaxTooltip = "Geben Sie einen %1-Wert zwischen %2 und %3 ein.";
var L_bobj_crv_ParamsStringMinOrMaxTooltip = "Die %1-L\u00E4nge f\u00FCr dieses Feld betr\u00E4gt %2.";
var L_bobj_crv_ParamsStringMinAndMaxTooltip = "Der Wert muss zwischen %1 und %2 Zeichen enthalten.";
var L_bobj_crv_ParamsYearToken = "yyyy";
var L_bobj_crv_ParamsMonthToken = "mm";
var L_bobj_crv_ParamsDayToken = "dd";
var L_bobj_crv_ParamsReadOnly = "Dies ist ein Parameter vom Typ \"Schreibgesch\u00FCtzt\".";
var L_bobj_crv_ParamsNoValue = "Kein Wert";
var L_bobj_crv_ParamsDuplicateValue = "Doppelte Werte sind nicht zul\u00E4ssig.";
var L_bobj_crv_ParamsEnterOptional = "Geben Sie %1 ein (optional)";
var L_bobj_crv_ParamsNoneSelected= "(Keine ausgew\u00E4hlt)";
var L_bobj_crv_ParamsClearValues= "Werte l\u00F6schen";
var L_bobj_crv_ParamsMoreValues= "%1 mehr Werte...";
var L_bobj_crv_ParamsMoreValue= "%1 mehr Wert...";
var L_bobj_crv_Error = "Fehler";
var L_bobj_crv_OK = "OK";
var L_bobj_crv_Cancel = "Abbrechen";
var L_bobj_crv_showDetails = "Details anzeigen";
var L_bobj_crv_hideDetails = "Details ausblenden";
var L_bobj_crv_RequestError = "Anforderung kann nicht verarbeitet werden";
var L_bobj_crv_ServletMissing = "Der Viewer kann keine Verbindung zum CrystalReportViewerServlet herstellen, durch das asynchrone Anforderungen verarbeitet werden.\nStellen Sie sicher, dass Servlet und Servlet-Zuordnung in der Anwendungsdatei \"web.xml\" ordnungsgem\u00E4\u00DF deklariert wurden.";
var L_bobj_crv_FlashRequired = "F\u00FCr diesen Inhalt wird Adobe Flash Player 11 oder h\u00F6her ben\u00F6tigt. {0}Klicken Sie hier, um das Programm zu installieren.";
var L_bobj_crv_ReadOnlyInPanel= "Dieser Parameter kann nicht in dem Fenster bearbeitet werden. \u00D6ffnen Sie das Dialogfeld \"Erweiterte Eingabeaufforderung\", um den Parameterwert zu bearbeiten.";

var L_bobj_crv_Tree_Drilldown_Node = "Drilldown auf Knoten %1 ausf\u00FChren";

var L_bobj_crv_ReportProcessingMessage = "Bitte warten: Dokument wird verarbeitet.";
var L_bobj_crv_PrintControlProcessingMessage = "Bitte warten: Die Crystal Reports-Drucksteuerung wird geladen.";

var L_bobj_crv_SundayShort = "S";
var L_bobj_crv_MondayShort = "M";
var L_bobj_crv_TuesdayShort = "D";
var L_bobj_crv_WednesdayShort = "M";
var L_bobj_crv_ThursdayShort = "D";
var L_bobj_crv_FridayShort = "F";
var L_bobj_crv_SaturdayShort = "S";

var L_bobj_crv_Minimum = "Minimal";
var L_bobj_crv_Maximum = "Maximal";

var L_bobj_crv_Date = "Datum";
var L_bobj_crv_Time = "Uhrzeit";
var L_bobj_crv_DateTime = "DatumUhrzeit";
var L_bobj_crv_Boolean = "Boolesch";
var L_bobj_crv_Number = "Zahl";
var L_bobj_crv_Text = "Text";

var L_bobj_crv_InteractiveParam_NoAjax = "Der von Ihnen verwendete Webbrowser ist nicht f\u00FCr die Anzeige des Parameterbereichs konfiguriert.";
var L_bobj_crv_AdvancedDialog_NoAjax= "Der Viewer kann das Dialogfeld f\u00FCr die erweiterten Eingabeaufforderungen nicht \u00F6ffnen.";

var L_bobj_crv_EnableAjax= "Wenden Sie sich an Ihren Administrator, um asynchrone Anforderungen zu aktivieren.";

var L_bobj_crv_LastRefreshed = "Zuletzt regeneriert : {0}";

var L_bobj_crv_Collapse = "Zuklappen";

var L_bobj_crv_CatalystTip = "Online-Ressourcen";
var L_bobj_crv_LoadingPage = "Seite {0} wird geladen ...";
var L_bobj_crv_TakeActionMenuTip = "W\u00E4hlen Sie eine gebundene Aktion aus";
var L_bobj_crv_TakeActionExecuteActionError = "Beim Durchf\u00FChren der Aktion ist ein Problem aufgetreten; wenden Sie sich an den Administrator.";

// # Note to translators, this string is used under a context menu item where no items are available so we show this disabled menu item instead
var L_bobj_crv_TakeActionNoActionsMenuLabel = "(Keine Aktionen verf\u00FCgbar)";

var L_bobj_crv_API_InvalidArray = "\'{0}\' ist vom Typ Array";
var L_bobj_crv_API_InvalidBoolean = "\'{0}\' ist vom Typ boolescher Wert";
var L_bobj_crv_API_InvalidPageNumber = "\'pageNumber\' ist eine Ganzzahl und gr\u00F6\u00DFer 0";
var L_bobj_crv_API_InstantiationFailed = "Neue Instanz von {0} muss erstellt werden.";
var L_bobj_crv_API_InvalidParamValue = "Der Wert des Parameterfelds \'{0}\' ist ung\u00FCltig.";
var L_bobj_crv_API_InvalidParamType = "Der Typ des Parameterfelds \'{0}\' ist ung\u00FCltig. W\u00E4hlen Sie aus SAP.CR.Parameter.DataTypes einen g\u00FCltigen Typ aus";
var L_bobj_crv_API_InvalidParamName = "Der Name des Parameterfelds muss eine Zeichenfolge mit einer L\u00E4nge gr\u00F6\u00DFer 0 sein.";

var L_bobj_crv_API_InvalidNumOfArguments = "{0} erfordert {1} Argument(e).";
var L_bobj_crv_API_InvalidValueType = "F\u00FCr diesen Parameter wird der Werttyp \'{0}\' erwartet.";
var L_bobj_crv_API_InvalidCanvasListener= "Der Listener muss eine Instanz von SAP.CR.Viewer.CanvasListener sein";
var L_bobj_crv_API_InvalidActionListener= "Der Listener muss eine Instanz von SAP.CR.Viewer.ActionListener sein";
var L_bobj_crv_API_ValueTypeUndefined= "Die Eigenschaft paramType des Parameterfelds ist ung\u00FCltig. Es werden nur Werte von SAP.CR.Parameter.DataTypes.* unterst\u00FCtzt.";
var L_bobj_crv_API_InvalidRangeBound = "Die Obergrenze und die Untergrenze k\u00F6nnen nur Werte von SAP.CR.Parameter.RangeBoundTypes verwenden";
var L_bobj_crv_API_InvalidBeginValue = "Ung\u00FCltiger Anfangswert";
var L_bobj_crv_API_InvalidEndValue = "Ung\u00FCltiger Endwert";
var L_bobj_crv_API_RangeBeginValueGreaterThanEndValue = "Der Anfangswert muss kleiner oder gleich dem Endwert sein";
var L_bobj_crv_API_InvalidLowerAndUpperRangeBound = "Der Wert der Untergrenze und der Obergrenze darf nicht identisch mit SAP.CR.Parameter.DataTypes.UNBOUNDED sein";
var L_bobj_crv_API_InvalidBeginAndEndValue = "Der Anfangs- und Endwert d\u00FCrfen nicht identisch sein.";
var L_bobj_crv_API_InvalidColor = "Die Farbe \'{0}\' ist nicht g\u00FCltig.";
var L_bobj_crv_API_InvalidResolution = "Die Aufl\u00F6sung {0} ist ung\u00FCltig.";
var L_bobj_crv_API_InvalidEventName = "\'{0}\' ist kein g\u00FCltiges Ereignis. W\u00E4hlen Sie ein Ereignis aus {1}";
var L_bobj_crv_API_InvalidViewerName = "Der Viewername \'{0}\' ist fehlerhaft formuliert. Er muss mit einem Buchstaben beginnen, gefolgt von einer beliebigen Anzahl Buchstaben oder Zahlen.";
var L_bobj_crv_API_ViewerInitFailed = "Fehler beim Initialisieren des Viewers";
var L_bobj_crv_API_ViewerLoadFailed = "Fehler beim Laden des Viewers";

var L_bobj_crv_API_InvalidPrintMode = "\'{0}\' ist kein g\u00FCltiger Druckmodus. W\u00E4hlen Sie einen Druckmodus aus {1}";
var L_bobj_crv_API_InvalidReportMode = "\'{0}\' ist kein g\u00FCltiger Berichtsmodus. W\u00E4hlen Sie einen Berichtsmodus aus {1} aus";
var L_bobj_crv_API_InvalidZoom = "\'{0}\' ist keine g\u00FCltige Zoomebene";
var L_bobj_crv_API_InvalidFunction = "Das erste Argument sollte eine Funktion sein";
var L_bobj_crv_API_InvalidFunctionAfterInit = "\'{0}\' kann nicht aufgerufen werden, nachdem der Viewer initialisiert wurde.";
