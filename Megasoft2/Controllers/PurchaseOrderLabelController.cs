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
    public class PurchaseOrderLabelController : Controller
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        LabelPrint objPrint = new LabelPrint();

        public ActionResult Index(string PurchaseOrder = null)
        {
            try
            {

                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                if(string.IsNullOrEmpty(PurchaseOrder))
                {
                    PurchaseOrderLabel model = new PurchaseOrderLabel();

                    return View(model);
                }
                else
                {
                    PurchaseOrderLabel model = new PurchaseOrderLabel();
                    PurchaseOrder = model.PurchaseOrder.PadLeft(15, '0');
                    var PoCheck = (from a in db.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a).ToList();
                    if (PoCheck.Count > 0)
                    {
                        model.PoLines = db.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company).ToList();
                        model.PurchaseOrder = PurchaseOrder;
                        if (model.PoLines.Count == 0)
                        {
                            ModelState.AddModelError("", "No outstanding lines found for Purchase Order");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Purchase Order not found.");
                    }
                    return View(model);
                }
                
            }
            catch(Exception ex)
            {
                PurchaseOrderLabel model = new PurchaseOrderLabel();
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }            
        }  

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(PurchaseOrderLabel model)
        {
            try
            {
                string User = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                if (!string.IsNullOrEmpty(model.PurchaseOrder))
                {
                    string PurchaseOrder = model.PurchaseOrder.PadLeft(15, '0');
                    var PoCheck = (from a in db.PorMasterHdrs where a.PurchaseOrder == PurchaseOrder select a).ToList();
                    if (PoCheck.Count > 0)
                    {
                        model.PoLines = db.sp_GetPurchaseOrderLinesForLabel(PurchaseOrder, User, Company).ToList();
                        model.PurchaseOrder = PurchaseOrder;
                        if (model.PoLines.Count == 0)
                        {
                            ModelState.AddModelError("", "No outstanding lines found for Purchase Order");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Purchase Order not found.");
                    }

                }
                else
                {
                    ModelState.AddModelError("","Purchase Order number cannot be blank!");
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public ActionResult PoLine(string PurchaseOrder, decimal Line)
        {
            LabelPrintPoLine line = new LabelPrintPoLine();
            var result = (from a in db.PorMasterDetails where a.PurchaseOrder == PurchaseOrder && a.Line == Line select a).FirstOrDefault();
            if(result!= null)
            {
                line.PurchaseOrder = result.PurchaseOrder;
                line.Line = (decimal)result.Line;
                line.StockCode = result.MStockCode;
                line.Description = result.MStockDes;

            }
            return View(line);
        }



  
        public ActionResult Search()
        {
            return PartialView("_Search");
        }


        [HttpPost]
        public ActionResult SearchPo(string query)
        {
            //if (query != null)
            //{
            //    try
            //    {


            //        var model = new LabelPrintPurchaseOrder
            //        {
            //            Lines = db.ms_sp_GetPurchaseOrderLines(query).ToList()
            //        };
                 
                    

            //        return PartialView("_Results", model);
            //    }
            //    catch (Exception e)
            //    {
            //        throw new Exception(e.Message);
            //    }
            //}
            return PartialView("Error");
        }
       
        public ActionResult GetResults(string searchTerm)
        {
            try
            {
                //List<LabelPrintPurchaseOrder> myDeserializedObjList = (List<LabelPrintPurchaseOrder>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPurchaseOrder>));
                //string searchTerm = myDeserializedObjList.FirstOrDefault().searchTerm;
                //var Lines = db.ms_sp_GetPurchaseOrderLines(query).ToList();
                LabelPrintPurchaseOrder lpo = new LabelPrintPurchaseOrder();
                //lpo.Lines = db.ms_sp_GetPurchaseOrderLines(searchTerm).ToList();
                return View("Index", lpo);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


      
        public ActionResult DisplayLabel(string PurchaseOrder, decimal Line)
        {
            try
            {
                LabelPrintPoLine line = new LabelPrintPoLine();
                var result = (from a in db.PorMasterDetails where a.PurchaseOrder == PurchaseOrder && a.Line == Line select a).FirstOrDefault();
                if (result != null)
                {
                    line.PurchaseOrder = result.PurchaseOrder;
                    line.Line = (decimal)result.Line;
                    line.StockCode = result.MStockCode;
                    line.Description = result.MStockDes;
                    line.NoOfLables = 1;
                }                               
                ViewBag.Header = "P/O : " + PurchaseOrder + " Line : " + Line;
                return PartialView(line);
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult PrintLabel(string details)
        {
            try
            {
                List<LabelPrintPoLine> detail = (List<LabelPrintPoLine>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<LabelPrintPoLine>));
                //string PoReceipt = objPrint.PostPoReceipt(detail);
                //if(!string.IsNullOrEmpty(PoReceipt))
                //{
                //    return Json(PoReceipt, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    using(var db = new WarehouseManagementEntities(""))
                //    {
                //        foreach (var item in detail)
                //        {
                //            mtPurchaseOrderLabel objLabel = new mtPurchaseOrderLabel();
                //            objLabel.PurchaseOrder = item.PurchaseOrder;
                //            objLabel.Line = item.Line;
                //            objLabel.DeliveryNote = item.DeliveryNote;
                //            objLabel.DeliveryDate = Convert.ToDateTime(item.DeliveryDate.ToString("yyyy-MM-dd"));
                //            objLabel.StockCode = item.StockCode;
                //            objLabel.Description = item.Description;
                //            objLabel.ReelQty = item.ReelQuantity;
                //            objLabel.ReelNumber = item.ReelNumber;
                //            objLabel.NoOfLabels = item.NoOfLables;
                //            objLabel.Username = HttpContext.User.Identity.Name.ToUpper();
                //            objLabel.TrnDate = DateTime.Now;
                //            db.mtPurchaseOrderLabels.Add(objLabel);
                //            db.SaveChanges();
                //        }
                //    }
                //    objPrint.PrintLabel();
                //    return Json("Completed Successfully", JsonRequestBehavior.AllowGet);
                    
                //}
                objPrint.PrintLabel(detail);
                return Json("Completed Successfully", JsonRequestBehavior.AllowGet);
                //return Json(objPrint.PrintPoLabel("","",""), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

       

    }
}
