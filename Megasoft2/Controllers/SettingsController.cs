using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class SettingsController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        //
        // GET: /Settings/
        public ActionResult Index()
        {
            return View();
        }
        [CustomAuthorize(Activity: "WarehouseSettings")]
        public ActionResult WarehouseSettings()
        {
            SettingsViewModel model = new SettingsViewModel();
            model.Warehouse = wdb.mtWhseManSettings.Find(1);
            return View(model);
        }

        [CustomAuthorize(Activity: "WarehouseSettings")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "SaveWarehouseSettings")]
        public ActionResult WarehouseSettings(SettingsViewModel model)
        {
            try
            {
                ModelState.AddModelError("", "Saved Successfully");
                return View("WarehouseSettings", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("WarehouseSettings", model);
            }
            
        }

        [CustomAuthorize(Activity: "ReportAutomationSettings")]
        public ActionResult ReportAutomationSettings()
        {
           
            return View(wdb.mtReportAutomations.AsEnumerable());
        }

        [CustomAuthorize(Activity: "ReportAutomationSettings")]
        public ActionResult AddReport(string Report)
        {
            try
            {
                ViewBag.Mode = this.LoadMode();
                ViewBag.YesNo = this.LoadYesNo();
                ViewBag.Days = this.LoadDays();
                ViewBag.Dates = this.LoadDates();
                SettingsViewModel model = new SettingsViewModel();
                if (Report != null)
                {
                    
                    model.ReportAutomation = wdb.mtReportAutomations.Find(Report);
                    return View(model);
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError("", "Error Loading details:  " + ex.Message);
                return View();
            }
        }

        [CustomAuthorize(Activity: "ReportAutomationSettings")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "AddNewReport")]
        public ActionResult AddNewReport(SettingsViewModel model)
        {
            try
            {
                ViewBag.Mode = this.LoadMode();
                ViewBag.YesNo = this.LoadYesNo();
                ViewBag.Days = this.LoadDays();
                ViewBag.Dates = this.LoadDates();
               var CheckIfExists= wdb.mtReportAutomations.Find(model.ReportAutomation.Report); 
                if (CheckIfExists != null)
                {
                    wdb.Entry(CheckIfExists).CurrentValues.SetValues(model.ReportAutomation);
                    wdb.SaveChanges();
                    ModelState.AddModelError("", "Report Mailing Updated.");
                }
                else
                {

                    wdb.mtReportAutomations.Add(model.ReportAutomation);
                    wdb.SaveChanges();
                    ModelState.AddModelError("", "Report added to mailing list.");
                }
              

                return View("AddReport",model);
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error Saving data:  " + ex.Message);
                ViewBag.Mode = this.LoadMode();
                ViewBag.YesNo = this.LoadYesNo();
                ViewBag.Days = this.LoadDays();
                ViewBag.Dates = this.LoadDates();
                return View("AddReport", model);
            }
        }

        [CustomAuthorize(Activity: "ReportAutomationSettings")]
        public ActionResult DeleteReport(string Report)
        {
            try
            {
                SettingsViewModel model = new SettingsViewModel();
                if (Report != null)
                {
                    model.ReportAutomation = wdb.mtReportAutomations.Find(Report);
                    wdb.Entry(model.ReportAutomation).State = System.Data.EntityState.Deleted;
                    wdb.SaveChanges();
                    ModelState.AddModelError("", "Deleted successfully.");
                    return View("ReportAutomationSettings", wdb.mtReportAutomations.AsEnumerable());
                }
                else
                {
                    ModelState.AddModelError("", "Report not found.");
                    return View("ReportAutomationSettings", wdb.mtReportAutomations.AsEnumerable());
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error Deleting report:  " + ex.Message);
                return View("ReportAutomationSettings", wdb.mtReportAutomations.AsEnumerable());
            }
        }

        public List<SelectListItem> LoadYesNo()
        {
            List<SelectListItem> YesNo = new List<SelectListItem>
            {
                new SelectListItem{Text = "Yes", Value="Y"},
                new SelectListItem{Text = "No", Value="N"}
            };
            return YesNo;
        }

        public List<SelectListItem> LoadMode()
        {
            List<SelectListItem> mode = new List<SelectListItem>
            {
                new SelectListItem{Text = "Daily", Value="Daily"},
                new SelectListItem{Text = "Weekly", Value="Weekly"},
                new SelectListItem{Text = "Monthly", Value="Monthly"}
            };
            return mode;
        }

        public List<SelectListItem> LoadDays()
        {
            List<SelectListItem> Days = new List<SelectListItem>
            {
                new SelectListItem{Text = "Monday", Value="Monday"},
                new SelectListItem{Text = "Tuesday", Value="Tuesday"},
                new SelectListItem{Text = "Wednesday", Value="Wednesday"},
                new SelectListItem{Text = "Thursday", Value="Thursday"},
                new SelectListItem{Text = "Friday", Value="Friday"},
                new SelectListItem{Text = "Saturday", Value="Saturday"},
                new SelectListItem{Text = "Sunday", Value="Sunday"}
            };
            return Days;
        }


        public List<SelectListItem> LoadDates()
        {
            List<SelectListItem> Dates = new List<SelectListItem>
            {
                new SelectListItem{Text = "1", Value="1"},
                new SelectListItem{Text = "2", Value="2"},
                new SelectListItem{Text = "3", Value="3"},
                new SelectListItem{Text = "4", Value="4"},
                new SelectListItem{Text = "5", Value="5"},
                new SelectListItem{Text = "6", Value="6"},
                new SelectListItem{Text = "7", Value="7"},
                new SelectListItem{Text = "8", Value="8"},
                new SelectListItem{Text = "9", Value="9"},
                new SelectListItem{Text = "10", Value="10"},
                new SelectListItem{Text = "11", Value="11"},
                new SelectListItem{Text = "12", Value="12"},
                new SelectListItem{Text = "13", Value="13"},
                new SelectListItem{Text = "14", Value="14"},
                new SelectListItem{Text = "15", Value="15"},
                new SelectListItem{Text = "16", Value="16"},
                new SelectListItem{Text = "17", Value="17"},
                new SelectListItem{Text = "18", Value="18"},
                new SelectListItem{Text = "19", Value="19"},
                new SelectListItem{Text = "20", Value="20"},
                new SelectListItem{Text = "21", Value="21"},
                new SelectListItem{Text = "22", Value="22"},
                new SelectListItem{Text = "23", Value="23"},
                new SelectListItem{Text = "24", Value="24"},
                new SelectListItem{Text = "25", Value="25"},
                new SelectListItem{Text = "26", Value="26"},
                new SelectListItem{Text = "27", Value="27"},
                new SelectListItem{Text = "28", Value="28"},
                new SelectListItem{Text = "29", Value="29"},
                new SelectListItem{Text = "30", Value="30"},
                new SelectListItem{Text = "31", Value="31"},
            };
            return Dates;
        }





    }
}
