using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class StockCodeImagesController : Controller
    {
        //
        // GET: /StockCodeImages/
        private WarehouseManagementEntities db = new WarehouseManagementEntities("");
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(StockCodeImagesViewModel model)
        {
            ModelState.Clear();
            if (model.StockCode == "")
            {
                 ModelState.AddModelError("","No Match Found");
            }
            else 
            {
                var InvMaster = (from a in db.InvMasters.AsNoTracking() where a.StockCode == model.StockCode select new { StockCode = a.StockCode, Description = a.Description, LongDesc = a.LongDesc }).FirstOrDefault();
                
                if (InvMaster != null)
                {
                    model.Description = InvMaster.Description;
                    model.LongDesc = InvMaster.LongDesc;
                    var ImageList = (from a in db.mtInvMasterImages where a.StockCode == model.StockCode select a).ToList();
                    model.ImageList = new List<string>();
                    foreach(var item in ImageList)
                    {
                        string imreBase64Data = Convert.ToBase64String(item.Image);
                        string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                        model.ImageList.Add(imgDataURL);
                    }
                    return View("Index", model);
                }
               
            }
            return View();
        }



    }
}
