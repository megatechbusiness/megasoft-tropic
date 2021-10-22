using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class MasterCardStockCodeBomSetupController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MasterCard BL = new MasterCard();

        [CustomAuthorize("MasterCardBom")]
        public ActionResult Index()
        {
            MasterCardViewModel model = new MasterCardViewModel();
            mtMasterCardHeader header = new mtMasterCardHeader();
            header.Id = 0;
            model.Header = header;
            //ViewBag.MainCode = "N";
            return View(header);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "GetMasterCardDetails")]
        public ActionResult LoadMasterCardDetails(mtMasterCardHeader master)
        {
            try
            {
                ModelState.Clear();
                mtMasterCardHeader model = new mtMasterCardHeader();
                int Id = master.Id;
                if (ModelState.IsValid)
                {
                    if (Id != 0)
                    {
                        var CheckIfIdExists = wdb.mtMasterCardHeaders.Find(Id);
                        if (CheckIfIdExists != null)
                        {
                            //model = GetMasterCardDetails(Id);
                            model = CheckIfIdExists;
                            ViewBag.ValidId = true;
                            ViewBag.MainCode = CheckStockCodeExistsInSyspro(CheckIfIdExists.StockCode);
                        }
                        else
                        {
                            ModelState.AddModelError("", "Master Card Id not found.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Master Card Id cannot be blank.");
                    }
                }




                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", master);
            }
        }


        public MasterCardViewModel GetMasterCardDetails(int Id)
        {
            MasterCardViewModel model = new MasterCardViewModel();
            model.Header = wdb.mtMasterCardHeaders.Find(Id);
            if (!string.IsNullOrEmpty(model.Header.Customer))
            {
                var CustName = wdb.sp_GetStereoCustomerName(model.Header.Customer).FirstOrDefault();

                if (CustName != null)
                {
                    model.Header.Customer = model.Header.Customer + " - " + CustName.Name;
                }
            }

            return model;
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



        [HttpGet]
        public JsonResult GetTreeData(int KeyId)
        {
            var Parent = (from a in wdb.mtMasterCardHeaders where a.Id == KeyId select a.StockCode).FirstOrDefault();
            var bomstruct = wdb.sp_MasterCardGetBomStructure(KeyId, Parent, "0", "B").ToList().Select(a => new TreeModel { Parent = a.ParentPart, Component = a.Component }).ToList();

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
                topLevel.Component = Parent;
                tree.Add(topLevel);

                var output = MapToTreeModelJsonCollection(tree);
                var json = JsonConvert.SerializeObject(output, Formatting.Indented);
                return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

        }


        //*******************************************************************************************************************************************************
        //Components and Operations


        [HttpGet]
        public ActionResult GetParentComponents(int KeyId, string ParentPart)
        {
            try
            {

                string BomExists = "N";
                var BomCheck = wdb.sp_MasterCardGetSysproBomStructure(ParentPart, "0").ToList();
                if (BomCheck.Count > 0)
                {
                    BomExists = "Y";
                }
                var result = (from a in wdb.mtMasterCardBomStructures.AsNoTracking() where a.Id == KeyId && a.ParentPart == ParentPart && a.Source == "B" select new { Id = a.Id, ParentPart = a.ParentPart, Component = a.Component, SequenceNum = a.SequenceNum, Route = a.Route, QtyPer = a.QtyPer, LayerPerc = a.LayerPerc, BomExists = BomExists }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public string CheckStockCodeExistsInSyspro(string StockCode)
        {
            try
            {
                var Syspro = (from a in wdb.InvMasters.AsNoTracking()
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



        [HttpGet]
        public ActionResult GetSelectedStockCodeDesc(string StockCode)
        {
            try
            {
                var Syspro = (from a in wdb.InvMasters.AsNoTracking() where a.StockCode == StockCode select new { StockCode = a.StockCode, Description = a.Description, LongDesc = a.LongDesc }).ToList();
                if (Syspro.Count > 0)
                {
                    return Json(Syspro, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var NonStock = (from a in wdb.mtNonStockMasters.AsNoTracking() where a.StockCode == StockCode select new { StockCode = a.StockCode, Description = a.Description, LongDesc = a.LongDesc }).ToList();
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
            MasterCardComponent obj = new MasterCardComponent();
            if (Component != null)
            {
                ViewBag.NewComponent = false;
                obj = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == id && a.ParentPart == ParentPart && a.Component == Component && a.SequenceNum == SequenceNum select new MasterCardComponent { Id = a.Id, ParentPart = a.ParentPart, Component = a.Component, Route = a.Route, SequenceNum = a.SequenceNum, QtyPer = a.QtyPer, LayerPerc = a.LayerPerc, ScrapQuantity = a.ScrapQuantity, ScrapPercentage = a.ScrapPercentage, Mode = Mode }).FirstOrDefault();
            }
            else
            {
                ViewBag.NewComponent = true;
            }
            return PartialView(obj);

        }

        public JsonResult StockCodeList()
        {
            var result = wdb.sp_MasterCardGetAllStockCodes("").ToList();
            var Stock = (from a in result select new { MStockCode = a.StockCode, MStockDes = a.Description, MStockingUom = a.StockUom }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveComponent(string details)
        {
            try
            {
                List<MasterCardComponent> myDeserializedObject = (List<MasterCardComponent>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MasterCardComponent>));
                var bom = myDeserializedObject.FirstOrDefault();
                if (bom != null)
                {
                    if (bom.Mode == "Change")
                    {
                        var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.SequenceNum == bom.SequenceNum && a.Component == bom.Component && a.Route == bom.Route select a).FirstOrDefault();
                        if (check == null)
                        {
                            return Json("Component not found!", JsonRequestBehavior.AllowGet);
                        }
                        check.QtyPer = bom.QtyPer;
                        check.LayerPerc = bom.LayerPerc;
                        check.ScrapQuantity = bom.ScrapQuantity;
                        check.ScrapPercentage = bom.ScrapPercentage;
                        check.DateLastSaved = DateTime.Now;
                        check.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                        wdb.Entry(check).State = EntityState.Modified;
                        wdb.SaveChanges();
                    }
                    else
                    {
                        var ValidateStockCode = (wdb.sp_MasterCardGetAllStockCodes(bom.Component)).ToList();
                        if (ValidateStockCode.Count == 0)
                        {
                            return Json("StockCode " + bom.Component + " not found!", JsonRequestBehavior.AllowGet);
                        }

                        var SysproBom = wdb.sp_MasterCardGetSysproBomStructure(bom.Component, "0").ToList().Where(a => a.LevelId != 0).ToList();
                        if (SysproBom.Count > 0)
                        {
                            string SequenceNum = "00";
                            var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component select a).ToList();
                            if (check.Count > 0)
                            {
                                var MaxSeq = Convert.ToInt16((check.Max(a => a.SequenceNum)));
                                SequenceNum = (MaxSeq + 1).ToString().PadLeft(2, '0');
                            }
                            mtMasterCardBomStructure obj = new mtMasterCardBomStructure();
                            obj.Id = bom.Id;
                            obj.ParentPart = bom.ParentPart;
                            obj.Component = bom.Component;
                            obj.Route = "0";
                            obj.SequenceNum = SequenceNum;
                            obj.Source = "B";
                            obj.QtyPer = bom.QtyPer;
                            obj.LayerPerc = bom.LayerPerc;
                            obj.ScrapQuantity = bom.ScrapQuantity;
                            obj.ScrapPercentage = bom.ScrapPercentage;
                            obj.DateLastSaved = DateTime.Now;
                            obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                            wdb.Entry(obj).State = EntityState.Added;
                            wdb.SaveChanges();

                            foreach (var co in SysproBom)
                            {
                                SequenceNum = "00";
                                check = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == bom.Id && a.ParentPart == co.ParentPart && a.Component == co.Component select a).ToList();
                                if (check.Count > 0)
                                {
                                    var MaxSeq = Convert.ToInt16((check.Max(a => a.SequenceNum)));
                                    SequenceNum = (MaxSeq + 1).ToString().PadLeft(2, '0');
                                }
                                obj = new mtMasterCardBomStructure();
                                obj.Id = bom.Id;
                                obj.ParentPart = co.ParentPart;
                                obj.Component = co.Component;
                                obj.Route = "0";
                                obj.SequenceNum = SequenceNum;
                                obj.Source = "B";
                                obj.QtyPer = co.QtyPer;
                                obj.LayerPerc = co.LayerPerc;
                                obj.ScrapQuantity = co.ScrapQuantity;
                                obj.ScrapPercentage = co.ScrapPercentage;
                                obj.DateLastSaved = DateTime.Now;
                                obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                                wdb.Entry(obj).State = EntityState.Added;
                                wdb.SaveChanges();
                            }
                        }
                        else
                        {
                            string SequenceNum = "00";
                            var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component select a).ToList();
                            if (check.Count > 0)
                            {
                                var MaxSeq = Convert.ToInt16((check.Max(a => a.SequenceNum)));
                                SequenceNum = (MaxSeq + 1).ToString().PadLeft(2, '0');
                            }
                            mtMasterCardBomStructure obj = new mtMasterCardBomStructure();
                            obj.Id = bom.Id;
                            obj.ParentPart = bom.ParentPart;
                            obj.Component = bom.Component;
                            obj.Route = "0";
                            obj.SequenceNum = SequenceNum;
                            obj.Source = "B";
                            obj.QtyPer = bom.QtyPer;
                            obj.LayerPerc = bom.LayerPerc;
                            obj.ScrapQuantity = bom.ScrapQuantity;
                            obj.ScrapPercentage = bom.ScrapPercentage;
                            obj.DateLastSaved = DateTime.Now;
                            obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                            wdb.Entry(obj).State = EntityState.Added;
                            wdb.SaveChanges();
                        }

                    }
                }

                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult DeleteComponent(string details)
        {
            try
            {
                List<mtMasterCardBomStructure> myDeserializedObject = (List<mtMasterCardBomStructure>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<mtMasterCardBomStructure>));
                var bom = myDeserializedObject.FirstOrDefault();
                if (bom != null)
                {
                    var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component && a.Route == bom.Route && a.SequenceNum == bom.SequenceNum select a).FirstOrDefault();
                    if (check != null)
                    {
                        wdb.Entry(check).State = EntityState.Deleted;
                        wdb.SaveChanges();
                    }
                    else
                    {
                        return Json("Component not found!", JsonRequestBehavior.AllowGet);
                    }
                }

                return Json("Deleted Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult BomCopyComponent(int id, string ParentPart)
        {
            MasterCardCopyComponent model = new MasterCardCopyComponent();
            model.KeyId = id;
            model.ToStockCode = ParentPart;
            model.CopyOption = "Delete";
            ViewBag.RouteList = (from m in wdb.BomRoutes.ToList() select new { Value = m.Route, Text = m.Route }).ToList();
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult LoadBomComponents(string ParentPart, string Route)
        {
            try
            {
                var result = wdb.sp_MasterCardGetSysproBomStructure(ParentPart, Route).ToList().Where(a => a.LevelId != 0).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveCopyComponent(string details)
        {
            try
            {
                List<MasterCardComponent> myDeserializedObject = (List<MasterCardComponent>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MasterCardComponent>));
                if (myDeserializedObject.Count > 0)
                {
                    var ToStockCode = myDeserializedObject.FirstOrDefault().ToStockCode;
                    var KeyId = myDeserializedObject.FirstOrDefault().Id;
                    if (myDeserializedObject.FirstOrDefault().CopyOption == "Delete")
                    {
                        var delitems = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == KeyId && a.ParentPart == ToStockCode select a).ToList();
                        foreach (var item in delitems)
                        {
                            wdb.Entry(item).State = EntityState.Deleted;
                            wdb.SaveChanges();
                        }

                        foreach (var bom in myDeserializedObject)
                        {

                            string SequenceNum = "00";
                            var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component select a).ToList();
                            if (check.Count > 0)
                            {
                                var MaxSeq = Convert.ToInt16((check.Max(a => a.SequenceNum)));
                                SequenceNum = (MaxSeq + 1).ToString().PadLeft(2, '0');
                            }
                            mtMasterCardBomStructure obj = new mtMasterCardBomStructure();
                            obj.Id = bom.Id;
                            obj.ParentPart = bom.ParentPart;
                            obj.Component = bom.Component;
                            obj.Route = "0";
                            obj.SequenceNum = SequenceNum;
                            obj.Source = "B";
                            obj.QtyPer = bom.QtyPer;
                            obj.LayerPerc = bom.LayerPerc;
                            obj.ScrapQuantity = bom.ScrapQuantity;
                            obj.ScrapPercentage = bom.ScrapPercentage;
                            obj.DateLastSaved = DateTime.Now;
                            obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                            wdb.Entry(obj).State = EntityState.Added;
                            wdb.SaveChanges();
                        }

                    }
                    else if (myDeserializedObject.FirstOrDefault().CopyOption == "Merge")
                    {
                        foreach (var bom in myDeserializedObject)
                        {

                            string SequenceNum = "00";
                            var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component select a).ToList();
                            if (check.Count > 0)
                            {
                                var MaxSeq = Convert.ToInt16((check.Max(a => a.SequenceNum)));
                                SequenceNum = (MaxSeq + 1).ToString().PadLeft(2, '0');
                            }
                            mtMasterCardBomStructure obj = new mtMasterCardBomStructure();
                            obj.Id = bom.Id;
                            obj.ParentPart = bom.ParentPart;
                            obj.Component = bom.Component;
                            obj.Route = "0";
                            obj.SequenceNum = SequenceNum;
                            obj.Source = "B";
                            obj.QtyPer = bom.QtyPer;
                            obj.LayerPerc = bom.LayerPerc;
                            obj.ScrapQuantity = bom.ScrapQuantity;
                            obj.ScrapPercentage = bom.ScrapPercentage;
                            obj.DateLastSaved = DateTime.Now;
                            obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                            wdb.Entry(obj).State = EntityState.Added;
                            wdb.SaveChanges();
                        }
                    }
                    else
                    {
                        foreach (var bom in myDeserializedObject)
                        {

                            string SequenceNum = "00";
                            var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "B" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Route == "0" && a.Component == bom.Component select a).ToList();
                            if (check.Count == 0)
                            {
                                mtMasterCardBomStructure obj = new mtMasterCardBomStructure();
                                obj.Id = bom.Id;
                                obj.ParentPart = bom.ParentPart;
                                obj.Component = bom.Component;
                                obj.Route = "0";
                                obj.SequenceNum = SequenceNum;
                                obj.Source = "B";
                                obj.QtyPer = bom.QtyPer;
                                obj.LayerPerc = bom.LayerPerc;
                                obj.ScrapQuantity = bom.ScrapQuantity;
                                obj.ScrapPercentage = bom.ScrapPercentage;
                                obj.DateLastSaved = DateTime.Now;
                                obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                                wdb.Entry(obj).State = EntityState.Added;
                                wdb.SaveChanges();
                            }
                        }
                    }


                }
                else
                {
                    return Json("No data found!", JsonRequestBehavior.AllowGet);
                }
                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult BomOperation(int id, string StockCode, string Mode, decimal Operation = 0)
        {
            ViewBag.WorkCentreList = (from m in wdb.BomWorkCentres select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
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
                var obj = (from a in wdb.mtMasterCardBomOperations where a.Id == id && a.StockCode == StockCode && a.Operation == Operation select new MasterCardBomOperation { Id = a.Id, StockCode = a.StockCode, Operation = a.Operation, Route = a.Route, WorkCentre = a.WorkCentre, Mode = "Change", Narrations = a.Narration, Quantity = (decimal)a.Quantity, TimeTaken = (decimal)a.TimeTaken, UnitRunTime = (decimal)a.UnitRunTime }).FirstOrDefault();
                return PartialView(obj);
            }

        }


        [HttpPost]
        public ActionResult SaveOperation(string details)
        {
            try
            {
                List<MasterCardBomOperation> myDeserializedObject = (List<MasterCardBomOperation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MasterCardBomOperation>));
                var bom = myDeserializedObject.FirstOrDefault();
                if (bom != null)
                {
                    if (bom.Mode == "Change")
                    {
                        var check = (from a in wdb.mtMasterCardBomOperations where a.Id == bom.Id && a.StockCode == bom.StockCode && a.Operation == bom.Operation && a.Route == bom.Route select a).FirstOrDefault();
                        if (check == null)
                        {
                            return Json("Component not found!", JsonRequestBehavior.AllowGet);
                        }
                        check.UnitRunTime = bom.UnitRunTime;
                        check.TimeTaken = bom.TimeTaken;
                        check.Quantity = bom.Quantity;
                        check.WorkCentre = bom.WorkCentre;
                        check.Narration = bom.Narrations;
                        check.DateLastSaved = DateTime.Now;
                        check.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                        wdb.Entry(check).State = EntityState.Modified;
                        wdb.SaveChanges();
                    }
                    else
                    {
                        int OpNo = 0;
                        var check = (from a in wdb.mtMasterCardBomOperations where a.Id == bom.Id && a.StockCode == bom.StockCode && a.Route == "0" select a).ToList();
                        if (check.Count > 0)
                        {
                            var MaxOp = Convert.ToInt16((check.Max(a => a.Operation)));
                            OpNo = (MaxOp + 1);
                        }
                        mtMasterCardBomOperation obj = new mtMasterCardBomOperation();
                        obj.Id = bom.Id;
                        obj.StockCode = bom.StockCode;
                        obj.Operation = OpNo;
                        obj.Route = "0";
                        obj.WorkCentre = bom.WorkCentre;
                        obj.UnitRunTime = bom.UnitRunTime;
                        obj.TimeTaken = bom.TimeTaken;
                        obj.Quantity = bom.Quantity;
                        obj.Narration = bom.Narrations;
                        obj.DateLastSaved = DateTime.Now;
                        obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                        wdb.Entry(obj).State = EntityState.Added;
                        wdb.SaveChanges();
                    }
                }

                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpGet]
        public ActionResult GetStockCodeOperations(int KeyId, string ParentPart)
        {
            try
            {
                var result = (from a in wdb.mtMasterCardBomOperations.AsNoTracking() where a.Id == KeyId && a.StockCode == ParentPart select a).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteOperation(string details)
        {
            try
            {
                List<MasterCardBomOperation> myDeserializedObject = (List<MasterCardBomOperation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MasterCardBomOperation>));
                var bom = myDeserializedObject.FirstOrDefault();
                if (bom != null)
                {
                    var check = (from a in wdb.mtMasterCardBomOperations where a.Id == bom.Id && a.StockCode == bom.StockCode && a.Route == bom.Route && a.Operation == bom.Operation select a).FirstOrDefault();
                    if (check != null)
                    {
                        wdb.Entry(check).State = EntityState.Deleted;
                        wdb.SaveChanges();
                    }
                    else
                    {
                        return Json("Operation not found!", JsonRequestBehavior.AllowGet);
                    }
                }

                return Json("Deleted Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult StockCodeCreationModal(int MasterCardId, string MainCode = "N")
        {
            GetDropDownData();

            //LoadDefaults

            MasterCardViewModel model = new MasterCardViewModel();
            mtMasterCardStockCode objStk = new mtMasterCardStockCode();
            objStk.WarehouseToUse = "FR";
            objStk.Decimals = 3;
            if (MasterCardId != 0)
            {
                var MasterCard = (from a in wdb.mtMasterCardHeaders where a.Id == MasterCardId select a).FirstOrDefault();
                if (MainCode == "Y")
                {
                    objStk.StockCode = MasterCard.StockCode;
                    objStk.Description = MasterCard.ProductDescription;
                    objStk.LongDesc = MasterCard.PrintDescription;
                    decimal Mass = (decimal)(((MasterCard.Width / 10) * (MasterCard.Length / 10) * MasterCard.Micron) / 5400);
                    //objStk.Mass = Math.Round(Mass, 3);
                }
            }
            model.MasterCardId = MasterCardId;

            model.stkobj = objStk;
            if (ModelState.IsValid)
            {

            }
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StockCodeCreationModal(MasterCardViewModel model)
        {

            if (model.stkobj.Mass == 0)
            {
                return Json("Mass cannot be blank or zero!", JsonRequestBehavior.AllowGet);
            }


            //ModelState.AddModelError("", "Stock Code created successfully.");

            var result = BL.PostStockCodeCreation(model);

            return Json(result, JsonRequestBehavior.AllowGet);
            //return PartialView(model);
            //JsonResult result = new JsonResult();
            //result.Data = model;
            //return result;
        }

        public void GetDropDownData()
        {
            ViewBag.WarehouseList = (from a in wdb.InvWhControls select new { Value = a.Warehouse, Description = a.Warehouse + " - " + a.Description });
            ViewBag.JobClassList = (from a in wdb.WipJobClasses select new { Value = a.JobClassification, Description = a.JobClassification + " - " + a.ClassDescription });
            ViewBag.ProductClassList = (from a in wdb.SalProductClassDes select new { Value = a.ProductClass, Description = a.ProductClass + " - " + a.Description });
            ViewBag.StockUomList = (from a in wdb.mtMasterCardStockUoms select new { Value = a.StockUom, Description = a.StockUom + " - " + a.Description });
            ViewBag.IndustryList = (from a in wdb.AdmFormValidations where a.FormType == "STK" && a.FieldName == "Inv004" select new { Value = a.Item, Description = a.Description });
            ViewBag.ProductTypeList = (from a in wdb.AdmFormValidations where a.FormType == "STK" && a.FieldName == "Inv005" select new { Value = a.Item, Description = a.Description });
            ViewBag.ProductSubTypeList = (from a in wdb.AdmFormValidations where a.FormType == "STK" && a.FieldName == "Inv006" select new { Value = a.Item, Description = a.Description });
        }


        public JsonResult GetMastercardCustomForm(int MasterCardId)
        {
            try
            {
                var result = (wdb.sp_MasterCardGetCustomFormData(MasterCardId).ToList());
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public JsonResult GetCopyStockCodeData(string StockCode)
        {
            try
            {
                var result = (wdb.sp_MasterCardGetStockCodeCopyData(StockCode).ToList());
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostBom")]
        public ActionResult PostBom(mtMasterCardHeader model)
        {
            try
            {
                ModelState.Clear();
                string result = BL.PostBom(model.Id);

                ModelState.AddModelError("", result);

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }


        [HttpPost]
        public ActionResult CheckEtCalcMethod(string details)
        {
            try
            {
                List<MasterCardBomOperation> myDeserializedObjList = (List<MasterCardBomOperation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MasterCardBomOperation>));
                if (myDeserializedObjList.Count > 0)
                {
                    string WorkCentre = myDeserializedObjList.FirstOrDefault().WorkCentre.Trim();
                    var result = wdb.sp_MasterCardGetWorkCentreDetails(WorkCentre).ToList();
                    if (result.Count > 0)
                    {
                        return Json(result.FirstOrDefault().EtCalcMeth, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error. WorkCentre : " + WorkCentre + " not found in Syspro.", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("Error - No Data. WorkCentre not found.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult BomCopyOperation(int id, string ParentPart)
        {
            MasterCardCopyComponent model = new MasterCardCopyComponent();
            model.KeyId = id;
            model.ToStockCode = ParentPart;
            model.CopyOption = "Delete";
            ViewBag.RouteList = (from m in wdb.BomRoutes.ToList() select new { Value = m.Route, Text = m.Route }).ToList();
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult LoadBomOperations(string ParentPart, string Route)
        {
            try
            {
                var result = wdb.sp_MasterCardGetSysproBomOperations(ParentPart, Route).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult SaveCopyOperations(string details)
        {
            try
            {
                List<MasterCardBomOperation> myDeserializedObject = (List<MasterCardBomOperation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MasterCardBomOperation>));
                if (myDeserializedObject.Count > 0)
                {
                    var ToStockCode = myDeserializedObject.FirstOrDefault().ToStockCode;
                    var KeyId = myDeserializedObject.FirstOrDefault().Id;
                    if (myDeserializedObject.FirstOrDefault().CopyOption == "Delete")
                    {
                        var delitems = (from a in wdb.mtMasterCardBomOperations where a.Id == KeyId && a.StockCode == ToStockCode select a).ToList();
                        foreach (var item in delitems)
                        {
                            wdb.Entry(item).State = EntityState.Deleted;
                            wdb.SaveChanges();
                        }
                        int OpNo = 0;
                        foreach (var bom in myDeserializedObject)
                        {

                            mtMasterCardBomOperation obj = new mtMasterCardBomOperation();
                            obj.Id = bom.Id;
                            obj.StockCode = bom.StockCode;
                            obj.Route = bom.Route;
                            obj.Operation = OpNo;
                            obj.WorkCentre = bom.WorkCentre;
                            obj.UnitRunTime = bom.UnitRunTime;
                            obj.TimeTaken = bom.TimeTaken;
                            obj.Quantity = bom.Quantity;
                            obj.Narration = bom.Narrations.Replace("\\n", "\n");
                            obj.DateLastSaved = DateTime.Now;
                            obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                            wdb.Entry(obj).State = EntityState.Added;
                            wdb.SaveChanges();

                            OpNo++;
                        }

                    }
                    else //if (myDeserializedObject.FirstOrDefault().CopyOption == "Merge")
                    {
                        foreach (var bom in myDeserializedObject)
                        {
                            var check = (from a in wdb.mtMasterCardBomOperations where a.Id == bom.Id && a.StockCode == bom.StockCode && a.WorkCentre == bom.WorkCentre select a).ToList();
                            if (check.Count == 0)
                            {


                                mtMasterCardBomOperation obj = new mtMasterCardBomOperation();
                                obj.Id = bom.Id;
                                obj.StockCode = bom.StockCode;
                                obj.Route = bom.Route;
                                obj.Operation = GetNextOpNo(bom.Id, bom.StockCode);
                                obj.WorkCentre = bom.WorkCentre;
                                obj.UnitRunTime = bom.UnitRunTime;
                                obj.TimeTaken = bom.TimeTaken;
                                obj.Quantity = bom.Quantity;
                                obj.Narration = bom.Narrations.TrimEnd('\r', '\n');
                                //obj.Narration = bom.Narrations.Replace("||||", Environment.NewLine);
                                obj.DateLastSaved = DateTime.Now;
                                obj.LastSavedBy = HttpContext.User.Identity.Name.ToUpper();
                                wdb.Entry(obj).State = EntityState.Added;
                                wdb.SaveChanges();
                            }
                        }
                    }


                }
                else
                {
                    return Json("No data found!", JsonRequestBehavior.AllowGet);
                }
                return Json("Saved Successfully!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public decimal GetNextOpNo(int KeyId, string StockCode)
        {
            try
            {
                using (var ldb = new WarehouseManagementEntities(""))
                {
                    var results = (from a in ldb.mtMasterCardBomOperations.AsNoTracking() where a.Id == KeyId && a.StockCode == StockCode select a).FirstOrDefault();
                    if (results==null)
                    {
                        return 0;
                    }
                    else
                    {
                        var result = (from a in ldb.mtMasterCardBomOperations.AsNoTracking() where a.Id == KeyId && a.StockCode == StockCode select a).Max(b => b.Operation);
                        return result + 1;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class NotesTemplate
        {
            public string SoText { get; set; }
            public string JobNarr { get; set; }
        }



        public JsonResult GetCopyNotesTemplate(int MasterCardId)
        {
            try
            {
                var result = new List<NotesTemplate>();

                StringBuilder Document = new StringBuilder();
                Document.Append("* SCALE OPERATORS TO ONLY WEIGHED UP 24HOURS AFTER CURING AND TO FOLLOW LABELS ON THE PALLET FOR\n");
                Document.Append("TRACEABILITY.\n");
                Document.Append("* SCALE OPERATORS ARE NOT ALLOWED TO WEIGH UP ANY REELS STRAIGHT/FRESH OF EXTRUDER UNLESS\n");
                Document.Append("INSTRUCTED TO DO SO BY SENIOR MANAGEMENT.\n");
                Document.Append("* SCALE SUPERVISOR AND SENIOR Q.C'S TO ENSURE THAT THIS IS CARRIED OUT.\n");
                Document.Append("* C.O.A'S TO BE SUPPLIED TO CUSTOMER\n");
                Document.Append("* SUPPLY COC\n");
                Document.Append("* CORE BUNGS TO BE USED BOTH ENDS\n");
                Document.Append("* EACH STICKER TO RECORD MANUFACTURERS NAME,DATE, SHIFT, ROLL NO. & NETT MASS\n");
                Document.Append("* EACH ROLL TO BE COMPLETELY WRAPPED IN A PROTECTIVE COVERING.\n");
                Document.Append("* EACH ROLL IDENTIFICATION STICKER INSIDE OF CORE AND OUTSIDE OF EACH PALLET.\n");
                Document.Append("“roll label is to be placed inside the core and on the outer roll wrap, NOT on the actual film”\n");
                Document.Append("*** ALL INVOICES TO BE E-MAILED TO sabap.sa@documentwarehouse.co.za ***\n");
                Document.Append("* MATERIAL NO:\n");
                Document.Append("* PLEASE MARK ALL PALLETS SENT TO CUSTOMER\n");
                Document.Append("*** DELIVER ON A CLOSED TRUCK ***\n");

                var masterCard = (from a in wdb.mtMasterCardHeaders where a.Id == MasterCardId select a).FirstOrDefault();

                StringBuilder SOTEXT = new StringBuilder();
                if (masterCard != null)
                {
                    SOTEXT.Append("\" " + masterCard.ProductDescription + " \"\n");

                    var colours = (from a in wdb.mtMasterCardPrintingColours where a.Id == MasterCardId select a).ToList();
                    if (colours.Count > 0)
                    {
                        var printing = (from a in wdb.mtMasterCardPrintings where a.Id == MasterCardId select a).ToList();
                        if (printing.Count > 0)
                        {
                            if (printing.First().Reverse)
                            {
                                SOTEXT.Append("* PTD " + colours.Count.ToString() + " COLS, 1 SIDE ON CLEAR. (REVERSE PRINT)\n");
                            }
                            else
                            {
                                SOTEXT.Append("* PTD " + colours.Count.ToString() + " COLS, 1 SIDE ON CLEAR. (SURFACE PRINT)\n");
                            }
                        }
                        else
                        {
                            SOTEXT.Append("* PTD " + colours.Count.ToString() + " COLS, 1 SIDE ON CLEAR.\n");
                        }

                    }


                }

                SOTEXT.Append("* MIN QTY:\n");
                SOTEXT.Append("* MATERIAL NO:\n");

                var item = new NotesTemplate();
                item.JobNarr = Document.ToString();
                item.SoText = SOTEXT.ToString();


                result.Add(item);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public ActionResult BomCopyComponentMasterCard(int id, string ParentPart)
        {
            MasterCardCopyComponent model = new MasterCardCopyComponent();
            model.KeyId = id;
            model.ToStockCode = ParentPart;
            model.CopyOption = "Delete";
            ViewBag.RouteList = (from m in wdb.BomRoutes.ToList() select new { Value = m.Route, Text = m.Route }).ToList();
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult LoadBomComponentsMasterCard(int Id)
        {
            try
            {
                var MasterCardHeader = (from a in wdb.mtMasterCardHeaders where a.Id == Id select a).FirstOrDefault();
                //sp_MasterCardGetBomStructure 1037, 'CLODBN081', '0','M'
                var result = wdb.sp_MasterCardGetBomStructure(Id, MasterCardHeader.StockCode, "0", "M").ToList().Where(a => a.LevelId != 0).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BomOperationMasterCard(int Id, string ParentPart)
        {
            MasterCardCopyOperation model = new MasterCardCopyOperation();
            model.KeyId = Id;
            model.ToStockCode = ParentPart;
            model.CopyOption = "Delete";
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult LoadBomOperationMasterCard(int Id,string ParentPart)
        {
            try
            {
                var results = new List<sp_MasterCardGetSysproBomOperations_Result>();
                var MasterCardHeader = (from a in wdb.mtMasterCardHeaders where a.Id == Id select a).FirstOrDefault();
                var Mass = (from a in wdb.InvMasters where a.StockCode== MasterCardHeader.StockCode select a.Mass).FirstOrDefault();

                if (MasterCardHeader.Extrusion==true)
                {
                    StringBuilder Document = new StringBuilder();

                    var Extr = new sp_MasterCardGetSysproBomOperations_Result();
                    var workCentre = (from a in wdb.mtMasterCardExtrusions where a.Id == Id select a.WorkCentre).FirstOrDefault();
                    //var RunTime = (from a in wdb.BomWorkCentres where a.WorkCentre == workCentre select a.RunTime ).FirstOrDefault();
                    Extr.StockCode = ParentPart;
                    Extr.WorkCentre = workCentre;
                    Extr.Route = "0";
                    if (MasterCardHeader.Quantity==null)
                    {
                        Extr.Quantity = 0;
                    }
                    else
                    {
                        Extr.Quantity = MasterCardHeader.Quantity;
                    }
                    var Runtime = wdb.sp_MasterCardGetUnitRunTime(workCentre,Mass).FirstOrDefault();
                    Extr.RunTime = Runtime.UnitRunTime;

                    var ExtrDetails = (from a in wdb.mtMasterCardExtrusions where a.Id == Id select a).FirstOrDefault();

                    Document.AppendLine("* SUPPLY PRINTING WIDTH " + ExtrDetails.SupplyPrintingWidth + " CLEAR SHEETING.");
                    Document.AppendLine("* "+ExtrDetails.Micron+" MIC.");
                    if (ExtrDetails.NewJob==true)
                    {
                        Document.AppendLine("* NEW JOB.");
                    }
                    if (ExtrDetails.NoDieLines==true)
                    {
                        Document.AppendLine("* NO DIE LINES.");
                    }
                    if (ExtrDetails.HighClarity==true)
                    {
                        Document.AppendLine("* HIGH CLARITY.");
                    }
                    Document.AppendLine("* ±"+ExtrDetails.GauageTolerance+ "%  GAUGE TOLERANCE.");
                    Document.AppendLine("* ±" + ExtrDetails.SupplyPrintingWidth + " PRINTING WIDTH.");

                    if (ExtrDetails.NoSlackEdge==true)
                    {
                        Document.AppendLine("* NO SLACK EDGE.");
                    }
                    if (ExtrDetails.NoGels==true)
                    {
                        Document.AppendLine("* NO GELS.");
                    }
                    if (ExtrDetails.TrimOnExtruder==true)
                    {
                        Document.AppendLine("* TRIM ON EXTRUDER.");
                    }
                    if (ExtrDetails.LowTension==true)
                    {
                        Document.AppendLine("* LOW TENSION.");
                    }
                    Document.AppendLine("* ±" + ExtrDetails.COFResultsTRD + " C.O.F RESULTS TRD/TRD.");
                    Document.AppendLine("* ±" + ExtrDetails.COFResultsCLR + " C.O.F RESULTS CLR/CLR.");

                    Extr.Narration = Document.ToString();
                    results.Add(Extr);
                }
                if (MasterCardHeader.Printing==true)
                {
                    StringBuilder Document = new StringBuilder();

                    var Print = new sp_MasterCardGetSysproBomOperations_Result();
                    var workCentre = (from a in wdb.mtMasterCardPrintings where a.Id == Id select a.WorkCentre).FirstOrDefault();
                    var RunTime = (from a in wdb.BomWorkCentres where a.WorkCentre == workCentre select a.RunTime).FirstOrDefault();
                    Print.StockCode = ParentPart;
                    Print.WorkCentre = workCentre;
                    Print.Route = "0";
                    if (MasterCardHeader.Quantity == null)
                    {
                        Print.Quantity = 0;
                    }
                    else
                    {
                        Print.Quantity = MasterCardHeader.Quantity;
                    }
                    var Runtime = wdb.sp_MasterCardGetUnitRunTime(workCentre, Mass).FirstOrDefault();
                    Print.RunTime = Runtime.UnitRunTime;

                    var PrintDetails = (from a in wdb.mtMasterCardPrintings where a.Id==Id select a).FirstOrDefault();
                    var PrintColors = (from a in wdb.mtMasterCardPrintingColours where a.Id == Id select a).ToList();
                    Document.AppendLine("* MICRON: " + MasterCardHeader.Micron);
                    Document.AppendLine("* PRINT IMAGE WIDTH:  " + PrintDetails.PrintImageWidth);
                    Document.AppendLine("* PRINT IMAGE LENGTH " + PrintDetails.PrintImageLength);

                    
                    if (PrintDetails.Reverse==true)
                    {
                        Document.AppendLine("* REVERSE.");
                    }
                    if (PrintDetails.Surface == true)
                    {
                        Document.AppendLine("* **** SURFACE PRINT****");
                    }

                    Document.AppendLine("* " + MasterCardHeader.ProductDescription);

                    if (PrintDetails.Reverse == true)
                    {
                        Document.AppendLine("* REVERSE PTD " + PrintColors.Count + " COLS,");
                    }
                    foreach (var item in PrintColors)
                    {
                        Document.Append(item.Colour+", ");
                    }
                    Document.AppendLine("* " + MasterCardHeader.PrintDescription+"**");

                    Document.AppendLine("* " + PrintDetails.PitchOrStep+ "TOLERANCE ON THE STEP/PITCH.");
                    Document.AppendLine("* CYLINDER cm: " + PrintDetails.CylinderCM);
                    Document.AppendLine("* " + PrintDetails.PrintDescription);
                    Document.AppendLine("* BARCODE: "+PrintDetails.Barcode);
                    Document.AppendLine("* BARCODE COLOUR: " + PrintDetails.BarcodeColour);
                    //Document.AppendLine("* No. ACROSS: " + PrintDetails.NumberAccross);
                    //Document.AppendLine("* No. AROUND: " + PrintDetails.NumberAround);

                    if (PrintDetails.Parev==true)
                    {
                        Document.AppendLine("* PAREV.");
                    }
                    if (PrintDetails.Halaal==true)
                    {
                        Document.AppendLine("* HALAAL.");
                    }
                    
                    Document.AppendLine("* FOLLOW SIGNED ARTWORK FOR PRINT LAYOUT, WORK TOWARDS CUSTOMERS SPECIFICATION,");
                    Document.AppendLine("* FOLLOW PANTONES FOR COLOUR MATCHING, MUST BE BRIGHT AND RICH.");
                    Document.AppendLine("* PRINT REGISTER MUST BE 100%, NO CHANCERS.");
                    Document.AppendLine("* CORRECT INK SYSTEM MUST BE USED.");
                    Document.AppendLine("* OPERATOR TO INSERT CORRECT DATE CODES FOR TRACEABILITY.");
                    Document.AppendLine("* SPECIAL INSTRUCTIONS: "+PrintDetails.SpecialInstructions);

                    Print.Narration = Document.ToString();

                    results.Add(Print);
                }
                if (MasterCardHeader.Slitting==true)
                {
                    StringBuilder Document = new StringBuilder();

                    var Slit = new sp_MasterCardGetSysproBomOperations_Result();
                    var workCentre = (from a in wdb.mtMasterCardSlittings where a.Id == Id select a.WorkCentre).FirstOrDefault();
                    var RunTime = (from a in wdb.BomWorkCentres where a.WorkCentre == workCentre select a.RunTime).FirstOrDefault();
                    Slit.StockCode = ParentPart;
                    Slit.WorkCentre = workCentre;
                    Slit.Route = "0";
                    if (MasterCardHeader.Quantity == null)
                    {
                        Slit.Quantity = 0;
                    }
                    else
                    {
                        Slit.Quantity = MasterCardHeader.Quantity;
                    }
                    var Runtime = wdb.sp_MasterCardGetUnitRunTime(workCentre, Mass).FirstOrDefault();
                    Slit.RunTime = Runtime.UnitRunTime;

                    var SlitDetails = (from a in wdb.mtMasterCardSlittings where a.Id == Id select a).FirstOrDefault();
                    Document.AppendLine("* MICRON: "+MasterCardHeader.Micron);
                    Document.AppendLine("* TOLERANCE: "+SlitDetails.Tolerance);
                    Document.AppendLine("* SLIT WIDTH(mm): "+SlitDetails.SlitWidth);
                    Document.AppendLine("* KGS PER REEL(kgs): "+SlitDetails.KgPerReel);
                    Document.AppendLine("* CORE WALL(mm): "+SlitDetails.CoreWall);
                    Document.AppendLine("* CORE ID(mm): " + SlitDetails.CoreId);
                    if (SlitDetails.LowTension==true)
                    {
                        Document.AppendLine("* LOW TENSION. ");
                    }
                    if (SlitDetails.FlushEvenWinding)
                    {
                        Document.AppendLine("* FLUSH AND EVEN WINDING.");
                    }
                    if (SlitDetails.NoMisprints==true)
                    {
                        Document.AppendLine("* NO MISPRINT");
                    }
                    Document.AppendLine("* TOLERANCE ON EDGE WEAVE: " + SlitDetails.ToleranceOnWedgeWeave);
                    Document.AppendLine("* TOLERANCE ON STEP: " + SlitDetails.ToleranceOnStep);

                    Document.AppendLine("* FOLLOW SIGNED ARTWORK FOR PRINT LAYOUT, WORK TOWARDS CUSTOMERS SPECIFICATION,");
                    Document.AppendLine("* FOLLOW PANTONES FOR COLOUR MATCHING, MUST BE BRIGHT AND RICH.");
                    Document.AppendLine("* SPECIAL INSTRUCTIONS: " + SlitDetails.SpecialInstructions);

                    Slit.Narration = Document.ToString();
                    results.Add(Slit);

                }
                if (MasterCardHeader.Bagging==true)
                {
                    StringBuilder Document = new StringBuilder();

                    var Bag = new sp_MasterCardGetSysproBomOperations_Result();
                    var workCentre = (from a in wdb.mtMasterCardBaggings where a.Id == Id select a.WorkCentre).FirstOrDefault();
                    var RunTime = (from a in wdb.BomWorkCentres where a.WorkCentre == workCentre select a.RunTime).FirstOrDefault();
                    Bag.StockCode = ParentPart;
                    Bag.WorkCentre = workCentre;
                    Bag.Route = "0";
                    if (MasterCardHeader.Quantity == null)
                    {
                        Bag.Quantity = 0;
                    }
                    else
                    {
                        Bag.Quantity = MasterCardHeader.Quantity;
                    }
                    var Runtime = wdb.sp_MasterCardGetUnitRunTime(workCentre, Mass).FirstOrDefault();
                    Bag.RunTime = Runtime.UnitRunTime;

                    var BagDetails = (from a in wdb.mtMasterCardBaggings where a.Id == Id select a).FirstOrDefault();
                    Document.AppendLine("* MICRON: " + MasterCardHeader.Micron);
                    Document.AppendLine("* LENGTH: " + MasterCardHeader.Length);
                    Document.AppendLine("* WIDTH: " + MasterCardHeader.Width);
                    if (BagDetails.DropTest==true)
                    {
                        Document.AppendLine("* DROP TEST ");
                    }
                    if (BagDetails.LeakProofTest==true)
                    {
                        Document.AppendLine("* LEAK PROOF TEST");
                    }
                    if (BagDetails.OvalCutWithHandle==true)
                    {
                        Document.AppendLine("* OVAL CUT WITH/WITHOUT HANDLE ");
                    }
                    if (BagDetails.NoBlockingMisprints == true)
                    {
                        Document.AppendLine("* NO BLOCKING, NO MISPRINTS ");
                    }
                    if (BagDetails.PanelsFlush == true)
                    {
                        Document.AppendLine("* PANELS TO BE FLUSH ");
                    }
                    if (BagDetails.StrongSideSeals == true)
                    {
                        Document.AppendLine("* STRONG SIDE SEALS ");
                    }
                    if (BagDetails.StrongSpineSeal == true)
                    {
                        Document.AppendLine("* STRONG FIN/SPIN SEAL ");
                    }
                    if (BagDetails.PrintCentral==true)
                    {
                        Document.AppendLine("* PRINT TO BE CENTRAL. ");
                    }
                    if (BagDetails.StrongTopBottomSeal == true)
                    {
                        Document.AppendLine("* STRONG TOP/BOTTOM SEAL ");
                    }
                    if (!string.IsNullOrEmpty(BagDetails.ToleranceOnWidth))
                    {
                        Document.AppendLine("* TOLERANCE ON WIDTH: "+ BagDetails.ToleranceOnWidth);
                    }
                    if (!string.IsNullOrEmpty(BagDetails.ToleranceOnLength))
                    {
                        Document.AppendLine("* TOLERANCE ON LENGTH: " + BagDetails.ToleranceOnWidth);
                    }
                    
                    Document.AppendLine("* SPECIAL INSTRUCTIONS: " + BagDetails.SpecialInstructions);

                    Bag.Narration = Document.ToString();
                    results.Add(Bag);
                }
                if (MasterCardHeader.Wicketing==true)
                {
                    StringBuilder Document = new StringBuilder();

                    var Wicket = new sp_MasterCardGetSysproBomOperations_Result();
                    var workCentre = (from a in wdb.mtMasterCardWicketings where a.Id == Id select a.WorkCentre).FirstOrDefault();
                    var RunTime = (from a in wdb.BomWorkCentres where a.WorkCentre == workCentre select a.RunTime).FirstOrDefault();
                    Wicket.StockCode = ParentPart;
                    Wicket.WorkCentre = workCentre;
                    Wicket.Route = "0";
                    if (MasterCardHeader.Quantity == null)
                    {
                        Wicket.Quantity = 0;
                    }
                    else
                    {
                        Wicket.Quantity = MasterCardHeader.Quantity;
                    }
                    var Runtime = wdb.sp_MasterCardGetUnitRunTime(workCentre, Mass).FirstOrDefault();
                    Wicket.RunTime = Runtime.UnitRunTime;

                    var WicketDetails = (from a in wdb.mtMasterCardWicketings where a.Id == Id select a).FirstOrDefault();
                    Document.AppendLine("* MICRON: " + MasterCardHeader.Micron);
                    Document.AppendLine("* LENGTH: " + MasterCardHeader.Length);
                    Document.AppendLine("* WIDTH: " + MasterCardHeader.Width);
                    Document.AppendLine("* SPECIAL INSTRUCTIONS: " + WicketDetails.SpecialInstructions);

                    Wicket.Narration = Document.ToString();
                    results.Add(Wicket);
                }
                if (MasterCardHeader.Lamination==true)
                {
                    StringBuilder Document = new StringBuilder();

                    var Lam = new sp_MasterCardGetSysproBomOperations_Result();
                    var workCentre = (from a in wdb.mtMasterCardLaminations where a.Id == Id select a.WorkCentre).FirstOrDefault();
                    var RunTime = (from a in wdb.BomWorkCentres where a.WorkCentre == workCentre select a.RunTime).FirstOrDefault();
                    Lam.StockCode = ParentPart;
                    Lam.WorkCentre = workCentre;
                    Lam.Route = "0";
                    if (MasterCardHeader.Quantity == null)
                    {
                        Lam.Quantity = 0;
                    }
                    else
                    {
                        Lam.Quantity = MasterCardHeader.Quantity;
                    }
                    var Runtime = wdb.sp_MasterCardGetUnitRunTime(workCentre, Mass).FirstOrDefault();
                    Lam.RunTime = Runtime.UnitRunTime;

                    var LamDetails = (from a in wdb.mtMasterCardLaminations where a.Id == Id select a).FirstOrDefault();
                    Document.AppendLine("* MICRON: " + MasterCardHeader.Micron);
                    Document.AppendLine("* LENGTH: " + MasterCardHeader.Length);
                    Document.AppendLine("* WIDTH: " + MasterCardHeader.Width);
                    Document.AppendLine("* SPECIAL INSTRUCTIONS: " + LamDetails.SpecialInstructions);

                    Lam.Narration = Document.ToString();
                    results.Add(Lam);
                }
               // var result = wdb.sp_MasterCardGetBomStructure(Id, MasterCardHeader.StockCode, "0", "M").ToList().Where(a => a.LevelId != 0).ToList();

                return Json(results, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
