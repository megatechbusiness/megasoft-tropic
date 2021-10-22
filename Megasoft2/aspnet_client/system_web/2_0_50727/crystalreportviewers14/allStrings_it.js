/* Copyright (c) Business Objects 2006. All rights reserved. */

var L_bobj_crv_MainReport = "Report principale";
// Viewer Toolbar tooltips
var L_bobj_crv_Back = "Indietro";
var L_bobj_crv_Forward = "Avanti";
var L_bobj_crv_Page = "Pagina";
var L_bobj_crv_PageNum = "Pagina {0}";
var L_bobj_crv_FirstPage = "Vai alla prima pagina";
var L_bobj_crv_PrevPage = "Vai alla pagina precedente";
var L_bobj_crv_NextPage = "Vai alla pagina successiva";
var L_bobj_crv_LastPage = "Vai all\'ultima pagina";
var L_bobj_crv_InvalidPageNumber = "Specificare un numero di pagina maggiore di 0.";
var L_bobj_crv_ParamPanel = "Pannello prompt";
var L_bobj_crv_Parameters = "Parametri";
var L_bobj_crv_GroupTree = "Struttura dei gruppi";
var L_bobj_crv_Search = "Trova";
var L_bobj_crv_SearchResults = "Trova risultati";
var L_bobj_crv_DrillUp = "Drill Up";
var L_bobj_crv_DrillTo = "Drill su %1";

var L_bobj_crv_Refresh = "Aggiorna report";
var L_bobj_crv_Zoom = "Zoom";
var L_bobj_crv_PageNav = "Navigazione pagine";
var L_bobj_crv_SelectPage = "Vai alla pagina";
var L_bobj_crv_SearchText = "Trova";
var L_bobj_crv_SearchOptions = "Trova opzioni";
var L_bobj_crv_Export = "Esporta questo report";
var L_bobj_crv_Print = "Stampa questo report";
var L_bobj_crv_History = "Cronologia";
var L_bobj_crv_ClearHistory = "Cancella cronologia";
var L_bobj_crv_TabList = "Elenco tabulazioni";
var L_bobj_crv_Minimize = "Riduci";
var L_bobj_crv_Logo=  "Logo Business Objects";
var L_bobj_crv_FileMenu = "Menu File";
var L_bobj_crv_ResultsFound = "{0} risultati trovati";
var L_bobj_crv_ResultsFoundInSubreport = "{0} risultati trovati in {1}";
var L_bobj_crv_MatchWholeWordsOnly = "Solo parole intere";
var L_bobj_crv_MatchCase = "Maiuscole/minuscole";
var L_bobj_crv_FindMore = "Trova altro";
var L_bobj_crv_Completed = "Completato";
var L_bobj_crv_Breadcrumb= "Barra di navigazione";
var L_bobj_crv_File = "File";

var L_bobj_crv_Show = "Mostra";
var L_bobj_crv_Hide = "Nascondi";

var L_bobj_crv_Find = "Trova...";
var L_bobj_crv_of = "%1 di %2"; // Example: Page "1 of 3"

var L_bobj_crv_submitBtnLbl = "Esporta";
var L_bobj_crv_ActiveXPrintDialogTitle = "Stampa";
var L_bobj_crv_PDFPrintDialogTitle = "Stampa su PDF";
var L_bobj_crv_PrintRangeLbl = "Intervallo di pagine:";
var L_bobj_crv_PrintAllLbl = "Tutte le pagine";
var L_bobj_crv_PrintPagesLbl = "Seleziona pagine";
var L_bobj_crv_PrintFromLbl = "Da:";
var L_bobj_crv_PrintToLbl = "A:";
var L_bobj_crv_PrintInfoTitle = "Stampa su PDF:";
var L_bobj_crv_PrintInfo1 = 'Il visualizzatore deve eseguire l\'esportazione in PDF per la stampa. Scegliere l\'opzione Stampa dal lettore PDF una volta aperto il documento.';
var L_bobj_crv_PrintInfo2 = 'Nota: \u00E8 necessario disporre di un lettore PDF per la stampa, ad esempio Adobe Reader';
var L_bobj_crv_PrintPageRangeError = "Immettere un intervallo di pagine valido.";

var L_bobj_crv_ExportBtnLbl = "Esporta";
var L_bobj_crv_ExportDialogTitle = "Esporta";
var L_bobj_crv_ExportFormatLbl = "Formato file:";
var L_bobj_crv_ExportInfoTitle = "Per esportare:";

