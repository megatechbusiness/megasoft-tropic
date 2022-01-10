using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.BusinessLogic;

namespace Megasoft2.Controllers
{
    public class WhseManWickettLabelDeleteController : Controller
    {
        //
        // GET: /WhseManWickettLabelDelete/
        WickettLabelDeleteBL BL = new WickettLabelDeleteBL();

        [CustomAuthorize(Activity: "JobLabelMaintenance")]
        public ActionResult Index()
        {
            string Username = BL.GetUsername();
            var Company = BL.GetCompany();
            return View();
        }

        //[CustomAuthorize(Activity: "DeleteJobLabel")]
        public ActionResult ValidateDetails(string details)
        {
            try
            {
                return Json(BL.ValidateBarcode(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
