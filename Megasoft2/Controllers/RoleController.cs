using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RoleController : Controller
    {
        private MegasoftEntities db = new MegasoftEntities();

        //
        // GET: /Role/
        [CustomAuthorize(Activity: "Roles")]
        public ActionResult Index()
        {
            return View(db.mtRoles.ToList());
        }

      

        //
        // GET: /Role/Create
        [CustomAuthorize(Activity: "Roles")]
        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /Role/Create
        [CustomAuthorize(Activity: "Roles")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mtRole mtrole)
        {
            if (ModelState.IsValid)
            {
                db.mtRoles.Add(mtrole);
                db.SaveChanges();
                return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
            }

            return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Role/Edit/5
        [CustomAuthorize(Activity: "Roles")]
        public ActionResult Edit(string id = null)
        {
            mtRole mtrole = db.mtRoles.Find(id);
            if (mtrole == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtrole);
        }

        //
        // POST: /Role/Edit/5
        [CustomAuthorize(Activity: "Roles")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtRole mtrole)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mtrole).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
            }
            return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Role/Delete/5
        [CustomAuthorize(Activity: "Roles")]
        public ActionResult Delete(string id = null)
        {
            mtRole mtrole = db.mtRoles.Find(id);
            if (mtrole == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtrole);
        }

        //
        // POST: /Role/Delete/5
        [CustomAuthorize(Activity: "Roles")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            mtRole mtrole = db.mtRoles.Find(id);
            db.mtRoles.Remove(mtrole);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [CustomAuthorize(Activity: "Roles")]
        public ActionResult SetAccess(string id)
        {            
            ViewBag.Role = id;
            List<sp_GetRoleAccess_Result> RoleFunc = db.sp_GetRoleAccess(id).ToList();
            
            var functionsGrouped = from b in RoleFunc
                                    group b by b.Program into g
                                   select new Group<sp_GetRoleAccess_Result, string> { Key = g.Key, Values = g };
            return PartialView(functionsGrouped.ToList());
        }

        [CustomAuthorize(Activity: "Roles")]
        [HttpPost]
        public ActionResult SaveRoleAccess(string details)
        {
            try
            {
                List<sp_GetRoleAccess_Result> myDeserializedObjList = (List<sp_GetRoleAccess_Result>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<sp_GetRoleAccess_Result>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var check = (from a in db.mtRoleFunctions where a.Role == item.Role.Trim() && a.ProgramFunction == item.ProgramFunction.Trim() select a.Role).ToList();
                        if (check.Count > 0)
                        {
                            if (item.HasAccess == "False")
                            {
                                var RoleFunction = new mtRoleFunction { Role = item.Role.Trim(), ProgramFunction = item.ProgramFunction.Trim() };
                                db.Entry(RoleFunction).State = EntityState.Deleted;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            if (item.HasAccess == "True")
                            {
                                var RoleFunction = new mtRoleFunction { Role = item.Role.Trim(), ProgramFunction = item.ProgramFunction.Trim() };
                                db.mtRoleFunctions.Add(RoleFunction);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}