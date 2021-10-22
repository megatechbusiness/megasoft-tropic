using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Megasoft2.Controllers
{
    public class WhseManBatchReceiptController : Controller
    {
        //
        // GET: /WhseManPalletReceipt/
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        WhseManProductionReceipt BL = new WhseManProductionReceipt();
        MegasoftEntities wdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult Index()
        {
            return View();
        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult ValidateDetails(string details)
        {
            try
            {
               
                    return Json(BL.ValidateBarcode(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult SearchPalletNo(string details)
        {
            try
            {
                return Json(BL.SearchPalletNo(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult SearchBailNo(string details)
        {
            try
            {
                return Json(BL.SearchBailNo(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult Last5Scans(string PalletNo)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                var Total = (from a in db.mtProductionLabels where a.PalletNo == PalletNo && a.ScannedBy == Username && a.Scanned == "Y" && (a.LabelReceipted != "Y" || a.LabelReceipted == null) select a.BatchId).ToList().Count();
                var lastFiveProducts = (from p in db.mtProductionLabels
                                        where p.PalletNo == PalletNo && p.ScannedBy == Username && p.Scanned == "Y" && (p.LabelReceipted != "Y" || p.LabelReceipted == null)
                                        orderby p.ScanDate descending
                                        select new { BatchId = p.BatchId,TotalBails = Total }).Take(5).ToList();
                return Json(lastFiveProducts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult DeleteBail(string details)
        {
            try
            {
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    string BailNo = myDeserializedObjList.FirstOrDefault().BailNo.Trim();
                    string Job = myDeserializedObjList.FirstOrDefault().Job.Trim().PadLeft(15, '0');
                    mtProductionLabel delete = new mtProductionLabel();
                    delete = db.mtProductionLabels.Find(Job, BailNo);
                    delete.PalletNo = "";
                    delete.Scanned = "N";
                    delete.ScanDate = null;
                    delete.ScannedBy = "";
                    db.Entry(delete).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                    return Json("", JsonRequestBehavior.AllowGet);

                }
                return Json("Error - No Data.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult PostJobReceipt(string details)
        {
            try
            {
                List<WhseManJobReceipt> myDeserializedObjList = (List<WhseManJobReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WhseManJobReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    string PalletNo = myDeserializedObjList.FirstOrDefault().Lot; //Pallet Passed to Lot in Javascript
                    return Json(BL.PostJobReceiptByBatch(PalletNo), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Data found.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult TransferBail()
        {
            return View();
        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult PostBailTransfer(string details)
        {
            try
            {
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach(var item in myDeserializedObjList)
                    {
                        string PalletNo = item.PalletNo;
                        string BatchId = item.BailNo;
                        string Job = item.Job.PadLeft(15, '0');

                        //Check if pallet no exists
                        var JobDetails = (from a in db.mtProductionLabels where a.BatchId == BatchId select a).ToList();
                        var PalletDetails = (from a in db.mtPalletControls where a.PalletNo == PalletNo select a).ToList();
                        if (JobDetails.Count > 0)
                        {
                            if (PalletDetails.Count > 0)
                            {
                                //Update Syspro
                                db.sp_ProductionLotCustomForm(BatchId, Job, PalletNo);
                                //Update Megasoft Table
                                mtProductionLabel label = new mtProductionLabel();
                                label = JobDetails.FirstOrDefault();
                                label.PalletNo = PalletNo;
                                db.Entry(label).State = System.Data.EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                return Json("Error: Pallet number doesnt exist. "+PalletNo, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("Error: Batch doesnt exist. "+ BatchId, JsonRequestBehavior.AllowGet);
                        }

                    }
                    return Json("Saved Successfully.", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error: No data.", JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception ex)
            {
                return Json("Error: "+ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult ScanPalletNo(string details)
        {
            try
            {
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    string PalletNo = myDeserializedObjList.FirstOrDefault().PalletNo;
                    //Check if pallet no exists
                    var PalletDetails = (from a in db.mtPalletControls where a.PalletNo == PalletNo select a).ToList();
                    if(PalletDetails.Count > 0)
                    {
                        if (PalletDetails.FirstOrDefault().Status !="C")
                        {
                            return Json(PalletNo, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("Error: Pallet is closed.", JsonRequestBehavior.AllowGet);
                        }
                        
                    }
                    else
                    {
                       return Json("Error: Pallet number doesnt exist.", JsonRequestBehavior.AllowGet);
                    } 
                }
                else
                {
                    return Json("Error: No data.", JsonRequestBehavior.AllowGet);
                }
            }
               
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult CheckBailNo(string details)
        {
            try
            {
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Bail = myDeserializedObjList.FirstOrDefault().BailNo;
                    //Check if pallet no exists
                    var BailDetails = (from a in db.mtProductionLabels where a.BatchId == Bail select a).ToList();
                    if (BailDetails.Count > 0)
                    {                    
                            return Json("", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error: Pallet number doesnt exist.", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Error: No data.", JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult OpenPallet()
        {
            return View();
        }
        [CustomAuthorize(Activity: "BatchReceipt")]
        public ActionResult OpenPalletNo(string details)
        {
            try
            {
                
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    string PalletNo = "";

                    PalletNo = myDeserializedObjList.FirstOrDefault().PalletNo;

                        var PalletDetails = (from a in db.mtPalletControls where a.PalletNo == PalletNo select a).ToList();

                            if (PalletDetails.Count > 0)
                            {
                                mtPalletControl pallet = new mtPalletControl();
                                pallet = PalletDetails.FirstOrDefault();
                                if(pallet.Status=="C")
                                {

                                    pallet.Status = "O";
                                    db.Entry(pallet).State = System.Data.EntityState.Modified;
                                    db.SaveChanges();
                                    return Json("Pallet " + PalletNo + " opened.", JsonRequestBehavior.AllowGet);
                            }
                                else
                                {
                                    pallet.Status = "C";
                                    db.Entry(pallet).State = System.Data.EntityState.Modified;
                                    db.SaveChanges();
                                    return Json("Pallet " + PalletNo + " closed.", JsonRequestBehavior.AllowGet);
                            }
                            }
                            else
                            {
                                return Json("Error: Pallet number doesnt exist. " + PalletNo, JsonRequestBehavior.AllowGet);
                            }
                }
                else
                {
                    return Json("Error: No data.", JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception ex)
            {
                return Json("Error: " + ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult TempReversal()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TempReversal(mtTmpLotsToReverse model)
        {
            try
            {
                ModelState.Clear();

                string returnmsg = BL.PostBatchReversal();

                ModelState.AddModelError("", returnmsg);

                return View("TempReversal", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("TempReversal", model);
            }
            
        }
        [CustomAuthorize(Activity: "ProductionReturn")]
        public ActionResult ScanReturnItems()
        {
            return View();
        }
        [CustomAuthorize(Activity: "ProductionReturn")]
        public ActionResult ValidateProductionReturnBatch(string details)
        {
            try
            {
                List<BatchReceipt> myDeserializedObjList = (List<BatchReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BatchReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    string BatchId = myDeserializedObjList.FirstOrDefault().BailNo;
                    //Check if pallet no exists
                    var BailDetails = (from a in db.mtProductionLabels where a.BatchId == BatchId select a).ToList();
                    if (BailDetails != null)
                    {
                        if (BailDetails.Count > 0)
                        {
                            if (BailDetails.FirstOrDefault().LabelReceipted == "Y")
                            {
                                var QtyOnHand = (from a in db.LotDetails where a.Lot == BatchId select a.QtyOnHand).FirstOrDefault();
                                if(QtyOnHand != 0)
                                {
                                    return Json("", JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json("Error: Cannot reverse batch, Quantity on hand is zero.", JsonRequestBehavior.AllowGet);
                                }
                               
                            }
                            else
                            {
                                return Json("Error: Batch isnt receipted.", JsonRequestBehavior.AllowGet);
                            }                         
                        }
                        else
                        {
                            return Json("Error: Batch doesnt exist.", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json("Error: Batch doesnt exist.", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Error: No data.", JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        [CustomAuthorize(Activity: "ProductionReturn")]
        public ActionResult PostProductionReturn(string details)
        {
            try
            {
                List<WhseManJobReceipt> myDeserializedObjList = (List<WhseManJobReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WhseManJobReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach(var item in myDeserializedObjList)
                    {
                        if(item.Quantity == 0)
                        {
                            var LotQty = (from a in db.LotDetails.AsNoTracking() where a.Lot == item.Lot select a.QtyOnHand).FirstOrDefault();
                            item.Quantity = LotQty;
                        }
                        
                    }
                    return Json(BL.PostProductionReturnJobReceipt(myDeserializedObjList), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Data found.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        [CustomAuthorize(Activity:"ProductionReturn")]
        public ActionResult ProductionReturns()
        {
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadReturnPallet")]
        public ActionResult LoadReturnPallet(BatchReceipt model)
        {
            try
            {
                ModelState.Clear();
                var Details = db.sp_GetProductionReturnPalletDetails("Pallet", model.PalletNo).ToList();
                if(Details.Count > 0)
                {
                    var SalesOrder = Details.FirstOrDefault().SalesOrder;
                    if(SalesOrder != null)
                    {
                        var ShipQty = Details.FirstOrDefault().MShipQty;
                        if(ShipQty > 0)
                        {
                            ModelState.AddModelError("", "Please zero the ship quantity on sales order: "+SalesOrder +" to continue.");
                        }
                        else
                        {
                            model.PalletDetails = Details;
                            model.Job = "";
                            model.JobPalletDetails = null;
                            model.JobStockCode = "";
                            model.JobStockDescription = "";
                            model.StockCode = Details.FirstOrDefault().StockCode;
                            model.StockDescription = Details.FirstOrDefault().StockDescription;
                            ViewBag.LoadPallet = true;
                        }
                    }
                    else
                    {
                        model.PalletDetails = Details;
                        model.JobPalletDetails = null;
                        model.JobStockCode = "";
                        model.JobStockDescription = "";
                        model.StockCode = Details.FirstOrDefault().StockCode;
                        model.StockDescription = Details.FirstOrDefault().StockDescription;
                        ViewBag.LoadPallet = true;
                    }

                }
                else
                {
                    ModelState.AddModelError("", "No data found.");
                }
                
                return View("ProductionReturns",model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("",ex.Message);
                return View("ProductionReturns", model);
            }
           
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadReturnJobPallet")]
        public ActionResult LoadReturnJobPallet(BatchReceipt model)
        {
            try
            {
                ModelState.Clear();
                string Job = model.Job.PadLeft(15, '0');
                var Details = db.sp_GetProductionReturnPalletDetails("Job",Job).ToList();
                if (Details.Count > 0)
                {
                    var SalesOrder = Details.FirstOrDefault().SalesOrder;
                    if (SalesOrder != null)
                    {
                        var ShipQty = Details.FirstOrDefault().MShipQty;
                        if (ShipQty > 0)
                        {
                            ModelState.AddModelError("", "Please zero the ship quantity on sales order: " + SalesOrder + " to continue.");
                        }
                        else
                        {
                            model.JobPalletDetails = Details;
                            model.PalletNo = "";
                            model.PalletDetails = null;
                            model.JobStockCode = Details.FirstOrDefault().StockCode;
                            model.JobStockDescription = Details.FirstOrDefault().StockDescription;
                            model.StockCode = "";
                            model.StockDescription = "";
                            ViewBag.LoadJobPallet = true;
                        }
                    }
                    else
                    {
                        model.JobPalletDetails = Details;
                        model.PalletDetails = null;
                        model.JobStockCode = Details.FirstOrDefault().StockCode;
                        model.JobStockDescription = Details.FirstOrDefault().StockDescription;
                        model.StockCode = "";
                        model.StockDescription = "";
                        ViewBag.LoadJobPallet = true;
                    }

                }
                else
                {
                    ModelState.AddModelError("", "No data found.");
                }

                return View("ProductionReturns", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("ProductionReturns", model);
            }

        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostReturnPallet")]
        public ActionResult PostReturnPallet(BatchReceipt model)
        {
            ModelState.Clear();
            BatchReceipt modelToReturn = new BatchReceipt();       
            var BatchToReverse = model.PalletDetails.Where(l => l.Selected == true).ToList();
            List<WhseManJobReceipt> result = BatchToReverse.Select(cl => new WhseManJobReceipt { Job = cl.Job, Quantity = Convert.ToDecimal(cl.NetQty), Lot = cl.BatchId, PalletNo = cl.PalletNo }).ToList();
            try
            {
                var Message = BL.PostProductionReturnJobReceipt(result);
                if(Message =="")
                {
                    Message = "Batch Reversed Successfully.";
                }
                else
                {
                    modelToReturn = model;
                }
                ModelState.AddModelError("", Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                modelToReturn = model;
            }
            return View("ProductionReturns", modelToReturn);

        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostReturnJobPallet")]
        public ActionResult PostReturnJobPallet(BatchReceipt model)
        {
            ModelState.Clear();
            BatchReceipt modelToReturn = new BatchReceipt();
            var BatchToReverse = model.JobPalletDetails.Where(l => l.Selected == true).ToList();
            List<WhseManJobReceipt> result = BatchToReverse.Select(cl => new WhseManJobReceipt { Job = cl.Job, Quantity = Convert.ToDecimal(cl.NetQty), Lot = cl.BatchId, PalletNo = cl.PalletNo }).ToList();
            try
            {
                var Message = BL.PostProductionReturnJobReceipt(result);
                if (Message == "")
                {
                    Message = "Batch Reversed Successfully.";
                }
                else
                {
                    modelToReturn = model;
                }
                ModelState.AddModelError("", Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                modelToReturn = model;
            }
            return View("ProductionReturns", modelToReturn);

        }

        [CustomAuthorize(Activity: "SplitPallet")]
        public ActionResult SplitPallet(BatchReceipt model)
        {
            return View();
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadSplitPallet")]
        public ActionResult LoadSplitPallet(BatchReceipt model)
        {
            try
            {
                ModelState.Clear();
                BatchReceipt newModel = new BatchReceipt();
                newModel.PalletNo = model.PalletNo;
                newModel.SplitPalletDetails = db.sp_GetSplitPalletDetails(model.PalletNo).ToList();
                newModel.Job = newModel.SplitPalletDetails.FirstOrDefault().Job;
                if (newModel.SplitPalletDetails.Count == 0)
                {
                    ModelState.AddModelError("", "Pallet not found.");
                }
                newModel.StockCode = db.WipMasters.Find(newModel.SplitPalletDetails.FirstOrDefault().Job).StockCode;
                newModel.StockDescription = db.WipMasters.Find(newModel.SplitPalletDetails.FirstOrDefault().Job).StockDescription;
                return View("SplitPallet", newModel);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("SplitPallet", model);
            }
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "TransferSplitPallet")]
        public ActionResult TransferSplitPallet(BatchReceipt model)
        {
            try {
                ModelState.Clear();
                string Job = model.SplitPalletDetails.FirstOrDefault().Job.PadLeft(15, '0');
                //create new pallet
                var NewPallet = db.sp_GetScalesMaxJobPalletNumber(Job, 0).FirstOrDefault();
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
                string NewPalletNo = Job.TrimStart(new Char[] { '0' }) + "-" + PalletNumberOnly.ToString().PadLeft(3, '0');
                control.PalletNo = NewPalletNo;
                control.Job = Job;
                control.PalletSeq = PalletNumberOnly;
                control.Status = "C";
                db.Entry(control).State = System.Data.EntityState.Added;
                db.SaveChanges();

                var selectedLots = (from a in model.SplitPalletDetails where a.Selected == true select a).ToList();
                foreach (var item in selectedLots)
                {
                    db.sp_ProductionLotCustomForm(item.BatchId, Job, NewPalletNo);

                    mtProductionLabel label = new mtProductionLabel();
                    label = db.mtProductionLabels.Find(Job, item.BatchId);
                    if (label != null)
                    {
                        label.PalletNo = NewPalletNo;
                        db.Entry(label).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                BatchReceipt newModel = new BatchReceipt();
                newModel.NewPalletNo = NewPalletNo;               
                newModel.PalletTransferedTo = db.sp_GetSplitPalletDetails(NewPalletNo).ToList();
                newModel.PalletReport = ExportPdf(NewPalletNo);
                newModel.PalletInformation = ExportPalletInformationPdf(NewPalletNo);
                return View("SplitPallet", newModel);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("SplitPallet", model);
            }
            
        }
        public ExportFile ExportPdf(string PalletNo)
        {
            try
            {
                string[] files = Directory.GetFiles(HttpContext.Server.MapPath("~/Reports/Bagging/BaggingPallet/"));

                foreach (string delFile in files)
                {
                    FileInfo fi = new FileInfo(delFile);
                    if (fi.LastWriteTime < DateTime.Now.AddDays(-7))
                        fi.Delete();
                }
            }
            catch (Exception err)
            {

            }

            try
            {
                var ReportPath = (from a in db.mtReportMasters where a.Program == "DynamicReports" && a.Report == "Pallet" select a.ReportPath).FirstOrDefault().Trim();
                ReportDocument rpt = new ReportDocument();
                rpt.Load(ReportPath);

                ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings["WarehouseManagementEntities"];
                if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
                {
                    throw new Exception("Fatal error: Missing connection string 'WarehouseManagementEntities' in web.config file");
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

               
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\Reports\Bagging\BaggingPallet\" + FileName;
                
                return file;

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load pallet report: "+ex.Message+" PalletNo: "+PalletNo);
            }
        }

        public ExportFile ExportPalletInformationPdf(string PalletNo)
        {
            try
            {
                var ReportPath = (from a in db.mtReportMasters where a.Program == "DynamicReports" && a.Report == "PalletInfo" select a.ReportPath).FirstOrDefault().Trim();
                ReportDocument rpt = new ReportDocument();
                rpt.Load(ReportPath);

                ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings["WarehouseManagementEntities"];
                if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
                {
                    throw new Exception("Fatal error: Missing connection string 'WarehouseManagementEntities' in web.config file");
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
                throw new Exception("Failed to load pallet information report: " + ex.Message + " PalletNo: " + PalletNo);
            }
        }
    }
}
