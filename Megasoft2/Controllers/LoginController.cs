using Megasoft2.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Megasoft2.Controllers
{
    public class LoginController : Controller
    {
        private MegasoftEntities db = new MegasoftEntities();

        public ActionResult Index()
        {
            Login l = new Login();
            //l.ResetPassword = "N";
            ViewBag.ForceReset = false;
            ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
            ViewBag.SmartScan = (from a in db.mtSystemSettings where a.Id == 1 select a.SmartScan).FirstOrDefault();
            return View(l);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Login l, string ReturnUrl = "")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(l.SmartId))
                    {
                        var isValidUser = Membership.ValidateUser(l.Username, l.Password);
                        if (isValidUser)
                        {
                            //if (Url.IsLocalUrl(ReturnUrl))
                            //{
                            //    return Redirect(ReturnUrl);
                            //}
                            //else
                            //{
                            if (!string.IsNullOrEmpty(l.NewPassword))
                            {
                                if (string.IsNullOrEmpty(l.NewPassword))
                                {
                                    l.ResetPassword = "Y";
                                    ViewBag.ForceReset = true;
                                    ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                                    ModelState.Remove("Password");
                                    ViewBag.ErrorMessage = "New Password cannot be blank";
                                    return View(l);
                                }

                                if (string.IsNullOrEmpty(l.ConfirmPassword))
                                {
                                    l.ResetPassword = "Y";
                                    ViewBag.ForceReset = true;
                                    ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                                    ModelState.Remove("Password");
                                    ViewBag.ErrorMessage = "Confirm Password cannot be blank";
                                    return View(l);
                                }

                                if (l.NewPassword != l.ConfirmPassword)
                                {
                                    l.ResetPassword = "Y";
                                    ViewBag.ForceReset = true;
                                    ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                                    ModelState.Remove("Password");
                                    ViewBag.ErrorMessage = "New Password does not match confirm password";
                                    return View(l);
                                }

                                using (var edb = new MegasoftEntities())
                                {
                                    var result = (from a in edb.mtUsers where a.Username == l.Username.ToUpper() select a).FirstOrDefault();
                                    result.Password = l.ConfirmPassword;
                                    result.ForcePasswordReset = false;
                                    edb.SaveChanges();
                                }
                            }

                            var DatabaseName = (from a in db.mtSysproAdmins
                                                join b in db.mtUserCompanies on a.Company equals b.Company
                                                where b.Username.Equals(l.Username.ToUpper())
                                                && b.Company.Equals(l.Company)
                                                select a.DatabaseName).FirstOrDefault();
                            if (string.IsNullOrEmpty(DatabaseName))
                            {
                                l.ResetPassword = "N";
                                ViewBag.ForceReset = false;
                                ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                                ModelState.Remove("Password");
                                ViewBag.ErrorMessage = "Access denied to Company " + l.Company;
                                return View();
                            }

                            var ForceReset = (from a in db.mtUsers where a.Username == l.Username.ToUpper() select a.ForcePasswordReset).FirstOrDefault();

                            if (ForceReset == true)
                            {
                                l.ResetPassword = "Y";
                                ViewBag.ForceReset = ForceReset;
                                ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                                ModelState.Remove("Password");
                                ViewBag.ErrorMessage = "Please change your password.";
                                return View(l);
                            }

                            FormsAuthentication.SetAuthCookie(l.Username, true);
                            HttpCookie SysproDatabase = new HttpCookie("SysproDatabase");
                            SysproDatabase.Value = DatabaseName;
                            Response.Cookies.Add(SysproDatabase);
                            var Settings = (from a in db.mtSystemSettings where a.Id == 1 select a).FirstOrDefault();

                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {

                                if (Settings.Dashboard == true)
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                                else
                                {
                                    return RedirectToAction("Home", "Home");
                                }
                            }
                            //}
                        }
                    }
                    else
                    {
                        var smartScan = db.sp_GetSmartScanGuidDetail(l.SmartId).ToList();
                        if (smartScan.Count > 0)
                        {
                            string Username = smartScan.FirstOrDefault().Username;
                            string company = smartScan.FirstOrDefault().Company;
                            string password = smartScan.FirstOrDefault().Password;
                            var isValidUser = Membership.ValidateUser(Username, password);
                            if (isValidUser)
                            {
                                var DatabaseName = (from a in db.mtSysproAdmins
                                                    join b in db.mtUserCompanies on a.Company equals b.Company
                                                    where b.Username.Equals(Username)
                                                    && b.Company.Equals(company)
                                                    select a.DatabaseName).FirstOrDefault();
                                FormsAuthentication.SetAuthCookie(Username, true);
                                HttpCookie SysproDatabase = new HttpCookie("SysproDatabase");
                                SysproDatabase.Value = DatabaseName;
                                Response.Cookies.Add(SysproDatabase);
                                return RedirectToAction(smartScan.FirstOrDefault().ActionUrl, smartScan.FirstOrDefault().Controller, new { SmartId = l.SmartId });
                            }
                        }
                        else
                        {
                            l.ResetPassword = "N";
                            ViewBag.ForceReset = false;
                            ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                            ModelState.Remove("Password");
                            ViewBag.ErrorMessage = "Invalid Smart Scan!";
                            return View();
                        }
                    }
                }
                l.ResetPassword = "N";
                ViewBag.ForceReset = false;
                ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                ModelState.Remove("Password");
                ViewBag.ErrorMessage = "Incorrect Username or Password Specified.";
                return View();
            }
            catch (Exception ex)
            {
                l.ResetPassword = "N";
                ViewBag.ForceReset = false;
                ViewBag.Company = (from a in db.mtSysproAdmins select new { Company = a.Company, DatabaseName = a.DatabaseName }).ToList();
                ModelState.Remove("Password");
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        //public ActionResult GetUserDetails(string SmartId)
        //{
        //    try
        //    {
        //    }
        //    catch(Exception ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}