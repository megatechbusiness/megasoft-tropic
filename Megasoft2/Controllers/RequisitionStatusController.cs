using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionStatusController : Controller
    {
        SysproEntities sdb = new SysproEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        RequisitionBL BL = new RequisitionBL();
        // GET: /RequisitionStatus/
        [CustomAuthorize("RequisitionStatus")]
        public ActionResult Index()
        {
            RequisitionStatusViewModel model = new RequisitionStatusViewModel();
            //string User = HttpContext.User.Identity.Name.ToUpper();
            //HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            //var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            //bool Requisitioner = BL.ProgramAccess("CreateRequisition");
            //bool Buyer = BL.ProgramAccess("RequestForQuote");
            //bool Authorizer = BL.ProgramAccess("Authorize");
            //bool ViewOnly = BL.ProgramAccess("RequisitionViewOnly");
            //bool isAdmin = (from a in mdb.mtUsers where a.Username == User select a.Administrator).FirstOrDefault();
            //model.Requisition = sdb.sp_GetRequisitionStatusReqList(Company, User, isAdmin, Requisitioner, Buyer, Authorizer, ViewOnly).ToList();
            //model.Grn = sdb.sp_GetRequisitionStatusGrnList(Company, User).ToList();
            //model.Invoice = sdb.sp_GetRequisitionStatusInvoiceList(Company, User).ToList();
            return View(model);
        }

        
        [CustomAuthorize("RequisitionStatus")]
        [HttpPost]
        public ActionResult Index(RequisitionStatusViewModel model)
        {
            string FilterText = "";
            if(!string.IsNullOrEmpty(model.FilterText))
            {
                FilterText = model.FilterText;
            }
            
            string User = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            bool Requisitioner = BL.ProgramAccess("CreateRequisition");
            bool Buyer = BL.ProgramAccess("RequestForQuote");
            bool Authorizer = BL.ProgramAccess("Authorize");
            bool ViewOnly = BL.ProgramAccess("RequisitionViewOnly");
            bool isAdmin = (from a in mdb.mtUsers where a.Username == User select a.Administrator).FirstOrDefault();
            model.Requisition = sdb.sp_GetRequisitionStatusReqList(Company, User, isAdmin, Requisitioner, Buyer, Authorizer, ViewOnly, FilterText.ToUpper()).ToList();
            model.Grn = sdb.sp_GetRequisitionStatusGrnList(Company, User, FilterText.ToUpper()).ToList();
            model.Invoice = sdb.sp_GetRequisitionStatusInvoiceList(Company, User, FilterText.ToUpper()).ToList();
            model.AwaitingAuth = sdb.sp_GetRequisitionStatusAwaitingAuthList(Company, User, FilterText.ToUpper()).ToList();
            model.PostedInvoice = sdb.sp_GetRequisitionStatusPostedInvoiceList(Company, User, FilterText.ToUpper()).ToList();
            return View("Index", model);
        }

    }
}