var L_bobj_crv_ParamsApply = "Applica";
var L_bobj_crv_ParamsAdvDlg = "Modifica valore parametro";
var L_bobj_crv_ParamsDeleteTooltip = "Elimina valore parametro";
var L_bobj_crv_ParamsAddValue = "Fai clic per aggiungere...";
var L_bobj_crv_ParamsApplyTip = "Applica valori dei parametri";
var L_bobj_crv_ParamsDlgTitle = "Inserisci valori";
var L_bobj_crv_ParamsCalBtn = "Calendario";
var L_bobj_crv_Reset= "Reimposta";
var L_bobj_crv_ResetTip = "Reimposta valori dei parametri";
var L_bobj_crv_ParamsDirtyTip = "Il valore del parametro \u00E8 cambiato. Fare clic sul pulsante Applica per applicare le modifiche.";
var L_bobj_crv_ParamsDataTip = "Parametro di recupero dati";
var L_bobj_crv_ParamsMaxNumDefaultValues = "Fare clic qui per visualizzare altri elementi...";
var L_bobj_crv_paramsOpenAdvance = "Pulsante prompt avanzato per \'%1\'";

var L_bobj_crv_ParamsInvalidTitle = "Il valore del parametro non \u00E8 valido";
var L_bobj_crv_ParamsTooLong = "Il valore del parametro non pu\u00F2 superare i %1 caratteri di lunghezza";
var L_bobj_crv_ParamsTooShort = "Il valore del parametro deve essere dato da almeno %1 caratteri";
var L_bobj_crv_ParamsBadNumber = "Questo parametro \u00E8 di tipo \"Numero\" e pu\u00F2 contenere solo un segno meno, cifre (\"0-9\"), simboli di raggruppamento cifre o un simbolo decimale.";
var L_bobj_crv_ParamsBadCurrency = "Questo parametro \u00E8 di tipo \"Valuta\" e pu\u00F2 contenere solo un segno meno, cifre (\"0-9\"), simboli di raggruppamento cifre o un simbolo decimale.";
var L_bobj_crv_ParamsBadDate = "Questo parametro \u00E8 di tipo \"Data\" e il formato corretto \u00E8 \"%1\" dove \"aaaa\" \u00E8 l\'anno a quattro cifre, \"mm\" \u00E8 il mese (ad esempio Gennaio = 1), e \"gg\" \u00E8 il giorno del mese.";
var L_bobj_crv_ParamsBadTime = "Questo parametro \u00E8 di tipo \"Ora\" e il formato corretto \u00E8 \"hh,mm,ss\" dove \"hh\" \u00E8 l\'ora in formato a 24 ore, \"mm\" sono i minuti e \"ss\" sono i secondi.";
var L_bobj_crv_ParamsBadDateTime = "Questo parametro \u00E8 di tipo \"DataOra\" e il formato corretto \u00E8 \"%1 hh:mm:ss\". \"aaaa\" \u00E8 l\'anno a quattro cifre, \"mm\" \u00E8 il mese (ad esempio Gennaio = 1), \"gg\" \u00E8 il giorno del mese, \"hh\" \u00E8 l\'ora in formato a 24 ore, \"mm\" sono i minuti e \"ss\" sono i secondi.";
var L_bobj_crv_ParamsMinTooltip = "Specificare un valore %1 maggiore o uguale a %2.";
var L_bobj_crv_ParamsMaxTooltip = "Specificare un valore %1 minore o uguale a %2.";
var L_bobj_crv_ParamsMinAndMaxTooltip = "Specificare un valore %1 compreso tra %2 e %3.";
var L_bobj_crv_ParamsStringMinOrMaxTooltip = "La lunghezza %1 per questo campo \u00E8 %2.";
var L_bobj_crv_ParamsStringMinAndMaxTooltip = "La lunghezza del valore deve essere compresa tra %1 e %2 caratteri.";
var L_bobj_crv_ParamsYearToken = "aaaa";
var L_bobj_crv_ParamsMonthToken = "mm";
var L_bobj_crv_ParamsDayToken = "gg";
var L_bobj_crv_ParamsReadOnly = "Questo parametro \u00E8 di tipo \"Solo lettura\".";
var L_bobj_crv_ParamsNoValue = "Nessun valore";
var L_bobj_crv_ParamsDuplicateValue = "Valori duplicati non consentiti.";
var L_bobj_crv_ParamsEnterOptional = "Immettere %1 (facoltativo)";
var L_bobj_crv_ParamsNoneSelected= "(nessuna selezione)";
var L_bobj_crv_ParamsClearValues= "Cancella valori";
var L_bobj_crv_ParamsMoreValues= "%1 altri valori...";
var L_bobj_crv_ParamsMoreValue= "%1 altro valore...";
var L_bobj_crv_Error = "Errore";
var L_bobj_crv_OK = "OK";
var L_bobj_crv_Cancel = "Annulla";
var L_bobj_crv_showDetails = "Mostra dettagli";
var L_bobj_crv_hideDetails = "Nascondi dettagli";
var L_bobj_crv_RequestError = "Impossibile elaborare la richiesta";
var L_bobj_crv_ServletMissing = "Il visualizzatore non \u00E8 in grado di connettersi a CrystalReportViewerServlet che gestisce le richieste asincrone.\nAssicurarsi che Servlet e Servlet-Mapping siano stati dichiarati correttamente nel file web.xml dell\'applicazione.";
var L_bobj_crv_FlashRequired = "Questo contenuto richiede Adobe Flash Player 11 o superiore. {0}Fare clic qui per installarlo.";
var L_bobj_crv_ReadOnlyInPanel= "Questo parametro non \u00E8 modificabile nel pannello. Aprire la finestra di dialogo dei prompt avanzati per modificarne il valore";

