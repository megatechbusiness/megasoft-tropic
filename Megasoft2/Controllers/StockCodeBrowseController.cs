using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class StockCodeBrowseController : Controller
    {

        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        public ActionResult Index(string SearchText)
        {

            if (string.IsNullOrEmpty(SearchText))
            {
                var StockCodes = from a in db.InvMasters.AsNoTracking() select a;
                return PartialView(StockCodes);
            }
            else
            {
                SearchText = SearchText.ToUpper();
                var StockCodes = from a in db.InvMasters.AsNoTracking() where (a.StockCode.ToUpper().Contains(SearchText) || a.Description.ToUpper().Contains(SearchText) || a.LongDesc.ToUpper().Contains(SearchText)) select a;
                return PartialView(StockCodes);
            }


        }



    }
}
