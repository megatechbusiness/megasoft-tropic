using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class DispatchVerificationController : Controller
    {
        //
        // GET: /DispatchVerification/
        DispatchVerificationBL BL = new DispatchVerificationBL();

        //[CustomAuthorize(Activity: "DispatchVerification")]
        public ActionResult Index()
        {
            var Company = BL.GetCompany();
            var Username = BL.GetUsername();

            return View();
        }

        //[CustomAuthorize(Activity: "DispatchVerification")]
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

        [HttpPost]
        //[CustomAuthorize(Activity: "DispatchVerification")]
        public ActionResult ValidateDispatchNote(string DispatchNote)
        {
            try
            {
                return Json(BL.ValidateDispatchNote(DispatchNote), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //[CustomAuthorize(Activity: "DispatchVerification")]
        public ActionResult Verification(string details)
        {
            try
            {
                return Json(BL.btnComplete(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
