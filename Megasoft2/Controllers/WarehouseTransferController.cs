using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WarehouseTransferController : Controller
    {

        //SysproEntities db = new SysproEntities("");
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproWarehouseTransfer objSyspro = new SysproWarehouseTransfer();
        MegasoftEntities wdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "Immediate")]
        public ActionResult Index(string SmartId = null)
        {
            if (SmartId == null)
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.SmartId = "";
                ViewBag.BinList = new List<SelectListItem>();
                return View();
            }
            else
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.SmartId = SmartId;
                ViewBag.BinList = new List<SelectListItem>();
                return View();
            }

        }



        [HttpPost]
        public ActionResult ValidateDetails(string details)
        {
            try
            {
                return Json(objSyspro.ValidateBarcode(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult GetStockAndSupplier(string details)
        {
            try
            {
                return Json(objSyspro.GetStockCodeCrossRef(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public ActionResult PostWarehouseTransfer(string details)
        {
            try
            {
                return Json(objSyspro.PostWarehouseTransfer(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public ActionResult CheckWarehouseMultiBin(string details)
        {
            try
            {
                List<MultiBin> myDeserializedObjList = (List<MultiBin>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MultiBin>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse.Trim();
                    var result = (from a in db.vw_InvWhControl where a.Warehouse == Warehouse select a).ToList();
                    if (result.Count > 0)
                    {
                        return Json(result.FirstOrDefault().UseMultipleBins, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error. Warehouse : " + Warehouse + " not found in Syspro.", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("Error - No Data. Warehouse not found.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public class MultiBin
        {
            public string Warehouse { get; set; }
            public string Source { get; set; }
        }


        public ActionResult LotQuery()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QueryLotData(string details)
        {
            try
            {
                return Json(objSyspro.QueryLot(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetBinList(string Warehouse)
        {
            try
            {
                return Json(db.sp_GetBinsByWarehouse(Warehouse), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSmartIdData(string SmartId)
        {
            try
            {
                return Json(wdb.sp_GetSmartScanGuidDetail(SmartId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckAddLine(string StockCode)
        {
            try
            {
                var check = (from a in db.InvMasters.AsNoTracking() where a.StockCode == StockCode select a).FirstOrDefault();
                if (check.ProductClass == "PAP" || check.ProductClass == "BOAR")
                {
                    return Json("Y", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("N", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomAuthorize(Activity: "Immediate-Desktop")]
        public ActionResult DesktopIndex()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.SmartId = "";
            ViewBag.BinList = new List<SelectListItem>();
            return View("DesktopIndex");
        }


        public ActionResult StockCodeSearch()
        {
            return PartialView();
        }

        public JsonResult StockCodeList(string Warehouse)
        {
            var result = (from a in db.InvWarehouses.AsNoTracking() join b in db.InvMasters on a.StockCode equals b.StockCode where  (a.Warehouse == Warehouse && b.StockOnHold == "") select new { MStockCode = a.StockCode, MStockDes = b.Description, MStockingUom = b.StockUom, ProductClass = b.ProductClass, WarehouseToUse = b.WarehouseToUse }).Distinct().ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LotSearch()
        {
            return PartialView();
        }

        public JsonResult LotList(string Warehouse, string Bin, string StockCode)
        {
            var whcontrol = (from a in db.vw_InvWhControl where a.Warehouse == Warehouse select a).ToList();
            if (whcontrol.FirstOrDefault().UseMultipleBins == "Y")
            {
                //var result = (from a in db.LotDetails.AsNoTracking() where a.Warehouse == Warehouse && a.Bin == Bin && a.StockCode == StockCode && a.QtyOnHand != 0 select new { a.Warehouse, a.Bin, a.StockCode, a.Lot, a.QtyOnHand }).Distinct().ToList();
                var result = (from a in db.mt_TransfersGetLotList(Warehouse, StockCode, Bin, "Y") select a).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // var result = (from a in db.LotDetails.AsNoTracking() where a.Warehouse == Warehouse && a.StockCode == StockCode && a.QtyOnHand != 0 select new { a.Warehouse, a.Bin, a.StockCode, a.Lot, a.QtyOnHand }).Distinct().ToList();
                var result = (from a in db.mt_TransfersGetLotList(Warehouse, StockCode, Bin, "N") select a).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }



        public JsonResult GetPalletItems(string PalletNo, string SourceWarehouse, string DestWarehouse)
        {
            try
            {
                var result = (db.mt_GetPalletItemsByPalletNumberAndWarehouse(PalletNo, SourceWarehouse)).ToList();
                if (result.Count > 0)
                {
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No items found for pallet : " + PalletNo, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


    }
}
