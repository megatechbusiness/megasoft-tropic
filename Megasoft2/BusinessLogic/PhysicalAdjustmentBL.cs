using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Megasoft2.BusinessLogic
{
    public class PhysicalAdjustmentBL
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproCore sys = new SysproCore();

        public string BuildDocument(string Warehouse, string StockCode, string Bin, string Lot, string Reference, decimal Qty)
        {

            StringBuilder Document = new StringBuilder();
            Document.Append("<Item>");
            Document.Append("<Journal />");
            Document.Append("<Warehouse>" + Warehouse + "</Warehouse>");
            Document.Append("<StockCode>" + StockCode + "</StockCode>");
            Document.Append("<Version />");
            Document.Append("<Release />");
            Document.Append("<Quantity>" + Qty + "</Quantity>");
            Document.Append("<UnitOfMeasure />");
            Document.Append("<Units />");
            Document.Append("<Pieces />");
            Document.Append("<BinLocation>" + Bin + "</BinLocation>");
            Document.Append("<FifoBucket />");
            Document.Append("<Lot>" + Lot + "</Lot>");
            Document.Append("<Reference>" + Reference + "</Reference>");
            Document.Append("<Notation>" + Reference + "</Notation>");
            //Document.Append("<LedgerCode>30-5620</LedgerCode>");
            Document.Append("<PasswordForLedgerCode />");
            Document.Append("</Item>");

            return Document.ToString();
        }

        public string BuildParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Inventory Adjustments Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostInvAdjustments xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMA.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<PhysicalCount>N</PhysicalCount>");
            Parameter.Append("<PostingPeriod>C</PostingPeriod>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<IgnoreAnalysis>Y</IgnoreAnalysis>");
            Parameter.Append("<AdjustExpiredLots>Y</AdjustExpiredLots>");
            Parameter.Append("<UpdateOriginalQuantityReceived>N</UpdateOriginalQuantityReceived>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostInvAdjustments>");

            return Parameter.ToString();

        }

        public string PostAdjustment(PhysicalAdjustment model)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Inventory Adjustments Business Object");
                Document.Append("-->");
                Document.Append("<PostInvAdjustments xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMADOC.XSD\">");

                //Ignore Zero Values
                var adjustableData = (from a in model.Stock where a.NewQty < 0 select a).ToList();


                foreach(var item in adjustableData)
                {
                    Document.Append(this.BuildDocument(item.Warehouse, item.StockCode, item.Bin, item.Lot, model.Reference, (decimal)item.NewQty));
                }
                Document.Append("</PostInvAdjustments>");

                string Guid = sys.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    return "Failed to Log in to Syspro.";
                }

                string ErrorMessage;

                string XmlOut = sys.SysproPost(Guid, this.BuildParameter(), Document.ToString(), "INVTMA");

                sys.SysproLogoff(Guid);
                ErrorMessage = sys.GetXmlErrors(XmlOut);
                string Journal = sys.GetFirstXmlValue(XmlOut, "Journal");


                if(!string.IsNullOrEmpty(ErrorMessage))
                {
                    return ErrorMessage;
                }
                else
                {
                    var xDoc = XDocument.Parse(XmlOut.ToString());
                    var GlJournal = (from p in xDoc.Descendants("postinvadjustments").Descendants("GlJournal") select p).FirstOrDefault();
                    return "Posted Successfully! Journal : " + Journal + " GL Journal : " + GlJournal.Element("GlJournal").Value;
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}