var L_bobj_crv_Tree_Drilldown_Node = "Esegui drilldown nodo %1";

var L_bobj_crv_ReportProcessingMessage = "Attendere il completamento dell\'elaborazione del documento.";
var L_bobj_crv_PrintControlProcessingMessage = "Attendere il caricamento del controllo di stampa di Crystal Reports.";

var L_bobj_crv_SundayShort = "D";
var L_bobj_crv_MondayShort = "L";
var L_bobj_crv_TuesdayShort = "M";
var L_bobj_crv_WednesdayShort = "M";
var L_bobj_crv_ThursdayShort = "G";
var L_bobj_crv_FridayShort = "V";
var L_bobj_crv_SaturdayShort = "S";

var L_bobj_crv_Minimum = "minimo";
var L_bobj_crv_Maximum = "massimo";

var L_bobj_crv_Date = "Data";
var L_bobj_crv_Time = "Ora";
var L_bobj_crv_DateTime = "DateTime";
var L_bobj_crv_Boolean = "Booleano";
var L_bobj_crv_Number = "Numero";
var L_bobj_crv_Text = "Testo";

var L_bobj_crv_InteractiveParam_NoAjax = "Il browser in uso non \u00E8 configurato per mostrare il riquadro del parametro.";
var L_bobj_crv_AdvancedDialog_NoAjax= "Il visualizzatore non \u00E8 in grado di aprire una finestra di dialogo dei prompt avanzati.";

var L_bobj_crv_EnableAjax= "Contattare l\'amministratore per abilitare le richieste asincrone.";

var L_bobj_crv_LastRefreshed = "Ultimo aggiornamento: {0}";

var L_bobj_crv_Collapse = "Comprimi";

var L_bobj_crv_CatalystTip = "Risorse in linea";
var L_bobj_crv_LoadingPage = "Caricamento pagina {0} in corso...";
var L_bobj_crv_TakeActionMenuTip = "Seleziona un\'azione di associazione";
var L_bobj_crv_TakeActionExecuteActionError = "Si \u00E8 verificato un problema durante l\'esecuzione dell\'azione; contattare l\'amministratore.";

// # Note to translators, this string is used under a context menu item where no items are available so we show this disabled menu item instead
var L_bobj_crv_TakeActionNoActionsMenuLabel = "(Nessuna azione disponibile)";

var L_bobj_crv_API_InvalidArray = "\'{0}\' \u00E8 di tipo Array";
var L_bobj_crv_API_InvalidBoolean = "\'{0}\' \u00E8 di tipo Boolean";
var L_bobj_crv_API_InvalidPageNumber = "\'pageNumber\' \u00E8 di tipo Integer e maggiore di 0";
var L_bobj_crv_API_InstantiationFailed = "\u00C8 necessario creare una nuova istanza di {0}.";
var L_bobj_crv_API_InvalidParamValue = "Il valore del campo parametro \'{0}\' non \u00E8 valido.";
var L_bobj_crv_API_InvalidParamType = "Il tipo di campo parametro \'{0}\' non \u00E8 valido. Selezionare un tipo valido da SAP.CR.Parameter.DataTypes";
var L_bobj_crv_API_InvalidParamName = "Il nome del campo parametro deve essere una stringa di lunghezza superiore a 0.";

