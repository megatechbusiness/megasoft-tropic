using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using WinSCP;

namespace Megasoft2.BusinessLogic
{
    public class InvoiceExtractBL
    {
        //This is a comment
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        SysproCore objSyspro = new SysproCore();
        public string GetInvoiceHeader(string XmlOut, string CustomerRef, string DocumentType, ref string PurchaseOrder)
        {
            try
            {
                var xDoc = XDocument.Parse(XmlOut);

                string DocDescendants;

                if (DocumentType == "i")
                {
                    DocDescendants = "DispatchInvoiceDocument";
                }
                else
                    DocDescendants = "SalesOrderInvoiceDocument";


                List<XmlHeader> DetailList = new List<XmlHeader>();
                if (DocumentType == "i")
                {
                    DetailList = (from p in xDoc.Descendants(DocDescendants)
                                  select new XmlHeader
                                  {
                                      RecordHeader = "A",
                                      Type = p.Element("DocumentType").Value,
                                      InvoiceNumber = p.Element("DocumentNumber").Value,
                                      InvoiceDate = p.Element("Header").Element("DocumentDate").Value,
                                      DespatchDate = p.Element("Header").Element("ActualDeliveryDate").Value,
                                      PoNumber = p.Element("Header").Element("CustomerPoNumber").Value,
                                      SupplierOrderNumber = "",
                                      OB10Ref = CustomerRef,
                                      InvoiceAccountNo = p.Element("Header").Element("Customer").Value,
                                      InvoiceAccountName = p.Element("Header").Element("CustomerName").Value,
                                      InvoiceAddress1 = p.Element("Header").Element("SoldToAddr1").Value,
                                      InvoiceAddress2 = p.Element("Header").Element("SoldToAddr2").Value,
                                      InvoiceAddress3 = p.Element("Header").Element("SoldToAddr3").Value,
                                      InvoiceAddress4 = p.Element("Header").Element("SoldToAddr4").Value,
                                      InvoiceAddress5 = p.Element("Header").Element("SoldToAddr5").Value,
                                      DeliveryAccountNo = "",
                                      DeliveryName = p.Element("Header").Element("ShipCustomerName").Value,
                                      DeliveryAddress1 = p.Element("Header").Element("ShipAddress1").Value,
                                      DeliveryAddress2 = p.Element("Header").Element("ShipAddress2").Value,
                                      DeliveryAddress3 = p.Element("Header").Element("ShipAddress3").Value,
                                      DeliveryAddress4 = p.Element("Header").Element("ShipAddress4").Value,
                                      DeliveryAddress5 = p.Element("Header").Element("ShipAddress5").Value,
                                      CurrencyCode = p.Element("Header").Element("Currency").Value,
                                      TaxPointDate = "",
                                      OriginalInvForCredit = "",
                                      DeliveryNoteNumber = p.Element("Header").Element("DispatchNote").Value

                                  }).ToList();
                }
                else
                {
                    DetailList = (from p in xDoc.Descendants(DocDescendants)
                                  select new XmlHeader
                                  {
                                      RecordHeader = "A",
                                      Type = p.Element("DocumentType").Value,
                                      InvoiceNumber = p.Element("DocumentNumber").Value,
                                      InvoiceDate = p.Element("Header").Element("DocumentDate").Value,
                                      //DespatchDate = p.Element("Header").Element("ActualDeliveryDate").Value,
                                      PoNumber = p.Element("Header").Element("CustomerPoNumber").Value,
                                      SupplierOrderNumber = "",
                                      OB10Ref = CustomerRef,
                                      InvoiceAccountNo = p.Element("Header").Element("Customer").Value,
                                      InvoiceAccountName = p.Element("Header").Element("CustomerName").Value,
                                      InvoiceAddress1 = p.Element("Header").Element("SoldToAddr1").Value,
                                      InvoiceAddress2 = p.Element("Header").Element("SoldToAddr2").Value,
                                      InvoiceAddress3 = p.Element("Header").Element("SoldToAddr3").Value,
                                      InvoiceAddress4 = p.Element("Header").Element("SoldToAddr4").Value,
                                      InvoiceAddress5 = p.Element("Header").Element("SoldToAddr5").Value,
                                      DeliveryAccountNo = "",
                                      DeliveryName = p.Element("Header").Element("ShipCustomerName").Value,
                                      DeliveryAddress1 = p.Element("Header").Element("ShipAddress1").Value,
                                      DeliveryAddress2 = p.Element("Header").Element("ShipAddress2").Value,
                                      DeliveryAddress3 = p.Element("Header").Element("ShipAddress3").Value,
                                      DeliveryAddress4 = p.Element("Header").Element("ShipAddress4").Value,
                                      DeliveryAddress5 = p.Element("Header").Element("ShipAddress5").Value,
                                      CurrencyCode = p.Element("Header").Element("Currency").Value,
                                      TaxPointDate = "",
                                      OriginalInvForCredit = p.Element("Header").Element("InvoiceCredited").Value,
                                      DeliveryNoteNumber = p.Element("DocumentNumber").Value //Direct Invoice does not have a dispatch not numberp.Element("Header").Element("DispatchNote").Value

                                  }).ToList();
                }


                //Declaration
                StringBuilder strHeader = new StringBuilder();
                foreach (var item in DetailList)
                {

                    PurchaseOrder = item.PoNumber;
                    //Building Document content
                    strHeader.Append("<InvoiceHeader>");
                    strHeader.Append("<RecordHeader>" + item.RecordHeader.ToString().Trim() + "</RecordHeader>");
                    strHeader.Append("<Type>" + item.Type.ToString().Trim() + "</Type>");
                    strHeader.Append("<InvoiceNumber>" + item.InvoiceNumber.ToString().Trim().TrimStart('0') + "</InvoiceNumber>");
                    strHeader.Append("<InvoiceDate>" + item.InvoiceDate.ToString().Trim() + "</InvoiceDate>");
                    if (DocumentType == "i")
                    {
                        strHeader.Append("<DespatchDate>" + item.DespatchDate.ToString().Trim() + "</DespatchDate>");
                    }
                    strHeader.Append("<PoNumber><![CDATA[" + item.PoNumber.ToString().Trim() + "]]></PoNumber>");
                    strHeader.Append("<SupplierOrderNumber><![CDATA[" + item.SupplierOrderNumber.ToString().Trim() + "]]></SupplierOrderNumber>");
                    strHeader.Append("<OB10Ref><![CDATA[" + item.OB10Ref.ToString().Trim() + "]]></OB10Ref>");
                    strHeader.Append("<InvoiceAccountNo><![CDATA[" + item.InvoiceAccountNo.ToString().Trim() + "]]></InvoiceAccountNo>");
                    strHeader.Append("<InvoiceAccountName><![CDATA[" + item.InvoiceAccountName.ToString().Trim() + "]]></InvoiceAccountName>");
                    strHeader.Append("<InvoiceAddress1><![CDATA[" + item.InvoiceAddress1.ToString().Trim() + "]]></InvoiceAddress1>");
                    strHeader.Append("<InvoiceAddress2><![CDATA[" + item.InvoiceAddress2.ToString().Trim() + "]]></InvoiceAddress2>");
                    strHeader.Append("<InvoiceAddress3><![CDATA[" + item.InvoiceAddress3.ToString().Trim() + "]]></InvoiceAddress3>");
                    strHeader.Append("<InvoiceAddress4><![CDATA[" + item.InvoiceAddress4.ToString().Trim() + "]]></InvoiceAddress4>");
                    strHeader.Append("<InvoiceAddress5><![CDATA[" + item.InvoiceAddress5.ToString().Trim() + "]]></InvoiceAddress5>");
                    strHeader.Append("<DeliveryAccountNo><![CDATA[" + item.DeliveryAccountNo.ToString().Trim() + "]]></DeliveryAccountNo>");
                    strHeader.Append("<DeliveryName><![CDATA[" + item.DeliveryName.ToString().Trim() + "]]></DeliveryName>");
                    strHeader.Append("<DeliveryAddress1><![CDATA[" + item.DeliveryAddress1.ToString().Trim() + "]]></DeliveryAddress1>");
                    strHeader.Append("<DeliveryAddress2><![CDATA[" + item.DeliveryAddress2.ToString().Trim() + "]]></DeliveryAddress2>");
                    strHeader.Append("<DeliveryAddress3><![CDATA[" + item.DeliveryAddress3.ToString().Trim() + "]]></DeliveryAddress3>");
                    strHeader.Append("<DeliveryAddress4><![CDATA[" + item.DeliveryAddress4.ToString().Trim() + "]]></DeliveryAddress4>");
                    strHeader.Append("<DeliveryAddress5><![CDATA[" + item.DeliveryAddress5.ToString().Trim() + "]]></DeliveryAddress5>");
                    strHeader.Append("<CurrencyCode><![CDATA[" + item.CurrencyCode.ToString().Trim() + "]]></CurrencyCode>");
                    strHeader.Append("<TaxPointDate><![CDATA[" + item.TaxPointDate.ToString().Trim() + "]]></TaxPointDate>");
                    strHeader.Append("<OriginalInvForCredit><![CDATA[" + item.OriginalInvForCredit.ToString().Trim() + "]]></OriginalInvForCredit>");
                    strHeader.Append("<DeliveryNoteNumber><![CDATA[" + item.DeliveryNoteNumber.ToString().Trim() + "]]></DeliveryNoteNumber>");
                    strHeader.Append("</InvoiceHeader>");

                }

                return strHeader.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class XmlHeader
        {
            public string RecordHeader { get; set; }
            public string Type { get; set; }
            public string InvoiceNumber { get; set; }
            public string InvoiceDate { get; set; }
            public string DespatchDate { get; set; }
            public string PoNumber { get; set; }
            public string SupplierOrderNumber { get; set; }
            public string OB10Ref { get; set; }
            public string InvoiceAccountNo { get; set; }
            public string InvoiceAccountName { get; set; }
            public string InvoiceAddress1 { get; set; }
            public string InvoiceAddress2 { get; set; }
            public string InvoiceAddress3 { get; set; }
            public string InvoiceAddress4 { get; set; }
            public string InvoiceAddress5 { get; set; }
            public string DeliveryAccountNo { get; set; }
            public string DeliveryName { get; set; }
            public string DeliveryAddress1 { get; set; }
            public string DeliveryAddress2 { get; set; }
            public string DeliveryAddress3 { get; set; }
            public string DeliveryAddress4 { get; set; }
            public string DeliveryAddress5 { get; set; }
            public string CurrencyCode { get; set; }
            public string TaxPointDate { get; set; }
            public string OriginalInvForCredit { get; set; }
            public string TaxNumber { get; set; }
            public string RegNo { get; set; }
            public string ContactName { get; set; }
            public string ContactMobile { get; set; }
            public string ContactEmail { get; set; }
            public string CompanyName { get; set; }
            public string SalesOrder { get; set; }
            public string DeliveryNoteNumber { get; set; }

        }

        public class MerchandiseLine
        {
            public string Header { get; set; }
            public string PoNumber { get; set; }
            public string ProductCode { get; set; }
            public string CustomerStockCode { get; set; }
            public string ProductDescription { get; set; }
            public string UOM { get; set; }
            public string InvoiceQty { get; set; }
            public string UnitPrice { get; set; }
            public string TotalLineValueAfterDiscount { get; set; }
            public string VATRate { get; set; }
            public string VATCode { get; set; }
            public string VATLineAmount { get; set; }
            public string DiscountPercentage { get; set; }
            public string DiscountAmount { get; set; }
            public string SOLine { get; set; }
            public string LineType { get; set; }
            public string LineNumber { get; set; }
            public string SalesOrder { get; set; }

        }


        public string GetDispatchMerchandiseLines(string XmlOut, string Customer, string PurchaseOrder, string InvoiceDate)
        {
            try
            {
                var xDoc = XDocument.Parse(XmlOut);


                var DetailList = (from p in xDoc.Descendants("Merchandise")
                                  select new MerchandiseLine
                                  {
                                      Header = "B",
                                      PoNumber = PurchaseOrder,
                                      ProductCode = p.Element("MStockCode").Value,
                                      CustomerStockCode = this.GetCustomerXRef(p.Element("MStockCode").Value, Customer),
                                      ProductDescription = p.Element("MStockDes").Value,
                                      UOM = p.Element("MOrderUom").Value, //To Be Checked
                                      InvoiceQty = String.Format("{0:0.00}", p.Element("MShipQty").Value),
                                      UnitPrice = String.Format("{0:0.00}", p.Element("MPrice").Value),
                                      TotalLineValueAfterDiscount = p.Element("LineNetAfterDisc").Value,
                                      VATRate = this.GetTaxRate(p.Element("MTaxCode").Value, Convert.ToDateTime(InvoiceDate)),
                                      VATCode = p.Element("MTaxCode").Value,
                                      VATLineAmount = p.Element("LineSalesTax").Value,
                                      DiscountPercentage = p.Element("MDiscPct1").Value,
                                      DiscountAmount = p.Element("LineDiscValue").Value,
                                      SOLine = p.Element("SalesOrderLine").Value,
                                      LineType = ""

                                  }).ToList();

                //Declaration
                StringBuilder strMerchLine = new StringBuilder();

                foreach (var item in DetailList)
                {


                    //Building Document content
                    strMerchLine.Append("<InvoiceLineDetail>");
                    strMerchLine.Append("<Header>B</Header>");
                    strMerchLine.Append("<PoNumber><![CDATA[" + item.PoNumber.ToString().Trim() + "]]></PoNumber>");
                    strMerchLine.Append("<ProductCode><![CDATA[" + item.ProductCode.ToString().Trim() + "]]></ProductCode>");
                    strMerchLine.Append("<CustomerStockCode><![CDATA[" + item.CustomerStockCode.ToString().Trim() + "]]></CustomerStockCode>");
                    strMerchLine.Append("<ProductDescription><![CDATA[" + item.ProductDescription.ToString().Trim() + "]]></ProductDescription>");
                    strMerchLine.Append("<UOM><![CDATA[" + item.UOM.ToString().Trim() + "]]></UOM>");
                    strMerchLine.Append("<InvoiceQty>" + item.InvoiceQty.ToString().Trim() + "</InvoiceQty>");
                    strMerchLine.Append("<UnitPrice>" + item.UnitPrice.ToString().Trim() + "</UnitPrice>");
                    strMerchLine.Append("<TotalLineValueAfterDiscount>" + item.TotalLineValueAfterDiscount.ToString().Trim() + "</TotalLineValueAfterDiscount>");
                    strMerchLine.Append("<VATRate>" + item.VATRate.ToString().Trim() + "</VATRate>");
                    strMerchLine.Append("<VATCode>" + item.VATCode.ToString().Trim() + "</VATCode>");
                    strMerchLine.Append("<VATLineAmount>" + item.VATLineAmount.ToString().Trim() + "</VATLineAmount>");
                    strMerchLine.Append("<DiscountPercentage>" + item.DiscountPercentage.ToString().Trim() + "</DiscountPercentage>");
                    strMerchLine.Append("<DiscountAmount>" + item.DiscountAmount.ToString().Trim() + "</DiscountAmount>");
                    strMerchLine.Append("<SOLine>" + item.SOLine.ToString().Trim() + "</SOLine>");
                    strMerchLine.Append("<LineType>" + item.LineType.ToString().Trim() + "</LineType>");
                    strMerchLine.Append("</InvoiceLineDetail>");

                }

                return strMerchLine.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public class MiscellaneousLine
        {
            public string Header { get; set; }
            public string PoNumber { get; set; }
            public string ProductCode { get; set; }
            public string InvoiceQty { get; set; }
            public string UnitPrice { get; set; }
            public string TotalLineValueAfterDiscount { get; set; }
            public string VATRate { get; set; }
            public string VATCode { get; set; }
            public string VATLineAmount { get; set; }
            public string SOLine { get; set; }
        }

        public string GetDispatchMiscLines(string XmlOut, string LineType, string PurchaseOrder, string InvoiceDate)
        {
            try
            {
                //LineType = MiscCharge OR Freight

                var xDoc = XDocument.Parse(XmlOut);


                var DetailList = (from p in xDoc.Descendants(LineType)
                                  select new MiscellaneousLine
                                  {
                                      Header = "B",
                                      PoNumber = PurchaseOrder,
                                      ProductCode = p.Element("NComment").Value,
                                      InvoiceQty = p.Element("NMscChargeQty").Value,
                                      UnitPrice = p.Element("NSrvUnitPrice").Value,
                                      TotalLineValueAfterDiscount = p.Element("LineMscNetAmount").Value,
                                      VATRate = this.GetTaxRate(p.Element("NMscTaxCode").Value, Convert.ToDateTime(InvoiceDate)),
                                      VATCode = p.Element("NMscTaxCode").Value,
                                      VATLineAmount = p.Element("LineMscChargeTaxAmt").Value,
                                      SOLine = p.Element("SalesOrderLine").Value

                                  }).ToList();

                //Declaration
                StringBuilder strMiscLine = new StringBuilder();

                foreach (var item in DetailList)
                {

                    //Always return section type MiscCharge, even for LineType of Freight.
                    //Building Document content
                    strMiscLine.Append("<MiscCharge>");
                    strMiscLine.Append("<Header>" + item.Header.ToString().Trim() + "</Header>");
                    strMiscLine.Append("<PoNumber><![CDATA[" + item.PoNumber.ToString().Trim() + "]]></PoNumber>");
                    strMiscLine.Append("<ProductCode><![CDATA[" + item.ProductCode.ToString().Trim() + "]]></ProductCode>");
                    strMiscLine.Append("<InvoiceQty>" + item.InvoiceQty.ToString().Trim() + "</InvoiceQty>");
                    strMiscLine.Append("<UnitPrice>" + item.UnitPrice.ToString().Trim() + "</UnitPrice>");
                    strMiscLine.Append("<TotalLineValueAfterDiscount>" + item.TotalLineValueAfterDiscount.ToString().Trim() + "</TotalLineValueAfterDiscount>");
                    strMiscLine.Append("<VATRate>" + item.VATRate.ToString().Trim() + "</VATRate>");
                    strMiscLine.Append("<VATCode>" + item.VATCode.ToString().Trim() + "</VATCode>");
                    strMiscLine.Append("<VATLineAmount>" + item.VATLineAmount.ToString().Trim() + "</VATLineAmount>");
                    strMiscLine.Append("<SOLine>" + item.SOLine.ToString().Trim() + "</SOLine>");
                    strMiscLine.Append("</MiscCharge>");

                }

                return strMiscLine.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class InvoiceSummary
        {
            public string TotalNetAmount { get; set; }
            public string TotalVATAmount { get; set; }
            public string TotalGrossAmount { get; set; }
            public string InvoiceCredited { get; set; }
            public string TotalInvoiceAmount { get; set; }

        }

        public string GetTotals(string XmlOut, string DocumentType)
        {
            try
            {


                var xDoc = XDocument.Parse(XmlOut);

                string DocDescendants;

                if (DocumentType == "i")
                {
                    DocDescendants = "DispatchInvoiceDocument";
                }
                else
                    DocDescendants = "SalesOrderInvoiceDocument";

                var DetailList = (from p in xDoc.Descendants(DocDescendants)
                                  select new InvoiceSummary
                                  {
                                      TotalNetAmount = p.Element("Totals").Element("TotalInvAmtExclTax").Value,
                                      TotalVATAmount = p.Element("Totals").Element("TotalSalesTax").Value,
                                      TotalGrossAmount = p.Element("Totals").Element("TotalInvLessDisc").Value,
                                      InvoiceCredited = this.GetInvoiceCredited(p.Element("DocumentNumber").Value, p.Element("Header").Element("InvoiceCredited").Value)

                                  }).ToList();

                //Declaration
                StringBuilder strSummary = new StringBuilder();

                foreach (var item in DetailList)
                {


                    strSummary.Append("<InvoiceSummary>");
                    strSummary.Append("<Header>C</Header>");
                    strSummary.Append("<TotalNetAmount>" + item.TotalNetAmount.ToString().Trim() + "</TotalNetAmount>");
                    strSummary.Append("<TotalVATAmount>" + item.TotalVATAmount.ToString().Trim() + "</TotalVATAmount>");
                    strSummary.Append("<TotalGrossAmount>" + item.TotalGrossAmount.ToString().Trim() + "</TotalGrossAmount>");
                    strSummary.Append("<InvoiceCredited>" + item.InvoiceCredited.ToString().Trim() + "</InvoiceCredited>");
                    strSummary.Append("</InvoiceSummary>");

                }

                return strSummary.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetInvoiceCredited(string Invoice, string InvoiceCredited)
        {
            try
            {
                if (InvoiceCredited == "")
                {
                    return "No Credit Reference";
                }
                else if (Invoice.TrimStart('0') == InvoiceCredited)
                {
                    return "No Credit Reference";
                }
                else
                {
                    return InvoiceCredited;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class CommentLine
        {
            public string LineNumber { get; set; }
            public string SalesOrderLine { get; set; }
            public string NComment { get; set; }

        }


        public string GetComments(string XmlOut)
        {
            try
            {


                var xDoc = XDocument.Parse(XmlOut);


                var DetailList = (from p in xDoc.Descendants("Comment")
                                  select new CommentLine
                                  {
                                      LineNumber = p.Element("LineNumber").Value,
                                      SalesOrderLine = p.Element("SalesOrderLine").Value,
                                      NComment = p.Element("NComment").Value

                                  }).ToList();

                //Declaration
                StringBuilder strComment = new StringBuilder();

                foreach (var item in DetailList)
                {

                    //Building Document content
                    strComment.Append("<Comment>");
                    strComment.Append("<LineNumber>" + item.LineNumber.ToString().Trim() + "</LineNumber>");
                    strComment.Append("<SalesOrderLine>" + item.SalesOrderLine.ToString().Trim() + "</SalesOrderLine>");
                    strComment.Append("<NComment><![CDATA[" + item.NComment.ToString().Trim() + "]]></NComment>");
                    strComment.Append("</Comment>");

                }

                return strComment.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildInvoiceXmlNestle(string Invoice, string Customer, string CustomerRef, string DocumentType, string Guid, string CustomerClass, string InvoiceDate)
        {
            try
            {
                StringBuilder XmlResult = new StringBuilder();
                string XmlOut;



                if (DocumentType == "i")
                {
                    XmlOut = this.QueryDispatchInvoice(Customer, Invoice, DocumentType, Guid);
                }
                else
                {
                    XmlOut = this.QuerySalesOrderInvoice(Customer, Invoice, DocumentType, Guid);
                }

                string PurchaseOrder = "";

                XmlResult.Append("<Invoice>");
                XmlResult.Append(this.GetBankDetails(CustomerClass));
                XmlResult.Append(this.GetInvoiceHeader(XmlOut, CustomerRef, DocumentType, ref PurchaseOrder));
                XmlResult.Append(this.GetDispatchMerchandiseLines(XmlOut, Customer, PurchaseOrder, InvoiceDate));
                XmlResult.Append(this.GetDispatchMiscLines(XmlOut, "MiscCharge", PurchaseOrder, InvoiceDate));
                XmlResult.Append(this.GetDispatchMiscLines(XmlOut, "Freight", PurchaseOrder, InvoiceDate));
                XmlResult.Append(this.GetComments(XmlOut));
                XmlResult.Append(this.GetTotals(XmlOut, DocumentType));
                XmlResult.Append("</Invoice>");


                return XmlResult.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string QueryDispatchInvoice(string Customer, string Invoice, string DocumentType, string Guid)
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
                Document.Append("the use of the Dispatch Invoice Documents Query Business Object");
                Document.Append("-->");
                Document.Append("<Query xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORQND.XSD\">");
                Document.Append("<Option>");
                Document.Append("<Reprint>Y</Reprint>");
                Document.Append("<DocumentType>" + DocumentType + "</DocumentType>");
                Document.Append("<IncludeForms />");
                Document.Append("<IncludeCustomerForms />");
                Document.Append("<IncludeStockForms />");
                Document.Append("<IncludeDetailForms />");
                Document.Append("<IncludeNotes />");
                Document.Append("<IgnorePrintStatus />");
                Document.Append("<RoundMass>O</RoundMass>");
                Document.Append("<RoundMassDecs />");
                Document.Append("<RoundVolume>O</RoundVolume>");
                Document.Append("<RoundVolumeDecs />");
                Document.Append("<PrintTranslatedText />");
                Document.Append("<XslStylesheet />");
                Document.Append("</Option>");
                Document.Append("<Filter>");
                Document.Append("<Branch FilterType=\"A\" />");
                Document.Append("<Customer FilterType=\"L\" FilterValue=\"" + Customer + "\" />");
                Document.Append("<OrderType FilterType=\"A\" />");
                Document.Append("<CustomerPo FilterType=\"A\" />");
                Document.Append("<InvoiceNumber FilterType=\"L\" FilterValue=\"" + Invoice + "\" />");
                Document.Append("<GtrReference FilterType=\"A\" />");
                Document.Append("<Operator FilterType=\"A\" />");
                Document.Append("<GeographicArea FilterType=\"A\" />");
                Document.Append("</Filter>");
                Document.Append("</Query>");

                string XmlOut = objSyspro.SysproQuery(Guid, Document.ToString(), "SORQND");
                string ErrorMessage = objSyspro.GetXmlErrors(XmlOut);

                if (XmlOut.Trim() == "")
                {
                    throw new Exception("Dispatch Note Query Object returned blank!");
                }


                if (ErrorMessage == "")
                {
                    return XmlOut;
                }
                else
                    throw new Exception(ErrorMessage);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string QuerySalesOrderInvoice(string Customer, string Invoice, string DocumentType, string Guid)
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
                Document.Append("the use of the Sales Order Invoice Documents Query Business Object");
                Document.Append("-->");
                Document.Append("<Query xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORQID.XSD\">");
                Document.Append("<Option>");
                Document.Append("<Reprint>Y</Reprint>");
                Document.Append("<DocumentType>" + DocumentType + "</DocumentType>");
                Document.Append("<IncludeForms />");
                Document.Append("<IncludeCustomerForms />");
                Document.Append("<IncludeStockForms />");
                Document.Append("<IncludeDetailForms />");
                Document.Append("<IncludeNotes />");
                //Document.Append("<IncludeKitComponents>Y</IncludeKitComponents>");
                Document.Append("<IgnorePrintStatus />");
                Document.Append("<RoundMass>O</RoundMass>");
                Document.Append("<RoundMassDecs />");
                Document.Append("<RoundVolume>O</RoundVolume>");
                Document.Append("<RoundVolumeDecs />");
                Document.Append("<PrintTranslatedText />");
                Document.Append("<XslStylesheet />");
                Document.Append("</Option>");
                Document.Append("<Filter>");
                Document.Append("<Branch FilterType=\"A\" />");
                Document.Append("<Customer FilterType=\"A\" FilterValue=\"" + Customer + "\" />");
                Document.Append("<SourceWarehouse FilterType=\"A\" />");
                Document.Append("<TargetWarehouse FilterType=\"A\" />");
                Document.Append("<OrderType FilterType=\"A\" />");
                Document.Append("<CustomerPo FilterType=\"A\" />");
                Document.Append("<InvoiceNumber FilterType=\"L\" FilterValue=\"" + Invoice + "\" />");
                Document.Append("<SalesOrder FilterType=\"A\" />");
                Document.Append("<GtrReference FilterType=\"A\" />");
                Document.Append("<Operator FilterType=\"A\" />");
                Document.Append("<GeographicArea FilterType=\"A\" />");
                Document.Append("</Filter>");
                Document.Append("</Query>");


                string XmlOut = objSyspro.SysproQuery(Guid, Document.ToString(), "SORQID");

                if (XmlOut.Trim() == "")
                {
                    throw new Exception("Sales Order Query Object returned blank!");
                }

                string ErrorMessage = objSyspro.GetXmlErrors(XmlOut);

                if (ErrorMessage == "")
                {
                    return XmlOut;
                }
                else
                    throw new Exception(ErrorMessage);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetCustomerXRef(string StockCode, string Customer)
        {
            try
            {
                var result = wdb.sp_GetInvoiceExtractCustXRef(StockCode, Customer).ToList();
                if (result.Count > 0)
                {
                    return result.FirstOrDefault().CustStockCode;
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetTaxRate(string TaxCode, DateTime TrnDate)
        {
            try
            {
                return wdb.sp_GetInvoiceExtractTaxRate(TaxCode, TrnDate.Date).FirstOrDefault().TaxRate.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetBankDetails(string CustomerClass)
        {
            try
            {

                var result = (from a in wdb.mtBankingDetails where a.CustomerClass == CustomerClass select a).FirstOrDefault();

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<BankDetails>");
                Document.Append("<BankName>" + result.BankName + "</BankName>");
                Document.Append("<BankAccountNumber>" + result.BankAccountNo + "</BankAccountNumber>");
                Document.Append("<BankSortCode>" + result.BankSortCode + "</BankSortCode>");
                Document.Append("<BankAccountName><![CDATA[" + result.BankAccountName + "]]></BankAccountName>");
                Document.Append("<SWIFTNumber>" + result.SwiftNumber + "</SWIFTNumber>");
                Document.Append("<IBANNumber>" + result.IBANNumber + "</IBANNumber>");
                Document.Append("</BankDetails>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string DownloadNestleInvoice(CustomerInvoiceViewModel model, bool doSFTP, bool EdiService)
        {

            try
            {
                string Guid;
                string Username;
                if (EdiService == true)
                {
                    Guid = objSyspro.SysproLogin("MSEDI", "SysproCompanyA");
                    Username = "MSEDI";
                }
                else
                {
                    Guid = objSyspro.SysproLogin();
                    Username = HttpContext.Current.User.Identity.Name.ToUpper();
                }


                if (Guid != "")
                {
                    var PathSetting = (from a in wdb.mtInvoiceExtractSettings where a.CustomerClass == model.CustomerClass select a).FirstOrDefault();
                    StringBuilder XmlResult = new StringBuilder();
                    XmlResult.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    XmlResult.Append("<NestleInvoices>");

                    foreach (var item in model.Invoices)
                    {
                        if (item.DoExtract == true)
                        {
                            if (item.Invoice.Trim() != "")
                                XmlResult.Append(this.BuildInvoiceXmlNestle(item.Invoice, item.Customer, item.CustomerRef, item.DocumentType, Guid, model.CustomerClass, item.InvoiceDate));
                        }

                    }

                    XmlResult.Append("</NestleInvoices>");

                    objSyspro.SysproLogoff(Guid);


                    string FilePath = PathSetting.OutputFilePath + @"\Unprocessed\"; //Properties.Settings.Default.InvoicingOutputFilePath.Trim();

                    string FileName = model.CustomerClass.ToString().Trim() + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".xml";

                    string OutputPath = Path.Combine(FilePath, FileName);

                    File.WriteAllText(OutputPath, XmlResult.ToString());


                    //Log Invoices
                    if (doSFTP == false)
                    {
                        foreach (var item in model.Invoices)
                        {
                            if (item.DoExtract == true)
                            {
                                wdb.sp_SaveInvoiceExtractLog(item.Customer, item.Invoice, Username);
                            }
                        }


                    }
                    else
                    {
                        StringBuilder OutputMessage = new StringBuilder();
                        // Setup session options
                        //To generate the code below, open WINSCP.exe file on server, click advanced, under authentication browse and select the private key file.
                        //click ok.
                        //enter Hostname, username and port 22.
                        //click login
                        //on top menu click session-->generate session url/code
                        //select .net assembly code and paste the code below
                        //use details below to update settings table

                        //SessionOptions sessionOptions = new SessionOptions
                        //{
                        //    Protocol = Protocol.Sftp,
                        //    HostName = "transfer.tungsten-network.com",
                        //    UserName = "AAA717177935",
                        //    Password = "717177935",
                        //    SshHostKeyFingerprint = "ssh-rsa 2048 Mu0BV1mc6KrpT/pB8mCWDEr4AUSCdOx/J1HRw+HSIHw=",
                        //};

                        SessionOptions sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Sftp,
                            HostName = PathSetting.SFTPUrl,
                            UserName = PathSetting.SFTPUsername,
                            Password = PathSetting.SFTPPassword,
                            SshHostKeyFingerprint = PathSetting.SshHostKeyFingerprint
                        };




                        using (Session session = new Session())
                        {

                            try
                            {
                                // Connect
                                session.Open(sessionOptions);

                                // Upload files
                                TransferOptions transferOptions = new TransferOptions();
                                transferOptions.TransferMode = TransferMode.Binary;
                                transferOptions.FilePermissions = null; // This is default
                                transferOptions.PreserveTimestamp = false;

                                TransferOperationResult transferResult;
                                transferResult = session.PutFiles(PathSetting.OutputFilePath + @"\Unprocessed\*", "/" + PathSetting.SFTPFolder + "/", false, transferOptions);

                                // Throw on any error
                                transferResult.Check();

                                // Print results
                                //foreach (TransferEventArgs transfer in transferResult.Transfers)
                                //{
                                //    OutputMessage.AppendLine("Upload of " + transfer.FileName + " succeeded");
                                //}

                                this.MoveAllFiles(PathSetting.OutputFilePath, true);


                                //Log Invoices
                                foreach (var item in model.Invoices)
                                {
                                    if (item.DoExtract == true)
                                    {
                                        wdb.sp_SaveInvoiceExtractLog(item.Customer, item.Invoice, Username);
                                    }
                                }


                                this.SendEmail(model);

                            }
                            catch (Exception ex)
                            {
                                this.MoveAllFiles(PathSetting.OutputFilePath, false);
                                throw new Exception(ex.Message);
                            }

                        }
                    }

                    return "File Created Successfully!";
                }
                else
                {
                    return "Failed to Login to Syspro!";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string DownloadUnileverInvoice(CustomerInvoiceViewModel model, bool doSFTP, bool EdiService)
        {
            try
            {
                string Guid;
                if (EdiService == true)
                {
                    Guid = objSyspro.SysproLogin("MSEDI");
                }
                else
                {
                    Guid = objSyspro.SysproLogin();
                }
                if (Guid != "")
                {

                    var PathSetting = (from a in wdb.mtInvoiceExtractSettings where a.CustomerClass == model.CustomerClass select a).FirstOrDefault();


                    foreach (var item in model.Invoices)
                    {
                        if (item.DoExtract == true)
                        {
                            StringBuilder XmlResult = new StringBuilder();
                            XmlResult.Append("<?xml version=\"1.0\"?>");
                            string XmlOut;
                            if (item.DocumentType == "i")
                            {
                                XmlOut = this.QueryDispatchInvoice(item.Customer, item.Invoice, item.DocumentType, Guid);
                            }
                            else
                            {
                                XmlOut = this.QuerySalesOrderInvoice(item.Customer, item.Invoice, item.DocumentType, Guid);
                            }

                            XmlResult.Append("<Invoice xmlns=\"urn:schemas-basda-org:2000:salesInvoice:xdr:3.01\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"urn:schemas-basda-org:2000:salesInvoice:xdr:3.01 Invoice-v3.xsd\">");
                            XmlResult.Append(this.BuildUnileverHeader(item.DocumentType));
                            XmlResult.Append(this.BuildUnileverReference(item.Invoice, item.CustomerRef, item.CustomerPoNumber, item.DocumentType, XmlOut));
                            XmlResult.Append("<InvoiceDate>" + item.InvoiceDate + "</InvoiceDate>");
                            XmlResult.Append(this.BuildUnileverSupplier(XmlOut, item.DocumentType, "", item.Customer));
                            XmlResult.Append("<Delivery>");
                            XmlResult.Append("<PreferredDate>" + item.InvoiceDate + "</PreferredDate>");
                            XmlResult.Append("</Delivery>");
                            XmlResult.Append(this.BuildUnileverInvoiceTo(XmlOut, item.DocumentType, "", item.Customer));
                            XmlResult.Append(this.BuildUnileverInvoiceLine(XmlOut, item.CustomerPoNumber, item.Customer, item.InvoiceDate, item.DocumentType));
                            XmlResult.Append(this.BuildUnileverTotals(XmlOut, item.DocumentType, item.InvoiceDate));
                            XmlResult.Append("</Invoice>");

                            string FilePath = PathSetting.OutputFilePath + @"\Unprocessed\"; //Properties.Settings.Default.InvoicingOutputFilePath.Trim();

                            string FileName = item.Invoice.TrimStart('0') + ".xml";

                            string OutputPath = Path.Combine(FilePath, FileName);

                            File.WriteAllText(OutputPath, XmlResult.ToString());

                            //Save Blank File. Unilever Requirement.
                            File.WriteAllText(OutputPath + ".ok", "");

                            //Log Invoices
                            if (doSFTP == false)
                            {
                                wdb.sp_SaveInvoiceExtractLog(item.Customer, item.Invoice, HttpContext.Current.User.Identity.Name.ToUpper());
                            }

                        }

                    }

                    objSyspro.SysproLogoff(Guid);


                    if (doSFTP)
                    {
                        StringBuilder OutputMessage = new StringBuilder();
                        // Setup session options
                        //To generate the code below, open WINSCP.exe file on server, click advanced, under authentication browse and select the private key file.
                        //click ok.
                        //enter Hostname, username and port 22.
                        //click login
                        //on top menu click session-->generate session url/code
                        //select .net assembly code and paste the code below
                        //use details below to update settings table

                        //SessionOptions sessionOptions = new SessionOptions
                        //{
                        //    Protocol = Protocol.Sftp,
                        //    HostName = "si.tradeshift.com",
                        //    UserName = "XhT0HEYeS4y4T4Th6SL8jw",
                        //    SshHostKeyFingerprint = "ssh-rsa 2048 NUHBm1wHkhoOvGsRR4wDVKBLQkYaoORSMoCuwo49z7k=", //Fingerprint Key
                        //    SshPrivateKeyPath = @"C:\sftp_rsa.ppk" //Location of Private Key File On Server.
                        //};

                        SessionOptions sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Sftp,
                            HostName = PathSetting.SFTPUrl,
                            UserName = PathSetting.SFTPUsername,
                            Password = PathSetting.SFTPPassword,
                            SshHostKeyFingerprint = PathSetting.SshHostKeyFingerprint, //Fingerprint Key
                            //SshPrivateKeyPath = PathSetting.SshPrivateKeyPath //Location of Private Key File On Server.
                        };

                        using (Session session = new Session())
                        {

                            try
                            {
                                // Connect
                                session.Open(sessionOptions);

                                // Upload files
                                TransferOptions transferOptions = new TransferOptions();
                                transferOptions.TransferMode = TransferMode.Binary;

                                TransferOperationResult transferResult;
                                transferResult = session.PutFiles(PathSetting.OutputFilePath + @"\Unprocessed\*", "/" + PathSetting.SFTPFolder + "/", false, transferOptions);

                                // Throw on any error
                                transferResult.Check();

                                // Print results
                                //foreach (TransferEventArgs transfer in transferResult.Transfers)
                                //{
                                //    OutputMessage.AppendLine("Upload of " + transfer.FileName + " succeeded");
                                //}

                                this.MoveAllFiles(PathSetting.OutputFilePath, true);


                                //Log Invoices
                                foreach (var item in model.Invoices)
                                {
                                    if (item.DoExtract == true)
                                    {
                                        wdb.sp_SaveInvoiceExtractLog(item.Customer, item.Invoice, HttpContext.Current.User.Identity.Name.ToUpper());
                                    }
                                }

                                this.SendEmail(model);


                            }
                            catch (Exception ex)
                            {
                                this.MoveAllFiles(PathSetting.OutputFilePath, false);
                                throw new Exception(ex.Message);
                            }

                        }
                        return "Files Created Successfully!";
                    }
                    else
                    {
                        return "Files Created Successfully!";
                    }

                }
                else
                {
                    return "Failed to Login to Syspro!";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildUnileverHeader(string DocumentType)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<InvoiceHead>");
                Document.Append("<Schema>");
                Document.Append("<Version>3.05</Version>");
                Document.Append("</Schema>");
                Document.Append("<Parameters>");
                Document.Append("<Language>en-GB</Language>");
                Document.Append("<DecimalSeparator>.</DecimalSeparator>");
                Document.Append("<Precision>10.3</Precision>");
                Document.Append("</Parameters>");
                if (DocumentType == "C")
                {
                    Document.Append("<InvoiceType Code=\"CRN\">Credit Note</InvoiceType>");
                }
                else
                {
                    Document.Append("<InvoiceType Code=\"INV\">Commercial Invoice</InvoiceType>");
                }

                Document.Append("<InvoiceCurrency>");
                Document.Append("<Currency Code=\"ZAR\">Rand</Currency>");
                Document.Append("</InvoiceCurrency>");
                Document.Append("<Checksum></Checksum>");
                Document.Append("</InvoiceHead>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildUnileverReference(string Invoice, string Reference, string CustomerPoNumber, string DocumentType, string XmlOut)
        {
            try
            {
                var xDoc = XDocument.Parse(XmlOut);

                string DocDescendants;

                if (DocumentType == "i")
                {
                    DocDescendants = "DispatchInvoiceDocument";
                }
                else
                    DocDescendants = "SalesOrderInvoiceDocument";

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<InvoiceReferences>");
                Document.Append("<ContractOrderReference>" + Reference + "</ContractOrderReference>");
                Document.Append("<CostCentre></CostCentre>");
                Document.Append("<BuyersOrderNumber Preserve=\"true\">" + CustomerPoNumber + "</BuyersOrderNumber>");
                Document.Append("<ProjectCode Preserve=\"true\"></ProjectCode>");
                Document.Append("<SuppliersInvoiceNumber Preserve=\"true\">" + Invoice + "</SuppliersInvoiceNumber>");
                if (DocumentType == "C")
                {
                    string InvoiceCredited = (from p in xDoc.Descendants(DocDescendants)
                                              select new XmlHeader
                                              {

                                                  OriginalInvForCredit = p.Element("Header").Element("InvoiceCredited").Value,

                                              }).FirstOrDefault().OriginalInvForCredit;
                    Document.Append("<ResponseTo Preserve='true'>" + InvoiceCredited + "</ResponseTo>");
                }
                Document.Append("</InvoiceReferences>");






                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildUnileverSupplier(string XmlOut, string DocumentType, string Reference, string Customer)
        {
            try
            {

                var xDoc = XDocument.Parse(XmlOut);

                string DocDescendants;

                if (DocumentType == "i")
                {
                    DocDescendants = "DispatchInvoiceDocument";
                }
                else
                    DocDescendants = "SalesOrderInvoiceDocument";

                var Contact = wdb.sp_GetInvoiceExtractCustomerContactDetails(Customer).FirstOrDefault();

                string ContactName = "";
                string ContactNum = "";
                string ContactEmail = "";

                if (Contact != null)
                {
                    ContactName = Contact.EdiContactName;
                    ContactNum = Contact.EdiContactNumber;
                    ContactEmail = Contact.EdiContactEmail;
                }

                var DetailList = (from p in xDoc.Descendants(DocDescendants)
                                  select new XmlHeader
                                  {
                                      //RecordHeader = "A",
                                      //Type = p.Element("DocumentType").Value,
                                      //InvoiceNumber = p.Element("DocumentNumber").Value,
                                      //InvoiceDate = p.Element("Header").Element("DocumentDate").Value,
                                      //DespatchDate = p.Element("Header").Element("ActualDeliveryDate").Value,
                                      //PoNumber = p.Element("Header").Element("CustomerPoNumber").Value,
                                      //SupplierOrderNumber = "",
                                      //OB10Ref = Reference,
                                      //InvoiceAccountNo = p.Element("Header").Element("Customer").Value,
                                      InvoiceAccountName = p.Element("Header").Element("CompanyName").Value, //Tropic's Company Name
                                      InvoiceAddress1 = p.Element("Header").Element("CompanyAddress1").Value, //Tropic's Address
                                      InvoiceAddress2 = p.Element("Header").Element("CompanyAddress2").Value,
                                      InvoiceAddress3 = p.Element("Header").Element("CompanyAddress3").Value,
                                      InvoiceAddress4 = p.Element("Header").Element("CompanyPostalcode").Value,
                                      //InvoiceAddress5 = p.Element("Header").Element("SoldToAddr5").Value,
                                      //DeliveryAccountNo = "",
                                      //DeliveryName = p.Element("Header").Element("ShipCustomerName").Value,
                                      //DeliveryAddress1 = p.Element("Header").Element("ShipAddress1").Value,
                                      //DeliveryAddress2 = p.Element("Header").Element("ShipAddress2").Value,
                                      //DeliveryAddress3 = p.Element("Header").Element("ShipAddress3").Value,
                                      //DeliveryAddress4 = p.Element("Header").Element("ShipAddress4").Value,
                                      //DeliveryAddress5 = p.Element("Header").Element("ShipAddress5").Value,
                                      //CurrencyCode = p.Element("Header").Element("Currency").Value,
                                      //TaxPointDate = "",
                                      //OriginalInvForCredit = "",
                                      TaxNumber = p.Element("Header").Element("CompanyTaxNumber").Value, //Tropic's Tax No
                                      RegNo = p.Element("Header").Element("CompanyRegNumber").Value //Tropic's Reg No

                                  }).ToList();


                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<Supplier>");
                Document.Append("<SupplierReferences>");
                Document.Append("<TaxNumber>" + DetailList.FirstOrDefault().TaxNumber + "</TaxNumber>");
                Document.Append("<RegistrationNumber>" + DetailList.FirstOrDefault().RegNo + "</RegistrationNumber>");
                Document.Append("</SupplierReferences>");
                Document.Append("<Party>" + DetailList.FirstOrDefault().InvoiceAccountName.Replace("&", "&amp;") + "</Party>");
                Document.Append("<Address>");
                Document.Append("<AddressLine>" + DetailList.FirstOrDefault().InvoiceAddress1 + "</AddressLine>");
                Document.Append("<Street>" + DetailList.FirstOrDefault().InvoiceAddress2 + "</Street>");
                Document.Append("<City>" + DetailList.FirstOrDefault().InvoiceAddress3 + "</City>");
                Document.Append("<State></State>");
                Document.Append("<PostCode>" + DetailList.FirstOrDefault().InvoiceAddress4 + "</PostCode>");
                Document.Append("<Country Code=\"ZA\">SOUTH AFRICA</Country>");
                Document.Append("</Address>");
                Document.Append("<Contact>");
                Document.Append("<Name>" + ContactName + "</Name>");
                Document.Append("<UserID></UserID>");
                Document.Append("<Email>" + ContactEmail + "</Email>");
                Document.Append("<Mobile>" + ContactNum + "</Mobile>");
                Document.Append("</Contact>");
                Document.Append("</Supplier>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildUnileverInvoiceTo(string XmlOut, string DocumentType, string Reference, string Customer)
        {
            try
            {



                var xDoc = XDocument.Parse(XmlOut);

                string DocDescendants;

                if (DocumentType == "i")
                {
                    DocDescendants = "DispatchInvoiceDocument";
                }
                else
                    DocDescendants = "SalesOrderInvoiceDocument";

                var DetailList = (from p in xDoc.Descendants(DocDescendants)
                                  select new XmlHeader
                                  {
                                      //RecordHeader = "A",
                                      //Type = p.Element("DocumentType").Value,
                                      //InvoiceNumber = p.Element("DocumentNumber").Value,
                                      //InvoiceDate = p.Element("Header").Element("DocumentDate").Value,
                                      //DespatchDate = p.Element("Header").Element("ActualDeliveryDate").Value,
                                      //PoNumber = p.Element("Header").Element("CustomerPoNumber").Value,
                                      //SupplierOrderNumber = "",
                                      //OB10Ref = Reference,
                                      //InvoiceAccountNo = p.Element("Header").Element("Customer").Value,
                                      InvoiceAccountName = p.Element("Header").Element("CustomerName").Value,
                                      //InvoiceAddress1 = p.Element("Header").Element("SoldToAddr1").Value,
                                      //InvoiceAddress2 = p.Element("Header").Element("SoldToAddr2").Value,
                                      //InvoiceAddress3 = p.Element("Header").Element("SoldToAddr3").Value,
                                      //InvoiceAddress4 = p.Element("Header").Element("SoldToAddr4").Value,
                                      //InvoiceAddress5 = p.Element("Header").Element("SoldToAddr5").Value,
                                      //DeliveryAccountNo = "",
                                      //DeliveryName = p.Element("Header").Element("ShipCustomerName").Value,
                                      DeliveryAddress1 = p.Element("Header").Element("ShipAddress1").Value,
                                      //DeliveryAddress2 = p.Element("Header").Element("ShipAddress2").Value,
                                      //DeliveryAddress3 = p.Element("Header").Element("ShipAddress3").Value,
                                      //DeliveryAddress4 = p.Element("Header").Element("ShipAddress4").Value,
                                      //DeliveryAddress5 = p.Element("Header").Element("ShipAddress5").Value,
                                      //CurrencyCode = p.Element("Header").Element("Currency").Value,
                                      //TaxPointDate = "",
                                      //OriginalInvForCredit = "",
                                      TaxNumber = p.Element("Header").Element("CompanyTaxNo").Value//, //Unilever's Tax No
                                      //ContactName = p.Element("Header").Element("Contact").Value,
                                      //ContactMobile = p.Element("Header").Element("Telephone").Value,
                                      //ContactEmail = p.Element("Header").Element("Email").Value

                                  }).ToList();

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<InvoiceTo>");
                Document.Append("<InvoiceToReferences>");
                Document.Append("<TaxNumber>" + DetailList.FirstOrDefault().TaxNumber + "</TaxNumber>");
                Document.Append("<RegistrationNumber></RegistrationNumber>");
                Document.Append("<BuyersCodeForInvoiceTo></BuyersCodeForInvoiceTo>");
                Document.Append("</InvoiceToReferences>");
                Document.Append("<Party>" + DetailList.FirstOrDefault().InvoiceAccountName.Replace("&", "&amp;") + "</Party>");
                Document.Append("<Address>");
                Document.Append("<Street>" + DetailList.FirstOrDefault().DeliveryAddress1 + "</Street>");
                //Document.Append("<City><![CDATA[" + DetailList.FirstOrDefault().InvoiceAccountName + "]]></City>");
                //Document.Append("<PostCode><![CDATA[" + DetailList.FirstOrDefault().InvoiceAccountName + "]]></PostCode>");
                //Document.Append("<Country Code=\"GBR\">United Kingdom</Country>");
                Document.Append("</Address>");
                Document.Append("<Contact>");
                Document.Append("<Name></Name>");
                Document.Append("<UserID></UserID>");
                Document.Append("<Email></Email>");
                Document.Append("<Mobile></Mobile>");
                Document.Append("</Contact>");
                Document.Append("</InvoiceTo>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildUnileverInvoiceLine(string XmlOut, string PurchaseOrder, string Customer, string InvoiceDate, string DocumentType)
        {
            try
            {

                var xDoc = XDocument.Parse(XmlOut);

                string DocDescendants;

                if (DocumentType == "i")
                {
                    DocDescendants = "DispatchInvoiceDocument";
                }
                else
                    DocDescendants = "SalesOrderInvoiceDocument";

                string SalesOrder = (from p in xDoc.Descendants(DocDescendants)
                                     select new XmlHeader
                                     {

                                         SalesOrder = p.Element("Header").Element("SalesOrder").Value,

                                     }).FirstOrDefault().SalesOrder;


                var DetailList = (from p in xDoc.Descendants("Merchandise")
                                  select new MerchandiseLine
                                  {
                                      Header = "B",
                                      PoNumber = PurchaseOrder,
                                      ProductCode = p.Element("MStockCode").Value,
                                      CustomerStockCode = this.GetCustomerXRef(p.Element("MStockCode").Value, Customer),
                                      ProductDescription = p.Element("MStockDes").Value + ", " + p.Element("LongDesc").Value,
                                      UOM = p.Element("MOrderUom").Value, //To Be Checked
                                      InvoiceQty = String.Format("{0:0.00}", p.Element("MShipQty").Value),
                                      UnitPrice = String.Format("{0:0.00}", p.Element("MPrice").Value),
                                      TotalLineValueAfterDiscount = p.Element("LineNetAfterDisc").Value,
                                      VATRate = this.GetTaxRate(p.Element("MTaxCode").Value, Convert.ToDateTime(InvoiceDate)),
                                      VATCode = p.Element("MTaxCode").Value,
                                      VATLineAmount = p.Element("LineSalesTax").Value,
                                      DiscountPercentage = p.Element("MDiscPct1").Value,
                                      DiscountAmount = p.Element("LineDiscValue").Value,
                                      SOLine = p.Element("SalesOrderLine").Value,
                                      LineType = "",
                                      LineNumber = p.Element("LineNumber").Value,
                                      SalesOrder = SalesOrder

                                  }).ToList();


                //Declaration
                StringBuilder Document = new StringBuilder();

                foreach (var item in DetailList)
                {

                    int soLine = Convert.ToInt16(item.SOLine);
                    var soCustom = (from a in wdb.sp_GetSoCustomerPoLineNumber(item.SalesOrder, soLine) select a.CustomerPoLineNum).FirstOrDefault();
                    if (soCustom == 0)
                    {
                        soCustom = Convert.ToInt16(item.LineNumber);
                    }


                    //Building Document content
                    Document.Append("<InvoiceLine Action=\"Add\" TypeCode=\"GDS\" TypeDescription=\"Goods &amp; Services\">");
                    Document.Append("<LineNumber Preserve=\"true\">" + Convert.ToInt16(item.LineNumber) + "</LineNumber>");
                    Document.Append("<InvoiceLineReferences>");
                    //Document.Append("<ContractOrderReference>MOLU28001736760</ContractOrderReference>");
                    //Document.Append("<CostCentre>5250124502</CostCentre>");
                    //Document.Append("<ProjectCode Preserve=\"true\">14600152101011</ProjectCode>");
                    Document.Append("<BuyersOrderNumber Preserve=\"true\">" + PurchaseOrder + "</BuyersOrderNumber>");
                    Document.Append("<BuyersOrderLineReference Preserve=\"true\">" + soCustom.ToString() + "</BuyersOrderLineReference>");
                    Document.Append("</InvoiceLineReferences>");

                    Document.Append("<Product>");
                    Document.Append("<SuppliersProductCode>" + item.ProductCode + "</SuppliersProductCode>");
                    Document.Append("<Description>" + item.ProductDescription + "</Description>");
                    Document.Append("</Product>");

                    Document.Append("<Quantity>");
                    Document.Append("<Amount>" + item.InvoiceQty + "</Amount>");
                    Document.Append("</Quantity>");

                    Document.Append("<Price UOMCode=\"" + item.UOM + "\" UOMDescription=\"" + item.UOM + "\">");
                    Document.Append("<Units>1</Units>");
                    Document.Append("<UnitPrice>" + String.Format("{0:0.00}", item.UnitPrice) + "</UnitPrice>");
                    Document.Append("</Price>");

                    Document.Append("<LineTax>");
                    Document.Append("<MixedRateIndicator>0</MixedRateIndicator>");
                    Document.Append("<TaxRate>" + String.Format("{0:0.00}", item.VATRate) + "</TaxRate>");
                    Document.Append("<TaxValue>" + String.Format("{0:0.00}", item.VATLineAmount) + "</TaxValue>");
                    Document.Append("</LineTax>");

                    Document.Append("<LineTotal>" + String.Format("{0:0.00}", item.TotalLineValueAfterDiscount) + "</LineTotal>");
                    Document.Append("<InvoiceLineInformation></InvoiceLineInformation>");
                    Document.Append("<ExtendedDescription></ExtendedDescription>");
                    Document.Append("</InvoiceLine>");

                }




                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildUnileverTotals(string XmlOut, string DocumentType, string InvoiceDate)
        {
            try
            {

                var xDoc = XDocument.Parse(XmlOut);

                //string DocDescendants;

                //if (DocumentType == "i")
                //{
                //    DocDescendants = "DispatchInvoiceDocument";
                //}
                //else
                //    DocDescendants = "SalesOrderInvoiceDocument";



                var AllLines = (from p in xDoc.Descendants("Merchandise")
                                select new MerchandiseLine
                                {
                                    InvoiceQty = p.Element("MShipQty").Value,
                                    UnitPrice = String.Format("{0:0.00}", p.Element("MPrice").Value),
                                    TotalLineValueAfterDiscount = p.Element("LineNetAfterDisc").Value,
                                    VATRate = Convert.ToInt16(this.GetTaxRate(p.Element("MTaxCode").Value, Convert.ToDateTime(InvoiceDate))).ToString(),
                                    VATCode = p.Element("MTaxCode").Value,
                                    VATLineAmount = p.Element("LineSalesTax").Value,
                                    DiscountPercentage = p.Element("MDiscPct1").Value,
                                    DiscountAmount = p.Element("LineDiscValue").Value
                                }).ToList();

                var Vatable = (from a in AllLines where a.VATRate != "0" select new { ExVat = Convert.ToDecimal(a.InvoiceQty) * Convert.ToDecimal(a.UnitPrice), VatAmount = Convert.ToDecimal(a.VATLineAmount), InclVat = Convert.ToDecimal(a.InvoiceQty) * Convert.ToDecimal(a.UnitPrice) + Convert.ToDecimal(a.VATLineAmount) }).ToList();
                var NonVatable = (from a in AllLines where a.VATRate == "0" select new { ExVat = Convert.ToDecimal(a.InvoiceQty) * Convert.ToDecimal(a.UnitPrice), VatAmount = a.VATLineAmount, InclVat = Convert.ToDecimal(a.InvoiceQty) * Convert.ToDecimal(a.UnitPrice) + Convert.ToDecimal(a.VATLineAmount) }).ToList();

                var VatRate = (from p in xDoc.Descendants("Merchandise")
                               where p.Element("MTaxCode").Value != "E"
                               select new MerchandiseLine
                               {

                                   VATRate = this.GetTaxRate(p.Element("MTaxCode").Value, Convert.ToDateTime(InvoiceDate))

                               }).FirstOrDefault();



                var TotalLines = (from p in xDoc.Descendants("Merchandise")
                                  select new MerchandiseLine
                                  {

                                      LineNumber = p.Element("LineNumber").Value

                                  }).ToList().Count;


                //Declaration
                StringBuilder Document = new StringBuilder();

                int TotalVatCodes = 0;
                decimal SumNonVat = 0;
                decimal SumVatLinesExclVat = 0;
                //NonVatable
                if (NonVatable.Count > 0)
                {
                    TotalVatCodes++;
                    SumNonVat = (from a in NonVatable select a.ExVat).Sum();

                    //Building Document content
                    Document.Append("<TaxSubTotal>");
                    Document.Append("<TaxRate Code=\"E\">0.00</TaxRate>");
                    Document.Append("<NumberOfLinesAtRate>" + NonVatable.Count + "</NumberOfLinesAtRate>");
                    Document.Append("<TotalValueAtRate>" + String.Format("{0:0.00}", SumNonVat) + "</TotalValueAtRate>");
                    Document.Append("<TaxableValueAtRate>" + String.Format("{0:0.00}", SumNonVat) + "</TaxableValueAtRate>");
                    Document.Append("<TaxAtRate>0.00</TaxAtRate>");
                    Document.Append("<NetPaymentAtRate>" + String.Format("{0:0.00}", SumNonVat) + "</NetPaymentAtRate>");
                    Document.Append("<GrossPaymentAtRate>" + String.Format("{0:0.00}", SumNonVat) + "</GrossPaymentAtRate>");
                    Document.Append("<TaxCurrency>");
                    Document.Append("<Currency Code=\"ZAR\">RAND</Currency>");
                    Document.Append("</TaxCurrency>");
                    Document.Append("</TaxSubTotal>");
                }

                //Vatable
                if (Vatable.Count > 0)
                {
                    TotalVatCodes++;
                    //Building Document content
                    Document.Append("<TaxSubTotal>");
                    Document.Append("<TaxRate Code=\"S\">" + String.Format("{0:0.00}", VatRate.VATRate) + "</TaxRate>");
                    Document.Append("<NumberOfLinesAtRate>" + TotalLines + "</NumberOfLinesAtRate>");
                    Document.Append("<TotalValueAtRate>" + String.Format("{0:0.00}", Vatable.Sum(a => a.ExVat)) + "</TotalValueAtRate>");
                    Document.Append("<TaxableValueAtRate>" + String.Format("{0:0.00}", Vatable.Sum(a => a.ExVat)) + "</TaxableValueAtRate>");
                    Document.Append("<TaxAtRate>" + String.Format("{0:0.00}", Vatable.Sum(a => a.VatAmount)) + "</TaxAtRate>");
                    Document.Append("<NetPaymentAtRate>" + String.Format("{0:0.00}", Vatable.Sum(a => a.InclVat)) + "</NetPaymentAtRate>");
                    Document.Append("<GrossPaymentAtRate>" + String.Format("{0:0.00}", Vatable.Sum(a => a.InclVat)) + "</GrossPaymentAtRate>");
                    Document.Append("<TaxCurrency>");
                    Document.Append("<Currency Code=\"ZAR\">RAND</Currency>");
                    Document.Append("</TaxCurrency>");
                    Document.Append("</TaxSubTotal>");
                }

                var VatAmount = (from a in Vatable select a.VatAmount).Sum();
                var NetAmount = (from a in NonVatable select a.InclVat).Sum() + (from a in Vatable select a.InclVat).Sum();


                int NoOfLines = Convert.ToInt16(Vatable.Count()) + Convert.ToInt16(NonVatable.Count());

                Document.Append("<InvoiceTotal>");
                Document.Append("<NumberOfLines>" + NoOfLines + "</NumberOfLines>");
                Document.Append("<NumberOfTaxRates>" + TotalVatCodes + "</NumberOfTaxRates>");
                Document.Append("<LineValueTotal>" + String.Format("{0:0.00}", SumNonVat + SumVatLinesExclVat) + "</LineValueTotal>");
                Document.Append("<TaxableTotal>" + String.Format("{0:0.00}", SumNonVat + SumVatLinesExclVat) + "</TaxableTotal>");
                Document.Append("<TaxTotal>" + String.Format("{0:0.00}", VatAmount) + "</TaxTotal>");
                Document.Append("<NetPaymentTotal>" + String.Format("{0:0.00}", NetAmount) + "</NetPaymentTotal>");
                Document.Append("<GrossPaymentTotal>" + String.Format("{0:0.00}", NetAmount) + "</GrossPaymentTotal>");
                Document.Append("</InvoiceTotal>");

                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void MoveAllFiles(string Path, bool Passed)
        {
            try
            {
                String directoryName = Path;
                if (Passed)
                {
                    directoryName = directoryName + @"\Processed\";
                }
                else
                {
                    directoryName = directoryName + @"\Failed\";
                }
                DirectoryInfo dirInfo = new DirectoryInfo(directoryName);
                if (dirInfo.Exists == false)
                    Directory.CreateDirectory(directoryName);

                List<String> MyFiles = Directory
                                   .GetFiles(Path + @"\Unprocessed\", "*.*", SearchOption.AllDirectories).ToList();

                foreach (string file in MyFiles)
                {
                    FileInfo mFile = new FileInfo(file);
                    // to remove name collisions
                    if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                    {
                        mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void SendEmail(CustomerInvoiceViewModel model)
        {
            try
            {

                var emailSettings = (from a in wdb.mtInvoiceExtractSettings where a.CustomerClass == model.CustomerClass select a).FirstOrDefault();

                Mail objMail = new Mail();
                objMail.From = emailSettings.FromEmail;
                objMail.To = emailSettings.ToEmail;
                objMail.Subject = "EDI Invoices - " + model.CustomerClass;


                StringBuilder strBody = new StringBuilder();

                strBody.Append("<table style=\"border-collapse: collapse; width: 100%;\">");
                strBody.Append("<thead style=\"padding: 0.25rem;border: 1px solid #ccc;\">");
                strBody.Append("<tr style=\"text-align:left;font-weight:bold;\">");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">#</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">Invoice</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">CustomerPoNumber</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">InvoiceDate</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">SalesOrder</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">CustomerRef</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">CurrencyValue</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">Branch</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">Customer</th>");
                strBody.Append("<th style=\"text-align:left;font-weight:bold;\">DocumentType</th>");
                strBody.Append("</tr>");
                strBody.Append("</thead>");
                strBody.Append("<tbody>");

                int i = 0;
                foreach (var item in model.Invoices)
                {
                    if (item.DoExtract == true)
                    {
                        i++;
                        if (IsOdd(i))
                        {
                            strBody.Append("<tr style=\"background: #eee;\">");
                            strBody.Append("<td>" + item.RowNumber + "</td>");
                            strBody.Append("<td>" + item.Invoice + "</td>");
                            strBody.Append("<td>" + item.CustomerPoNumber + "</td>");
                            strBody.Append("<td>" + item.InvoiceDate + "</td>");
                            strBody.Append("<td>" + item.SalesOrder + "</td>");
                            strBody.Append("<td>" + item.CustomerRef + "</td>");
                            strBody.Append("<td>" + item.CurrencyValue + "</td>");
                            strBody.Append("<td>" + item.Branch + "</td>");
                            strBody.Append("<td>" + item.Customer + "</td>");
                            strBody.Append("<td>" + item.DocumentType + "</td>");
                            strBody.Append("</tr>");
                        }
                        else
                        {
                            strBody.Append("<tr>");
                            strBody.Append("<td>" + item.RowNumber + "</td>");
                            strBody.Append("<td>" + item.Invoice + "</td>");
                            strBody.Append("<td>" + item.CustomerPoNumber + "</td>");
                            strBody.Append("<td>" + item.InvoiceDate + "</td>");
                            strBody.Append("<td>" + item.SalesOrder + "</td>");
                            strBody.Append("<td>" + item.CustomerRef + "</td>");
                            strBody.Append("<td>" + item.CurrencyValue + "</td>");
                            strBody.Append("<td>" + item.Branch + "</td>");
                            strBody.Append("<td>" + item.Customer + "</td>");
                            strBody.Append("<td>" + item.DocumentType + "</td>");
                            strBody.Append("</tr>");
                        }

                    }
                }
                strBody.Append("</tbody>");
                strBody.Append("</table>");


                objMail.Body = strBody.ToString();
                objMail.CC = emailSettings.FromEmail;



                List<string> attachments = new List<string>();
                Email _email = new Email();
                _email.SendEmail(objMail, attachments, "InvoiceExtract");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool IsOdd(int value)
        {
            return value % 2 != 0;
        }


    }
}