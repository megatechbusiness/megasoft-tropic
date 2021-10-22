using Megasoft2.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WmsPickingController : Controller
    {

        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        SysproCore sys = new SysproCore();
        DespatchBL BL = new DespatchBL();
        //
        // GET: /WmsPicking/

        [CustomAuthorize("Picking")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetNextOrder(string WmsId, string SalesOrderLine)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(SalesOrderLine))
                {
                    SalesOrderLine = "0";
                }
                int CurrentWmsId = Convert.ToInt32(WmsId);
                int CurrentSoLine = Convert.ToInt32(SalesOrderLine);
                sp_GetWmsNextOrderForPicker_Result obj = new sp_GetWmsNextOrderForPicker_Result();
                var result = wdb.sp_GetWmsNextOrderForPicker(HttpContext.User.Identity.Name.ToUpper()).ToList();
                if(result.Count > 0)
                {
                    int LastOrder = (from a in result select a.WmsId).LastOrDefault();
                    if (CurrentWmsId == LastOrder)
                    {
                        obj = (from a in result select a).FirstOrDefault();

                    }
                    else
                    {
                        obj = (from a in result where a.WmsId > CurrentWmsId select a).FirstOrDefault();

                    }

                    //obj = (from a in result where a.WmsId != CurrentWmsId || a.SalesOrderLine != CurrentSoLine select a).FirstOrDefault();
                    //if(obj == null)
                    //{
                    //    obj = result.FirstOrDefault();
                    //}

                    List<sp_GetWmsNextOrderForPicker_Result> objOut = new List<sp_GetWmsNextOrderForPicker_Result>();
                    objOut.Add(obj);
                    return Json(objOut, JsonRequestBehavior.AllowGet);
                }
                return Json("Error: No orders found for picking.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPickingItems(string WmsId, int SalesOrderLine)
        {
            try
            {
                var username = HttpContext.User.Identity.Name.ToUpper();
                var result = (wdb.sp_GetWmsItemsForPicking(Convert.ToInt32(WmsId), username, SalesOrderLine)).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public JsonResult GetReasons()
        {
            try
            {
                var result = (from a in wdb.mtWmsReasons select new { Value = a.ReasonCode, Text = a.Description }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [HttpPost]
        public ActionResult ValidateScan(string Barcode, string WmsId, string SalesOrderLine)
        {
            try
            {
                var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                if(setting.SalesReleaseAutoAllocation == true)
                {
                    if (string.IsNullOrWhiteSpace(Barcode))
                    {
                        return Json("Error: Barcode cannot be blank.", JsonRequestBehavior.AllowGet);
                    }

                    int WMSID = Convert.ToInt32(WmsId);
                    int SOLine = Convert.ToInt32(SalesOrderLine);

                    var Traceable = (from a in wdb.mtWmsOrderMasters where a.WmsId == WMSID && a.SalesOrderLine == SOLine select a.TraceableType).FirstOrDefault();
                    if (Traceable != null)
                    {
                        if (Traceable == "T")
                        {
                            var PalletCheck = (from a in wdb.mtWmsOrderDetails where a.PalletNo == Barcode && a.WmsId == WMSID && a.SalesOrderLine == SOLine select a).ToList();
                            if (PalletCheck.Count > 0)
                            {
                                //We have identified the barcode scanned is a pallet;
                                //We need to get all the Lots for this pallet and save the quantity on hand against the Picked Qty
                                wdb.sp_WmsUpdateLotsPickedByPallet(WMSID, Barcode, SOLine);
                                return Json("", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {

                                var BatchCheck = (from a in wdb.mtWmsOrderDetails where a.Lot == Barcode && a.WmsId == WMSID && a.SalesOrderLine == SOLine select a).ToList();
                                if (BatchCheck.Count > 0)
                                {
                                    //We have identified the barcode scanned is a Lot/Batch
                                    //Update the picked qty based on Qty on hand.
                                    wdb.sp_WmsUpdateLotsPickedByLot(WMSID, Barcode, SOLine);
                                    return Json("", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json("Barcode scanned not released for picking!", JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        else
                        {
                            //Assume the barcode scanned is a stockcode barcode.
                            //Validate Stock Code
                            var StockCheck = (from a in wdb.mtWmsOrderDetails where a.StockCode == Barcode && a.WmsId == WMSID && a.SalesOrderLine == SOLine select a).ToList();
                            if (StockCheck.Count > 0)
                            {
                                //We have identified the barcode scanned is a StockCode
                                //return "StockScan". We now need the user to enter the quantity.
                                return Json("StockScan", JsonRequestBehavior.AllowGet);

                            }
                            else
                            {
                                return Json("Barcode scanned not found!", JsonRequestBehavior.AllowGet);
                            }

                        }
                    }


                    return Json("Error: No orders found for picking.", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Barcode))
                    {
                        return Json("Error: Barcode cannot be blank.", JsonRequestBehavior.AllowGet);
                    }

                    int WMSID = Convert.ToInt32(WmsId);
                    int SOLine = Convert.ToInt32(SalesOrderLine);

                    var Traceable = (from a in wdb.mtWmsOrderMasters where a.WmsId == WMSID select a.TraceableType).FirstOrDefault();
                    if (Traceable != null)
                    {
                        if (Traceable == "T")
                        {
                            var PalletCheck = (from a in wdb.mtWmsOrderDetails where a.PalletNo == Barcode select a).ToList();
                            if (PalletCheck.Count > 0)
                            {
                                return Json("Pallet number already added to a picking list.", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var BatchCheck = (from a in wdb.mtWmsOrderDetails where a.Lot == Barcode select a).ToList();
                                if (BatchCheck.Count > 0)
                                {
                                    return Json("Batch number already added to a picking list.", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    //Check Lot Custom Form for Pallet No.
                                    var sysCheck = wdb.sp_GetWmsCheckItemBatchOrPallet(Barcode, "Pallet").ToList();
                                    if (sysCheck.Count > 0)
                                    {
                                        //Pallet Found.
                                        wdb.sp_WmsSavePalletOrBatchScanned(Barcode, "Pallet", WMSID, HttpContext.User.Identity.Name.ToUpper(), SOLine);
                                        return Json("", JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        //Check Lot exists in Lot Detail.
                                        var sysCheckLot = wdb.sp_GetWmsCheckItemBatchOrPallet(Barcode, "Lot").ToList();
                                        if (sysCheckLot.Count > 0)
                                        {
                                            //Lot Found.
                                            wdb.sp_WmsSavePalletOrBatchScanned(Barcode, "Lot", WMSID, HttpContext.User.Identity.Name.ToUpper(), SOLine);
                                            return Json("", JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            return Json("Barcode not found.", JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            return Json("Cannot add item for non-traceable item!", JsonRequestBehavior.AllowGet);
                        }
                    }


                    return Json("Error: No orders found for picking.", JsonRequestBehavior.AllowGet);
                }
                
  
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DoBackOrderRelease(string WmsId, int SalesOrderLine)
        {
            try
            {
                string ErrorMessage = "";
                var AutoAllo = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.SalesReleaseAutoAllocation).FirstOrDefault();
                if(AutoAllo ==true)
                {
                    string Guid = sys.SysproLogin();
                    string XmlOut = sys.SysproPost(Guid, BL.BuildReleaseParameter(), BL.BuildReleaseDocument(Convert.ToInt32(WmsId), "Picker", SalesOrderLine), "SORTBO");
                    sys.SysproLogoff(Guid);
                    ErrorMessage = sys.GetXmlErrors(XmlOut);
                }
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    string username = HttpContext.User.Identity.Name.ToUpper();
                    int WMSID = Convert.ToInt32(WmsId);
                    using (var udb = new WarehouseManagementEntities(""))
                    {
                        var result = (from a in udb.mtWmsOrderDetails where a.WmsId == WMSID && a.SalesOrderLine == SalesOrderLine && a.QuantityPicked != 0 && a.Picker == username select a).ToList();
                        foreach (var item in result)
                        {
                            item.CheckoutDate = DateTime.Now;
                            item.CheckoutDone = "Y";
                            udb.Entry(item).State = System.Data.EntityState.Modified;
                            udb.SaveChanges();
                        }
                    }
                        
                    return Json("Checkout posted Successfully.", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(ErrorMessage, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult AddItem(string Barcode, string WmsId, string SalesOrderLine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Barcode))
                {
                    return Json("Error: Barcode cannot be blank.", JsonRequestBehavior.AllowGet);
                }

                int WMSID = Convert.ToInt32(WmsId);
                int SOLine = Convert.ToInt32(SalesOrderLine);

                var Traceable = (from a in wdb.mtWmsOrderMasters where a.WmsId == WMSID select a.TraceableType).FirstOrDefault();
                if (Traceable != null)
                {
                    if (Traceable == "T")
                    {
                        var PalletCheck = (from a in wdb.mtWmsOrderDetails where a.PalletNo == Barcode select a).ToList();
                        if (PalletCheck.Count > 0)
                        {                            
                            return Json("Pallet number already added to a picking list.", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            var BatchCheck = (from a in wdb.mtWmsOrderDetails where a.Lot == Barcode select a).ToList();
                            if (BatchCheck.Count > 0)
                            {
                                return Json("Batch number already added to a picking list.", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                //Check Lot Custom Form for Pallet No.
                                var sysCheck = wdb.sp_GetWmsCheckItemBatchOrPallet(Barcode, "Pallet").ToList();
                                if(sysCheck.Count > 0)
                                {
                                    //Pallet Found.
                                    wdb.sp_WmsSavePalletOrBatchScanned(Barcode, "Pallet", WMSID, HttpContext.User.Identity.Name.ToUpper(), SOLine);
                                    return Json("", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    //Check Lot exists in Lot Detail.
                                    var sysCheckLot = wdb.sp_GetWmsCheckItemBatchOrPallet(Barcode, "Lot").ToList();
                                    if (sysCheckLot.Count > 0)
                                    {
                                        //Lot Found.
                                        wdb.sp_WmsSavePalletOrBatchScanned(Barcode, "Lot", WMSID, HttpContext.User.Identity.Name.ToUpper(), SOLine);
                                        return Json("", JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json("Barcode not found.", JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return Json("Cannot add item for non-traceable item!", JsonRequestBehavior.AllowGet);
                    }
                }


                return Json("Error: No orders found for picking.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpPost]
        public ActionResult DeleteItem(string Barcode, string WmsId, string SalesOrderLine, string Reason)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Barcode))
                {
                    return Json("Error: Barcode cannot be blank.", JsonRequestBehavior.AllowGet);
                }

                int WMSID = Convert.ToInt32(WmsId);
                int SOLine = Convert.ToInt32(SalesOrderLine);

                var Traceable = (from a in wdb.mtWmsOrderMasters where a.WmsId == WMSID && a.SalesOrderLine == SOLine select a.TraceableType).FirstOrDefault();
                if (Traceable != null)
                {
                    if (Traceable == "T")
                    {
                        using (var pdb = new WarehouseManagementEntities(""))
                        {
                            var PalletCheck = (from a in pdb.mtWmsOrderDetails where a.PalletNo == Barcode && a.WmsId == WMSID && a.SalesOrderLine == SOLine select a).ToList();
                            if (PalletCheck.Count > 0)
                            {



                                foreach (var item in PalletCheck)
                                {
                                    pdb.Entry(item).State = System.Data.EntityState.Deleted;
                                    pdb.SaveChanges();
                                    LogDeletedScan(item.WmsId, item.WmsLine, item.SalesOrderLine, item.StockCode, item.Lot, item.PalletNo, "P", Convert.ToInt32(Reason));
                                }


                                return Json("", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var BatchCheck = (from a in pdb.mtWmsOrderDetails where a.Lot == Barcode && a.WmsId == WMSID && a.SalesOrderLine == SOLine select a).ToList();
                                if (BatchCheck.Count > 0)
                                {
                                    foreach (var bitem in BatchCheck)
                                    {
                                        pdb.Entry(bitem).State = System.Data.EntityState.Deleted;
                                        pdb.SaveChanges();
                                        LogDeletedScan(bitem.WmsId, bitem.WmsLine, bitem.SalesOrderLine, bitem.StockCode, bitem.Lot, bitem.PalletNo, "B", Convert.ToInt32(Reason));
                                    }
                                    return Json("", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json("Barcode scanned not released for picking!", JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Assume the barcode scanned is a stockcode barcode.
                        //Validate Stock Code
                        var StockCheck = (from a in wdb.mtWmsOrderDetails where a.StockCode == Barcode && a.WmsId == WMSID && a.SalesOrderLine == SOLine select a).ToList();
                        if (StockCheck.Count > 0)
                        {
                            //We have identified the barcode scanned is a StockCode
                            //return "StockScan". We now need the user to enter the quantity.
                            return Json("StockScan", JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            return Json("Barcode scanned not found!", JsonRequestBehavior.AllowGet);
                        }

                    }
                }


                return Json("Error: No orders found for picking.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public void LogDeletedScan(int WmsId, int WmsLine, int SalesOrderLine, string StockCode, string Lot, string Pallet, string ScanType, int ReasonCode)
        {
            try
            {
                mtWmsDeletedItem obj = new mtWmsDeletedItem();
                obj.WmsId = WmsId;
                obj.WmsLine = WmsLine;
                obj.SalesOrderLine = SalesOrderLine;
                obj.StockCode = StockCode;
                obj.Lot = Lot;
                obj.PalletId = Pallet;
                obj.ScanType = ScanType;
                obj.ReasonCode = ReasonCode;
                obj.DeletedBy = HttpContext.User.Identity.Name.ToUpper();
                obj.DateDeleted = DateTime.Now;
                wdb.Entry(obj).State = System.Data.EntityState.Added;
                wdb.SaveChanges();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




    }
}
