using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManDelayedPostingController : Controller
    {
        MegasoftEntities db = new MegasoftEntities();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        [CustomAuthorize(Activity: "DelayedPosting")]
        public ActionResult Index()
        {
            WhseManDelayedPostingWarehouse model = new WhseManDelayedPostingWarehouse();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            model.Warehouse = wdb.sp_GetDelayedPostingWarehouses(Company).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(WhseManDelayedPostingWarehouse model)
        {
            try
            {
                if (model != null)
                {
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    if (model.Warehouse.Count > 0)
                    {
                        db.sp_DeleteDelayedPostingWarehouse(Company);

                        if (model != null)
                        {
                            var SelectedCode = (from a in model.Warehouse where a.Allowed == true select a).ToList();
                            foreach (var item in SelectedCode)
                            {
                                wdb.sp_SaveDelayedPostingWarehouse(Company, item.Warehouse);
                            }                            
                        }
                    }

                    ModelState.AddModelError("", "Saved!");
                    return View("Index", model);
                }
                else
                {
                    ModelState.AddModelError("", "No data found!");
                    return View("Index", model);
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }



        [CustomAuthorize(Activity: "DelayedPostingManager")]
        public ActionResult PostingManager()
        {
            try
            {
                var result = wdb.sp_GetDelayedPostingErrors().ToList();
                WhseManDelayedPostingWarehouse model = new WhseManDelayedPostingWarehouse();
                model.Errors = result;
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var unposted = (from a in wdb.sp_GetDelayedPostingData(Company).ToList() select a).ToList().Count();

                ViewBag.Unposted = unposted.ToString() + " unposted transactions";

                return View("PostingManager", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Unposted = "";
                return View("PostingManager", new WhseManDelayedPostingWarehouse());
            }
        }


        [HttpPost]
        [CustomAuthorize(Activity: "DelayedPostingManager")]
        [MultipleButton(Name = "action", Argument = "PostingManager")]
        public ActionResult PostingManager(WhseManDelayedPostingWarehouse model)
        {
            try
            {
                var result = wdb.sp_GetDelayedPostingErrors().ToList();
                model.Errors = result;

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var unposted = (from a in wdb.sp_GetDelayedPostingData(Company).ToList() select a).ToList().Count();

                ViewBag.Unposted = unposted.ToString() + " unposted transactions";

                return View("PostingManager", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Unposted = "";
                return View("PostingManager", new WhseManDelayedPostingWarehouse());
            }
        }



        [HttpPost]
        [CustomAuthorize(Activity: "DelayedPostingManager")]
        [MultipleButton(Name = "action", Argument = "RePost")]
        public ActionResult RePost(WhseManDelayedPostingWarehouse model)
        {
            try
            {
                if(model.Errors != null)
                {
                    foreach(var item in model.Errors)
                    {
                        if(item.MovementType == "ProductionReceipt")
                        {
                            using (var mfdb = new WarehouseManagementEntities(""))
                            {
                                var _tbl = (from a in mfdb.mtProductionLabels where a.Job == item.StockCode && a.BatchId == item.Lot select a).FirstOrDefault();
                                _tbl.LabelReceipted = "D";
                                _tbl.ErrorMessage = "";
                                mfdb.Entry(_tbl).State = System.Data.EntityState.Modified;
                                mfdb.SaveChanges();
                            }
                        }
                        else
                        {
                            int Id = item.TrnId;
                            using (var udb = new WarehouseManagementEntities(""))
                            {
                                var result = (from a in udb.mtInvDelayedPostings where a.TrnId == Id select a).FirstOrDefault();
                                result.Status = 1;
                                udb.Entry(result).State = System.Data.EntityState.Modified;
                                udb.SaveChanges();
                            }
                        }
                        
                    }

                    ModelState.AddModelError("", "Items Queued for Posting.");
                }

                var outresult = wdb.sp_GetDelayedPostingErrors().ToList();
                model.Errors = outresult;

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var unposted = (from a in wdb.sp_GetDelayedPostingData(Company).ToList() select a).ToList().Count();
                ViewBag.Unposted = unposted.ToString() + " unposted transactions";
                return View("PostingManager", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Unposted = "";
                return View("PostingManager", model);
            }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "DelayedPostingManager")]
        [MultipleButton(Name = "action", Argument = "DeleteItem")]
        public ActionResult DeleteItem(WhseManDelayedPostingWarehouse model)
        {
            try
            {
                ModelState.Clear();
                if (model.Errors != null)
                {
                    foreach (var item in model.Errors)
                    {
                        if(item.DeleteItem == true)
                        {
                            if (item.MovementType == "ProductionReceipt")
                            {
                                using (var mfdb = new WarehouseManagementEntities(""))
                                {
                                    var _tbl = (from a in mfdb.mtProductionLabels where a.Job == item.StockCode && a.BatchId == item.Lot select a).FirstOrDefault();
                                    _tbl.LabelReceipted = "R";
                                    _tbl.ErrorMessage = "Item removed from delayed posting.";
                                    mfdb.Entry(_tbl).State = System.Data.EntityState.Modified;
                                    mfdb.SaveChanges();
                                }
                            }
                            else
                            {
                                int Id = item.TrnId;
                                using (var udb = new WarehouseManagementEntities(""))
                                {
                                    var result = (from a in udb.mtInvDelayedPostings where a.TrnId == Id select a).FirstOrDefault();
                                    udb.Entry(result).State = System.Data.EntityState.Deleted;
                                    udb.SaveChanges();
                                }
                            }
                        }                       
                    }

                    ModelState.AddModelError("", "Items Deleted.");
                }

                var outresult = wdb.sp_GetDelayedPostingErrors().ToList();
                model.Errors = outresult;

                var unposted = (from a in wdb.mtInvDelayedPostings where a.Status == 1 select a).ToList().Count();
                ViewBag.Unposted = unposted.ToString() + " unposted transactions";
                return View("PostingManager", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Unposted = "";
                return View("PostingManager", model);
            }
        }

        [CustomAuthorize(Activity: "DelayedPostingCue")]
        public ActionResult DelayedPostingCue()
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var result = wdb.sp_GetDelayedPostingCueData(Company).ToList();
                WhseManDelayedPostingWarehouse model = new WhseManDelayedPostingWarehouse();
                model.Cue = result;

                return View("DelayedPostingCue", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Unposted = "";
                return View("DelayedPostingCue", new WhseManDelayedPostingWarehouse());
            }
        }

    }
}
