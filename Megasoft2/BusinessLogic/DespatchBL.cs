using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class DespatchBL
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();

        public string BuildReleaseDocument(int WmsId, string FromWhere, int SalesOrderLine)
        {
            try
            {
                string username = HttpContext.Current.User.Identity.Name.ToUpper();
                List<mtWmsOrderDetail> result;
                if(FromWhere == "Picker")
                {
                    result = (from a in wdb.mtWmsOrderDetails where a.WmsId == WmsId && a.SalesOrderLine == SalesOrderLine && a.QuantityPicked != 0 && a.Picker == username  select a).ToList();
                }
                else
                {
                    result = (from a in wdb.mtWmsOrderDetails where a.WmsId == WmsId && a.SalesOrderLine == SalesOrderLine && a.QuantityPicked != 0 && a.CheckoutDone == "Y" select a).ToList();
                }
               
                var Order = (from a in wdb.mtWmsOrderMasters where a.WmsId == WmsId && a.SalesOrderLine == SalesOrderLine select a).FirstOrDefault();

                //Declaration
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the Sales Order Back Order Release Business Object");
                Document.Append("-->");
                Document.Append("<PostSorBackOrderRelease xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTBODOC.XSD\">");

                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {

                        Document.Append("<Item>");
                        //Document.Append("<LatestAcceptedLotExpiryDate>2006-09-16</LatestAcceptedLotExpiryDate>");
                        //Document.Append("<LatestAcceptedSerialExpiryDate>2006-09-16</LatestAcceptedSerialExpiryDate>");
                        //Document.Append("<LatestAcceptedSerialScrapDate>2006-09-16</LatestAcceptedSerialScrapDate>");
                        //Document.Append("<ScheduleAllocateDate>2006-10-16</ScheduleAllocateDate>");
                        //Document.Append("<ScheduleLineShipDate>2006-10-16</ScheduleLineShipDate>");
                        //Document.Append("<Customer>000008</Customer>");
                        Document.Append("<SalesOrder>" + Order.SalesOrder + "</SalesOrder>");
                        //Document.Append("<StockCode>LOT100</StockCode>");
                        //Document.Append("<Warehouse>FG</Warehouse>");
                        Document.Append("<Quantity>" + item.QuantityPicked + "</Quantity>");
                        //Document.Append("<ActualShipQty>" + item.QtyOnHand + "</ActualShipQty>");
                        //Document.Append("");
                        Document.Append("<UnitOfMeasure />");
                        Document.Append("<Units />");
                        Document.Append("<Pieces />");
                        Document.Append("<ReleaseFromMultipleLines>N</ReleaseFromMultipleLines>");
                        Document.Append("<SalesOrderLine>" + Order.SalesOrderLine + "</SalesOrderLine>");
                        Document.Append("<CompleteLine>N</CompleteLine>");
                        Document.Append("<AdjustOrderQuantity>Y</AdjustOrderQuantity>");
                        //Document.Append("<Serials>");
                        //Document.Append("<SerialNumber />");
                        //Document.Append("<SerialQuantity />");
                        //Document.Append("<SerialCreationDate />");
                        //Document.Append("<SerialExpiryDate />");
                        //Document.Append("<SerialScrapDate />");
                        //Document.Append("<SerialLocation />");
                        //Document.Append("<SerialUnits />");
                        //Document.Append("<SerialPieces />");
                        //Document.Append("</Serials>");
                        if (Order.TraceableType == "T")
                        {
                            Document.Append("<Lot><![CDATA[" + item.Lot + "]]></Lot>");
                        }
                        Document.Append("<Bins>");
                        Document.Append("<BinLocation><![CDATA[" + item.Lot + "]]></BinLocation>");
                        Document.Append("<BinQuantity>" + item.QuantityPicked + "</BinQuantity>");
                        Document.Append("<BinUnits />");
                        Document.Append("<BinPieces />");
                        Document.Append("</Bins>");

                        Document.Append("<OrderStatus>3</OrderStatus>");
                        Document.Append("<ReleaseFromShip>N</ReleaseFromShip>");
                        Document.Append("<ZeroShipQuantity>N</ZeroShipQuantity>");
                        Document.Append("<AllocateSerialNumbers>N</AllocateSerialNumbers>");
                        Document.Append("<eSignature>");
                        Document.Append("</eSignature>");
                        Document.Append("</Item>");

                    }
                }
                else
                {
                    if(FromWhere == "Picker")
                    {
                        throw new Exception("No picked items found.");
                    }
                    else
                    {
                        return "";
                    }
                }



                Document.Append("</PostSorBackOrderRelease>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildReleaseParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the Sales Order Back Order Release Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostSorBackOrderRelease xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTBO.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<AddQuantityToBatchSerial>N</AddQuantityToBatchSerial>");
            Parameter.Append("<IgnoreAutoDepletion>Y</IgnoreAutoDepletion>");
            Parameter.Append("<ShipKitFromDefaultBin>N</ShipKitFromDefaultBin>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostSorBackOrderRelease>");

            return Parameter.ToString();
        }


        public string PostDespatchCreation(string Guid, int WmsId, int SalesOrderLine)
        {
            try
            {
                var Header = (from a in wdb.mtWmsOrderMasters where a.WmsId == WmsId && a.SalesOrderLine == SalesOrderLine select a).FirstOrDefault();
                string XmlOut = sys.SysproPost(Guid, BuildDespatchParameter(), BuildDespatchNoteDocument(Header.SalesOrder, Header.SalesOrderLine, WmsId, Header.TraceableType), "SORTDN");
                
                return XmlOut;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildDespatchNoteDocument(string SalesOrder, int SalesOrderLine, int WmsId, string TraceableType)
        {
            try
            {

                var result = (from a in wdb.mtWmsOrderDetails where a.WmsId == WmsId && a.SalesOrderLine == SalesOrderLine && a.QuantityPicked != 0 && a.CheckoutDone == "Y" select a).ToList();
                if(result.Count > 0)
                {
                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2018 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("This is an example XML instance to demonstrate");
                    Document.Append("use of the SO Dispatch Note Transaction Posting Business Object");
                    Document.Append("-->");
                    Document.Append("<PostDispatchNotes xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTDNDOC.XSD\">");
                    Document.Append("<DispatchNote>");
                    Document.Append("<DispatchHeader>");
                    Document.Append("<SalesOrder>" + SalesOrder + "</SalesOrder>");
                    //Document.Append("<ShippingInstrs>Ship via Hong Kong</ShippingInstrs>");
                    //Document.Append("<CustomerName>The SYSPRO Outdoors Company</CustomerName>");
                    //Document.Append("<ShipAddress1>This is the alternate delivery address 1</ShipAddress1>");
                    //Document.Append("<ShipAddress2>This is the alternate delivery address 2</ShipAddress2>");
                    //Document.Append("<ShipAddress3>This is the alternate delivery address 3</ShipAddress3>");
                    //Document.Append("<ShipAddress3Locality>This is the delivery address 3 location</ShipAddress3Locality>");
                    //Document.Append("<ShipAddress4>This is the alternate delivery address 4</ShipAddress4>");
                    //Document.Append("<ShipAddress5>This is the alternate delivery address 5</ShipAddress5>");
                    //Document.Append("<ShipPostalCode>90210</ShipPostalCode>");
                    //Document.Append("<ShipGpsLat />");
                    //Document.Append("<ShipGpsLong />");
                    //Document.Append("<LanguageCode />");
                    //Document.Append("<MultiShipCode>2</MultiShipCode>");
                    //Document.Append("<SpecialInstrs>Handle with care</SpecialInstrs>");
                    //Document.Append("<PlannedDispatchDate>2006-12-20</PlannedDispatchDate>");
                    //Document.Append("<UserDefined1 />");
                    //Document.Append("<UserDefined2 />");
                  //  Document.Append("<DispatchNoteNumber />");
                    //Document.Append("<DispatchComments />");
                    //Document.Append("<Nationality />");
                    //Document.Append("<DeliveryTerms />");
                    //Document.Append("<ShippingLocation />");
                    //Document.Append("<TransactionNature />");
                    //Document.Append("<TransportMode />");
                    //Document.Append("<ProcessFlag />");
                    //Document.Append("<State />");
                    //Document.Append("<CountyZip />");
                    //Document.Append("<City />");
                    //Document.Append("<CompanyTaxNumber />");
                    Document.Append("<DispatchStatusPrinted>N</DispatchStatusPrinted>");
                    //Document.Append("<ActualDispatchDate />");
                    //Document.Append("<DocumentFormat />");
                    //Document.Append("<eSignature />");
                    Document.Append("</DispatchHeader>");
                    Document.Append("<DispatchDetails>");

                    foreach(var item in result)
                    {
                        Document.Append("<MerchandiseLine>");
                        Document.Append("<SalesOrderLine>" + SalesOrderLine + "</SalesOrderLine>");
                        Document.Append("<DispatchQty>" + item.QuantityPicked + "</DispatchQty>");
                        //Document.Append("<Units />");
                        //Document.Append("<Pieces />");
                        //Document.Append("<StockingDispatchQty />");
                        if(TraceableType == "T")
                        {
                            Document.Append("<Lot><![CDATA[" + item.Lot + "]]></Lot>");
                        }

                        Document.Append("<Bins>");
                        Document.Append("<BinLocation><![CDATA[" + item.Bin + "]]></BinLocation>");
                        Document.Append("<BinQuantity>" + item.QuantityPicked + "</BinQuantity>");
                        Document.Append("<BinUnits />");
                        Document.Append("<BinPieces />");
                        Document.Append("</Bins>");
                        //Document.Append("<Serials>");
                        //Document.Append("<SerialNumber />");
                        //Document.Append("<SerialQuantity />");
                        //Document.Append("<SerialUnits />");
                        //Document.Append("<SerialPieces />");
                        //Document.Append("<SerialCreationDate />");
                        //Document.Append("<SerialExpiryDate />");
                        //Document.Append("<SerialScrapDate />");
                        //Document.Append("<SerialLocation />");
                        //Document.Append("</Serials>");
                        //Document.Append("<OverOrUnderDispatch>N</OverOrUnderDispatch>");
                        //Document.Append("<DispatchZeroQty />");
                        //Document.Append("<BasisForDispatch>B</BasisForDispatch>");
                        //Document.Append("<BackorderShipBeforeValidation>N</BackorderShipBeforeValidation>");
                        //Document.Append("<BackOrderNonStkShipBeforeValidation>N</BackOrderNonStkShipBeforeValidation>");
                        Document.Append("</MerchandiseLine>");
                    }

                    
                    //Document.Append("<CommentLine>");
                    //Document.Append("<Comment>Ensure saddle is color coded</Comment>");
                    //Document.Append("<AttachedLineNumber>1</AttachedLineNumber>");
                    //Document.Append("<CommentType />");
                    //Document.Append("</CommentLine>");
                    //Document.Append("<MiscChargeLine>");
                    //Document.Append("<MiscChargeValue>78.56</MiscChargeValue>");
                    //Document.Append("<MiscChargeCost>20.00</MiscChargeCost>");
                    //Document.Append("<MiscProductClass>_OTH</MiscProductClass>");
                    //Document.Append("<MiscTaxCode>A</MiscTaxCode>");
                    //Document.Append("<MiscNotTaxable />");
                    //Document.Append("<MiscFstCode>B</MiscFstCode>");
                    //Document.Append("<MiscNotFstTaxable />");
                    //Document.Append("<MiscDescription>Sundry Items</MiscDescription>");
                    //Document.Append("</MiscChargeLine>");
                    //Document.Append("<FreightLine>");
                    //Document.Append("<FreightValue>19.00</FreightValue>");
                    //Document.Append("<FreightCost>12.00</FreightCost>");
                    //Document.Append("<FreightTaxCode>A</FreightTaxCode>");
                    //Document.Append("<FreightNotTaxable />");
                    //Document.Append("<FreightFstCode>B</FreightFstCode>");
                    //Document.Append("<FreightNotFstTaxable />");
                    //Document.Append("</FreightLine>");
                    Document.Append("</DispatchDetails>");
                    Document.Append("</DispatchNote>");
                    Document.Append("</PostDispatchNotes>");

                    return Document.ToString();
                }
                else
                {
                    throw new Exception("No data found to dispatch.");
                }
                

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildDespatchParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2018 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the SO Dispatch Note Transaction Posting Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostDispatchNotes xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTDN.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<BasisForDispatch>B</BasisForDispatch>");
            Parameter.Append("<NonMerchandiseSource>I</NonMerchandiseSource>");
            Parameter.Append("<IgnoreAutoDepletion>Y</IgnoreAutoDepletion>");
            Parameter.Append("<RetainZeroNonMerchCost>N</RetainZeroNonMerchCost>");
            Parameter.Append("<CopyCustomForm>N</CopyCustomForm>");
            Parameter.Append("<AppendToExistingLine>Y</AppendToExistingLine>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostDispatchNotes>");

            return Parameter.ToString();

        }

        public string BuildReleaseShipQtyDocument(List<mtWmsOrderMaster> mtWms)
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
                Document.Append("use of the Sales Order Back Order Release Business Object");
                Document.Append("-->");
                Document.Append("<PostSorBackOrderRelease xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTBODOC.XSD\">");

                foreach (var line in mtWms)
                {
                    var SorDetail = wdb.sp_GetSalesOrderDetailLine(line.SalesOrder, line.SalesOrderLine).FirstOrDefault();
                    if (SorDetail != null)
                    {
                        if (SorDetail.MShipQty > 0)
                        {
                            Document.Append("<Item>");
                            //Document.Append("<LatestAcceptedLotExpiryDate>2006-09-16</LatestAcceptedLotExpiryDate>");
                            //Document.Append("<LatestAcceptedSerialExpiryDate>2006-09-16</LatestAcceptedSerialExpiryDate>");
                            //Document.Append("<LatestAcceptedSerialScrapDate>2006-09-16</LatestAcceptedSerialScrapDate>");
                            //Document.Append("<ScheduleAllocateDate>2006-10-16</ScheduleAllocateDate>");
                            //Document.Append("<ScheduleLineShipDate>2006-10-16</ScheduleLineShipDate>");
                            //Document.Append("<Customer>000008</Customer>");
                            Document.Append("<SalesOrder>" + line.SalesOrder + "</SalesOrder>");
                            //Document.Append("<StockCode>LOT100</StockCode>");
                            //Document.Append("<Warehouse>FG</Warehouse>");
                            //Document.Append("<Quantity>" + item.QuantityPicked + "</Quantity>");
                            //Document.Append("<ActualShipQty>" + item.QtyOnHand + "</ActualShipQty>");
                            //Document.Append("");
                            Document.Append("<UnitOfMeasure />");
                            Document.Append("<Units />");
                            Document.Append("<Pieces />");
                            Document.Append("<ReleaseFromMultipleLines>N</ReleaseFromMultipleLines>");
                            Document.Append("<SalesOrderLine>" + line.SalesOrderLine + "</SalesOrderLine>");
                            Document.Append("<CompleteLine>N</CompleteLine>");
                            Document.Append("<AdjustOrderQuantity>N</AdjustOrderQuantity>");
                            //Document.Append("<Serials>");
                            //Document.Append("<SerialNumber />");
                            //Document.Append("<SerialQuantity />");
                            //Document.Append("<SerialCreationDate />");
                            //Document.Append("<SerialExpiryDate />");
                            //Document.Append("<SerialScrapDate />");
                            //Document.Append("<SerialLocation />");
                            //Document.Append("<SerialUnits />");
                            //Document.Append("<SerialPieces />");
                            //Document.Append("</Serials>");
                            //if (Order.TraceableType == "T")
                            //{
                            //    Document.Append("<Lot><![CDATA[" + item.Lot + "]]></Lot>");
                            //}
                            //Document.Append("<Bins>");
                            //Document.Append("<BinLocation><![CDATA[" + item.Lot + "]]></BinLocation>");
                            //Document.Append("<BinQuantity>" + item.QuantityPicked + "</BinQuantity>");
                            //Document.Append("<BinUnits />");
                            //Document.Append("<BinPieces />");
                            //Document.Append("</Bins>");

                            Document.Append("<OrderStatus>3</OrderStatus>");
                            Document.Append("<ReleaseFromShip>Y</ReleaseFromShip>");
                            Document.Append("<ZeroShipQuantity>Y</ZeroShipQuantity>");
                            Document.Append("<AllocateSerialNumbers>N</AllocateSerialNumbers>");
                            Document.Append("<eSignature>");
                            Document.Append("</eSignature>");
                            Document.Append("</Item>");

                        }
                    }
                    
                }

                Document.Append("</PostSorBackOrderRelease>");

                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public string ZeroShipQty(string Guid, int WmsId, int SalesOrderLine)
        {
            try
            {
                var Lines = wdb.sp_GetWmsDespatchItems(WmsId, SalesOrderLine).Where(a => a.MShipQty != 0).ToList();
                if (Lines.Count > 0)
                {
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("This is an example XML instance to demonstrate");
                    Document.Append("use of the Sales Order Back Order Release Business Object");
                    Document.Append("-->");
                    Document.Append("<PostSorBackOrderRelease xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTBODOC.XSD\">");

                    foreach (var item in Lines)
                    {
                        Document.Append("<Item>");
                        Document.Append("<SalesOrder>" + item.SalesOrder + "</SalesOrder>");
                        Document.Append("<ReleaseFromMultipleLines>N</ReleaseFromMultipleLines>");
                        Document.Append("<SalesOrderLine>" + item.SalesOrderLine + "</SalesOrderLine>");
                        Document.Append("<CompleteLine>N</CompleteLine>");
                        Document.Append("<AdjustOrderQuantity>N</AdjustOrderQuantity>");
                        Document.Append("<OrderStatus>3</OrderStatus>");
                        Document.Append("<ReleaseFromShip>Y</ReleaseFromShip>");
                        Document.Append("<ZeroShipQuantity>Y</ZeroShipQuantity>");
                        Document.Append("<AllocateSerialNumbers>N</AllocateSerialNumbers>");
                        Document.Append("<eSignature>");
                        Document.Append("</eSignature>");
                        Document.Append("</Item>");

                    }
                    Document.Append("</PostSorBackOrderRelease>");

                    string XmlOut = sys.SysproPost(Guid, BuildReleaseParameter(), Document.ToString(), "SORTBO");
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    return ErrorMessage;
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