using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.ViewModel;

namespace Megasoft2.Controllers
{
    public class EmailSettingsController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();

        public ActionResult Index()
        {
            EmailSettingsViewModel model = new EmailSettingsViewModel();
            model.EmailSettingsList = (from a in mdb.mtEmailSettings select a).ToList();
            return View(model);
        }

        public ActionResult Create(string EmailProgram)
        {
            EmailSettingsViewModel model = new EmailSettingsViewModel();
            try
            {
                
                if (!string.IsNullOrEmpty(EmailProgram))
                {
                    var check = (from a in mdb.mtEmailSettings where a.EmailProgram == EmailProgram select a).FirstOrDefault();
                    model.EmailProgram = check.EmailProgram;
                    model.SmtpHost = check.SmtpHost;
                    model.SmtpPort = check.SmtpPort;
                    model.FromAddress = check.FromAddress;
                    model.FromAddressPassword = check.FromAddressPassword;
                    model.EmailEnableSsl = check.EmailEnableSsl;
                    return View("Create",model);
                }
                return View("Create", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create", model);
            }
            
        }

        [HttpPost]
        public ActionResult Create(EmailSettingsViewModel model)
        {
            try
            {
                var check = (from a in mdb.mtEmailSettings where a.EmailProgram == model.EmailProgram select a).FirstOrDefault();
                if (check == null)
                {
                    mtEmailSetting obj = new mtEmailSetting();
                    obj.EmailProgram = model.EmailProgram;
                    obj.SmtpHost=model.SmtpHost;
                    obj.SmtpPort=model.SmtpPort;
                    obj.FromAddress = model.FromAddress;
                    obj.FromAddressPassword = model.FromAddressPassword;
                    obj.EmailEnableSsl = model.EmailEnableSsl;
                    mdb.Entry(obj).State = System.Data.EntityState.Added;
                    mdb.SaveChanges();
                }
                else
                {
                    check.SmtpHost = model.SmtpHost;
                    check.SmtpPort = model.SmtpPort;
                    check.FromAddress = model.FromAddress;
                    check.FromAddressPassword = model.FromAddressPassword;
                    check.EmailEnableSsl = model.EmailEnableSsl;
                    mdb.Entry(check).State = System.Data.EntityState.Modified;
                    mdb.SaveChanges();
                }
                ModelState.AddModelError("", "Email program saved.");
                return View("Create", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Create", model);
            }
        }

        public ActionResult Delete(string EmailSettings)
        {
            try
            {
                EmailSettingsViewModel model = new EmailSettingsViewModel();
                var check = (from a in mdb.mtEmailSettings where a.EmailProgram == EmailSettings select a).FirstOrDefault();
                if (check != null)
                {
                    mdb.Entry(check).State = System.Data.EntityState.Deleted;
                    mdb.SaveChanges();
                    ModelState.AddModelError("", "Deleted successfully.");
                    model.EmailSettingsList = (from a in mdb.mtEmailSettings select a).ToList();
                }
                else
                {
                    ModelState.AddModelError("", "Email program does not exist");
                }
                return View("Index", model);
            }
            catch (Exception ex)
            {
                EmailSettingsViewModel model = new EmailSettingsViewModel();
                model.EmailSettingsList = (from a in mdb.mtEmailSettings select a).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

    }
}
