using Megasoft2.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransferOutController : Controller
    {
        //
        // GET: /TransferOut/

        //SysproEntities db = new SysproEntities("");
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproWarehouseTransfer objSyspro = new SysproWarehouseTransfer();
        MegasoftEntities wdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "TransferOut")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in wdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            return View();
        }


        [HttpPost]
        [CustomAuthorize(Activity: "TransferOut")]
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
        [CustomAuthorize(Activity: "TransferOut")]
        public ActionResult PostTransferOut(string details)
        {
            try
            {
                return Json(objSyspro.PostTransferOut(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [CustomAuthorize(Activity: "TransferOut")]
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

    }
}
