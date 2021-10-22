using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class DepartmentMasterController : Controller
    {
        private SysproEntities db = new SysproEntities("");

        //
        // GET: /DepartmentMaster/
        [CustomAuthorize(Activity: "Departments")]
        public ActionResult Index()
        {
            return View(db.mtDepartments.ToList());
        }


        //
        // GET: /DepartmentMaster/Create
        [CustomAuthorize(Activity: "Departments")]
        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /DepartmentMaster/Create
        [CustomAuthorize(Activity: "Departments")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mtDepartment mtdepartment)
        {
            if (ModelState.IsValid)
            {
                mtdepartment.Operator = HttpContext.User.Identity.Name.ToUpper().Trim();
                mtdepartment.TrnDate = DateTime.Now;
                db.mtDepartments.Add(mtdepartment);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(mtdepartment);
        }

        //
        // GET: /DepartmentMaster/Edit/5
        [CustomAuthorize(Activity: "Departments")]
        public ActionResult Edit(string id = null)
        {
            mtDepartment mtdepartment = db.mtDepartments.Find(id);
            if (mtdepartment == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtdepartment);
        }

        //
        // POST: /DepartmentMaster/Edit/5
        [CustomAuthorize(Activity: "Departments")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtDepartment mtdepartment)
        {
            if (ModelState.IsValid)
            {
                mtdepartment.Operator = HttpContext.User.Identity.Name.ToUpper().Trim();
                mtdepartment.TrnDate = DateTime.Now;
                db.Entry(mtdepartment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mtdepartment);
        }

        //
        // GET: /DepartmentMaster/Delete/5
        [CustomAuthorize(Activity: "Departments")]
        public ActionResult Delete(string id = null)
        {
            mtDepartment mtdepartment = db.mtDepartments.Find(id);
            if (mtdepartment == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtdepartment);
        }

        //
        // POST: /DepartmentMaster/Delete/5
        [CustomAuthorize(Activity: "Departments")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            mtDepartment mtdepartment = db.mtDepartments.Find(id);
            db.mtDepartments.Remove(mtdepartment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}