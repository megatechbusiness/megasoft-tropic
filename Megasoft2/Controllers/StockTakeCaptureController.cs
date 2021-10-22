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
    public class StockTakeCaptureController : Controller
    {
        //
        // GET: /StockTakeOn/
       // WarehouseManagementEntities st = new WarehouseManagementEntities("");
        WarehouseManagementEntities st = new WarehouseManagementEntities("");
        SysproStockTakeOn objSyspro = new SysproStockTakeOn();
        SysproCore sys = new SysproCore();
        BusinessLogic.StockTakeImport post = new BusinessLogic.StockTakeImport();

        MegasoftEntities wdb = new MegasoftEntities();
        [CustomAuthorize(Activity: "StockTakeCapture")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");

            var WhList = st.sp_GetStockTakeWarehouse().ToList();

            ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse +" - "+a.Description }).ToList();
            return View();
        }


        [HttpPost]
        public ActionResult GetUom(string StockCode)
        {
            var result = (from a in st.InvMasters where a.StockCode==StockCode select a.StockUom ).FirstOrDefault();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Activity: "StockTakeCapture")]
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
        [HttpPost]
        
        public ActionResult PostStockTake(string details)
        {
            try
            {
                return Json(objSyspro.PostStockTake(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        public class MultiBin
        {
            public string Warehouse { get; set; }
            public string Source { get; set; }
        }


        public ActionResult DeleteStockTakeEntry(string details)
        {
            try
            {
                List<StockTakeOn> myDeserializedObjList = (List<StockTakeOn>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<StockTakeOn>));
                foreach (var item in myDeserializedObjList)
                {
                    st.sp_DeleteStockTakeEntry(item.Warehouse.Trim(),item.Id);
                }
                return Json("Deleted Successfully.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [CustomAuthorize(Activity: "StockTakeCapture")]
        public ActionResult ValidateDetails(string details)
        {
            try
            {
                return Json(objSyspro.ValidateBarcode(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
         [HttpPost]
         [CustomAuthorize(Activity: "StockTakeCapture")]
        public ActionResult GetLast3Scans(string Warehouse)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();  
                var rows = st.sp_GetLast3Scans(Warehouse,Username).ToList();
                return Json(rows, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
         public ActionResult StockTakeReview()
         {
             var WhList = st.sp_GetStockTakeWarehouse().ToList();

             ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();

             ViewBag.Increase = LoadIncrease().ToList();

             return View();
         }

         [HttpPost]
         [MultipleButton(Name = "action", Argument = "StockTakeReview")]
         [ValidateAntiForgeryToken]
         [CustomAuthorize(Activity: "StockTakeReview")]
         public ActionResult StockTakeReview(StockTakeReview model)
         {


             var result = st.sp_GetStockReview(model.Warehouse).ToList();

             StockTakeReview objOut = new StockTakeReview();
             objOut.Warehouse = model.Warehouse;
             objOut.Stock = result;

             var WhList = st.sp_GetStockTakeWarehouse().ToList();

             ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();

             ViewBag.Increase = LoadIncrease().ToList();
             return View("StockTakeReview", objOut);
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
         [MultipleButton(Name = "action", Argument = "PostSysproStockTake")]
         [ValidateAntiForgeryToken]
         [CustomAuthorize(Activity: "StockTakeReview")]
         public ActionResult PostSysproStockTake(StockTakeReview model)
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
                     string Guid = sys.SysproLogin();
                     string XmlOut = sys.SysproPost(Guid, post.BuildStockTakeParameter(model.Warehouse, model.Increase), post.BuildStockTakeDocForReview(model.Stock, model.Reference), "INVTSC");
                     sys.SysproLogoff(Guid);
                     string ErrorMessage = sys.GetXmlErrors(XmlOut);
                     if (string.IsNullOrEmpty(ErrorMessage))
                     {
                         ModelState.AddModelError("", "Posted Successfully.");
                         //st.sp_ArchiveStockTake(model.Warehouse, HttpContext.User.Identity.Name.ToUpper());
                     }
                     else
                     {
                         ModelState.AddModelError("", ErrorMessage);
                     }
                 }


                 var WhList = st.sp_GetStockTakeWarehouse().ToList();
                 ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                 ViewBag.Increase = LoadIncrease().ToList();
                 var outModel = new StockTakeReview();
                 outModel.Warehouse = model.Warehouse;
                 outModel.Increase = model.Increase;
                 outModel.Stock = st.sp_GetStockReview(model.Warehouse).ToList();
                 //RedirectToAction("StockTakeReview");
                 return View("StockTakeReview", outModel);

             }
             catch (Exception ex)
             {
                 var WhList = st.sp_GetStockTakeWarehouse().ToList();
                 ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                 ViewBag.Increase = LoadIncrease().ToList();
                 ModelState.AddModelError("", ex.Message);
                 var outModel = new StockTakeReview();
                 outModel.Warehouse = model.Warehouse;
                 outModel.Increase = model.Increase;
                 outModel.Stock = st.sp_GetStockReview(model.Warehouse).ToList();
                 return View("StockTakeReview", outModel);
             }
         }


         public ActionResult SaveScan()
         {
             return View();
         }

    }
}
