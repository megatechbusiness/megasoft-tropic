using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class StockTakeImportController : Controller
    {
        //
        // GET: /StockTakeImport/
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        SysproCore sys = new SysproCore();
        BusinessLogic.StockTakeImport post = new BusinessLogic.StockTakeImport();

        [CustomAuthorize(Activity: "StockTakeImport")]
        public ActionResult Index()
        {
            var WhList = db.sp_GetStockTakeWarehouse().ToList();
            ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.Increase = LoadIncrease().ToList();
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Index")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "StockTakeImport")]
        public ActionResult Index(Megasoft2.ViewModel.StockTakeImport model)
        {
            try
            {
                ModelState.Clear();
                var WhList = db.sp_GetStockTakeWarehouse().ToList();
                ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.Increase = LoadIncrease().ToList();

                var outModel = new Megasoft2.ViewModel.StockTakeImport();
                outModel.Warehouse = model.Warehouse;
                outModel.Increase = model.Increase;
                outModel.Detail = db.sp_GetStockTakeCaptureByWarehouse(model.Warehouse).ToList();

                if (outModel.Detail.Count == 0)
                {
                    ModelState.AddModelError("", "No items found.");
                }

                return View("Index", outModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
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


        [CustomAuthorize(Activity: "StockTakeImport")]
        public ActionResult DeleteItem(int ItemId, string Warehouse, string Increase)
        {
            try
            {
                ModelState.Clear();

                db.sp_DeleteStockTakeEntry(Warehouse, ItemId);

                var WhList = db.sp_GetStockTakeWarehouse().ToList();
                ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.Increase = LoadIncrease().ToList();

                var outModel = new Megasoft2.ViewModel.StockTakeImport();
                outModel.Warehouse = Warehouse;
                outModel.Increase = Increase;
                outModel.Detail = db.sp_GetStockTakeCaptureByWarehouse(Warehouse).ToList();
                return View("Index", outModel);
            }
            catch (Exception ex)
            {
                var WhList = db.sp_GetStockTakeWarehouse().ToList();
                ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.Increase = LoadIncrease().ToList();
                ModelState.AddModelError("", ex.Message);
                var outModel = new Megasoft2.ViewModel.StockTakeImport();
                outModel.Warehouse = Warehouse;
                outModel.Increase = Increase;
                outModel.Detail = db.sp_GetStockTakeCaptureByWarehouse(Warehouse).ToList();
                return View("Index", outModel);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostStockTake")]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "StockTakeImport")]
        public ActionResult PostStockTake(Megasoft2.ViewModel.StockTakeImport model)
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
                    string XmlOut = sys.SysproPost(Guid, post.BuildStockTakeParameter(model.Warehouse, model.Increase), post.BuildStockTakeDoc(model.Warehouse, model.Increase.ToString(), model.Reference), "INVTSC");
                    sys.SysproLogoff(Guid);
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);
                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        ModelState.AddModelError("", "Posted Successfully.");
                        db.sp_ArchiveStockTake(model.Warehouse, HttpContext.User.Identity.Name.ToUpper());
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                    }
                }


                var WhList = db.sp_GetStockTakeWarehouse().ToList();
                ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.Increase = LoadIncrease().ToList();
                var outModel = new Megasoft2.ViewModel.StockTakeImport();
                outModel.Warehouse = model.Warehouse;
                outModel.Increase = model.Increase;
                outModel.Detail = db.sp_GetStockTakeCaptureByWarehouse(model.Warehouse).ToList();
                return View("Index", outModel);

            }
            catch (Exception ex)
            {
                var WhList = db.sp_GetStockTakeWarehouse().ToList();
                ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                ViewBag.Increase = LoadIncrease().ToList();
                ModelState.AddModelError("", ex.Message);
                var outModel = new Megasoft2.ViewModel.StockTakeImport();
                outModel.Warehouse = model.Warehouse;
                outModel.Increase = model.Increase;
                outModel.Detail = db.sp_GetStockTakeCaptureByWarehouse(model.Warehouse).ToList();
                return View("Index", outModel);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PurgeData")]
        [ValidateAntiForgeryToken]
        public ActionResult PurgeData(Megasoft2.ViewModel.StockTakeImport model)
        {
            ModelState.Clear();
            var WhList = db.sp_GetStockTakeWarehouse().ToList();
            ViewBag.Warehouse = (from a in WhList select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.Increase = LoadIncrease().ToList();
            try
            {

                db.sp_DeleteStockTakeByWarehouse(model.Warehouse);

                ModelState.AddModelError("", "Data purged for Warehouse : " + model.Warehouse + ".");

                model.Detail = db.sp_GetStockTakeCaptureByWarehouse(model.Warehouse).ToList();



                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


    }
}
