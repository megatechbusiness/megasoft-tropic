using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManPoReceiptController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        LabelPrint objPrint = new LabelPrint();
        //
        // GET: /WhseManPoReceipt/

        [CustomAuthorize(Activity: "PoReceipts")]
        public ActionResult Index(string PurchaseOrder = null)
        {
            ModelState.Clear();
            if (PurchaseOrder != null)
            {
                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                WhseManPoReceipt model = new WhseManPoReceipt();
                string Po = PurchaseOrder.PadLeft(15, '0');
                var result = wdb.sp_GetPoLabelLines(Po, User, Company).ToList();
                if (result.Count == 0)
                {
                    ModelState.AddModelError("", "No unposted items found for Purchase Order : " + model.PurchaseOrder);
                }
                model.PurchaseOrder = Po;
                model.ReelLines = result;
                model.DeliveryDate = DateTime.Now;
                return View("Index", model);
            }
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Index")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PoReceipts")]
        public ActionResult Index(WhseManPoReceipt model)
        {
            try
            {
                ModelState.Clear();

                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                string Po = model.PurchaseOrder.PadLeft(15, '0');
                var result = wdb.sp_GetPoLabelLines(Po, User, Company).ToList();
                if (result.Count == 0)
                {
                    ModelState.AddModelError("", "No unposted items found for Purchase Order : " + model.PurchaseOrder);
                    return View("Index", model);
                }
                model.ReelLines = result;
                model.DeliveryDate = DateTime.Now;
                model.DeliveryNote = result.FirstOrDefault().DeliveryNote;
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PoReceipt")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PoReceipts")]
        public ActionResult PoReceipt(WhseManPoReceipt model)
        {
            try
            {
                ModelState.Clear();

                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                if (model.ReelLines == null)
                {
                    ModelState.AddModelError("", "No unposted items found for Purchase Order : " + model.PurchaseOrder);
                    return View("Index", new WhseManPoReceipt());
                }
                var LinesToPost = (from a in model.ReelLines where a.PostFlag == true select a).ToList();
                if (LinesToPost.Count == 0)
                {
                    ModelState.AddModelError("", "No lines selected to post. Delivery Date :" + model.DeliveryDate);
                    string Po = model.PurchaseOrder.PadLeft(15, '0');
                    var result = wdb.sp_GetPoLabelLines(Po, User, Company).ToList();
                    model.ReelLines = result;
                    return View("Index", model);
                }

                if (string.IsNullOrEmpty(model.DeliveryNote))
                {
                    ModelState.AddModelError("", "Delivery Note required.");
                    string Po = model.PurchaseOrder.PadLeft(15, '0');
                    var result = wdb.sp_GetPoLabelLines(Po, User, Company).ToList();
                    model.ReelLines = result;
                    return View("Index", model);
                }

                if (model.DeliveryDate == null)
                {
                    ModelState.AddModelError("", "Delivery Date required.");
                    string Po = model.PurchaseOrder.PadLeft(15, '0');
                    var result = wdb.sp_GetPoLabelLines(Po, User, Company).ToList();
                    model.ReelLines = result;
                    return View("Index", model);
                }

                //var ReelLines = new List<sp_GetPoLabelLines_Result>();

                bool ExpenseLinesValid = true;

                foreach (var item in model.ReelLines)
                {
                    var DirectExpese = wdb.mt_DirectExpenseByStockCode(item.StockCode).FirstOrDefault();
                    if (DirectExpese != null)
                    {
                        if (DirectExpese.DirectExpenseIssue == "Y")
                        {
                            var TraceableType = (from a in wdb.InvMasters where a.StockCode.Equals(item.StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod, ProductClass = a.ProductClass }).FirstOrDefault();
                            var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == "DIREXP" && a.ProductClass == TraceableType.ProductClass select a.GlCode).FirstOrDefault();


                            if (string.IsNullOrWhiteSpace(GlCode))
                            {
                                ModelState.AddModelError("", "No GL code found in Matrix for Cost Centre: DIREXP Product Cass: " + TraceableType.ProductClass);
                                ExpenseLinesValid = false;
                            }
                        }
                    }

                }

                if (ExpenseLinesValid == false)
                {
                    string Po = model.PurchaseOrder.PadLeft(15, '0');
                    var result = wdb.sp_GetPoLabelLines(Po, User, Company).ToList();
                    model.ReelLines = result;
                    return View("Index", model);
                }

                //var LineCount = (from a in model.ReelLines where a.PostFlag == true select a.Line).Distinct().ToList();
                //if(LineCount.Count > 1)
                //{
                //    ModelState.AddModelError("", "Only 1 Purchase Order Line can be receipted at a time.");
                //    string Po = model.PurchaseOrder.PadLeft(15, '0');
                //    var result = wdb.sp_GetPoLabelLines(Po, User, Company).ToList();
                //    model.ReelLines = result;
                //    return View("Index", model);
                //}



                string DirectExpenseIssue = "";
                string PoReceipt = objPrint.PostPoReceipt(model.ReelLines, model.DeliveryDate, model.DeliveryNote);
                if (PoReceipt.Contains("Posted Successfully. Grn :"))
                {
                    DirectExpenseIssue = objPrint.PostSysproExpenseIssue(model.ReelLines);
                }
                //string DirectExpenseIssue = objPrint.PostSysproExpenseIssue(model.ReelLines);
                ModelState.AddModelError("", PoReceipt + "\n " + DirectExpenseIssue);

                string nPo = model.PurchaseOrder.PadLeft(15, '0');
                var nresult = wdb.sp_GetPoLabelLines(nPo, User, Company).ToList();
                model.ReelLines = nresult;
                return View("Index", model);


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        [CustomAuthorize(Activity: "PoReceipts")]
        public ActionResult DeleteReel(string PurchaseOrder, decimal Line, string ReelNo)
        {
            try
            {
                string username = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                string Po = PurchaseOrder.PadLeft(15, '0');
                wdb.sp_DeletePurchaseOrderReel(Po, (int)Line, ReelNo, username);
                ModelState.AddModelError("", "Deleted Successfully.");
                WhseManPoReceipt model = new WhseManPoReceipt();
                var result = wdb.sp_GetPoLabelLines(Po, username, Company).ToList();
                model.ReelLines = result;
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);


                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                WhseManPoReceipt model = new WhseManPoReceipt();
                string Po = PurchaseOrder.PadLeft(15, '0');
                var result = wdb.sp_GetPoLabelLines(Po, User, Company).ToList();
                model.ReelLines = result;
                return View("Index", model);
            }
        }

        [CustomAuthorize(Activity: "PoReceipts")]
        public ActionResult NonMerch(string PurchaseOrder, decimal Line)
        {
            try
            {
                PurchaseOrder = PurchaseOrder.PadLeft(15, '0');
                LabelPrintPoLine model = new LabelPrintPoLine();
                model.PurchaseOrder = PurchaseOrder;
                model.Line = Line;
                var grnSus = wdb.sp_GetGrnSuspenseAccByPoLine(PurchaseOrder, (int)Line).FirstOrDefault();
                model.GrnSuspense = grnSus.GrnLdgAcc;
                return View(model);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }




    }
}
