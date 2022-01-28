using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransportSystemWaybillEntryController : Controller
    {

        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "TransportWaybillEntry")]
        public ActionResult Index(int TrackId = 0)
        {
            ViewBag.CanSavePOD = CanSavePOD();
            if (TrackId == 0)
            {
                ViewBag.IsValidTrackId = false;
                return View();
            }
            else
            {
                ModelState.Clear();
                TransportSystemWaybillEntryViewModel model = new TransportSystemWaybillEntryViewModel();

                var Header = (from a in wdb.mtTransportWaybillHdrs where a.TrackId == TrackId select a).ToList();
                if (Header.Count > 0)
                {
                    ViewBag.IsValidTrackId = true;
                    var detail = wdb.sp_GetTransWaybillDetailByTrackId(TrackId).ToList();//(from a in wdb.mtTransportWaybillDetails where a.TrackId == TrackId select a).ToList();
                    model.TrackId = TrackId;
                    model.Transporter = Header.FirstOrDefault().Transporter;
                    model.RegNo = Header.FirstOrDefault().VehicleReg;
                    model.Driver = Header.FirstOrDefault().Driver;
                    model.Detail = detail;

                }
                else
                {
                    ViewBag.IsValidTrackId = false;
                    ModelState.AddModelError("", "Track Id not found!");
                }

                return View("Index", model);
            }

        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadTrackId")]
        public ActionResult LoadTrackId(TransportSystemWaybillEntryViewModel model)
        {
            ViewBag.CanSavePOD = CanSavePOD();
            try
            {
                ModelState.Clear();

                if (model.TrackId == 0)
                {
                    ViewBag.IsValidTrackId = false;
                }
                else
                {
                    var Header = (from a in wdb.mtTransportWaybillHdrs where a.TrackId == model.TrackId select a).ToList();
                    if (Header.Count > 0)
                    {
                        ViewBag.IsValidTrackId = true;
                        var detail = wdb.sp_GetTransWaybillDetailByTrackId(model.TrackId).ToList();
                        model.TrackId = model.TrackId;
                        model.Transporter = Header.FirstOrDefault().Transporter;
                        model.RegNo = Header.FirstOrDefault().VehicleReg;
                        model.Driver = Header.FirstOrDefault().Driver;
                        model.Detail = detail;

                    }
                    else
                    {
                        ViewBag.IsValidTrackId = false;
                        ModelState.AddModelError("", "Track Id not found!");
                    }

                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveWaybill")]
        public ActionResult SaveWaybill(TransportSystemWaybillEntryViewModel model)
        {
            ViewBag.CanSavePOD = CanSavePOD();
            try
            {
                ModelState.Clear();
                var NewTrackId = 0;

                using (var hdb = new WarehouseManagementEntities(""))
                {
                    var TrackId = (from a in hdb.mtTransportWaybillHdrs where a.TrackId == model.TrackId select a).FirstOrDefault();
                    if (TrackId == null)
                    {
                        //Add header first
                        mtTransportWaybillHdr hdr = new mtTransportWaybillHdr();
                        hdr.Transporter = model.Transporter;
                        hdr.VehicleReg = model.RegNo;
                        hdr.Driver = model.Driver;
                        hdr.TrnDate = DateTime.Now;
                        hdr.Username = HttpContext.User.Identity.Name.ToUpper();
                        hdb.Entry(hdr).State = EntityState.Added;
                        hdb.SaveChanges();
                        NewTrackId = hdr.TrackId;

                    }
                    else
                    {
                        NewTrackId = model.TrackId;
                        TrackId.Transporter = model.Transporter;
                        TrackId.VehicleReg = model.RegNo;
                        TrackId.Driver = model.Driver;
                        TrackId.TrnDate = DateTime.Now;
                        TrackId.Username = HttpContext.User.Identity.Name.ToUpper();
                        hdb.Entry(TrackId).State = EntityState.Modified;
                        hdb.SaveChanges();
                    }
                }

                using (var cdb = new WarehouseManagementEntities(""))
                {
                    string DispatchNote = model.DispatchNote.PadLeft(15, '0');
                    var check = (from a in cdb.mtTransportWaybillDetails where a.TrackId == NewTrackId && a.Waybill == model.Waybill && a.DispatchNote == DispatchNote && a.DispatchNoteLine == model.DispatchNoteLine select a).FirstOrDefault();
                    if (check != null)
                    {
                        //Update required
                        check.TrackId = check.TrackId; //Primary Key
                        check.Waybill = check.Waybill.Trim(); //Primary Key
                        check.DispatchNote = DispatchNote;
                        check.DispatchNoteLine = model.DispatchNoteLine;
                        check.Customer = model.Customer;
                        check.Province = model.Province;
                        check.Town = model.Town;
                        check.StockCode = model.StockCode;
                        check.StockDesc = model.StockDesc;
                        check.DispatchQty = model.DispatchQty;
                        check.DispatchUom = model.DispatchUom;
                        check.LoadQty = model.LoadQty;
                        check.LoadUom = model.LoadUom;
                        check.Pallets = model.Pallets;
                        ///check.Weight = model.Weight; changed 06/03/2019
                        check.Weight = model.LoadQty;
                        check.Notes = model.Notes;
                        check.TrnDate = DateTime.Now;
                        check.Username = HttpContext.User.Identity.Name.ToUpper();
                        check.WaybillReturn = model.WaybillReturn;
                        check.DeliveryDate = model.DeliveryDate;
                        check.PODDate = model.PODDate;
                        check.PODComment = model.PODComment;
                        cdb.Entry(check).State = EntityState.Modified;
                        cdb.SaveChanges();

                    }
                    else
                    {

                        //Add new line
                        mtTransportWaybillDetail obj = new mtTransportWaybillDetail();
                        obj.TrackId = NewTrackId;
                        obj.Waybill = model.Waybill.Trim();
                        obj.DispatchNote = DispatchNote;
                        obj.DispatchNoteLine = model.DispatchNoteLine;
                        obj.Customer = model.Customer;
                        obj.Province = model.Province;
                        obj.Town = model.Town;
                        obj.StockCode = model.StockCode;
                        obj.StockDesc = model.StockDesc;
                        obj.DispatchQty = model.DispatchQty;
                        obj.DispatchUom = model.DispatchUom;
                        obj.LoadQty = model.LoadQty;
                        obj.LoadUom = model.LoadUom;
                        obj.Pallets = model.Pallets;
                        //obj.Weight = model.Weight;
                        obj.Weight = model.LoadQty;
                        obj.Notes = model.Notes;
                        obj.TrnDate = DateTime.Now;
                        obj.Username = HttpContext.User.Identity.Name.ToUpper();
                        obj.WaybillReturn = model.WaybillReturn;
                        obj.DeliveryDate = model.DeliveryDate;
                        obj.PODDate = model.PODDate;
                        obj.PODComment = model.PODComment;
                        cdb.Entry(obj).State = EntityState.Added;
                        cdb.SaveChanges();

                    }
                }

                TransportSystemWaybillEntryViewModel modelOut = new TransportSystemWaybillEntryViewModel();

                modelOut.TrackId = NewTrackId;
                modelOut.Transporter = model.Transporter;
                modelOut.RegNo = model.RegNo;
                modelOut.Driver = model.Driver;

                modelOut.Waybill = model.Waybill.Trim();

                var detail = wdb.sp_GetTransWaybillDetailByTrackId(NewTrackId).ToList();//(from a in wdb.mtTransportWaybillDetails where a.TrackId == NewTrackId select a).ToList();
                modelOut.Detail = detail;

                ViewBag.IsValidTrackId = true;
                //return View("Index", modelOut);
                return RedirectToAction("Index", new { TrackId = NewTrackId });



            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                if (model.TrackId == 0)
                {
                    ViewBag.IsValidTrackId = false;
                }
                else
                {
                    ViewBag.IsValidTrackId = true;
                }

                return View("Index", model);
            }
        }


        public bool CanSavePOD()
        {
            try
            {
                var Admin = (from a in mdb.mtUsers where a.Username == HttpContext.User.Identity.Name.ToUpper() && a.Administrator == true select a).ToList();
                if (Admin.Count > 0)
                {
                    return true;
                }
                var Emergency = (from a in mdb.mtOpFunctions where a.Username == HttpContext.User.Identity.Name.ToUpper() && a.Program == "Transport" && a.ProgramFunction == "SavePOD" select a).ToList();
                if (Emergency.Count > 0)
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

        public ActionResult DeleteWaybill(int TrackId, string Waybill, string DispatchNote, int DispatchNoteLine)
        {
            try
            {
                TransportSystemWaybillEntryViewModel model = new TransportSystemWaybillEntryViewModel();
                ViewBag.IsValidTrackId = true;


                using (var ddb = new WarehouseManagementEntities(""))
                {
                    DispatchNote = DispatchNote.PadLeft(15, '0');
                    var item = (from a in ddb.mtTransportWaybillDetails where a.TrackId == TrackId && a.Waybill == Waybill && a.DispatchNote == DispatchNote && a.DispatchNoteLine == DispatchNoteLine select a).FirstOrDefault();
                    ddb.Entry(item).State = EntityState.Deleted;
                    ddb.SaveChanges();
                }




                var Header = (from a in wdb.mtTransportWaybillHdrs where a.TrackId == TrackId select a).ToList();
                var detail = wdb.sp_GetTransWaybillDetailByTrackId(TrackId).ToList();//(from a in wdb.mtTransportWaybillDetails where a.TrackId == TrackId select a).ToList();



                model.TrackId = TrackId;
                model.Transporter = Header.FirstOrDefault().Transporter;
                model.RegNo = Header.FirstOrDefault().VehicleReg;
                model.Driver = Header.FirstOrDefault().Driver;
                model.Detail = detail;
                ModelState.AddModelError("", "Deleted Successfully!");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                TransportSystemWaybillEntryViewModel model = new TransportSystemWaybillEntryViewModel();
                ViewBag.IsValidTrackId = true;
                var Header = (from a in wdb.mtTransportWaybillHdrs where a.TrackId == TrackId select a).ToList();
                var detail = wdb.sp_GetTransWaybillDetailByTrackId(TrackId).ToList();//(from a in wdb.mtTransportWaybillDetails where a.TrackId == TrackId select a).ToList();
                model.TrackId = TrackId;
                model.Transporter = Header.FirstOrDefault().Transporter;
                model.RegNo = Header.FirstOrDefault().VehicleReg;
                model.Driver = Header.FirstOrDefault().Driver;
                model.Detail = detail;
                return View("Index", model);
            }
        }


        public ActionResult TransporterSearch()
        {
            return PartialView();
        }

        public ActionResult TransporterList(string FilterText)
        {
            //if (FilterText == "")
            //{
            FilterText = "";
            //}

            var result = wdb.sp_GetTransporters(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VehicleRegSearch()
        {
            return PartialView();
        }

        public ActionResult VehicleRegList(string Transporter, string FilterText)
        {
            //if (FilterText == "")
            //{
            //    FilterText = "NULL";
            //}
            FilterText = "";
            var result = wdb.sp_GetTransVehicleReg(Transporter, FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DispatchNoteSearch()
        {
            return PartialView();
        }

        public ActionResult DispatchNoteList(string Customer, string FilterText)
        {
            FilterText = "";
            var result = wdb.sp_GetTransDispatchNotes(Customer, FilterText);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CustomerSearch()
        {
            return PartialView();
        }

        public ActionResult CustomerList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            var result = wdb.sp_GetTransCustomers(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ProvinceSearch()
        {
            return PartialView();
        }

        public ActionResult ProvinceList()
        {
            var result = (from a in wdb.sp_GetTransLocations() where a.Type == "Province" select a).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TownSearch()
        {
            return PartialView();
        }

        public ActionResult TownList()
        {

            var result = (from a in wdb.sp_GetTransLocations() where a.Type == "Town" select a).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TrackIdSearch()
        {
            return PartialView();
        }

        public ActionResult TrackIdList()
        {

            var result = (from a in wdb.mtTransportWaybillDetails where (a.PurchaseOrder == null || a.PurchaseOrder == "") select a).OrderByDescending(a => a.TrackId).Take(1000).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult WaybillScanCapture()
        {
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PrintPdf")]
        public ActionResult PrintPdf(TransportSystemWaybillEntryViewModel model)
        {
            model.PrintPdf = ExportPdf(model.TrackId);
            ViewBag.CanSavePOD = CanSavePOD();
            ViewBag.IsValidTrackId = true;
            return View("Index", model);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PrintLoadVerification")]
        public ActionResult PrintLoadVerification(TransportSystemWaybillEntryViewModel model)
        {
            model.PrintVerificationPdf = ExportLoadVerfificationPdf(model.TrackId);
            ViewBag.CanSavePOD = CanSavePOD();
            ViewBag.IsValidTrackId = true;
            return View("Index", model);
        }

        public void PdfCleanup()
        {
            try
            {
                string[] files = Directory.GetFiles(HttpContext.Server.MapPath("~/Reports/TransportSystem/TransportSchedule/"));

                foreach (string delFile in files)
                {
                    FileInfo fi = new FileInfo(delFile);
                    if (fi.LastWriteTime < DateTime.Now.AddDays(-20)) fi.Delete();
                }
            }
            catch (Exception err)
            {

            }
        }

        public ExportFile ExportPdf(int TrackId)
        {
            try
            {
                var ReportPath = (from a in wdb.mtReportMasters where a.Program == "DynamicReports" && a.Report == "TransportScheduleDriverManifest" select a.ReportPath).FirstOrDefault().Trim();
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

                rpt.SetParameterValue("@TrackId", TrackId);

                string FilePath = HttpContext.Server.MapPath("~/Reports/TransportSystem/TransportSchedule/");

                string FileName = TrackId + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);

                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\Reports\TransportSystem\TransportSchedule\" + FileName;
                //file.FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/") + FileName;
                //file.FilePath = OutputPath;
                PdfCleanup();
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public ExportFile ExportLoadVerfificationPdf(int TrackId)
        {
            try
            {
                var ReportPath = (from a in wdb.mtReportMasters where a.Program == "DynamicReports" && a.Report == "TransportLoadVerification" select a.ReportPath).FirstOrDefault().Trim();
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

                rpt.SetParameterValue("@TrackId", TrackId);

                string FilePath = HttpContext.Server.MapPath("~/Reports/TransportSystem/TransportSchedule/");

                string FileName = "LoadVerification " + TrackId + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);

                //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, true, Report + "_" + DateTime.Now.Date);
                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);
                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                ExportFile file = new ExportFile();
                file.FileName = FileName;
                file.FilePath = @"..\Reports\TransportSystem\TransportSchedule\" + FileName;
                //file.FilePath = HttpContext.Current.Server.MapPath("~/RequisitionSystem/RequestForQuote/") + FileName;
                //file.FilePath = OutputPath;
                PdfCleanup();
                return file;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
