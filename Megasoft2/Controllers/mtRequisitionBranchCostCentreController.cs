using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class mtRequisitionBranchCostCentreController : Controller
    {
        MegasoftEntities db = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        //[CustomAuthorize(Activity: "reqBranch")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var ReqBranchList = (from a in wdb.mtReqBranchCostCentres where a.Company == Company select a).ToList();
            return View(ReqBranchList);
        }

        public ActionResult Create(string Branch = "")
        {
            string Username = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.ReqBranchList = new SelectList(wdb.mtReqBranches.Where(a => a.CompanyCode == Company).ToList(), "Branch", "Branch");
            var CostCentreList = wdb.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
            ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
            try
            {
                var model = (from a in wdb.mtReqBranchCostCentres where a.Branch == Branch && a.Company == Company select a).FirstOrDefault();
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }

        [HttpPost]
        public ActionResult Create(mtReqBranchCostCentre model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ModelState.Clear();
                var BranchCostCentre = (from a in wdb.mtReqBranchCostCentres where a.Branch == model.Branch && a.CostCentre == model.CostCentre && a.Company == Company select a).FirstOrDefault();
                model.Company = Company;
                if (BranchCostCentre == null)
                {
                    model.Company = Company;
                    wdb.Entry(model).State = System.Data.EntityState.Added;
                    wdb.SaveChanges();
                }
                else
                {
                    BranchCostCentre.Branch = model.Branch;
                    wdb.Entry(BranchCostCentre).State = System.Data.EntityState.Modified;
                    wdb.SaveChanges();
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

        public ActionResult Delete(string Branch, string CostCentre)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

            try
            {

                var BranchCostCentre = (from a in wdb.mtReqBranchCostCentres where a.Branch == Branch && a.CostCentre == CostCentre && a.Company == Company select a).FirstOrDefault();
                wdb.Entry(BranchCostCentre).State = System.Data.EntityState.Deleted;
                wdb.SaveChanges();
                ModelState.AddModelError("", "Deleted.");


                var ReqBranchList = (from a in wdb.mtReqBranchCostCentres where a.Company == Company select a).ToList();
                return View("Index", ReqBranchList);
            }
            catch (Exception ex)
            {
                var ReqBranchList = (from a in wdb.mtReqBranchCostCentres where a.Company == Company select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", ReqBranchList);
            }
        }

    }
}
