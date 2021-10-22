using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class SitesController : Controller
    {
        private SysproEntities db = new SysproEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();

        //
        // GET: /Sites/

        public ActionResult Index(string Branch = null)
        {
            ViewBag.Branch = Branch;
            mtBranchSite sites = new mtBranchSite();
            var BranchSites = (from a in db.mtBranchSites where a.Branch == Branch select a).ToList();
            return View(BranchSites);
        }

        //
        // GET: /Sites/Create

        public ActionResult Create(string Branch)
        {
            ViewBag.Branch = Branch;
            return View();
        }

        //
        // POST: /Sites/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mtBranchSite mtbranchsite)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var BranchSiteCheck = (from a in db.mtBranchSites where a.Branch == mtbranchsite.Branch && a.Site == mtbranchsite.Site select a).ToList();
                    if (BranchSiteCheck.Count > 0)
                    {
                        return Json("Error : Branch " + mtbranchsite.Branch + " - " + mtbranchsite.Site + " already exists!", JsonRequestBehavior.AllowGet);
                    }
                    db.mtBranchSites.Add(mtbranchsite);
                    db.SaveChanges();
                    return Json(new { redirectTo = Url.Action("Index", "Departments", new { Branch = mtbranchsite.Branch, Site = mtbranchsite.Site }) });
                }

                return View("Index");
            }
            catch (Exception ex)
            {
                return Json("Error : " + ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // GET: /Sites/Edit/5

        public ActionResult Edit(string Branch = null, string Site = null)
        {
            mtBranchSite mtbranchsite = (from a in db.mtBranchSites where a.Branch == Branch && a.Site == Site select a).FirstOrDefault();
            if (mtbranchsite == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtbranchsite);
        }

        //
        // POST: /Sites/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtBranchSite mtbranchsite)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mtbranchsite).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { Branch = mtbranchsite.Branch });
            }
            return RedirectToAction("Index", new { Branch = mtbranchsite.Branch });
        }

        //
        // GET: /Sites/Delete/5

        public ActionResult Delete(string Branch = null, string Site = null)
        {
            mtBranchSite mtbranchsite = (from a in db.mtBranchSites where a.Branch == Branch && a.Site == Site select a).FirstOrDefault();
            if (mtbranchsite == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtbranchsite);
        }

        //
        // POST: /Sites/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Branch = null, string Site = null)
        {
            mtBranchSite mtbranchsite = (from a in db.mtBranchSites where a.Branch == Branch && a.Site == Site select a).FirstOrDefault();
            db.mtBranchSites.Remove(mtbranchsite);

            List<mtBranchSiteDept> mtdept = (from a in db.mtBranchSiteDepts where a.Site == Site && a.Branch == Branch select a).ToList();
            
            foreach(var item in mtdept)
            {
                db.mtBranchSiteDepts.Remove(item);
            }

            db.SaveChanges();
            return RedirectToAction("Index", new { Branch = Branch });
        }


        public ActionResult GlCodeList(string Branch, string Site, string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var result = db.sp_GetGlCodesByBranchSite(Branch, Site, HttpContext.User.Identity.Name.ToUpper(), Company, FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GlCodeSearch(string Branch, string Site)
        {
            ViewBag.Branch = Branch;
            ViewBag.Site = Site;
            return PartialView();
        }


        public JsonResult ApBranchList()
        {
            var result = (from a in db.ApBranches select new { Branch = a.Branch, Description = a.Description, ApBrnGlCode = a.ApBrnGlCode }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ApBranchSearch()
        {            
            return PartialView();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}