using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManMaterialIssueController : Controller
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        BusinessLogic.SysproMaterialIssue objMat = new BusinessLogic.SysproMaterialIssue();
        private LabelPrint objPrint = new LabelPrint();
        MegasoftEntities mdb = new MegasoftEntities();
        //
        // GET: /WhseManMaterialIssue/

        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            
            return View();
        }


        [HttpPost]
        public ActionResult PostMaterialIssue(string details)
        {
            try
            {
                var resultPostMaterialIssue = objMat.PostMaterialIssue(details);
                return Json(resultPostMaterialIssue, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