var L_bobj_crv_API_InvalidNumOfArguments = "{0} richiede {1} argomento/i.";
var L_bobj_crv_API_InvalidValueType = "Il tipo di valore previsto per questo parametro \u00E8 \'{0}\'.";
var L_bobj_crv_API_InvalidCanvasListener= "Il listener deve essere un\'istanza di SAP.CR.Viewer.CanvasListener";
var L_bobj_crv_API_InvalidActionListener= "Il listener deve essere un\'istanza di SAP.CR.Viewer.ActionListener";
var L_bobj_crv_API_ValueTypeUndefined= "La propriet\u00E0 paramType del campo parametro non \u00E8 valido. Sono supportati solo i valori da SAP.CR.Parameter.DataTypes.*.";
var L_bobj_crv_API_InvalidRangeBound = "Il limite superiore e il limite inferiore possono utilizzare solo valori da SAP.CR.Parameter.RangeBoundTypes";
var L_bobj_crv_API_InvalidBeginValue = "Il valore iniziale non \u00E8 valido";
var L_bobj_crv_API_InvalidEndValue = "Il valore finale non \u00E8 valido";
var L_bobj_crv_API_RangeBeginValueGreaterThanEndValue = "Il valore iniziale deve essere inferiore o uguale al valore finale";
var L_bobj_crv_API_InvalidLowerAndUpperRangeBound = "I valori per il limite inferiore e il limite superiore non possono essere uguali a SAP.CR.Parameter.DataTypes.UNBOUNDED";
var L_bobj_crv_API_InvalidBeginAndEndValue = "I valori di inizio e di fine non possono essere uguali.";
var L_bobj_crv_API_InvalidColor = "Il colore \'{0}\' non \u00E8 valido.";
var L_bobj_crv_API_InvalidResolution = "La risoluzione {0} non \u00E8 valida";
var L_bobj_crv_API_InvalidEventName = "\'{0}\' non \u00E8 un evento valido. Scegliere un evento da {1}";
var L_bobj_crv_API_InvalidViewerName = "Formato non corretto del nome del visualizzatore \'{0}\'. Deve iniziare con una lettera e pu\u00F2 contenere un numero qualsiasi di lettere o cifre.";
var L_bobj_crv_API_ViewerInitFailed = "Impossibile inizializzare il visualizzatore";
var L_bobj_crv_API_ViewerLoadFailed = "Impossibile caricare il visualizzatore";

var L_bobj_crv_API_InvalidPrintMode = "\'{0}\' non \u00E8 una modalit\u00E0 di stampa valida. Scegliere una modalit\u00E0 di stampa da {1}";
var L_bobj_crv_API_InvalidReportMode = "\'{0}\' non \u00E8 una modalit\u00E0 report valida. Scegliere una modalit\u00E0 report da {1}";
var L_bobj_crv_API_InvalidZoom = "\'{0}\' non \u00E8 un livello di zoom valido";
var L_bobj_crv_API_InvalidFunction = "Il primo argomento deve essere una funzione";
var L_bobj_crv_API_InvalidFunctionAfterInit = "Impossibile chiamare \'{0}\' dopo l\'inizializzazione del visualizzatore.";
// <script>
/*
=============================================================
WebIntelligence(r) Report Panel
Copyright(c) 2001-2003 Business Objects S.A.
All rights reserved

Use and support of this software is governed by the terms
and conditions of the software license agreement and support
policy of Business Objects S.A. and/or its subsidiaries. 
The Business Objects products and technology are protected
by the US patent number 5,555,403 and 6,247,008

File: labels.js


=============================================================
*/

L_DHTMLLIB_month=new Array();

L_DHTMLLIB_month[0]="GENNAIO";
L_DHTMLLIB_month[1]="FEBBRAIO";
L_DHTMLLIB_month[2]="MARZO";
L_DHTMLLIB_month[3]="APRILE";
L_DHTMLLIB_month[4]="MAGGIO";
L_DHTMLLIB_month[5]="GIUGNO";
L_DHTMLLIB_month[6]="LUGLIO";
L_DHTMLLIB_month[7]="AGOSTO";
L_DHTMLLIB_month[8]="SETTEMBRE";
L_DHTMLLIB_month[9]="OTTOBRE";
L_DHTMLLIB_month[10]="NOVEMBRE";
L_DHTMLLIB_month[11]="DICEMBRE";

L_DHTMLLIB_closeDialog="Chiudi finestra";

L_DHTMLLIB_openMenu="Fare clic qui per accedere alle opzioni di {0}";

L_DHTMLLIB_expandedLab="Espanso";
L_DHTMLLIB_collapsedLab="Compresso";
L_DHTMLLIB_selectedLab="Selezionati";

