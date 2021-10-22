using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class ProductionReceiptController : Controller
    {
        BusinessLogic.ProductionReceipt objProd = new BusinessLogic.ProductionReceipt();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PostProductionReceipt(string details)
        {
            try
            {
                return Json(objProd.PostProductionReceipt(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
