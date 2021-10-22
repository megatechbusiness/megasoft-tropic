using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Megasoft2.Controllers
{
    public class UserController : Controller
    {
        private MegasoftEntities db = new MegasoftEntities();
        private SysproEntities sdb = new SysproEntities("");
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        //
        // GET: /User/
        [CustomAuthorize(Activity:"Users")]
        public ActionResult Index()
        {
            
            var UseRoles = (from a in db.mtSystemSettings select a.UseRoles).FirstOrDefault();
            ViewBag.UseRoles = UseRoles;
            return View(db.mtUsers.ToList());
        }

        //
        // GET: /User/Create
        [CustomAuthorize(Activity: "Users")]
        public ActionResult Create(string Username = null)
        {
            try
            {                
                var UseRoles = (from a in db.mtSystemSettings select a.UseRoles).FirstOrDefault();
                ViewBag.UseRoles = UseRoles;
                var Roles = (from a in db.mtRoles select a);
                ViewBag.Roles = Roles;

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var userModel = new UserViewModel();
                var user = (from a in db.mtUsers where a.Username == Username select a).FirstOrDefault();
                userModel.User = user;
                userModel.FunctionAccess = db.sp_GetOpFunctionAccess(Username).ToList();
                
                if(Username != null)
                {
                    if (userModel.User.IsBuyer == true)
                    {
                        userModel.BuyerReq = db.sp_GetBuyerRequisitionerLink(Username).ToList();
                    }
                }
                

                //Get Installation  Options
                var settings = (from a in db.mtSystemSettings where a.Id == 1 select a).FirstOrDefault();

                if(settings.BranchAccess == true)
                {
                    userModel.BranchAccess = sdb.sp_GetBranchAccess(Company, Username).ToList();
                }
                
                if (settings.GlAuthorisation == true)
                {
                    userModel.Authorization = sdb.sp_GetAuthorizationGlCodes(Company, Username).ToList();
                }         
                
                if (settings.ProductClassLimit == true)
                {
                    userModel.ProdClassLimit = sdb.sp_GetProductClassLimits(Company, Username).ToList();
                    
                    ////Should create installation option for BuyerStats as well
                    userModel.UserBuyerStats = db.sp_GetUserBuyerStats(Username).ToList();
                }
                if (settings.ScaleAccess == true)
                {
                    userModel.Scales = wdb.sp_GetUserScales(Company, Username).ToList();
                }
                if (settings.WarehouseAccess == true)
                {
                    userModel.Warehouses = wdb.sp_GetUserWarehouses(Company, Username).ToList();
                }
                if (settings.PrinterAccess == true)
                {
                    userModel.Printers = db.sp_GetUserPrinters(Company, Username).ToList();
                }
                if (settings.DepartmentAccess == true)
                {
                    userModel.Departments = wdb.sp_GetUserDepartments(Company, Username).ToList();
                }
                if (settings.ReportAccess == true)
                {
                    userModel.Reports = wdb.sp_GetUserReports(Company, Username).ToList();
                }

                ViewBag.BranchAccess = settings.BranchAccess;
                ViewBag.GlAuthorisation = settings.GlAuthorisation;
                ViewBag.ProductClassLimit = settings.ProductClassLimit;
                ViewBag.Scale = settings.ScaleAccess;
                ViewBag.PrinterAccess = settings.PrinterAccess;
                ViewBag.WarehouseAccess = settings.WarehouseAccess;
                ViewBag.Department = settings.DepartmentAccess;
                ViewBag.Report = settings.ReportAccess;

                return View(userModel);

            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        //
        // POST: /User/Create
        [CustomAuthorize(Activity: "Users")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel userModel)
        {
            try
            {
                ModelState.Clear();
                var UseRoles = (from a in db.mtSystemSettings select a.UseRoles).FirstOrDefault();
                ViewBag.UseRoles = UseRoles;
                var Roles = (from a in db.mtRoles select a);
                ViewBag.Roles = Roles;
                if (ModelState.IsValid)
                {
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    var checkUser = (from a in db.mtUsers.AsNoTracking() where a.Username == userModel.User.Username select a).ToList();
                    if(checkUser.Count > 0)
                    {
                        
                        db.Entry(userModel.User).State = EntityState.Modified;
                        db.SaveChanges();
                        
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(userModel.User.ReqPrefix))
                        {
                            var ReqPre = (from a in db.mtUsers where a.ReqPrefix == userModel.User.ReqPrefix select a).ToList();
                            if (ReqPre.Count > 0)
                            {
                                ModelState.AddModelError("", "Requisition Prefix already in use. Please choose a new Prefix.");
                                return View(userModel);
                            }
                        }
                        
                        if(userModel.User.LastReqNo == null)
                        {
                            userModel.User.LastReqNo = 0;
                        }
                        db.mtUsers.Add(userModel.User);
                        db.SaveChanges();
                        //this.Dispose();
                    }


                    //Get Installation  Options
                    var settings = (from a in db.mtSystemSettings where a.Id == 1 select a).FirstOrDefault();

                    if (settings.BranchAccess == true)
                    {
                        //Branch Access

                        db.sp_DeleteUserBranchAccess(Company, userModel.User.Username);

                        foreach (var item in userModel.BranchAccess)
                        {
                            db.mtOpBranchAccesses.Add(new mtOpBranchAccess
                            {
                                Branch = item.Branch,
                                Site = item.Site,
                                Department = item.Department,
                                Allowed = item.Allowed,
                                Operator = userModel.User.Username,
                                Company = Company
                            });
                            db.SaveChanges();
                        }
                    }

                    

                    //User Access
                    db.sp_DeleteUserSecurityAccess(userModel.User.Username);

                    foreach(var item in userModel.FunctionAccess)
                    {
                        if(item.HasAccess == true)
                        {
                            db.mtOpFunctions.Add(new mtOpFunction
                            {
                                Username = userModel.User.Username.ToUpper(),
                                Program = item.Program,
                                ProgramFunction = item.ProgramFunction
                            });
                            db.SaveChanges();
                        }                        
                    }

                    //Gl Code Access
                    if (settings.GlAuthorisation == true)
                    {
                        
                        db.sp_DeleteUserGlCodeAccess(Company, userModel.User.Username);

                        if (userModel.Authorization != null)
                        {
                            var SelectedCode = (from a in userModel.Authorization where a.Allowed == true select a).ToList();
                            foreach (var item in SelectedCode)
                            {

                                sdb.sp_SaveGlCodeAccess(Company, userModel.User.Username, item.CostCode);

                            }
                        }
                    }
                    

                    //Product Class Spend Limits
                    if (settings.ProductClassLimit == true)
                    {
                        db.sp_DeleteUserSpendLimits(Company, userModel.User.Username);

                        var Spend = (from a in userModel.ProdClassLimit where a.Limit != 0 select a).ToList();
                        foreach (var item in Spend)
                        {
                            db.mtUserProductClassLimits.Add(new mtUserProductClassLimit
                            {
                                Company = Company,
                                Username = userModel.User.Username.Trim(),
                                ProductClass = item.ProductClass,
                                Limit = item.Limit
                            });
                            db.SaveChanges();

                        }
                        userModel.Authorization = sdb.sp_GetAuthorizationGlCodes(Company, userModel.User.Username.Trim()).ToList();
                    }





                    //Get Installation  Options
                    //var settings = (from a in db.mtSystemSettings where a.Id == 1 select a).FirstOrDefault();

                    if (settings.BranchAccess == true)
                    {
                        userModel.BranchAccess = sdb.sp_GetBranchAccess(Company, userModel.User.Username.Trim()).ToList();
                    }

                    if (settings.GlAuthorisation == true)
                    {
                        userModel.Authorization = sdb.sp_GetAuthorizationGlCodes(Company, userModel.User.Username.Trim()).ToList();
                    }

                    if (settings.ProductClassLimit == true)
                    {
                        userModel.ProdClassLimit = sdb.sp_GetProductClassLimits(Company, userModel.User.Username.Trim()).ToList();

                        //Should create installation option for BuyerStats as well
                        //UserBuyerStats
                        db.sp_DeleteUserBuyerStats(userModel.User.Username);
                        foreach (var item in userModel.UserBuyerStats)
                        {
                            if (item.HasAccess == true)
                            {
                                db.mtUserBuyerStats.Add(new mtUserBuyerStat
                                {
                                    Buyer = item.Buyer,
                                    Username = item.Username

                                });
                                db.SaveChanges();
                            }

                        }
                        userModel.UserBuyerStats = db.sp_GetUserBuyerStats(userModel.User.Username.ToUpper()).ToList();

                        //Should create installation option for BuyerReq Link as well
                        //Buyer Requisitioner Link
                        if(userModel.BuyerReq != null)
                        {
                            if(userModel.BuyerReq.Count > 0)
                            {
                                db.sp_DeleteBuyerRequisitionerLink(userModel.User.Username.ToUpper());
                                foreach (var item in userModel.BuyerReq)
                                {
                                    if (item.HasAccess == true)
                                    {
                                        db.mtBuyerRequisitioners.Add(new mtBuyerRequisitioner
                                        {
                                            Buyer = userModel.User.Username.ToUpper(),
                                            Requisitioner = item.Requisitioner

                                        });
                                        db.SaveChanges();
                                    }

                                }

                                userModel.BuyerReq = db.sp_GetBuyerRequisitionerLink(userModel.User.Username.ToUpper()).ToList();
                            }
                        }
                        
                    }
                    else
                    {
                        if (settings.WarehouseAccess == true)
                        {
                            //Warehouse Access Control
                            if (userModel.Warehouses != null)
                            {
                                if (userModel.Warehouses.Count > 0)
                                {
                                    db.sp_DeleteUserWarehouse(Company, userModel.User.Username);

                                    if (userModel.Warehouses != null)
                                    {
                                        var SelectedCode = (from a in userModel.Warehouses where a.Allowed == true select a).ToList();
                                        foreach (var item in SelectedCode)
                                        {

                                            wdb.sp_SaveWarehouseAccess(Company, userModel.User.Username, item.Warehouse);

                                        }
                                    }
                                }
                            }
                        }
                        if (settings.PrinterAccess == true)
                        {
                            if (userModel.Printers != null)
                            {
                                if (userModel.Printers.Count > 0)
                                {
                                    db.sp_DeleteUserPrinters(Company, userModel.User.Username);

                                    if (userModel.Printers != null)
                                    {
                                        var SelectedCode = (from a in userModel.Printers where a.Allowed == true select a).ToList();
                                        foreach (var item in SelectedCode)
                                        {
                                            db.sp_SavePrinterAccess(Company, userModel.User.Username, item.PrinterName);
                                        }
                                    }
                                }
                            }
                        }
                        if (settings.ScaleAccess == true)
                        {
                            if (userModel.Scales != null)
                            {
                                if (userModel.Scales.Count > 0)
                                {
                                    db.sp_DeleteUserScales(Company, userModel.User.Username);

                                    if (userModel.Scales != null)
                                    {
                                        var SelectedCode = (from a in userModel.Scales where a.Allowed == true select a).ToList();
                                        foreach (var item in SelectedCode)
                                        {

                                            db.sp_SaveScaleAccess(Company, userModel.User.Username, item.ScaleModelId);

                                        }
                                    }
                                }
                            }
                        }
                        if (settings.DepartmentAccess == true)
                        {
                            if (userModel.Departments != null)
                            {
                                if (userModel.Departments.Count > 0)
                                {
                                    db.sp_DeleteUserDepartment(Company, userModel.User.Username);

                                    if (userModel.Departments != null)
                                    {
                                        var SelectedCode = (from a in userModel.Departments where a.Allowed == true select a).ToList();
                                        foreach (var item in SelectedCode)
                                        {

                                            db.sp_SaveDepartmentAccess(Company, userModel.User.Username, item.CostCentre);

                                        }
                                    }
                                }
                            }
                        }
                        if (settings.ReportAccess == true)
                        {
                            if (userModel.Reports != null)
                            {
                                if (userModel.Reports.Count > 0)
                                {
                                    db.sp_DeleteUserReport(Company, userModel.User.Username);

                                    if (userModel.Reports != null)
                                    {
                                        var SelectedCode = (from a in userModel.Reports where a.Allowed == true select a).ToList();
                                        foreach (var item in SelectedCode)
                                        {

                                            db.sp_SaveReportAccess(Company, userModel.User.Username, item.Program,item.Report);

                                        }
                                    }
                                }
                            }
                        }
                    }
                    ViewBag.BranchAccess = settings.BranchAccess;
                    ViewBag.GlAuthorisation = settings.GlAuthorisation;
                    ViewBag.ProductClassLimit = settings.ProductClassLimit;
                    ViewBag.Scale = settings.ScaleAccess;
                    ViewBag.PrinterAccess = settings.PrinterAccess;
                    ViewBag.WarehouseAccess = settings.WarehouseAccess;
                    ViewBag.Department = settings.DepartmentAccess;
                    ViewBag.Report = settings.ReportAccess;

                    ModelState.AddModelError("", "Saved Successfully.");
                    return View(userModel);
                    
                }
                
                ModelState.AddModelError("", "One or more fields are entered incorrectly!");
                return View(userModel);
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }

        }

        //
        // GET: /User/Edit/5
        [CustomAuthorize(Activity: "Users")]
        public ActionResult Edit(string id = null)
        {
            try
            {
                
                mtUser mtuser = db.mtUsers.Find(id);
                if (mtuser == null)
                {
                    return HttpNotFound();
                }
                
                return PartialView(mtuser);
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: /User/Edit/5
        [CustomAuthorize(Activity: "Users")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtUser mtuser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mtuser).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
            }
            return Json("One or more fields are entered incorrectly!", JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /User/Delete/5
        [CustomAuthorize(Activity: "Users")]
        public ActionResult Delete(string id = null)
        {
            mtUser mtuser = db.mtUsers.Find(id);
            if (mtuser == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtuser);
        }

        //
        // POST: /User/Delete/5
        [CustomAuthorize(Activity: "Users")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            mtUser mtuser = db.mtUsers.Find(id);
            db.mtUsers.Remove(mtuser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [CustomAuthorize(Activity: "Users")]
        public ActionResult AssignRole(string id)
        {
            ViewBag.Username = id;
            List<sp_GetUserRole_Result> UserRole = db.sp_GetUserRole(id).ToList();
            return PartialView(UserRole);
        }

        [CustomAuthorize(Activity: "Users")]
        [HttpPost]
        public ActionResult SaveRoleAccess(string details)
        {
            try
            {
                List<sp_GetUserRole_Result> myDeserializedObjList = (List<sp_GetUserRole_Result>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<sp_GetUserRole_Result>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var check = (from a in db.mtUserRoles where a.Username == item.Username.Trim() && a.Role == item.Role.Trim() select a.Username).ToList();
                        if (check.Count > 0)
                        {
                            if (item.HasAccess == "False")
                            {
                                var UserRole = new mtUserRole { Username = item.Username.Trim(), Role = item.Role.Trim() };
                                db.Entry(UserRole).State = EntityState.Deleted;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            if (item.HasAccess == "True")
                            {
                                var UserRole = new mtUserRole { Username = item.Username.Trim(), Role = item.Role.Trim() };
                                db.mtUserRoles.Add(UserRole);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        
        public ActionResult ChangePassword(string id = null)
        {
            mtUser mtuser = db.mtUsers.Find(id);
            if (mtuser == null)
            {
                return HttpNotFound();
            }
            ChangePassword ch = new ChangePassword();
            ch.Username = mtuser.Username;
            return PartialView(ch);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePassword c)
        {
            if (ModelState.IsValid)
            {
                var isValid = Membership.Provider.ChangePassword(c.Username, c.oldPassword, c.newPassword);
                if (isValid)
                {
                    return Json("Password Updated Successfully.", JsonRequestBehavior.AllowGet);
                }
            }
            ModelState.Remove("oldPassword");
            ModelState.Remove("newPassword");
            ViewBag.ErrorMessage = "Incorrect Old Password Specified.";
            return Json("Incorrect Old Password Specified.", JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Activity: "Users")]
        public ActionResult AssignCompany(string id)
        {
            ViewBag.Username = id;
            List<sp_GetUserCompanies_Result> UserRole = db.sp_GetUserCompanies(id).ToList();
            return PartialView(UserRole);
        }

        [CustomAuthorize(Activity: "Users")]
        [HttpPost]
        public ActionResult SaveCompanyAccess(string details)
        {
            try
            {
                List<sp_GetUserCompanies_Result> myDeserializedObjList = (List<sp_GetUserCompanies_Result>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<sp_GetUserCompanies_Result>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var check = (from a in db.mtUserCompanies where a.Username == item.Username.Trim() && a.Company == item.Company.Trim() select a.Username).ToList();
                        if (check.Count > 0)
                        {
                            if (item.HasAccess == "False")
                            {
                                var UserCompany = new mtUserCompany { Username = item.Username.Trim(), Company = item.Company.Trim() };
                                db.Entry(UserCompany).State = EntityState.Deleted;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            if (item.HasAccess == "True")
                            {
                                var UserCompany = new mtUserCompany { Username = item.Username.Trim(), Company = item.Company.Trim() };
                                db.mtUserCompanies.Add(UserCompany);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetRoleAccess(string Role)
        {
            try
            {
                var _Role = db.sp_GetRoleAccess(Role).ToList();
                return Json(_Role, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GlCodeList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            var result = (from a in sdb.GenMasters where a.ReportIndex1.Trim() == FilterText.Trim() && (a.AccountType == "R" || a.AccountType == "E") select new AuthorizationLimit { GlCode = a.GlCode, CostCode = a.ReportIndex2, Description = a.Description, Limit = 0 }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            sdb.Dispose();
            base.Dispose(disposing);
        }
    }
}