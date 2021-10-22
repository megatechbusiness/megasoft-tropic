using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RequisitionSettingsController : Controller
    {
        private MegasoftEntities mdb = new MegasoftEntities();
        private SysproEntities sdb = new SysproEntities("");

        [CustomAuthorize(Activity: "Configuration")]
        public ActionResult Index( )
        {
            RequisitionSettings model = new RequisitionSettings();
            try
            {
                model.Settings = (from a in sdb.mtRequisitionSettings  select a).FirstOrDefault();
                model.Global = (from a in mdb.mtSystemSettings select a).FirstOrDefault();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);

            }
        }
        
        //
        // POST: /Requisition Settings/Edit
        [CustomAuthorize(Activity: "")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index( RequisitionSettings model)
        {
            try{
                        //Update Local Settings
                        mtRequisitionSetting objReqSet = new mtRequisitionSetting();
                        objReqSet.SettingId = model.Settings.SettingId;
                        objReqSet.OneQuoteMinValue = model.Settings.OneQuoteMinValue;
                        objReqSet.OneQuoteMaxValue = model.Settings.OneQuoteMaxValue;
                        objReqSet.TwoQuoteMinValue = model.Settings.TwoQuoteMinValue;
                        objReqSet.TwoQuoteMaxValue = model.Settings.TwoQuoteMaxValue;
                        objReqSet.ThreeQuoteMinValue = model.Settings.ThreeQuoteMinValue;
                        objReqSet.RequestForQuoteOutputPath = model.Settings.RequestForQuoteOutputPath;
                        objReqSet.InvoiceReviewLimit = model.Settings.InvoiceReviewLimit;
                        objReqSet.ExemptTaxCode = model.Settings.ExemptTaxCode;
                        objReqSet.LastGrn = model.Settings.LastGrn;
                        objReqSet.NonStockedControlAccount = model.Settings.NonStockedControlAccount;
                        objReqSet.GrnClearingAccount = model.Settings.GrnClearingAccount; 
                        sdb.Entry(objReqSet).State = EntityState.Modified;
                        sdb.SaveChanges();

                        //Update Global Settings
                        mtSystemSetting objSysSet = new mtSystemSetting();
                        objSysSet.Id = model.Global.Id;
                        objSysSet.UseRoles = model.Global.UseRoles;
                        objSysSet.SmtpHost = model.Global.SmtpHost;
                        objSysSet.SmtpPort = model.Global.SmtpPort;
                        objSysSet.FromAddress = model.Global.FromAddress;
                        objSysSet.ReportExportPath = model.Global.ReportExportPath;
                        objSysSet.MegasoftServiceIntervalMin = model.Global.MegasoftServiceIntervalMin;
                        objSysSet.Dashboard = model.Global.Dashboard;
                        objSysSet.BranchAccess = model.Global.BranchAccess;
                        objSysSet.GlAuthorisation = model.Global.GlAuthorisation;
                        objSysSet.ProductClassLimit = model.Global.ProductClassLimit;
                        mdb.Entry(objSysSet).State = EntityState.Modified;
                        mdb.SaveChanges();
                        //Output Saved Successfully
                        ModelState.AddModelError("", "Saved Succesfully");                   
                }
            
            catch (Exception ex)
                {
                        ModelState.AddModelError("", ex.Message);
                        return View(model);
                }
                        return View(model);
        }
        public JsonResult GlList()
        {           
            var result = sdb.sp_GetGlCodes();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GlSearch(string GlCodeId)
        {
            @ViewBag.GlCodeId = GlCodeId;
            return PartialView();
        }       
  }
          
}
