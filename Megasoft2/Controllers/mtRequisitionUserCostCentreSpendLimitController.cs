using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class mtRequisitionUserCostCentreSpendLimitController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities db = new WarehouseManagementEntities("");

        //[CustomAuthorize(Activity: "reqBranch")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            var ReqUserCostCentreSpendLimitList = (from a in mdb.mtReqUserCostCentreSpendLimits where a.Company == Company select a).ToList();
            return View(ReqUserCostCentreSpendLimitList);
        }

        public ActionResult Create(string Username = "")
        {

            string LoginUser = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.UserList = new SelectList(mdb.mtUsers.ToList(), "Username", "Username");
            var CostCentreList = db.sp_GetUserDepartments(Company, LoginUser).Where(a => a.Allowed == true).ToList();
            ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
            ViewBag.LocalCurrencyList = LoadYesNo();//new SelectList(db.mtReqUserCostCentreSpendLimits.Where(a => a.Company == Company).ToList(), "LocalCurrency", "LocalCurrency");

            try
            {

                var model = new RequisitionSpendLimitViewModel();
                model.UserSpendLimitDetail = (from a in mdb.mtReqUserCostCentreSpendLimits where a.Username == Username && a.Company == Company select a).ToList();
                model.UserSpendLimit = model.UserSpendLimitDetail.FirstOrDefault();


                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }

        }

        public List<SelectListItem> LoadYesNo()
        {
            List<SelectListItem> Yesno = new List<SelectListItem>
            {
                new SelectListItem{Text = "Y", Value = "Y"},
                new SelectListItem{Text = "N", Value = "N"}
            };

            return Yesno;
        }


        [HttpPost]
        public ActionResult Create(RequisitionSpendLimitViewModel model)
        {
            ModelState.Clear();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            string Username = HttpContext.User.Identity.Name.ToUpper();
            ViewBag.UserList = new SelectList(mdb.mtUsers.ToList(), "Username", "Username");
            var CostCentreList = db.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
            ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
            ViewBag.LocalCurrencyList = LoadYesNo();//new SelectList(db.mtReqUserCostCentreSpendLimits.Where(a => a.Company == Company).ToList(), "LocalCurrency", "LocalCurrency");
            try
            {
                var Supp = (from a in mdb.mtReqUserCostCentreSpendLimits where a.Username == model.UserSpendLimit.Username && a.CostCentre == model.UserSpendLimit.CostCentre && a.Company == Company select a).FirstOrDefault();
                model.UserSpendLimit.Company = Company;
                if (Supp == null)
                {
                    model.UserSpendLimit.Company = Company;
                    model.UserSpendLimit.Category = "";
                    mdb.Entry(model.UserSpendLimit).State = System.Data.EntityState.Added;
                    mdb.SaveChanges();
                }
                else
                {

                    Supp.SpendLimit = model.UserSpendLimit.SpendLimit;
                    Supp.Currency = model.UserSpendLimit.Currency;
                    Supp.LocalCurrency = model.UserSpendLimit.LocalCurrency;
                    model.UserSpendLimit.Category = "";
                    mdb.Entry(Supp).State = System.Data.EntityState.Modified;
                    mdb.SaveChanges();

                }

                model.UserSpendLimitDetail = (from a in mdb.mtReqUserCostCentreSpendLimits where a.Username == model.UserSpendLimit.Username && a.Company == Company select a).ToList();
                model.UserSpendLimit = model.UserSpendLimitDetail.FirstOrDefault();

                ModelState.AddModelError("", "Saved.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }

        public ActionResult Delete(string CostCentre)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            try
            {

                var pclass = (from a in mdb.mtReqUserCostCentreSpendLimits where a.CostCentre == CostCentre && a.Company == Company select a).FirstOrDefault();
                mdb.Entry(pclass).State = System.Data.EntityState.Deleted;
                mdb.SaveChanges();
                ModelState.AddModelError("", "Deleted.");


                var ReqUserCostCentreSpendLimitList = (from a in mdb.mtReqUserCostCentreSpendLimits where a.Company == Company select a).ToList();
                return View("Index", ReqUserCostCentreSpendLimitList);
            }
            catch (Exception ex)
            {
                var ReqUserCostCentreSpendLimitList = (from a in mdb.mtReqUserCostCentreSpendLimits where a.Company == Company select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", ReqUserCostCentreSpendLimitList);
            }
        }

    }
}

