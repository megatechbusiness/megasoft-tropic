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
    public class WhseManPalletAuditController : Controller
    {
        //
        // GET: /WhseManPalletAudit/
        WarehouseManagementEntities st = new WarehouseManagementEntities("");
        SysproCore sys = new SysproCore();
        BusinessLogic.StockTakeImport post = new BusinessLogic.StockTakeImport();

        [CustomAuthorize(Activity: "PalletAudit")]
        public ActionResult Index()
        {
            var WhList = st.sp_GetPalletAuditWarehouse().ToList();

            ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            return View();
        }

        [CustomAuthorize(Activity: "PalletAuditPurge")]
        public ActionResult PalletAuditPurge()
        {

            ViewBag.WarehouseList = (from a in st.mtPalletAudits select new { Warehouse = a.Warehouse, Description = a.Warehouse }).Distinct().ToList();
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Activity: "PalletAuditPurge")]
        [MultipleButton(Name = "action", Argument = "PalletAuditPurge")]
        public ActionResult PalletAuditPurge(PalletAudit model)
        {
            try
            {
                st.sp_PalletAuditPurge(model.Warehouse);
                ModelState.AddModelError("", "Records deleted successfully.");
                ViewBag.WarehouseList = (from a in st.mtPalletAudits select new { Warehouse = a.Warehouse, Description = a.Warehouse }).Distinct().ToList();
                return View("PalletAuditPurge");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.WarehouseList = (from a in st.mtPalletAudits select new { Warehouse = a.Warehouse, Description = a.Warehouse }).Distinct().ToList();
                return View("PalletAuditPurge");
            }
        }


        [HttpPost]
        [CustomAuthorize(Activity: "PalletAuditPurge")]
        [MultipleButton(Name = "action", Argument = "CopyToStockTake")]
        public ActionResult CopyToStockTake(PalletAudit model)
        {
            try
            {

                st.sp_CopyPalletAuditToStockTake(model.Warehouse);
                ModelState.AddModelError("", "Records Copied successfully.");
                ViewBag.WarehouseList = (from a in st.mtPalletAudits select new { Warehouse = a.Warehouse, Description = a.Warehouse }).Distinct().ToList();
                return View("PalletAuditPurge");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.WarehouseList = (from a in st.mtPalletAudits select new { Warehouse = a.Warehouse, Description = a.Warehouse }).Distinct().ToList();
                return View("PalletAuditPurge");
            }
        }



        [HttpPost]
        [CustomAuthorize(Activity: "PalletAudit")]
        public ActionResult CheckWarehouseMultiBin(string details)
        {
            try
            {
                List<MultiBin> myDeserializedObjList = (List<MultiBin>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MultiBin>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse.Trim();
                    var result = (from a in st.vw_InvWhControl where a.Warehouse == Warehouse select a).ToList();
                    if (result.Count > 0)
                    {
                        ////*********FOR TROPIC TAKE-ON LOTS ONLY******************
                        //if(Warehouse == "RS" || Warehouse == "RM")
                        //{
                        //    return Json("Y", JsonRequestBehavior.AllowGet);
                        //}
                        ////*********FOR TROPIC TAKE-ON LOTS ONLY******************
                        return Json(result.FirstOrDefault().UseMultipleBins, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error. Warehouse : " + Warehouse + " not found in Syspro.", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("Error - No Data. Warehouse not found.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.InnerException.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public class MultiBin
        {
            public string Warehouse { get; set; }
            public string Source { get; set; }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "PalletAudit")]
        public ActionResult ValidateDetails(string details)
        {
            try
            {
                return Json(this.ValidateBarcode(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public string ValidateBarcode(string details)
        {
            try
            {
                List<PalletAudit> myDeserializedObjList = (List<PalletAudit>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<PalletAudit>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var StockCodeCheck = st.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                        if (StockCodeCheck == null)
                        {
                            return "StockCode not found!.";
                        }

                        var StockWarehouseCheck = st.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse)).FirstOrDefault();
                        if (StockWarehouseCheck == null)
                        {
                            return "StockCode not stocked in Warehouse " + item.Warehouse + "!.";
                        }


                        var PalletScanned = (from a in st.mtPalletAudits where a.StockCode == item.StockCode && a.Lot == item.Lot select a).ToList();
                        if(PalletScanned.Count > 0)
                        {
                            return "Item already scanned!";
                        }
                        
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public ActionResult DeleteItem(string details)
        {
            try
            {
                List<PalletAudit> myDeserializedObjList = (List<PalletAudit>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<PalletAudit>));
                foreach (var item in myDeserializedObjList)
                {
                    st.sp_DeletePalletAudit(item.Warehouse.Trim(), item.Id);
                }
                return Json("Deleted Successfully.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "PalletAudit")]
        public ActionResult GetLast3Scans(string Warehouse)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var rows = st.sp_GetPalletAuditScans(Warehouse, Username).ToList();
                return Json(rows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult SavePalletAudit(string details)
        {
            try
            {
                string Barcode = this.ValidateBarcode(details);
                if (Barcode == "")
                {
                    List<PalletAudit> myDeserializedObjList = (List<PalletAudit>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<PalletAudit>));
                    foreach (var item in myDeserializedObjList)
                    {
                        st.sp_SavePalletAudit(item.Warehouse, item.Bin, item.StockCode, item.Lot, (decimal)item.Quantity, HttpContext.User.Identity.Name.ToUpper());
                    }
                    return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
                }

                else return Json(Barcode, JsonRequestBehavior.AllowGet); ;


            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet); ;
            }
        }


        [CustomAuthorize(Activity: "PalletAuditReview")]
        public ActionResult PalletAuditReview()
        {
            var WhList = st.sp_GetStockTakeWarehouse().ToList();
            ViewBag.WarehouseList = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.IncreaseList = LoadIncrease().ToList();
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadPalletAuditReview")]
        [ValidateAntiForgeryToken]        
        public ActionResult LoadPalletAuditReview(PalletAuditViewModel model)
        {
            ModelState.Clear();
            var WhList = st.sp_GetStockTakeWarehouse().ToList();
            //var WhList = (from a in st.vw_InvWhControl where a.Warehouse == "RM" select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.WarehouseList = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.IncreaseList = LoadIncrease().ToList();
            var detail = st.sp_PalletOrderReport().Where(a => a.ScanWhse == model.Warehouse || a.CurrWhse == model.Warehouse).Where(a => a.StockCode == model.StockCode).OrderBy(a => a.StockCode).ThenBy(a => a.BatchId).ThenBy(a => a.BatchSeqNum).ToList();
            model.Detail = detail;
            return View("PalletAuditReview", model);
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SavePalletAuditReview")]
        [ValidateAntiForgeryToken]
        public ActionResult SavePalletAuditReview(PalletAuditViewModel model)
        {
            ModelState.Clear();
            if(model.Detail.Count > 0)
            {
                foreach(var item in model.Detail)
                {

                    st.sp_SavePalletAuditReview(item.ScanWhse, item.ScanBin, item.StockCode, item.Lot, item.Quantity, HttpContext.User.Identity.Name.ToUpper());
                    //if (!string.IsNullOrEmpty(item.ScanWhse))
                    //{
                    //    if (!string.IsNullOrEmpty(item.ScanBin))
                    //    {
                    //        //if (item.Quantity != 0)
                    //        //{
                    //            //Add or Update Entry
                    //            st.sp_SavePalletAuditReview(item.ScanWhse, item.ScanBin, item.StockCode, item.Lot, item.Quantity, HttpContext.User.Identity.Name.ToUpper());
                    //        //}
                    //        //else
                    //        //{
                    //        //    //Delete Entry
                    //        //    using (var ddb = new WarehouseManagementEntities(""))
                    //        //    {
                    //        //        var delEnt = (from a in ddb.mtPalletAudits where a.Warehouse == item.ScanWhse && a.Bin == item.ScanBin && a.StockCode == item.StockCode && a.Lot == item.Lot select a).FirstOrDefault();
                    //        //        if (delEnt != null)
                    //        //        {
                    //        //            ddb.Entry(delEnt).State = System.Data.EntityState.Deleted;
                    //        //            ddb.SaveChanges();
                    //        //        }
                    //        //    }
                    //        //}
                    //    }
                    //}
                    //else
                    //{
                    //    //Delete Entry
                    //    using (var ddb = new WarehouseManagementEntities(""))
                    //    {
                    //        var delEnt = (from a in ddb.mtPalletAudits where a.Warehouse == item.ScanWhse && a.Bin == item.ScanBin && a.StockCode == item.StockCode && a.Lot == item.Lot select a).FirstOrDefault();
                    //        if (delEnt != null)
                    //        {
                    //            ddb.Entry(delEnt).State = System.Data.EntityState.Deleted;
                    //            ddb.SaveChanges();
                    //        }
                    //    }
                    //}
                }
            }
            var WhList = st.sp_GetStockTakeWarehouse().ToList();
            //var WhList = (from a in st.vw_InvWhControl where a.Warehouse == "RM" select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.WarehouseList = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.IncreaseList = LoadIncrease().ToList();
            var detail = st.sp_PalletOrderReport().Where(a => a.ScanWhse == model.Warehouse || a.CurrWhse == model.Warehouse).Where(a => a.StockCode == model.StockCode).OrderBy(a => a.StockCode).ThenBy(a => a.BatchId).ThenBy(a => a.BatchSeqNum).ToList();
            model.Detail = detail;
            return View("PalletAuditReview", model);
        }

        public List<SelectListItem> LoadIncrease()
        {
            List<SelectListItem> Increase = new List<SelectListItem>
            {

                new SelectListItem{Text = "Replace", Value="Replace"},
                new SelectListItem{Text = "Increase", Value="Increase"}
            };
            return Increase;
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostStockTake")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "StockTakeImport")]
        public ActionResult PostStockTake(PalletAuditViewModel model)
        {
            try
            {
                ModelState.Clear();

                if (string.IsNullOrEmpty(model.Reference))
                {
                    ModelState.AddModelError("", "Please enter a Reference.");
                }
                else
                {

                    st.sp_CopyPalletAuditToStockTake(model.Warehouse);

                    string Guid = sys.SysproLogin();
                    string XmlOut = sys.SysproPost(Guid, post.BuildStockTakeParameter(model.Warehouse, model.Increase), post.BuildStockTakeDoc(model.Warehouse, model.Increase.ToString(), model.Reference), "INVTSC");
                    sys.SysproLogoff(Guid);
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        ModelState.AddModelError("", "Posted Successfully.");
                        st.sp_ArchiveStockTake(model.Warehouse, HttpContext.User.Identity.Name.ToUpper());
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                    }
                }


                

                var WhList = st.sp_GetStockTakeWarehouse().ToList();
                //var WhList = (from a in st.vw_InvWhControl where a.Warehouse == "RM" select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.WarehouseList = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.IncreaseList = LoadIncrease().ToList();
                var detail = st.sp_PalletOrderReport().Where(a => a.ScanWhse == model.Warehouse || a.CurrWhse == model.Warehouse).Where(a => a.StockCode == model.StockCode).OrderBy(a => a.StockCode).ThenBy(a => a.BatchId).ThenBy(a => a.BatchSeqNum).ToList();
                model.Detail = detail;
                return View("PalletAuditReview", model);
            }
            catch (Exception ex)
            {
                var WhList = st.sp_GetStockTakeWarehouse().ToList();
                //var WhList = (from a in st.vw_InvWhControl where a.Warehouse == "RM" select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.WarehouseList = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.IncreaseList = LoadIncrease().ToList();
                ModelState.AddModelError("", ex.Message);
                return View("PalletAuditReview", model);
            }
        }
        

    }
}
