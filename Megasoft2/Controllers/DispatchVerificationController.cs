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
        public ActionResult ValidateDispatchNote(string DispatchNote, int TrackId)
        {
            try
            {
                return Json(BL.ValidateDispatchNote(DispatchNote, TrackId), JsonRequestBehavior.AllowGet);
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
                string result = BL.btnComplete(details);
                ModelState.AddModelError("", result);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        

        //[CustomAuthorize(Activity: "DispatchVerification")]
        public ActionResult GetItemsScanned(int TrackId, string DispatchNote)
        {
           
            try
            {

                return Json(BL.GetScansByTrackIdDispatch(DispatchNote, TrackId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteScannedItem(string details)
        {
            try
            {
                return Json(BL.DeleteScanByTrackIdDispatch(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
