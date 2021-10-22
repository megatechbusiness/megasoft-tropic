using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransportSystemPvJournalController : Controller
    {
        //
        // GET: /TransportSystemPvJournal/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();
        TransportSystemBL gl = new TransportSystemBL();

        [CustomAuthorize(Activity: "TransportPvJournal")]
        public ActionResult Index()
        {
            try
            {
                ModelState.Clear();
                TransportSystemPvJournalViewModel model = new TransportSystemPvJournalViewModel();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var result = wdb.sp_GetTransPvJournal(Company).ToList();
                if (result.Count > 0)
                {
                    var Errors = (from a in result where a.ErrorMessage != "" select new { ErrorMessage = a.ErrorMessage, Transporter = a.Transporter }).Distinct().ToList();
                    if(Errors.Count > 0)
                    {
                        foreach(var e in Errors)
                        {
                            ModelState.AddModelError("", e.ErrorMessage);                            
                        }
                        
                    }
                    else
                    {
                        List<sp_GetTransPvJournal_Result> outResult = new List<sp_GetTransPvJournal_Result>();
                        outResult = result.GroupBy(g => g.GLCode).Select(s => new sp_GetTransPvJournal_Result
                            {
                                GLCode = s.First().GLCode,
                                Amount = s.Sum(a => a.Amount)
                            }).ToList();

                        model.Detail = outResult;
                        
                    }
                    model.Year = (int)result.FirstOrDefault().GlYear;
                    model.Month = (int)result.FirstOrDefault().GlPeriod;
                }
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                TransportSystemPvJournalViewModel model = new TransportSystemPvJournalViewModel();
                return View("Index", model);
            }
        }

        [HttpPost]
        public ActionResult Index(TransportSystemPvJournalViewModel model)
        {
            try
            {
                ModelState.Clear();
                if(model.Year == 0)
                {
                    ModelState.AddModelError("", "Please enter a GL Year.");
                    return View("Index", model);
                }

                if (model.Month == 0)
                {
                    ModelState.AddModelError("", "Please enter a GL Month.");
                    return View("Index", model);
                }

                if (string.IsNullOrEmpty(model.Reference))
                {
                    ModelState.AddModelError("", "Please enter a Reference.");
                    return View("Index", model);
                }

                string Guid = sys.SysproLogin();
                if (string.IsNullOrEmpty(Guid))
                {
                    throw new Exception("Failed to login to Syspro!");
                }
                else
                {
                    string XmlOut = sys.SysproPost(Guid, "GENTJL", gl.BuildGlDocument(model), gl.BuildGlParameter());
                    sys.SysproLogoff(Guid);
                    string ErrorMessage = sys.GetXmlErrors(XmlOut);

                    if (!string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        ModelState.AddModelError("", ErrorMessage);
                        return View("Index", model);
                    }
                    else
                    {
                        string Journal = sys.GetFirstXmlValue(XmlOut, "GlJournal");
                        ModelState.AddModelError("", "Posted Successfully. Journal: " + Journal);
                        model.Detail = null;
                        return View("Index", model);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

    }
}
