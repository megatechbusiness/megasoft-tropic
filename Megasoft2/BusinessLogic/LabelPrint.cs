using Megasoft2.Models;

//using Seagull.BarTender.Print;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class LabelPrint
    {
        private SysproCore sys = new SysproCore();
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();

        public string PrintPoLabel(string StockCode, string Lot, string Quantity)
        {
            try
            {
                //using (Engine btEngine = new Engine())
                //{
                //    btEngine.Start();
                //    LabelFormatDocument label = btEngine.Documents.Open(@"‪C:\Development\Freedom\Label\Test.btw");
                //    label.Print();
                //    btEngine.Stop();
                //}
                return "Label Printed";
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }
        }

        public string PostPoReceipt(List<sp_GetPoLabelLines_Result> detail, DateTime DeliveryDate, string DeliveryNote)
        {
            try
            {
                detail = (from a in detail where a.PostFlag == true select a).OrderBy(a => a.Line).ToList();
                if (detail.Count > 0)
                {


                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("This is an example XML instance to demonstrate");
                    Document.Append("use of the Purchase Order Receipts Business Object");
                    Document.Append("-->");
                    Document.Append("<PostPurchaseOrderReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"portordoc.xsd\">");

                    int line = 0;



                    foreach (var item in detail)
                    {

                        string Po = detail.FirstOrDefault().PurchaseOrder;
                        int PoLine = (int)item.Line;


                        var outstanding = wdb.sp_GetOustandingPoQty(Po, PoLine).ToList();
                        var SumRecQty = (from a in detail where a.Line == PoLine && a.PostFlag == true select a.ReelQty).Sum();
                        if (SumRecQty > outstanding.FirstOrDefault().OutstandingQty)
                        {
                            return "Post quantity of " + SumRecQty + " exceeds outstanding quantity of " + outstanding.FirstOrDefault().OutstandingQty + " for PO: " + Po + "/" + PoLine;
                        }


                        var NonMerchList = (from a in wdb.mtPurchaseOrderNonMerchCosts where a.PurchaseOrder == Po && a.Line == PoLine && a.Posted == "N" select a).ToList();

                        int RowCount = 1;
                        int TotalRows = (from a in detail where a.PostFlag == true select a.ReelNumber).Count();
                        int TotalRowsLess1 = 1; //Default TotalRowsLess to 1 in case of situation where only 1 item. This will prevent divide by zero error later on.                    
                        if (TotalRows > 1)
                        {
                            TotalRowsLess1 = TotalRows - 1;
                        }





                        //Get Non Merch By Line. This is to prevent allocating non merch to different lines
                        //var AllNonMerch = wdb.sp_GetNonMerchCostsByPo(detail.FirstOrDefault().PurchaseOrder, (int)item.Line).ToList();

                        if (item.PostFlag == true)
                        {
                            Document.Append("<Item>");
                            Document.Append("<Receipt>");
                            Document.Append("<Journal />");
                            Document.Append("<PurchaseOrder>" + item.PurchaseOrder + "</PurchaseOrder>");
                            Document.Append("<PurchaseOrderLine>" + item.Line + "</PurchaseOrderLine>");
                            Document.Append("<Warehouse />");
                            Document.Append("<Quantity>" + item.ReelQty + "</Quantity>");
                            Document.Append("<UnitOfMeasure />");
                            Document.Append("<Units />");
                            Document.Append("<Pieces />");
                            Document.Append("<DeliveryNote><![CDATA[" + DeliveryNote + "]]></DeliveryNote>");
                            Document.Append("<DeliveryDate><![CDATA[" + DeliveryDate.ToString("yyyy-MM-dd") + "]]></DeliveryDate>");
                            Document.Append("<Cost />");
                            Document.Append("<CostBasis>P</CostBasis>");
                            Document.Append("<SwitchOnGRNMatching>N</SwitchOnGRNMatching>");

                            //Document.Append("<GRNNumber></GRNNumber>");

                            //Document.Append("<Reference>" + StockDesc + "</Reference>");
                            Document.Append("<GRNSource>1</GRNSource>");
                            Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");

                            if (this.CheckItemTraceable(item.PurchaseOrder, item.Line) == true)
                            {
                                Document.Append("<Lot>" + item.ReelNumber + "</Lot>");
                                Document.Append("<LotExpiryDate />");
                                Document.Append("<Certificate />");
                                Document.Append("<Concession />");
                            }
                            else if (this.CheckItemSerial(item.PurchaseOrder, item.Line) == true)
                            {
                                Document.Append("<Serials>");
                                Document.Append("<SerialNumber><![CDATA[" + item.ReelNumber + "]]></SerialNumber>");
                                Document.Append("<SerialQuantity>" + item.ReelQty + "</SerialQuantity>");
                                //Document.Append("<SerialUnits/>");
                                //Document.Append("<SerialPieces/>");
                                //Document.Append("<SerialFifoBucket></SerialFifoBucket>");
                                //Document.Append("<SerialLocation></SerialLocation>");
                                //Document.Append("<SerialExpiryDate>2006-10-31</SerialExpiryDate>");
                                Document.Append("</Serials>");
                            }

                            Document.Append("<PurchaseOrderLineComplete>N</PurchaseOrderLineComplete>");
                            Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
                            Document.Append("<ChangeSalesOrderStatus>N</ChangeSalesOrderStatus>");

                            Document.Append("<CostMultiplier />");
                            Document.Append("<Notation><![CDATA[" + item.ReelNumber + "]]></Notation>");

                            if (item.UseMultipleBins == "Y")
                            {
                                Document.Append("<Bins>");
                                Document.Append("<BinLocation>" + item.Bin + "</BinLocation>");
                                Document.Append("<BinQuantity>" + item.ReelQty + "</BinQuantity>");
                                Document.Append("<BinUnits />");
                                Document.Append("<BinPieces />");
                                Document.Append("</Bins>");
                            }


                            if (NonMerchList.Count > 0)
                            {
                                decimal TotalPerReel = 0;


                                decimal TotalNAmount = 0;
                                foreach (var nm in NonMerchList)
                                {
                                    decimal nmAmount = 0;

                                    //The non merch  cost is divided between the number of reels.
                                    //Due to rounding we allocate the remainder on the last reel.
                                    //If we on the last row then we get the total amount - total amount divided by the total rows less 1.
                                    //Eg. Non Merch Cost = R8000 and we have 12 Reels
                                    //TotalRows = 12
                                    //TotalRowsLess1 = 11
                                    //Each row cost = Amount / TotalRows
                                    //=8000/12
                                    //=666.67
                                    //The Last Row will be:
                                    //666.67 * 11 = 7333.37
                                    //=8000 - 7333.37
                                    //=666.63
                                    //As a validation:
                                    //666.67*11=7333.37 + 666.63 = 8000

                                    if (RowCount == TotalRows)
                                    {
                                        if (TotalRows == 1)//if only distributing to one row
                                        {
                                            nmAmount = (decimal)(Math.Round((decimal)nm.Amount / TotalRows, 2));
                                        }
                                        else
                                        {
                                            nmAmount = (decimal)(nm.Amount) - ((decimal)(Math.Round((decimal)nm.Amount / TotalRows, 2)) * TotalRowsLess1);
                                        }

                                    }
                                    else
                                    {
                                        nmAmount = (decimal)(Math.Round((decimal)nm.Amount / TotalRows, 2));
                                    }

                                    string Ref = nm.Reference;
                                    if (nm.Reference.Length > 9)
                                    {
                                        Ref = nm.Reference.Substring(0, 9);
                                    }
                                    Document.Append("<NonMerchandiseDistribution>");
                                    Document.Append("<NmReference><![CDATA[" + Ref + "]]></NmReference>");
                                    Document.Append("<NmLedgerCode><![CDATA[" + nm.GlCode + "]]></NmLedgerCode>");
                                    Document.Append("<NmSupplier><![CDATA[" + nm.Supplier + "]]></NmSupplier>");
                                    Document.Append("<NmAmount>" + String.Format("{0:#######0.00}", nmAmount) + "</NmAmount>");
                                    Document.Append("</NonMerchandiseDistribution>");

                                    TotalNAmount += nmAmount;
                                }

                                Document.Append("<ApplyCostMultiplier>Y</ApplyCostMultiplier>");
                                Document.Append("<NonMerchandiseCost>" + String.Format("{0:#######0.00}", TotalNAmount) + "</NonMerchandiseCost>");
                            }
                            else
                            {
                                Document.Append("<ApplyCostMultiplier>N</ApplyCostMultiplier>");
                                Document.Append("<NonMerchandiseCost>0</NonMerchandiseCost>");
                            }


                            Document.Append("</Receipt>");
                            Document.Append("</Item>");

                            RowCount++;
                        }
                    }
                    Document.Append("</PostPurchaseOrderReceipts>");

                    string Guid = sys.SysproLogin();
                    string XmlOut = sys.SysproPost(Guid, this.BuildGrnParameter(), Document.ToString(), "PORTOR");
                    sys.SysproLogoff(Guid);
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        //Posted.
                        string sysproGrn = sys.GetFirstXmlValue(XmlOut, "Grn");
                        foreach (var nline in detail)
                        {
                            if (nline.PostFlag == true)
                            {
                                wdb.sp_UpdatePoPostedFlagByReel(nline.PurchaseOrder, (int)nline.Line, nline.ReelNumber, "Y", sysproGrn, HttpContext.Current.User.Identity.Name.ToUpper());
                                wdb.sp_UpdateNonMerchPostedFlag(nline.PurchaseOrder, (int)nline.Line, sysproGrn);
                                //Save to mtLotDetail
                            }
                        }

                        ErrorMessage = "Posted Successfully. Grn : " + sysproGrn;
                    }
                    return ErrorMessage;
                }
                else
                {
                    return "No data found!";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostSysproExpenseIssue(List<sp_GetPoLabelLines_Result> detail)
        {
            try
            {
                bool hasLines = false;
                detail = (from a in detail where a.PostFlag == true select a).OrderBy(a => a.Line).ToList();
                if (detail.Count > 0)
                {
                    string XmlOut, ErrorMessage;
                    string Guid = sys.SysproLogin();
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Inventory Expense Issues Business Object");
                    Document.Append("-->");
                    Document.Append("<PostInvExpenseIssues xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMEDOC.XSD\">");



                    //var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == CostCentre && a.WorkCentre == WorkCentre && a.ProductClass == TraceableType.ProductClass select a.GlCode).FirstOrDefault();


                    foreach (var item in detail)
                    {
                        var PoLine = (from a in wdb.PorMasterDetails where a.PurchaseOrder == item.PurchaseOrder && a.Line == item.Line select a).FirstOrDefault();
                        var MultiBins = (from a in wdb.vw_InvWhControl where a.Warehouse.Equals(PoLine.MWarehouse) select a.UseMultipleBins).FirstOrDefault();
                        var TraceableType = (from a in wdb.InvMasters where a.StockCode.Equals(item.StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod, ProductClass = a.ProductClass }).FirstOrDefault();
                        var DirectExpese = wdb.mt_DirectExpenseByStockCode(item.StockCode).FirstOrDefault();
                        if (DirectExpese != null)
                        {
                            if (DirectExpese.DirectExpenseIssue == "Y")
                            {
                                hasLines = true;
                                Document.Append("<Item>");
                                Document.Append("<Journal/>");
                                Document.Append("<Warehouse><![CDATA[" + PoLine.MWarehouse + "]]></Warehouse>");
                                Document.Append("<StockCode><![CDATA[" + item.StockCode + "]]></StockCode>");
                                Document.Append("<Version/>");
                                Document.Append("<Release/>");
                                Document.Append("<Quantity>" + item.ReelQty + "</Quantity>");
                                Document.Append("<UnitOfMeasure/>");
                                Document.Append("<Units/>");
                                Document.Append("<Pieces/>");
                                if (MultiBins == "Y")
                                {
                                    Document.Append("<BinLocation><![CDATA[" + item.Bin + "]]></BinLocation>");
                                }

                                Document.Append("<FifoBucket></FifoBucket>");

                                if (TraceableType.TraceableType == "T")
                                {
                                    Document.Append("<Lot><![CDATA[" + item.ReelNumber + "]]></Lot>");
                                }
                                else
                                {
                                    if (TraceableType.SerialMethod != "N")
                                    {
                                        Document.Append("<Serials>");
                                        Document.Append("<SerialNumber><![CDATA[" + item.ReelNumber + "]]></SerialNumber>");
                                        Document.Append("<SerialQuantity>" + item.ReelQty + "</SerialQuantity>");
                                        //Document.Append("<SerialUnits/>");
                                        //Document.Append("<SerialPieces/>");
                                        //Document.Append("<SerialFifoBucket></SerialFifoBucket>");
                                        //Document.Append("<SerialLocation></SerialLocation>");
                                        //Document.Append("<SerialExpiryDate>2006-10-31</SerialExpiryDate>");
                                        Document.Append("</Serials>");

                                        //Document.Append("<SerialAllocation>");
                                        //Document.Append("<FromSerialNumber><![CDATA[" + item.Lot + "]]></FromSerialNumber>");
                                        //Document.Append("<ToSerialNumber><![CDATA[" + item.Lot + "]]></ToSerialNumber>");
                                        //Document.Append("<SerialQuantity>" + item.Quantity + "</SerialQuantity>");
                                        //Document.Append("</SerialAllocation>");
                                    }
                                }




                                //string Employee = Employee;
                                //string EmployeeText = item.Employee;
                                //int index = EmployeeText.IndexOf(" -- ");
                                //if (index == 0)
                                //{
                                //    Employee = item.Employee.Substring(0, 30);
                                //}
                                //else
                                //{
                                //    Employee = item.Employee.Substring(0, index);
                                //}


                                var Reference = PoLine.PurchaseOrder + "-" + item.Line;
                                if (!string.IsNullOrWhiteSpace(Reference))
                                {
                                    if (Reference.Length > 30)
                                    {
                                        Reference = Reference.Substring(0, 30);
                                    }
                                }
                                var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == "DIREXP" && a.ProductClass == TraceableType.ProductClass select a.GlCode).FirstOrDefault();

                                Document.Append("<Reference><![CDATA[" + Reference + "]]></Reference>");
                                Document.Append("<Notation><![CDATA[" + Reference + "]]></Notation>");
                                Document.Append("<LedgerCode><![CDATA[" + GlCode + "]]></LedgerCode>");
                                Document.Append("<PasswordForLedgerCode/>");
                                Document.Append("</Item>");
                            }
                        }

                    }

                    if (hasLines == false)
                    {
                        return "";
                    }


                    Document.Append("</PostInvExpenseIssues>");

                    //Declaration
                    StringBuilder Parameter = new StringBuilder();

                    //Building Parameter content
                    Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Parameter.Append("<!--");
                    Parameter.Append("Sample XML for the Inventory Expense Issues Business Object");
                    Parameter.Append("-->");
                    Parameter.Append("<PostInvExpenseIssues xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTME.XSD\">");
                    Parameter.Append("<Parameters>");
                    Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
                    Parameter.Append("<PostingPeriod>C</PostingPeriod>");
                    Parameter.Append("<CreateLotNumber>N</CreateLotNumber>");
                    Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                    Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                    Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                    Parameter.Append("<IgnoreAnalysis>Y</IgnoreAnalysis>");
                    Parameter.Append("<AskAnalysis>N</AskAnalysis>");
                    Parameter.Append("<CalledFrom/>");
                    Parameter.Append("</Parameters>");
                    Parameter.Append("</PostInvExpenseIssues>");


                    XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "INVTME");
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        string Journal = sys.GetXmlValue(XmlOut, "Journal");
                        ErrorMessage = "Direct Expense Issue Posted Successfully! Journal: " + Journal;
                    }
                    return ErrorMessage;
                }
                else
                {
                    return "No data found!";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CheckItemTraceable(string PurchaseOrder, decimal Line)
        {
            try
            {
                using (var db = new WarehouseManagementEntities(""))
                {
                    var stock = (from a in db.PorMasterDetails where a.PurchaseOrder == PurchaseOrder && a.Line == Line select a.MStockCode).ToList();
                    if (stock.Count > 0)
                    {
                        var Traceable = (from a in db.InvMasters where a.StockCode == stock.FirstOrDefault() select a.TraceableType).FirstOrDefault();
                        if (Traceable == "T")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool CheckItemSerial(string PurchaseOrder, decimal Line)
        {
            try
            {
                using (var db = new WarehouseManagementEntities(""))
                {
                    var stock = (from a in db.PorMasterDetails where a.PurchaseOrder == PurchaseOrder && a.Line == Line select a.MStockCode).ToList();
                    if (stock.Count > 0)
                    {
                        var Traceable = (from a in db.InvMasters where a.StockCode == stock.FirstOrDefault() select a.SerialMethod).FirstOrDefault();
                        if (Traceable != "N")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CheckStockCodeTraceable(string StockCode)
        {
            try
            {
                using (var db = new WarehouseManagementEntities(""))
                {
                    var Traceable = (from a in db.InvMasters where a.StockCode == StockCode select a).FirstOrDefault();
                    if (Traceable.TraceableType == "T")
                    {
                        return true;
                    }
                    else
                    {
                        if (Traceable.SerialMethod != "N")
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildGrnParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the Purchase Order Receipts Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostPurchaseOrderReceipts xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"portor.xsd\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<NonStockedWhToUse>");
            Parameter.Append("</NonStockedWhToUse>");
            Parameter.Append("<GRNMatchingAction>A</GRNMatchingAction>");
            Parameter.Append("<AllowBlankSupplier>N</AllowBlankSupplier>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ManualSerialTransfersAllowed>N</ManualSerialTransfersAllowed>");
            Parameter.Append("<IgnoreAnalysis>N</IgnoreAnalysis>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostPurchaseOrderReceipts>");

            return Parameter.ToString();
        }

        public string PrintLabel(List<LabelPrintPoLine> detail)
        {
            try
            {
                //Template used - 25/07/2017
                //^XA
                //^PON
                //^FX
                //^CF0,40
                //^FO70,50^FDFreedom Stationery^FS
                //^CF0,30
                //^FO550,70^FD<<DATE>>^FS
                //^CF0,40
                //^FO70,100^GB700,1,3^FS

                //^FX
                //^CFA,30
                //^FO70,120^FDPo/Line: 123/1^FS
                //^FO70,160^FDSupplier: ADB001^FS
                //^FO70,200^FDStockCode: A100^FS
                //^FO70,240^FDDesc: TEST StockCode^FS
                //^FO70,280^FDReel No: A100-001^FS
                //^FO70,320^FDSup-Code: 102-587^FS
                //^FO70,360^FDMass: 550^FS
                //^FO70,400^FDMtr: 21000^FS
                //^CFA,15
                //^FO70,440^GB700,1,3^FS
                //^FO500,500^GB250,250,3^FS
                //^FO540,280^BQ,2,5^FDQA,B100|MON240560|600.5|200.678|1234567|V00123^FS
                //^XZ

                if (detail.Count > 0)
                {
                    string PurchaseOrder = detail.FirstOrDefault().PurchaseOrder.PadLeft(15, '0');
                    var Sup = (from a in wdb.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a.Supplier).FirstOrDefault();
                    foreach (var item in detail)
                    {
                        string TemplatePath = HttpContext.Current.Server.MapPath("~/PurchaseOrderLabel/Labels/PoLabel.txt").ToString();
                        if (!string.IsNullOrWhiteSpace(detail.FirstOrDefault().Program))
                        {
                            TemplatePath = HttpContext.Current.Server.MapPath("~/PurchaseOrderLabel/Labels/PoLabelMaintenance.txt").ToString();
                        }
                        StreamReader reader = new StreamReader(TemplatePath);

                        string Template = reader.ReadToEnd();
                        Template = Template.Replace("<<DATE>>", DateTime.Now.ToString("dd-MM-yyyy hh:mm"));
                        Template = Template.Replace("<<PoLine>>", item.PurchaseOrder.TrimStart(new Char[] { '0' }).Trim() + "/" + item.Line.ToString().Trim());
                        Template = Template.Replace("<<Supplier>>", Sup.TrimStart(new Char[] { '0' }).Trim());
                        Template = Template.Replace("<<StockCode>>", item.StockCode);
                        Template = Template.Replace("<<Desc>>", item.Description);



                        if (string.IsNullOrWhiteSpace(detail.FirstOrDefault().Program))
                        {
                            Template = Template.Replace("<<ReelNo>>", item.ReelNumber);
                        }
                        else
                        {
                            if (CheckStockCodeTraceable(item.StockCode))
                            {
                                Template = Template.Replace("<<ReelNo>>", "Lot/Serial: " + item.ReelNumber);
                            }
                            else
                            {
                                Template = Template.Replace("<<ReelNo>>", "");
                            }
                        }


                        Template = Template.Replace("<<SupCode>>", "");
                        Template = Template.Replace("<<Mass>>", item.ReelQuantity.ToString());
                        Template = Template.Replace("<<Mtr>>", "");
                        if (item.Warehouse != "**")
                        {
                            Template = Template.Replace("<<Barcode>>", item.StockCode.Trim() + "|" + "|" + item.ReelQuantity + "|0|" + item.ReelNumber.Trim() + "|" + Sup.TrimStart(new Char[] { '0' }).Trim()); //"B100|MON240560|600.5|200.678|1234567|V00123|012558"
                            Template = Template.Replace("<<NonStocked>>", "");
                        }
                        else
                        {
                            Template = Template.Replace("<<Barcode>>", ""); //"B100|MON240560|600.5|200.678|1234567|V00123|012558"
                            Template = Template.Replace("<<NonStocked>>", "**Non-Stocked**");


                        }
                        Template = Template.Replace("<<NoOfLabels>>", item.NoOfLables.ToString().Trim());
                        reader.Close();
                        StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/PurchaseOrderLabel/Labels/PoLabelTemp.zpl").ToString(), false);
                        writer.WriteLine(Template);
                        writer.Close();
                        string Printer = detail.FirstOrDefault().Printer;
                        string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                        File.Copy(HttpContext.Current.Server.MapPath("~/PurchaseOrderLabel/Labels/PoLabelTemp.zpl").ToString(), PrinterPath, true);
                    }
                }

                return "Label Printed Successfully!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PrintStockLabel(List<LabelPrintPoLine> detail)
        {
            try
            {
                //Template used - 25/07/2017
                //^XA
                //^PON
                //^FX
                //^CF0,40
                //^FO70,50^FDFreedom Stationery^FS
                //^CF0,30
                //^FO550,70^FD<<DATE>>^FS
                //^CF0,40
                //^FO70,100^GB700,1,3^FS

                //^FX
                //^CFA,30
                //^FO70,120^FDPo/Line: 123/1^FS
                //^FO70,160^FDSupplier: ADB001^FS
                //^FO70,200^FDStockCode: A100^FS
                //^FO70,240^FDDesc: TEST StockCode^FS
                //^FO70,280^FDReel No: A100-001^FS
                //^FO70,320^FDSup-Code: 102-587^FS
                //^FO70,360^FDMass: 550^FS
                //^FO70,400^FDMtr: 21000^FS
                //^CFA,15
                //^FO70,440^GB700,1,3^FS
                //^FO500,500^GB250,250,3^FS
                //^FO540,280^BQ,2,5^FDQA,B100|MON240560|600.5|200.678|1234567|V00123^FS
                //^XZ

                if (detail.Count > 0)
                {
                    foreach (var item in detail)
                    {

                        string TemplatePath = HttpContext.Current.Server.MapPath("~/PurchaseOrderLabel/Labels/PoLabel.txt").ToString();
                        if (!string.IsNullOrWhiteSpace(detail.FirstOrDefault().Program))
                        {
                            TemplatePath = HttpContext.Current.Server.MapPath("~/PurchaseOrderLabel/Labels/PoLabelMaintenance.txt").ToString();
                        }
                        StreamReader reader = new StreamReader(TemplatePath);
                        string Template = reader.ReadToEnd();
                        Template = Template.Replace("<<PoLine>>", "");
                        Template = Template.Replace("<<Supplier>>", "");
                        Template = Template.Replace("<<DATE>>", DateTime.Now.Date.ToString("yyyy-MM-dd"));
                        Template = Template.Replace("<<StockCode>>", item.StockCode);
                        Template = Template.Replace("<<Desc>>", item.Description);
                        if (string.IsNullOrWhiteSpace(detail.FirstOrDefault().Program))
                        {
                            Template = Template.Replace("<<ReelNo>>", item.ReelNumber);
                        }
                        else
                        {
                            if (CheckStockCodeTraceable(item.StockCode))
                            {
                                Template = Template.Replace("<<ReelNo>>", "Lot: " + item.ReelNumber);
                            }
                            else
                            {
                                Template = Template.Replace("<<ReelNo>>", "");
                            }
                        }

                        Template = Template.Replace("<<SupCode>>", "");
                        Template = Template.Replace("<<Mass>>", item.ReelQuantity.ToString());
                        Template = Template.Replace("<<Mtr>>", "");
                        Template = Template.Replace("<<Barcode>>", item.StockCode.Trim() + "|" + "|" + item.ReelQuantity + "|0|" + item.ReelNumber.Trim() + "|"); //"B100|MON240560|600.5|200.678|1234567|V00123|012558"
                        Template = Template.Replace("<<NoOfLabels>>", item.NoOfLables.ToString().Trim());
                        Template = Template.Replace("<<Bin>>", item.Bin.ToString().Trim());
                        reader.Close();
                        StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/PurchaseOrderLabel/Labels/PoLabelTemp.zpl").ToString(), false);
                        writer.WriteLine(Template);
                        writer.Close();
                        string Printer = detail.FirstOrDefault().Printer;
                        string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                        File.Copy(HttpContext.Current.Server.MapPath("~/PurchaseOrderLabel/Labels/PoLabelTemp.zpl").ToString(), PrinterPath, true);

                        using (var sdb = new WarehouseManagementEntities(""))
                        {
                            mtStockLabel obj = new mtStockLabel();
                            obj.StockCode = item.StockCode.Trim();
                            obj.Lot = item.ReelNumber.Trim();
                            obj.Quantity = item.ReelQuantity;
                            obj.Warehouse = "";
                            obj.TrnDate = DateTime.Now;
                            obj.Bin = item.Bin;
                            sdb.Entry(obj).State = System.Data.EntityState.Added;
                            sdb.SaveChanges();
                        }
                    }
                }

                return "Label Printed Successfully!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PrintJobLabel(List<mtProductionLabel> detail, string Printer, string BatchSpec, string Department, string LastBatch)
        {
            try
            {
                //Template used - 25/07/2017
                //^XA
                //^PON
                //^FX
                //^CF0,40
                //^FO70,50^FDFreedom Stationery^FS
                //^CF0,30
                //^FO550,70^FD<<DATE>>^FS
                //^CF0,40
                //^FO70,100^GB700,1,3^FS

                //^FX
                //^CFA,30
                //^FO70,120^FDJob: 123/1^FS
                //^FO70,160^FDJob Desc.: ADB001^FS
                //^FO70,200^FDStockCode: A100^FS
                //^FO70,240^FDDesc: TEST StockCode^FS
                //^FO70,280^FDPallet: A100-001^FS
                //^FO70,320^FDQuantity: 102-587^FS
                //^CFA,15
                //^FO70,440^GB700,1,3^FS
                //^FO500,500^GB250,250,3^FS
                //^FO540,280^BQ,2,5^FDQA,B100|MON240560|600.5|200.678|1234567|V00123^FS
                //^XZ

                var ReelNo = "";
                var PrintOpRef = "";
                string operatorRef = "";

                if (!string.IsNullOrWhiteSpace(detail.FirstOrDefault().LotIssued))
                {
                    var result = wdb.mt_GetProductionDetailsByLot(detail.FirstOrDefault().LotIssued).ToList();
                    if (result.Count > 0)
                    {
                        ReelNo = result.FirstOrDefault().BatchId;
                        PrintOpRef = result.FirstOrDefault().PrintOpReference;
                    }

                }




                if (PrintOpRef == null)
                {
                    PrintOpRef = "";
                }
                if (detail.Count > 0)
                {
                    var JobDetail = wdb.sp_GetProductionJobDetails(detail.FirstOrDefault().Job.PadLeft(15, '0')).FirstOrDefault();

                    //foreach (var item in detail)
                    for (int i = 0; i < detail.Count; i++)
                    {
                        StreamReader reader;
                        if (Department == "Wicket")
                        {
                            reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabel.txt").ToString());
                        }
                        else if (Department == "Bag")
                        {
                            reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabel.txt").ToString());
                        }
                        else if (Department == "WICKET")
                        {
                            reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabel.txt").ToString());
                        }
                        else
                        {
                            reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabel.txt").ToString());
                        }

                        string Template = reader.ReadToEnd();
                        if (Department == "Bag")
                        {
                            var PrintFlag = (from a in wdb.mt_GetFlagPrintTropicHeader(JobDetail.StockCode) select a).FirstOrDefault();
                            if (PrintFlag != null)
                            {
                                if (PrintFlag.PrintTropicHeader == "YES")
                                {
                                    Template = Template.Replace("<<HEADER>>", "TROPIC PLASTIC (PTY) LTD");
                                    Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                                }
                                else
                                {
                                    Template = Template.Replace("<<HEADER>>", " ");
                                    Template = Template.Replace("<<CUSTOMER>>", " ");

                                }
                            }

                        }
                        else
                        {
                            Template = Template.Replace("<<HEADER>>", "TROPIC PLASTIC (PTY) LTD");
                            Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                        }

                        Template = Template.Replace("<<DATE>>", DateTime.Now.Date.ToString("yyyy-MM-dd"));
                        Template = Template.Replace("<<JOBNO>>", detail[i].Job.TrimStart(new Char[] { '0' }).Trim());
                        Template = Template.Replace("<<JOBDESC>>", JobDetail.JobDescription);
                        Template = Template.Replace("<<STOCKCODE>>", JobDetail.StockCode);
                        Template = Template.Replace("<<DESC>>", JobDetail.StockDescription);
                        Template = Template.Replace("<<PALLET>>", detail[i].BatchId);
                        Template = Template.Replace("<<QUANTITY>>", detail[i].NetQty.ToString());
                        Template = Template.Replace("<<REEL>>", ReelNo.ToString());
                        Template = Template.Replace("<<PRINTOP>>", PrintOpRef.ToString());
                        Template = Template.Replace("<<BARCODE>>", JobDetail.StockCode.Trim() + "|" + "|" + detail[i].NetQty.ToString() + "|0|" + detail[i].BatchId + "||" + detail[i].Job.TrimStart(new Char[] { '0' }).Trim()); //"B100||550|0|518-1||518"
                        Template = Template.Replace("<<NOOFLABELS>>", "1");
                        Template = Template.Replace("<<WORKCENTRE>>", detail.FirstOrDefault().WorkCentre);
                        var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                        if (setting.PalletNoReq == true)
                        {
                            if (!string.IsNullOrEmpty(detail[i].Operator))
                            {
                                if (detail[i].Operator.Contains("--"))
                                {
                                    string[] OPER = detail[i].Operator.Split('-');
                                    detail[i].Operator = OPER[0];
                                }
                            }
                            string QC2 = "";
                            if (!string.IsNullOrEmpty(detail[i].QC1))
                            {
                                if (detail[i].QC1.Contains("--"))
                                {
                                    string[] qc = detail[i].QC1.Split('-');
                                    detail[i].QC1 = qc[0];
                                }

                                if (detail[i].QC1.Contains("/"))
                                {
                                    string[] qc = detail[i].QC1.Split('/');
                                    detail[i].QC1 = qc[0];
                                    QC2 = qc[1];
                                }
                                else if (detail[i].QC1.Contains(@"\"))
                                {
                                    string[] qc = detail[i].QC1.Split('\\');
                                    detail[i].QC1 = qc[0];
                                    QC2 = qc[1];
                                }

                            }
                            if (!string.IsNullOrEmpty(detail[i].Packer))
                            {
                                if (detail[i].Packer.Contains("--"))
                                {
                                    string[] PACK = detail[i].Packer.Split('-');
                                    detail[i].Packer = PACK[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(detail[i].Supervisor))
                            {
                                if (detail[i].Supervisor.Contains("--"))
                                {
                                    string[] a = detail[i].Supervisor.Split('-');
                                    detail[i].Supervisor = a[0];
                                }
                            }

                            Template = Template.Replace("<<STOCKCODE>>", JobDetail.StockCode);
                            Template = Template.Replace("<<STOCKDESC>>", JobDetail.StockDescription);
                            //Template = Template.Replace("<<REFERENCE>>", JobDetail.Reference);
                            Template = Template.Replace("<<REFERENCE>>", detail.FirstOrDefault().Reference);
                            Template = Template.Replace("<<BAGSPECS>>", JobDetail.BagSpecs);

                            if (string.IsNullOrWhiteSpace(LastBatch))
                            {
                                Template = Template.Replace("<<BAILQTY>>", Convert.ToString(BatchSpec));
                            }
                            else
                            {
                                if (i == detail.Count - 1)
                                {
                                    Template = Template.Replace("<<BAILQTY>>", Convert.ToString(LastBatch));
                                }
                                else
                                {
                                    Template = Template.Replace("<<BAILQTY>>", Convert.ToString(BatchSpec));
                                }
                            }
                            if (Department == "Bag")
                            {
                                var PrintFlag = (from a in wdb.mt_GetFlagPrintTropicHeader(JobDetail.StockCode) select a).FirstOrDefault();
                                if (PrintFlag != null)
                                {
                                    if (PrintFlag.PrintTropicHeader == "YES")
                                    {
                                        Template = Template.Replace("<<HEADER>>", "TROPIC PLASTIC (PTY) LTD");
                                        Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);

                                    }
                                    else
                                    {
                                        Template = Template.Replace("<<HEADER>>", " ");
                                        Template = Template.Replace("<<CUSTOMER>>", " ");

                                    }
                                }

                            }
                            else
                            {
                                Template = Template.Replace("<<HEADER>>", "TROPIC PLASTIC (PTY) LTD");
                                Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                            }

                            Template = Template.Replace("<<OPNO>>", detail[i].Operator);
                            Template = Template.Replace("<<QC1>>", detail[i].QC1);
                            Template = Template.Replace("<<QC2>>", QC2);
                            Template = Template.Replace("<<PACKER>>", detail[i].Packer);
                            Template = Template.Replace("<<SUPERVISOR>>", detail[i].Supervisor);
                            Template = Template.Replace("<<SETTER>>", detail[i].Supervisor);
                            Template = Template.Replace("<<BAILNO>>", detail[i].BatchId);
                        }
                        reader.Close();
                        StreamWriter writer;
                        if (Department == "Wicket")
                        {
                            writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabelTemp.zpl").ToString(), false);
                        }
                        else if (Department == "Bag")
                        {
                            writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabelTemp.zpl").ToString(), false);
                        }
                        else if (Department == "WICKET")
                        {
                            writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabelTemp.zpl").ToString(), false);
                        }
                        else
                        {
                            writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabelTemp.zpl").ToString(), false);
                        }
                        writer.WriteLine(Template);
                        writer.Close();
                        string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                        if (Department == "Wicket")
                        {
                            File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        }
                        else if (Department == "Bag")
                        {
                            File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        }
                        else if (Department == "WICKET")
                        {
                            File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        }
                        else
                        {
                            File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        }

                    }
                }

                return "Label Printed Successfully!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PrintScanCard(List<LabelPrintPoLine> detail)
        {
            try
            {
                //Template used - 25/07/2017
                //^XA
                //^PON
                //^FX
                //^CF0,40
                //^FO70,50^FDFreedom Stationery^FS
                //^CF0,40
                //^FO70,100^GB700,1,3^FS

                //^FX
                //^CFA,30
                //^FO70,200^FDStockCode: <<STOCKCODE>>^FS
                //^FO70,240^FDDesc: <<DESC>>^FS
                //^CFA,15
                //^FO70,440^GB700,1,3^FS
                //^FO500,500^GB250,250,3^FS
                //^FO540,280^BQ,2,5^FDQA,<<BARCODE>>^FS
                //^XZ

                if (detail.Count > 0)
                {
                    foreach (var item in detail)
                    {
                        StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ScanCards/Labels/ScanCardLabel.txt").ToString());
                        string Template = reader.ReadToEnd();
                        Template = Template.Replace("<<STOCKCODE>>", item.StockCode);
                        Template = Template.Replace("<<DESC>>", item.Description);
                        Template = Template.Replace("<<BARCODE>>", item.Barcode.Trim());
                        Template = Template.Replace("<<NoOfLabels>>", item.NoOfLables.ToString().Trim());
                        reader.Close();
                        StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ScanCards/Labels/ScanCardLabelTemp.zpl").ToString(), false);
                        writer.WriteLine(Template);
                        writer.Close();
                        string Printer = detail.FirstOrDefault().Printer;
                        string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                        File.Copy(HttpContext.Current.Server.MapPath("~/ScanCards/Labels/ScanCardLabelTemp.zpl").ToString(), PrinterPath, true);
                    }
                }

                return "Label Printed Successfully!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PrintBinLabel(List<LabelPrintPoLine> detail)
        {
            try
            {
                //Template used - 20/11/2017
                //^XA
                //^FX
                //^CF0,60
                //^FO250,50^FDTropic Plastic^FS
                //^FO300,150^FDBin: 340^FS
                //^FX
                //^BY5,2,270
                //^FO220,350^BC^FD340^FS
                //^XZ

                if (detail.Count > 0)
                {
                    foreach (var item in detail)
                    {
                        StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/BinLabel/Labels/BinLabel.txt").ToString());
                        string Template = reader.ReadToEnd();
                        Template = Template.Replace("<<Bin>>", item.Bin.Trim());

                        reader.Close();
                        StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/BinLabel/Labels/BinLabelTemp.zpl").ToString(), false);
                        writer.WriteLine(Template);
                        writer.Close();
                        string Printer = detail.FirstOrDefault().Printer;
                        string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                        File.Copy(HttpContext.Current.Server.MapPath("~/BinLabel/Labels/BinLabelTemp.zpl").ToString(), PrinterPath, true);
                    }
                }
                return "Label Printed Successfully!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PrintScaleLabel(List<mtProductionLabel> detail, string Printer, string Department)
        {
            string StockDescriptionLine1 = "";
            string StockDescriptionLine2 = "";
            string CustomerNameLine1 = "";
            string CustomerNameLine2 = "";
            string SerialBarcode = "";

            if (detail.Count > 0)
            {
                string Job = detail.FirstOrDefault().Job.PadLeft(15, '0');
                var JobDetails = wdb.sp_GetScalesJobDetails(Job).FirstOrDefault();
                var setting = (from a in wdb.mtScaleSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();

                string StockDescriptionLines = SplitTextLines(JobDetails.StockDescription, (int)setting.LabelTextWidth);

                if (StockDescriptionLines.IndexOf("@|@") > 1)
                {
                    StockDescriptionLine1 = StockDescriptionLines.Substring(0, StockDescriptionLines.IndexOf("@|@"));
                    StockDescriptionLine2 = StockDescriptionLines.Substring(StockDescriptionLines.IndexOf("@|@") + 3).Trim();
                }
                else
                {
                    StockDescriptionLine1 = StockDescriptionLines;
                    StockDescriptionLine2 = "";
                }

                string CustomerNameLines = SplitTextLines(JobDetails.Customer, (int)setting.LabelTextWidth);
                if (CustomerNameLines.IndexOf("@|@") > 1)
                {
                    CustomerNameLine1 = CustomerNameLines.Substring(0, CustomerNameLines.IndexOf("@|@"));
                    CustomerNameLine2 = CustomerNameLines.Substring(CustomerNameLines.IndexOf("@|@") + 3).Trim();
                }
                else
                {
                    CustomerNameLine1 = CustomerNameLines;
                    CustomerNameLine2 = "";
                }

                foreach (var item in detail)
                {
                    SerialBarcode = CreateSerialBarcode(detail.FirstOrDefault().BatchId);
                    //SerialBarcode = detail.FirstOrDefault().BatchId;


                    //string LabelDept = Department;
                    //var BomCostCentre = wdb.sp_GetProductionBomOperations(Job).ToList().OrderByDescending(a => a.Operation).FirstOrDefault().CostCentre;
                    //if (BomCostCentre != null)
                    //{
                    //    if (Department.ToUpper() == BomCostCentre)
                    //    {
                    //        LabelDept = "SLIT";
                    //    }
                    //}




                    StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/ScaleLabel.txt").ToString());
                    string Template = reader.ReadToEnd();
                    decimal core = Math.Round((decimal)item.Core, 2);
                    decimal gross = Math.Round((decimal)item.GrossQty, 2);
                    decimal net = Math.Round((decimal)item.NetQty, 2);

                    if (Department == "SLIT" || Department == "EXTR")
                    {
                        Template = Template.Replace("##STOCKDESCRIPTION1##", StockDescriptionLine1);
                        Template = Template.Replace("##STOCKDESCRIPTION2##", StockDescriptionLine2);
                    }
                    else
                    {
                        Template = Template.Replace("##STOCKDESCRIPTION1##", JobDetails.JobDescription);
                        Template = Template.Replace("##STOCKDESCRIPTION2##", JobDetails.StockCode);
                    }
                    Template = Template.Replace("##STOCKCODE##", JobDetails.StockCode);
                    Template = Template.Replace("##CUSTSTOCKCODE##", JobDetails.CustStockCode);
                    // Template = Template.Replace("##CUSTSTOCKCODE##", "OPERATOR: "+ detail.FirstOrDefault().Operator);

                    Template = Template.Replace("##DIMENSIONS##", JobDetails.Dimensions + "Mic " + JobDetails.Microns);
                    Template = Template.Replace("##CUSTOMERNAME1##", CustomerNameLine1);
                    Template = Template.Replace("##CUSTOMERNAME2##", CustomerNameLine2);
                    Template = Template.Replace("##SERIALNUMBER##", item.BatchId.Trim()); //"B100||550|0|518-1||518"
                    Template = Template.Replace("##SERIALBARCODE##", SerialBarcode);
                    Template = Template.Replace("##NETMASS##", net.ToString() + "kg");
                    Template = Template.Replace("##COREMASS##", core.ToString() + "kg");
                    Template = Template.Replace("##GROSSMASS##", (net + core).ToString() + "kg");
                    Template = Template.Replace("##SCALEID##", detail.FirstOrDefault().ScaleModelId.ToString());
                    Template = Template.Replace("##JOB##", detail.FirstOrDefault().Job.ToString().TrimStart('0'));
                    Template = Template.Replace("##BARCODE##", JobDetails.StockCode + "|" + "|" + net.ToString() + "|0|" + item.BatchId.Trim() + "||" + item.Job.Trim().TrimStart(new Char[] { '0' }).Trim()); //"B100||550|0|518-1||518"
                    if (Department == "SLIT")
                    {
                        Template = Template.Replace("##METERS##", item.Meters.ToString());
                    }
                    if (Department == "PRINT")
                    {
                        Template = Template.Replace("##EXTRUSIONWC##", "E" + detail.FirstOrDefault().PreviousWorkCentre);
                        Template = Template.Replace("##SLITTINGWC##", "P" + detail.FirstOrDefault().WorkCentre);
                        Template = Template.Replace("##DEPARTMENTOP##", "DEPT-" + Department + "  OPERATOR: " + detail.FirstOrDefault().Operator);
                        Template = Template.Replace("##PRINTOPREFERENCE##", detail.FirstOrDefault().PrintOpReference);
                    }
                    else
                    {
                        Template = Template.Replace("##EXTRUSIONWC##", JobDetails.ExtrusionWorkCentre);
                        Template = Template.Replace("##SLITTINGWC##", JobDetails.SlittingWorkCentre);
                        Template = Template.Replace("##DEPARTMENTOP##", "DEPT-" + Department);
                    }

                    Template = Template.Replace("##DATE##", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

                    reader.Close();
                    StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/ScaleLabelTemp.lbl").ToString(), false);
                    writer.WriteLine(Template);
                    writer.Close();
                    string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                    File.Copy(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/ScaleLabelTemp.lbl").ToString(), PrinterPath, true);
                }

                return "";

            }
            else
            {
                return "No data.";
            }
        }



        public void PrintPalletLabel(string PalletNo, string Printer, string Department)
        {
            try
            {
                string StockDescriptionLine1 = "";
                string StockDescriptionLine2 = "";
                var detail = (from a in wdb.mtProductionLabels where a.PalletNo == PalletNo && a.LabelReceipted == "Y" select a).ToList();
                string Job = detail.FirstOrDefault().Job.PadLeft(15, '0');
                var JobDetails = wdb.sp_GetScalesJobDetails(Job).FirstOrDefault();
                //SerialBarcode = detail.FirstOrDefault().BatchId;
                StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/PalletLabel.txt").ToString());
                string Template = reader.ReadToEnd();
                decimal core = Math.Round((decimal)detail.Sum(a => a.Core), 2);
                decimal gross = Math.Round((decimal)detail.Sum(a => a.GrossQty), 2);
                decimal net = Math.Round((decimal)detail.Sum(a => a.NetQty), 2);
                decimal palletWeight = net + core;
                var setting = (from a in wdb.mtScaleSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();

                string StockDescriptionLines = SplitTextLines(JobDetails.StockDescription, (int)setting.LabelTextWidth);

                if (StockDescriptionLines.IndexOf("@|@") > 1)
                {
                    StockDescriptionLine1 = StockDescriptionLines.Substring(0, StockDescriptionLines.IndexOf("@|@"));
                    StockDescriptionLine2 = StockDescriptionLines.Substring(StockDescriptionLines.IndexOf("@|@") + 3).Trim();
                }
                else
                {
                    StockDescriptionLine1 = StockDescriptionLines;
                    StockDescriptionLine2 = "";
                }


                var SerialBarcode = detail.FirstOrDefault().Job + "|" + palletWeight.ToString();
                if (Department == "SLIT" || Department == "EXTR")
                {
                    Template = Template.Replace("##STOCKDESCRIPTION1##", StockDescriptionLine1);
                    Template = Template.Replace("##STOCKDESCRIPTION2##", StockDescriptionLine2);
                }
                else
                {
                    Template = Template.Replace("##STOCKDESCRIPTION1##", JobDetails.JobDescription);
                    Template = Template.Replace("##STOCKDESCRIPTION2##", JobDetails.StockCode);
                }
                Template = Template.Replace("##STOCKCODE##", JobDetails.StockCode);
                Template = Template.Replace("##CUSTSTOCKCODE##", JobDetails.CustStockCode);
                // Template = Template.Replace("##CUSTSTOCKCODE##", "OPERATOR: "+ detail.FirstOrDefault().Operator);

                Template = Template.Replace("##DIMENSIONS##", JobDetails.Dimensions + "Mic " + JobDetails.Microns);
                //Template = Template.Replace("##CUSTOMERNAME1##", CustomerNameLine1);
                //Template = Template.Replace("##CUSTOMERNAME2##", CustomerNameLine2);
                //Template = Template.Replace("##SERIALNUMBER##", item.BatchId.Trim()); //"B100||550|0|518-1||518"
                Template = Template.Replace("##SERIALBARCODE##", SerialBarcode);
                Template = Template.Replace("##NETMASS##", net.ToString() + "kg");
                Template = Template.Replace("##COREMASS##", core.ToString() + "kg");
                Template = Template.Replace("##GROSSMASS##", (net + core).ToString() + "kg");
                Template = Template.Replace("##SCALEID##", detail.FirstOrDefault().ScaleModelId.ToString());
                Template = Template.Replace("##JOB##", detail.FirstOrDefault().Job.ToString().TrimStart('0'));
                //Template = Template.Replace("##BARCODE##", JobDetails.StockCode + "|" + "|" + net.ToString() + "|0|" + item.BatchId.Trim() + "||" + item.Job.Trim().TrimStart(new Char[] { '0' }).Trim()); //"B100||550|0|518-1||518"
                if (Department == "PRINT")
                {
                    Template = Template.Replace("##EXTRUSIONWC##", "E" + detail.FirstOrDefault().PreviousWorkCentre);
                    Template = Template.Replace("##SLITTINGWC##", "P" + detail.FirstOrDefault().WorkCentre);
                    Template = Template.Replace("##DEPARTMENTOP##", "DEPT-" + Department + "  OPERATOR: " + detail.FirstOrDefault().Operator);
                }
                else
                {
                    Template = Template.Replace("##EXTRUSIONWC##", JobDetails.ExtrusionWorkCentre);
                    Template = Template.Replace("##SLITTINGWC##", JobDetails.SlittingWorkCentre);
                    Template = Template.Replace("##DEPARTMENTOP##", "DEPT-" + Department);
                }

                Template = Template.Replace("##DATE##", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

                reader.Close();
                StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/PalletLabelTemp.lbl").ToString(), false);
                writer.WriteLine(Template);
                writer.Close();
                string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                File.Copy(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/PalletLabelTemp.lbl").ToString(), PrinterPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void PrintPalletLbl(string StockCode, string Job, decimal? Weight, string Department, string PrinterName, string PalletNo)
        {
            try
            {
                var mic = wdb.mt_GetMicronDimensionByPalletNo(PalletNo).FirstOrDefault();
                var desc = (from a in wdb.mtProductionLabels join b in wdb.WipMasters on a.Job equals b.Job where a.Job == Job select b.StockDescription).FirstOrDefault();
                StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/PalletLabel.txt").ToString());
                string Template = reader.ReadToEnd();
                Template = Template.Replace("##STOCKCODE##", StockCode);
                //Template = Template.Replace("##CUSTOMERNAME1##", CustomerNameLine1);
                //Template = Template.Replace("##CUSTOMERNAME2##", CustomerNameLine2);
                Template = Template.Replace("##PALLETNO##", PalletNo.Trim()); // "L-U1-M3-U001-2478||55.100|0|540-001||540"
                Template = Template.Replace("##STOCKCODEDESCRIPTION##", desc);
                Template = Template.Replace("##JOB##", Job.TrimStart('0'));
                Template = Template.Replace("##MICRON##", mic.GenMicron.ToString().Split('.')[0]);
                Template = Template.Replace("##DIMENSION##", mic.InvoiceDim);
                Template = Template.Replace("##WEIGHT##", Weight.ToString());
                Template = Template.Replace("##BARCODE##", StockCode + "|" + "|" + Weight.ToString() + "|0|" + PalletNo.Trim() + "||" + Job.Trim().TrimStart(new Char[] { '0' }).Trim()); //"L-U1-M3-U001-2478||55.100|0|540-001||540"

                Template = Template.Replace("##DATE##", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

                reader.Close();
                StreamWriter writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/PalletLabelTemp.lbl").ToString(), false);
                writer.WriteLine(Template);
                writer.Close();
                string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == PrinterName select a.PrinterPath).FirstOrDefault();
                File.Copy(HttpContext.Current.Server.MapPath("~/ScaleLabel/Labels/" + Department + "/PalletLabelTemp.lbl").ToString(), PrinterPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string SplitTextLines(string StringToSplit, int maxChars)
        {
            string returnString = "";

            StringToSplit = Regex.Replace(StringToSplit, @"\s+", " ");
            StringToSplit = StringToSplit.Substring(0, Math.Min(maxChars * 2, StringToSplit.Length));

            if (StringToSplit.Length > maxChars)
            {
                int splitPos = StringToSplit.Substring(0, Math.Min(maxChars, StringToSplit.Length)).LastIndexOfAny(new char[] { ' ', '-' });
                if (splitPos > 0 && splitPos < maxChars)
                {
                    return returnString =
                        StringToSplit.Substring(0, splitPos + 1).TrimEnd() + "@|@" +
                        StringToSplit.Substring(splitPos + 1).TrimStart();
                }
                else
                {
                    return returnString = StringToSplit;
                }
            }
            else
            {
                return returnString = StringToSplit;
            }
        }
        public string CreateSerialBarcode(string SerialNumber)
        {

            // Regex regex = new Regex(Regex.Escape("-"));
            //SerialNumber = regex.Replace(SerialNumber, "&E-", 1);
            //// SerialNumber = regex.Replace(SerialNumber, "-", 1);
            return SerialNumber;
        }

        public string ReprintPrintJobLabel(List<mtProductionLabel> detail, string Printer, string Department)
        {
            try
            {
                if (detail.Count > 0)
                {
                    var JobDetail = wdb.sp_GetProductionJobDetails(detail.FirstOrDefault().Job.PadLeft(15, '0')).FirstOrDefault();

                    foreach (var item in detail)
                    {
                        StreamReader reader;
                        if (Department == "Wicket")
                        {
                            reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabel.txt").ToString());
                        }
                        else if (Department == "WICKET")
                        {
                            reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabel.txt").ToString());
                        }
                        else if (Department == "BAG")
                        {
                            reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabel.txt").ToString());
                        }
                        else
                        {
                            reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabel.txt").ToString());
                        }


                        var ReelNo = "";
                        var PrintOpRef = "";
                        string operatorRef = "";

                        if (!string.IsNullOrWhiteSpace(item.LotIssued))
                        {
                            var result = wdb.mt_GetProductionDetailsByLot(item.LotIssued).ToList();
                            if (result.Count > 0)
                            {
                                ReelNo = result.FirstOrDefault().BatchId;
                                PrintOpRef = result.FirstOrDefault().PrintOpReference;
                            }

                        }

                        string Template = reader.ReadToEnd();
                        Template = Template.Replace("<<DATE>>", DateTime.Now.Date.ToString("yyyy-MM-dd"));
                        Template = Template.Replace("<<JOBNO>>", item.Job.TrimStart(new Char[] { '0' }).Trim());
                        Template = Template.Replace("<<JOBDESC>>", JobDetail.JobDescription);
                        Template = Template.Replace("<<STOCKCODE>>", JobDetail.StockCode);
                        Template = Template.Replace("<<DESC>>", JobDetail.StockDescription);
                        Template = Template.Replace("<<PALLET>>", item.BatchId);
                        Template = Template.Replace("<<QUANTITY>>", item.NetQty.ToString());
                        Template = Template.Replace("<<BARCODE>>", JobDetail.StockCode.Trim() + "|" + "|" + item.NetQty.ToString() + "|0|" + item.BatchId + "||" + item.Job.TrimStart(new Char[] { '0' }).Trim()); //"B100||550|0|518-1||518"
                        Template = Template.Replace("<<NOOFLABELS>>", "1");
                        Template = Template.Replace("<<WORKCENTRE>>", "");
                        var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                        if (setting.PalletNoReq == true)
                        {
                            if (!string.IsNullOrEmpty(item.Operator))
                            {
                                if (item.Operator.Contains("--"))
                                {
                                    string[] OPER = item.Operator.Split('-');
                                    item.Operator = OPER[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(item.QC1))
                            {
                                if (item.QC1.Contains("--"))
                                {
                                    string[] qc = item.QC1.Split('-');
                                    item.QC1 = qc[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(item.Packer))
                            {
                                if (item.Packer.Contains("--"))
                                {
                                    string[] PACK = item.Packer.Split('-');
                                    item.Packer = PACK[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(item.Supervisor))
                            {
                                if (item.Supervisor.Contains("--"))
                                {
                                    string[] a = item.Supervisor.Split('-');
                                    item.Supervisor = a[0];
                                }
                            }

                            string BatchQty = Convert.ToString(item.NetQty);
                            string CustomFormQty = JobDetail.BatchQty;
                            if (!string.IsNullOrEmpty(BatchQty))
                            {
                                if (CustomFormQty.Contains("="))
                                {
                                    string[] a = CustomFormQty.Split('=');
                                    string value = a[1].Trim();
                                    if (BatchQty == value)
                                    {
                                        BatchQty = CustomFormQty;
                                    }

                                }
                            }

                            Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                            Template = Template.Replace("<<STOCKCODE>>", JobDetail.StockCode);
                            Template = Template.Replace("<<STOCKDESC>>", JobDetail.StockDescription);
                            Template = Template.Replace("<<REFERENCE>>", JobDetail.Reference);
                            Template = Template.Replace("<<BAGSPECS>>", JobDetail.BagSpecs);
                            Template = Template.Replace("<<BAILQTY>>", BatchQty);
                            Template = Template.Replace("<<OPNO>>", item.Operator);
                            Template = Template.Replace("<<QC1>>", item.QC1);
                            Template = Template.Replace("<<QC2>>", "");
                            Template = Template.Replace("<<PACKER>>", item.Packer);
                            Template = Template.Replace("<<SUPERVISOR>>", item.Supervisor);
                            Template = Template.Replace("<<SETTER>>", item.Supervisor);
                            Template = Template.Replace("<<BAILNO>>", item.BatchId);
                            Template = Template.Replace("<<REEL>>", ReelNo.ToString());
                            Template = Template.Replace("<<PRINTOP>>", PrintOpRef.ToString());

                        }
                        reader.Close();
                        StreamWriter writer;
                        if (Department == "Wicket")
                        {
                            writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabelTemp.zpl").ToString(), false);
                        }
                        else if (Department == "WICKET")
                        {
                            writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabelTemp.zpl").ToString(), false);
                        }
                        else if (Department == "BAG")
                        {
                            writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabelTemp.zpl").ToString(), false);
                        }
                        else
                        {
                            writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabelTemp.zpl").ToString(), false);
                        }
                        writer.WriteLine(Template);
                        writer.Close();
                        string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                        if (Department == "Wicket")
                        {
                            File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        }
                        else if (Department == "WICKET")
                        {
                            File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        }
                        else if (Department == "BAG")
                        {
                            File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        }
                        else
                        {
                            File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        }

                    }
                }

                return "Label Printed Successfully!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //2022-09-30-SR Print function for PackLabelPrintController
        public string PrintPackLabel(List<mtProductionPackLabelPrint> packDetails)
        {
            try
            {
                //Template used - 2022/10/13
                //^XA
                //^ PON
                //^ FX
                //^ CF0,40
                //^ FO30,20 ^ FDTROPIC PLASTIC(PTY) LTD ^ FS
                //^ CF0,30
                //^ FO630,30 ^ FD << DATE >> ^FS
                //^ CF0,40
                //^ FO20,55 ^ GB780,1,3 ^ FS

                //^ FX
                //^ CF0,35
                //^ FO30,70 ^ FDCustomer: << CUSTOMER >> ^FS
                //^ FO30,110 ^ FDProduct: << JOBDESC >> ^FS
                //^ FO30,150 ^ FDStockCode: << STOCKCODE >> ^FS
                //^ FO30,190 ^ FDREF.: << REFERENCE >> ^FS
                //^ FO30,230 ^ FDBAG SPECS: << BAGSPECS >> ^FS
                //^ FO30,270 ^ FDBATCH NO.: << JOBNO >> ^FS
                //^ FO30,310 ^ FDQTY: << BAGPERPACK >> ^FS
                //^ FO30,350 ^ FDOPT.: << OPNO >> ^FS
                //^ FO110,375 ^ GB80,1,3 ^ FS
                //^ FO30,390 ^ FDQC 1: << QC1 >> ^FS
                //^ FO110,415 ^ GB80,1,3 ^ FS
                //^ FO440,390 ^ FDPACKER: << PACKER >> ^FS
                //^ FO570,415 ^ GB80,1,3 ^ FS
                //^ FO30,430 ^ FDSUPERVISOR: << SUPERVISOR >> ^FS
                //^ FO230,455 ^ GB80,1,3 ^ FS
                //^ FO440,430 ^ FDWC:<< WORKCENTRE >> ^FS
                //^ FO500,455 ^ GB80,1,3 ^ FS
                //^ FO30,510 ^ FDExt No.: << EXTNO >> ^FS
                //^ FO440,510 ^ FDExt Roll: << EXTROLL >> ^FS
                //^ FO30,550 ^ FDPrinter OP: << PRINTEROP >> ^FS
                //^ FO440,550 ^ FDPrint Roll: << PRINTROLL >> ^FS
                //^ FO30,600 ^ FDBATCH: << BATCHID >> ^FS



                //^ PQ1
                //^ XZ
                string Job = packDetails[0].Job;
                string BatchId = packDetails[0].BatchId;

                //List<mtProductionLabel> detail = (from a in wdb.mtProductionLabels where a.Job == packDetails[0].Job && a.BatchId == packDetails[0].BatchId select a).ToList();
                List<mtProductionLabel> detail = (from a in wdb.mtProductionLabels where a.Job == Job && a.BatchId == BatchId select a).ToList();
                var ReelNo = "";
                var PrintOpRef = "";
                string operatorRef = "";

                if (!string.IsNullOrWhiteSpace(detail.FirstOrDefault().LotIssued))
                {
                    var result = wdb.mt_GetProductionDetailsByLot(detail.FirstOrDefault().LotIssued).ToList();
                    if (result.Count > 0)
                    {
                        ReelNo = result.FirstOrDefault().BatchId;
                        PrintOpRef = result.FirstOrDefault().PrintOpReference;
                    }

                }




                if (PrintOpRef == null)
                {
                    PrintOpRef = "";
                }
                if (detail.Count > 0)
                {
                    var JobDetail = wdb.sp_GetProductionJobDetails(detail.FirstOrDefault().Job.PadLeft(15, '0')).FirstOrDefault();

                    //foreach (var item in detail)
                    for (int i = 0; i < detail.Count; i++)
                    {
                        StreamReader reader;
                        //if (Department == "Wicket")
                        //{
                        //    reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabel.txt").ToString());
                        //}
                        //else if (Department == "Bag")
                        //{
                        //    reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabel.txt").ToString());
                        //}
                        //else if (Department == "WICKET")
                        //{
                        //    reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabel.txt").ToString());
                        //}
                        //else
                        //{
                        //    reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabel.txt").ToString());
                        //}
                        reader = new StreamReader(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/PackLabel/PackLabel.txt").ToString());
                        string Template = reader.ReadToEnd();
                        //if (Department == "Bag")
                        //{
                        //    var PrintFlag = (from a in wdb.mt_GetFlagPrintTropicHeader(JobDetail.StockCode) select a).FirstOrDefault();
                        //    if (PrintFlag != null)
                        //    {
                        //        if (PrintFlag.PrintTropicHeader == "YES")
                        //        {
                        //            Template = Template.Replace("<<HEADER>>", "TROPIC PLASTIC (PTY) LTD");
                        //            Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                        //        }
                        //        else
                        //        {
                        //            Template = Template.Replace("<<HEADER>>", " ");
                        //            Template = Template.Replace("<<CUSTOMER>>", " ");

                        //        }
                        //    }

                        //}
                        //else
                        //{
                        //    Template = Template.Replace("<<HEADER>>", "TROPIC PLASTIC (PTY) LTD");
                        //    Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                        //}
                        //var BatchID = packDetails[0].BatchId + "-" +  packDetails[0].PackNo.ToString().PadLeft(2,'0') ;//delete
                        Template = Template.Replace("<<BATCHID>>", packDetails[0].BatchPackNo);
                        Template = Template.Replace("<<EXTNO>>", packDetails[0].ExtruderNo);
                        Template = Template.Replace("<<EXTROLL>>", packDetails[0].ExtruderRoll);
                        Template = Template.Replace("<<PRINTROLL>>", packDetails[0].PrintRoll);
                        Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                        Template = Template.Replace("<<DATE>>", DateTime.Now.Date.ToString("yyyy-MM-dd"));
                        Template = Template.Replace("<<JOBNO>>", detail[i].Job.TrimStart(new Char[] { '0' }).Trim());
                        Template = Template.Replace("<<JOBDESC>>", JobDetail.JobDescription);
                        Template = Template.Replace("<<STOCKCODE>>", JobDetail.StockCode);
                        Template = Template.Replace("<<DESC>>", JobDetail.StockDescription);
                        Template = Template.Replace("<<PALLET>>", detail[i].BatchId);
                        Template = Template.Replace("<<QUANTITY>>", detail[i].NetQty.ToString());
                        Template = Template.Replace("<<REEL>>", ReelNo.ToString());
                        Template = Template.Replace("<<PRINTEROP>>", packDetails[0].PrinterOp);
                        Template = Template.Replace("<<BARCODE>>", JobDetail.StockCode.Trim() + "|" + "|" + detail[i].NetQty.ToString() + "|0|" + detail[i].BatchId + "||" + detail[i].Job.TrimStart(new Char[] { '0' }).Trim()); //"B100||550|0|518-1||518"
                        Template = Template.Replace("<<NOOFLABELS>>", packDetails[0].NoOfLabels.ToString());
                        Template = Template.Replace("<<WORKCENTRE>>", detail.FirstOrDefault().WorkCentre);
                        var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                        if (setting.PalletNoReq == true)
                        {
                            if (!string.IsNullOrEmpty(detail[i].Operator))
                            {
                                if (detail[i].Operator.Contains("--"))
                                {
                                    string[] OPER = detail[i].Operator.Split('-');
                                    detail[i].Operator = OPER[0];
                                }
                            }
                            string QC2 = "";
                            if (!string.IsNullOrEmpty(detail[i].QC1))
                            {
                                if (detail[i].QC1.Contains("--"))
                                {
                                    string[] qc = detail[i].QC1.Split('-');
                                    detail[i].QC1 = qc[0];
                                }

                                if (detail[i].QC1.Contains("/"))
                                {
                                    string[] qc = detail[i].QC1.Split('/');
                                    detail[i].QC1 = qc[0];
                                    QC2 = qc[1];
                                }
                                else if (detail[i].QC1.Contains(@"\"))
                                {
                                    string[] qc = detail[i].QC1.Split('\\');
                                    detail[i].QC1 = qc[0];
                                    QC2 = qc[1];
                                }

                            }
                            if (!string.IsNullOrEmpty(detail[i].Packer))
                            {
                                if (detail[i].Packer.Contains("--"))
                                {
                                    string[] PACK = detail[i].Packer.Split('-');
                                    detail[i].Packer = PACK[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(detail[i].Supervisor))
                            {
                                if (detail[i].Supervisor.Contains("--"))
                                {
                                    string[] a = detail[i].Supervisor.Split('-');
                                    detail[i].Supervisor = a[0];
                                }
                            }

                            Template = Template.Replace("<<STOCKCODE>>", JobDetail.StockCode);
                            Template = Template.Replace("<<STOCKDESC>>", JobDetail.StockDescription);
                            //Template = Template.Replace("<<REFERENCE>>", JobDetail.Reference);
                            Template = Template.Replace("<<REFERENCE>>", detail.FirstOrDefault().Reference);
                            Template = Template.Replace("<<BAGSPECS>>", JobDetail.BagSpecs);

                            //if (string.IsNullOrWhiteSpace(LastBatch))
                            //{
                            //    Template = Template.Replace("<<BAILQTY>>", Convert.ToString(BatchSpec));
                            //}
                            //else
                            //{
                            //    if (i == detail.Count - 1)
                            //    {
                            //        Template = Template.Replace("<<BAILQTY>>", Convert.ToString(LastBatch));
                            //    }
                            //    else
                            //    {
                            //        Template = Template.Replace("<<BAILQTY>>", Convert.ToString(BatchSpec));
                            //    }
                            //}
                            Template = Template.Replace("<<BAGPERPACK>>", packDetails[0].PackSize.ToString());
                            //if (Department == "Bag")
                            //{
                            //    var PrintFlag = (from a in wdb.mt_GetFlagPrintTropicHeader(JobDetail.StockCode) select a).FirstOrDefault();
                            //    if (PrintFlag != null)
                            //    {
                            //        if (PrintFlag.PrintTropicHeader == "YES")
                            //        {
                            //            Template = Template.Replace("<<HEADER>>", "TROPIC PLASTIC (PTY) LTD");
                            //            Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);

                            //        }
                            //        else
                            //        {
                            //            Template = Template.Replace("<<HEADER>>", " ");
                            //            Template = Template.Replace("<<CUSTOMER>>", " ");

                            //        }
                            //    }

                            //}
                            //else
                            //{
                            //    Template = Template.Replace("<<HEADER>>", "TROPIC PLASTIC (PTY) LTD");
                            //    Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                            //}
                            Template = Template.Replace("<<CUSTOMER>>", JobDetail.Customer);
                            Template = Template.Replace("<<OPNO>>", detail[i].Operator);
                            Template = Template.Replace("<<QC1>>", detail[i].QC1);
                            Template = Template.Replace("<<QC2>>", QC2);
                            Template = Template.Replace("<<PACKER>>", detail[i].Packer);
                            Template = Template.Replace("<<SUPERVISOR>>", detail[i].Supervisor);
                            Template = Template.Replace("<<SETTER>>", detail[i].Supervisor);
                            Template = Template.Replace("<<BAILNO>>", detail[i].BatchId);
                        }
                        reader.Close();
                        StreamWriter writer;
                        //if (Department == "Wicket")
                        //{
                        //    writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabelTemp.zpl").ToString(), false);
                        //}
                        //else if (Department == "Bag")
                        //{
                        //    writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabelTemp.zpl").ToString(), false);
                        //}
                        //else if (Department == "WICKET")
                        //{
                        //    writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabelTemp.zpl").ToString(), false);
                        //}
                        //else
                        //{
                        //    writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabelTemp.zpl").ToString(), false);
                        //}
                        writer = new StreamWriter(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/PackLabel/PackLabel.zpl").ToString(), false);
                        writer.WriteLine(Template);
                        writer.Close();
                        string Printer = packDetails[0].Printer;
                        string PrinterPath = (from a in mdb.mtLabelPrinters where a.PrinterName == Printer select a.PrinterPath).FirstOrDefault();
                        //if (Department == "Wicket")
                        //{
                        //    File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WicketLabel/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        //}
                        //else if (Department == "Bag")
                        //{
                        //    File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/BaggingLabel/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        //}
                        //else if (Department == "WICKET")
                        //{
                        //    File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/WICKET/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        //}
                        //else
                        //{
                        //    File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/JobLabelTemp.zpl").ToString(), PrinterPath, true);
                        //}
                        File.Copy(HttpContext.Current.Server.MapPath("~/ProductionLabel/Labels/PackLabel/PackLabel.zpl").ToString(), PrinterPath, true);
                    }
                }

                return "Label Printed Successfully!";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}