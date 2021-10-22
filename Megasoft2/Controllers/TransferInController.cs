using Megasoft2.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransferInController : Controller
    {
        //
        // GET: /TransferIn/

        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproTransferIn objSyspro = new SysproTransferIn();
        MegasoftEntities wdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "TransferIn")]
        public ActionResult Index()
        {
            
            var settings = (from a in db.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
            if(settings.TransferInScanItem == true)
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                return View("TransferInByScan");
            }
            else
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                return View();
            }
            
        }

        [CustomAuthorize(Activity: "TransferIn")]
        public ActionResult TransferInByScan()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Activity: "TransferIn")]
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
        [CustomAuthorize(Activity: "TransferIn")]
        public ActionResult PostWarehouseTransfer(string details)
        {
            try
            {
                List<TransferInData> myDeserializedObjList = (List<TransferInData>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<TransferInData>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse;
                    string GtrReference = myDeserializedObjList.FirstOrDefault().GtrReference;
                    return Json(objSyspro.PostTransferIn(Warehouse, GtrReference), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Data Found.", JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                return Json("Exception :" + ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        [CustomAuthorize(Activity: "TransferIn")]
        public JsonResult GetReferences(string Warehouse)
        {
            try
            {                
                var result = db.sp_GetGtrReferenceByWarehouse(Warehouse).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomAuthorize(Activity: "TransferIn")]
        public JsonResult GetReferenceDetail(string GtrReference)
        {
            try
            {
                var result = db.sp_GetGtrDetailByReference(GtrReference).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public class TransferInData
        {
            public string Warehouse { get; set; }
            public string GtrReference { get; set; }
        }
    }
}
