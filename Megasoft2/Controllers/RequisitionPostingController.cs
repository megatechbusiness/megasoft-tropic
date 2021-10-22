using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionPostingController : Controller
    {
        //
        // GET: /RequisitionPosting/
        SysproEntities sdb = new SysproEntities("");

        [CustomAuthorize(Activity: "PostingErrors")]
        public ActionResult Index()
        {
            try
            {
                PostingViewModel PO = new PostingViewModel();
                var errorsOnly = sdb.sp_GetPostingErrors(Convert.ToDateTime("2017-04-01"), Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")), true).ToList();
                PO.PostErrors = errorsOnly;// sdb.sp_GetPostingErrors(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")), Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"))).ToList();
                PO.FromDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                PO.ToDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                ViewBag.Unposted = sdb.sp_GetUnpostedTransactionsCount().FirstOrDefault().TransactionsPending;
                return View(PO);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

        }
        [HttpPost]
        [CustomAuthorize(Activity: "PostingErrors")]
        [MultipleButton(Name = "action", Argument = "Index")]
        public ActionResult Index(PostingViewModel PO)
        {
            try
            {
                if (PO.ToDate < PO.FromDate)
                {
                    ModelState.AddModelError("", "To Date cannot be before From Date.");
                    PO.PostErrors.Clear();
                    return View("Index", PO);
                }
                PO.PostErrors = sdb.sp_GetPostingErrors(Convert.ToDateTime(PO.FromDate.ToString("yyyy-MM-dd")), Convert.ToDateTime(PO.ToDate.ToString("yyyy-MM-dd")), false).ToList();
                PO.FromDate = Convert.ToDateTime(PO.FromDate.ToString("yyyy-MM-dd"));
                PO.ToDate = Convert.ToDateTime(PO.ToDate.ToString("yyyy-MM-dd"));
                ViewBag.Unposted = sdb.sp_GetUnpostedTransactionsCount().FirstOrDefault().TransactionsPending;
                return View("Index",PO);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }

        }


        [HttpPost]
        [CustomAuthorize(Activity: "PostingErrors")]
        [MultipleButton(Name = "action", Argument = "PostRows")]
        public ActionResult PostRows(PostingViewModel PO)
        {
            try
            {
                ModelState.Clear();
                foreach(var line in PO.PostErrors)
                {
                    if(line.RePost == true)
                    {
                        using(var edb = new SysproEntities(""))
                        {
                            var result = (from a in sdb.mtGrnDetails where a.Grn == line.Grn select a).ToList();
                            foreach(var re in result)
                            {
                                if (re.PostStatus == 3)
                                {
                                    //update to 1
                                    re.PostStatus = 1;
                                    re.GrnError = "";
                                }
                                else if (re.PostStatus == 5)
                                {
                                    //update to 2
                                    re.PostStatus = 2;
                                    re.MaterialAllocationError = "";
                                }
                                else if (re.PostStatus == 7)
                                {
                                    //update to 4
                                    re.PostStatus = 4;
                                    re.MaterialIssueError = "";
                                }
                                else if (re.PostStatus == 9)
                                {
                                    //update to 6
                                    re.PostStatus = 6;
                                    re.InvoiceError = "";
                                }
                                else if (re.PostStatus == 11)
                                {
                                    //update to 8
                                    re.PostStatus = 8;
                                    re.GrnAdjustmentError = "";
                                }
                                edb.Entry(re).State = System.Data.EntityState.Modified;
                                edb.SaveChanges();
                            }
                            

                            
                        }
                        
                    }
                }

                ModelState.AddModelError("", "Selected items queued for posting.");
                PO.PostErrors = sdb.sp_GetPostingErrors(Convert.ToDateTime(PO.FromDate.ToString("yyyy-MM-dd")), Convert.ToDateTime(PO.ToDate.ToString("yyyy-MM-dd")), false).ToList();
                PO.FromDate = Convert.ToDateTime(PO.FromDate.ToString("yyyy-MM-dd"));
                PO.ToDate = Convert.ToDateTime(PO.ToDate.ToString("yyyy-MM-dd"));
                return View("Index", PO);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index");
            }

        }

    }
}
