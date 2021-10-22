using Megasoft2.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class BinTransferController : Controller
    {
        //
        // GET: /BinTransfer/
        //SysproEntities db = new SysproEntities("");
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproBinTransfer objSyspro = new SysproBinTransfer();
        MegasoftEntities wdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "BinTransfer")]
        public ActionResult Index(string SmartId = null)
        {
            if (SmartId == null)
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.SmartId = "";
                ViewBag.Bin = null;
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                return View();
            }
            else
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.SmartId = SmartId;
                ViewBag.Bin = null;
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                return View();
            }
        }

        public ActionResult DesktopIndex()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.SmartId = "";
            ViewBag.Bin = new List<SelectListItem>();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            return View("DesktopIndex");
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
                var result = (from a in db.LotDetails.AsNoTracking() where a.Warehouse == Warehouse && a.Bin == Bin && a.StockCode == StockCode && a.QtyOnHand != 0 select new { a.Warehouse, a.Bin, a.StockCode, a.Lot, a.QtyOnHand }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = (from a in db.LotDetails.AsNoTracking() where a.Warehouse == Warehouse && a.StockCode == StockCode && a.QtyOnHand != 0 select new { a.Warehouse, a.Bin, a.StockCode, a.Lot, a.QtyOnHand }).Distinct().ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        [CustomAuthorize(Activity: "BinTransfer")]
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
        [CustomAuthorize(Activity: "BinTransfer")]
        public ActionResult GetBins(string Warehouse)
        {
            try
            {
                var Bins = db.sp_GetBins(Warehouse.ToString().Trim()).ToList().Select(c => new
                {
                    ID = c.Bin,
                    Text = c.Bin
                });
                return Json(Bins, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [CustomAuthorize(Activity: "BinTransfer")]
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
        [CustomAuthorize(Activity: "BinTransfer")]
        public ActionResult PostBinTransfer(string details)
        {
            try
            {
                return Json(objSyspro.PostBinTransfer(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        [CustomAuthorize(Activity: "BinTransfer")]
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


        [CustomAuthorize(Activity: "BinTransfer")]
        public ActionResult CheckStockCodeBin(string details)
        {
            try
            {
                return Json(objSyspro.CheckStockCodeBin(details), JsonRequestBehavior.AllowGet);
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

    }
}
