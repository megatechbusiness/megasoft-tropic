using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class InkSystemController : Controller
    {
        //
        // GET: /InkSystem/
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        InkSystem inks = new InkSystem();
        SysproCore sys = new SysproCore();

        public ActionResult Index()
        {
            GetDropDownData();
            return View();
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadInks")]
        public ActionResult Load(InkSystemViewModel model)
        {
            GetDropDownData();
            InkComponets componets = new InkComponets();
            model.componets = componets;
            model.componets.Component = model.StockCode;
            return View("Index", model);
        }

        public ActionResult StockCodeLookUp()
        {
            //var result = (from a in db.InvMasters
            //              where a.PartCategory == "M"
            //              select new { a.StockCode, a.Description, a.StockUom, a.LongDesc }).Distinct().ToList();
            var result = db.sp_mtInksGetPrintingParentStockCodes().ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
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
            ViewBag.BomRouteList = (from a in db.sp_mtInkSystemGetBomRoute() select new { Value = a.Route, Description = a.Supplier });
        }

        public ActionResult BrowseStockCode()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult GetStockCodeOperations(string ParentPart, string Route)
        {
            try
            {
                var result = db.sp_mtInksGetOperationByStockCode(ParentPart, Route);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetParentComponents(string ParentPart, string Route)
        {
            try
            {
                var result = db.sp_mtInksGetBomStructureByParentPart(ParentPart, Route);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetSelectedStockCodeDesc(string StockCode)
        {
            try
            {
                var Syspro = (from a in db.sp_mtInksGetNumberOfColours(StockCode) where a.StockCode == StockCode select new { StockCode = a.StockCode, Description = a.Description, LongDesc = a.LongDesc, Total = a.Total }).ToList();
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

        [HttpGet]
        public JsonResult GetTreeData(string StockCode, string Route)
        {
            // var Parent = (from a in db.mtMasterCardHeaders where a.StockCode == StockCode select a.StockCode).FirstOrDefault();
            var bomstruct = db.sp_mtInksGetSysproBomStructure(StockCode, Route).OrderBy(a => a.SequenceNum).ToList().Select(a => new TreeModel { Parent = a.ParentPart, Component = a.Component }).ToList();

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

        public ActionResult BomOperation(string StockCode, string Route, string Mode, decimal Operation = 0)
        {
            ModelState.Clear();
            ViewBag.WorkCentreList = (from m in db.BomWorkCentres select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
            if (Mode == "Add")
            {
                InkBomOperation obj = new InkBomOperation();
                obj.StockCode = StockCode;
                obj.Mode = Mode;
                return PartialView(obj);
            }
            //return PartialView();
            else
            {
                var obj = (from a in db.sp_mtInksGetOperationByStockCode(StockCode, Route) where a.Operation == Operation select new InkBomOperation { StockCode = a.StockCode, Operation = a.Operation, Route = a.Route, WorkCentre = a.WorkCentre, Mode = Mode, UnitRunTime = (decimal)a.IRunTime, Quantity = (decimal)a.IQuantity, TimeTaken = (decimal)a.ITimeTaken, Narrations = a.Narration }).FirstOrDefault();
                if (obj != null)
                {
                    obj.Narrations = obj.Narrations.Replace("\r", "");
                    obj.Narrations = obj.Narrations.Replace("||||", Environment.NewLine);
                    return PartialView(obj);

                }
                return PartialView(obj);

            }

        }

        public ActionResult BomComponent(string ParentPart, string Route, string Mode, string Component = null, string SequenceNum = null)
        {
            ModelState.Clear();
            InkComponets obj = new InkComponets();
            obj.ParentPart = ParentPart;
            obj.Mode = Mode;
            ViewBag.BomComponentTypeList = (from a in db.sp_mtInkSystemGetBomComponentType() select new { Value = a.ComponentType, Description = a.Description }).OrderBy(x => x.Description).Reverse();

            if (Component != null)
            {
                ViewBag.NewComponent = false;
                obj = new InkComponets();
                obj = (from a in db.sp_mtInksGetBomStructureByParentPart(ParentPart, Route) where a.ParentPart == ParentPart && a.Component == Component && a.SequenceNum == SequenceNum select new InkComponets { ParentPart = a.ParentPart, Component = a.Component, StockDescription = a.StockDescription, Route = a.Route, SequenceNum = a.SequenceNum, QtyPer = a.QtyPer, LayerPerc = a.LayerPerc, ScrapQuantity = a.ScrapQty, ScrapPercentage = a.ScrapPerc, Mode = Mode, Analox = a.Analox }).FirstOrDefault();
                if (obj != null)
                {
                    return PartialView(obj);
                }
            }
            else
            {
                obj.ParentPart = ParentPart;
                obj.Mode = Mode;
                ViewBag.NewComponent = true;

            }
            return PartialView(obj);

        }

        public JsonResult StockCodeList(string Route)
        {
            var Supplier = (from a in db.sp_mtInkSystemGetBomRoute().ToList() where a.Route == Route select a.Supplier).FirstOrDefault();
            var result = db.sp_mtInksGetAllStockCodes(Supplier).ToList();
            var Stock = (from a in result select new { MStockCode = a.StockCode, MStockDes = a.Description, MStockingUom = a.StockUom }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ComponentType()
        {
            var result = db.sp_mtInkSystemGetBomComponentType().ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveOperation(string details)
        {
            try
            {
                List<InkBomOperation> myDeserializedObject = (List<InkBomOperation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<InkBomOperation>));
                var bom = myDeserializedObject.FirstOrDefault();

                var ActionType = "";
                if (bom.Mode == "Change")
                {
                    var checkbom = db.sp_mtInkSystemGetBomNarrationsForPostingUpdate(bom.StockCode, bom.Route, bom.Operation.ToString()).FirstOrDefault();
                    if (checkbom.Narration == null)
                    {
                        ActionType = "A";
                        var InksBomNarrationUpdate = inks.PostAltBomNarration(bom, ActionType);
                        var InksBomUpdate = inks.PostBomOperation(bom, "C");
                        ModelState.AddModelError("", InksBomNarrationUpdate + " " + InksBomNarrationUpdate);
                        ViewBag.Message = InksBomUpdate;
                    }
                    else
                    {
                        ActionType = "C";
                        var InksBomUpdate = inks.PostBomOperation(bom, ActionType);
                        var InksBomNarration = inks.PostAltBomNarration(bom, ActionType);
                        ModelState.AddModelError("", InksBomNarration + "" + InksBomUpdate);
                        ViewBag.Message = InksBomNarration + " " + InksBomUpdate;
                    }
                }
                else if (bom.Mode == "Add")
                {
                    ActionType = "A";
                    var OperationMax = (from a in db.BomOperations
                                        where a.StockCode == bom.StockCode && a.Route == bom.Route
                                        select a.Operation).ToList();
                    if (OperationMax.Count > 0)
                    {
                        bom.Operation = Convert.ToDecimal((OperationMax.Max() + 1).ToString().PadLeft(4, '0'));
                        var InksBomAdd = inks.PostBomOperation(bom, ActionType);
                        var InksBomNarration = inks.PostAltBomNarration(bom, ActionType);
                        ModelState.AddModelError("", InksBomNarration + "" + InksBomAdd);
                        ViewBag.Message = InksBomAdd + " " + InksBomNarration;
                    }
                    else
                    {
                        bom.Operation = 1;
                        var InksBomAdd = inks.PostBomOperation(bom, ActionType);
                        var InksBomNarration = inks.PostAltBomNarration(bom, ActionType);
                        ModelState.AddModelError("", InksBomNarration + " " + InksBomAdd);
                        ViewBag.Message = InksBomAdd;
                    }

                }
                else
                {
                    ActionType = "D";
                    var DeleteOperation = inks.PostBomOperation(bom, ActionType);
                    //var InksBomNarration = inks.PostAltBomNarration(bom, ActionType);
                    ModelState.AddModelError("", DeleteOperation);
                    ViewBag.Message = DeleteOperation;
                }

                return Json(ViewBag.Message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveComponent(string details)
        {
            try
            {
                List<InkComponets> myDeserializedObject = (List<InkComponets>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<InkComponets>));
                var bom = myDeserializedObject.FirstOrDefault();
                var ActionType = "";
                if (bom.Mode == "Change")
                {
                    //check if bom structure 
                    var checkbom = (from x in db.BomStructures
                                    where x.ParentPart == bom.ParentPart && x.Component == bom.Component && x.Route == bom.Route
                                    select x).FirstOrDefault();
                    if (checkbom == null)
                    {
                        ActionType = "A";
                    }
                    else
                    {
                        ActionType = "C";
                        var SequenceNum = (from a in db.BomStructures
                                           where a.ParentPart == bom.ParentPart && a.Route == bom.Route && a.Component == bom.Component
                                           select a.SequenceNum).FirstOrDefault();
                        bom.SequenceNum = SequenceNum;
                        var UpdateBomStructure = inks.PostBomStructure(bom, ActionType);
                        ModelState.AddModelError("", UpdateBomStructure);
                        ViewBag.Message = UpdateBomStructure;
                    }

                }
                else if (bom.Mode == "Add")
                {
                    var SequenceNum = (from a in db.BomStructures
                                       where a.ParentPart == bom.ParentPart && a.Route == bom.Route
                                       select a.SequenceNum).Max();
                    double SequenceNumber = 0;
                    SequenceNumber = Convert.ToDouble(SequenceNum) + 1;
                    bom.SequenceNum = SequenceNumber.ToString().PadLeft(5, '0');
                    ActionType = "A";
                    var AddBomStructure = inks.PostBomStructure(bom, ActionType);
                    ModelState.AddModelError("", AddBomStructure);
                    ViewBag.Message = AddBomStructure;

                }
                else
                {
                    ActionType = "D";
                    var SequenceNum = (from a in db.BomStructures
                                       where a.ParentPart == bom.ParentPart && a.Route == bom.Route && a.Component == bom.Component
                                       select a.SequenceNum).FirstOrDefault();
                    bom.SequenceNum = SequenceNum;
                    var DeleteBomStructure = inks.PostBomStructure(bom, ActionType);
                    ModelState.AddModelError("", DeleteBomStructure);
                    ViewBag.Message = DeleteBomStructure;

                }
                return Json(ViewBag.Message, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