L_DHTMLLIB_calendarNextMonthLab="Vai al mese successivo";
L_DHTMLLIB_calendarPrevMonthLab="Vai al mese precedente";
L_DHTMLLIB_calendarNextYearLab="Vai all\'anno successivo";
L_DHTMLLIB_calendarPrevYearLab="Vai all\'anno precedente";
L_DHTMLLIB_calendarSelectionLab="Giorno selezionato";

L_DHTMLLIB_menuCheckLab="Selezionato";
L_DHTMLLIB_menuDisableLab="Disattivato";
	
L_DHTMLLIB_level="Livello";
L_DHTMLLIB_closeTab="Chiudi scheda";
L_DHTMLLIB_of=" di ";

L_DHTMLLIB_helpLab="?";

L_DHTMLLIB_cancelButtonLab="Annulla";

L_DHTMLLIB__modifiers= new Array();
L_DHTMLLIB__modifiers[0]="CTRL+";
L_DHTMLLIB__modifiers[1]="MAIUSC+";
L_DHTMLLIB__modifiers[2]="ALT+";
/* Copyright (c) Business Objects 2006. All rights reserved. */

// LOCALIZATION STRING

// Strings for calendar.js and calendar_param.js
var L_Today     = "Oggi";
var L_January   = "Gennaio";
var L_February  = "Febbraio";
var L_March     = "Marzo";
var L_April     = "Aprile";
var L_May       = "Maggio";
var L_June      = "Giugno";
var L_July      = "Luglio";
var L_August    = "Agosto";
var L_September = "Settembre";
var L_October   = "Ottobre";
var L_November  = "Novembre";
var L_December  = "Dicembre";
var L_Su        = "Do";
var L_Mo        = "Lu";
var L_Tu        = "Ma";
var L_We        = "Me";
var L_Th        = "Gi";
var L_Fr        = "Ve";
var L_Sa        = "Sa";

// Strings for prompts.js and prompts_param.js
var L_YYYY          = "aaaa";
var L_MM            = "mm";
var L_DD            = "gg";
var L_BadNumber     = "Questo parametro \u00E8 di tipo \"Numero\" e pu\u00F2 contenere solo un simbolo negativo, cifre (\"0-9\"), simboli di raggruppamento cifre o un simbolo decimale. Correggere il valore del parametro immesso.";
var L_BadCurrency   = "Questo parametro \u00E8 di tipo \"Valuta\" e pu\u00F2 contenere solo un segno meno, cifre (\"0-9\"), simboli di raggruppamento cifre o un simbolo decimale. Correggere il valore del parametro immesso.";
var L_BadDate       = "Questo parametro \u00E8 di tipo \"Data\" e deve essere nel formato \"%1\" dove \"aaaa\" \u00E8 l\'anno a quattro cifre, \"mm\" \u00E8 il mese (ad esempio Gennaio = 1), e \"gg\" \u00E8 il giorno del mese.";
var L_BadDateTime   = "Questo parametro \u00E8 di tipo \"DataOra\" e il formato corretto \u00E8 \"%1 hh:mm:ss\". \"aaaa\" \u00E8 l\'anno a quattro cifre, \"mm\" \u00E8 il mese (ad esempio Gennaio = 1), \"gg\" \u00E8 il giorno del mese, \"hh\" \u00E8 l\'ora in formato a 24 ore, \"mm\" sono i minuti e \"ss\" sono i secondi.";
var L_BadTime       = "Questo parametro \u00E8 di tipo \"Ora\" e deve essere nel formato \"hh,mm,ss\" dove \"hh\" \u00E8 l\'ora in formato a 24 ore, \"mm\" sono i minuti e \"ss\" sono i secondi.";
var L_NoValue       = "Nessun valore";
var L_BadValue      = "Per impostare \"Nessun valore\", \u00E8 necessario impostare i valori Da e A su \"Nessun valore\".";
var L_BadBound      = "Impossibile impostare \"Nessun limite inferiore\" insieme a \"Nessun limite superiore\".";
var L_NoValueAlready = "Questo parametro \u00E8 gi\u00E0 impostato su \"Nessun valore\". Rimuovere \"Nessun valore\" prima di aggiungere altri valori.";
var L_RangeError    = "L\'inizio dell\'intervallo non pu\u00F2 essere maggiore della fine dell\'intervallo.";
var L_NoDateEntered = "\u00C8 necessario immettere una data.";
var L_Empty         = "Immettere un valore.";

// Strings for filter dialog
var L_closeDialog="Chiudi finestra";

var L_SetFilter = "Imposta filtro";
var L_OK        = "OK";
var L_Cancel    = "Annulla";

 /* Crystal Decisions Confidential Proprietary Information */
