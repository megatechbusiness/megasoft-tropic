using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class StereoSystemGrnController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        private StereoSystem BL = new StereoSystem();

        //
        // GET: /StereoSystemGrn/
        [CustomAuthorize(Activity: "Grn")]
        public ActionResult Index(int TrackId = 0)
        {
            StereoSystemGrnViewModel model = new StereoSystemGrnViewModel();
            model.InvoiceDate = DateTime.Now;
            ViewBag.LoadPo = false;
            return View(model);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadPo")]
        public ActionResult LoadPo(StereoSystemGrnViewModel model)
        {
            try
            {
                if(string.IsNullOrEmpty(model.PurchaseOrder))
                {
                    ViewBag.LoadPo = false;
                    ModelState.AddModelError("", "Please enter a purchase order number.");
                    return View("Index", model);
                }
                ModelState.Clear();
                model.PoLine = wdb.sp_StereoPoLinesForGrn(model.PurchaseOrder.PadLeft(15, '0')).ToList();
                
                if (model.PoLine.Count == 0)
                {
                    model.PurchaseOrder = model.PurchaseOrder.PadLeft(15, '0');
                    ViewBag.LoadPo = false;
                    ModelState.AddModelError("", "No outstanding lines for this purchase order");
                }
                else
                {
                    ViewBag.LoadPo = true;
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Grn")]
        public ActionResult PostGrn(StereoSystemGrnViewModel model)
        {
            try
            {
                        ModelState.Clear();
                        ModelState.AddModelError("", BL.PostGrnAp(model));
                        ViewBag.LoadPo = false;
                        StereoSystemGrnViewModel m = new StereoSystemGrnViewModel();
                        m.InvoiceDate = DateTime.Now;
                        return View("Index", m);


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.LoadPo = true;
                return View("Index", model);
            }
        }
    }
}