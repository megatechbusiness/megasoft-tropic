using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class SysproWarehouseTransfer
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore objSyspro = new SysproCore();
        public string ValidateBarcode(string details)
        {
            try
            {
                List<WarehouseTransfer> myDeserializedObjList = (List<WarehouseTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WarehouseTransfer>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var StockCodeCheck = db.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                        if (StockCodeCheck == null)
                        {
                            return "StockCode not found!.";
                        }

                        var StockWarehouseCheck = db.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.SourceWarehouse)).FirstOrDefault();
                        if (StockWarehouseCheck == null)
                        {
                            return "StockCode not stocked in Warehouse " + item.SourceWarehouse + "!.";
                        }
                        if(!string.IsNullOrEmpty(item.DestinationWarehouse))
                        {
                            var DestinationStockWarehouseCheck = db.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.DestinationWarehouse)).FirstOrDefault();
                            if (DestinationStockWarehouseCheck == null)
                            {
                                return "StockCode not stocked in Warehouse " + item.DestinationWarehouse + "!.";
                            }
                        }
                        if (item.Quantity == 0)
                        {
                            return "Quantity cannot be zero!";
                        }

                        var TraceableCheck = db.InvMasters.Where(a => a.StockCode.Equals(item.StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                        if (TraceableCheck != null)
                        {
                            //StockCode is Traceable -- Lot number required
                            if (string.IsNullOrEmpty(item.LotNumber))
                            {
                                return "StockCode is Lot Traceable. Lot number required";
                            }
                            else
                            {
                                var LotCheck = db.LotDetails.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.SourceWarehouse) && a.Lot.Equals(item.LotNumber)).FirstOrDefault();
                                if (LotCheck == null)
                                {
                                    return "Lot " + item.LotNumber + " not found for StockCode " + item.StockCode + " in Warehouse " + item.SourceWarehouse + "!";
                                }
                                else
                                {
                                    var LotQtyCheck = db.LotDetails.Where(a => a.StockCode.Equals(item.StockCode)
                                                                            && a.Warehouse.Equals(item.SourceWarehouse)
                                                                            && a.Lot.Equals(item.LotNumber)
                                                                          ).Select(a => a.QtyOnHand).Sum();
                                    if (LotQtyCheck < item.Quantity)
                                    {
                                        return "Quantity on hand is less than Quantity to transfer for Lot " + item.LotNumber + "!";
                                    }
                                    else
                                    {
                                        return "";
                                    }
                                }
                            }
                        }
                        else
                        {
                            //StockCode is not Traceable -- Check Quantity
                            var QtyCheck = db.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode)
                                                                          && a.Warehouse.Equals(item.SourceWarehouse)
                                                                          ).Select(a => a.QtyOnHand).Sum();
                            if (QtyCheck < item.Quantity)
                            {
                                return "Quantity on hand is less than Quantity to transfer for StockCode " + item.StockCode + "!";
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public List<LabelPrintPoLine> GetStockCodeCrossRef(string details)
        {
            try
            {
                LabelPrintPoLine obj = new LabelPrintPoLine();
                List<WarehouseTransfer> myDeserializedObjList = (List<WarehouseTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WarehouseTransfer>));
                if (myDeserializedObjList.Count > 0)
                {
                    string SupplierStockCode = myDeserializedObjList.FirstOrDefault().Barcode;
                    string Warehouse = myDeserializedObjList.FirstOrDefault().SourceWarehouse;

                    //First check if barcode in import table
                    var LotNo = SupplierStockCode.Substring(0, 9);
                    var import = (from a in db.mtPorDeliveryImports where a.Lot == LotNo select a).FirstOrDefault();
                    if(import != null)
                    {
                        var det = (from a in db.LotDetails where a.Warehouse == Warehouse && a.StockCode == import.StockCode && a.Lot == import.Lot && a.QtyOnHand != 0 select a).FirstOrDefault();
                        obj.StockCode = import.StockCode;
                        obj.ReelNumber = import.Lot;
                        obj.ReelQuantity = det.QtyOnHand;
                        List<LabelPrintPoLine> objOut = new List<LabelPrintPoLine>();
                        objOut.Add(obj);
                        return objOut;
                    }


                    var result = db.sp_GetStockCodeBySupplierCode(SupplierStockCode).FirstOrDefault();
                    
                    if(result == null)
                    {
                        List<LabelPrintPoLine> objOut = new List<LabelPrintPoLine>();
                        objOut.Add(obj);
                        return objOut;
                    }
                    else
                    {
                        obj.Supplier = result.Supplier;
                        obj.StockCode = result.StockCode;
                        List<LabelPrintPoLine> objOut = new List<LabelPrintPoLine>();
                        objOut.Add(obj);
                        return objOut;
                    }
                }
                else
                {
                    List<LabelPrintPoLine> objOut = new List<LabelPrintPoLine>();
                    return objOut;
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string PostWarehouseTransfer(string details)
        {
            try
            {
                string Barcode = this.ValidateBarcode(details);
                if (Barcode == "")
                {
                    List<WarehouseTransfer> myDeserializedObjList = (List<WarehouseTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WarehouseTransfer>));
                    if (myDeserializedObjList.Count > 0)
                    {
                        //Check if delayed posting has been turned on for either warehouse.
                        //If it has, then save to table. Service will post transactions after delayed posting turned off.
                        if(this.CheckDelayedPosting(myDeserializedObjList.FirstOrDefault().SourceWarehouse) == true)
                        {
                            //Save delayed posting
                            foreach(var item in myDeserializedObjList)
                            {
                                this.SaveDelayedPosting("Immediate", item.SourceWarehouse, item.SourceBin, item.DestinationWarehouse, item.DestinationBin, item.StockCode, item.LotNumber, item.Quantity);
                            }
                            return "Items queued for posting.";
                        }
                        else
                        {
                            if(this.CheckDelayedPosting(myDeserializedObjList.FirstOrDefault().DestinationWarehouse) == true)
                            {
                                //Save delayed posting
                                foreach (var item in myDeserializedObjList)
                                {
                                    this.SaveDelayedPosting("Immediate", item.SourceWarehouse, item.SourceBin, item.DestinationWarehouse, item.DestinationBin, item.StockCode, item.LotNumber, item.Quantity);
                                }
                                return "Items queued for posting.";
                            }                            
                        }


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
                        Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                        Document.Append("<!--");
                        Document.Append("Sample XML for the Inventory Warehouse Transfer Out Business Object");
                        Document.Append("-->");
                        Document.Append("<PostInvWhTransferOut xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMODOC.XSD\">");

                        foreach (var item in myDeserializedObjList)
                        {
                            Document.Append(this.BuildWarehouseTransferDocument(item.SourceWarehouse, item.SourceBin, item.StockCode, item.LotNumber, item.Quantity.ToString(), item.DestinationWarehouse, item.DestinationBin));                            
                        }
                        Document.Append("</PostInvWhTransferOut>");

                        Parameter = this.BuildWarehouseTransferParameter();
                        XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "INVTMO");
                        objSyspro.SysproLogoff(Guid);
                        ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                        string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                        if (string.IsNullOrEmpty(ErrorMessage))
                        {
                            foreach (var item in myDeserializedObjList)
                            {
                                db.sp_SaveInvTransfer("Immediate", item.SourceWarehouse, item.SourceBin, item.DestinationWarehouse, item.DestinationBin, item.StockCode, item.LotNumber, item.Quantity, Journal, "", HttpContext.Current.User.Identity.Name.ToUpper());
                            }
                            return "Posting Complete. Jnl : " + Journal;
                        }
                        else
                        {
                            return "Error : " + ErrorMessage;
                        }
                    }
                    return "No data found to post!";
                }
                return Barcode;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string BuildWarehouseTransferDocument(string SourceWarehouse, string SourceBin, string StockCode, string Lot, string Quantity, string DestinationWarehouse, string DestinationBin)
        {
            try
            {
                var MultiBins = (from a in db.vw_InvWhControl where a.Warehouse.Equals(SourceWarehouse) select a.UseMultipleBins).FirstOrDefault();
                var TraceableType = (from a in db.InvMasters where a.StockCode.Equals(StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod }).FirstOrDefault();
                
                //Declaration
                StringBuilder Document = new StringBuilder();

                Document.Append("<Item>");
                Document.Append("<Journal/>");
                Document.Append("<Immediate>Y</Immediate>");
                Document.Append("<NoDestination>N</NoDestination>");
                Document.Append("<FromWarehouse>" + SourceWarehouse + "</FromWarehouse>");
                if(MultiBins == "Y")
                {
                    Document.Append("<FromBin>" + SourceBin + "</FromBin>");
                }                
                Document.Append("<StockCode>" + StockCode + "</StockCode>");
                Document.Append("<Version/>");
                Document.Append("<Release/>");
                Document.Append("<Quantity>" + Quantity + "</Quantity>");
                Document.Append("<UnitOfMeasure/>");
                Document.Append("<Units/>");
                Document.Append("<Pieces/>");
                Document.Append("<ToWarehouse>" + DestinationWarehouse + "</ToWarehouse>");
                if (MultiBins == "Y")
                {
                    Document.Append("<ToBin>" + DestinationBin + "</ToBin>");
                }                
                Document.Append("<ToWhJournal></ToWhJournal>");
                if(TraceableType.TraceableType == "T")
                {
                    Document.Append("<Lot>" + Lot + "</Lot>");
                }

                Document.Append("<Reference>" + Lot + "</Reference>");
                Document.Append("<Notation>" + Lot + "</Notation>");
                Document.Append("<LedgerCode/>");
                //Document.Append("<PasswordForLedgerCode/>");
                //if(TraceableType.SerialMethod != "N")
                //{
                //    Document.Append("<Serials>");
                //    Document.Append("<SerialNumber>BCS11495</SerialNumber>");
                //    Document.Append("<SerialQuantity>1</SerialQuantity>");
                //    Document.Append("<SerialUnits/>");
                //    Document.Append("<SerialPieces/>");
                //    Document.Append("<SerialLocation/>");
                //    Document.Append("</Serials>");
                //    Document.Append("<SerialAllocation>");
                //    Document.Append("<FromSerialNumber>BCS11497</FromSerialNumber>");
                //    Document.Append("<ToSerialNumber>BCS11499</ToSerialNumber>");
                //    Document.Append("<SerialQuantity>3</SerialQuantity>");
                //    Document.Append("</SerialAllocation>");
                //}
                
                Document.Append("<eSignature/>");
                Document.Append("</Item>");
                
                return Document.ToString();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildWarehouseTransferParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Inventory Warehouse Transfer Out Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostInvWhTransferOut xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMO.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<CreateDestinationWarehouse>N</CreateDestinationWarehouse>");
            Parameter.Append("<TransferCostIfReceivingWhCostZero>N</TransferCostIfReceivingWhCostZero>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostInvWhTransferOut>");

            return Parameter.ToString();

        }


        public string PostTransferOut(string details)
        {
            try
            {
                string Barcode = this.ValidateBarcode(details);
                if (Barcode == "")
                {
                    List<WarehouseTransfer> myDeserializedObjList = (List<WarehouseTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WarehouseTransfer>));
                    if (myDeserializedObjList.Count > 0)
                    {
                        StringBuilder Document = new StringBuilder();
                        //Building Document content
                        Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                        Document.Append("<!--");
                        Document.Append("Sample XML for the Inventory Goods in Transit Warehouse Transfer Out Business Object");
                        Document.Append("-->");
                        Document.Append("<PostInvGitWhTransferOut xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMTDOC.XSD\">");
                        

                        string Guid = objSyspro.SysproLogin();
                        if (string.IsNullOrEmpty(Guid))
                        {
                            return "Failed to Log in to Syspro.";
                        }

                        foreach (var item in myDeserializedObjList)
                        {

                            Document.Append(this.BuildTransferOutDocument(item.SourceWarehouse, item.SourceBin, item.StockCode, item.LotNumber, item.Quantity.ToString(), item.DestinationWarehouse, item.DestinationBin));                            

                        }

                        
                        Document.Append("</PostInvGitWhTransferOut>");

                        string Parameter, XmlOut, ErrorMessage;

                        Parameter = this.BuildTransferOutParameter();
                        XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "INVTMT");
                        objSyspro.SysproLogoff(Guid);
                        ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                        string GITRef = objSyspro.GetFirstXmlValue(XmlOut, "GtrReference");
                        string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                        if (string.IsNullOrEmpty(ErrorMessage))
                        {
                            foreach (var item in myDeserializedObjList)
                            {
                                db.sp_SaveInvTransfer("TransferOut", item.SourceWarehouse, item.SourceBin, item.DestinationWarehouse, item.DestinationBin, item.StockCode, item.LotNumber, item.Quantity, "", GITRef, HttpContext.Current.User.Identity.Name.ToUpper());
                            }
                            return "Posting Complete. GIT : " + GITRef + ";Jnl :" + Journal;

                        }
                        else
                        {
                            return "Error : " + ErrorMessage;
                        }
                    }
                    return "No data found to post!";
                }
                return Barcode;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildTransferOutDocument(string SourceWarehouse, string SourceBin, string StockCode, string Lot, string Quantity, string DestinationWarehouse, string DestinationBin)
        {
            try
            {
                var MultiBins = (from a in db.vw_InvWhControl where a.Warehouse.Equals(SourceWarehouse) select a.UseMultipleBins).FirstOrDefault();
                var TraceableType = (from a in db.InvMasters where a.StockCode.Equals(StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod }).FirstOrDefault();
                //Declaration
                StringBuilder Document = new StringBuilder();

                Document.Append("<Item>");
                Document.Append("<Journal />");
                Document.Append("<GtrReference />");
                Document.Append("<FromWarehouse>" + SourceWarehouse + "</FromWarehouse>");
                if(MultiBins == "Y")
                {
                    Document.Append("<BinLocation>" + SourceBin + "</BinLocation>");
                }

                Document.Append("<StockCode>" + StockCode + "</StockCode>");
                Document.Append("<Version />");
                Document.Append("<Release />");
                Document.Append("<Quantity>" + Quantity + "</Quantity>");
                Document.Append("<UnitOfMeasure />");
                Document.Append("<Units />");
                Document.Append("<Pieces />");
                Document.Append("<ToWarehouse>" + DestinationWarehouse + "</ToWarehouse>");
                if (TraceableType.TraceableType == "T")
                {
                    Document.Append("<Lot>" + Lot + "</Lot>");
                }
                
                Document.Append("<FifoBucket>");
                Document.Append("</FifoBucket>");
                Document.Append("<Notation>" + Lot + "</Notation>");
                //Document.Append("<Serials>");
                //Document.Append("<SerialNumber>0205</SerialNumber>");
                //Document.Append("<SerialQuantity>1</SerialQuantity>");
                //Document.Append("<SerialUnits />");
                //Document.Append("<SerialPieces />");
                //Document.Append("<SerialFifoBucket>");
                //Document.Append("</SerialFifoBucket>");
                //Document.Append("</Serials>");
                //Document.Append("<Serials>");
                //Document.Append("<SerialNumber>0209</SerialNumber>");
                //Document.Append("<SerialQuantity>1</SerialQuantity>");
                //Document.Append("<SerialUnits />");
                //Document.Append("<SerialPieces />");
                //Document.Append("</Serials>");
                //Document.Append("<SerialAllocation>");
                //Document.Append("<FromSerialNumber>0215</FromSerialNumber>");
                //Document.Append("<ToSerialNumber>0222</ToSerialNumber>");
                //Document.Append("<SerialQuantity>8</SerialQuantity>");
                //Document.Append("</SerialAllocation>");
                Document.Append("<eSignature />");
                Document.Append("</Item>");

                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildTransferOutParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Inventory Goods in Transit Warehouse Transfer Out Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostInvGitWhTransferOut xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMT.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<TransactionDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</TransactionDate>");
            Parameter.Append("<CreateDestinationWarehouse>N</CreateDestinationWarehouse>");
            Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostInvGitWhTransferOut>");

            return Parameter.ToString();

        }


        public List<WarehouseTransfer> QueryLot(string details)
        {
            try
            {
                WarehouseTransfer obj = new WarehouseTransfer();
                List<WarehouseTransfer> myDeserializedObjList = (List<WarehouseTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WarehouseTransfer>));
                if (myDeserializedObjList.Count > 0)
                {
                    string StockCode = myDeserializedObjList.FirstOrDefault().StockCode;
                    string Lot = myDeserializedObjList.FirstOrDefault().LotNumber;
                    
                    var result = (from a in db.LotDetails where a.StockCode == StockCode && a.Lot == Lot select new WarehouseTransfer { StockCode = a.StockCode, LotNumber = a.Lot, DestinationWarehouse = a.Warehouse, DestinationBin = a.Bin, Quantity = a.QtyOnHand }).ToList();
                    if(result.Count > 0)
                    {
                        return result;
                    }
                    else
                    {
                        List<WarehouseTransfer> objOut = new List<WarehouseTransfer>();
                        objOut.Add(obj);
                        return objOut;
                    }
                }
                else
                {
                    List<WarehouseTransfer> objOut = new List<WarehouseTransfer>();
                    objOut.Add(obj);
                    return objOut;
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool CheckDelayedPosting(string Warehouse)
        {
            try
            {
                HttpCookie database = HttpContext.Current.Request.Cookies.Get("SysproDatabase");
                var company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var result = (from a in mdb.mtDelayedPostingWarehouses where a.Company == company && a.Warehouse == Warehouse select a).ToList();
                if(result.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void SaveDelayedPosting(string MovementType, string FromWarehouse, string FromBin, string ToWarehouse, string ToBin, string StockCode, string Lot, decimal Quantity)
        {
            try
            {
                using (var cdb = new WarehouseManagementEntities(""))
                {
                    var result = (from a in cdb.mtInvDelayedPostings where a.MovementType == MovementType && a.FromWarehouse == FromWarehouse && a.FromBin == FromBin && a.ToWarehouse == ToWarehouse && a.ToBin == ToBin && a.StockCode == StockCode && a.Lot == Lot && a.Quantity == Quantity && a.Status == 1 select a).ToList();
                    if (result.Count == 0)
                    {
                        mtInvDelayedPosting obj = new mtInvDelayedPosting();
                        obj.MovementType = MovementType;
                        obj.FromWarehouse = FromWarehouse;
                        obj.FromBin = FromBin;
                        obj.ToWarehouse = ToWarehouse;
                        obj.ToBin = ToBin;
                        obj.StockCode = StockCode;
                        obj.Lot = Lot;
                        obj.Quantity = Quantity;
                        obj.Username = HttpContext.Current.User.Identity.Name.ToUpper();
                        obj.TrnDate = DateTime.Now;
                        obj.Status = 1;
                        cdb.Entry(obj).State = System.Data.EntityState.Added;
                        cdb.SaveChanges();
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
    
}