using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionPvJournalController : Controller
    {
        //
        // GET: /RequisitionPvJournal/
        [CustomAuthorize(Activity: "ProvisionalJournal")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string Extract = null)
        {
            try
            {
                var result = new List<sp_GetProvGlJournal_Result>();
                using (var db = new SysproEntities(""))
                {
                    result = db.sp_GetProvGlJournal().ToList();
                }

                if (result.Count > 0)
                {
                    StringBuilder Document = new StringBuilder();
                    Document.AppendLine(";SYSPRO IMPORT - Version=001 - GENP13");
                    foreach (var line in result)
                    {
                        Document.AppendLine(line.EntryDet);
                    }
                    var byteArray = Encoding.ASCII.GetBytes(Document.ToString());
                    var stream = new MemoryStream(byteArray);

                    return File(stream, "text/plain", HttpContext.User.Identity.Name.ToUpper() + "_" + DateTime.Now.ToShortDateString() + "_" + "PVJournal.txt");
                }
                else
                {
                    ModelState.AddModelError("", "No data found!");
                    return View();
                }
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

    }
}
