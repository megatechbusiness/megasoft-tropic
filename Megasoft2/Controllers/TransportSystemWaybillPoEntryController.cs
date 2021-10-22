using Megasoft2.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransportSystemWaybillPoEntryController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        TransportSystem BL = new TransportSystem();
        // GET: /TransportSystemWaybillPoEntry/

        [CustomAuthorize(Activity: "TransportPurchaseOrders")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetWaybillPo(string Supplier)
        {
            try
            {
                var result = wdb.sp_GetTransWaybillsPoEntry(Supplier.ToUpper());
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        
        public ActionResult AddPoLine()
        {
            return PartialView();
        }
        
        public ActionResult CommentLine()
        {
            return PartialView();
        }

        public ActionResult SupplierSearch()
        {
            return PartialView();
        }

        public JsonResult SupplierList()
        {
            //if (FilterText == "")
            //{
            //    FilterText = "NULL";
            //}
            var result = wdb.sp_GetTransporters("");
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WaybillSearch()
        {
            return PartialView();
        }
        public JsonResult WaybillList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            var result = wdb.sp_GetTransWaybillsPoEntry(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PostPurchaseOrder(string details)
        {
            try
            {
                return Json(BL.PostPurchaseOrder(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult TaxCode()
        {
            return PartialView();
        }



        public ActionResult TaxCodeList()
        {

            var result = (from a in wdb.AdmTaxes select new { TaxCode = a.TaxCode, Description = a.Description }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}
