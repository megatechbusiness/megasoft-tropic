using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransportSystemPoInvoiceController : Controller
    {
        //
        // GET: /TransportSystemPoInvoice/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        TransportSystemBL BL = new TransportSystemBL();


        [CustomAuthorize(Activity: "TransportInvoice")]
        public ActionResult Index()
        {
            try
            {
                return View();
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }            
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadPo")]
        public ActionResult LoadPo(TransportSystemWaybillEntryViewModel model)
        {
            try
            {
                ModelState.Clear();

                var result = wdb.sp_GetTransporterPoLines(model.PurchaseOrder).ToList();

                if(result.Count == 0)
                {
                    TransportSystemWaybillEntryViewModel newModel = new TransportSystemWaybillEntryViewModel();
                    newModel.PurchaseOrder = model.PurchaseOrder;
                    ModelState.AddModelError("", "Purchase Order not found!");
                    return View("Index", newModel);
                }

                TransportSystemWaybillEntryViewModel modelOut = new TransportSystemWaybillEntryViewModel();

                modelOut.PurchaseOrder = result.FirstOrDefault().PurchaseOrder;
                modelOut.Supplier = result.FirstOrDefault().Supplier;
                modelOut.SupplierName = result.FirstOrDefault().SupplierName;
                modelOut.PoLines = result;


                return View("Index", modelOut);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }
        

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostGrnInvoice")]
        public ActionResult PostGrnInvoice(TransportSystemWaybillEntryViewModel model)
        {
            try
            {
                ModelState.Clear();
                string PostReturn = BL.PostGrnAp(model);
                ModelState.AddModelError("", PostReturn);
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
