using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class MasterCardMaintenanceController : Controller
    {
        //
        // GET: /MasterCardMaintenance/
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();


        [CustomAuthorize("MasterCardMaintenance")]
        public ActionResult Index()
        {
            GetDropDownData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "Index")]
        public ActionResult Index(MaterCardMaintenanceViewModel model, IEnumerable<HttpPostedFileBase> FileUpload)
        {
            GetDropDownData();
            try
            {
                if (ModelState.IsValid)
                {
                    var OldModel = new sp_MasterCardGetStockCodeDataForUpdate_Result();
                    using (var odb = new WarehouseManagementEntities(""))
                    {
                        OldModel = odb.sp_MasterCardGetStockCodeDataForUpdate(model.StockCode, "MASTERCARD").FirstOrDefault();
                    }

                    //StockCode
                    model.stockCodeUpdate.Id = model.Id;
                    model.stockCodeUpdate.StockCode = model.StockCode;
                    if (model.stockCodeUpdate.JobClassification == null)
                    {
                        model.stockCodeUpdate.JobClassification = "";
                    }



                    db.Entry(model.stockCodeUpdate).State = System.Data.EntityState.Modified;
                    db.SaveChanges();



                    //Custom Form

                    model.FormUpdate.StockCode = model.StockCode;
                    model.FormUpdate.Id = model.stockCodeUpdate.Id;
                    db.Entry(model.FormUpdate).State = System.Data.EntityState.Modified;
                    db.SaveChanges();

                    var NewModel = new sp_MasterCardGetStockCodeDataForUpdate_Result();
                    using (var ndb = new WarehouseManagementEntities(""))
                    {
                        NewModel = ndb.sp_MasterCardGetStockCodeDataForUpdate(model.StockCode, "MasterCard").FirstOrDefault();
                    }

                    List<EntityChanges> audit = AuditHelper.EnumeratePropertyDifferences(OldModel, NewModel);
                    this.SaveAuditTrailStockCode(model.Id, "C", "StockCode", model.StockCode, audit);


                    //Warehouses
                    foreach (var item in model.warehouseUpdate_Result)
                    {
                        if (item.Allowed == true)
                        {
                            var check = (from a in db.mtMasterCardStockCodeWarehouseUpdates
                                         where a.Warehouse == item.Warehouse && item.Allowed == true
                                         select a).ToList();
                            if (check.Count == 0)
                            {

                                var SysproCheck = (from a in db.InvWarehouses where a.StockCode == model.StockCode && a.Warehouse == item.Warehouse select a.StockCode).FirstOrDefault();
                                if (string.IsNullOrWhiteSpace(SysproCheck))
                                {
                                    var obj = new mtMasterCardStockCodeWarehouseUpdate
                                    {
                                        Id = model.Id,
                                        StockCode = model.StockCode,
                                        Warehouse = item.Warehouse
                                    };
                                    db.mtMasterCardStockCodeWarehouseUpdates.Add(obj);
                                    db.SaveChanges();
                                    //var NewWarehouse = db.sp_MasterCardGetWarehouseUpdate(model.Id, model.StockCode).FirstOrDefault();
                                    this.SaveAuditTrailWarehouse(model.Id, "A", "AdditionalWarehouse", obj.StockCode, obj.Warehouse);
                                }

                            }

                        }
                    }


                    int Line = 0;
                    model.FileUpload = FileUpload;
                    foreach (var file in model.FileUpload)
                    {
                        if (file != null)
                        {
                            byte[] UploadFile = new byte[file.InputStream.Length];
                            file.InputStream.Read(UploadFile, 0, UploadFile.Length);
                            var isValid = (from a in db.mtMasterCardAttachments
                                           where a.StockCode == model.StockCode
                                           select a).FirstOrDefault();

                            if (isValid == null)
                            {
                                Line = 1;
                                mtMasterCardAttachment obj = new mtMasterCardAttachment()
                                {
                                    AuditId = model.Id,
                                    StockCode = model.StockCode,
                                    Line = Line,
                                    FileName = file.FileName,
                                    Attachment = UploadFile
                                };

                                db.mtMasterCardAttachments.Add(obj);
                                db.SaveChanges();
                                ModelState.AddModelError("", file.FileName + " Uploaded Successfully.");
                                this.SaveAuditTrailAttachments(obj.AuditId, "A", "File Attachment", obj.StockCode, obj.FileName);
                            }
                            else
                            {
                                var lineExist = (from a in db.mtMasterCardAttachments
                                                 where a.StockCode == model.StockCode
                                                 select a.Line).Max();

                                var Lines = lineExist + 1;
                                Line = Lines;
                                mtMasterCardAttachment obj = new mtMasterCardAttachment()
                                {
                                    AuditId = model.Id,
                                    StockCode = model.StockCode,
                                    Line = Line,
                                    FileName = file.FileName,
                                    Attachment = UploadFile
                                };
                                model.StockCode = obj.StockCode;
                                db.mtMasterCardAttachments.Add(obj);
                                db.SaveChanges();
                                ModelState.AddModelError("", file.FileName + " Uploaded Successfully.");
                                this.SaveAuditTrailAttachments(obj.AuditId, "A", "File Attachment", model.StockCode, obj.FileName);
                            }
                        }
                    }
                    var resultAttachment = db.sp_MasterCardGetAttachmentsByStockCode(model.StockCode).ToList();
                    model.AttachmentList = resultAttachment;


                    return View("Index", model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "GetMasterCardDetails")]
        public ActionResult LoadMasterCardDetails(MaterCardMaintenanceViewModel model)
        {
            GetDropDownData();
            ModelState.Clear();
            try
            {
                var update = (from x in db.mtMasterCardStockCodeUpdates
                              where x.StockCode == model.StockCode
                              select x).FirstOrDefault();
                if (update != null)
                {
                    var resultUpdate = db.sp_MasterCardGetStockCodeDataForUpdate(model.StockCode, "UPDATE").FirstOrDefault();
                    model.stockCodeUpdate = MapStockCodeFields(resultUpdate);
                    model.FormUpdate = MapCustomFormFields(resultUpdate);
                    model.Id = update.Id;
                    model.warehouseUpdate_Result = db.sp_MasterCardGetWarehouseUpdate(model.Id, model.StockCode).ToList();

                    var result = db.sp_MasterCardGetAttachmentsByStockCode(model.StockCode).ToList();
                    model.AttachmentList = result;


                }
                else
                {
                    var result = db.sp_MasterCardGetStockCodeDataForUpdate(model.StockCode, "SYSPRO").FirstOrDefault();
                    if (result != null)
                    {
                        model.stockCodeUpdate = MapStockCodeFields(result);
                        model.FormUpdate = MapCustomFormFields(result);
                        model.stockCodeUpdate.StockCode = model.StockCode;
                        db.mtMasterCardStockCodeUpdates.Add(model.stockCodeUpdate);
                        db.SaveChanges();

                        model.Id = model.stockCodeUpdate.Id;

                        model.FormUpdate.StockCode = model.StockCode;
                        model.FormUpdate.Id = model.stockCodeUpdate.Id;
                        db.mtMasterCardStockCodeCustomFormUpdates.Add(model.FormUpdate);
                        db.SaveChanges();
                        model.warehouseUpdate_Result = db.sp_MasterCardGetWarehouseUpdate(model.Id, model.StockCode).ToList();

                        var resultAttachment = db.sp_MasterCardGetAttachmentsByStockCode(model.StockCode).ToList();
                        model.AttachmentList = resultAttachment;

                    }
                    else
                    {
                        ModelState.AddModelError("", "StockCode not found.");
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

        public ActionResult StockCodeLookUp(string FilterText)
        {
            var result = db.InvMasters.ToList();

            var lookup = (from a in result
                          select new { a.StockCode, a.Description, a.StockUom, a.LongDesc }).ToList();
            return Json(lookup, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowseStockCode()
        {
            return PartialView();
        }

        public string CheckStockCodeExistsInSyspro(string StockCode)
        {
            try
            {
                var Syspro = (from a in db.InvMasters.AsNoTracking()
                              where a.StockCode == StockCode
                              select a.StockCode).ToList();

                if (Syspro.Count > 0)
                {
                    return "Y";
                }
                else
                {
                    return "N";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void GetDropDownData()
        {
            ViewBag.WarehouseList = (from a in db.InvWhControls select new { Value = a.Warehouse, Description = a.Warehouse + " - " + a.Description });
            ViewBag.JobClassList = (from a in db.WipJobClasses select new { Value = a.JobClassification, Description = a.JobClassification + " - " + a.ClassDescription });
            ViewBag.ProductClassList = (from a in db.SalProductClassDes select new { Value = a.ProductClass, Description = a.ProductClass + " - " + a.Description });
            ViewBag.StockUomList = (from a in db.mtMasterCardStockUoms select new { Value = a.StockUom, Description = a.StockUom + " - " + a.Description });
            ViewBag.IndustryList = (from a in db.AdmFormValidations where a.FormType == "STK" && a.FieldName == "Inv004" select new { Value = a.Item, Description = a.Description });
            ViewBag.ProductTypeList = (from a in db.AdmFormValidations where a.FormType == "STK" && a.FieldName == "Inv005" select new { Value = a.Item, Description = a.Description });
            ViewBag.ProductSubTypeList = (from a in db.AdmFormValidations where a.FormType == "STK" && a.FieldName == "Inv006" select new { Value = a.Item, Description = a.Description });
        }
        public JsonResult GetCopyStockCodeData(string StockCode)
        {
            try
            {
                var result = (db.sp_MasterCardGetStockCodeCopyData(StockCode).ToList());
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpGet]
        public JsonResult GetTreeData(string StockCode)
        {
            // var Parent = (from a in db.mtMasterCardHeaders where a.StockCode == StockCode select a.StockCode).FirstOrDefault();
            var bomstruct = db.sp_MasterCardGetSysproBomStructure(StockCode, "0").ToList().Select(a => new TreeModel { Parent = a.ParentPart, Component = a.Component }).ToList();

            if (bomstruct.Count > 0)
            {
                var output = MapToTreeModelJsonCollection(bomstruct);
                var json = JsonConvert.SerializeObject(output, Formatting.Indented);
                return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                List<TreeModel> tree = new List<TreeModel>();
                TreeModel topLevel = new TreeModel();
                topLevel.Parent = "";
                topLevel.Component = StockCode;
                tree.Add(topLevel);

                var output = MapToTreeModelJsonCollection(tree);
                var json = JsonConvert.SerializeObject(output, Formatting.Indented);
                return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

        }
        public class TreeModel
        {
            public string Component { get; set; }
            public string Parent { get; set; }
            public string text { get; set; }
        }

        static ICollection<TreeModelJson> MapToTreeModelJsonCollection(ICollection<TreeModel> source)
        {
            // map all items
            var allItems = source.Select(e => new TreeModelJson
            {
                Parent = e.Parent ?? "none",
                Component = e.Component,
                text = e.Component
            }).ToList();

            // build tree structure
            foreach (var item in allItems)
            {
                var children = allItems.Where(e => e.Parent == item.Component).ToList();
                if (children.Any())
                {
                    item.nodes = children;
                }
            }

            allItems.RemoveRange(1, allItems.Count - 1);
            // return only root items
            return allItems.ToList();

        }

        public class TreeViewNode
        {
            public string id { get; set; }
            public string parent { get; set; }
            public string text { get; set; }
        }
        class TreeModelJson
        {
            [JsonProperty("Parent")]
            public string Parent { get; set; }
            [JsonProperty("Component")]
            public string Component { get; set; }
            [JsonProperty("text")]
            public string text { get; set; }
            [JsonProperty("nodes", NullValueHandling = NullValueHandling.Ignore)]
            public ICollection<TreeModelJson> nodes { get; set; }
        }

        public mtMasterCardStockCodeUpdate MapStockCodeFields(sp_MasterCardGetStockCodeDataForUpdate_Result model)
        {
            mtMasterCardStockCodeUpdate obj = new mtMasterCardStockCodeUpdate();
            obj.StockCode = model.StockCode;
            obj.Description = model.Description;
            obj.LongDesc = model.LongDesc;
            obj.StockUom = model.StockUom;
            obj.AlternateUom = model.AlternateUom;
            obj.OtherUom = model.OtherUom;
            obj.ConvFactAltUom = model.ConvFactAltUom;
            obj.ConvFactOthUom = model.ConvFactOthUom;
            obj.ConvMulDiv = model.ConvMulDiv;
            obj.MulDiv = model.MulDiv;
            obj.Mass = model.Mass;
            obj.Volume = model.Volume;
            obj.Decimals = model.Decimals;
            obj.Micron = model.GenMicron;
            obj.PriceCategory = model.PriceCategory;
            obj.PriceMethod = model.PriceMethod;
            obj.PartCategory = model.PartCategory;
            obj.WarehouseToUse = model.WarehouseToUse;
            obj.JobClassification = model.JobClassification;
            obj.ProductClass = model.ProductClass;
            obj.Traceable = model.TraceableType;
            obj.TaxCode = model.TaxCode;
            obj.ListPriceCode = model.ListPriceCode;
            obj.SalesOrderAddText = model.SoText;
            obj.JobNarrations = model.JobText;
            return obj;
        }

        public mtMasterCardStockCodeCustomFormUpdate MapCustomFormFields(sp_MasterCardGetStockCodeDataForUpdate_Result model)
        {
            mtMasterCardStockCodeCustomFormUpdate obj = new mtMasterCardStockCodeCustomFormUpdate();
            obj.StockCode = model.StockCode;
            obj.InvoiceDim = model.InvoiceDim;
            obj.BarCode = model.BarCode;
            obj.GenWidth = model.GenWidth;
            obj.GenLayFlatWidthSiz = model.GenLayFlatWidthSiz;
            obj.GenMicron = model.GenMicron;
            obj.GenTreatment = model.GenTreatment;
            obj.GenDyneValue = model.GenDyneValue;
            obj.GenSlit = model.GenSlit;
            obj.GenInkCost1000 = model.GenInkCost1000;
            obj.GenPalletWrapRoll = model.GenPalletWrapRoll;
            obj.GenPalletRolls = model.GenPalletRolls;
            obj.PrintPrintFront = model.PrintPrintFront;
            obj.PrintPrintBack = model.PrintPrintBack;
            obj.PrintStepSize = model.PrintStepSize;
            obj.PrintCylinderSize = model.PrintCylinderSize;
            obj.PrintAround = model.PrintAround;
            obj.PrintAcross = model.PrintAcross;
            obj.PrintCoverageF = model.PrintCoverageF;
            obj.PrintCoverageB = model.PrintCoverageB;
            obj.PrintLinePrint = model.PrintLinePrint;
            obj.BagWidthSize = model.BagWidthSize;
            obj.BagLengthSize = model.BagLengthSize;
            obj.BagTopGusset = model.BagTopGusset;
            obj.BagBottomGusset = model.BagBottomGusset;
            obj.BagRightGusset = model.BagRightGusset;
            obj.BagLeftGusset = model.BagLeftGusset;
            obj.BagLipSize = model.BagLipSize;
            obj.BagHeaderSeal = model.BagHeaderSeal;
            obj.BagSealType = model.BagSealType;
            obj.BagPerPack = model.BagPerPack;
            obj.BagPerBale = model.BagPerBale;
            obj.ExtrRollsUp = model.ExtrRollsUp;
            obj.ExtrKgPerRoll = model.ExtrKgPerRoll;
            obj.ExtrMetresPerRoll = model.ExtrMetresPerRoll;
            obj.ExtrLFWidthSize = model.ExtrLFWidthSize;
            obj.ExtrDoubleWind = model.ExtrDoubleWind;
            obj.ExtrCoreWeight = model.ExtrCoreWeight;
            obj.SlitSheetSlits = model.SlitSheetSlits;
            obj.SlitRollsUp = model.SlitRollsUp;
            obj.ExtrCoreWall = model.ExtrCoreWall;
            obj.ExtrSlittingCode = model.ExtrSlittingCode;
            obj.PrintPitch = model.PrintPitch;
            obj.PrintMReel = model.PrintMReel;
            obj.PrintLayFlatWidth = model.PrintLayFlatWidth;
            obj.PrintFinalUnwind = model.PrintFinalUnwind;
            obj.SlitWidth = model.SlitWidth;
            obj.SlitTolerance = model.SlitTolerance;
            obj.SlitKgReel = model.SlitKgReel;
            obj.SlitInterleaved = model.SlitInterleaved;
            obj.SlitCoreID = model.SlitCoreID;
            obj.SlitReelOdMin = model.SlitReelOdMin;
            obj.SlitReelOdMax = model.SlitReelOdMax;
            obj.SlitEyemarkSize = model.SlitEyemarkSize;
            obj.SlitHolePunchSize = model.SlitHolePunchSize;
            obj.BagUnitsBundle = model.BagUnitsBundle;
            obj.BagBundlesParcel = model.BagBundlesParcel;
            obj.BagBagUnitsParcel = model.BagBagUnitsParcel;
            obj.MaterialCost = model.MaterialCost;
            obj.ActualInkCost = model.ActualInkCost;
            obj.SpineSeal = model.SpineSeal;
            obj.Grams = model.Grams;
            obj.Inkd = model.Inkd;
            obj.PrintRepeat = model.PrintRepeat;
            obj.BagOpening = model.BagOpening;
            obj.Handle = model.Handle;
            obj.PunchHoles = model.PunchHoles;
            obj.Wicket = model.Wicket;
            obj.Perforation = model.Perforation;
            obj.ReelOd = model.ReelOd;
            obj.BagSeal = model.BagSeal;
            obj.NewManufacturingCo = model.NewManufacturingCo;
            obj.OldManufacturingCo = model.OldManufacturingCo;
            obj.NewInksSolventsCos = model.NewInksSolventsCos;
            obj.OldInksSolventsCos = model.OldInksSolventsCos;
            obj.NewAdhesivesGlueCo = model.NewAdhesivesGlueCo;
            obj.OldAdhesivesGlueCo = model.OldAdhesivesGlueCo;
            obj.NewConsumablesCost = model.NewConsumablesCost;
            obj.OldConsumablesCost = model.OldConsumablesCost;
            obj.NewTransportCost = model.NewTransportCost;
            obj.OldTransportCost = model.OldTransportCost;
            obj.NewOtherCost = model.NewOtherCost;
            obj.OldOtherCost = model.OldOtherCost;
            obj.InkSystem = model.InkSystem;
            obj.RawMaterialCode = model.RawMaterialCode;
            obj.GenInkCostKg = model.GenInkCostKg;
            obj.LabelMicron = model.LabelMicron;
            obj.MD = model.MD;
            obj.TD = model.TD;
            obj.CofFF = model.CofFF;
            obj.CofFS = model.CofFS;
            obj.ExtrSlitWidth = model.ExtrSlitWidth;
            obj.ApprovedInkCode = model.ApprovedInkCode;
            obj.SuppCurrInkCost = model.SuppCurrInkCost;
            obj.PressReturn = model.PressReturn;
            obj.BoxColour = model.BoxColour;
            obj.NoCalc = model.NoCalc;
            obj.ProductType = model.ProductType;
            obj.ProductSubType1 = model.ProductSubType1;
            obj.Industry = model.Industry;
            obj.AltName = model.AltName;

            return obj;
        }

        public mtMasterCardBomOperationsUpdate MapBomFields(sp_MasterCardGetStockCodeDataForUpdate_Result model)
        {
            mtMasterCardBomOperationsUpdate obj = new mtMasterCardBomOperationsUpdate();

            return obj;
        }

        [HttpGet]
        public ActionResult GetStockCodeOperations(string ParentPart, int Id)
        {
            MaterCardMaintenanceViewModel model = new MaterCardMaintenanceViewModel();

            try
            {
                var result = db.sp_MasterCardUpdateGetOperationsByStockCode(ParentPart, Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult StockCodeList()
        {
            var result = db.sp_MasterCardGetAllStockCodes("").ToList();
            var Stock = (from a in result select new { MStockCode = a.StockCode, MStockDes = a.Description, MStockingUom = a.StockUom }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetParentComponents(string ParentPart, int Id)
        {
            try
            {
                var result = db.sp_MasterCardUpdateGetComponentsByParentPart(ParentPart, Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CheckIfStockCodeExist(string StockCode)
        {
            try
            {
                var result = (from x in db.mtMasterCardStockCodeUpdates
                              where x.StockCode == StockCode
                              select x).ToList();
                if (result.Count > 0)
                {
                    return Json("Y", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("N", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json("N", ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetSelectedStockCodeDesc(string StockCode)
        {
            try
            {
                var Syspro = (from a in db.InvMasters.AsNoTracking() where a.StockCode == StockCode select new { StockCode = a.StockCode, Description = a.Description, LongDesc = a.LongDesc }).ToList();
                if (Syspro.Count > 0)
                {
                    return Json(Syspro, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var NonStock = (from a in db.mtNonStockMasters.AsNoTracking() where a.StockCode == StockCode select new { StockCode = a.StockCode, Description = a.Description, LongDesc = a.LongDesc }).ToList();
                    return Json(NonStock, JsonRequestBehavior.AllowGet); ;
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BomComponent(int id, string ParentPart, string Mode, string Component = null, string SequenceNum = null)
        {
            ModelState.Clear();
            MasterCardComponent obj = new MasterCardComponent();
            obj.Id = id;
            obj.ParentPart = ParentPart;
            obj.Mode = Mode;
            if (Component != null)
            {
                ViewBag.NewComponent = false;
                obj = new MasterCardComponent();
                obj = (from a in db.sp_MasterCardUpdateGetComponentsByParentPart(ParentPart, id) where a.ParentPart == ParentPart && a.Component == Component && a.SequenceNum == SequenceNum select new MasterCardComponent { Id = id, ParentPart = a.ParentPart, Component = a.Component, Route = a.Route, SequenceNum = a.SequenceNum, QtyPer = a.QtyPer, LayerPerc = a.LayerPerc, ScrapQuantity = a.ScrapQty, ScrapPercentage = a.ScrapPerc, Mode = Mode }).FirstOrDefault();
                if (obj != null)
                {
                    return PartialView(obj);
                }
            }
            else
            {
                obj.Id = id;
                obj.ParentPart = ParentPart;
                obj.Mode = Mode;
                ViewBag.NewComponent = true;

            }
            return PartialView(obj);

        }

        public ActionResult BomOperation(int id, string StockCode, string Mode, decimal Operation = 0)
        {
            ModelState.Clear();
            ViewBag.WorkCentreList = (from m in db.BomWorkCentres select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
            if (Mode == "Add")
            {
                MasterCardBomOperation obj = new MasterCardBomOperation();
                obj.Id = id;
                obj.StockCode = StockCode;
                obj.Mode = Mode;
                return PartialView(obj);
            }
            else
            {
                var obj = (from a in db.sp_MasterCardUpdateGetOperationsByStockCode(StockCode, id) where a.Operation == Operation select new MasterCardBomOperation { Id = id, StockCode = a.StockCode, Operation = a.Operation, Route = a.Route, WorkCentre = a.WorkCentre, Mode = Mode, UnitRunTime = (decimal)a.IRunTime, Quantity = (decimal)a.IQuantity, TimeTaken = (decimal)a.ITimeTaken, Narrations = a.Narration }).FirstOrDefault();
                if (obj != null)
                {
                    obj.Narrations = obj.Narrations.Replace("\r", "");
                    obj.Narrations = obj.Narrations.Replace("||||", Environment.NewLine);
                    return PartialView(obj);

                }
                return PartialView(obj);

            }

        }

        [HttpPost]
        public ActionResult SaveComponent(string details)
        {
            try
            {
                List<MasterCardComponent> myDeserializedObject = (List<MasterCardComponent>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MasterCardComponent>));
                var bom = myDeserializedObject.FirstOrDefault();
                var ActionType = "";
                if (bom.Mode == "Change")
                {
                    //check if bom structure 
                    var checkbom = (from x in db.BomStructures
                                    where x.ParentPart == bom.ParentPart && x.Component == bom.Component && x.Route == "0"
                                    select x).FirstOrDefault();
                    if (checkbom == null)
                    {
                        ActionType = "A";
                    }
                    else
                    {
                        ActionType = "C";
                    }

                }
                else if (bom.Mode == "Add")
                {
                    ActionType = "A";
                }
                else
                {
                    ActionType = "D";
                }


                var check = (from x in db.mtMasterCardBomStructureUpdates
                             where x.Id == bom.Id && x.ParentPart == bom.ParentPart && x.Component == bom.Component
                             select x).FirstOrDefault();
                var oldValue = new mtMasterCardBomStructureUpdate();
                using (var odb = new WarehouseManagementEntities(""))
                {
                    oldValue = (from x in odb.mtMasterCardBomStructureUpdates
                                where x.Id == bom.Id && x.ParentPart == bom.ParentPart && x.Component == bom.Component
                                select x).FirstOrDefault();
                }


                if (check != null)
                {
                    check.ActionType = ActionType;
                    check.QtyPer = bom.QtyPer;
                    check.LayerPerc = bom.LayerPerc;
                    check.ScrapQuantity = bom.ScrapQuantity;
                    check.ScrapPercentage = bom.ScrapPercentage;
                    check.DateLastSaved = DateTime.Now;
                    check.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                    db.Entry(check).State = EntityState.Modified;
                    db.SaveChanges();

                }
                else
                {
                    if (ActionType == "C")
                    {
                        using (var wdb = new WarehouseManagementEntities(""))
                        {
                            //oldValue = (from a in wdb.BomStructures.AsNoTracking() where a.ParentPart == bom.ParentPart && a.Component == bom.Component select new mtMasterCardBomStructureUpdate { Id = bom.Id, ActionType = "C", ParentPart = bom.ParentPart, Component = bom.Component, QtyPer = a.QtyPer, Route = bom.Route, ScrapQuantity = a.ScrapQuantity, ScrapPercentage = a.ScrapPercentage }).FirstOrDefault();
                            oldValue = GetSysproBomStructure(bom.ParentPart, bom.Component, bom.Id);
                        }

                    }

                    string SequenceNum = "000000";
                    mtMasterCardBomStructureUpdate obj = new mtMasterCardBomStructureUpdate();
                    obj.Id = bom.Id;
                    obj.ParentPart = bom.ParentPart;
                    obj.Component = bom.Component;
                    obj.Route = "0";
                    obj.SequenceNum = SequenceNum;
                    obj.Source = "B";
                    obj.ActionType = ActionType;
                    obj.QtyPer = bom.QtyPer;
                    obj.LayerPerc = bom.LayerPerc;
                    obj.ScrapQuantity = bom.ScrapQuantity;
                    obj.ScrapPercentage = bom.ScrapPercentage;
                    obj.DateLastSaved = DateTime.Now;
                    obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                    db.Entry(obj).State = EntityState.Added;
                    db.SaveChanges();

                }

                if (ActionType != "C")
                {
                    this.SaveAuditTrailBomComponentsAdd(bom.Id, ActionType, "BomComponent", bom.ParentPart, bom.Component);
                }
                else
                {
                    var newValue = new mtMasterCardBomStructureUpdate();
                    using (var ndb = new WarehouseManagementEntities(""))
                    {
                        newValue = (from x in ndb.mtMasterCardBomStructureUpdates
                                    where x.Id == bom.Id && x.ParentPart == bom.ParentPart && x.Component == bom.Component
                                    select x).FirstOrDefault();
                    }

                    List<EntityChanges> audit = AuditHelper.EnumeratePropertyDifferences(oldValue, newValue);
                    this.SaveAuditTrailBomComponentsChanges(bom.Id, ActionType, "BomComponent", bom.ParentPart, bom.Component, audit);
                }

                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        public mtMasterCardBomOperationsUpdate GetSysproBomOperation(string StockCode, decimal Operation, int Id)
        {
            try
            {


                //var sysproBom = (from a in db.BomOperations.AsNoTracking() where a.StockCode == StockCode && a.Operation == Operation select new { Id = Id, ActionType = "C", StockCode = StockCode, Operation = Operation, WorkCentre = a.WorkCentre, Route = "0", UnitRunTime = a.IRunTime, TimeTaken = a.ITimeTaken, Quantity = a.IQuantity, Narration = "" }).FirstOrDefault();
                var sysproBom = (from a in db.sp_MasterCardUpdateGetOperationsByStockCode(StockCode, Id) where a.StockCode == StockCode && a.Operation == Operation select new { Id = Id, ActionType = "C", StockCode = StockCode, Operation = Operation, WorkCentre = a.WorkCentre, Route = "0", UnitRunTime = a.IRunTime, TimeTaken = a.ITimeTaken, Quantity = a.IQuantity, Narration = a.Narration }).FirstOrDefault();
                mtMasterCardBomOperationsUpdate obj = new mtMasterCardBomOperationsUpdate();
                obj.Id = Id;
                obj.ActionType = "C";
                obj.StockCode = StockCode;
                obj.Operation = Operation;
                obj.WorkCentre = sysproBom.WorkCentre;
                obj.UnitRunTime = sysproBom.UnitRunTime;
                obj.Quantity = sysproBom.Quantity;
                obj.TimeTaken = sysproBom.TimeTaken;
                obj.Route = sysproBom.Route;
                obj.Narration = sysproBom.Narration;
                return obj;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public mtMasterCardBomStructureUpdate GetSysproBomStructure(string ParentPart, string Component, int Id)
        {
            try
            {
                var sysproBom = (from a in db.BomStructures.AsNoTracking() where a.ParentPart == ParentPart && a.Component == Component select new { Id = Id, ActionType = "C", ParentPart = ParentPart, Component = Component, QtyPer = a.QtyPer, Route = "0", ScrapQuantity = a.ScrapQuantity, ScrapPercentage = a.ScrapPercentage }).FirstOrDefault();
                mtMasterCardBomStructureUpdate obj = new mtMasterCardBomStructureUpdate();
                obj.Id = Id;
                obj.ActionType = "C";
                obj.ParentPart = ParentPart;
                obj.Component = Component;
                obj.QtyPer = sysproBom.QtyPer;
                obj.Route = "0";
                obj.ScrapQuantity = sysproBom.ScrapQuantity;
                obj.ScrapPercentage = sysproBom.ScrapPercentage;
                return obj;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult SaveOperation(string details)
        {
            try
            {
                List<MasterCardBomOperation> myDeserializedObject = (List<MasterCardBomOperation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MasterCardBomOperation>));

                var bom = myDeserializedObject.FirstOrDefault();

                var ActionType = "";
                if (bom.Mode == "Change")
                {
                    var checkbom = (from x in db.BomOperations
                                    where x.StockCode == bom.StockCode && x.Route == "0" && x.Operation == bom.Operation
                                    select x).FirstOrDefault();
                    if (checkbom == null)
                    {
                        ActionType = "A";
                    }
                    else
                    {
                        ActionType = "C";
                    }
                }
                else if (bom.Mode == "Add")
                {
                    ActionType = "A";
                    bom.Operation = 0;
                }
                else
                {
                    ActionType = "D";
                }
                var check = (from a in db.mtMasterCardBomOperationsUpdates
                             where a.Id == bom.Id && a.StockCode == bom.StockCode && a.Route == bom.Route && a.Operation == bom.Operation
                             select a).FirstOrDefault();

                var oldValue = new mtMasterCardBomOperationsUpdate();
                using (var odb = new WarehouseManagementEntities(""))
                {
                    oldValue = (from a in odb.mtMasterCardBomOperationsUpdates
                                where a.Id == bom.Id && a.StockCode == bom.StockCode && a.Route == bom.Route && a.Operation == bom.Operation
                                select a).FirstOrDefault();
                }
                if (check != null)
                {
                    check.ActionType = ActionType;
                    check.WorkCentre = bom.WorkCentre;
                    check.UnitRunTime = bom.UnitRunTime;
                    check.TimeTaken = bom.TimeTaken;
                    check.Quantity = bom.Quantity;
                    check.Narration = bom.Narrations;
                    check.DateLastSaved = DateTime.Now;
                    check.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                    db.Entry(check).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {


                    if (ActionType == "C")
                    {
                        using (var wdb = new WarehouseManagementEntities(""))
                        {
                            //oldValue = (from a in wdb.BomOperations.AsNoTracking()
                            //where a.StockCode == bom.StockCode && a.Operation == bom.Operation
                            //select new mtMasterCardBomOperationsUpdate { Id = bom.Id, ActionType = "C", StockCode = bom.StockCode, Operation = bom.Operation, WorkCentre = a.WorkCentre, Route = bom.Route, UnitRunTime = a.IRunTime, TimeTaken = a.ITimeTaken, Quantity = a.IQuantity, Narration = bom.Narrations }).FirstOrDefault();
                            oldValue = GetSysproBomOperation(bom.StockCode, bom.Operation, bom.Id);
                            oldValue.Narration = oldValue.Narration.Replace("\r", "");
                            oldValue.Narration = oldValue.Narration.Replace("||||", Environment.NewLine);
                        }

                    }


                    mtMasterCardBomOperationsUpdate obj = new mtMasterCardBomOperationsUpdate();
                    obj.Id = bom.Id;
                    obj.StockCode = bom.StockCode;
                    obj.Operation = bom.Operation;
                    obj.Route = bom.Route;
                    obj.ActionType = ActionType;
                    obj.WorkCentre = bom.WorkCentre;
                    obj.UnitRunTime = bom.UnitRunTime;
                    obj.TimeTaken = bom.TimeTaken;
                    obj.Quantity = bom.Quantity;
                    obj.Narration = bom.Narrations;
                    obj.DateLastSaved = DateTime.Now;
                    obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                    db.Entry(obj).State = EntityState.Added;
                    db.SaveChanges();
                }
                if (ActionType != "C")
                {
                    this.SaveAuditTrailBomOperationsAdd(bom.Id, ActionType, "BomOperations", bom.StockCode, bom.WorkCentre, bom.Operation);
                }
                else
                {
                    var newValue = new mtMasterCardBomOperationsUpdate();
                    using (var ndb = new WarehouseManagementEntities(""))
                    {
                        newValue = (from x in ndb.mtMasterCardBomOperationsUpdates
                                    where x.Id == bom.Id && x.StockCode == bom.StockCode && x.WorkCentre == bom.WorkCentre
                                    select x).FirstOrDefault();
                    }


                    List<EntityChanges> audit = AuditHelper.EnumeratePropertyDifferences(oldValue, newValue);
                    this.SaveAuditTrailBomOperationsChanges(bom.Id, ActionType, "BomOperation", bom.StockCode, bom.WorkCentre, bom.Operation, audit);
                }

                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (DbEntityValidationException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "PostStockCode")]
        public ActionResult PostStockCode(MaterCardMaintenanceViewModel model)
        {
            GetDropDownData();
            try
            {
                SysproCore sys = new SysproCore();
                //Do Login Here. Remove all other logins
                string Guid = sys.SysproLogin();
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    //Handle error
                    //Modelstate.add
                    //return View;
                }

                MasterCardMaintenance maintenance = new MasterCardMaintenance();
                var result = maintenance.PostStockCodeCreation(model.Id, Guid);
                ModelState.AddModelError("", result);

                var resultBomAdd = maintenance.PostBom(model.Id, "A", Guid);
                ModelState.AddModelError("", resultBomAdd);


                var resultBomUpdate = maintenance.PostBom(model.Id, "C", Guid);
                ModelState.AddModelError("", resultBomUpdate);

                var resultBomDelete = maintenance.PostBom(model.Id, "D", Guid);
                ModelState.AddModelError("", resultBomDelete);
                int Id = model.Id;



                var check = (from x in db.mtMasterCardUpdateAudits
                             where x.KeyId == Id && x.Posted == null
                             select x).ToList();
                if (check.Count > 0)
                {
                    foreach (var item in check)
                    {
                        item.Posted = true;
                        item.DatePosted = DateTime.Now;
                        item.PostedBy = HttpContext.User.Identity.Name.ToUpper();
                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                }



                var stockCodeDelete = (from a in db.mtMasterCardStockCodeUpdates where a.Id == model.Id select a).FirstOrDefault();
                db.Entry(stockCodeDelete).State = EntityState.Deleted;
                db.SaveChanges();

                var CustomFormsDelete = (from a in db.mtMasterCardStockCodeCustomFormUpdates where a.Id == model.Id select a).FirstOrDefault();
                db.Entry(CustomFormsDelete).State = EntityState.Deleted;
                db.SaveChanges();

                var BomOppDelete = (from a in db.mtMasterCardBomOperationsUpdates where a.Id == model.Id select a).ToList();
                foreach (var item in BomOppDelete)
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                }

                var BomComponentDelete = (from a in db.mtMasterCardBomStructureUpdates where a.Id == model.Id select a).ToList();
                foreach (var item in BomComponentDelete)
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                }

                var WarehouseDelete = (from a in db.mtMasterCardStockCodeWarehouseUpdates where a.Id == model.Id select a).ToList();
                foreach (var item in WarehouseDelete)
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                }
                return View("Index", model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex);
            }
            finally
            {
                //Logoff
            }
            return View("Index", model);
        }

        public void SaveAuditTrailStockCode(int KeyId, string TrnType, string Program, string StockCode, List<EntityChanges> audit)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                if (audit.Count > 0)
                {
                    foreach (var item in audit)
                    {
                        if (item.KeyField == "SoText" || item.KeyField == "JobText")
                        {
                            if (item.OldValue != item.NewValue.Replace("\n", string.Empty))
                            {
                                mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                                obj.KeyId = KeyId;
                                obj.TrnType = TrnType;
                                obj.Program = Program;
                                obj.StockCode = StockCode;
                                obj.KeyField = item.KeyField;
                                obj.OldValue = item.OldValue;
                                obj.NewValue = item.NewValue;
                                obj.Username = Username;
                                obj.TrnDate = DateTime.Now;
                                db.Entry(obj).State = EntityState.Added;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                            obj.KeyId = KeyId;
                            obj.TrnType = TrnType;
                            obj.Program = Program;
                            obj.StockCode = StockCode;
                            obj.KeyField = item.KeyField;
                            obj.OldValue = item.OldValue;
                            obj.NewValue = item.NewValue;
                            obj.Username = Username;
                            obj.TrnDate = DateTime.Now;
                            db.Entry(obj).State = EntityState.Added;
                            db.SaveChanges();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveAuditTrailCustomForm(int KeyId, string TrnType, string Program, string StockCode, List<EntityChanges> audit)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                if (audit.Count > 0)
                {
                    foreach (var item in audit)
                    {
                        mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                        obj.KeyId = KeyId;
                        obj.TrnType = TrnType;
                        obj.Program = Program;
                        obj.StockCode = StockCode;
                        obj.KeyField = item.KeyField;
                        obj.OldValue = item.OldValue;
                        obj.NewValue = item.NewValue;
                        obj.Username = Username;
                        obj.TrnDate = DateTime.Now;
                        db.Entry(obj).State = EntityState.Added;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveAuditTrailWarehouse(int KeyId, string TrnType, string Program, string StockCode, string Warehouse)
        {
            try
            {
                // EntityChanges audit= new EntityChanges();
                string Username = HttpContext.User.Identity.Name.ToUpper();
                mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                obj.KeyId = KeyId;
                obj.TrnType = TrnType;
                obj.Program = Program;
                obj.StockCode = StockCode;
                obj.KeyField = "Warehouse";
                obj.OldValue = "";
                obj.NewValue = Warehouse;
                obj.Username = Username;
                obj.TrnDate = DateTime.Now;
                db.Entry(obj).State = EntityState.Added;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveAuditTrailBomComponentsAdd(int KeyId, string TrnType, string Program, string StockCode, string Component)
        {
            try
            {
                // EntityChanges audit= new EntityChanges();
                string Username = HttpContext.User.Identity.Name.ToUpper();
                mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                obj.KeyId = KeyId;
                obj.TrnType = TrnType;
                obj.Program = Program;
                obj.StockCode = StockCode;
                obj.KeyField = "Component";
                obj.OldValue = "";
                obj.NewValue = Component;
                obj.Username = Username;
                obj.TrnDate = DateTime.Now;
                db.Entry(obj).State = EntityState.Added;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveAuditTrailBomComponentsChanges(int KeyId, string TrnType, string Program, string StockCode, string Component, List<EntityChanges> audit)
        {
            try
            {
                string[] stringArray = { "SequenceNum", "Source", "LayerPerc", "LastSavedBy", "DateLastSaved" };

                string Username = HttpContext.User.Identity.Name.ToUpper();
                foreach (var item in audit)
                {
                    if (!stringArray.Any(item.KeyField.Contains))
                    {
                        mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                        obj.KeyId = KeyId;
                        obj.TrnType = TrnType;
                        obj.Program = Program;
                        obj.StockCode = StockCode;
                        obj.KeyField = item.KeyField;
                        obj.OldValue = "";
                        obj.NewValue = item.NewValue;
                        obj.Username = Username;
                        obj.TrnDate = DateTime.Now;
                        obj.ParentPart = StockCode;
                        obj.Component = Component;
                        db.Entry(obj).State = EntityState.Added;
                        db.SaveChanges();
                    }

                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveAuditTrailBomOperationsAdd(int KeyId, string TrnType, string Program, string StockCode, string workCentre, decimal Operation)
        {
            try
            {
                // EntityChanges audit= new EntityChanges();
                string Username = HttpContext.User.Identity.Name.ToUpper();
                mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                obj.KeyId = KeyId;
                obj.TrnType = TrnType;
                obj.Program = Program;
                obj.StockCode = StockCode;
                obj.KeyField = "WorkCentre-Operation";
                obj.OldValue = "";
                obj.NewValue = (workCentre + " - " + Operation.ToString());
                obj.Username = Username;
                obj.TrnDate = DateTime.Now;
                db.Entry(obj).State = EntityState.Added;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveAuditTrailBomOperationsChanges(int KeyId, string TrnType, string Program, string StockCode, string workCentre, decimal Operation, List<EntityChanges> audit)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                string[] stringArray = { "Route", "LastSavedBy", "DateLastSaved" };
                foreach (var item in audit)
                {
                    if (!stringArray.Any(item.KeyField.Contains))
                    {
                        if (item.KeyField == "Narration")
                        {
                            mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                            obj.KeyId = KeyId;
                            obj.TrnType = TrnType;
                            obj.Program = Program;
                            obj.StockCode = StockCode;
                            obj.KeyField = item.KeyField;
                            obj.OldValue = item.OldValue;
                            obj.NewValue = item.NewValue.Replace("\n", string.Empty);
                            obj.Username = Username;
                            obj.TrnDate = DateTime.Now;
                            db.Entry(obj).State = EntityState.Added;
                            db.SaveChanges();
                        }
                        else
                        {
                            mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                            obj.KeyId = KeyId;
                            obj.TrnType = TrnType;
                            obj.Program = Program;
                            obj.StockCode = StockCode;
                            obj.KeyField = item.KeyField;
                            obj.OldValue = item.OldValue;
                            obj.NewValue = item.NewValue;
                            obj.Username = Username;
                            obj.TrnDate = DateTime.Now;
                            db.Entry(obj).State = EntityState.Added;
                            db.SaveChanges();
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [MultipleButton(Name = "action", Argument = "Review")]
        [HttpPost]
        public ActionResult Review(int Id)
        {
            ViewBag.CanPost = ProgramAccess("PostMasterCardMaintenance");
            MaterCardMaintenanceViewModel model = new MaterCardMaintenanceViewModel();
            var result = (from x in db.mtMasterCardUpdateAudits
                          where x.Posted == null && x.KeyId == Id
                          select x).ToList();
            if (result.Count > 0)
            {
                model.Id = result.FirstOrDefault().KeyId;
                model.updateAudits = result;
                return View("Review", model);
            }
            else
            {
                ModelState.AddModelError("", "No Data To Be Posted");
            }
            return View("Review", model);
        }

        public bool ProgramAccess(string ProgramFunction)
        {
            try
            {
                var Admin = (from a in mdb.mtUsers where a.Username == HttpContext.User.Identity.Name.ToUpper() && a.Administrator == true select a).ToList();
                if (Admin.Count > 0)
                {
                    return true;
                }
                var HasAccess = (from a in mdb.mtOpFunctions where a.Username == HttpContext.User.Identity.Name.ToUpper() && a.Program == "MasterCard" && a.ProgramFunction == ProgramFunction select a).ToList();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "CancelPosting")]
        public ActionResult CancelPosting(MaterCardMaintenanceViewModel model)//<<--Undo all 'Logs'-->>
        {
            GetDropDownData();
            try
            {
                var stockCodeDelete = (from a in db.mtMasterCardStockCodeUpdates where a.Id == model.Id select a).FirstOrDefault();
                db.Entry(stockCodeDelete).State = EntityState.Deleted;
                db.SaveChanges();



                var CustomFormsDelete = (from a in db.mtMasterCardStockCodeCustomFormUpdates where a.Id == model.Id select a).FirstOrDefault();
                db.Entry(CustomFormsDelete).State = EntityState.Deleted;
                db.SaveChanges();


                var BomOppDelete = (from a in db.mtMasterCardBomOperationsUpdates where a.Id == model.Id select a).ToList();
                foreach (var item in BomOppDelete)
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                }



                var BomComponentDelete = (from a in db.mtMasterCardBomStructureUpdates where a.Id == model.Id select a).ToList();
                foreach (var item in BomComponentDelete)
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                }


                var WarehouseDelete = (from a in db.mtMasterCardStockCodeWarehouseUpdates where a.Id == model.Id select a).ToList();
                foreach (var item in WarehouseDelete)
                {
                    db.Entry(item).State = EntityState.Deleted;
                    db.SaveChanges();
                }

                this.SaveAuditTrailUndo(model.Id, stockCodeDelete.StockCode);
                ModelState.AddModelError("", "All Posting Data Canceled");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex);
            }
            return View("Index", model);

        }

        public void SaveAuditTrailUndo(int KeyId, string StockCode)
        {
            try
            {
                // EntityChanges audit= new EntityChanges();
                string Username = HttpContext.User.Identity.Name.ToUpper();
                mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                obj.KeyId = KeyId;
                obj.TrnType = "D";
                obj.Program = "Cancelled";
                obj.StockCode = StockCode;
                obj.KeyField = "Cancelled";
                obj.OldValue = "";
                obj.NewValue = "";
                obj.Username = Username;
                obj.TrnDate = DateTime.Now;
                db.Entry(obj).State = EntityState.Added;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void SaveAuditTrailAttachments(int KeyId, string TrnType, string Program, string StockCode, string FileName)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                obj.KeyId = KeyId;
                obj.TrnType = TrnType;
                obj.Program = Program;
                obj.StockCode = StockCode;
                obj.KeyField = StockCode;
                obj.OldValue = "";
                obj.NewValue = FileName;
                obj.Username = Username;
                obj.TrnDate = DateTime.Now;
                db.Entry(obj).State = EntityState.Added;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }


        public void SaveAuditTrailAttachmentsDelete(int KeyId, string TrnType, string Program, string StockCode, string FileName)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                mtMasterCardUpdateAudit obj = new mtMasterCardUpdateAudit();
                obj.KeyId = KeyId;
                obj.TrnType = TrnType;
                obj.Program = Program;
                obj.StockCode = StockCode;
                obj.KeyField = StockCode;
                obj.OldValue = FileName;
                obj.NewValue = "";
                obj.Username = Username;
                obj.TrnDate = DateTime.Now;
                db.Entry(obj).State = EntityState.Added;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }

        public ActionResult ViewPdf(string StockCode, int Line)
        {
            GetDropDownData();
            MaterCardMaintenanceViewModel model = new MaterCardMaintenanceViewModel();
            try
            {
                var GetPdf = (from a in db.sp_MasterCardGetAttachmentsByStockCode(StockCode) where a.Line == Line select a).FirstOrDefault();
                string ContentType = GetPdf.FileName;
                MemoryStream pdfStream = new MemoryStream();
                byte[] byteArray = GetPdf.Attachment;
                if (ContentType.Contains(".pdf"))
                {
                    pdfStream.Write(byteArray, 0, byteArray.Length);
                    pdfStream.Position = 0;
                    return new FileStreamResult(pdfStream, "application/pdf");
                }
                else
                {
                    model.Image = new List<string>();
                    string imreBase64Data = Convert.ToBase64String(byteArray);
                    string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                    model.Image.Add(imgDataURL);
                    return View("ViewPdf", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        public ActionResult DeleteFile(int AuditId, string StockCode, int Line)
        {
            GetDropDownData();
            var result = (from a in db.mtMasterCardAttachments where a.AuditId == AuditId && a.StockCode == StockCode && a.Line == Line select a).FirstOrDefault();
            db.Entry(result).State = EntityState.Deleted;
            db.SaveChanges();
            this.SaveAuditTrailAttachmentsDelete(result.AuditId, "D", "File Attachment", result.StockCode, result.FileName);
            MaterCardMaintenanceViewModel model = new MaterCardMaintenanceViewModel();
            var resultAttachment = db.sp_MasterCardGetAttachmentsByStockCode(result.StockCode).ToList();
            var resultCustomForm = db.sp_MasterCardGetStockCodeDataForUpdate(result.StockCode, "SYSPRO").FirstOrDefault();
            model.stockCodeUpdate = MapStockCodeFields(resultCustomForm);
            model.FormUpdate = MapCustomFormFields(resultCustomForm);
            model.AttachmentList = resultAttachment;
            model.Id = AuditId;
            model.warehouseUpdate_Result = db.sp_MasterCardGetWarehouseUpdate(result.AuditId, result.StockCode).ToList();
            return View("Index", model);
        }



    }
}
