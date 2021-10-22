using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionStatusGLController : Controller
    {
        //
        // GET: /RequisitionStatusGL/
        SysproEntities sdb = new SysproEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        RequisitionBL BL = new RequisitionBL();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(RequisitionStatusGLViewModel model)
        {
            try
            {
                var result = (from a in sdb.GenMasters where a.GlCode == model.GlCode select a).ToList();
                if(result.Count > 0)
                {
                    //RequisitionStatusGLViewModel model = new RequisitionStatusGLViewModel();
                    string User = HttpContext.User.Identity.Name.ToUpper();
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    bool Requisitioner = BL.ProgramAccess("CreateRequisition");
                    bool Buyer = BL.ProgramAccess("RequestForQuote");
                    bool Authorizer = BL.ProgramAccess("Authorize");
                    bool ViewOnly = BL.ProgramAccess("RequisitionViewOnly");
                    bool isAdmin = (from a in mdb.mtUsers where a.Username == User select a.Administrator).FirstOrDefault();
                    model.Requisition = sdb.sp_GetRequisitionStatusReqListByGL(Company, User, isAdmin, Requisitioner, Buyer, Authorizer, ViewOnly, model.GlCode, model.Job).ToList();
                    model.Grn = sdb.sp_GetRequisitionStatusGrnListByGL(Company, User, model.GlCode, model.Job).ToList();
                    model.Invoice = sdb.sp_GetRequisitionStatusInvoiceListByGL(Company, User, model.GlCode, model.Job).ToList();
                    model.AwaitingAuth = sdb.sp_GetRequisitionStatusAwaitingAuthByGL(Company, User, model.GlCode, model.Job).ToList();
                    model.PostedInvoice = sdb.sp_GetRequisitionStatusPostedInvoiceListByGL(Company, User, model.GlCode, model.Job).ToList();
                    model.Description = result.FirstOrDefault().Description;
                    return View(model);
                }
                ModelState.AddModelError("", "GL Code not found");
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            
        }


        public ActionResult GlCodeList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var result = sdb.sp_GetGlCodesByUser(HttpContext.User.Identity.Name.ToUpper(), Company, FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GlCodeSearch()
        {
            return PartialView();
        }

        public JsonResult JobList(string GlCode, string FilterText)
        {
            var result = sdb.sp_GetJobsByGlCode(GlCode, FilterText);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult JobSearch()
        {
            return PartialView();
        }

    }
}
