using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManPhysicalAdjustmentController : Controller
    {
        //
        // GET: /WhseManPhysicalAdjustment/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        PhysicalAdjustmentBL objPost = new PhysicalAdjustmentBL();

        [CustomAuthorize(Activity: "PhysicalAdjustment")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            return View("Index");
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadData")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PhysicalAdjustment")]
        public ActionResult LoadData(PhysicalAdjustment model)
        {

            try
            {
                ModelState.Clear();
                var result = wdb.sp_GetLotsToAdjust(model.Warehouse).ToList();


                if(!string.IsNullOrEmpty(model.StockCode))
                {
                    result = (from a in result where a.QtyOnHand <= model.Limit && a.StockCode == model.StockCode select a).ToList();
                }
                else
                {
                    result = (from a in result where a.QtyOnHand <= model.Limit select a).ToList();
                }

                if(result.Count == 0)
                {
                    ModelState.AddModelError("", "No data found!");
                }

                PhysicalAdjustment objOut = new PhysicalAdjustment();
                objOut.Warehouse = model.Warehouse;
                objOut.StockCode = model.StockCode;
                objOut.Limit = model.Limit;
                objOut.Stock = result;

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();


                return View("Index", objOut);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                return View("Index");
            }
            
        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostAdjustment")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PhysicalAdjustment")]
        public ActionResult PostAdjustment(PhysicalAdjustment model)
        {

            try
            {
                ModelState.Clear();

                bool HasErrors = false;
                if(model.Stock.Count > 0)
                {
                    foreach(var item in model.Stock)
                    {
                        if(item.NewQty > 0)
                        {
                            ModelState.AddModelError("", "Adjustment greater than zero not allowed for Lot : " + item.Lot + " - StockCode : " + item.StockCode + "!");
                            HasErrors = true;
                        }
                        
                    }

                    //var check = (from a in model.Stock where a.NewQty);
                }
                else
                {
                    HasErrors = true;
                    ModelState.AddModelError("", "No data found!");
                }


                List<sp_GetLotsToAdjust_Result> result = new List<sp_GetLotsToAdjust_Result>();
                if(HasErrors == false)
                {
                    string Out = objPost.PostAdjustment(model);

                    ModelState.AddModelError("", Out);

                    result = wdb.sp_GetLotsToAdjust(model.Stock.FirstOrDefault().Warehouse).ToList();
                    if (!string.IsNullOrEmpty(model.StockCode))
                    {
                        result = (from a in result where a.QtyOnHand <= model.Limit && a.StockCode == model.StockCode select a).ToList();
                    }
                    else
                    {
                        result = (from a in result where a.QtyOnHand <= model.Limit select a).ToList();
                    }
                }
                else
                {
                    result = model.Stock;
                }
                

                

                PhysicalAdjustment objOut = new PhysicalAdjustment();
                objOut.Warehouse = model.Stock.FirstOrDefault().Warehouse;
                objOut.StockCode = model.StockCode;
                objOut.Limit = model.Limit;
                objOut.Stock = result;

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();


                return View("Index", objOut);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                return View("Index");
            }

        }

    }
}
