using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using Megasoft2.BusinessLogic;

namespace Megasoft2.Controllers
{
    public class HomeController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        SysproEntities sdb = new SysproEntities("");
        //RequisitionBL BL = new RequisitionBL();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        //
        // GET: /Home/
        [Authorize]
        public ActionResult Index()
        {
            //string User = HttpContext.User.Identity.Name.ToUpper();
            //HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            ////Get Authorization level for Invoice Review


            //var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            //// var GrnPO = sdb.sp_PurchaseOrderGrnList().ToList().Count();
            //// var InvMatch = sdb.sp_GetGrnListForInvoicing().ToList().Count();
            //// int AuthLevel = BL.GetInvoiceAuthorisationLevel();
            //// var Invoices = sdb.sp_GetInvoicesForReview(AuthLevel).ToList().Count();


            ////Check if user has access
            //bool Requisitioner = BL.ProgramAccess("CreateRequisition");
            //bool Buyer = BL.ProgramAccess("RequestForQuote");
            //bool Authorizer = BL.ProgramAccess("Authorize");
            //bool isInvMatching = BL.ProgramAccess("InvoiceMatching");
            //bool isAdmin = (from a in mdb.mtUsers where a.Username == User select a.Administrator).FirstOrDefault();
            //bool isGrn = BL.ProgramAccess("Grn");
            //bool isInvoicer = BL.ProgramAccess("InvoiceReview");
            //bool isPostManager = BL.ProgramAccess("PostingManager");

            //RequisitionViewModel req = new RequisitionViewModel();
            //req.Status = sdb.sp_GetReqStatus(Company, User, Requisitioner, Buyer, Authorizer, isAdmin, isPostManager).ToList();

            //ViewBag.Req = Requisitioner;
            //ViewBag.Buyer = Buyer;
            //ViewBag.Authorizer = Authorizer;
            //ViewBag.isAdmin = isAdmin;
            //ViewBag.isPostManager = isPostManager;

            //if (isInvMatching == true)
            //{
            //    var InvMatch = sdb.sp_GetGrnListForInvoicing(User, Company).ToList().Count();
            //    ViewBag.InvMatch = InvMatch;
            //    ViewBag.isInvMatching = isInvMatching;

            //}
            //if (isGrn == true)
            //{
            //    var GrnPO = sdb.sp_PurchaseOrderGrnList(Company, User).ToList().Count();
            //    ViewBag.Grn = GrnPO;
            //    ViewBag.isGrn = isGrn;
            //}
            //if (isInvoicer == true)
            //{
            //    int AuthLevel = BL.GetInvoiceAuthorisationLevel();
            //    var Invoices = sdb.sp_GetInvoicesForReview(AuthLevel).ToList().Count();
            //    ViewBag.Invoices = Invoices;
            //    ViewBag.isInvoicer = isInvoicer;
            //}
            //if (isAdmin == true)
            //{
            //    var Post = sdb.sp_GetReqStatus(Company, User, false, false, false, false, true).ToList();
            //    ViewBag.PostError = Post[0].Error;
            //}

            //return View(req);

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminIndex()
        {
            return View();
        }


        public ActionResult Home()
        {
            return View();
        }

        public ActionResult Cards()
        {
            return View();
        }


        public ActionResult GetMenu()
        {
            var menu = mdb.sp_GetOpFunctionMenu(HttpContext.User.Identity.Name.ToUpper()).ToList();

            MenuViewModel model = new MenuViewModel();
            var topMenu = menu.Select(i => new { i.Menu, i.IconClass, i.MenuSequence }).Distinct().OrderBy(b => b.MenuSequence).ToList();
            model.Header = (from a in topMenu select new MenuViewModel.MenuHeader { Menu = a.Menu, Icon = a.IconClass }).ToList();
            model.Detail = menu.OrderBy(a => a.SubMenuSequence).ToList();

            return PartialView(model);
        }



        public JsonResult GetMegasoftAlerts()
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                string User = HttpContext.User.Identity.Name.ToUpper();
                var AlertList = wdb.sp_GetMegasoftAlerts(User, false).ToList();
                return Json(AlertList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
