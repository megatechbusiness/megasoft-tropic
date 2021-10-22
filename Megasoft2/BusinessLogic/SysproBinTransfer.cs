using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace Megasoft2.BusinessLogic
{
    public class SysproBinTransfer
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore objSyspro = new SysproCore();
        public string ValidateBarcode(string details)
        {
            try
            {
                List<BinTransfer> myDeserializedObjList = (List<BinTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BinTransfer>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var StockCodeCheck = db.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                        if (StockCodeCheck == null)
                        {
                            return "StockCode not found!.";
                        }

                        var StockWarehouseCheck = db.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse)).FirstOrDefault();
                        if (StockWarehouseCheck == null)
                        {
                            return "StockCode not stocked in Warehouse " + item.Warehouse + "!.";
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
                                var LotCheck = db.LotDetails.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse) && a.Lot.Equals(item.LotNumber)).FirstOrDefault();
                                if (LotCheck == null)
                                {
                                    return "Lot " + item.LotNumber + " not found for StockCode " + item.StockCode + " in Warehouse " + item.Warehouse + "!";
                                }
                                else
                                {
                                    var LotQtyCheck = db.LotDetails.Where(a => a.StockCode.Equals(item.StockCode)
                                                                            && a.Warehouse.Equals(item.Warehouse)
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
                                                                          && a.Warehouse.Equals(item.Warehouse)
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
                    var result = db.sp_GetStockCodeBySupplierCode(SupplierStockCode).FirstOrDefault();

                    if (result == null)
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //public List<BinTransfer> GetBins(string details)
        //{
        //    try
        //    {
        //        List<BinTransfer> myDeserializedObjList = (List<BinTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BinTransfer>));
        //        if (myDeserializedObjList.Count > 0)
        //        {
        //            string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse;
        //        var result = db.sp_GetBins(Warehouse);
        //        if (result == null)
        //        {

        //            return null;
        //        }
        //        else
        //        {
        //            return db.sp_GetBins(Warehouse).ToList();
        //        }
        //        }
                
         


        //        //List<BinTransfer> myDeserializedObjList = (List<BinTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BinTransfer>));
        //        //if (myDeserializedObjList.Count > 0)
        //        //{
        //        //    string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse;
        //        //    var result =  db.sp_GetBins(Warehouse);

        //        //    if (result == null)
        //        //    {

        //        //        return null;
        //        //    }
        //        //    else
        //        //    {
        //        //        obj.Bin= result.Bins;

        //        //        List<Bin> objOut = new List<Bin>();
        //        //        objOut.Add(obj);
        //        //        return objOut;
        //        //    }
               
         
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public string PostBinTransfer(string details)
        {
            try
            {
                string Barcode = this.ValidateBarcode(details);
                if (Barcode == "")
                {
                    List<BinTransfer> myDeserializedObjList = (List<BinTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BinTransfer>));
                    if (myDeserializedObjList.Count > 0)
                    {


                        //Check if delayed posting has been turned on for warehouse.
                        //If it has, then save to table. Service will post transactions after delayed posting turned off.
                        if (this.CheckDelayedPosting(myDeserializedObjList.FirstOrDefault().Warehouse) == true)
                        {
                            //Save delayed posting
                            foreach (var item in myDeserializedObjList)
                            {
                                this.SaveDelayedPosting("Bin", item.Warehouse, item.SourceBin, item.Warehouse, item.DestinationBin, item.StockCode, item.LotNumber, item.Quantity);
                            }
                            return "Items queued for posting.";
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
                        Document.Append("Sample XML for the Inventory Bin Transfers Business Object");
                        Document.Append("-->");
                        Document.Append("<PostInvBinTransfers xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMBDOC.XSD\">");

                        foreach (var item in myDeserializedObjList)
                        {
                            Document.Append(this.BuildBinTransferDocument(item.Warehouse, item.SourceBin, item.StockCode, item.LotNumber, item.Quantity.ToString(), item.DestinationBin));
                        }
                        Document.Append("</PostInvBinTransfers>");

                        Parameter = this.BuildBinTransferParameter();
                        XmlOut = objSyspro.SysproPost(Guid, Parameter, Document.ToString(), "INVTMB");
                        objSyspro.SysproLogoff(Guid);
                        ErrorMessage = objSyspro.GetXmlErrors(XmlOut);
                        string Journal = objSyspro.GetFirstXmlValue(XmlOut, "Journal");
                        if (string.IsNullOrEmpty(ErrorMessage))
                        {
                            foreach (var item in myDeserializedObjList)
                            {
                                db.sp_SaveInvTransfer("Bin", item.Warehouse, item.SourceBin, item.Warehouse, item.DestinationBin, item.StockCode, item.LotNumber, item.Quantity, Journal, "", HttpContext.Current.User.Identity.Name.ToUpper());
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


        public string BuildBinTransferDocument(string Warehouse, string SourceBin, string StockCode, string Lot, string Quantity, string DestinationBin)
        {
            try
            {
               // var MultiBins = (from a in db.InvWhControls where a.Warehouse.Equals(SourceWarehouse) select a.UseMultipleBins).FirstOrDefault();
                var TraceableType = (from a in db.InvMasters where a.StockCode.Equals(StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod }).FirstOrDefault();

                //Declaration
                StringBuilder Document = new StringBuilder();

                Document.Append("<Item>");
                Document.Append("<Journal/>");
                Document.Append("<Warehouse>"+Warehouse+"</Warehouse>");
                Document.Append("<StockCode>" + StockCode + "</StockCode>");
                Document.Append("<Version/>");
                Document.Append("<Release/>");
                Document.Append("<Quantity>" + Quantity + "</Quantity>");
                Document.Append("<UnitOfMeasure/>");
                Document.Append("<Units/>");
                Document.Append("<Pieces/>");
                Document.Append("<FromBin>" + SourceBin + "</FromBin>");
                Document.Append("<ToBin>" + DestinationBin + "</ToBin>");
                if (TraceableType.TraceableType == "T")
                {
                    Document.Append("<Lot>"+Lot+"</Lot>");
                }
                
                Document.Append("<UpdateZeroLotDate/>");
                Document.Append("<Reference>" + Lot + "</Reference>");
                Document.Append("<Notation>"+Lot+"</Notation>");
                Document.Append("<eSignature/>");
                Document.Append("</Item>");


                return Document.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildBinTransferParameter()
        {
                        //Declaration
                        StringBuilder Parameter = new StringBuilder();

                        //Building Parameter content
                        Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                        Parameter.Append("<!--");
                        Parameter.Append("Sample XML for the Inventory Bin Transfers Business Object");
                        Parameter.Append("-->");
                        Parameter.Append("<PostInvBinTransfers xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"INVTMB.XSD\">");
                        Parameter.Append("<Parameters>");
                        Parameter.Append("<TransactionDate>"+DateTime.Now.ToString("yyyy-MM-dd")+"</TransactionDate>");
                        Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                        Parameter.Append("<PostingPeriod>C</PostingPeriod>");
                        Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                        Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                        Parameter.Append("<WarnIfLotDatesDiffer>N</WarnIfLotDatesDiffer>");
                        Parameter.Append("</Parameters>");
                        Parameter.Append("</PostInvBinTransfers>");
            
            return Parameter.ToString();

        }


        public string CheckStockCodeBin(string details)
        {
            try
            {

                List<BinTransfer> myDeserializedObjList = (List<BinTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BinTransfer>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse;
                    string SourceBin = myDeserializedObjList.FirstOrDefault().SourceBin;
                    string DestinationBin = myDeserializedObjList.FirstOrDefault().DestinationBin;
                    string StockCode = myDeserializedObjList.FirstOrDefault().StockCode;
                    string Error = string.Empty;

                    var SourceResult = db.sp_CheckStockCodeBins(Warehouse, StockCode, SourceBin).ToList();
                    var DestinationResult = db.sp_CheckStockCodeBins(Warehouse, StockCode, DestinationBin).ToList();

                    if (SourceResult.Count() == 0)
                    {
                        Error = "Stock code: " + StockCode + " not in bin " + SourceBin + ";";


                    }
                    if (DestinationResult.Count() == 0)
                    {
                        Error += "Stock code: " + StockCode + " not in bin " + DestinationBin + ";";
                    }
                    return Error;

                }
                return "Error";
            }
            catch (Exception ex)
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
                if (result.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void SaveDelayedPosting(string MovementType, string FromWarehouse, string FromBin, string ToWarehouse, string ToBin, string StockCode, string Lot, decimal Quantity)
        {
            try
            {

                using(var cdb = new WarehouseManagementEntities(""))
                {
                    var result = (from a in cdb.mtInvDelayedPostings where a.MovementType == MovementType && a.FromWarehouse == FromWarehouse && a.FromBin == FromBin && a.ToWarehouse == ToWarehouse && a.ToBin == ToBin && a.StockCode == StockCode && a.Lot == Lot && a.Quantity == Quantity && a.Status == 1 select a).ToList();
                    if(result.Count == 0)
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