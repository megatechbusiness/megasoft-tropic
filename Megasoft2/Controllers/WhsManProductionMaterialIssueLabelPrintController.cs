using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhsManProductionMaterialIssueLabelPrintController : Controller
    {
        //
        // GET: /WhsManProductionMaterialIssueLabelPrint/
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        private LabelPrint objPrint = new LabelPrint();

        [CustomAuthorize(Activity: "PrintLabel")]
        public ActionResult Index()
        {
            string Username = User.Identity.Name.ToString().ToUpper();
            var Access = (from a in mdb.mtOpFunctions where a.Username == Username && a.ProgramFunction == "Wicket" select a).ToList().FirstOrDefault();
            if (Access != null)
            {
                WhseManProductMatIssueViewModel model = new WhseManProductMatIssueViewModel();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");

                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
                ViewBag.AddField = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobLabelAddField1).ToList().FirstOrDefault();
                //model.Department = Department;
                if (ViewBag.AddField == true)
                {
                    ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == "WICKET" select new { Text = a.WorkCentre, Value = a.WorkCentre }).ToList();
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

        [MultipleButton(Name = "action", Argument = "LoadLotDetails")]
        [HttpPost]
        public ActionResult LoadLotDetails(WhseManProductMatIssueViewModel model)
        {
            ModelState.Clear();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            ViewBag.AddField = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobLabelAddField1).ToList().FirstOrDefault();
            //model.Department = Department;
            if (ViewBag.AddField == true)
            {
                ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == "WICKET" select new { Text = a.WorkCentre, Value = a.WorkCentre }).ToList();
                var Employees = wdb.sp_BaggingLabelEmployees().ToList();
                ViewBag.OperatorList = (from a in Employees where a.ProcessTask == "OPERATOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                ViewBag.PackerList = (from a in Employees where a.ProcessTask == "PACKER" select new { Text = a.Employee, Value = a.Employee }).ToList();
                ViewBag.QcList = (from a in Employees where a.ProcessTask == "QC" select new { Text = a.Employee, Value = a.Employee }).ToList();
                ViewBag.Supervisor = (from a in Employees where a.ProcessTask == "SUPERVISOR" select new { Text = a.Employee, Value = a.Employee }).ToList();

            }
            try
            {
                var result = wdb.mt_ProductionLotDetails(model.Lot).ToList();
                if (result.Count > 0)
                {
                    model.JobDetails = result.FirstOrDefault();

                    var LastBatch = result.FirstOrDefault().ProdQty - (result.FirstOrDefault().BailQty * result.FirstOrDefault().NumberofLabels);
                    if (LastBatch == 0)
                    {
                        LastBatch = result.FirstOrDefault().BailQty;
                    }
                    model.LastBatch = LastBatch;
                }
                else
                {
                    ModelState.AddModelError("", "No Data found for selected Lot number.");
                }


                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }
        [MultipleButton(Name = "action", Argument = "PrintLabel")]
        [HttpPost]
        public ActionResult PrintLabel(WhseManProductMatIssueViewModel model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            string Username = User.Identity.Name.ToString().ToUpper();
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.Printers = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Text = a.PrinterName, Value = a.PrinterName }).ToList();
            ViewBag.AddField = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.JobLabelAddField1).ToList().FirstOrDefault();
            model.Department = "WICKET";
            if (ViewBag.AddField == true)
            {
                ViewBag.WorkCentreList = (from a in wdb.BomWorkCentres where a.CostCentre == "WICKET" select new { Text = a.WorkCentre, Value = a.WorkCentre }).ToList();
                var Employees = wdb.sp_BaggingLabelEmployees().ToList();
                ViewBag.OperatorList = (from a in Employees where a.ProcessTask == "OPERATOR" select new { Text = a.Employee, Value = a.Employee }).ToList();
                ViewBag.PackerList = (from a in Employees where a.ProcessTask == "PACKER" select new { Text = a.Employee, Value = a.Employee }).ToList();
                ViewBag.QcList = (from a in Employees where a.ProcessTask == "QC" select new { Text = a.Employee, Value = a.Employee }).ToList();
                ViewBag.Supervisor = (from a in Employees where a.ProcessTask == "SUPERVISOR" select new { Text = a.Employee, Value = a.Employee }).ToList();

            }
            try
            {
                if (model.JobDetails != null)
                {
                    string Job = model.JobDetails.Job.PadLeft(15, '0');
                    string StockCode = model.JobDetails.StockCode;

                    if (ProgramAccess("AllowOverPrintLabels") == false)
                    {
                        decimal LabelQtyPrinted = Convert.ToDecimal(model.JobDetails.LabelsPrinted);
                        decimal NoOfLabel = Convert.ToDecimal(model.JobDetails.NumberofLabels);
                        decimal LabelOutstanding = Convert.ToDecimal(model.JobDetails.OutstandingLabels);
                        if (NoOfLabel > LabelOutstanding)
                        {
                            return Json("You have exceeded the maximum number of labels printed for Lot : " + model.Lot, JsonRequestBehavior.AllowGet);
                        }
                    }

                    int NextNo = 1;
                    var NextPallet = wdb.sp_GetMaxJobPalletNumber(model.JobDetails.Job.PadLeft(15, '0')).ToList();
                    if (NextPallet.Count > 0)
                    {
                        NextNo = (int)NextPallet.FirstOrDefault().NumberOnly;
                    }


                    List<mtProductionLabel> LabelDetail = new List<mtProductionLabel>();
                    for (int i = 0; i < model.JobDetails.NumberofLabels; i++)
                    {
                        mtProductionLabel obj = new mtProductionLabel();
                        obj.Job = model.JobDetails.Job.PadLeft(15, '0');
                        if (model.JobDetails.BatchQty.ToString().Contains("="))
                        {
                            string[] qty = model.JobDetails.BatchQty.ToString().Split('=');
                            obj.GrossQty = Convert.ToDecimal(qty[1].Trim());
                        }
                        else
                        {
                            obj.GrossQty = Convert.ToDecimal(model.JobDetails.BatchQty);
                        }

                        obj.Core = 0;
                        obj.Tare = 0;

                        obj.NetQty = obj.GrossQty - (obj.Core + obj.Tare);


                        obj.NoOfLabels = Convert.ToInt32(model.JobDetails.NumberofLabels);
                        obj.Username = HttpContext.User.Identity.Name.ToUpper();
                        obj.LabelPrinted = "Y";
                        obj.DatePrinted = DateTime.Now;
                        var setting = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).ToList().FirstOrDefault();
                        if (setting.PalletNoReq == true)
                        {

                            obj.BatchId = model.JobDetails.Job.TrimStart('0') + "-" + setting.DefaultPalletNo + "-" + NextNo.ToString().PadLeft(4, '0');

                            obj.Operator = HttpContext.User.Identity.Name.ToUpper();
                            obj.Packer = HttpContext.User.Identity.Name.ToUpper();

                            //JR 21/09/2021
                            if (!string.IsNullOrEmpty(model.JobDetails.QC1))
                            {
                                if (model.JobDetails.QC1.Contains("-"))
                                {
                                    string[] a = model.JobDetails.QC1.Split('-');
                                    model.JobDetails.QC1 = a[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(model.JobDetails.Supervisor))
                            {
                                if (model.JobDetails.Supervisor.Contains("-"))
                                {
                                    string[] a = model.JobDetails.Supervisor.Split('-');
                                    model.JobDetails.Supervisor = a[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(model.JobDetails.Operator))
                            {
                                if (model.JobDetails.Operator.Contains("-"))
                                {
                                    string[] a = model.JobDetails.Operator.Split('-');
                                    model.JobDetails.Operator = a[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(model.JobDetails.Packer))
                            {
                                if (model.JobDetails.Packer.Contains("-"))
                                {
                                    string[] a = model.JobDetails.Packer.Split('-');
                                    model.JobDetails.Packer = a[0];
                                }
                            }
                            if (!string.IsNullOrEmpty(model.JobDetails.Customer))
                            {
                                if (model.JobDetails.Customer.Contains("-"))
                                {
                                    string[] a = model.JobDetails.Customer.Split('-');
                                    model.JobDetails.Customer = a[0];
                                }
                            }


                        }
                        else
                        {
                            obj.BatchId = model.JobDetails.Job.TrimStart('0') + "-" + NextNo.ToString().PadLeft(4, '0');
                        }

                        var BomCostCentre = wdb.sp_GetProductionBomOperations(Job).ToList().OrderByDescending(a => a.Operation).FirstOrDefault().CostCentre;
                        if (BomCostCentre != null)
                        {
                            if (model.Department.ToUpper() != BomCostCentre)
                            {
                                obj.LabelReceipted = "I";
                            }
                        }

                        obj.Department = model.Department;
                        obj.LotIssued = model.Lot;
                        obj.Customer = model.JobDetails.Customer;
                        obj.Supervisor = model.JobDetails.Supervisor;
                        obj.WorkCentre = model.JobDetails.WorkCentre;
                        obj.Operator = model.JobDetails.Operator;
                        obj.Packer = model.JobDetails.Packer;
                        obj.QC1 = model.JobDetails.QC1;
                        obj.Reference = model.JobDetails.Reference;
                        wdb.mtProductionLabels.Add(obj);
                        wdb.SaveChanges();
                        LabelDetail.Add(obj);
                        NextNo++;
                    }
                    string result = objPrint.PrintJobLabel(LabelDetail, model.Printer, model.JobDetails.BatchQty.ToString(), model.Department, model.LastBatch.ToString());
                    ModelState.AddModelError("", "Label(s) printed.");
                    return View("Index", model);
                }
                else
                {
                    ModelState.AddModelError("", "Error: Unable to Print Label");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        public ActionResult LotSearch(/*string Department*/)
        {
            //Department = "WICKET";
            //ViewBag.Department = Department;
            return PartialView();
        }

        public ActionResult LotList()
        {
            try
            {
                var result = wdb.mt_ProductionMaterialIssueLots();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        public bool ProgramAccess(string ProgramFunction)
        {
            try
            {
                var HasAccess = (from a in mdb.mtOpFunctions where a.Username == HttpContext.User.Identity.Name.ToUpper() && a.Program == "Production" && a.ProgramFunction == ProgramFunction select a).ToList();
                if (HasAccess.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
