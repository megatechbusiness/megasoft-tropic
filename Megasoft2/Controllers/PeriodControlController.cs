using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class PeriodControlController : Controller
    {
        //
        // GET: /PeriodControl/
        SysproEntities sdb = new SysproEntities("");
        [CustomAuthorize(Activity: "PeriodControl")]
        public ActionResult Index()
        {
            PeriodControlViewModel model = new PeriodControlViewModel();
            var result = (from a in sdb.mtPeriodControls where a.CurrentMonth == true select a).FirstOrDefault();
            model.CurrentMonth = result.FinMonth;
            model.CurrentYear = result.FinYear;
            model.Periods = sdb.sp_GetPeriodControlSetup(result.FinYear).ToList();
            model.FinYear = result.FinYear;
            return View(model);
        }

        [CustomAuthorize(Activity: "PeriodControl")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Load")]
        [ValidateAntiForgeryToken]
        public ActionResult Load(PeriodControlViewModel model)
        {
            try
            {
                ModelState.Clear();
                PeriodControlViewModel PCon = new PeriodControlViewModel();
                var result = (from a in sdb.mtPeriodControls where a.CurrentMonth == true select a).FirstOrDefault();
                PCon.CurrentMonth = result.FinMonth;
                PCon.CurrentYear = result.FinYear;
                PCon.Periods = sdb.sp_GetPeriodControlSetup(model.FinYear).ToList();
                PCon.FinYear = model.FinYear;
                return View("Index", PCon);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        [CustomAuthorize(Activity: "PeriodControl")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "save")]
        [ValidateAntiForgeryToken]
        public ActionResult save(PeriodControlViewModel model)
        {
            try
            {
                ModelState.Clear();
                if(model != null)
                {
                    sdb.sp_DeletePeriodByYear(model.FinYear);
                    foreach(var item in model.Periods)
                    {
                        sdb.sp_SavePeriodControl(model.FinYear, item.FinMonth, item.StartDate, item.EndDate);
                    }
                    sdb.sp_SaveCurrentPeriod(model.CurrentYear, model.CurrentMonth);
                    ModelState.AddModelError("", "Saved Successfully.");
                    PeriodControlViewModel PCon = new PeriodControlViewModel();
                    var result = (from a in sdb.mtPeriodControls where a.CurrentMonth == true select a).FirstOrDefault();
                    PCon.CurrentMonth = result.FinMonth;
                    PCon.CurrentYear = result.FinYear;
                    PCon.Periods = sdb.sp_GetPeriodControlSetup(model.FinYear).ToList();
                    PCon.FinYear = model.FinYear;
                    return View("Index", PCon);
                }
                else
                {
                    ModelState.AddModelError("", "No data found.");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

    }
}
