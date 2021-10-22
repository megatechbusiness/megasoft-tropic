using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class PurchaseOrderBrowseController : Controller
    {

        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        public ActionResult Index(string SearchText)
        {

            if (string.IsNullOrEmpty(SearchText))
            {
                List<sp_GetPurchaseOrderListForLabels_Result> Po = db.sp_GetPurchaseOrderListForLabels("").OrderBy(a => a.PurchaseOrder).ToList();
                return PartialView(Po);
            }
            else
            {
                List<sp_GetPurchaseOrderListForLabels_Result> Po = db.sp_GetPurchaseOrderListForLabels(SearchText.ToUpper()).OrderBy(a => a.PurchaseOrder).ToList();
                
                return PartialView(Po);
            }
           

        }

    }
}
