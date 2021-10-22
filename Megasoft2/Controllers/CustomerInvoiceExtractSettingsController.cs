using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class CustomerInvoiceExtractSettingsController : Controller
    {
        private WarehouseManagementEntities db = new WarehouseManagementEntities("");

        

        //
        // GET: /CustomerInvoiceExtractSettings/Create
        [CustomAuthorize("CustomerInvoiceExtractSettings")]
        public ActionResult Create()
        {
            var result = db.sp_GetInvoiceExtractCustomerClass().ToList();
            ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
            ViewBag.Mode = this.LoadMode();
            ViewBag.YesNo = this.LoadYesNo();
            ViewBag.Days = this.LoadDays();
            ViewBag.Dates = this.LoadDates();
            return View();
        }

        //
        // POST: /CustomerInvoiceExtractSettings/Create

        

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadClass")]
        public ActionResult LoadClass(mtInvoiceExtractSetting model)
        {
            try
            {
                ModelState.Clear();
                mtInvoiceExtractSetting setting = (from a in db.mtInvoiceExtractSettings where a.CustomerClass == model.CustomerClass select a).FirstOrDefault();
                
                var result = db.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                ViewBag.Mode = this.LoadMode();
                ViewBag.YesNo = this.LoadYesNo();
                ViewBag.Days = this.LoadDays();
                ViewBag.Dates = this.LoadDates();                
                return View("Create", setting);

            }
            catch(Exception ex)
            {
                var result = db.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                ViewBag.Mode = this.LoadMode();
                ViewBag.YesNo = this.LoadYesNo();
                ViewBag.Days = this.LoadDays();
                ViewBag.Dates = this.LoadDates();
                ModelState.AddModelError("", ex.Message);
                return View("Create", model);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveSetting")]
        public ActionResult SaveSetting(mtInvoiceExtractSetting model)
        {
            try
            {
                ModelState.Clear();
                bool doUpdate = false;
                using(var wdb = new WarehouseManagementEntities(""))
                {
                    var setting = (from a in wdb.mtInvoiceExtractSettings where a.CustomerClass == model.CustomerClass select a).FirstOrDefault();
                    if(setting != null)
                    {
                        //Update
                        doUpdate = true;
                    }
                    else
                    {
                        doUpdate = false;
                    }
                }

                if(doUpdate == true)
                {
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(model).State = EntityState.Added;
                    db.SaveChanges();
                }
                ModelState.AddModelError("", "Saved Successfully.");
                //var settingout = (from a in db.mtInvoiceExtractSettings where a.CustomerClass == model.CustomerClass select a).FirstOrDefault();
                var result = db.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                ViewBag.Mode = this.LoadMode();
                ViewBag.YesNo = this.LoadYesNo();
                ViewBag.Days = this.LoadDays();
                ViewBag.Dates = this.LoadDates();
                return View("Create", model);

            }
            catch (Exception ex)
            {
                var result = db.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                ViewBag.Mode = this.LoadMode();
                ViewBag.YesNo = this.LoadYesNo();
                ViewBag.Days = this.LoadDays();
                ViewBag.Dates = this.LoadDates();
                ModelState.AddModelError("", ex.Message);
                return View("Create", model);
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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}