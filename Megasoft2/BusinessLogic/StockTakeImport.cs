using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class StockTakeImport
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        public string BuildStockTakeDoc(string Warehouse, string Replace, string Reference)
        {
            try
            {
                var detail = db.sp_GetStockTakeCaptureByWarehouse(Warehouse).ToList();

                if (detail.Count > 0)
                {
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Stock Take Detail Line Capture Business Object");
                    Document.Append("-->");
                    Document.Append("<StockTake xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTSCDOC.XSD\">");
                    Document.Append("<StockTakeDetail>");

                    foreach (var item in detail)
                    {
                        Document.Append("<Item>");
                        Document.Append("<LineType>S</LineType>");
                        Document.Append("<WarehouseCode>" + Warehouse + "</WarehouseCode>");
                        Document.Append("<StockCode>" + item.StockCode + "</StockCode>");
                        Document.Append("<QtyCaptured>" + item.Quantity + "</QtyCaptured>");
                        if (string.IsNullOrWhiteSpace(item.Bin))
                        {
                            Document.Append("<Bin>" + item.Warehouse + "</Bin>");
                        }
                        else
                        {
                            Document.Append("<Bin>" + item.Bin + "</Bin>");
                        }

                        Document.Append("<Reference>" + Reference + "</Reference>");
                        Document.Append("<Serial>" + item.SerialNumber + "</Serial>");
                        //Document.Append("<Pieces>0</Pieces>");
                        Document.Append("<Lot>" + item.Lot + "</Lot>");
                        //Document.Append("<LotExpiryDate>" + Convert.ToDateTime(item.lo + "</LotExpiryDate>");
                        //Document.Append("<Version>44</Version>");
                        //Document.Append("<Release>21</Release>");
                        //Document.Append("<TicketNumber>000465</TicketNumber>");
                        //Document.Append("<UnitOfMeasure>EA</UnitOfMeasure>");
                        Document.Append("</Item>");
                    }



                    Document.Append("</StockTakeDetail>");
                    Document.Append("</StockTake>");

                    return Document.ToString();
                }
                return "";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildStockTakeParameter(string Warehouse, string Increase)
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Stock Take Import Business Object");
            Parameter.Append("-->");
            Parameter.Append("<StockTake xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTSC.XSD\">");
            Parameter.Append("<Key>");
            Parameter.Append("<WarehouseCode>" + Warehouse + "</WarehouseCode>");
            Parameter.Append("</Key>");
            Parameter.Append("<Option>");
            Parameter.Append("<CreateBins>Y</CreateBins>");
            Parameter.Append("<CreateSerials>N</CreateSerials>");
            Parameter.Append("<CreateLots>Y</CreateLots>");
            Parameter.Append("<ValidateReturnsAll>N</ValidateReturnsAll>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<IgnoreOtherWhsItems>Y</IgnoreOtherWhsItems>");
            Parameter.Append("<CaptureTicketNumbers>N</CaptureTicketNumbers>");
            Parameter.Append("<TicketNumbersExist>N</TicketNumbersExist>");
            if (Increase == "Increase")
            {
                Parameter.Append("<DefaultCaptureMethod>I</DefaultCaptureMethod>");
            }
            else
            {
                Parameter.Append("<DefaultCaptureMethod>R</DefaultCaptureMethod>");
            }
            Parameter.Append("<XslStylesheet />");
            Parameter.Append("</Option>");
            Parameter.Append("</StockTake>");

            return Parameter.ToString();

        }



        public string BuildStockTakeDocForReview(List<sp_GetStockReview_Result> detail, string Reference)
        {
            try
            {


                if (detail.Count > 0)
                {
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Stock Take Detail Line Capture Business Object");
                    Document.Append("-->");
                    Document.Append("<StockTake xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTSCDOC.XSD\">");
                    Document.Append("<StockTakeDetail>");

                    foreach (var item in detail)
                    {
                        Document.Append("<Item>");
                        Document.Append("<LineType>S</LineType>");
                        Document.Append("<WarehouseCode>" + item.Warehouse + "</WarehouseCode>");
                        Document.Append("<StockCode>" + item.StockCode + "</StockCode>");
                        Document.Append("<QtyCaptured>" + item.CapturedQty + "</QtyCaptured>");
                        Document.Append("<Bin>" + item.Bin + "</Bin>");
                        Document.Append("<Reference>" + Reference + "</Reference>");
                        //Document.Append("<Serial>" + item.SerialNumber + "</Serial>");
                        //Document.Append("<Pieces>0</Pieces>");
                        Document.Append("<Lot>" + item.Lot + "</Lot>");
                        //Document.Append("<LotExpiryDate>" + Convert.ToDateTime(item.lo + "</LotExpiryDate>");
                        //Document.Append("<Version>44</Version>");
                        //Document.Append("<Release>21</Release>");
                        //Document.Append("<TicketNumber>000465</TicketNumber>");
                        //Document.Append("<UnitOfMeasure>EA</UnitOfMeasure>");
                        Document.Append("</Item>");
                    }



                    Document.Append("</StockTakeDetail>");
                    Document.Append("</StockTake>");

                    return Document.ToString();
                }
                return "";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



    }
}