using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class MasterCardMultimediaViewerController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");


        [CustomAuthorize("MasterCardArtwork")]
        public ActionResult Index(string Customer = "")
        {
            //ModelState.AddModelError("", Customer);
            MasterCardViewModel model = new MasterCardViewModel();
            model.Header = new mtMasterCardHeader();
            try
            {
                if (!string.IsNullOrWhiteSpace(Customer))
                {
                    var result = wdb.sp_MasterCardGetStockCodeArtwork(Customer).ToList();
                    model.Multimedia = result;
                    model.Header.Customer = Customer;
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
        [MultipleButton(Name = "action", Argument = "Load")]
        public ActionResult Index(MasterCardViewModel model)
        {
            ModelState.Clear();
            try
            {
                var result = wdb.sp_MasterCardGetStockCodeArtwork(model.Header.Customer).ToList();
                model.Multimedia = result;
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        public FileResult GetReport(string Customer, string StockCode)
        {
            var filepath = (from a in wdb.sp_MasterCardGetStockCodeArtwork(Customer) where (a.StockCode == StockCode) select a).FirstOrDefault();
            string ReportURL = filepath.MultiMediaFilePath;
            byte[] FileBytes = System.IO.File.ReadAllBytes(ReportURL);
            return File(FileBytes, "application/pdf");
        }

    }
}
