using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.ViewModel;

namespace Megasoft2.Controllers
{
    public class InventoryQueryController : Controller
    {
        //
        // GET: /InventoryQuery/
        //
        [CustomAuthorize(Activity: "InventoryQuery")]
        public ActionResult Index()
        {
            ModelState.Clear();

            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Index")]
        public ActionResult Index(InventoryQueryViewModel model)
        {
            ModelState.Clear();

            WarehouseManagementEntities db = new WarehouseManagementEntities("");

            try
            {
                //get InvMaster and InvWarehouse stock data
                if (model.StockCode != null)
                {
                    model.Items = db.mt_InvQueryStockCodeDetails(model.StockCode).ToList();

                    if (model.Items.Count > 0)
                    {
                        model.Description = model.Items.FirstOrDefault().Description;
                        model.StockUom = model.Items.FirstOrDefault().StockUom;
                        model.Supplier = model.Items.FirstOrDefault().Supplier;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Stock Code not found.");
                    }

                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View("Index", model);
        }
    }
}
