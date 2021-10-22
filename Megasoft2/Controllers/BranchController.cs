using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class BranchController : Controller
    {
        private SysproEntities db = new SysproEntities("");

        //
        // GET: /Branch/
        [CustomAuthorize(Activity: "Branches")]
        public ActionResult Index()
        {
            try
            {
                return View(db.mtBranches.ToList());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }


        //
        // GET: /Branch/Create
        [CustomAuthorize(Activity: "Branches")]
        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /Branch/Create
        [CustomAuthorize(Activity: "Branches")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mtBranch mtbranch)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var BranchCheck = (from a in db.mtBranches where a.Branch == mtbranch.Branch select a).ToList();
                    if(BranchCheck.Count > 0)
                    {
                        return Json("Error : Branch " + mtbranch.Branch + " already exists!", JsonRequestBehavior.AllowGet);
                    }
                    db.mtBranches.Add(mtbranch);
                    db.SaveChanges();
                    return Json(new { redirectTo = Url.Action("Index", "Sites", new { Branch = mtbranch.Branch }) });
                }

                return View("Index");
            }
            catch (Exception ex)
            {
                return Json("Error : " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /Branch/Edit/5
        [CustomAuthorize(Activity: "Branches")]
        public ActionResult Edit(string id = null)
        {
            try
            {
                mtBranch mtbranch = db.mtBranches.Find(id);
                if (mtbranch == null)
                {
                    return HttpNotFound();
                }
                return View(mtbranch);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        //
        // POST: /Branch/Edit/5
        [CustomAuthorize(Activity: "Branches")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtBranch mtbranch)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mtbranch).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mtbranch);
        }

        //
        // GET: /Branch/Delete/5
        [CustomAuthorize(Activity: "Branches")]
        public ActionResult Delete(string id = null)
        {
            mtBranch mtbranch = db.mtBranches.Find(id);
            if (mtbranch == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtbranch);
        }

        //
        // POST: /Branch/Delete/5
        [CustomAuthorize(Activity: "Branches")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                mtBranch mtbranch = db.mtBranches.Find(id);
                db.mtBranches.Remove(mtbranch);
                

                List<mtBranchSite> mtsite = (from a in db.mtBranchSites where a.Branch == id select a).ToList();
                foreach(var item in mtsite)
                {
                    db.mtBranchSites.Remove(item);
                }
                
                

                List<mtBranchSiteDept> mtdept = (from a in db.mtBranchSiteDepts where a.Branch == id select a).ToList();
                foreach (var item in mtdept)
                {
                    db.mtBranchSiteDepts.Remove(item);
                }
               

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}