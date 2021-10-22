using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionAuditTrailController : Controller
    {
        //
        // GET: /RequisitionAuditTrail/
        SysproEntities sdb = new SysproEntities("");

        [CustomAuthorize(Activity: "AuditTrail")]
        public ActionResult Index()
        {
            var result = sdb.sp_GetAuditTrail("").Take(100).ToList();
            return View(result);
        }

        public JsonResult GetAudit(string FilterText)
        {
            try
            {
                if(FilterText == "")
                {
                    var result = sdb.sp_GetAuditTrail(FilterText.ToUpper()).Take(100).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = sdb.sp_GetAuditTrail(FilterText.ToUpper()).ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
