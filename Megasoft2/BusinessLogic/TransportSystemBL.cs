using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class TransportSystemBL
    {
        SysproCore objSys = new SysproCore();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        public string BuildGrnDocument(string PurchaseOrder, string Invoice)
        {
            try
            {

                var result = wdb.sp_GetTransporterPoLines(PurchaseOrder).ToList();
                result = (from a in result where a.OutstandingQty > 0 select a).ToList();
                if(result.Count > 0)
                {

                    result = (from a in result where a.LineType != "6" select a).ToList();

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

                    foreach(var item in result)
                    {
                        Document.Append("<Item>");
                        Document.Append("<Receipt>");
                        Document.Append("<Journal />");
                        Document.Append("<PurchaseOrder>" + result.FirstOrDefault().PurchaseOrder + "</PurchaseOrder>");
                        Document.Append("<PurchaseOrderLine>" + item.Line + "</PurchaseOrderLine>");
                        Document.Append("<Warehouse />");
                        //Document.Append("<StockCode>" + StockDesc + "</StockCode>");
                        Document.Append("<Quantity>" + item.OutstandingQty + "</Quantity>");
                        Document.Append("<UnitOfMeasure />");
                        Document.Append("<Units />");
                        Document.Append("<Pieces />");
                        Document.Append("<DeliveryNote><![CDATA[" + Invoice + "]]></DeliveryNote>");
                        Document.Append("<Cost />");
                        Document.Append("<CostBasis>P</CostBasis>");
                        Document.Append("<SwitchOnGRNMatching>N</SwitchOnGRNMatching>");
                        //Document.Append("<GRNNumber></GRNNumber>");

                        Document.Append("<Reference>" + Invoice + "</Reference>");
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


        public string BuildApInvoiceDocument(string Supplier, string Grn, string Po, string Invoice, DateTime InvoiceDate, decimal InvoiceAmount, List<GrnDetail> grnDetail, ref decimal PermissableTax)
        {
            try
            {

                var Tax = (wdb.sp_GetTransApTaxValue(Po.PadLeft(15, '0'), InvoiceDate).ToList());


                var highestRate = (from a in Tax select a).ToList().OrderByDescending(a => a.TaxRate).FirstOrDefault().TaxRate;


                var CountRate = (from a in Tax select a.TaxCode).Distinct().ToList();

                PermissableTax = 0.1m;

                var GrnValue = grnDetail.Sum(a => a.OrigGrnValue);

                var TaxAmount = Tax.Sum(a => a.TaxValue);
                if(CountRate.Count() > 1)
                {
                    //multiple tax code scenario
                    if(highestRate != 0)
                    {
                        //Tax at 15% of Invoice less calcuated tax
                        PermissableTax = (GrnValue * (highestRate/100));
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
                Document.Append("<TaxValue>" + Tax.Sum(a => a.TaxValue) + "</TaxValue>");
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

                foreach(var item in grnDetail)
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
            catch(Exception ex)
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
                Parameter.Append("<AutomaticTaxCalculation>N</AutomaticTaxCalculation>");
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


        public string PostGrn(TransportSystemWaybillEntryViewModel model, string Guid)
        {
            try
            {
                string Document = this.BuildGrnDocument(model.PurchaseOrder, model.Invoice);
                if(Document == "")
                {
                    return "";
                }
                string Parameter = this.BuildGrnParameter();

                string XmlOut = objSys.SysproPost(Guid, Parameter, Document, "PORTOR");

                string ErrorMessage = objSys.GetXmlErrors(XmlOut);
                if(!string.IsNullOrEmpty(ErrorMessage))
                {
                    return "Error : " + ErrorMessage;
                }
                else
                {
                    string Grn = objSys.GetFirstXmlValue(XmlOut, "Grn");
                    using(var udb = new WarehouseManagementEntities(""))
                    {
                        var result = (from a in udb.mtTransportWaybillDetails where a.PurchaseOrder == model.PurchaseOrder select a).ToList();
                        foreach(var item in result)
                        {
                            item.Grn = Grn;
                            item.GrnDate = DateTime.Now;
                            item.GrnUser = HttpContext.Current.User.Identity.Name.ToUpper();
                            item.Invoice = model.Invoice;
                            item.InvoiceDate = model.InvoiceDate;
                            item.InvoiceAmount = model.InvoiceAmount;
                            udb.Entry(item).State = System.Data.EntityState.Modified;
                            udb.SaveChanges();
                        }
                    }
                    return "";
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostAp(TransportSystemWaybillEntryViewModel model, string Guid, string Grn)
        {
            try
            {

                Grn = Grn.PadLeft(15, '0');
                var result = (from a in wdb.GrnDetails.AsNoTracking() where a.Grn == Grn && a.Supplier == model.Supplier select a).ToList();

                decimal PermissableTax = 0;
                string Document = this.BuildApInvoiceDocument(model.Supplier, Grn, model.PurchaseOrder, model.Invoice, model.InvoiceDate, model.InvoiceAmount, result, ref PermissableTax);




                string Parameter = this.BuildGrnMatchingParameter((int)result.FirstOrDefault().GrnYear, (int)result.FirstOrDefault().GrnMonth, PermissableTax);

                string XmlOut = objSys.SysproPost(Guid, Parameter, Document, "APSTIN");

                string ErrorMessage = objSys.GetXmlErrors(XmlOut);
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return "Error : " + ErrorMessage;
                }
                else
                {
                    string Journal = objSys.GetFirstXmlValue(XmlOut, "Journal");
                    string JournalYear = objSys.GetFirstXmlValue(XmlOut, "TrnYear");
                    string JournalMonth = objSys.GetFirstXmlValue(XmlOut, "TrnMonth");
                    using (var udb = new WarehouseManagementEntities(""))
                    {
                        var po = (from a in udb.mtTransportWaybillDetails where a.PurchaseOrder == model.PurchaseOrder select a).ToList();
                        foreach (var item in po)
                        {
                            item.ApJournal = Journal;
                            item.ApDate = DateTime.Now;
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

        public string PostGrnAp(TransportSystemWaybillEntryViewModel model)
        {
            try
            {
                string Guid = objSys.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to Log in to Syspro.";
                }
                string GrnPost = this.PostGrn(model, Guid);
                if(string.IsNullOrEmpty(GrnPost))
                {
                    //Grn posted successfully. Do Ap
                    var Grn = (from a in wdb.mtTransportWaybillDetails where a.PurchaseOrder == model.PurchaseOrder select a.Grn).FirstOrDefault();
                    if(string.IsNullOrEmpty(Grn))
                    {
                        return "Failed to get Grn number!";
                    }
                    else
                    {
                        string ApPost = this.PostAp(model, Guid, Grn);
                        if(string.IsNullOrEmpty(ApPost))
                        {
                            var Journal = (from a in wdb.mtTransportWaybillDetails where a.PurchaseOrder == model.PurchaseOrder select new { ApJournal = a.ApJournal, ApYear = a.ApYear, ApMonth = a.ApMonth }).FirstOrDefault();
                            if (string.IsNullOrEmpty(Grn))
                            {
                                return "Failed to get AP Journal number!";
                            }
                            return "Posted Successfully. Grn : " + Grn + " AP Journal : " + Journal.ApJournal + " AP Year : " + Journal.ApYear + " AP Month : " + Journal.ApMonth;
                        }
                        else
                        {
                            return ApPost;
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


        public string BuildGlDocument(TransportSystemPvJournalViewModel model)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the GL Journal Posting  Business Object");
                Document.Append("-->");
                Document.Append("<PostGlJournal xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"GENTJLDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<JournalType>N</JournalType>");
                Document.Append("<JournalNumber />");
                Document.Append("<AuthorizeJournal>A</AuthorizeJournal>");
                Document.Append("<PostJournal>Y</PostJournal>");
                Document.Append("<CancelJournal>N</CancelJournal>");
                Document.Append("<HoldJournal>N</HoldJournal>");
                Document.Append("<JournalDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</JournalDate>");
                Document.Append("<JournalReference><![CDATA[" + model.Reference + "]]></JournalReference>");
                Document.Append("<Notation></Notation>");
                Document.Append("<PostingPeriod>" + model.Year + "</PostingPeriod>");
                Document.Append("<PostingYear>" + model.Month + "</PostingYear>");
                Document.Append("<esignature />");
                Document.Append("<SourceCode />");
                Document.Append("<Currency />");
                Document.Append("<AuditorsPassword />");
                Document.Append("<Source></Source>");
                Document.Append("<Type></Type>");
                //Document.Append("<Company>" + Properties.Settings.Default.CompanyID + "</Company>");

                foreach (var item in model.Detail)
                {
                    Document.Append("<JournalDetails>");
                    Document.Append("<EntryNumber />");
                    Document.Append("<LedgerCode><![CDATA[" + item.GLCode + "]]></LedgerCode>");
                    Document.Append("<LedgerCodePassword />");
                    Document.Append("<Date />");
                    Document.Append("<Reference><![CDATA[" + model.Reference + "]]></Reference>");
                    Document.Append("<EntryAmount>" + item.Amount + "</EntryAmount>");
                    Document.Append("<DeleteEntry />");
                    //Document.Append("<Comment><![CDATA[" + item.VIPComment + "]]></Comment>");
                    Document.Append("<AnalysisEntry />");
                    Document.Append("<InterCompanyDetail>N</InterCompanyDetail>");
                    Document.Append("<InterCompanyId />");
                    Document.Append("<InterCompanyDebit />");
                    Document.Append("<InterCompanyDebitPass />");
                    Document.Append("<InterCompanyCredit />");
                    Document.Append("<InterCompanyCreditPass />");
                    //Document.Append("<SourceAp>H</SourceAp>");
                    Document.Append("<SourceJournal>");
                    Document.Append("</SourceJournal>");
                    Document.Append("<AdditionalReference>");
                    Document.Append("</AdditionalReference>");
                    Document.Append("<CommitmentFlag>");
                    Document.Append("</CommitmentFlag>");
                    Document.Append("<CurrencyInfo>");
                    Document.Append("</CurrencyInfo>");
                    Document.Append("<TransactionDate>");
                    Document.Append("</TransactionDate>");
                    Document.Append("<ModifiedFlag>");
                    Document.Append("</ModifiedFlag>");
                    //if (item.AnalysisRequired == "Y")
                    //{
                    //    Document.Append("<AnalysisLineEntry>");
                    //    Document.Append("<AnalysisCode1><![CDATA[" + item.AnalysisCode + "]]></AnalysisCode1>");
                    //    Document.Append("<AnalysisCode2></AnalysisCode2>");
                    //    Document.Append("<AnalysisCode3></AnalysisCode3>");
                    //    Document.Append("<AnalysisCode4>");
                    //    Document.Append("</AnalysisCode4>");
                    //    Document.Append("<AnalysisCode5>");
                    //    Document.Append("</AnalysisCode5>");
                    //    Document.Append("<StartDate>");
                    //    Document.Append("</StartDate>");
                    //    Document.Append("<EndDate>");
                    //    Document.Append("</EndDate>");
                    //    Document.Append("<EntryAmount>" + item.EntryValue + "</EntryAmount>");
                    //    Document.Append("<Comment></Comment>");
                    //    Document.Append("</AnalysisLineEntry>");
                    //}

                    Document.Append("</JournalDetails>");
                }

                Document.Append("</Item>");
                Document.Append("</PostGlJournal>");

                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildGlParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the GL Journal Posting  Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostGlJournal xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"GENTJL.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ActionType>A</ActionType>");
            Parameter.Append("<ValidatePasswords>Y</ValidatePasswords>");
            Parameter.Append("<DeleteEntriesonMaint>N</DeleteEntriesonMaint>");
            Parameter.Append("<IgnoreAnalysis>N</IgnoreAnalysis>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostGlJournal>");

            return Parameter.ToString();

        }
        public string BuildPoMaintenanceDoc(TransportPoMaintenanceViewModel model)
        {
            try
            {

                var LedgerCode = (from a in wdb.mtTransporters where a.Transporter == model.Supplier select a.GLCode).FirstOrDefault();


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
                Document.Append("<Orders>");
                Document.Append("<OrderHeader>");
                Document.Append("<OrderActionType>C</OrderActionType>");
                Document.Append("<PurchaseOrder>" + model.PurchaseOrder + "</PurchaseOrder>");
                Document.Append("</OrderHeader>");
                Document.Append("<OrderDetails>");
                foreach (var line in model.Detail)
                {
                    if (line.TrackId == 0)
                    {
                        Document.Append("<StockLine>");
                        Document.Append("<PurchaseOrderLine>" + line.Line + "</PurchaseOrderLine>");
                        Document.Append("<LineActionType>C</LineActionType>");
                        Document.Append("<Price>" + line.MPrice + "</Price>");
                        Document.Append("<TaxCode>" + line.MTaxCode + "</TaxCode>");
                        Document.Append("</StockLine>");
                    }
                    else
                    {

                        var WaybillDet = (from a in wdb.mtTransportWaybillDetails where a.TrackId == line.TrackId && a.Waybill == line.Waybill && a.DispatchNote == line.DispatchNote && a.DispatchNoteLine == line.DispatchNoteLine select a).FirstOrDefault();


                        string Description = "";
                        if (WaybillDet.Weight == 0)
                        {
                            Description = WaybillDet.Customer + '-' + WaybillDet.Town;
                        }
                        else
                        {
                            Description = WaybillDet.Customer + '-' + WaybillDet.Town + "-" + WaybillDet.Weight.ToString() + "kg";
                        }

                        Document.Append("<StockLine>");
                        Document.Append("<PurchaseOrderLine>" + line.Line + "</PurchaseOrderLine>");
                        Document.Append("<LineActionType>A</LineActionType>");
                        Document.Append("<StockCode><![CDATA[" + line.Waybill.ToString().Trim() + "]]></StockCode>");
                        Document.Append("<StockDescription><![CDATA[" + Description + "]]></StockDescription>");
                        Document.Append("<Warehouse>**</Warehouse>");
                        Document.Append("<SupCatalogue>" + WaybillDet.SeqNo + "</SupCatalogue>");
                        Document.Append("<OrderQty>1</OrderQty>");
                        Document.Append("<OrderUom>EA</OrderUom>");
                        Document.Append("<PriceMethod>M</PriceMethod>");
                        Document.Append("<Price>" + line.MPrice + "</Price>");
                        Document.Append("<PriceUom>EA</PriceUom>");
                        Document.Append("<TaxCode>" + line.MTaxCode + "</TaxCode>");
                        Document.Append("<LedgerCode><![CDATA[" + LedgerCode.ToString() + "]]></LedgerCode>");
                        Document.Append("<ProductClass>0017</ProductClass>");
                        Document.Append("</StockLine>");
                    }
                }




                //Document.Append("<CommentLine>");
                //Document.Append("<PurchaseOrderLine>2</PurchaseOrderLine>");
                //Document.Append("<LineActionType>A</LineActionType>");
                //Document.Append("<Comment>Ensure saddle is color coded</Comment>");
                //Document.Append("<AttachedToStkLineNumber>1</AttachedToStkLineNumber>");
                //Document.Append("<DeleteAttachedCommentLines />");
                //Document.Append("<ChangeSingleCommentLine />");
                //Document.Append("</CommentLine>");
                Document.Append("</OrderDetails>");
                Document.Append("</Orders>");
                Document.Append("</PostPurchaseOrders>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string PostMaintenance(TransportPoMaintenanceViewModel model)
        {
            try
            {
                string Guid = objSys.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to Log in to Syspro.";
                }
                string Document, Parameter, XmlOut, ErrorMessage;

                if (model.Detail.Count > 0)
                {
                    Document = this.BuildPoMaintenanceDoc(model);
                    Parameter = this.BuildPurchaseOrderParameter();
                    XmlOut = objSys.SysproPost(Guid, Parameter, Document, "PORTOI");
                    objSys.SysproLogoff(Guid);
                    ErrorMessage = objSys.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        foreach (var line in model.Detail)
                        {
                            if (line.TrackId != 0)
                            {
                                mtTransportWaybillDetail det = new mtTransportWaybillDetail();
                                det = wdb.mtTransportWaybillDetails.Find(Convert.ToInt32(line.TrackId), line.Waybill.ToString().Trim(), line.DispatchNote.ToString().Trim(), Convert.ToInt32(line.DispatchNoteLine));
                                wdb.sp_TransUpdateWaybill(line.TrackId, line.Waybill, line.DispatchNote, Convert.ToInt32(line.DispatchNoteLine), model.PurchaseOrder.PadLeft(15, '0'), 0, HttpContext.Current.User.Identity.Name.ToUpper());
                                wdb.sp_TransUpdatePurchaseOrderLine(model.PurchaseOrder.PadLeft(15, '0'), det.SeqNo);
                            }
                        }
                        return "Purchase Order updated successfully.";
                    }
                    else
                    {
                        return ErrorMessage;
                    }
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
                Parameter.Append("<AllowZeroPrice>Y</AllowZeroPrice>");
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
                Parameter.Append("<OutputItemforDetailLines>N</OutputItemforDetailLines>");
                //Parameter.Append("<Status>1</Status>");
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


        public string BuildDeleteineDoc(string PurchaseOrder, int Line)
        {
            try
            {
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
                Document.Append("<Orders>");
                Document.Append("<OrderHeader>");
                Document.Append("<OrderActionType>C</OrderActionType>");
                Document.Append("<PurchaseOrder>" + PurchaseOrder + "</PurchaseOrder>");
                Document.Append("</OrderHeader>");
                Document.Append("<OrderDetails>");

                Document.Append("<StockLine>");
                Document.Append("<PurchaseOrderLine>" + Line + "</PurchaseOrderLine>");
                Document.Append("<LineActionType>D</LineActionType>");
                Document.Append("</StockLine>");

                Document.Append("</OrderDetails>");
                Document.Append("</Orders>");
                Document.Append("</PostPurchaseOrders>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string PostDelete(string PurchaseOrder, int Line, int SeqNo)
        {
            try
            {
                string Guid = objSys.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to Log in to Syspro.";
                }
                string Document, Parameter, XmlOut, ErrorMessage;

                Document = this.BuildDeleteineDoc(PurchaseOrder, Line);
                Parameter = this.BuildPurchaseOrderParameter();
                XmlOut = objSys.SysproPost(Guid, Parameter, Document, "PORTOI");
                objSys.SysproLogoff(Guid);
                ErrorMessage = objSys.GetXmlErrors(XmlOut);
                if (string.IsNullOrEmpty(ErrorMessage))
                {

                    using (var ddb = new WarehouseManagementEntities(""))
                    {
                        var podel = (from a in ddb.mtTransportWaybillDetails where a.PurchaseOrder == PurchaseOrder && a.PurchaseOrderLine == Line && a.SeqNo == SeqNo select a).FirstOrDefault();
                        if (podel != null)
                        {
                            podel.PurchaseOrder = null;
                            podel.PurchaseOrderLine = null;
                            podel.PoCreatedBy = null;
                            podel.PoCreatedDate = null;
                            ddb.Entry(podel).State = System.Data.EntityState.Modified;
                            ddb.SaveChanges();
                        }
                    }
                    return "Purchase Order Line Deleted successfully.";
                }
                else
                {
                    return ErrorMessage;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }




}