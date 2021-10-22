using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManProductionLabelController : Controller
    {
        //
        // GET: /WhseManProductionLabel/
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        private MegasoftEntities mdb = new MegasoftEntities();
        private LabelPrint objPrint = new LabelPrint();

        [CustomAuthorize(Activity: "PrintLabel")]
        public ActionResult Index(string Department)
        {
            try
            {
                if (Department == null)
                {
                    WhseManProductionLabelPrint model = new WhseManProductionLabelPrint();
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    string Username = User.Identity.Name.ToString().ToUpper();
                    var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                    ViewBag.AddField = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobLabelAddField1).ToList().FirstOrDefault();
                    model.Department = "";
                    if (ViewBag.AddField == true)
                    {
                        ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == "BAG" select new { Text = a.WorkCentre, Value = a.WorkCentre }).ToList();
                        var Employees = wdb.sp_BaggingLabelEmployees().ToList();
                        ViewBag.OperatorList = (from a in Employees where a.ProcessTask == "OPERATOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                        ViewBag.PackerList = (from a in Employees where a.ProcessTask == "PACKER" select new { Text = a.Employee, Value = a.Employee }).ToList();
                        ViewBag.QcList = (from a in Employees where a.ProcessTask == "QC" select new { Text = a.Employee, Value = a.Employee }).ToList();
                        ViewBag.Supervisor = (from a in Employees where a.ProcessTask == "SUPERVISOR" select new { Text = a.Employee, Value = a.Employee }).ToList();

                    }
                    return View(model);
                }
                else
                {
                    string Username = HttpContext.User.Identity.Name.ToString().ToUpper();
                    var Access = (from a in mdb.mtOpFunctions where a.Username == Username && a.ProgramFunction == Department select a).ToList().FirstOrDefault();
                    if (Access != null)
                    {
                        WhseManProductionLabelPrint model = new WhseManProductionLabelPrint();
                        HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                        var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                        ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                        ViewBag.AddField = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobLabelAddField1).ToList().FirstOrDefault();
                        model.Department = Department;
                        if (ViewBag.AddField == true)
                        {
                            ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == "BAG" select new { Text = a.WorkCentre, Value = a.WorkCentre }).ToList();
                            var Employees = wdb.sp_BaggingLabelEmployees().ToList();
                            ViewBag.OperatorList = (from a in Employees where a.ProcessTask == "OPERATOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                            ViewBag.PackerList = (from a in Employees where a.ProcessTask == "PACKER" select new { Text = a.Employee, Value = a.Employee }).ToList();
                            ViewBag.QcList = (from a in Employees where a.ProcessTask == "QC" select new { Text = a.Employee, Value = a.Employee }).ToList();
                            ViewBag.Supervisor = (from a in Employees where a.ProcessTask == "SUPERVISOR" select new { Text = a.Employee, Value = a.Employee }).ToList();

                        }
                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("Index", "AccessDenied");
                    }
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpPost]
        public ActionResult Index(WhseManProductionLabelPrint model)
        {
            try
            {
                ModelState.Clear();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                string Username = User.Identity.Name.ToString().ToUpper();
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                ViewBag.AddField = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobLabelAddField1).ToList().FirstOrDefault();
                if (ViewBag.AddField == true)
                {
                    ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == "BAG" select new { Text = a.WorkCentre, Value = a.WorkCentre }).ToList();
                    var Employees = wdb.sp_BaggingLabelEmployees().ToList();
                    ViewBag.OperatorList = (from a in Employees where a.ProcessTask == "OPERATOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                    ViewBag.PackerList = (from a in Employees where a.ProcessTask == "PACKER" select new { Text = a.Employee, Value = a.Employee }).ToList();
                    ViewBag.QcList = (from a in Employees where a.ProcessTask == "QC" select new { Text = a.Employee, Value = a.Employee }).ToList();
                    ViewBag.Supervisor = (from a in Employees where a.ProcessTask == "SUPERVISOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                }
                var CheckJobCostCentre = wdb.sp_GetProductionJobs(model.Department.ToUpper()).ToList();
                if (CheckJobCostCentre.Count > 0)
                {
                    var check = (from c in CheckJobCostCentre where c.Job == model.JobDetails.Job.PadLeft(15, '0') select c).ToList();
                    if (check.Count > 0)
                    {
                        var result = wdb.sp_GetProductionJobDetails(model.JobDetails.Job.PadLeft(15, '0')).FirstOrDefault();
                        if (result != null)
                        {

                            //var LabelModel = new WhseManProductionLabelPrint();
                            model.JobDetails = result;
                            return View(model);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Job Details not found.");
                            return View(model);
                        }

                    }
                    else
                    {
                        ModelState.AddModelError("", "Job not found.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Job not found.");
                    return View(model);
                }


            }
            catch (Exception ex)
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                string Username = User.Identity.Name.ToString().ToUpper();
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                ViewBag.AddField = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobLabelAddField1).ToList().FirstOrDefault();
                if (ViewBag.AddField == true)
                {
                    ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == "BAG" select new { Text = a.WorkCentre, Value = a.WorkCentre }).ToList();
                    var Employees = wdb.sp_BaggingLabelEmployees().ToList();
                    ViewBag.OperatorList = (from a in Employees where a.ProcessTask == "OPERATOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                    ViewBag.PackerList = (from a in Employees where a.ProcessTask == "PACKER" select new { Text = a.Employee, Value = a.Employee }).ToList();
                    ViewBag.QcList = (from a in Employees where a.ProcessTask == "QC" select new { Text = a.Employee, Value = a.Employee }).ToList();
                    ViewBag.Supervisor = (from a in Employees where a.ProcessTask == "SUPERVISOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                }
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [CustomAuthorize(Activity: "PrintLabel")]
        public ActionResult PrintLabel(string details)
        {
            try
            {
                List<ProductionLabel> detail = (List<ProductionLabel>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<ProductionLabel>));
                if (detail.Count > 0)
                {
                    string Job = detail.FirstOrDefault().Job.PadLeft(15, '0');
                    string StockCode = detail.FirstOrDefault().StockCode;
                    var StockUom = (from a in wdb.InvMasters where a.StockCode == StockCode select a.StockUom).ToList().FirstOrDefault();
                    decimal QtyMan = Convert.ToDecimal(detail.FirstOrDefault().QtyManufactured);
                    decimal QtyProd = Convert.ToDecimal(detail.FirstOrDefault().ProductionQty);
                    decimal QtyToMake = Convert.ToDecimal(detail.FirstOrDefault().QtyToMake);
                    string LastBatchQty = "";
                    var Tolerance = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobReceiptTolerance).ToList().FirstOrDefault();
                    if (StockUom == "TH")
                    {
                        QtyProd = QtyProd / 1000;
                    }
                    decimal TotalManufactured = QtyProd + QtyMan;
                    decimal TotalManufacturedPlusTolerance = 0;
                    if (Tolerance > 0)
                    {
                        TotalManufacturedPlusTolerance = QtyToMake + (QtyToMake * (Convert.ToDecimal(Tolerance) / 100));
                    }
                    else
                    {
                        TotalManufacturedPlusTolerance = QtyToMake;
                    }
                    if (TotalManufactured > TotalManufacturedPlusTolerance)
                    {
                        return Json("Label Cannot be printed for quantity produced.", JsonRequestBehavior.AllowGet);
                    }
                    int NextNo = 1;
                    var NextPallet = wdb.sp_GetMaxJobPalletNumber(detail.FirstOrDefault().Job.PadLeft(15, '0')).ToList();
                    if (NextPallet.Count > 0)
                    {
                        NextNo = (int)NextPallet.FirstOrDefault().NumberOnly;
                    }


                    List<mtProductionLabel> LabelDetail = new List<mtProductionLabel>();
                    for (int i = 0; i < detail.FirstOrDefault().NoOfLabels; i++)
                    {
                        mtProductionLabel obj = new mtProductionLabel();
                        obj.Job = detail.FirstOrDefault().Job.PadLeft(15, '0');
                        //obj.ProductionQty = detail.FirstOrDefault().ProductionQty;
                        if (detail.FirstOrDefault().BatchQty.Contains("="))
                        {
                            string[] qty = detail.FirstOrDefault().BatchQty.Split('=');
                            obj.GrossQty = Convert.ToDecimal(qty[1].Trim());
                        }
                        else
                        {
                            obj.GrossQty = Convert.ToDecimal(detail.FirstOrDefault().BatchQty);
                        }

                        if (i == (detail.FirstOrDefault().NoOfLabels - 1))
                        {
                            if (obj.GrossQty != detail.FirstOrDefault().LastBatch)
                            {
                                LastBatchQty = detail.FirstOrDefault().LastBatch.ToString();
                            }
                            obj.GrossQty = detail.FirstOrDefault().LastBatch;

                        }

                        //Bagging and Wicketting - No Core or Tare therefore NetQty = GrossQty - (Core + Tare)
                        obj.Core = 0;
                        obj.Tare = 0;

                        obj.NetQty = obj.GrossQty - (obj.Core + obj.Tare);


                        obj.NoOfLabels = detail.FirstOrDefault().NoOfLabels;
                        obj.Username = HttpContext.User.Identity.Name.ToUpper();
                        obj.LabelPrinted = "Y";
                        obj.DatePrinted = DateTime.Now;
                        var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                        if (setting.PalletNoReq == true)
                        {
                            if (!string.IsNullOrEmpty(detail.FirstOrDefault().QC1))
                            {
                                if (detail.FirstOrDefault().QC1.Contains("-"))
                                {
                                    string[] a = detail.FirstOrDefault().QC1.Split('-');
                                    detail.FirstOrDefault().QC1 = a[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(detail.FirstOrDefault().Supervisor))
                            {
                                if (detail.FirstOrDefault().Supervisor.Contains("-"))
                                {
                                    string[] a = detail.FirstOrDefault().Supervisor.Split('-');
                                    detail.FirstOrDefault().Supervisor = a[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(detail.FirstOrDefault().Operator))
                            {
                                if (detail.FirstOrDefault().Operator.Contains("-"))
                                {
                                    string[] a = detail.FirstOrDefault().Operator.Split('-');
                                    detail.FirstOrDefault().Operator = a[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(detail.FirstOrDefault().Packer))
                            {
                                if (detail.FirstOrDefault().Packer.Contains("-"))
                                {
                                    string[] a = detail.FirstOrDefault().Packer.Split('-');
                                    detail.FirstOrDefault().Packer = a[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(detail.FirstOrDefault().Customer))
                            {
                                if (detail.FirstOrDefault().Customer.Contains("-"))
                                {
                                    string[] a = detail.FirstOrDefault().Customer.Split('-');
                                    detail.FirstOrDefault().Customer = a[0];
                                }
                            }
                            obj.BatchId = detail.FirstOrDefault().Job.TrimStart('0') + "-" + setting.DefaultPalletNo + "-" + NextNo.ToString().PadLeft(4, '0');
                            obj.Customer = detail.FirstOrDefault().Customer;
                            obj.Supervisor = detail.FirstOrDefault().Supervisor;
                            obj.WorkCentre = detail.FirstOrDefault().WorkCentre;
                            obj.Operator = detail.FirstOrDefault().Operator;
                            obj.Packer = detail.FirstOrDefault().Packer;
                            obj.QC1 = detail.FirstOrDefault().QC1;
                            obj.Reference = detail.FirstOrDefault().Reference;
                        }
                        else
                        {
                            obj.BatchId = detail.FirstOrDefault().Job.TrimStart('0') + "-" + NextNo.ToString().PadLeft(4, '0');
                        }

                        //Check if Label Produced is for final operation.
                        //If it is not final operation we will not receipt this label as it will be receipted during final operation.
                        //Check last operation in BOM and check if the Cost Centre is the same. i.e. Department = Bagging.
                        //Check if the last operation on the BOM belongs to the Bagging operation. If it does NOT then we updated the receipted flag to "I".
                        var BomCostCentre = wdb.sp_GetProductionBomOperations(Job).ToList().OrderByDescending(a => a.Operation).FirstOrDefault().CostCentre;
                        if (BomCostCentre != null)
                        {
                            if (detail.FirstOrDefault().Department.ToUpper() != BomCostCentre)
                            {
                                obj.LabelReceipted = "I";
                            }
                        }

                        obj.Department = detail.FirstOrDefault().Department;



                        wdb.mtProductionLabels.Add(obj);
                        wdb.SaveChanges();
                        LabelDetail.Add(obj);

                        //obj.PalletQty
                        NextNo++;
                    }
                    string result = objPrint.PrintJobLabel(LabelDetail, detail.FirstOrDefault().Printer, detail.FirstOrDefault().BatchQty, detail.FirstOrDefault().Department, LastBatchQty);

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                return Json("No data found.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ViewBag.AddField = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobLabelAddField1).ToList().FirstOrDefault();
                if (ViewBag.AddField == true)
                {
                    ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == "BAG" select new { Text = a.WorkCentre, Value = a.WorkCentre }).ToList();
                    var Employees = wdb.sp_BaggingLabelEmployees().ToList();
                    ViewBag.OperatorList = (from a in Employees where a.ProcessTask == "OPERATOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                    ViewBag.PackerList = (from a in Employees where a.ProcessTask == "PACKER" select new { Text = a.Employee, Value = a.Employee }).ToList();
                    ViewBag.QcList = (from a in Employees where a.ProcessTask == "QC" select new { Text = a.Employee, Value = a.Employee }).ToList();
                }
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult JobSearch(string Department)
        {
            ViewBag.Department = Department;
            return PartialView();
        }
        public ActionResult JobList(string FilterText)
        {
            try
            {
                if (FilterText == null)
                {
                    FilterText = "";
                }
                //This filter only filters on job number
                var result = wdb.sp_GetProductionJobs(FilterText.ToUpper());
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }



        public ActionResult ProductionReporting()
        {
            return View();
        }
    }
}