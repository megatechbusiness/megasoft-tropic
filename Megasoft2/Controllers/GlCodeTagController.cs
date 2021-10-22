using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class GlCodeTagController : Controller
    {
        //
        // GET: /GlCodeTag/
        SysproEntities sdb = new SysproEntities("");
        MegasoftEntities mdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "TagsEnquiry")]
        public ActionResult Index()
        {
            string User = HttpContext.User.Identity.Name.ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var result = sdb.sp_GetGlCodeTagList(User, Company, "").ToList();
            return View(result);
        }

        public JsonResult GetTagList(string FilterText)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                string User = HttpContext.User.Identity.Name.ToUpper();
                var result = sdb.sp_GetGlCodeTagList(User, Company, FilterText.ToUpper()).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public JsonResult GetDetail(string GlCode, string AnalysisCode)
        {
            try
            {
                var result = sdb.sp_GetGlCodeTagDetail(GlCode, AnalysisCode).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
