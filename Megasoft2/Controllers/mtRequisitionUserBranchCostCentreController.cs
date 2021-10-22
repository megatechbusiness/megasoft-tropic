using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class mtRequisitionUserBranchCostCentreController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        //[CustomAuthorize(Activity: "mtReqUserBranchCostCentre")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            RequisitionUserBranchCostCentreViewModel model = new RequisitionUserBranchCostCentreViewModel();
            model.Detail = db.sp_mtReqGetUserBranchCostCentreAccess(Company).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(RequisitionUserBranchCostCentreViewModel model)
        {
            ModelState.Clear();
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                foreach (var item in model.Detail)
                {
                    var check = (from a in db.mtReqUserBranchCostCentres where a.Company == Company && a.Username == item.Username && a.Branch == item.Branch && a.CostCentre == item.CostCentre select a).FirstOrDefault();
                    if (check == null)
                    {
                        if (item.Allowed)
                        {
                            mtReqUserBranchCostCentre obj = new mtReqUserBranchCostCentre();
                            obj.Company = Company;
                            obj.Username = item.Username;
                            obj.Branch = item.Branch;
                            obj.CostCentre = item.CostCentre;
                            db.Entry(obj).State = System.Data.EntityState.Added;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        if (item.Allowed == false)
                        {
                            db.Entry(check).State = System.Data.EntityState.Deleted;
                            db.SaveChanges();
                        }
                    }
                }

                ModelState.AddModelError("", "Saved!");


                model.Detail = db.sp_mtReqGetUserBranchCostCentreAccess(Company).ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }

        }

        public ActionResult Create(string Branch = "")
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            ViewBag.UserList = new SelectList(mdb.mtUsers.ToList(), "Username", "Username");
            ViewBag.ReqBranchList = new SelectList(db.mtReqBranches.Where(a => a.CompanyCode == Company).ToList(), "Branch", "Branch");
            ViewBag.CostCentreList = new SelectList(db.mtReqCostCentres.Where(a => a.Company == Company).ToList(), "CostCentre", "CostCentre");
            try
            {
                var model = (from a in db.mtReqUserBranchCostCentres where a.Branch == Branch && a.Company == Company select a).FirstOrDefault();
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }

        [HttpPost]
        public ActionResult Create(mtReqUserBranchCostCentre model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                ModelState.Clear();
                var Supp = (from a in db.mtReqUserBranchCostCentres where a.Branch == model.Branch && a.CostCentre == model.CostCentre && a.Company == Company select a).FirstOrDefault();
                model.Company = Company;
                if (Supp == null)
                {
                    model.Company = Company;
                    db.Entry(model).State = System.Data.EntityState.Added;
                    db.SaveChanges();
                }
                else
                {
                    Supp.Branch = model.Branch;
                    db.Entry(Supp).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }
                ModelState.AddModelError("", "Saved.");
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }

        public ActionResult Delete(string Branch)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            try
            {

                var pclass = (from a in db.mtReqUserBranchCostCentres where a.Branch == Branch && a.Company == Company select a).FirstOrDefault();
                db.Entry(pclass).State = System.Data.EntityState.Deleted;
                db.SaveChanges();
                ModelState.AddModelError("", "Deleted.");


                var UserBranchCostCentreList = (from a in db.mtReqUserBranchCostCentres where a.Company == Company select a).ToList();
                return View("Index", UserBranchCostCentreList);
            }
            catch (Exception ex)
            {
                var UserBranchCostCentreList = (from a in db.mtReqUserBranchCostCentres where a.Company == Company select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", UserBranchCostCentreList);
            }
        }
    }
}