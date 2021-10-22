using LinqToExcel;
using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class SupplierContractPricingExcelImportController : Controller
    {

        WarehouseManagementEntities wdb = new WarehouseManagementEntities();
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore objSys = new SysproCore();
        SupplierContractsBL objPost = new SupplierContractsBL();

        [CustomAuthorize(Activity: "SupplierContractPricingImport")]
        public ActionResult Index(string HasFile = "", string WorkSheet = "")
        {
            if (HasFile != "")
            {

                string username = HttpContext.User.Identity.Name.ToUpper();
                //Load the file
                var excelFile = new ExcelQueryFactory(HasFile);

                var WorkSheetNames = excelFile.GetWorksheetNames().Select(a => new { Text = a.ToString(), Value = a.ToString() }).ToList();

                ViewBag.WorkSheetList = WorkSheetNames;

                var columnMapping = (from a in wdb.mtSupplierContractPricingColumnMappings where a.Username == username select a).FirstOrDefault();

                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.ContractReference, columnMapping.ContractReference);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.Supplier, columnMapping.Supplier);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.StockCode, columnMapping.StockCode);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.StartDate, columnMapping.StartDate);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.StartDate, columnMapping.StartDate);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.ExpiryDate, columnMapping.ExpiryDate);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.PurchasePrice, columnMapping.PurchasePrice);

                var ExcelData = (from c in excelFile.Worksheet<sp_GetSupplierContractsPricingData_Result>(WorkSheet)
                                 select c).Take(100).ToList();

                SupplierContractPriceViewModel contract = new SupplierContractPriceViewModel();
                contract.Detail = ExcelData;
                contract.FilePath = HasFile;
                contract.SheetName = WorkSheet;
                return View("Index", contract);
            }
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Import")]
        public ActionResult Import(HttpPostedFileBase FileUpload)
        {
            try
            {
                SupplierContractPriceViewModel model = new SupplierContractPriceViewModel();
                if (FileUpload != null)
                {
                    if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" || FileUpload.ContentType == "application/octet-stream")
                    {
                        string filename = FileUpload.FileName;

                        if (filename.EndsWith(".xlsx"))
                        {
                            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                            var SysproAdmin = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a).FirstOrDefault();
                            var Company = SysproAdmin.Company;
                            var FriendlyName = SysproAdmin.FriendlyCompanyCode;


                            if (!Directory.Exists(Server.MapPath("~/SupplierContractPricingImportTemp/" + Company + "/" + HttpContext.User.Identity.Name.ToUpper() + "/")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/SupplierContractPricingImportTemp/" + Company + "/" + HttpContext.User.Identity.Name.ToUpper() + "/"));
                            }

                            string targetpath = Server.MapPath("~/SupplierContractPricingImportTemp/" + Company + "/" + HttpContext.User.Identity.Name.ToUpper() + "/");

                            FileUpload.SaveAs(targetpath + filename);
                            string pathToExcelFile = targetpath + filename;

                            var excelFile = new ExcelQueryFactory(pathToExcelFile);
                            var WorkSheetNames = excelFile.GetWorksheetNames().Select(a => new { Text = a.ToString(), Value = a.ToString() }).ToList();

                            ViewBag.WorkSheetList = WorkSheetNames;

                            model.FileName = FileUpload.FileName;
                            model.FilePath = pathToExcelFile;
                        }

                        else
                        {
                            ModelState.AddModelError("", "Invalid file format. Expected format .xlsx");
                        }
                        return View("Index", model);
                    }
                    else
                    {

                        ModelState.AddModelError("", "Invalid file format. Expected excel file format .xlsx - " + FileUpload.ContentType.ToString());
                        return View("Index", model);

                    }

                }
                else
                {

                    ModelState.AddModelError("", "Please choose a file");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index");
            }
        }


        public ActionResult ContractPricingColumnMapper(string FilePath, string SelectedSheetName)
        {
            try
            {
                SupplierPriceColumnMapping model = new SupplierPriceColumnMapping();
                string username = HttpContext.User.Identity.Name.ToUpper();
                var columnMap = (from a in wdb.mtSupplierContractPricingColumnMappings where a.Username == username select a).FirstOrDefault();
                if (columnMap != null)
                {
                    model.ContractReference = columnMap.ContractReference;
                    model.Supplier = columnMap.Supplier;
                    model.StockCode = columnMap.StockCode;
                    model.StartDate = columnMap.StartDate;
                    model.ExpiryDate = columnMap.ExpiryDate;
                    model.PurchasePrice = columnMap.PurchasePrice;
                }


                model.FilePath = FilePath;
                model.SheetName = SelectedSheetName;
                var excelFile = new ExcelQueryFactory(FilePath);
                var ColumnNames = excelFile.GetColumnNames(SelectedSheetName).Select(a => new { Text = a.ToString(), Value = a.ToString() }).ToList();
                ViewBag.FieldList = ColumnNames;
                return PartialView(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return PartialView();
            }

        }


        [HttpPost]
        public ActionResult ContractPricingColumnMapper(SupplierPriceColumnMapping model)
        {
            try
            {

                //Load the file
                var excelFile = new ExcelQueryFactory(model.FilePath);

                var WorkSheetNames = excelFile.GetWorksheetNames().Select(a => new { Text = a.ToString(), Value = a.ToString() }).ToList();

                ViewBag.WorkSheetList = WorkSheetNames;

                SaveColumnMapping(model);

                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.ContractReference, model.ContractReference);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.Supplier, model.Supplier);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.StockCode, model.StockCode);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.StartDate, model.StartDate);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.ExpiryDate, model.ExpiryDate);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.PurchasePrice, model.PurchasePrice);

                var ExcelData = (from c in excelFile.Worksheet<sp_GetSupplierContractsPricingData_Result>(model.SheetName)
                                 select c).Take(100).ToList();

                SupplierContractPriceViewModel contract = new SupplierContractPriceViewModel();
                contract.Detail = ExcelData;
                contract.FilePath = model.FilePath;
                contract.SheetName = model.SheetName;

                return RedirectToAction("Index", new { HasFile = model.FilePath, WorkSheet = model.SheetName });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", new SupplierContractPriceViewModel());
            }

        }

        public void SaveColumnMapping(SupplierPriceColumnMapping model)
        {
            try
            {
                string username = HttpContext.User.Identity.Name.ToUpper();
                var check = (from a in wdb.mtSupplierContractPricingColumnMappings where a.Username == username select a).FirstOrDefault();
                if (check != null)
                {
                    check.ContractReference = model.ContractReference;
                    check.Supplier = model.Supplier;
                    check.StockCode = model.StockCode;
                    check.StartDate = model.StartDate;
                    check.ExpiryDate = model.ExpiryDate;
                    check.PurchasePrice = model.PurchasePrice;
                    wdb.Entry(check).State = System.Data.EntityState.Modified;
                    wdb.SaveChanges();
                }
                else
                {
                    mtSupplierContractPricingColumnMapping pref = new mtSupplierContractPricingColumnMapping();
                    pref.Username = username;
                    pref.ContractReference = model.ContractReference;
                    pref.Supplier = model.Supplier;
                    pref.StockCode = model.StockCode;
                    pref.StartDate = model.StartDate;
                    pref.ExpiryDate = model.ExpiryDate;
                    pref.PurchasePrice = model.PurchasePrice;
                    wdb.Entry(pref).State = System.Data.EntityState.Added;
                    wdb.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostContractPrice")]
        public ActionResult PostContractPrice(SupplierContractPriceViewModel model)
        {
            ModelState.Clear();
            try
            {
                string username = HttpContext.User.Identity.Name.ToUpper();
                //Load the file
                var excelFile = new ExcelQueryFactory(model.FilePath);

                var WorkSheetNames = excelFile.GetWorksheetNames().Select(a => new { Text = a.ToString(), Value = a.ToString() }).ToList();

                ViewBag.WorkSheetList = WorkSheetNames;

                var columnMapping = (from a in wdb.mtSupplierContractPricingColumnMappings where a.Username == username select a).FirstOrDefault();

                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.ContractReference, columnMapping.ContractReference);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.Supplier, columnMapping.Supplier);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.StockCode, columnMapping.StockCode);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.StartDate, columnMapping.StartDate);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.ExpiryDate, columnMapping.ExpiryDate);
                excelFile.AddMapping<SupplierPriceColumnMapping>(x => x.PurchasePrice, columnMapping.PurchasePrice);

                var ExcelData = (from c in excelFile.Worksheet<sp_GetSupplierContractsPricingData_Result>(model.SheetName)
                                 select c).ToList();
                SupplierContractPriceViewModel contract = new SupplierContractPriceViewModel();
                if (ExcelData.Count > 0)
                {
                    string Guid = objSys.SysproLogin();
                    string Document = objPost.BuildContractPriceDocument(ExcelData);
                    string Parameter = objPost.BuildContractPriceParameter();

                    string XmlOut = objSys.SysproPost(Guid, Parameter, Document, "PORTSC");

                    objSys.SysproLogoff(Guid);

                    string ErrorMessage = objSys.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {

                        ModelState.AddModelError("", "Posted Successfully");
                        return View("Index", contract);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Posting Failed. " + ErrorMessage + ". Please re-import the file.");
                        return View("Index", contract);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No data found. Please re-import the file.");
                    return View("Index", contract);
                }





            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", new SupplierContractPriceViewModel());
            }

        }

    }
}
