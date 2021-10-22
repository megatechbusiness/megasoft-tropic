using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class DepartmentsController : Controller
    {
        private SysproEntities db = new SysproEntities("");

        //
        // GET: /Departments/

        public ActionResult Index(string Branch = null, string Site = null)
        {
            ViewBag.Branch = Branch;
            ViewBag.Site = Site;
            var Dept = (from a in db.mtBranchSiteDepts where a.Branch == Branch && a.Site == Site select a).ToList();
            return View(Dept);
        }

        //
        // GET: /Departments/Create

        public ActionResult Create(string Branch, string Site)
        {
            ViewBag.Branch = Branch;
            ViewBag.Site = Site;
            ViewBag.Departments = (from a in db.mtDepartments
                                   where !(from o in db.mtBranchSiteDepts where o.Branch == Branch && o.Site == Site
                                           select o.Department)
                                       .Contains(a.Department)
                                   select new SelectListItem { Text = a.Department + " - " + a.Name, Value = a.Department }).ToList();
            return PartialView();
        }

        //
        // POST: /Departments/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mtBranchSiteDept mtbranchsitedept)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var BranchSiteDeptCheck = (from a in db.mtBranchSiteDepts where a.Branch == mtbranchsitedept.Branch && a.Site == mtbranchsitedept.Site && a.Department == mtbranchsitedept.Department select a).ToList();
                    if (BranchSiteDeptCheck.Count > 0)
                    {
                        return Json("Error : Branch " + mtbranchsitedept.Branch + " - " + mtbranchsitedept.Site + " - " + mtbranchsitedept.Department + " already exists!", JsonRequestBehavior.AllowGet);
                    }

                    var DepartmentName = (from a in db.mtDepartments where a.Department == mtbranchsitedept.Department select a.Name).FirstOrDefault().Trim();
                    mtbranchsitedept.DepartmentName = DepartmentName;

                    db.mtBranchSiteDepts.Add(mtbranchsitedept);
                   
                    db.SaveChanges();
                    return Json(new { redirectTo = Url.Action("Index", "Departments", new { Branch = mtbranchsitedept.Branch, Site = mtbranchsitedept.Site }) });
                }

                return View("Index");
            }
            catch (Exception ex)
            {
                return Json("Error : " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /Departments/Edit/5

        public ActionResult Edit(string Branch = null, string Site = null, string Department = null)
        {
            mtBranchSiteDept mtbranchsitedept = (from a in db.mtBranchSiteDepts where a.Branch == Branch && a.Site == Site && a.Department == Department select a).FirstOrDefault();
            if (mtbranchsitedept == null)
            {
                return HttpNotFound();
            }
            return View(mtbranchsitedept);
        }

        //
        // POST: /Departments/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtBranchSiteDept mtbranchsitedept)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mtbranchsitedept).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Departments", new { Branch = mtbranchsitedept.Branch, Site = mtbranchsitedept.Site });
            }
            return View(mtbranchsitedept);
        }

        //
        // GET: /Departments/Delete/5

        public ActionResult Delete(string Branch = null, string Site = null, string Department = null)
        {
            mtBranchSiteDept mtbranchsitedept = (from a in db.mtBranchSiteDepts where a.Branch == Branch && a.Site == Site && a.Department == Department select a).FirstOrDefault();
            if (mtbranchsitedept == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtbranchsitedept);
        }

        //
        // POST: /Departments/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Branch = null, string Site = null, string Department = null)
        {
            mtBranchSiteDept mtbranchsitedept = (from a in db.mtBranchSiteDepts where a.Branch == Branch && a.Site == Site && a.Department == Department select a).FirstOrDefault();
            db.mtBranchSiteDepts.Remove(mtbranchsitedept);

            db.SaveChanges();
            return RedirectToAction("Index", "Departments", new { Branch = mtbranchsitedept.Branch, Site = mtbranchsitedept.Site });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}