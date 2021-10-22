using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;



using Megasoft2.ViewModel;namespace Megasoft2.Controllers
{
    public class RequisitionAuditController : Controller
    {
        //
        // GET: /RequisitionAudit/
        SysproEntities sdb = new SysproEntities("");
        [CustomAuthorize(Activity: "RequisitionAudit")]
        public ActionResult Index()
        {
            try
            {
                RequisitionAuditsViewModel AU = new RequisitionAuditsViewModel();
                AU.Audit = sdb.sp_GetRequisitionAudit(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")), Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),"","").Take(100).ToList();
                AU.FromDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                AU.ToDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                return View(AU);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
        [HttpPost]
        [CustomAuthorize(Activity: "RequisitionAudit")]
        public ActionResult Index( RequisitionAuditsViewModel model)
        {
                try
                {                    

                    if (model.ToDate < model.FromDate)
                    {
                        ModelState.AddModelError("", "To Date cannot be before From Date.");
                        model.Audit.Clear();
                        return View("Index", model);
                    }
                    
                    if (model.FilterText == null) { model.FilterText = ""; }
                    if (model.FilterText2 == null) { model.FilterText2 = ""; }
                    model.Audit = sdb.sp_GetRequisitionAudit(Convert.ToDateTime(model.FromDate.ToString("yyyy-MM-dd")), Convert.ToDateTime(model.ToDate.ToString("yyyy-MM-dd")), model.FilterText.ToUpper(), model.FilterText2.ToUpper()).Take(100).ToList();
                    model.FromDate = Convert.ToDateTime(model.FromDate.ToString("yyyy-MM-dd"));
                    model.ToDate = Convert.ToDateTime(model.ToDate.ToString("yyyy-MM-dd"));
                 

                    return View("Index", model);

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View();
                }
           
            }

        } 

    }

