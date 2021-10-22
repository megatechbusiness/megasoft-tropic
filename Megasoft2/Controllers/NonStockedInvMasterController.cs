using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class NonStockedInvMasterController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "NonStockedMaster")]
        public ActionResult Index()
        {
            var model = (from a in wdb.mtNonStockMasters select a).ToList();
            return View(model);
        }

        [CustomAuthorize(Activity: "NonStockedMaster")]
        public ActionResult Edit(string StockCode = null)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.WarehouseList = (from a in WhList where a.Allowed == true select new { Value = a.Warehouse, Text = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.ProductClassList = (from a in wdb.SalProductClassDes select new { Value = a.ProductClass, Text = a.ProductClass + " - " + a.Description }).ToList();
            ViewBag.PartCategoryList = LoadPartCategory();
            ViewBag.MultDivList = LoadMultDiv();
            ViewBag.TraceableTypeList = LoadTraceable();

            if (string.IsNullOrWhiteSpace(StockCode))
            {
                return View(new mtNonStockMaster());
            }
            var model = (from a in wdb.mtNonStockMasters where a.StockCode == StockCode select a).FirstOrDefault();
            return View(model);
        }

        [CustomAuthorize(Activity: "NonStockedMaster")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(mtNonStockMaster model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.WarehouseList = (from a in WhList where a.Allowed == true select new { Value = a.Warehouse, Text = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.ProductClassList = (from a in wdb.SalProductClassDes select new { Value = a.ProductClass, Text = a.ProductClass + " - " + a.Description }).ToList();
            ViewBag.PartCategoryList = LoadPartCategory();
            ViewBag.MultDivList = LoadMultDiv();
            ViewBag.TraceableTypeList = LoadTraceable();
            try
            {
                model.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                model.DateLastSaved = DateTime.Now;
                using (var cdb = new WarehouseManagementEntities(""))
                {
                    var check = (from a in cdb.mtNonStockMasters where a.StockCode == model.StockCode select a).ToList();
                    if (check.Count > 0)
                    {
                        wdb.Entry(model).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        var SysproCheck = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == model.StockCode select a).ToList();
                        if (SysproCheck.Count > 0)
                        {
                            model.StockCode = null;
                            ModelState.AddModelError("", "StockCode already exists in Syspro!");
                            return View(model);
                        }
                        wdb.Entry(model).State = System.Data.EntityState.Added;
                    }
                    wdb.SaveChanges();
                }
                
                
                ModelState.AddModelError("", "Saved Successfully");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }

        }

        [CustomAuthorize(Activity: "NonStockedMaster")]
        public ActionResult Delete(string StockCode = null)
        {
            var check = (from a in wdb.mtNonStockMasters where a.StockCode == StockCode select a).ToList();
            if (check.Count > 0)
            {
                wdb.Entry(check.FirstOrDefault()).State = System.Data.EntityState.Deleted;
                ModelState.AddModelError("", "Deleted Successfully");
            }
            else
            {
                ModelState.AddModelError("", "StockCode not found!");
            }
            var model = (from a in wdb.mtNonStockMasters select a).ToList();
            return View("Index", model);
        }

        public List<SelectListItem> LoadMultDiv()
        {
            List<SelectListItem> MD = new List<SelectListItem>
            {
                new SelectListItem{Text = "M", Value="M"},
                new SelectListItem{Text = "D", Value="D"}
            };
            return MD;
        }

        public List<SelectListItem> LoadTraceable()
        {
            List<SelectListItem> TR = new List<SelectListItem>
            {
                new SelectListItem{Text = "Non - Traceable", Value="N"},
                new SelectListItem{Text = "Traceable", Value="T"},
                new SelectListItem{Text = "Inspection Required", Value="I"}
            };
            return TR;
        }

        public List<SelectListItem> LoadPartCategory()
        {
            List<SelectListItem> PC = new List<SelectListItem>
            {
                new SelectListItem{Text = "Made in", Value="M"},
                new SelectListItem{Text = "Bought out", Value="M"},
                new SelectListItem{Text = "Phantom Part", Value="G"}
            };
            return PC;
        }


    }
}
