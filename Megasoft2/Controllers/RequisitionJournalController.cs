using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionJournalController : Controller
    {
        SysproEntities sdb = new SysproEntities("");
        MegasoftEntities db = new MegasoftEntities();
        //
        // GET: /RequisitionJournal/
        [CustomAuthorize(Activity: "JournalUpdate")]
        public ActionResult Index()
        {
            try
            {
                JournalUpdateViewModel result = new JournalUpdateViewModel();
                result.Journals = sdb.sp_GetJournalsToUpdate().ToList();
                return View(result);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpPost]
        public ActionResult Index(JournalUpdateViewModel model)
        {
            try
            {
                ModelState.Clear();
                if(model.Journals.Count() > 0)
                {
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in db.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    foreach(var item in model.Journals)
                    {
                        if(item.UpdateJurnal == true)
                        {
                            //get grn detail
                            //if IssueJournal not blank then update issue journal
                            var MatJournal = (from a in sdb.mtGrnDetails where a.Grn == item.Grn && a.IssueJournal != null select new { IssueJournal = a.IssueJournal, IssueJournalYear = a.IssueJournalYear, IssueJournalMonth = a.IssueJournalMonth }).Distinct().ToList();
                            if(MatJournal.Count > 0)
                            {
                                foreach(var journal in MatJournal)
                                {
                                    if(!string.IsNullOrEmpty(journal.IssueJournal))
                                    {
                                        sdb.f_GenTransStkDesc(Company, Convert.ToInt32(journal.IssueJournalYear), Convert.ToInt32(journal.IssueJournalMonth), Convert.ToInt32(journal.IssueJournal), "IN");
                                    }
                                }
                            }


                            //if journal not blank then update journal
                            var InvJournal = (from a in sdb.mtGrnDetails where a.Grn == item.Grn && a.Journal != null select new { Journal = a.Journal, JournalYear = a.GrnJournalYear, JournalMonth = a.GrnJournalMonth }).Distinct().ToList();
                            if(InvJournal.Count > 0)
                            {
                                foreach(var journal in InvJournal)
                                {
                                    if(!string.IsNullOrEmpty(journal.Journal))
                                    {
                                        sdb.f_GenTransStkDesc(Company, Convert.ToInt32(journal.JournalYear), Convert.ToInt32(journal.JournalMonth), Convert.ToInt32(journal.Journal), "IN");
                                    }
                                }
                            }



                            //if multi vat line then update ap journal
                            var MultiVat = (from a in sdb.mtGrnDetails where a.Grn == item.Grn select a.TaxCode).Distinct().ToList();
                            if(MultiVat.Count > 1)
                            {
                                var ApJournal = (from a in sdb.mtGrnDetails where a.Grn == item.Grn && a.ApJournal != null select new { Journal = a.ApJournal, JournalYear = a.ApJournalYear, JournalMonth = a.ApJournalMonth }).Distinct().ToList();
                                if (ApJournal.Count > 0)
                                {
                                    foreach (var journal in ApJournal)
                                    {
                                        if (!string.IsNullOrEmpty(journal.Journal))
                                        {
                                            sdb.f_GenTransStkDesc(Company, Convert.ToInt32(journal.JournalYear), Convert.ToInt32(journal.JournalMonth), Convert.ToInt32(journal.Journal), "AP");
                                        }
                                    }
                                }
                            }
                            
                            
                            
                            using(var edb = new SysproEntities(""))
                            {
                                var result = (from a in edb.mtGrnDetails where a.Grn == item.Grn select a).ToList();
                                foreach(var re in result)
                                {
                                    re.JournalUpdated = "Y";
                                    edb.Entry(re).State = System.Data.EntityState.Modified;
                                    edb.SaveChanges();
                                }
                            }
                        }
                    }
                    ModelState.AddModelError("", "Journals updated successfully.");
                }
                else
                {
                    ModelState.AddModelError("", "No data found.");
                }
                model.Journals = sdb.sp_GetJournalsToUpdate().ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

    }
}
