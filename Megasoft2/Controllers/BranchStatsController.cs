using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class BranchStatsController : Controller
    {
        //
        // GET: /BranchStats/
        MegasoftEntities mdb = new MegasoftEntities();
        SysproEntities sdb = new SysproEntities("");
        [CustomAuthorize(Activity: "BuyerStats")]
        public ActionResult Index(Guid eGuid)
        {
            try
            {
                if(eGuid == Guid.Empty)
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
                    var stats = sdb.sp_GetBuyerStats(dates.FromDate, dates.ToDate, eGuid).ToList();
                    string User = HttpContext.User.Identity.Name.ToUpper();
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    BuyerStatsViewModel Stats = new BuyerStatsViewModel();
                    Stats.Branch = sdb.sp_GetBuyerStatsBranchList(User, Company).ToList();
                    Stats.Buyer = sdb.sp_GetBuyerStatsBuyerList(User, Company).ToList();
                    Stats.BuyerStats = stats;
                    Stats.eGuid = eGuid;
                    Stats.FromDate = dates.FromDate;
                    Stats.ToDate = dates.ToDate;
                    return View(Stats);
                }
                
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            
        }

        [HttpPost]
        public ActionResult Index(BuyerStatsViewModel buyerStatsViewModel)
        {
            try
            {
                Guid eGuid = Guid.NewGuid();
                
                foreach (var buy in buyerStatsViewModel.Buyer)
                {
                    if(buy.ViewStats == true)
                    {
                        mtBuyerStatsFilter objBuy = new mtBuyerStatsFilter();
                        objBuy.Guid = eGuid;
                        objBuy.FromDate = buyerStatsViewModel.FromDate;
                        objBuy.ToDate = buyerStatsViewModel.ToDate;
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
                return View(Stats);
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


        public ActionResult Detail(Guid eGuid, string DetailType)
        {
            try
            {
                var result = sdb.sp_GetBuyerStatsDetail(eGuid, DetailType).ToList();
                return View(result);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        
        public ActionResult BuyerOutstandingPo(Guid eGuid, string DetailType)
        {
            try
            {
                BuyerStatsViewModel Stats = new BuyerStatsViewModel();

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

        public ActionResult BuyerTurnaroundTimes(Guid eGuid)
        {
            try
            {
                BuyerStatsViewModel Stats = new BuyerStatsViewModel();
                var result = sdb.sp_BuyerTurnaroundTimes(eGuid).ToList();
                Stats.TurnaroundTimes = result;
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
