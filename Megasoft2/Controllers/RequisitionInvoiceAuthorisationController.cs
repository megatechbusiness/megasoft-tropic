using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionInvoiceAuthorisationController : Controller
    {

        SysproEntities sdb = new SysproEntities("");
        RequisitionBL BL = new RequisitionBL();
        //
        // GET: /RequisitionInvoiceAuthorisation/

        [CustomAuthorize("InvoiceReview", "InvoiceReviewViewOnly")]
        public ActionResult Index()
        {
            int AuthLevel = BL.GetInvoiceAuthorisationLevel();
            var Invoices = sdb.sp_GetInvoicesForReview(AuthLevel).ToList();
            InvoiceAuthViewModel objModel = new InvoiceAuthViewModel();
            objModel.Invoices = Invoices;
            ViewBag.InvoiceReview = BL.ProgramAccess("InvoiceReview");
            return View(objModel);
        }


        [CustomAuthorize(Activity: "InvoiceReview")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Index")]
        public ActionResult Index(InvoiceAuthViewModel model)
        {
            try
            {
                ModelState.Clear();
                if(model.Invoices.Count > 0)
                {
                    int AuthLevel = BL.GetInvoiceAuthorisationLevel();
                    foreach(var item in model.Invoices)
                    {
                        sdb.sp_UpdateInvoiceAuthorisation(AuthLevel, item.Grn, HttpContext.User.Identity.Name.ToUpper(), item.GrnAuthorized);
                        //var result = (from a in sdb.mtGrnDetails where a.Grn == item.Grn select a).ToList();
                        //foreach(var re in result)
                        //{
                        //    if(AuthLevel < 3)
                        //    {
                        //        if (re.AuthorizedLevel1 == true && item.GrnAuthorized == false || re.AuthorizedLevel1 == false && item.GrnAuthorized == false)
                        //        {
                        //            re.AuthorizedLevel1 = item.GrnAuthorized;
                        //            re.Level1AuthorizedBy = null;
                        //        }
                        //        else
                        //        {
                        //            re.AuthorizedLevel1 = item.GrnAuthorized;
                        //            re.Level1AuthorizedBy = HttpContext.User.Identity.Name.ToUpper();
                        //        }
                                
                        //    }
                        //    else
                        //    {
                        //        if (re.AuthorizedLevel2 == true && item.GrnAuthorized == false || re.AuthorizedLevel2 == false && item.GrnAuthorized == false)
                        //        {
                        //            re.AuthorizedLevel2 = item.GrnAuthorized;
                        //            re.Level2AuthorizedBy = null;
                        //        }
                        //        else
                        //        {
                        //            re.AuthorizedLevel2 = item.GrnAuthorized;
                        //            re.Level2AuthorizedBy = HttpContext.User.Identity.Name.ToUpper();
                        //        }
                        //    }
                        //    sdb.Entry(re).State = System.Data.EntityState.Modified;
                        //    sdb.SaveChanges();
                        //}
                    }
                }
                ModelState.AddModelError("", "Saved Successfully.");
                int newAuthLevel = BL.GetInvoiceAuthorisationLevel();
                var Invoices = sdb.sp_GetInvoicesForReview(newAuthLevel).ToList();
                InvoiceAuthViewModel objModel = new InvoiceAuthViewModel();
                objModel.Invoices = Invoices;
                ViewBag.InvoiceReview = BL.ProgramAccess("InvoiceReview");
                return View(objModel);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [ValidateInput(false)] 
        [CustomAuthorize(Activity: "InvoiceReview")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostRows")]
        public ActionResult PostRows(InvoiceAuthViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (model.Invoices.Count > 0)
                {
                    //int AuthLevel = BL.GetInvoiceAuthorisationLevel();
                    //foreach (var item in model.Invoices)
                    //{
                    //    List<mtGrnDetail> result;
                    //    if(AuthLevel < 3)
                    //    {
                    //        result = (from a in sdb.mtGrnDetails where a.Grn == item.Grn && a.AuthorizedLevel1 == true select a).ToList();
                    //    }
                    //    else
                    //    {
                    //        result = (from a in sdb.mtGrnDetails where a.Grn == item.Grn && a.AuthorizedLevel2 == true select a).ToList();
                    //    }
                    //    foreach (var re in result)
                    //    {
                    //        if(item.GrnAuthorized == true)
                    //        {
                    //            re.PostStatus = 1;
                    //        }
                    //        sdb.Entry(re).State = System.Data.EntityState.Modified;
                    //        sdb.SaveChanges();
                    //    }
                    //}

                    sdb.sp_UpdateGrnForPosting();


                }
                ModelState.AddModelError("", "Authorised items queued for posting.");
                int newAuthLevel = BL.GetInvoiceAuthorisationLevel();
                var Invoices = sdb.sp_GetInvoicesForReview(newAuthLevel).ToList();
                InvoiceAuthViewModel objModel = new InvoiceAuthViewModel();
                objModel.Invoices = Invoices;
                ViewBag.InvoiceReview = BL.ProgramAccess("InvoiceReview");
                return View("Index", objModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [CustomAuthorize(Activity: "InvoiceReview")]
        public ActionResult ReviewInvoice(string Grn)
        {
            try
            {
                InvoiceMatching Inv = new InvoiceMatching();
                var GrnLines = sdb.sp_GetGrnLinesForInvoicing(Grn).ToList();
                Inv.PoLines = GrnLines;
                Inv.PurchaseOrder = GrnLines.FirstOrDefault().PurchaseOrder;
                Inv.Grn = Grn;
                Inv.DeliveryNote = GrnLines.FirstOrDefault().DeliveryNote;
                Inv.DeliveryNoteDate = GrnLines.FirstOrDefault().DeliveryNoteDate;
                Inv.Requisition = GrnLines.FirstOrDefault().Requisition;
                Inv.Branch = GrnLines.FirstOrDefault().Branch;
                Inv.Site = GrnLines.FirstOrDefault().Site;
                Inv.PoAttached = GrnLines.FirstOrDefault().PoAttached;
                Inv.GrnAttached = GrnLines.FirstOrDefault().GrnAttached;
                Inv.PodAttached = GrnLines.FirstOrDefault().PodAttached;
                Inv.CastAndExtensions = GrnLines.FirstOrDefault().CastAndExtensions;
                Inv.GlCodeChecked = GrnLines.FirstOrDefault().GlCodeChecked;
                Inv.SupplierCodeChecked = GrnLines.FirstOrDefault().SupplierCodeChecked;
                var Totals = sdb.sp_GetGrnTotals(Grn).ToList();
                Inv.SubTotal = (decimal)Totals.FirstOrDefault().SubTotal;
                Inv.Vat = (decimal)Totals.FirstOrDefault().Vat;
                Inv.Total = (decimal)Totals.FirstOrDefault().Total;
                Inv.Invoice = GrnLines.FirstOrDefault().Invoice;
                Inv.InvoiceAmount = (decimal)GrnLines.FirstOrDefault().InvoiceAmount;
                Inv.InvoiceDate = GrnLines.FirstOrDefault().InvoiceDate;
                ViewBag.IsAdmin = BL.isAdmin();
                ViewBag.InvoiceReview = BL.ProgramAccess("InvoiceReview");
                return View(Inv);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }


        public ActionResult InvoiceList()
        {
            ViewBag.InvoiceReview = BL.ProgramAccess("InvoiceReview");
            return View();
        }


        public JsonResult InvoiceAuthList()
        {
            try
            {
                int AuthLevel = BL.GetInvoiceAuthorisationLevel();
                var Invoices = sdb.sp_GetInvoicesForReview(AuthLevel).ToList();

                return Json(Invoices, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [CustomAuthorize(Activity: "InvoiceReview")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "InvoiceList")]
        public ActionResult InvoiceList(InvoiceAuthViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (model.Invoices.Count > 0)
                {
                    int AuthLevel = BL.GetInvoiceAuthorisationLevel();
                    foreach (var item in model.Invoices)
                    {
                        sdb.sp_UpdateInvoiceAuthorisation(AuthLevel, item.Grn, HttpContext.User.Identity.Name.ToUpper(), item.GrnAuthorized);
                        
                    }
                }
                ModelState.AddModelError("", "Saved Successfully.");
                int newAuthLevel = BL.GetInvoiceAuthorisationLevel();
                var Invoices = sdb.sp_GetInvoicesForReview(newAuthLevel).ToList();
                InvoiceAuthViewModel objModel = new InvoiceAuthViewModel();
                objModel.Invoices = Invoices;
                ViewBag.InvoiceReview = BL.ProgramAccess("InvoiceReview");
                return View("InvoiceList", objModel);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public ActionResult InvoiceTotals()
        {
            var result = sdb.sp_GetInvoiceReviewTotals().ToList();
            return PartialView(result);
        }
    }
}
