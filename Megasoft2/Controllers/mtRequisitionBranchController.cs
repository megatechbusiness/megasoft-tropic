using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class mtRequisitionBranchController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        WarehouseManagementEntities db = new WarehouseManagementEntities("");

        //[CustomAuthorize(Activity: "reqBranch")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var ReqBranchList = (from a in db.mtReqBranches where a.CompanyCode == Company select a).ToList();
            return View(ReqBranchList);
        }

        public ActionResult Create(string Branch = "")
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var model = (from a in db.mtReqBranches where a.Branch == Branch && a.CompanyCode == Company select a).FirstOrDefault();
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create");
            }
        }
        [HttpPost]
        public ActionResult Create(mtReqBranch model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ModelState.Clear();
                var br = (from a in db.mtReqBranches where a.Branch == model.Branch && a.CompanyCode == Company select a).FirstOrDefault();
                if (br == null)
                {
                    model.CompanyCode = Company;
                    db.Entry(model).State = System.Data.EntityState.Added;
                    db.SaveChanges();
                }
                else
                {
                    br.Description = model.Description;
                    db.Entry(br).State = System.Data.EntityState.Modified;
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

                var br = (from a in db.mtReqBranches where a.Branch == Branch && a.CompanyCode == Company select a).FirstOrDefault();
                db.Entry(br).State = System.Data.EntityState.Deleted;
                db.SaveChanges();
                ModelState.AddModelError("", "Deleted.");


                var branchList = (from a in db.mtReqBranches where a.CompanyCode == Company select a).ToList();
                return View("Index", branchList);
            }
            catch (Exception ex)
            {
                var ReqBranchList = (from a in db.mtReqBranches where a.CompanyCode == Company select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", ReqBranchList);
            }
        }

    }
}