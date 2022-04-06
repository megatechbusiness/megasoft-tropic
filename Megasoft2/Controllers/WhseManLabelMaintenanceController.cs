using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManLabelMaintenanceController : Controller
    {
        //
        // GET: /WhseManLabelMaintenance/
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        private LabelPrint objPrint = new LabelPrint();

        [CustomAuthorize(Activity: "JobLabelMaintenance")]
        public ActionResult Index()
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            var DeptList = (from a in wdb.sp_GetUserDepartments(Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
            ViewBag.DepartmentList = DeptList;
            return View();
        }

        public JsonResult GetGridData(string Job)
        {
            try
            {
                Job = Job.PadLeft(15, '0');
                var result = wdb.sp_GetLabelsPrintedForJob(Job).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "DeleteJobLabel")]
        public ActionResult DeletePallet(string details)
        {
            try
            {
                List<WhseManJobReceipt> detail = (List<WhseManJobReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WhseManJobReceipt>));
                if (detail.Count > 0)
                {
                    string Job = detail.FirstOrDefault().Job.PadLeft(15, '0');
                    string Pallet = detail.FirstOrDefault().Lot;
                    wdb.sp_DeleteProductionLabel(Job, Pallet, HttpContext.User.Identity.Name.ToUpper());
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No data found.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult PrintLabel(string details)
        {
            try
            {
                List<ProductionLabel> detail = (List<ProductionLabel>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<ProductionLabel>));

                if (detail.Count > 0)
                {
                    string Job = detail.FirstOrDefault().Job.PadLeft(15, '0');

                    List<mtProductionLabel> LabelDetail = new List<mtProductionLabel>();
                    foreach (var item in detail)
                    {
                        var result = (from a in wdb.mtProductionLabels where a.Job == Job && a.BatchId == item.BatchId select a).ToList();
                        mtProductionLabel obj = new mtProductionLabel();
                        obj.Job = Job;
                        obj.Core = result.FirstOrDefault().Core;
                        obj.GrossQty = result.FirstOrDefault().GrossQty;
                        obj.NetQty = Convert.ToDecimal(item.BatchQty);
                        obj.NoOfLabels = result.FirstOrDefault().NoOfLabels;
                        obj.Username = HttpContext.User.Identity.Name.ToUpper();
                        obj.LabelPrinted = "Y";
                        obj.DatePrinted = DateTime.Now;
                        var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                        if (setting.PalletNoReq == true)
                        {
                            obj.BatchId = result.FirstOrDefault().BatchId;
                            obj.Customer = result.FirstOrDefault().Customer;
                            obj.Supervisor = result.FirstOrDefault().Supervisor;
                            obj.WorkCentre = result.FirstOrDefault().WorkCentre;
                            obj.Operator = result.FirstOrDefault().Operator;
                            obj.Packer = result.FirstOrDefault().Packer;
                            obj.QC1 = result.FirstOrDefault().QC1;
                            obj.Reference = result.FirstOrDefault().Reference;
                        }
                        else
                        {
                            obj.BatchId = result.FirstOrDefault().BatchId;
                        }

                        obj.LotIssued = result.FirstOrDefault().LotIssued;
                        LabelDetail.Add(obj);

                        var Traceable = (from Z in wdb.WipMasters where Z.Job == Job && Z.TraceableType != "T" select Z).ToList();
                        if (Traceable.Count > 0)
                        {
                            mtProductionLabel l = new mtProductionLabel();
                            l = wdb.mtProductionLabels.Find(Job, item.BatchId);
                            l.NetQty = Convert.ToDecimal(item.BatchQty);
                            l.GrossQty = Convert.ToDecimal(item.BatchQty);
                            wdb.Entry(l).State = EntityState.Modified;
                            wdb.SaveChanges();
                        }
                    }
                    if (detail.FirstOrDefault().Department == "WICKET" || detail.FirstOrDefault().Department == "BAG")
                    {
                        return Json(objPrint.ReprintPrintJobLabel(LabelDetail, detail.FirstOrDefault().Printer, detail.FirstOrDefault().Department), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        string printresult = objPrint.PrintScaleLabel(LabelDetail, detail.FirstOrDefault().Printer, detail.FirstOrDefault().Department);
                        if (string.IsNullOrWhiteSpace(printresult))
                        {
                            return Json("Label(s) printed.", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(printresult, JsonRequestBehavior.AllowGet);
                        }

                    }

                }
                else
                {
                    return Json("No data found.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}