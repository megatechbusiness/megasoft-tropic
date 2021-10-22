using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.Models;
using Megasoft2.ViewModel;


namespace Megasoft2.Controllers
{
    public class RequisitionEnquiryController : Controller
    {

        SysproEntities sys = new SysproEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        // GET: /RequisitionEnquiry/

        [CustomAuthorize(Activity: "RequisitionEnquiry")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            ViewBag.Holder = (from a in db.sp_mtReqGetRequisitionList(requser, Company) select new { Holder = a.Holder }).Distinct();
            return View("Index");            
        }

        [MultipleButton(Name = "action", Argument = "ReqLoad")]
        [HttpPost]
        public ActionResult ReqLoad(RequisitionEnquiryViewModel model)
        {
            ModelState.Clear();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            ViewBag.Holder = (from a in db.sp_mtReqGetRequisitionList(requser, Company) select new { Holder = a.Holder }).Distinct();
            try
            {
                var reqDetail = db.sp_mtReqGetRequisitionEnquiry(model.ReqOrPONumber, model.Holder).ToList();
                model.Enquiry = reqDetail;
                ModelState.AddModelError("","Requisitions Loaded Successful!");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }
        

    }
}
