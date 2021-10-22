using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class SupplierContractsBL
    {
        public string BuildContractPriceParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the PO Supplier Contract Posting Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostSupplierContract xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTSC.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<UseSupplierStockCode>N</UseSupplierStockCode>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostSupplierContract>");

            return Parameter.ToString();
        }



        public string BuildContractPriceXrefDocument(List<sp_GetSupContractsStockCodesBySupplier_Result> detail, string Contract, string StartDate, string ExpiryDate)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("This is an example XML instance to demonstrate");
            Document.Append("use of the PO Supplier Contract Posting Business Object");
            Document.Append("-->");
            Document.Append("<PostSupplierContract xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTSCDOC.XSD\">");
            foreach (var item in detail)
            {

                if (item.Price != 0)
                {
                    Document.Append("<Item>");
                    Document.Append("<Supplier><![CDATA[" + item.Supplier + "]]></Supplier>");
                    Document.Append("<StockCode><![CDATA[" + item.StockCode + "]]></StockCode>");
                    Document.Append("<Contract><![CDATA[" + Contract + "]]></Contract>");
                    Document.Append("<PriceReference><![CDATA[" + Contract + "]]></PriceReference>");
                    Document.Append("<PurchasePrice><![CDATA[" + item.Price + "]]></PurchasePrice>");
                    Document.Append("<PriceUom><![CDATA[" + item.StockUom + "]]></PriceUom>");
                    //Document.Append("<DiscountCode>A</DiscountCode>");
                    //Document.Append("<PackSize>10</PackSize>");
                    //Document.Append("<MinimumQty>10.000</MinimumQty>");
                    Document.Append("<PriceStartDate>" + Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd") + "</PriceStartDate>");

                    if (!string.IsNullOrEmpty(ExpiryDate))
                    {
                        Document.Append("<PriceExpiryDate>" + Convert.ToDateTime(ExpiryDate).ToString("yyyy-MM-dd") + "</PriceExpiryDate>");
                    }


                    //Document.Append("<PriceQuoteDate>2006-12-12</PriceQuoteDate>");
                    //Document.Append("<QuoteLeadTime>12</QuoteLeadTime>");
                    //Document.Append("<FreightCharges>10.00</FreightCharges>");
                    //Document.Append("<DiscPct1>10.00</DiscPct1>");
                    //Document.Append("<DiscPct2>8.00</DiscPct2>");
                    //Document.Append("<DiscPct3>3.00</DiscPct3>");
                    //Document.Append("<MinimumQtyUom>EA</MinimumQtyUom>");
                    //Document.Append("<PriceComment>price you pay</PriceComment>");
                    //Document.Append("<ShippingInstrs>with care</ShippingInstrs>");
                    //Document.Append("<FreightComment>Use wrap</FreightComment>");
                    Document.Append("</Item>");
                }
            }

            Document.Append("</PostSupplierContract>");
            return Document.ToString();
        }



        public string BuildContractPriceDocument(List<sp_GetSupplierContractsPricingData_Result> detail)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("This is an example XML instance to demonstrate");
            Document.Append("use of the PO Supplier Contract Posting Business Object");
            Document.Append("-->");
            Document.Append("<PostSupplierContract xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTSCDOC.XSD\">");
            foreach (var item in detail)
            {


                Document.Append("<Item>");
                Document.Append("<Supplier><![CDATA[" + item.Supplier + "]]></Supplier>");
                Document.Append("<StockCode><![CDATA[" + item.StockCode + "]]></StockCode>");
                Document.Append("<Contract><![CDATA[" + item.Contract + "]]></Contract>");
                Document.Append("<PriceReference><![CDATA[" + item.Contract + "]]></PriceReference>");
                Document.Append("<PurchasePrice><![CDATA[" + item.NewPrice + "]]></PurchasePrice>");
                Document.Append("<PriceUom><![CDATA[" + item.PriceUom + "]]></PriceUom>");
                //Document.Append("<DiscountCode>A</DiscountCode>");
                //Document.Append("<PackSize>10</PackSize>");
                //Document.Append("<MinimumQty>10.000</MinimumQty>");
                Document.Append("<PriceStartDate>" + Convert.ToDateTime(item.StartDate).ToString("yyyy-MM-dd") + "</PriceStartDate>");

                if (!string.IsNullOrEmpty(item.ExpiryDate))
                {
                    Document.Append("<PriceExpiryDate>" + Convert.ToDateTime(item.ExpiryDate).ToString("yyyy-MM-dd") + "</PriceExpiryDate>");
                }


                //Document.Append("<PriceQuoteDate>2006-12-12</PriceQuoteDate>");
                //Document.Append("<QuoteLeadTime>12</QuoteLeadTime>");
                //Document.Append("<FreightCharges>10.00</FreightCharges>");
                //Document.Append("<DiscPct1>10.00</DiscPct1>");
                //Document.Append("<DiscPct2>8.00</DiscPct2>");
                //Document.Append("<DiscPct3>3.00</DiscPct3>");
                //Document.Append("<MinimumQtyUom>EA</MinimumQtyUom>");
                //Document.Append("<PriceComment>price you pay</PriceComment>");
                //Document.Append("<ShippingInstrs>with care</ShippingInstrs>");
                //Document.Append("<FreightComment>Use wrap</FreightComment>");
                Document.Append("</Item>");

            }

            Document.Append("</PostSupplierContract>");
            return Document.ToString();

        }
    }
}