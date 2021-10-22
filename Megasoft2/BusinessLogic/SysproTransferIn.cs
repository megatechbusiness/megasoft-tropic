using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class SysproTransferIn
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproCore objSyspro = new SysproCore();
        public string ValidateBarcode(string details)
        {
            try
            {
                //List<TransferIn> myDeserializedObjList = (List<TransferIn>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<TransferIn>));
                //if (myDeserializedObjList.Count > 0)
                //{
                //    foreach (var item in myDeserializedObjList)
                //    {
                //        var StockCodeCheck = db.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                //        if (StockCodeCheck == null)
                //        {
                //            return "StockCode not found!.";
                //        }

                //        var StockWarehouseCheck = db.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse)).FirstOrDefault();
                //        if (StockWarehouseCheck == null)
                //        {
                //            return "StockCode not stocked in Warehouse " + item.Warehouse + "!.";
                //        }

                //        if (item.Quantity == 0)
                //        {
                //            return "Quantity cannot be zero!";
                //        }

                //        var TraceableCheck = db.InvMasters.Where(a => a.StockCode.Equals(item.StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                //        if (TraceableCheck != null)
                //        {
                //            //StockCode is Traceable -- Lot number required
                //            if (string.IsNullOrEmpty(item.LotNumber))
                //            {
                //                return "StockCode is Lot Traceable. Lot number required";
                //            }
                //            else
                //            {
                //                var LotCheck = db.LotDetails.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse) && a.Lot.Equals(item.LotNumber)).FirstOrDefault();
                //                if (LotCheck == null)
                //                {
                //                    return "Lot " + item.LotNumber + " not found for StockCode " + item.StockCode + " in Warehouse " + item.Warehouse + "!";
                //                }
                //                else
                //                {
                //                    var LotQtyCheck = db.LotDetails.Where(a => a.StockCode.Equals(item.StockCode)
                //                                                            && a.Warehouse.Equals(item.Warehouse)
                //                                                            && a.Lot.Equals(item.LotNumber)
                //                                                          ).Select(a => a.QtyOnHand).Sum();
                //                    if (LotQtyCheck < item.Quantity)
                //                    {
                //                        return "Quantity on hand is less than Quantity to transfer for Lot " + item.LotNumber + "!";
                //                    }
                //                    else
                //                    {
                //                        return "";
                //                    }
                //                }
                //            }
                //        }
                //        else
                //        {
                //            //StockCode is not Traceable -- Check Quantity
                //            var QtyCheck = db.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode)
                //                                                          && a.Warehouse.Equals(item.Warehouse)
                //                                                          ).Select(a => a.QtyOnHand).Sum();
                //            if (QtyCheck < item.Quantity)
                //            {
                //                return "Quantity on hand is less than Quantity to transfer for StockCode " + item.StockCode + "!";
                //            }
                //            else
                //            {
                //                return "";
                //            }
                //        }
                //    }
                //}
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string PostTransferIn(string Warehouse, string GtrReference)
        {
            try
            {
                var result = db.sp_GetGtrDetailByReference(GtrReference).ToList();
                if(result.Count > 0)
                {

                    string Guid = objSyspro.SysproLogin();
                    if (string.IsNullOrEmpty(Guid))
                    {
                        return "Failed to Log in to Syspro.";
                    }


                    //Declaration
                    StringBuilder Document = new StringBuilder();

                    //Building Document content
                    Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                    Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                    Document.Append("<!--");
                    Document.Append("Sample XML for the Inventory GIT Warehouse Transfer IN Business Object");
                    Document.Append("-->");
                    Document.Append("<PostInvGitWhTransferIn xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMNDOC.XSD\">");
                    foreach(var item in result)
                    {
                        var TraceableType = (from a in db.InvMasters where a.StockCode.Equals(item.StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod }).FirstOrDefault();

                        Document.Append("<Item>");
                        Document.Append("<Journal />");
                        Document.Append("<Key>");
                        Document.Append("<GtrReference>" + item.GtrReference + "</GtrReference>");
                        Document.Append("<SourceWarehouse>" + item.SourceWarehouse + "</SourceWarehouse>");
                        Document.Append("<TargetWarehouse>" + item.TargetWarehouse + "</TargetWarehouse>");
                        Document.Append("<LineNumber>" + item.Line + "</LineNumber>");
                        Document.Append("</Key>");
                        Document.Append("<Quantity>" + item.Quantity + "</Quantity>");
                        Document.Append("<Units />");
                        Document.Append("<Pieces />");
                        //Document.Append("<ActionBackToSource>A</ActionBackToSource>");
                        Document.Append("<TargetBin />");
                        //Document.Append("<ExpenseLedgerCode>00-1540</ExpenseLedgerCode>");
                        //Document.Append("<PasswordForLedgerCode />");
                        if (TraceableType.TraceableType == "T")
                        {
                            Document.Append("<Lot>");
                            Document.Append("<LotNumber>" + item.ReelNo + "</LotNumber>");
                            Document.Append("<LotBin></LotBin>");
                            Document.Append("</Lot>");
                        }
                        
                        //Document.Append("<Serials>");
                        //Document.Append("<SerialNumber>0205</SerialNumber>");
                        //Document.Append("<SerialQuantity>10</SerialQuantity>");
                        //Document.Append("<SerialUnits />");
                        //Document.Append("<SerialPieces />");
                        //Document.Append("</Serials>");
                        //Document.Append("<Serials>");
                        //Document.Append("<SerialNumber>0206</SerialNumber>");
                        //Document.Append("<SerialQuantity>2</SerialQuantity>");
                        //Document.Append("<SerialUnits />");
                        //Document.Append("<SerialPieces />");
                        //Document.Append("</Serials>");
                        //Document.Append("<SourceBins>");
                        //Document.Append("<BinLocation>E1</BinLocation>");
                        //Document.Append("<BinQuantity>5.000</BinQuantity>");
                        //Document.Append("<BinUnits />");
                        //Document.Append("<BinPieces />");
                        //Document.Append("</SourceBins>");
                        //Document.Append("<SourceBins>");
                        //Document.Append("<BinLocation>E2</BinLocation>");
                        //Document.Append("<BinQuantity>2.000</BinQuantity>");
                        //Document.Append("<BinUnits />");
                        //Document.Append("<BinPieces />");
                        //Document.Append("</SourceBins>");
                        //Document.Append("<ApplyCostMultiplier>Y</ApplyCostMultiplier>");
                        //Document.Append("<CostMultiplier />");
                        //Document.Append("<NonMerchandiseCost>150.00</NonMerchandiseCost>");
                        //Document.Append("<NonMerchandiseDistribution>");
                        //Document.Append("<NmReference>Cost Ref</NmReference>");
                        //Document.Append("<NmLedgerCode>30-6200</NmLedgerCode>");
                        //Document.Append("<NmAmount>150.00</NmAmount>");
                        //Document.Append("</NonMerchandiseDistribution>");
                        Document.Append("<Notation>" + item.ReelNo + "</Notation>");
                        Document.Append("<eSignature />");
                        Document.Append("</Item>");
                        

                    }
                    Document.Append("</PostInvGitWhTransferIn>");

                    string XmlOut, ErrorMessage;
                    XmlOut = objSyspro.SysproPost(Guid, this.BuildTransferInParameter(), Document.ToString(), "INVTMN");
                    objSyspro.SysproLogoff(Guid);
                    ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                    string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        foreach(var item in result)
                        {
                            db.sp_SaveInvTransfer("TransferIn", item.SourceWarehouse, "", item.TargetWarehouse, "", item.StockCode, item.ReelNo, item.Quantity, Journal, item.GtrReference, HttpContext.Current.User.Identity.Name.ToUpper());
                        }
                        return "Posting Complete. Jnl : " + Journal;
                    }
                    else
                    {
                        return "Error : " + ErrorMessage;
                    }
                }
                else
                {
                    return "Error : No data found.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public string BuildTransferInParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Inventory GIT Warehouse Transfer IN Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostInvGitWhTransferIn xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMN.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<UpdateOriginatingOrder>N</UpdateOriginatingOrder>");
            Parameter.Append("<UseDefaultWarehouseBin>Y</UseDefaultWarehouseBin>");
            Parameter.Append("<DefaultBinToUse />");
            Parameter.Append("<CreateBinLocation>N</CreateBinLocation>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostInvGitWhTransferIn>");

            return Parameter.ToString();
        }



        //public string PostTransferInByItem(List<WarehouseTransfer> result)
        //{
        //    try
        //    {
        //        if (result.Count > 0)
        //        {

        //            string Guid = objSyspro.SysproLogin();
        //            if (string.IsNullOrEmpty(Guid))
        //            {
        //                return "Failed to Log in to Syspro.";
        //            }


        //            //Declaration
        //            StringBuilder Document = new StringBuilder();

        //            //Building Document content
        //            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
        //            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
        //            Document.Append("<!--");
        //            Document.Append("Sample XML for the Inventory GIT Warehouse Transfer IN Business Object");
        //            Document.Append("-->");
        //            Document.Append("<PostInvGitWhTransferIn xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMNDOC.XSD\">");
        //            foreach (var item in result)
        //            {

        //                Document.Append("<Item>");
        //                Document.Append("<Journal />");
        //                Document.Append("<Key>");
        //                Document.Append("<GtrReference>" + item.GtrReference + "</GtrReference>");
        //                Document.Append("<SourceWarehouse>" + item.SourceWarehouse + "</SourceWarehouse>");
        //                Document.Append("<TargetWarehouse>" + item.TargetWarehouse + "</TargetWarehouse>");
        //                Document.Append("<LineNumber>" + item.Line + "</LineNumber>");
        //                Document.Append("</Key>");
        //                Document.Append("<Quantity>" + item.Quantity + "</Quantity>");
        //                Document.Append("<Units />");
        //                Document.Append("<Pieces />");
        //                //Document.Append("<ActionBackToSource>A</ActionBackToSource>");
        //                Document.Append("<TargetBin />");
        //                //Document.Append("<ExpenseLedgerCode>00-1540</ExpenseLedgerCode>");
        //                //Document.Append("<PasswordForLedgerCode />");
        //                //Document.Append("<Lot>");
        //                //Document.Append("<LotNumber>21</LotNumber>");
        //                //Document.Append("<LotBin>E1</LotBin>");
        //                //Document.Append("</Lot>");
        //                //Document.Append("<Serials>");
        //                //Document.Append("<SerialNumber>0205</SerialNumber>");
        //                //Document.Append("<SerialQuantity>10</SerialQuantity>");
        //                //Document.Append("<SerialUnits />");
        //                //Document.Append("<SerialPieces />");
        //                //Document.Append("</Serials>");
        //                //Document.Append("<Serials>");
        //                //Document.Append("<SerialNumber>0206</SerialNumber>");
        //                //Document.Append("<SerialQuantity>2</SerialQuantity>");
        //                //Document.Append("<SerialUnits />");
        //                //Document.Append("<SerialPieces />");
        //                //Document.Append("</Serials>");
        //                //Document.Append("<SourceBins>");
        //                //Document.Append("<BinLocation>E1</BinLocation>");
        //                //Document.Append("<BinQuantity>5.000</BinQuantity>");
        //                //Document.Append("<BinUnits />");
        //                //Document.Append("<BinPieces />");
        //                //Document.Append("</SourceBins>");
        //                //Document.Append("<SourceBins>");
        //                //Document.Append("<BinLocation>E2</BinLocation>");
        //                //Document.Append("<BinQuantity>2.000</BinQuantity>");
        //                //Document.Append("<BinUnits />");
        //                //Document.Append("<BinPieces />");
        //                //Document.Append("</SourceBins>");
        //                //Document.Append("<ApplyCostMultiplier>Y</ApplyCostMultiplier>");
        //                //Document.Append("<CostMultiplier />");
        //                //Document.Append("<NonMerchandiseCost>150.00</NonMerchandiseCost>");
        //                //Document.Append("<NonMerchandiseDistribution>");
        //                //Document.Append("<NmReference>Cost Ref</NmReference>");
        //                //Document.Append("<NmLedgerCode>30-6200</NmLedgerCode>");
        //                //Document.Append("<NmAmount>150.00</NmAmount>");
        //                //Document.Append("</NonMerchandiseDistribution>");
        //                Document.Append("<Notation>" + item.ReelNo + "</Notation>");
        //                Document.Append("<eSignature />");
        //                Document.Append("</Item>");


        //            }
        //            Document.Append("</PostInvGitWhTransferIn>");

        //            string XmlOut, ErrorMessage;
        //            XmlOut = objSyspro.SysproPost(Guid, this.BuildTransferInParameter(), Document.ToString(), "INVTMN");
        //            objSyspro.SysproLogoff(Guid);
        //            ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
        //            string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
        //            if (string.IsNullOrEmpty(ErrorMessage))
        //            {
        //                foreach (var item in result)
        //                {
        //                    db.sp_SaveInvTransfer("TransferIn", item.SourceWarehouse, "", item.TargetWarehouse, "", item.StockCode, item.ReelNo, item.Quantity, Journal, item.GtrReference, HttpContext.Current.User.Identity.Name.ToUpper());
        //                }
        //                return "Posting Complete. Jnl : " + Journal;
        //            }
        //            else
        //            {
        //                return "Error : " + ErrorMessage;
        //            }
        //        }
        //        else
        //        {
        //            return "Error : No data found.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
    }
}