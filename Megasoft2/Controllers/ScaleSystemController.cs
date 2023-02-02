using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class ScaleSystemController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        private LabelPrint BL = new LabelPrint();
        private WhseManProductionReceipt receipt = new WhseManProductionReceipt();
        private SysproCore sys = new SysproCore();
        //
        // GET: /ScaleSystem/
        [CustomAuthorize(Activity: "ScaleReceipt")]
        public ActionResult Index()
        {
            ScaleSystemViewModel model = new ScaleSystemViewModel();
            string Username = User.Identity.Name.ToString().ToUpper(); 
            ViewBag.OperatorList = new List<SelectListItem>();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
            ViewBag.Scales = scale;
            ViewBag.DepartmentList = new List<SelectListItem>();
            ViewBag.PalletLoaded = false;

            return View(model);
        }


        public JsonResult GetOperator(string Department,string Job)
        {
            Job = Job.PadLeft(15, '0');
            string Username = User.Identity.Name.ToString().ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
            ViewBag.Scales = scale;

            var DeptList = (from a in wdb.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
            ViewBag.DepartmentList = DeptList;

            if (Department == "PRINT")
            {
                var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                ViewBag.OperatorList = OprList;
            }
            else if (Department == "EXTR")
            {
                var OprList = (from a in wdb.sp_GetScaleOperators("EXTR").ToList() select new { Value = a.Employee + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                ViewBag.OperatorList = OprList;
            }
            else
            {
                ViewBag.OperatorList = new List<SelectListItem>();
            }

            return Json(ViewBag.OperatorList,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadJob")]
        public ActionResult LoadJob(ScaleSystemViewModel model)
        {
            try
            {
                ModelState.Clear();
                string Job = model.Job.PadLeft(15, '0');
                string Username = User.Identity.Name.ToString().ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
                ViewBag.Scales = scale;

                var DeptList = (from a in wdb.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
                ViewBag.DepartmentList = DeptList;
                if (DeptList.Count > 0)
                {
                    var print = (from a in DeptList where a.Text.Contains("PRINT") select a.Text).FirstOrDefault();
                    model.Department = print;

                    if (model.Department == "PRINT" || model.Department == "PRINTING")
                    {
                        var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                        ViewBag.OperatorList = OprList;
                    }
                    else if (model.Department == "EXTR")
                    {
                        var OprList = (from a in wdb.sp_GetScaleOperators("EXTR").ToList() select new { Value = a.Employee + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                        ViewBag.OperatorList = OprList;
                    }
                    else
                    {
                        ViewBag.OperatorList = new List<SelectListItem>();
                    }
                }
                else
                {
                    ViewBag.OperatorList = new List<SelectListItem>();
                    ModelState.AddModelError("", "No Departments found.");
                    ViewBag.PalletLoaded = false;
                    return View("Index", model);
                }


                var Header = wdb.sp_GetScalesJobDetails(Job).ToList().FirstOrDefault();
                var settings = (from a in wdb.mtScales where a.ScaleModelId == model.Scale select a).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(settings.Warehouse))
                {
                    if (Header.Warehouse != settings.Warehouse)
                    {
                        var WarehouseCheck = (from a in wdb.InvWarehouses.AsNoTracking() where a.Warehouse == settings.Warehouse && a.StockCode == Header.StockCode select a).ToList();
                        if (WarehouseCheck.Count == 0)
                        {
                            ModelState.AddModelError("", "StockCode : " + Header.StockCode + " not stocked in Warehouse : " + settings.Warehouse);
                            ViewBag.PalletLoaded = false;
                            return View("Index", model);
                        }
                    }
                }



                if (Header != null)
                {
                    model.JobDescription = Header.JobDescription;
                    model.StockCode = Header.StockCode;
                    model.StockDescription = Header.StockDescription;
                    model.Customer = Header.Customer;
                    model.CustStockCode = Header.CustStockCode;
                    model.QtyOutstanding = (decimal)Header.QtyOutstanding;
                    model.QtyManufactured = (decimal)Header.QtyManufactured;
                    model.QtyToMake = (decimal)Header.QtyToMake;
                    model.Barcode = Header.Barcode;

                    var OpenPallets = wdb.mtPalletControls.Where(x => x.Status != "C" && x.Job == Job && x.ScaleId == model.Scale).ToList();
                    if (OpenPallets.Count > 1)
                    {
                        ViewBag.MultiPalletNo = (from a in OpenPallets select new { Value = a.PalletNo, Text = a.PalletNo }).ToList();
                    }

                    var Lines = wdb.sp_GetScaleRolls(OpenPallets.Min(x => x.PalletNo)).ToList();
                    if (Lines.Count > 0)
                    {
                        //existing pallet not receipted
                        model.Rolls = Lines;
                        model.Pallet = Lines.FirstOrDefault().PalletNo.ToString();
                        model.Gross = model.Rolls.Sum(a => a.GrossMass).Value;
                        model.Net = model.Rolls.Sum(a => a.NetMass).Value;
                        model.Core = (decimal)Lines.FirstOrDefault().Core;
                        model.Tare = (decimal)Lines.FirstOrDefault().Tare;
                    }
                    else
                    {
                        //check if theres a pallet open but no reels
                        var Search = (from c in wdb.mtPalletControls where c.Job == Job && c.ScaleId == model.Scale && c.Status != "C" select c).ToList();
                        if (Search.Count > 0)
                        {
                            model.Pallet = Search.FirstOrDefault().PalletNo;
                        }
                        else
                        {
                            //create new pallet
                            var NewPallet = wdb.sp_GetScalesMaxJobPalletNumber(Job, model.Scale).FirstOrDefault();
                            int PalletNumberOnly;
                            if (NewPallet == null)
                            {
                                PalletNumberOnly = 1;
                            }
                            else
                            {
                                PalletNumberOnly = (int)NewPallet.NumberOnly;
                            }
                            mtPalletControl control = new mtPalletControl();
                            string PalletNo = Job.TrimStart(new Char[] { '0' }) + "-" + PalletNumberOnly.ToString().PadLeft(3, '0');
                            control.PalletNo = PalletNo;
                            control.Job = Job;
                            control.PalletSeq = PalletNumberOnly;
                            control.Status = "O";
                            control.ScaleId = model.Scale;
                            wdb.Entry(control).State = System.Data.EntityState.Added;
                            wdb.SaveChanges();
                            model.Pallet = PalletNo;
                        }
                        //Changed to Setting per scale - mtScales Table.
                        //var settings = (from c in wdb.mtScaleSettings where c.SettingId == 1 select c).FirstOrDefault();
                        //model.Core = (decimal)settings.Core;
                        //model.Tare = (decimal)settings.Tare;

                        model.Core = (decimal)settings.Core;
                        model.Tare = (decimal)settings.Tare;



                    }
                    ViewBag.PalletLoaded = true;
                    return View("Index", model);
                }
                else
                {
                    ViewBag.PalletLoaded = false;
                    ModelState.AddModelError("", "Job not found.");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error getting Job Details: " + ex.Message.ToString());
                ViewBag.PalletLoaded = false;
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AddLine")]
        public ActionResult AddLine(ScaleSystemViewModel model)
        {
            ModelState.Clear();
            List<mtProductionLabel> LabelDetail = new List<mtProductionLabel>();
            string Job = model.Job.PadLeft(15, '0');
            ScaleSystemViewModel s = model;
            //string PrintOp = model.PrintOperator.Split('-')[0];
            if (model.Department=="PRINT")
            {
                var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                ViewBag.OperatorList = OprList;
            }
            else
            {
                ViewBag.OperatorList = new List<SelectListItem>();
            }
            var PrinterName = (from a in wdb.mtScales where a.ScaleModelId == model.Scale select a.PrinterName).FirstOrDefault();
            if (string.IsNullOrEmpty(PrinterName))
            {
                ModelState.AddModelError("", "No printer specified for scale. Please contact administrator.");
            }
            else
            {
                if (!string.IsNullOrEmpty(model.Department))
                {
                    try
                    {
                        var counter = model.Department == "SLIT"? model.NoOfRolls: 1; 
                        for (int j = 0; j < counter; j++)
                        {
                            string Pallet = model.Pallet;
                            var MaxReel = 1;
                            var reels = (from a in wdb.mtProductionLabels where a.Job == Job && a.PalletNo == Pallet select a).ToList();
                            if (reels.Count > 0)
                            {
                                MaxReel = (int)reels.Max(b => b.BatchSequence) + 1;
                            }
                            string BatchIdToCheck = model.Pallet + "-" + MaxReel.ToString().PadLeft(4, '0');

                            //CODE WILL CHECK DELETED LABEL TABLE TO ENSURE DUPLICATE LABEL ISNT GENERATED 17/05/2019 SYLVAIN
                            var AllDeletedBatchIdsForJob = (from a in wdb.mtProductionLabelsDeleteds where a.Job == Job select a).ToList();
                            var CheckIfDeleted = (from a in AllDeletedBatchIdsForJob where a.BatchId == BatchIdToCheck select a).ToList();

                            while (CheckIfDeleted.Count > 0)
                            {
                                MaxReel = MaxReel + 1;
                                BatchIdToCheck = model.Pallet + "-" + MaxReel.ToString().PadLeft(4, '0');
                                CheckIfDeleted = (from a in AllDeletedBatchIdsForJob where a.BatchId == BatchIdToCheck select a).ToList();
                            }

                            mtProductionLabel obj = new mtProductionLabel();
                            obj.Job = Job;
                            if (model.Department == "EXTR" || model.Department == "SLIT" || model.Department == "SCALE")
                            {
                                obj.NetQty = model.Weight - (model.Core);
                            }
                            else
                            {
                                obj.NetQty = model.Weight - (model.Core + model.Tare);
                            }
                            //14082020 - Changed to look at Uom
                            //var Header = wdb.sp_GetScalesJobDetails(Job).ToList().FirstOrDefault();
                            //if (Header.StockUom.Substring(0, 1).ToUpper() == "K")
                            //{
                            //    obj.NetQty = model.Weight - (model.Core);
                            //}
                            //else
                            //{
                            //    obj.NetQty = model.Weight - (model.Core + model.Tare);
                            //}

                            if (model.Department == "SLIT")
                            {
                                obj.Meters = model.Meters;
                                obj.ParentRoll = model.ParentRoll;
                                obj.NoOfRolls = model.NoOfRolls;
                            }
                            obj.GrossQty = model.Weight;
                            obj.Username = HttpContext.User.Identity.Name.ToUpper();
                            obj.LabelPrinted = "Y";
                            obj.DatePrinted = DateTime.Now;
                            obj.BatchId = BatchIdToCheck;
                            obj.Customer = model.Customer;
                            obj.Core = model.Core;
                            obj.Tare = model.Tare;
                            obj.PalletNo = model.Pallet;
                            obj.BatchSequence = MaxReel;
                            obj.Department = model.Department;
                            string Initials = "";
                            DateTime printTime = Convert.ToDateTime(DateTime.Now.ToString("hh:mm tt"));
                            //DateTime Date = Convert.ToDateTime(DateTime.Now.ToString("dd-MM-yy"));
                            //var x = DateTime.Parse("00:00");
                            if (model.Department=="PRINT"|| model.Department=="PRINTING")
                            {
                                var BomInitial = wdb.sp_GetPrintScaleOperatorInitial(model.Operator).FirstOrDefault();
                                if (!string.IsNullOrWhiteSpace(BomInitial.Initial))
                                {
                                    Initials = BomInitial.Initial;
                                }
                                else
                                {
                                    for (int i = 1; i < model.Operator.Length - 1; i++)
                                    {
                                        if (model.Operator[i] == ' ')
                                        {
                                            Initials = "" + Char.ToUpper(model.Operator[0]) + Char.ToUpper(model.Operator[i + 1]);
                                        }
                                    }
                                }
                            
                                DateTime hourOne = DateTime.Parse("00:00 AM");
                                DateTime hourTwo = DateTime.Parse("06:00 AM");
                                if (printTime>=hourOne && printTime<= hourTwo)
                                {
                                    printTime = printTime.AddDays(-1);
                                }
                                model.PrintOpReference = printTime.ToString("dd")+" "+ Initials + printTime.ToString("MM")+ printTime.ToString("yy") ;
                                obj.PrintOpReference = model.PrintOpReference;
                            }
                            obj.ScaleModelId = model.Scale;
                            if (!string.IsNullOrEmpty(model.Operator))
                            {
                                model.Operator = model.Operator.ToUpper();
                                obj.Operator = model.Operator;
                            }

                            if (!string.IsNullOrEmpty(model.PrinterNo))
                            {
                                model.PrinterNo = model.PrinterNo.ToUpper();
                                obj.WorkCentre = model.PrinterNo;
                            }

                            if (!string.IsNullOrEmpty(model.ExtruderNo))
                            {
                                model.ExtruderNo = model.ExtruderNo.ToUpper();
                                obj.PreviousWorkCentre = model.ExtruderNo;
                            }
                            //Check if Label Produced is for final operation.
                            //If it is not final operation we will not receipt this label as it will be receipted during final operation.
                            //Check last operation in BOM and check if the Cost Centre is the same. i.e. Department = Bagging.
                            //Check if the last operation on the BOM belongs to the Bagging operation. If it does NOT then we updated the receipted flag to "I".
                            var BomCostCentre = wdb.sp_GetProductionBomOperations(Job).ToList().OrderByDescending(a => a.Operation).FirstOrDefault().CostCentre;
                            if (BomCostCentre != null)
                            {
                                if (model.Department.ToUpper() != BomCostCentre)
                                {
                                    obj.LabelReceipted = "I";
                                }
                            }
                            //On Scales there is no scanning so we set the scanned flag to Y and the scanned by to the user
                            obj.Scanned = "Y";
                            obj.ScannedBy = HttpContext.User.Identity.Name.ToUpper();
                            wdb.mtProductionLabels.Add(obj);
                            wdb.SaveChanges();
                            LabelDetail.Add(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error Saving Reel, " + ex.Message.ToString());
                    }
                    try
                    {
                        //Dont print label if manual scale selected
                        if (model.Scale != 1034)
                        {
                            var Result = BL.PrintScaleLabel(LabelDetail, PrinterName, model.Department);
                            if (Result != "")
                            {
                                ModelState.AddModelError("", "Error, " + Result);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error printing label, " + ex.Message.ToString());
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please select a Department.");
                }
            }

            string Username = User.Identity.Name.ToString().ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + "-" + m.FriendlyName }).ToList();

            ViewBag.Scales = scale;
            ViewBag.DepartmentList = (from a in wdb.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();

            var OpenPallets = wdb.mtPalletControls.Where(x => x.Status != "C" && x.Job == Job && x.ScaleId == model.Scale).ToList();
            if (OpenPallets.Count > 1)
            {
                ViewBag.MultiPalletNo = (from a in OpenPallets select new { Value = a.PalletNo, Text = a.PalletNo }).ToList();
            }

            var Lines = wdb.sp_GetScaleRolls(model.Pallet).ToList();
            if (Lines.Count > 0)
            {
                s.Rolls = Lines;
                model.Gross = model.Rolls.Sum(a => a.GrossMass).Value;
                model.Net = model.Rolls.Sum(a => a.NetMass).Value;
            }
            ViewBag.PalletLoaded = true;
            model.Weight = 0;

            return View("Index", s);
        }

        [HttpPost]
        public ActionResult GetScaleWeight(string Scale)

        {
            //first check if i can get weight
            try
            {
                int ScaleNo = Convert.ToInt32(Scale);
                var ScaleWeight = wdb.mtScales.Where(a => a.ScaleModelId == ScaleNo).FirstOrDefault();
                if (ScaleWeight != null)
                {
                    if (ScaleWeight.CurrWeight != 0)
                    {
                        string Weight = ScaleWeight.CurrWeight.ToString();

                        mtScale scale = ScaleWeight;
                        scale.CurrWeight = 0;
                        wdb.Entry(scale).State = EntityState.Modified;
                        wdb.SaveChanges();

                        return Json(Weight, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error: Could not get weight from scale. Please try again. Current weight is " + ScaleWeight.CurrWeight, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Error: Could not find scale. Please try again. ", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("Error: Could not get scale weight. " + ex.Message.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostJobReceipt")]
        public ActionResult PostJobReceipt(ScaleSystemViewModel model)
        {
            try
            {
                ModelState.Clear();
                string PostResult = "";
                string WarehouseResult = "";
                string Job = model.Job.PadLeft(15, '0');
                string Username = HttpContext.User.Identity.Name.ToUpper();
                //Check if Job Requires Receipting
                var SubProcessCheck = (from a in wdb.mtProductionLabels where a.PalletNo == model.Pallet && a.Scanned == "Y" select a).ToList();
                if (model.Department == "PRINT" || model.Department == "PRINTING")
                {
                    var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                    ViewBag.OperatorList = OprList;
                }
                else
                {
                    ViewBag.OperatorList = new List<SelectListItem>();
                }
                if (SubProcessCheck.Count > 0)
                {
                    if (SubProcessCheck.FirstOrDefault().LabelReceipted == "I")
                    {
                        using (var cdb = new WarehouseManagementEntities(""))
                        {
                            var closePallet = (from a in cdb.mtPalletControls where a.PalletNo == model.Pallet select a).FirstOrDefault();
                            closePallet.Status = "C";
                            cdb.Entry(closePallet).State = EntityState.Modified;
                            cdb.SaveChanges();
                        }

                        var OldPallet = model.Pallet;

                        //create new pallet
                        var NewPallet = wdb.sp_GetScalesMaxJobPalletNumber(Job, model.Scale).FirstOrDefault();
                        int PalletNumberOnly;
                        if (NewPallet == null)
                        {
                            PalletNumberOnly = 1;
                        }
                        else
                        {
                            PalletNumberOnly = (int)NewPallet.NumberOnly;
                        }
                        mtPalletControl control = new mtPalletControl();
                        string PalletNo = Job.TrimStart(new Char[] { '0' }) + "-" + PalletNumberOnly.ToString().PadLeft(3, '0');
                        control.PalletNo = PalletNo;
                        control.Job = Job;
                        control.PalletSeq = PalletNumberOnly;
                        control.Status = "O";
                        control.ScaleId = model.Scale;
                        wdb.Entry(control).State = System.Data.EntityState.Added;
                        wdb.SaveChanges();
                        model.Pallet = PalletNo;

                        PostResult = "Pallet Closed Successfully.";

                        //Generate Pallet Summary
                        model.PalletReport = ExportPdf(OldPallet);
                        model.PalletInformation = ExportPalletInformationPdf(OldPallet);
                    }
                    else
                    {
                        PostResult = receipt.PostJobReceiptByBatch(model.Pallet);
                        var OldPallet = model.Pallet;
                        if (PostResult.Contains("Job Receipt Completed Successfully") || PostResult.Contains("Delayed posting activated"))
                        {
                            //create new pallet
                            var NewPallet = wdb.sp_GetScalesMaxJobPalletNumber(Job, model.Scale).FirstOrDefault();
                            int PalletNumberOnly;
                            if (NewPallet == null)
                            {
                                PalletNumberOnly = 1;
                            }
                            else
                            {
                                PalletNumberOnly = (int)NewPallet.NumberOnly;
                            }
                            mtPalletControl control = new mtPalletControl();
                            string PalletNo = Job.TrimStart(new Char[] { '0' }) + "-" + PalletNumberOnly.ToString().PadLeft(3, '0');
                            control.PalletNo = PalletNo;
                            control.Job = Job;
                            control.PalletSeq = PalletNumberOnly;
                            control.Status = "O";
                            control.ScaleId = model.Scale;
                            wdb.Entry(control).State = System.Data.EntityState.Added;
                            wdb.SaveChanges();
                            model.Pallet = PalletNo;

                            //Generate Pallet Summary
                            model.PalletReport = ExportPdf(OldPallet);
                            model.PalletInformation = ExportPalletInformationPdf(OldPallet);

                            //If Job posted successfully,  check warehouse against scale. If warehouse found post immediate transfer
                            if (PostResult.Contains("Job Receipt Completed Successfully"))
                            {
                                var scaleSettings = (from a in wdb.mtScales where a.ScaleModelId == model.Scale select a).FirstOrDefault();
                                if (!string.IsNullOrWhiteSpace(scaleSettings.Warehouse))
                                {
                                    try
                                    {
                                        var Header = wdb.sp_GetScalesJobDetails(Job).ToList().FirstOrDefault();
                                        if (!string.IsNullOrWhiteSpace(Header.SalesOrder))
                                        {
                                            // zero the ship quantity against the order
                                            string ErrorMessage=PostSorBackOrderRelease(Header.SalesOrder, Header.SalesOrderLine);
                                            if (string.IsNullOrWhiteSpace(ErrorMessage))
                                            {
                                                //Post immediate transfer
                                                WarehouseResult = receipt.PostWarehouseTransfer(OldPallet, scaleSettings.Warehouse);
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("", "Failed to transfer stock to Warehouse : " + scaleSettings.Warehouse + ". Failed to zero ship quantity."  + ErrorMessage);
                                            }
                                        }
                                        else
                                        {
                                            WarehouseResult = receipt.PostWarehouseTransfer(OldPallet, scaleSettings.Warehouse);
                                        }
                                    }
                                    catch (Exception warehouseEx)
                                    {
                                        ModelState.AddModelError("", warehouseEx.Message);
                                    }
                                }
                            }


                        }
                    }
                }

                ModelState.AddModelError("", PostResult);

                if (!string.IsNullOrWhiteSpace(WarehouseResult))
                {
                    ModelState.AddModelError("", WarehouseResult);
                }

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + "-" + m.FriendlyName }).ToList();
                ViewBag.Scales = scale;
                ViewBag.DepartmentList = (from a in wdb.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
                //Load Lines again incase some got posted

                var OpenPallets = wdb.mtPalletControls.Where(x => x.Status != "C" && x.Job == Job && x.ScaleId == model.Scale).ToList();
                if (OpenPallets.Count > 1)
                {
                    ViewBag.MultiPalletNo = (from a in OpenPallets select new { Value = a.PalletNo, Text = a.PalletNo }).ToList();
                }

                var Lines = wdb.sp_GetScaleRolls(OpenPallets.Min(x => x.PalletNo)).ToList();
                if (Lines.Count > 0)
                {
                    model.Rolls = Lines;
                    model.Gross = model.Rolls.Sum(a => a.GrossMass).Value;
                    model.Net = model.Rolls.Sum(a => a.NetMass).Value;
                }
                else
                {
                    model.Rolls = null;
                    model.Gross = 0;
                    model.Net = 0;
                }
                ViewBag.PalletLoaded = true;
                return View("Index", model);
            }
            catch (Exception ex)
            {
                string Job = model.Job.PadLeft(15, '0');
                string Username = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + "-" + m.FriendlyName }).ToList();
                ViewBag.Scales = scale;
                ViewBag.DepartmentList = (from a in wdb.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
                ViewBag.PalletLoaded = true;
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        public ActionResult LabelReprint(string BatchId)
        {
            ModelState.Clear();

            List<mtProductionLabel> LabelDetail = new List<mtProductionLabel>();
            ScaleSystemViewModel model = new ScaleSystemViewModel();
            var Details = (from a in wdb.mtProductionLabels where a.BatchId == BatchId select a).FirstOrDefault();
            int Scale = (int)Details.ScaleModelId;
            if (Details.Department == "PRINT")
            {
                var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                ViewBag.OperatorList = OprList;
            }
            else
            {
                ViewBag.OperatorList = new List<SelectListItem>();
            }
            string Job = Details.Job;
            var PrinterName = (from a in wdb.mtScales where a.ScaleModelId == Scale select a.PrinterName).FirstOrDefault();

            mtProductionLabel obj = Details;
            LabelDetail.Add(obj);
            try
            {
                var Result = BL.PrintScaleLabel(LabelDetail, PrinterName, Details.Department);
                if (Result != "")
                {
                    ModelState.AddModelError("", "Error, " + Result);
                }
                //return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error printing label, " + ex.Message.ToString());
            }
            model.Job = Job;
            model.Scale = Scale;
            string Username = User.Identity.Name.ToString().ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + "-" + m.FriendlyName }).ToList();
            ViewBag.Scales = scale;
            ViewBag.DepartmentList = (from a in wdb.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
            var Header = wdb.sp_GetScalesJobDetails(Job).ToList().FirstOrDefault();
            if (Header != null)
            {
                model.ExtruderNo = Details.PreviousWorkCentre;
                model.PrinterNo = Details.WorkCentre;
                model.Operator = Details.Operator;
                model.JobDescription = Header.JobDescription;
                model.StockCode = Header.StockCode;
                model.StockDescription = Header.StockDescription;
                model.Customer = Header.Customer;
                model.CustStockCode = Header.CustStockCode;
                model.QtyOutstanding = (decimal)Header.QtyOutstanding;
                model.QtyManufactured = (decimal)Header.QtyManufactured;
                model.QtyToMake = (decimal)Header.QtyToMake;

                var OpenPallets = wdb.mtPalletControls.Where(x => x.Status != "C" && x.Job == Job && x.ScaleId == model.Scale).ToList();
                if (OpenPallets.Count > 1)
                {
                    ViewBag.MultiPalletNo = (from a in OpenPallets select new { Value = a.PalletNo, Text = a.PalletNo }).ToList();
                }

                var Lines = wdb.sp_GetScaleRolls(Details.PalletNo).ToList();
                if (Lines.Count > 0)
                {
                    //existing pallet not receipted
                    model.Rolls = Lines;
                    model.Pallet = Lines.FirstOrDefault().PalletNo.ToString();
                    model.Gross = model.Rolls.Sum(a => a.GrossMass).Value;
                    model.Net = model.Rolls.Sum(a => a.NetMass).Value;
                    model.Core = (decimal)Lines.FirstOrDefault().Core;
                    model.Tare = (decimal)Lines.FirstOrDefault().Tare;
                }
            }
            ViewBag.PalletLoaded = true;
            return View("Index", model);
        }

        public ActionResult DeleteLine(string BatchId, int Scale)
        {
            ModelState.Clear();
            string Username = User.Identity.Name.ToString().ToUpper();
            ScaleSystemViewModel model = new ScaleSystemViewModel();
            var Details = (from a in wdb.mtProductionLabels where a.BatchId == BatchId select a).FirstOrDefault();
            if (Details.Department == "PRINT")
            {
                var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                ViewBag.OperatorList = OprList;
            }
            else
            {
                ViewBag.OperatorList = new List<SelectListItem>();
            }
            string Job = Details.Job;
            try
            {
                //wdb.Entry(Details).State = System.Data.EntityState.Deleted;
                //wdb.SaveChanges();
                wdb.sp_LogDeletedProductionLabel(Job, BatchId, Username);
                ModelState.AddModelError("", "Deleted Succesfully");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error deleting label, " + ex.Message.ToString());
            }

            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + "-" + m.FriendlyName }).ToList();
            ViewBag.Scales = scale;
            model.Scale = Scale;
            ViewBag.DepartmentList = (from a in wdb.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
            var Header = wdb.sp_GetScalesJobDetails(Job).ToList().FirstOrDefault();
            model.Job = Job;
            if (Header != null)
            {
                model.ExtruderNo = Details.Reference;
                model.PrinterNo = Details.WorkCentre;
                model.Operator = Details.Operator;
                model.JobDescription = Header.JobDescription;
                model.StockCode = Header.StockCode;
                model.StockDescription = Header.StockDescription;
                model.Customer = Header.Customer;
                model.CustStockCode = Header.CustStockCode;
                model.QtyOutstanding = (decimal)Header.QtyOutstanding;
                model.QtyManufactured = (decimal)Header.QtyManufactured;
                model.QtyToMake = (decimal)Header.QtyToMake;

                var OpenPallets = wdb.mtPalletControls.Where(x => x.Status != "C" && x.Job == Job && x.ScaleId == model.Scale).ToList();
                if (OpenPallets.Count > 1)
                {
                    ViewBag.MultiPalletNo = (from a in OpenPallets select new { Value = a.PalletNo, Text = a.PalletNo }).ToList();
                }

                var Lines = wdb.sp_GetScaleRolls(Details.PalletNo).ToList();
                if (Lines.Count > 0)
                {
                    //existing pallet not receipted
                    model.Rolls = Lines;
                    model.Pallet = Lines.FirstOrDefault().PalletNo.ToString();
                    model.Gross = model.Rolls.Sum(a => a.GrossMass).Value;
                    model.Net = model.Rolls.Sum(a => a.NetMass).Value;
                    model.Core = (decimal)Lines.FirstOrDefault().Core;
                    model.Tare = (decimal)Lines.FirstOrDefault().Tare;
                }
                else
                {
                    model.Pallet = Details.PalletNo;
                }
            }
            ViewBag.PalletLoaded = true;
            return View("Index", model);
        }

        public ActionResult ScaleTransfer()
        {
            string Username = User.Identity.Name.ToString().ToUpper();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            ViewBag.Scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadScaleTransferDetails")]
        public ActionResult LoadScaleTransferDetails(ScaleSystemViewModel model)
        {
            try
            {
                ModelState.Clear();
                string Username = User.Identity.Name.ToString().ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
                if (model.Department == "PRINT" || model.Department == "PRINTING")
                {
                    var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                    ViewBag.OperatorList = OprList;
                }
                else
                {
                    ViewBag.OperatorList = new List<SelectListItem>();
                }
                string Job = model.Job.PadLeft(15, '0');
                int Scale = model.CurrScale;

                var JobDetails = (from a in wdb.mtProductionLabels where a.Job == Job select a).ToList();
                if (JobDetails.Count > 0)
                {
                    var ScaleDetails = (from b in JobDetails where b.ScaleModelId == Scale select b).ToList();
                    if (ScaleDetails.Count > 0)
                    {
                        var GetPallet = wdb.sp_GetScaleTransferPalletNo(Job, Scale).Where(N => N.Status != "C").ToList();
                        if (GetPallet.Count == 0)
                        {
                            ModelState.AddModelError("", "Warning there are no open pallets for this job on selected scale");
                            return View("ScaleTransfer", model);
                        }
                        if (GetPallet.Count > 1)
                        {
                            ModelState.AddModelError("", "Warning you have more than 1 open pallets for this job on selected scale.");
                            return View("ScaleTransfer", model);
                        }
                        model.Pallet = GetPallet.FirstOrDefault().PalletNo;
                        model.Net = (decimal)JobDetails.Where(a => a.PalletNo == model.Pallet).Sum(c => c.NetQty).GetValueOrDefault(0);
                        return View("ScaleTransfer", model);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Scale not linked to job. Please select another scale.");
                        return View("ScaleTransfer", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Job not found");
                    return View("ScaleTransfer", model);
                }
            }
            catch (Exception ex)
            {
                string Username = User.Identity.Name.ToString().ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
                ModelState.AddModelError("", ex.Message);
                return View("ScaleTransfer", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "TransferScale")]
        public ActionResult TransferScale(ScaleSystemViewModel model)
        {
            try
            {
                ModelState.Clear();
                ScaleSystemViewModel newModel = new ScaleSystemViewModel();
                string Username = User.Identity.Name.ToString().ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
                if (model.Department == "PRINT" || model.Department == "PRINTING")
                {
                    var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                    ViewBag.OperatorList = OprList;
                }
                else
                {
                    ViewBag.OperatorList = new List<SelectListItem>();
                }
                string Job = model.Job.PadLeft(15, '0');
                string PalletNo = model.Pallet;
                int CurrScale = model.CurrScale;
                int DestScale = model.DestScale;

                var JobDetails = (from a in wdb.mtProductionLabels where a.PalletNo == PalletNo select a).ToList();
                foreach (var item in JobDetails)
                {
                    mtProductionLabel control = new mtProductionLabel();
                    control = wdb.mtProductionLabels.Find(item.Job, item.BatchId);
                    control.ScaleModelId = DestScale;
                    wdb.Entry(control).State = System.Data.EntityState.Modified;
                    wdb.SaveChanges();
                }
                mtPalletControl up = new mtPalletControl();
                up = wdb.mtPalletControls.Find(PalletNo);
                up.ScaleId = DestScale;
                wdb.Entry(up).State = System.Data.EntityState.Modified;
                wdb.SaveChanges();

                ModelState.AddModelError("", "Saved Successfully");
                return View("ScaleTransfer", newModel);
            }
            catch (Exception ex)
            {
                string Username = User.Identity.Name.ToString().ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                ViewBag.Scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
                if (model.Department == "PRINT" || model.Department == "PRINTING")
                {
                    var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                    ViewBag.OperatorList = OprList;
                }
                else
                {
                    ViewBag.OperatorList = new List<SelectListItem>();
                }
                ModelState.AddModelError("", ex.Message);
                return View("ScaleTransfer", model);
            }
        }

        public ExportFile ExportPdf(string PalletNo)
        {
            try
            {
                var ReportPath = (from a in wdb.mtReportMasters where a.Program == "DynamicReports" && a.Report == "Pallet" select a.ReportPath).FirstOrDefault().Trim();
                ReportDocument rpt = new ReportDocument();
                rpt.Load(ReportPath);

                ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings["SysproEntities"];
                if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
                {
                    throw new Exception("Fatal error: Missing connection string 'SysproEntities' in web.config file");
                }
                string sysproConnectionString = sysproSettings.ConnectionString;
                EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(sysproConnectionString);
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

                string password = sqlConnectionStringBuilder.Password;
                string userId = sqlConnectionStringBuilder.UserID;

                rpt.SetDatabaseLogon(userId, password);

                rpt.SetParameterValue("@PalletNo", PalletNo);

                string FilePath = HttpContext.Server.MapPath("~/Reports/Bagging/BaggingPallet/");

                string FileName = PalletNo + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);

                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\Reports\Bagging\BaggingPallet\" + FileName;
                //file.FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/") + FileName;
                //file.FilePath = OutputPath;
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public ExportFile ExportPalletInformationPdf(string PalletNo)
        {
            try
            {
                var ReportPath = (from a in wdb.mtReportMasters where a.Program == "DynamicReports" && a.Report == "PalletInfo" select a.ReportPath).FirstOrDefault().Trim();
                ReportDocument rpt = new ReportDocument();
                rpt.Load(ReportPath);

                ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings["SysproEntities"];
                if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
                {
                    throw new Exception("Fatal error: Missing connection string 'SysproEntities' in web.config file");
                }
                string sysproConnectionString = sysproSettings.ConnectionString;
                EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(sysproConnectionString);
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

                string password = sqlConnectionStringBuilder.Password;
                string userId = sqlConnectionStringBuilder.UserID;

                rpt.SetDatabaseLogon(userId, password);

                rpt.SetParameterValue("@PalletNo", PalletNo);

                string FilePath = HttpContext.Server.MapPath("~/Reports/Bagging/BaggingPallet/");

                string FileName = "PalletInfo-" + PalletNo + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);

                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\Reports\Bagging\BaggingPallet\" + FileName;
                //file.FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/") + FileName;
                //file.FilePath = OutputPath;
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RefreshPallet")]
        public ActionResult RefreshPallet(ScaleSystemViewModel model)
        {
            try
            {
                ModelState.Clear();
                string Job = model.Job.PadLeft(15, '0');
                string Username = User.Identity.Name.ToString().ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
                ViewBag.Scales = scale;
                if (model.Department == "PRINT" || model.Department == "PRINTING")
                {
                    var OprList = (from a in wdb.sp_GetScaleOperators("PRINT").ToList() select new { Value = a.Employee + " - " + a.Name, Text = a.Employee + " - " + a.Name }).ToList();
                    ViewBag.OperatorList = OprList;
                }
                else
                {
                    ViewBag.OperatorList = new List<SelectListItem>();
                }
                var DeptList = (from a in wdb.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
                ViewBag.DepartmentList = DeptList;
                if (DeptList.Count > 0)
                {
                    model.Department = DeptList.FirstOrDefault().Value;
                }
                else
                {
                    ModelState.AddModelError("", "No Departments found.");
                    ViewBag.PalletLoaded = false;
                    return View("Index", model);
                }

                var Header = wdb.sp_GetScalesJobDetails(Job).ToList().FirstOrDefault();
                if (Header != null)
                {
                    model.JobDescription = Header.JobDescription;
                    model.StockCode = Header.StockCode;
                    model.StockDescription = Header.StockDescription;
                    model.Customer = Header.Customer;
                    model.CustStockCode = Header.CustStockCode;
                    model.QtyOutstanding = (decimal)Header.QtyOutstanding;
                    model.QtyManufactured = (decimal)Header.QtyManufactured;
                    model.QtyToMake = (decimal)Header.QtyToMake;

                    var OpenPallets = wdb.mtPalletControls.Where(x => x.Status != "C" && x.Job == Job && x.ScaleId == model.Scale).ToList();
                    if (OpenPallets.Count > 1)
                    {
                        ViewBag.MultiPalletNo = (from a in OpenPallets select new { Value = a.PalletNo, Text = a.PalletNo }).ToList();
                    }

                    var Lines = wdb.sp_GetScaleRolls(model.Pallet).ToList();
                    if (Lines.Count > 0)
                    {
                        //existing pallet not receipted
                        model.Rolls = Lines;
                        model.Pallet = Lines.FirstOrDefault().PalletNo.ToString();
                        model.Gross = model.Rolls.Sum(a => a.GrossMass).Value;
                        model.Net = model.Rolls.Sum(a => a.NetMass).Value;
                        model.Core = (decimal)Lines.FirstOrDefault().Core;
                        model.Tare = (decimal)Lines.FirstOrDefault().Tare;
                    }
                    else
                    {
                        var settings = (from a in wdb.mtScales where a.ScaleModelId == model.Scale select a).FirstOrDefault();
                        model.Core = (decimal)settings.Core;
                        model.Tare = (decimal)settings.Tare;

                        model.Rolls = null;
                        model.Gross = 0;
                        model.Net = 0;
                    }
                    ViewBag.PalletLoaded = true;
                    return View("Index", model);
                }
                else
                {
                    ViewBag.PalletLoaded = false;
                    ModelState.AddModelError("", "Job not found.");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error getting Job Details: " + ex.Message.ToString());
                ViewBag.PalletLoaded = false;
                return View("Index", model);
            }
        }

        public string PostSorBackOrderRelease(string SalesOrder, decimal SalesOrderLine)
        {
            string Guid = sys.SysproLogin();
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("This is an example XML instance to demonstrate");
            Document.Append("use of the Sales Order Back Order Release Business Object");
            Document.Append("-->");
            Document.Append("<PostSorBackOrderRelease xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTBODOC.XSD\">");


            Document.Append("<Item>");
            Document.Append("<SalesOrder>" + SalesOrder + "</SalesOrder>");
            Document.Append("<ReleaseFromMultipleLines>N</ReleaseFromMultipleLines>");
            Document.Append("<SalesOrderLine>" + SalesOrderLine + "</SalesOrderLine>");
            Document.Append("<CompleteLine>N</CompleteLine>");
            Document.Append("<AdjustOrderQuantity>N</AdjustOrderQuantity>");
            Document.Append("<OrderStatus>3</OrderStatus>");
            Document.Append("<ReleaseFromShip>Y</ReleaseFromShip>");
            Document.Append("<ZeroShipQuantity>Y</ZeroShipQuantity>");
            Document.Append("<AllocateSerialNumbers>N</AllocateSerialNumbers>");
            Document.Append("<eSignature>");
            Document.Append("</eSignature>");
            Document.Append("</Item>");


            Document.Append("</PostSorBackOrderRelease>");

            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the Sales Order Back Order Release Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostSorBackOrderRelease xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORTBO.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<AddQuantityToBatchSerial>N</AddQuantityToBatchSerial>");
            Parameter.Append("<IgnoreAutoDepletion>N</IgnoreAutoDepletion>");
            Parameter.Append("<ShipKitFromDefaultBin>N</ShipKitFromDefaultBin>");
            Parameter.Append("<PickFunction>A</PickFunction>");
            Parameter.Append("<Destinationbin></Destinationbin>");
            Parameter.Append("<Pick></Pick>");
            Parameter.Append("<PickSequence>B</PickSequence>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostSorBackOrderRelease>");

            string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "SORTBO");
            string ErrorMessage = sys.GetXmlErrors(XmlOut);
            return ErrorMessage;
           

        }
        [CustomAuthorize(Activity: "DispatchScales")]
        public ActionResult PalletScan()
        {
            string Username = User.Identity.Name.ToString().ToUpper();
            ViewBag.OperatorList = new List<SelectListItem>();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
            ViewBag.Scales = scale;
            ViewBag.PalletLoaded = false;
            ViewBag.DepartmentList = new List<SelectListItem>();
            return View();
        }

        [HttpPost]
        [MultipleButton(Name ="action", Argument = "LoadPalletDetails")]
        public ActionResult LoadPalletDetails(ScaleSystemViewModel model)
        {
            try
            {
                string Username = User.Identity.Name.ToString().ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
                ViewBag.Scales = scale;
                ViewBag.PalletLoaded = true;
                var palletDetail = wdb.mt_GetPalletDetailsByPalletId(model.Pallet).FirstOrDefault();
                if (palletDetail==null)
                {
                    ModelState.AddModelError("", "Please Enter the Correct Pallet Number.");
                    return View("PalletScan", model);
                }
                else
                {
                    model.PalletList = palletDetail;
                    var DeptList = (from a in wdb.sp_GetProductionDepartments(model.PalletList.Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
                    ViewBag.DepartmentList = DeptList;
                    ModelState.AddModelError("", "Pallet Details Loaded Successfully");
                    return View("PalletScan", model);
                }
                
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("PalletScan", model);
            }            
        }

        [HttpPost]
        [MultipleButton(Name ="action",Argument = "AddPalletLines")]
        public ActionResult AddPalletLines(ScaleSystemViewModel model)
        {
            try
            {
                string Username = User.Identity.Name.ToString().ToUpper();
                ViewBag.OperatorList = new List<SelectListItem>();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var scale = (from m in wdb.sp_GetUserScaleAccess(Company, Username).ToList() select new { Value = m.ScaleModelId, Text = m.ScaleModelId + " - " + m.FriendlyName }).ToList();
                ViewBag.Scales = scale;
                ViewBag.PalletLoaded = true;

                var palletDetail = wdb.mt_GetPalletDetailsByPalletId(model.Pallet).FirstOrDefault();
                model.PalletList = palletDetail;
                var DeptList = (from a in wdb.sp_GetProductionDepartments(model.PalletList.Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
                ViewBag.DepartmentList = DeptList;
                var checkList = (from a in wdb.mtProductionPalletWeights where a.Job == model.PalletList.Job && a.PalletNo == model.Pallet select a).FirstOrDefault();
                if (checkList == null)
                {
                    mtProductionPalletWeight obj = new mtProductionPalletWeight()
                    {
                        StockCode = palletDetail.StockCode,
                        Job = palletDetail.Job,
                        GrossQty = palletDetail.GrossQtyTotal,
                        NetQty = palletDetail.NetQtyTotal,
                        ScaleWeight = model.Weight,
                        Username = Username,
                        ScaleDate = DateTime.Now,
                        ScaleModelId = model.Scale,
                        PalletNo = model.Pallet,
                    };
                    wdb.mtProductionPalletWeights.Add(obj);
                    wdb.SaveChanges();

                    var PrinterName = (from a in wdb.mtScales where a.ScaleModelId == model.Scale select a.PrinterName).FirstOrDefault();
                    try
                    {
                        BL.PrintPalletLbl(obj.StockCode, obj.Job, obj.ScaleWeight, DeptList.FirstOrDefault().Text, PrinterName, model.Pallet);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error printing label, " + ex.Message.ToString());
                    }
                }
                else
                {
                    mtProductionPalletWeight obj = new mtProductionPalletWeight()
                    {
                        Job= palletDetail.Job,
                        PalletNo=model.Pallet,
                        StockCode = palletDetail.StockCode,
                        GrossQty = palletDetail.GrossQtyTotal,
                        NetQty = palletDetail.NetQtyTotal,
                        ScaleWeight = model.Weight,
                        Username = Username,
                        ScaleDate = DateTime.Now,
                        ScaleModelId = model.Scale,
                        
                    };
                    wdb.Entry(checkList).CurrentValues.SetValues(obj);
                    wdb.SaveChanges();

                    var PrinterName = (from a in wdb.mtScales where a.ScaleModelId == model.Scale select a.PrinterName).FirstOrDefault();
                    try
                    {
                        BL.PrintPalletLbl(obj.StockCode, obj.Job, obj.ScaleWeight, DeptList.FirstOrDefault().Text, PrinterName, model.Pallet);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error printing label, " + ex.Message.ToString());
                    }
                }

                ModelState.AddModelError("", "Label Print Successful");
                return View("PalletScan",model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("PalletScan",model);
            }
        }
        [CustomAuthorize(Activity: "DispatchScales")]
        public ActionResult PalletScanner()
        {
            return View();
        }
    }
}