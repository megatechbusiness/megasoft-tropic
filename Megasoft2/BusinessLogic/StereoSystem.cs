using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class StereoSystem
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private SysproEntities sdb = new SysproEntities("");
        private SysproCore objSyspro = new SysproCore();

        public string BuildGrnParameter()
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildGrn(StereoSystemGrnViewModel model)
        {
            var result = (from a in model.PoLine.ToList() where a.Selected == true select a).ToList();

            if (result.Count > 0)
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

                foreach (var item in result)
                {
                    Document.Append("<Item>");
                    Document.Append("<Receipt>");
                    Document.Append("<Journal />");
                    Document.Append("<PurchaseOrder>" + model.PurchaseOrder + "</PurchaseOrder>");
                    Document.Append("<PurchaseOrderLine>" + item.Line + "</PurchaseOrderLine>");
                    Document.Append("<Warehouse />");
                    //Document.Append("<StockCode>" + StockDesc + "</StockCode>");
                    Document.Append("<Quantity>" + item.GrnQty + "</Quantity>");
                    Document.Append("<UnitOfMeasure />");
                    Document.Append("<Units />");
                    Document.Append("<Pieces />");
                    Document.Append("<DeliveryNote><![CDATA[" + model.Invoice + "]]></DeliveryNote>");
                    Document.Append("<Cost />");
                    Document.Append("<CostBasis>P</CostBasis>");
                    Document.Append("<SwitchOnGRNMatching>N</SwitchOnGRNMatching>");
                    //Document.Append("<GRNNumber></GRNNumber>");

                    Document.Append("<Reference>" + model.Invoice + "</Reference>");
                    Document.Append("<GRNSource>1</GRNSource>");
                    Document.Append("<UseSingleTypeABCElements>N</UseSingleTypeABCElements>");
                    //Document.Append("<Lot />");
                    //Document.Append("<LotExpiryDate />");
                    //Document.Append("<Certificate />");
                    //Document.Append("<Concession />");
                    //Document.Append("<Bins>");
                    //Document.Append("<BinLocation>A1</BinLocation>");
                    //Document.Append("<BinQuantity>750.000</BinQuantity>");
                    //Document.Append("<BinUnits />");
                    //Document.Append("<BinPieces />");
                    //Document.Append("</Bins>");
                    //Document.Append("<Serials>");
                    //Document.Append("<SerialNumber>0205</SerialNumber>");
                    //Document.Append("<SerialQuantity>1</SerialQuantity>");
                    //Document.Append("<SerialUnits />");
                    //Document.Append("<SerialPieces />");
                    //Document.Append("<SerialExpiryDate />");
                    //Document.Append("<SerialLocation />");
                    //Document.Append("</Serials>");
                    //Document.Append("<SerialRange>");
                    //Document.Append("<SerialPrefix>999</SerialPrefix>");
                    //Document.Append("<SerialSuffix>1</SerialSuffix>");
                    //Document.Append("<StartSerialNumber />");
                    //Document.Append("<SerialQuantity>8</SerialQuantity>");
                    //Document.Append("<SerialExpiryDate />");
                    //Document.Append("<SerialLocation />");
                    //Document.Append("</SerialRange>");
                    Document.Append("<PurchaseOrderLineComplete>N</PurchaseOrderLineComplete>");
                    Document.Append("<IncreaseSalesOrderQuantity>N</IncreaseSalesOrderQuantity>");
                    Document.Append("<ChangeSalesOrderStatus>N</ChangeSalesOrderStatus>");
                    Document.Append("<ApplyCostMultiplier>N</ApplyCostMultiplier>");
                    Document.Append("<CostMultiplier />");
                    //Document.Append("<Notation><![CDATA[" + Notation + "]]></Notation>");
                    //if (Warehouse == "**")
                    //{
                    //    Document.Append("<LedgerCode><![CDATA[" + SuspenseAccount + "]]></LedgerCode>");
                    //    Document.Append("<PasswordForLedgerCode />");
                    //    Document.Append("<DebitLedgerCode><![CDATA[" + GlCode + "]]></DebitLedgerCode>");
                    //    Document.Append("<PasswordForDebitLedgerCode />");
                    //}

                    //Document.Append("<CountryOfOrigin />");
                    //Document.Append("<DeliveryTerms />");
                    //Document.Append("<ShippingLocation>");
                    //Document.Append("</ShippingLocation>");
                    //Document.Append("<NatureOfTransaction />");
                    //Document.Append("<ModeOfTransport />");
                    //Document.Append("<TradersReference />");
                    //Document.Append("<TariffCode />");
                    //Document.Append("<UnitMass />");
                    //Document.Append("<SupplementaryUnits>N</SupplementaryUnits>");
                    //Document.Append("<SupplementaryUnitsFactor>");
                    //Document.Append("</SupplementaryUnitsFactor>");
                    //Document.Append("<AnalysisEntry />");
                    //Document.Append("<AnalysisLineEntry>");
                    //Document.Append("<AnalysisCode1>Air</AnalysisCode1>");
                    //Document.Append("<AnalysisCode2>Conf</AnalysisCode2>");
                    //Document.Append("<AnalysisCode3>East</AnalysisCode3>");
                    //Document.Append("<AnalysisCode4 />");
                    //Document.Append("<AnalysisCode5 />");
                    //Document.Append("<StartDate />");
                    //Document.Append("<EndDate />");
                    //Document.Append("<EntryAmount>100</EntryAmount>");
                    //Document.Append("<Comment>Analysis entry details</Comment>");
                    //Document.Append("</AnalysisLineEntry>");
                    //Document.Append("<DebitAnalysisEntry />");

                    //var AnalysisRequired = (from a in sdb.GenMasters.AsNoTracking() where a.GlCode == GlCode select a).ToList();

                    //if (AnalysisRequired.Count > 0)
                    //{
                    //    if (AnalysisRequired.FirstOrDefault().AnalysisRequired == "Y")
                    //    {
                    //        decimal EntryAmount = Math.Round(Convert.ToDecimal(Price * Qty), 2);
                    //        Document.Append("<DebitAnalysisLineEntry>");
                    //        Document.Append("<AnalysisCode1><![CDATA[" + AnalysisEntry + "]]></AnalysisCode1>");
                    //        Document.Append("<StartDate />");
                    //        Document.Append("<EndDate />");
                    //        Document.Append("<EntryAmount>" + EntryAmount + "</EntryAmount>");
                    //        Document.Append("<Comment></Comment>");
                    //        Document.Append("</DebitAnalysisLineEntry>");

                    //        //Document.Append("<AnalysisLineEntry>");
                    //        //Document.Append("<AnalysisCode1>" + AnalysisEntry + "</AnalysisCode1>");
                    //        //Document.Append("<StartDate />");
                    //        //Document.Append("<EndDate />");
                    //        //Document.Append("<EntryAmount>" + EntryAmount + "</EntryAmount>");
                    //        //Document.Append("<Comment></Comment>");
                    //        //Document.Append("</AnalysisLineEntry>");
                    //    }
                    //}

                    //Document.Append("<eSignature />");
                    Document.Append("</Receipt>");
                    Document.Append("</Item>");
                }

                Document.Append("</PostPurchaseOrderReceipts>");
                return Document.ToString();
            }
            else
            {
                return "";
            }
        }

        public string PostGrn(StereoSystemGrnViewModel model, string Guid)
        {
            try
            {
                var CheckGrn = (from a in wdb.mtStereoDetails where a.PurchaseOrder == model.PurchaseOrder && a.Invoice.ToUpper() == model.Invoice.ToUpper() && a.Grn != "" select a.Grn).ToList();
                if (CheckGrn.Count > 0)
                {
                    return "";
                }

                string Document = BuildGrn(model);
                if (Document == "")
                {
                    return "";
                }
                string Parameter = this.BuildGrnParameter();

                string XmlOut = objSyspro.SysproPost(Guid, Parameter, Document, "PORTOR");

                string ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return "Error : " + ErrorMessage;
                }
                else
                {
                    string Grn = objSyspro.GetFirstXmlValue(XmlOut, "Grn");

                    using (var udb = new WarehouseManagementEntities(""))
                    {
                        string Username = HttpContext.Current.User.Identity.Name.ToUpper(); 
                        var result = (from a in model.PoLine.ToList() where a.Selected == true select a).ToList();
                        foreach (var item in result)
                        {
                            udb.sp_StereoUpdateGrnLine(model.PurchaseOrder, (int)item.Line, Grn.PadLeft(15, '0'), model.Invoice.ToUpper(), model.InvoiceDate, model.InvoiceAmount, item.GrnWidth, item.GrnLength,Username);
                        }

                    }
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostAp(StereoSystemGrnViewModel model, string Grn, string Guid)
        {
            try
            {
                var CheckAP = (from a in wdb.mtStereoDetails where a.PurchaseOrder == model.PurchaseOrder && a.Invoice.ToUpper() == model.Invoice.ToUpper() && a.Grn == Grn && a.ApJournal != "" select a.ApJournal).ToList();
                if (CheckAP.Count > 0)
                {
                    return "";
                }            
                var Header = (from a in wdb.mtStereoHdrs where a.PurchaseOrder == model.PurchaseOrder select a).ToList().FirstOrDefault();
                Grn = Grn.PadLeft(15, '0');
                var result = (from a in wdb.GrnDetails.AsNoTracking() where a.Grn == Grn && a.Supplier == Header.SupplierReference select a).ToList();

                decimal PermissableTax = 0;
                string Document = this.BuildApInvoiceDocument(Header.SupplierReference.ToString(), Grn, model.PurchaseOrder, model.Invoice, model.InvoiceDate, model.InvoiceAmount, result, ref PermissableTax);

                string Parameter = this.BuildGrnMatchingParameter((int)result.FirstOrDefault().GrnYear, (int)result.FirstOrDefault().GrnMonth, PermissableTax);

                string XmlOut = objSyspro.SysproPost(Guid, Parameter, Document, "APSTIN");

                string ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return "Error : " + ErrorMessage;
                }
                else
                {
                    string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                    string JournalYear = objSyspro.GetFirstXmlValue(XmlOut, "TrnYear");
                    string JournalMonth = objSyspro.GetFirstXmlValue(XmlOut, "TrnMonth");

                    using (var udb = new WarehouseManagementEntities(""))
                    {
                        var po = (from a in udb.mtStereoDetails where a.Grn == Grn select a).ToList();
                        foreach (var item in po)
                        {
                            item.ApJournal = Journal;
                            item.ApDate = DateTime.Now;
                            item.InvoiceAmount = model.InvoiceAmount;
                            item.InvoiceDate = model.InvoiceDate;
                            item.ApUser = HttpContext.Current.User.Identity.Name.ToUpper();
                            item.ApYear = Convert.ToDecimal(JournalYear);
                            item.ApMonth = Convert.ToDecimal(JournalMonth);
                            udb.Entry(item).State = System.Data.EntityState.Modified;
                            udb.SaveChanges();
                        }
                    }
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildApInvoiceDocument(string Supplier, string Grn, string Po, string Invoice, DateTime InvoiceDate, decimal InvoiceAmount, List<GrnDetail> grnDetail, ref decimal PermissableTax)
        {
            try
            {
                var Tax = (wdb.sp_GetStereoApTaxValue(Po.PadLeft(15, '0'), InvoiceDate).ToList());

                var highestRate = (from a in Tax select a).ToList().OrderByDescending(a => a.TaxRate).FirstOrDefault().TaxRate;

                var CountRate = (from a in Tax select a.TaxCode).Distinct().ToList();

                PermissableTax = 0.1m;

                var GrnValue = grnDetail.Sum(a => a.OrigGrnValue);

                var TaxAmount = Tax.Sum(a => a.TaxValue);
                if (CountRate.Count() > 1)
                {
                    //multiple tax code scenario
                    if (highestRate != 0)
                    {
                        //Tax at 15% of Invoice less calcuated tax
                        PermissableTax = (GrnValue * (highestRate / 100));
                        PermissableTax = PermissableTax - (decimal)TaxAmount;
                    }
                    else
                    {
                        PermissableTax = 0.1m;
                    }
                }
                else
                {
                    //Single Tax Code
                }

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Post AP Invoice Business Object");
                Document.Append("-->");
                Document.Append("<PostApInvoice xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"APSTINDOC.XSD\">");
                Document.Append("<Item>");
                //Document.Append("<eSignature>{12345678-1234-1234-1234-123456789012}</eSignature>");
                Document.Append("<Supplier><![CDATA[" + Supplier + "]]></Supplier>");
                Document.Append("<TransactionCode>I</TransactionCode>");
                Document.Append("<Branch />");
                Document.Append("<Invoice><![CDATA[" + Invoice + "]]></Invoice>");
                Document.Append("<EntryNumber />");
                Document.Append("<TransactionValue>" + InvoiceAmount + "</TransactionValue>");
                //Document.Append("<FreightCharge>2.00</FreightCharge>");
                //Document.Append("<MiscellaneousCharge />");
                Document.Append("<Notation><![CDATA[" + Invoice + "]]></Notation>");
                Document.Append("<TransactionReference><![CDATA[" + Invoice + "]]></TransactionReference>");
                Document.Append("<JournalNotation><![CDATA[" + Invoice + "]]></JournalNotation>");
                Document.Append("<DiscountBasis>P</DiscountBasis>");
                Document.Append("<DiscountableValue>0</DiscountableValue>");
                Document.Append("<DiscountPercentageValue>0</DiscountPercentageValue>");
                Document.Append("<InvoiceDate>" + InvoiceDate.ToString("yyyy-MM-dd") + "</InvoiceDate>");
                Document.Append("<DueDate />");
                Document.Append("<DiscountDate />");
                Document.Append("<ExchRateAtEntry />");
                Document.Append("<FixedExchRate />");
                Document.Append("<TaxBasis>E</TaxBasis>");
                Document.Append("<CalculateQstOnTax>Y</CalculateQstOnTax>");
                if (!string.IsNullOrEmpty(Tax.FirstOrDefault().TaxCode))
                {
                    Document.Append("<TaxCode>" + Tax.FirstOrDefault().TaxCode + "</TaxCode>");
                }
                else
                {
                    Document.Append("<TaxCode>E</TaxCode>");
                }
                //Document.Append("<TaxValue>" + Tax.Sum(a => a.TaxValue) + "</TaxValue>");
                Document.Append("<TaxValue></TaxValue>");
                Document.Append("<SecondTaxCode></SecondTaxCode>");
                Document.Append("<SecondTaxValue></SecondTaxValue>");
                Document.Append("<WithholdingTaxCode></WithholdingTaxCode>");
                Document.Append("<WithholdingTaxValue></WithholdingTaxValue>");
                Document.Append("<EcAcquisition>Y</EcAcquisition>");
                //Document.Append("<Nationality>PT</Nationality>");
                Document.Append("<AutoVoucherCreated>N</AutoVoucherCreated>");
                Document.Append("<POGLCode />");
                //Document.Append("<AnalysisEntry />");
                //Document.Append("<AnalysisLineEntry>");
                //Document.Append("<AnalysisCode1>Air</AnalysisCode1>");
                //Document.Append("<AnalysisCode2>Conf</AnalysisCode2>");
                //Document.Append("<AnalysisCode3>East</AnalysisCode3>");
                //Document.Append("<AnalysisCode4 />");
                //Document.Append("<AnalysisCode5 />");
                //Document.Append("<StartDate />");
                //Document.Append("<EndDate />");
                //Document.Append("<EntryAmount>100</EntryAmount>");
                //Document.Append("<Comment>Analysis entry details</Comment>");
                //Document.Append("</AnalysisLineEntry>");
                //Document.Append("<PpvAnalysisEntry />");
                //Document.Append("<PpvAnalysisLineEntry>");
                //Document.Append("<AnalysisCode1>Air</AnalysisCode1>");
                //Document.Append("<AnalysisCode2>Conf</AnalysisCode2>");
                //Document.Append("<AnalysisCode3>East</AnalysisCode3>");
                //Document.Append("<AnalysisCode4 />");
                //Document.Append("<AnalysisCode5 />");
                //Document.Append("<StartDate />");
                //Document.Append("<EndDate />");
                //Document.Append("<EntryAmount>100</EntryAmount>");
                //Document.Append("<Comment>Analysis entry details</Comment>");
                //Document.Append("</PpvAnalysisLineEntry>");

                foreach (var item in grnDetail)
                {
                    Document.Append("<GrnDetails>");
                    //Document.Append("<GrnDetailKey>0000001000000220I0000400001</GrnDetailKey>");
                    Document.Append("<GrnMatchType>O</GrnMatchType>");
                    Document.Append("<GrnMatchValue>" + item.CurGrnValue + "</GrnMatchValue>");
                    //Document.Append("<GrnPartialMatchQuantity>1</GrnPartialMatchQuantity>");
                    //Document.Append("<GrnPartialMatchValue>88.91</GrnPartialMatchValue>");
                    Document.Append("<GrnSupplier><![CDATA[" + item.Supplier + "]]></GrnSupplier>");
                    Document.Append("<GrnGrn>" + item.Grn + "</GrnGrn>");
                    Document.Append("<GrnFlag>" + item.GrnSource + "</GrnFlag>");
                    Document.Append("<GrnJournal>" + item.Journal + "</GrnJournal>");
                    Document.Append("<GrnEntry>" + item.JournalEntry + "</GrnEntry>");
                    Document.Append("</GrnDetails>");
                }

                //Document.Append("<LedgerDistribution>");
                //Document.Append("<LedgerCode>10-5500</LedgerCode>");
                //Document.Append("<PasswordForLedgerCode />");
                //Document.Append("<LedgerTaxCode>A</LedgerTaxCode>");
                //Document.Append("<LedgerSecondTaxCode>F</LedgerSecondTaxCode>");
                //Document.Append("<LedgerWithholdingTaxCode>F</LedgerWithholdingTaxCode>");
                //Document.Append("<LedgerWithholdingTaxExpenseType>G</LedgerWithholdingTaxExpenseType>");
                //Document.Append("<LedgerNotation>Ledger notation</LedgerNotation>");
                //Document.Append("<LedgerValue>2.00</LedgerValue>");
                //Document.Append("<TaxOnlyLine>N</TaxOnlyLine>");
                //Document.Append("<AnalysisEntry />");
                //Document.Append("<AnalysisLineEntry>");
                //Document.Append("<AnalysisCode1>Air</AnalysisCode1>");
                //Document.Append("<AnalysisCode2>Conf</AnalysisCode2>");
                //Document.Append("<AnalysisCode3>East</AnalysisCode3>");
                //Document.Append("<AnalysisCode4 />");
                //Document.Append("<AnalysisCode5 />");
                //Document.Append("<StartDate />");
                //Document.Append("<EndDate />");
                //Document.Append("<EntryAmount>100</EntryAmount>");
                //Document.Append("<Comment>Analysis entry details</Comment>");
                //Document.Append("</AnalysisLineEntry>");
                //Document.Append("</LedgerDistribution>");
                Document.Append("</Item>");

                Document.Append("</PostApInvoice>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildGrnMatchingParameter(int GrnYear, int GrnMonth, decimal permissableTax)
        {
            try
            {
                string PostingPeriod = "C";
                var PostPeriod = wdb.sp_GetAPPostingPeriod(GrnYear, GrnMonth).ToList();
                if (PostPeriod.Count > 0)
                {
                    PostingPeriod = PostPeriod.FirstOrDefault().PostingPeriod;
                }

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Post AP Invoice Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostApInvoice xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"APSTIN.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<PostingPeriod>" + PostingPeriod + "</PostingPeriod>");
                Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<PostZeroAdjustments>N</PostZeroAdjustments>");
                Parameter.Append("<DefaultDiscountDate />");
                Parameter.Append("<DefaultDueDate />");
                Parameter.Append("<LedgerDistributionCurrency>L</LedgerDistributionCurrency>");
                Parameter.Append("<AutomaticTaxCalculation>Y</AutomaticTaxCalculation>");
                //if(MultiTax == "Y")
                //{
                Parameter.Append("<PermissibleTaxVariance>" + Math.Round(permissableTax + 10, 2) + "</PermissibleTaxVariance>");
                //}

                Parameter.Append("<ApArContraTrx>N</ApArContraTrx>");
                Parameter.Append("<IgnoreAnalysis>N</IgnoreAnalysis>");
                Parameter.Append("<ApTaxReliefAsOfDate>");
                Parameter.Append("</ApTaxReliefAsOfDate>");
                Parameter.Append("<ApTaxReliefNoOfDays>");
                Parameter.Append("</ApTaxReliefNoOfDays>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostApInvoice>");

                return Parameter.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostGrnAp(StereoSystemGrnViewModel model)
        {
            try
            {
                model.PurchaseOrder = model.PurchaseOrder.PadLeft(15, '0');

                string Guid = objSyspro.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to Log in to Syspro.";
                }
                string GrnPost ="";
                if(model.PoLine != null)
                {
                    var result = (from a in model.PoLine.ToList() where a.Selected == true select a).ToList();

                    if (result != null)
                    {
                        if (result.ToList().Count > 0)
                        {
                            GrnPost = this.PostGrn(model, Guid);
                        }
                    }
                }


                if (string.IsNullOrEmpty(GrnPost))
                {
                    //Grn posted successfully. Do Ap
                    var Grn = (from a in wdb.mtStereoDetails where a.PurchaseOrder == model.PurchaseOrder && a.Invoice.ToUpper() == model.Invoice.ToUpper() select a.Grn).FirstOrDefault();
                    if (string.IsNullOrEmpty(Grn))
                    {
                        return "Failed to get Grn number!";
                    }
                    else
                    {
                        string ApPost = this.PostAp(model, Grn, Guid);
                        if (string.IsNullOrEmpty(ApPost))
                        {
                            //var Header = (from a in wdb.mtStereoHdrs where a.PurchaseOrder == model.PurchaseOrder select a.ReqNo).FirstOrDefault();
                            var Journal = (from a in wdb.mtStereoDetails where a.Grn == Grn select new { ApJournal = a.ApJournal, ApYear = a.ApYear, ApMonth = a.ApMonth }).FirstOrDefault();
                            if (string.IsNullOrEmpty(Grn))
                            {
                                return "Failed to get AP Journal number!";
                            }
                            return "Posted Successfully. Grn : " + Grn + " AP Journal : " + Journal.ApJournal + " AP Year : " + Journal.ApYear + " AP Month : " + Journal.ApMonth;
                        }
                        else
                        {
                            return "Grn was posted successfully Grn:" + Grn + ". Ap Posting failed " + ApPost;
                        }
                    }
                }
                else
                {
                    return GrnPost;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildPurchaseOrderParameter()
        {
            try
            {
                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("This is an example XML instance to demonstrate");
                Parameter.Append("use of the Purchase Order Transaction Posting Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostPurchaseOrders xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTOI.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                Parameter.Append("<AllowNonStockItems>Y</AllowNonStockItems>");
                Parameter.Append("<AllowZeroPrice>N</AllowZeroPrice>");
                Parameter.Append("<ValidateWorkingDays>N</ValidateWorkingDays>");
                Parameter.Append("<AllowPoWhenBlanketPo>N</AllowPoWhenBlanketPo>");
                Parameter.Append("<DefaultMemoCode>S</DefaultMemoCode>");
                Parameter.Append("<FixedExchangeRate>N</FixedExchangeRate>");
                Parameter.Append("<DefaultMemoDays>12</DefaultMemoDays>");
                Parameter.Append("<AllowBlankLedgerCode>Y</AllowBlankLedgerCode>");
                Parameter.Append("<DefaultDeliveryAddress/>");
                Parameter.Append("<CalcDueDate>N</CalcDueDate>");
                Parameter.Append("<InsertDangerousGoodsText>N</InsertDangerousGoodsText>");
                Parameter.Append("<InsertAdditionalPOText>N</InsertAdditionalPOText>");
                Parameter.Append("<OutputItemforDetailLines>Y</OutputItemforDetailLines>");
                Parameter.Append("<Status>1</Status>");
                Parameter.Append("<StatusInProcess/>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostPurchaseOrders>");

                return Parameter.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostPurchaseOrder(StereoSystemAddStereoViewModel model)
        {
            try
            {
                string Guid = objSyspro.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to Log in to Syspro.";
                }
                string Parameter, XmlOut, ErrorMessage;

                //Declaration
                StringBuilder Document = new StringBuilder();
                var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == model.ReqNo select a).FirstOrDefault();
                var Detail = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo select a).ToList();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the Purchase Order Transaction Posting Business Object");
                Document.Append("-->");
                Document.Append("<PostPurchaseOrders xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTOIDOC.XSD\">");
                if (Header != null)
                {
                    if (string.IsNullOrEmpty(Header.PurchaseOrder))
                    {
                        //CREATE PURCHASE ORDER
                        Document.Append(BuildPurchaseOrder(model));
                    }
                    else
                    {
                        //EDIT PURCHASE ORDER OR ADD LINE
                        Document.Append(BuildEditPurchaseOrder(model));
                    }
                }
                Document.Append("</PostPurchaseOrders>");
                Parameter = BuildPurchaseOrderParameter();
                XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "PORTOI");
                objSyspro.SysproLogoff(Guid);
                ErrorMessage = objSyspro.GetXmlErrors(XmlOut);

                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    string PurchaseOrder = objSyspro.GetXmlValue(XmlOut, "PurchaseOrder").Replace(@";", "").PadLeft(15, '0');
                    var Username = HttpContext.Current.User.Identity.Name.ToUpper();

                    //STEP 1 MARRY SYSPRO LINE TO STEREO LINE
                    var UnMatchedPoLine = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.SysproPurchaseOrderLine == null select a).ToList();
                    var MatchedPoLine = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.SysproPurchaseOrderLine != null select a).ToList();
                    var PlateCat = (from a in wdb.PorMasterHdr_ where a.PurchaseOrder == PurchaseOrder select a.PurchaseOrder).ToList();
                    var Settings = (from a in wdb.mtStereoSettings where a.SettingId == 1 select a).FirstOrDefault();

                    using (var hdb = new WarehouseManagementEntities(""))
                    {
                        foreach (var item in UnMatchedPoLine)
                        {
                            //UPDATE EACH LINE IN mtStereoDetail WITH CORRESPONDING SYSPRO LineNo
                            wdb.sp_StereoUpdatePurchaseOrderLine(PurchaseOrder.PadLeft(15, '0'), item.Line, item.ReqNo);

                            var SysproLine = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.Line == item.Line select a.SysproPurchaseOrderLine).FirstOrDefault();

                            //UPDATE PoPrice mtStereoDetail
                            mtStereoDetail po = new mtStereoDetail();
                            po = wdb.mtStereoDetails.Find(item.ReqNo, item.Line);
                            po.PoPrice = item.Width * item.Length * item.UnitPrice;
                            po.PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                            po.PoCreatedDate = DateTime.Now;
                   
                            po.SysproPurchaseOrderLine = SysproLine;
                            wdb.Entry(po).State = EntityState.Modified;
                            wdb.SaveChanges();

                            //ADD LENGTH AND WIDTH TO CUSTOM FORM
                            wdb.sp_StereoSaveCustomDetail(PurchaseOrder.PadLeft(15, '0'), SysproLine.Value, (decimal)item.Width, (decimal)item.Length);

                            //Remove Line From Catalogue
                            wdb.sp_StereoUpdatePurchaseOrderCatalogue(PurchaseOrder.PadLeft(15, '0'), item.Line);
                        }

                        //UPDATE ALL EXISTING PO LINES
                        foreach (var item in MatchedPoLine)
                        {
                            //UPDATE PoPrice mtStereoDetail
                            mtStereoDetail po = new mtStereoDetail();
                            po = wdb.mtStereoDetails.Find(item.ReqNo, item.Line);
                            po.PoPrice = item.Width * item.Length * item.UnitPrice;
                            po.PurchaseOrder = item.PurchaseOrder;
                            wdb.Entry(po).State = EntityState.Modified;
                            wdb.SaveChanges();

                            //ADD LENGTH AND WIDTH TO CUSTOM FORM
                            wdb.sp_StereoUpdateCustomDetail(PurchaseOrder.PadLeft(15, '0'), item.SysproPurchaseOrderLine, (decimal)item.Width,(decimal) item.Length);
                        }

                        mtStereoHdr flag = new mtStereoHdr();
                        flag = wdb.mtStereoHdrs.Find(model.ReqNo);
                        if(flag != null)
                        {
                            if (flag.PoCreated != "Y")
                            {
                                flag.PoCreated = "Y";
                                flag.PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                                hdb.Entry(flag).State = EntityState.Modified;
                                hdb.SaveChanges();
                            }
                        }  
                    }
                    //PurchaseOrder Doesnt Exist in Custom Form
                    if (PlateCat.Count == 0)
                    {
                       wdb.sp_StereoSaveCustomHdr(PurchaseOrder.PadLeft(15, '0'), Header.PlateCategory);
                    }
                    else
                    {
                        wdb.sp_StereoUpdateCustomHdr(PurchaseOrder.PadLeft(15, '0'), Header.PlateCategory);
                    }

                    return "Posting Complete, Purchase Order:" + PurchaseOrder;
                }
                return "Posting Failed: " + ErrorMessage;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildPurchaseOrder(StereoSystemAddStereoViewModel model)
        {
            var CheckPOCreated = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo select a.PurchaseOrder).FirstOrDefault();
            //Declaration
            StringBuilder Document = new StringBuilder();
            var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == model.ReqNo select a).FirstOrDefault();
            var Detail = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo select a).ToList();

            Document.Append("<Orders>");
            Document.Append("<OrderHeader>");
            Document.Append("<OrderActionType>A</OrderActionType>");

            Document.Append("<Supplier>" + Header.SupplierReference.ToString() + "</Supplier>");
            Document.Append("<ExchRateFixed/>");
            Document.Append("<ExchangeRate/>");
            Document.Append("<OrderType>L</OrderType>");
            if (Header.Taxable.Trim() == "Y")
            {
                Document.Append("<TaxStatus>N</TaxStatus>");
            }
            else
            {
                Document.Append("<TaxStatus>E</TaxStatus>");
            }
            Document.Append("<PaymentTerms/>");
            Document.Append("<InvoiceTerms>1</InvoiceTerms>");
            Document.Append("<CustomerPoNumber>" + Header.Quotation + "</CustomerPoNumber>");

            Document.Append("<OrderDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</OrderDate>");
            Document.Append("<DueDate>" + Convert.ToDateTime(Header.DateStereosRequired).ToString("yyyy-MM-dd") + "</DueDate>");
            Document.Append("<MemoDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</MemoDate>");
            Document.Append("<ApplyDueDateToLines>A</ApplyDueDateToLines>");
            Document.Append("<MemoCode/>");
            Document.Append("<Buyer>IS</Buyer>");

            Document.Append("<AutoVoucher></AutoVoucher>");
            Document.Append("<LanguageCode></LanguageCode>");
            Document.Append("<Warehouse>**</Warehouse>");
            Document.Append("<DiscountLessPlus/>");

            Document.Append("<ChgPOStatToReadyToPrint/>");
            Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
            Document.Append("<eSignature/>");
            Document.Append("</OrderHeader>");
            Document.Append("<OrderDetails>");
            foreach (var item in Detail)
            {
                decimal PoPrice = Convert.ToDecimal((item.Length * item.Width) * item.UnitPrice);
                Document.Append("<StockLine>");
                Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                Document.Append("<LineActionType>A</LineActionType>");
                Document.Append("<StockCode>" + item.StockCode.Trim().ToUpper() + "</StockCode>");
                Document.Append("<StockDescription>" + Header.PrintDescription+ "</StockDescription>");
                    //string PrintDescrip = Header.PrintDescription;
                    //int len = PrintDescrip.Length + item.Colour.Length+1;
                    //if (len > 50)
                    //{
                    //    PrintDescrip = PrintDescrip.Substring(0, PrintDescrip.Length - item.Colour.Length);
                    //    Document.Append("<StockDescription>" + PrintDescrip+"-"+item.Colour.ToUpper() + "</StockDescription>");
                    //}
                    //else
                    //{
                    //    Document.Append("<StockDescription>" + Header.PrintDescription + "-" + item.Colour.ToUpper() + "</StockDescription>");
                    //}

                Document.Append("<Warehouse>**</Warehouse>");
                Document.Append("<SupCatalogue>" + item.Line + "</SupCatalogue>");
                Document.Append("<OrderQty>" + item.Quantity + "</OrderQty>");
                Document.Append("<OrderUom>EA</OrderUom>");
                Document.Append("<Units/>");
                Document.Append("<Pieces/>");
                Document.Append("<PriceMethod>M</PriceMethod>");
                Document.Append("<SupplierContract/>");
                Document.Append("<SupplierContractReference/>");
                Document.Append("<Price>" + Math.Round(Convert.ToDecimal(PoPrice), 3) + "</Price>");
                Document.Append("<PriceUom>EA</PriceUom>");

                Document.Append("<Taxable>" + Header.Taxable + "</Taxable>");

                if (Header.Taxable.Trim() == "Y")
                {
                    Document.Append("<TaxCode>" + item.TaxCode.Trim() + "</TaxCode>");
                }

                Document.Append("<Job/>");
                Document.Append("<HierHead/>");
                Document.Append("<Section1/>");
                Document.Append("<Section2/>");
                Document.Append("<Section3/>");
                Document.Append("<Section4/>");
                Document.Append("<Version/>");
                Document.Append("<Release/>");
                Document.Append("<LatestDueDate/>");
                Document.Append("<OriginalDueDate/>");
                Document.Append("<RescheduleDueDate/>");
                Document.Append("<LedgerCode>" + item.GlCode.Trim() + "</LedgerCode>");
                Document.Append("<PasswordForLedgerCode/>");
                Document.Append("<SubcontractOp/>");
                Document.Append("<InspectionReqd/>");
                Document.Append("<ProductClass>OTH</ProductClass>");
                Document.Append("<NonsUnitMass/>");
                Document.Append("<NonsUnitVol/>");
                Document.Append("<BlanketPurchaseOrder/>");
                Document.Append("<AttachOrderToBPO/>");
                Document.Append("<WithholdingTaxExpenseType>G</WithholdingTaxExpenseType>");
                Document.Append("<NonStockedLine>Y</NonStockedLine>");
                Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
                Document.Append("</StockLine>");
            }

            Document.Append("<CommentLine>");
            Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
            Document.Append("<LineActionType>A</LineActionType>");
            Document.Append("<Comment>DELIVERED ON : " + Convert.ToDateTime(Header.DateStereosRequired).ToShortDateString() + "</Comment>");
            Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
            Document.Append("<DeleteAttachedCommentLines/>");
            Document.Append("<ChangeSingleCommentLine/>");
            Document.Append("</CommentLine>");

            Document.Append("<CommentLine>");
            Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
            Document.Append("<LineActionType>A</LineActionType>");
            Document.Append("<Comment>DEPT : STEREO PURCHASES    " + Detail.FirstOrDefault().GlCode + "</Comment>");
            Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
            Document.Append("<DeleteAttachedCommentLines/>");
            Document.Append("<ChangeSingleCommentLine/>");
            Document.Append("</CommentLine>");

            if (!string.IsNullOrEmpty(Header.Customer))
            {
                var Customer = wdb.sp_GetStereoCustomerName(Header.Customer).ToList();
                if (Customer.Count() > 0)
                {
                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment>CUSTOMER : " + Customer.FirstOrDefault().Name+ "</Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");
                }
            }
            if (!string.IsNullOrEmpty(Header.Quotation))
            {
                Document.Append("<CommentLine>");
                Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                Document.Append("<LineActionType>A</LineActionType>");
                Document.Append("<Comment>QUOTE NO: "+ Header.Quotation + "</Comment>");
                Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                Document.Append("<DeleteAttachedCommentLines/>");
                Document.Append("<ChangeSingleCommentLine/>");
                Document.Append("</CommentLine>");
            }
            var TermsCode = wdb.sp_GetSupplierTermsCode(Header.SupplierReference).FirstOrDefault();
            if (!string.IsNullOrEmpty(TermsCode.Description))
            {
                Document.Append("<CommentLine>");
                Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                Document.Append("<LineActionType>A</LineActionType>");
                Document.Append("<Comment>TERMS : " + TermsCode.Description + " FROM DATE OF STATEMENT</Comment>");
                Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                Document.Append("<DeleteAttachedCommentLines/>");
                Document.Append("<ChangeSingleCommentLine/>");
                Document.Append("</CommentLine>");
            }
            Document.Append("<CommentLine>");
            Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
            Document.Append("<LineActionType>A</LineActionType>");
            Document.Append("<Comment>NB : E-MAIL QUERIES OF THIS ORDER TO</Comment>");
            Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
            Document.Append("<DeleteAttachedCommentLines/>");
            Document.Append("<ChangeSingleCommentLine/>");
            Document.Append("</CommentLine>");

            var Email = (from a in wdb.mtStereoSuppliers where a.Supplier == Header.SupplierReference select a.Email).ToList();
            if (Email.Count() > 0)
            {
                Document.Append("<CommentLine>");
                Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                Document.Append("<LineActionType>A</LineActionType>");
                Document.Append("<Comment>     " + Email.FirstOrDefault().Trim() + "</Comment>");
                Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                Document.Append("<DeleteAttachedCommentLines/>");
                Document.Append("<ChangeSingleCommentLine/>");
                Document.Append("</CommentLine>");
            }

            Document.Append("</OrderDetails>");
            Document.Append("</Orders>");

            return Document.ToString();
        }

        public string BuildEditPurchaseOrder(StereoSystemAddStereoViewModel model)
        {
            var CheckPOCreated = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo select a.PurchaseOrder).FirstOrDefault();
            //Declaration
            StringBuilder Document = new StringBuilder();
            var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == model.ReqNo select a).FirstOrDefault();
            var Detail = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo select a).ToList();

            Document.Append("<Orders>");
            Document.Append("<OrderHeader>");
            Document.Append("<OrderActionType>C</OrderActionType>");
            Document.Append("<ExchRateFixed/>");
            Document.Append("<ExchangeRate/>");
            Document.Append("<OrderType>L</OrderType>");

            if (Header.Taxable.Trim() == "Y")
            {
                Document.Append("<TaxStatus>N</TaxStatus>");
            }
            else
            {
                Document.Append("<TaxStatus>E</TaxStatus>");
            }
            Document.Append("<PaymentTerms/>");
            Document.Append("<InvoiceTerms>1</InvoiceTerms>");
            Document.Append("<CustomerPoNumber>" + Header.Quotation + "</CustomerPoNumber>");
            Document.Append("<ApplyDueDateToLines>A</ApplyDueDateToLines>");
            Document.Append("<MemoCode/>");
            Document.Append("<Buyer>IS</Buyer>");
            Document.Append("<AutoVoucher></AutoVoucher>");
            Document.Append("<LanguageCode></LanguageCode>");
            Document.Append("<Warehouse>**</Warehouse>");
            Document.Append("<DiscountLessPlus/>");
            Document.Append("<PurchaseOrder>" + Header.PurchaseOrder + "</PurchaseOrder>");
            Document.Append("<ChgPOStatToReadyToPrint/>");
            Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
            Document.Append("<eSignature/>");
            Document.Append("</OrderHeader>");
            if (Header.PurchaseOrder != "")
            {
                //Deletes all comments
                var CommentLine = wdb.sp_GetStereoPoCommentLines(Header.PurchaseOrder).ToList();
                foreach (var line in CommentLine)
                {
                    Document.Append("<OrderDetails>");
                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine>" + line.Value + "</PurchaseOrderLine>");
                    Document.Append("<LineActionType>D</LineActionType>");
                    Document.Append("<Comment></Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");
                    Document.Append("</OrderDetails>");
                }

            }
            Document.Append("<OrderDetails>");
            foreach (var item in Detail)
            {
                var CheckGrnComplete = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.Line == item.Line && a.Grn != "" select a).ToList();
                if (CheckGrnComplete.Count == 0)
                {
                    //ADD OR AMMEND LINE
                    var CheckLineExists = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.Line == item.Line select a.SysproPurchaseOrderLine).FirstOrDefault();
                    decimal PoPrice = Convert.ToDecimal((item.Length * item.Width) * item.UnitPrice);
                    Document.Append("<StockLine>");
                    if (CheckLineExists.ToString() == "")
                    {
                        //ADD NEW LINE
                        Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                        Document.Append("<LineActionType>A</LineActionType>");
                        Document.Append("<StockCode>" + item.StockCode.Trim().ToUpper() + "</StockCode>");
                        Document.Append("<StockDescription>" + Header.PrintDescription + "</StockDescription>");
                        Document.Append("<Warehouse>**</Warehouse>");
                        Document.Append("<SupCatalogue>" + item.Line + "</SupCatalogue>");
                        Document.Append("<PriceMethod>M</PriceMethod>");
                        Document.Append("<LedgerCode>" + item.GlCode.Trim() + "</LedgerCode>");
                    }
                    else
                    {
                        //UPDATE EXISTING
                       
                        Document.Append("<StockDescription>" + Header.PrintDescription + "</StockDescription>");
                        Document.Append("<PurchaseOrderLine>" + item.SysproPurchaseOrderLine + "</PurchaseOrderLine>");
                        Document.Append("<LineActionType>C</LineActionType>");
                    }

                    Document.Append("<OrderQty>" + item.Quantity + "</OrderQty>");
                    Document.Append("<OrderUom>EA</OrderUom>");
                    Document.Append("<Pieces/>");
                    Document.Append("<SupplierContract/>");
                    Document.Append("<Price>" + Math.Round(Convert.ToDecimal(PoPrice), 3) + "</Price>");
                    Document.Append("<PriceUom>EA</PriceUom>");

                    Document.Append("<Taxable>" + Header.Taxable + "</Taxable>");
                    if (Header.Taxable.Trim() == "Y")
                    {
                        Document.Append("<TaxCode>" + item.TaxCode.Trim() + "</TaxCode>");
                    }
                    Document.Append("<Job/>");
                    Document.Append("<HierHead/>");
                    Document.Append("<Section1/>");
                    Document.Append("<Section2/>");
                    Document.Append("<Section3/>");
                    Document.Append("<Section4/>");
                    Document.Append("<Version/>");
                    Document.Append("<Release/>");
                    Document.Append("<LatestDueDate/>");
                    Document.Append("<OriginalDueDate/>");
                    Document.Append("<RescheduleDueDate/>");
                    Document.Append("<PasswordForLedgerCode/>");
                    Document.Append("<SubcontractOp/>");
                    Document.Append("<InspectionReqd/>");
                    Document.Append("<ProductClass>OTH</ProductClass>");
                    Document.Append("<NonsUnitMass/>");
                    Document.Append("<NonsUnitVol/>");
                    Document.Append("<BlanketPurchaseOrder/>");
                    Document.Append("<AttachOrderToBPO/>");
                    Document.Append("<WithholdingTaxExpenseType>G</WithholdingTaxExpenseType>");
                    Document.Append("<NonStockedLine>Y</NonStockedLine>");
                    Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
                    Document.Append("</StockLine>");    
                }    
            }
                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment>DELIVERED ON : " + Convert.ToDateTime(Header.DateStereosRequired).ToShortDateString() + "</Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");

                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment>DEPT : STEREO PURCHASES    " + Detail.FirstOrDefault().GlCode + "</Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");

                    if (!string.IsNullOrEmpty(Header.Customer))
                    {
                        var Customer = wdb.sp_GetStereoCustomerName(Header.Customer).ToList();
                        if (Customer.Count() > 0)
                        {
                            Document.Append("<CommentLine>");
                            Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                            Document.Append("<LineActionType>A</LineActionType>");
                            Document.Append("<Comment>CUSTOMER : " + Customer.FirstOrDefault().Name + "</Comment>");
                            Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                            Document.Append("<DeleteAttachedCommentLines/>");
                            Document.Append("<ChangeSingleCommentLine/>");
                            Document.Append("</CommentLine>");
                        }
                    }
                    if (!string.IsNullOrEmpty(Header.Quotation))
                    {
                        Document.Append("<CommentLine>");
                        Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                        Document.Append("<LineActionType>A</LineActionType>");
                        Document.Append("<Comment>QUOTE NO: " + Header.Quotation + "</Comment>");
                        Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                        Document.Append("<DeleteAttachedCommentLines/>");
                        Document.Append("<ChangeSingleCommentLine/>");
                        Document.Append("</CommentLine>");
                    }
                    var TermsCode = wdb.sp_GetSupplierTermsCode(Header.SupplierReference).FirstOrDefault();
                    if (!string.IsNullOrEmpty(TermsCode.Description))
                    {
                        Document.Append("<CommentLine>");
                        Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                        Document.Append("<LineActionType>A</LineActionType>");
                        Document.Append("<Comment>TERMS : " + TermsCode.Description + " FROM DATE OF STATEMENT</Comment>");
                        Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                        Document.Append("<DeleteAttachedCommentLines/>");
                        Document.Append("<ChangeSingleCommentLine/>");
                        Document.Append("</CommentLine>");
                    }
                    Document.Append("<CommentLine>");
                    Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                    Document.Append("<LineActionType>A</LineActionType>");
                    Document.Append("<Comment>NB : E-MAIL QUERIES OF THIS ORDER TO</Comment>");
                    Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                    Document.Append("<DeleteAttachedCommentLines/>");
                    Document.Append("<ChangeSingleCommentLine/>");
                    Document.Append("</CommentLine>");

                    var Email = (from a in wdb.mtStereoSuppliers where a.Supplier == Header.SupplierReference select a.Email).ToList();
                    if (Email.Count() > 0)
                    {
                        Document.Append("<CommentLine>");
                        Document.Append("<PurchaseOrderLine></PurchaseOrderLine>");
                        Document.Append("<LineActionType>A</LineActionType>");
                        Document.Append("<Comment>     " + Email.FirstOrDefault().Trim() + "</Comment>");
                        Document.Append("<AttachedToStkLineNumber></AttachedToStkLineNumber>");
                        Document.Append("<DeleteAttachedCommentLines/>");
                        Document.Append("<ChangeSingleCommentLine/>");
                        Document.Append("</CommentLine>");
                    }
                        Document.Append("</OrderDetails>");
                        Document.Append("</Orders>");
            

            return Document.ToString();
        }

        public string DeleteLine(int ReqNo, int Line)
        {
            try
            {
                var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == ReqNo select a).FirstOrDefault();
                var Detail = (from a in wdb.mtStereoDetails where a.ReqNo == ReqNo && a.Line == Line select a).FirstOrDefault();

                var SysproLine = (from a in wdb.PorMasterDetails where a.PurchaseOrder == Detail.PurchaseOrder && a.Line == Detail.SysproPurchaseOrderLine select a).ToList();
                if (SysproLine.Count > 0)
                {
                    string Guid = objSyspro.SysproLogin();
                    if (string.IsNullOrEmpty(Guid))
                    {
                        return "Failed to Log in to Syspro.";
                    }
                    string Parameter, XmlOut, ErrorMessage;

                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("This is an example XML instance to demonstrate");
                    Document.Append("use of the Purchase Order Transaction Posting Business Object");
                    Document.Append("-->");
                    Document.Append("<PostPurchaseOrders xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTOIDOC.XSD\">");

                    //build bo
                    Document.Append(BuildDeleteLinePurchaseOrder(ReqNo, Line));

                    Document.Append("</PostPurchaseOrders>");

                    Parameter = BuildPurchaseOrderParameter();

                    XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "PORTOI");
                    objSyspro.SysproLogoff(Guid);
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        //DELETE CUSTOM FORM LENGH AND WIDTH

                        var sys = wdb.PorMasterDetail_.Find(Detail.PurchaseOrder.PadLeft(15, '0'), Detail.SysproPurchaseOrderLine);
                        wdb.PorMasterDetail_.Remove(sys);
                        wdb.SaveChanges();
                    }
                    else { return "Delete Failed: " + ErrorMessage; }
                }

                //DELETE FROM mtStereoDetail
                var det = wdb.mtStereoDetails.Find(ReqNo, Line);
                wdb.mtStereoDetails.Remove(det);
                wdb.SaveChanges();
                return " Line Deleted Successfully";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildDeleteLinePurchaseOrder(int ReqNo, int Line)
        {
            var Detail = (from a in wdb.mtStereoDetails where a.ReqNo == ReqNo && a.Line == Line select a).ToList();
            var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == ReqNo select a).ToList();

            //Declaration
            StringBuilder Document = new StringBuilder();

            Document.Append("<Orders>");
            Document.Append("<OrderHeader>");
            Document.Append("<OrderActionType>C</OrderActionType>");
            //Document.Append("<Supplier>" + Header.FirstOrDefault().SupplierReference + "</Supplier>");
            Document.Append("<ExchRateFixed/>");
            Document.Append("<ExchangeRate/>");
            Document.Append("<OrderType>L</OrderType>");
            Document.Append("<PaymentTerms/>");
            Document.Append("<CustomerPoNumber/>");
            Document.Append("<ApplyDueDateToLines>A</ApplyDueDateToLines>");
            Document.Append("<MemoCode/>");
            Document.Append("<Buyer>IS</Buyer>");
            Document.Append("<AutoVoucher></AutoVoucher>");
            Document.Append("<LanguageCode></LanguageCode>");
            Document.Append("<Warehouse>**</Warehouse>");
            Document.Append("<DiscountLessPlus/>");
            Document.Append("<PurchaseOrder>" + Detail.FirstOrDefault().PurchaseOrder.PadLeft(15, '0') + "</PurchaseOrder>");
            Document.Append("<ChgPOStatToReadyToPrint/>");
            Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
            Document.Append("<eSignature/>");
            Document.Append("</OrderHeader>");
            Document.Append("<OrderDetails>");
            Document.Append("<StockLine>");

            //DELETE LINE
            Document.Append("<PurchaseOrderLine>" + Detail.FirstOrDefault().SysproPurchaseOrderLine + "</PurchaseOrderLine>");
            Document.Append("<LineActionType>D</LineActionType>");

            Document.Append("<OrderQty></OrderQty>");
            Document.Append("<Pieces/>");
            Document.Append("<SupplierContract/>");
            Document.Append("<Price></Price>");
            Document.Append("<Taxable></Taxable>");
            Document.Append("<TaxCode></TaxCode>");
            Document.Append("<Job/>");
            Document.Append("<HierHead/>");
            Document.Append("<Section1/>");
            Document.Append("<Section2/>");
            Document.Append("<Section3/>");
            Document.Append("<Section4/>");
            Document.Append("<Version/>");
            Document.Append("<Release/>");
            Document.Append("<LatestDueDate/>");
            Document.Append("<OriginalDueDate/>");
            Document.Append("<RescheduleDueDate/>");
            Document.Append("<PasswordForLedgerCode/>");
            Document.Append("<SubcontractOp/>");
            Document.Append("<InspectionReqd/>");
            Document.Append("<ProductClass>OTH</ProductClass>");
            Document.Append("<NonsUnitMass/>");
            Document.Append("<NonsUnitVol/>");
            Document.Append("<BlanketPurchaseOrder/>");
            Document.Append("<AttachOrderToBPO/>");
            Document.Append("<WithholdingTaxExpenseType>G</WithholdingTaxExpenseType>");
            Document.Append("<NonStockedLine>Y</NonStockedLine>");
            Document.Append("<IncludeInMrp>Y</IncludeInMrp>");
            Document.Append("</StockLine>");

            Document.Append("</OrderDetails>");
            Document.Append("</Orders>");

            return Document.ToString();
        }
    }
}