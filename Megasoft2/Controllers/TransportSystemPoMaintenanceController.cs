using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransportSystemPoMaintenanceController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        TransportSystemBL BL = new TransportSystemBL();
        //
        // GET: /TransportSystemPoMaintenance/


        [CustomAuthorize(Activity: "TransportPOMaintenance")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadPo")]
        public ActionResult LoadPo(TransportPoMaintenanceViewModel model)
        {
            try
            {
                ModelState.Clear();
                var result = wdb.sp_GetTransPoLinesForMaintenance(model.PurchaseOrder.PadLeft(15, '0')).ToList();
                model.Detail = result;
                model.Supplier = result.FirstOrDefault().Supplier;
                return View("Index", model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostPo")]
        public ActionResult PostPo(TransportPoMaintenanceViewModel model)
        {
            try
            {
                ModelState.Clear();


                string PostResult = BL.PostMaintenance(model);
                ModelState.AddModelError("", PostResult);

                if(PostResult == "Purchase Order updated successfully.")
                {
                    var result = wdb.sp_GetTransPoLinesForMaintenance(model.PurchaseOrder.PadLeft(15, '0')).ToList();
                    model.Detail = result;
                    model.Supplier = result.FirstOrDefault().Supplier;
                }



                return View("Index", model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
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


        public ActionResult DeleteLine(string PurchaseOrder, int Line, int SeqNo)
        {
            try
            {
                ModelState.Clear();
                TransportPoMaintenanceViewModel model = new TransportPoMaintenanceViewModel();
                string PostResult = BL.PostDelete(PurchaseOrder, Line, SeqNo);
                if (PostResult == "Purchase Order Line Deleted successfully.")
                {
                    var result = wdb.sp_GetTransPoLinesForMaintenance(PurchaseOrder.PadLeft(15, '0')).ToList();
                    model.Detail = result;
                    model.Supplier = result.FirstOrDefault().Supplier;
                }
                ModelState.AddModelError("", PostResult);
                return View("Index", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index");
            }
        }

    }
}
