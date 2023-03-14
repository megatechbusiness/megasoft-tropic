using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static DotNetOpenAuth.OpenId.Extensions.AttributeExchange.WellKnownAttributes.Contact;

namespace Megasoft2.Controllers
{
    public class DesPlanPickingController : Controller
    {
        //
        // GET: /DesPlanPicking/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");


        [CustomAuthorize(Activity: "DesPlanPicking")]
        public ActionResult Index()
        {
            DispatchPlannerViewModel model = new DispatchPlannerViewModel();
            model.Plans = (from a in wdb.mtDispatchPlans where a.Picker == HttpContext.User.Identity.Name.ToUpper() select a).ToList();

            return View(model);
        }


        [CustomAuthorize(Activity: "DesPlanPicking")]
        public ActionResult Picking(string Cust, string So, int SoLine)
        {
            DispatchPlannerViewModel model = new DispatchPlannerViewModel();
            model.Plans = (from a in wdb.mtDispatchPlans where a.Picker == HttpContext.User.Identity.Name.ToUpper() && a.Customer == Cust && a.SalesOrder == So && a.SalesOrderLine == SoLine select a).ToList();
            return View(model);
        }
    }
}
