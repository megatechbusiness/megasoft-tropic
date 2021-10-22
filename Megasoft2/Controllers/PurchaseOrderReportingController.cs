using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class PurchaseOrderReportingController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();
        SysproEntities sdb = new SysproEntities("");
        [CustomAuthorize("PurchaseOrderReporting")]
        public ActionResult Index(Guid eGuid)
        {
            try
            {
                if (eGuid == Guid.Empty)
                {
                    string User = HttpContext.User.Identity.Name.ToUpper();
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    BuyerStatsViewModel Stats = new BuyerStatsViewModel();
                    Stats.Branch = sdb.sp_GetBuyerStatsBranchList(User, Company).ToList();
                    Stats.Buyer = sdb.sp_GetBuyerStatsBuyerList(User, Company).ToList();
                    return View(Stats);
                }
                else
                {
                    var dates = (from a in sdb.mtBuyerStatsFilters where a.Guid == eGuid select a).FirstOrDefault();
                    
                    string User = HttpContext.User.Identity.Name.ToUpper();
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    BuyerStatsViewModel Stats = new BuyerStatsViewModel();
                    Stats.Branch = sdb.sp_GetBuyerStatsBranchList(User, Company).ToList();
                    Stats.Buyer = sdb.sp_GetBuyerStatsBuyerList(User, Company).ToList();                    
                    Stats.eGuid = eGuid;
                    return View(Stats);

                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

        }

        [HttpPost]
        [CustomAuthorize("PurchaseOrderReporting")]
        public ActionResult Index(BuyerStatsViewModel buyerStatsViewModel)
        {
            try
            {
                Guid eGuid = Guid.NewGuid();

                foreach (var buy in buyerStatsViewModel.Buyer)
                {
                    if (buy.ViewStats == true)
                    {
                        mtBuyerStatsFilter objBuy = new mtBuyerStatsFilter();
                        objBuy.Guid = eGuid;
                        objBuy.Branch = "";
                        objBuy.Buyer = buy.Operator;
                        objBuy.TrnDate = DateTime.Now;
                        sdb.mtBuyerStatsFilters.Add(objBuy);
                        sdb.SaveChanges();
                    }

                }

                var stats = sdb.sp_GetBuyerStats(buyerStatsViewModel.FromDate, buyerStatsViewModel.ToDate, eGuid).ToList();
                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                BuyerStatsViewModel Stats = new BuyerStatsViewModel();
                Stats.Branch = sdb.sp_GetBuyerStatsBranchList(User, Company).ToList();
                Stats.Buyer = sdb.sp_GetBuyerStatsBuyerList(User, Company).ToList();
                Stats.BuyerStats = stats;
                Stats.eGuid = eGuid;
                //return View(Stats);
                return RedirectToAction("BuyerOutstandingPo", new { eGuid = eGuid, DetailType = "1" });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                BuyerStatsViewModel Stats = new BuyerStatsViewModel();
                Stats.Branch = sdb.sp_GetBuyerStatsBranchList(User, Company).ToList();
                Stats.Buyer = sdb.sp_GetBuyerStatsBuyerList(User, Company).ToList();
                return View(Stats);
            }
        }

        [CustomAuthorize("PurchaseOrderReporting")]
        public ActionResult BuyerOutstandingPo(Guid eGuid, string DetailType)
        {
            try
            {
                BuyerStatsViewModel Stats = new BuyerStatsViewModel();
                if (DetailType == "1")
                {
                    Stats.ReportName = "Outstanding Purchase Deliveries";
                }
                else if (DetailType == "2")
                {
                    Stats.ReportName = "No of Days past Due Date";
                }
                else
                {
                    Stats.ReportName = "No Of Days to Due Date";
                }
                var result = sdb.sp_BuyerOutstandingPo(eGuid, DetailType).ToList();
                Stats.BuyerOutstandingPo = result;
                Stats.eGuid = eGuid;
                return View(Stats);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

    }
}
