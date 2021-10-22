using Megasoft2.Models;
using Megasoft2.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Megasoft2.Controllers
{
    public class MasterCardMainController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        [CustomAuthorize("MasterCard")]
        public ActionResult Index()
        {

            //ViewBag.Json = (new JavaScriptSerializer()).Serialize(GetBomStructure());
            MasterCardViewModel model = new MasterCardViewModel();
            mtMasterCardHeader head = new mtMasterCardHeader();
            head.StockCode = "";
            model.Header = head;
            GetDropDownData();
            return View(model);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Save")]
        public ActionResult Index(MasterCardViewModel model, HttpPostedFileBase FileUpload)
        {
            GetDropDownData();
            try
            {
                ModelState.Clear();
                if (ModelState.IsValid)
                {

                    var FilePath = "";
                    var filename = "";
                    var Path = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a.MaterCardMultimediaPath).FirstOrDefault();
                    if (FileUpload != null)
                    {
                        filename = FileUpload.FileName;


                        string targetpath = Path;

                        FileUpload.SaveAs(targetpath + filename);
                        FilePath = targetpath + filename;

                    }

                    if (model.Header.Id == 0)
                    {

                        //New Entry
                        model.Header.DateSaved = DateTime.Now;
                        model.Header.MultiMediaFilePath = FilePath;
                        wdb.mtMasterCardHeaders.Add(model.Header);
                        wdb.SaveChanges();
                    }

                    if (FileUpload != null)
                    {
                        model.Header.MultiMediaFilePath = FilePath;
                    }

                    wdb.mtMasterCardHeaders.Attach(model.Header);
                    wdb.Entry(model.Header).State = EntityState.Modified;

                    if (model.Header.Extrusion)
                    {
                        model.Extrusion.Id = model.Header.Id;
                        var find = wdb.mtMasterCardExtrusions.Find(model.Header.Id);
                        if (find == null)
                        {
                            wdb.mtMasterCardExtrusions.Add(model.Extrusion);
                        }
                        else
                        {
                            wdb.Entry(find).CurrentValues.SetValues(model.Extrusion);
                        }
                    }

                    if (model.Header.Printing)
                    {
                        model.Printing.Id = model.Header.Id;
                        var find = wdb.mtMasterCardPrintings.Find(model.Header.Id);
                        if (find == null)
                        {

                            wdb.mtMasterCardPrintings.Add(model.Printing);
                        }
                        else
                        {
                            wdb.Entry(find).CurrentValues.SetValues(model.Printing);
                        }
                    }

                    if (model.Header.Slitting)
                    {
                        model.Slitting.Id = model.Header.Id;
                        var find = wdb.mtMasterCardSlittings.Find(model.Header.Id);
                        if (find == null)
                        {

                            wdb.mtMasterCardSlittings.Add(model.Slitting);
                        }
                        else
                        {
                            wdb.Entry(find).CurrentValues.SetValues(model.Slitting);
                        }
                    }

                    if (model.Header.Bagging)
                    {
                        model.Bagging.Id = model.Header.Id;
                        var find = wdb.mtMasterCardBaggings.Find(model.Header.Id);
                        if (find == null)
                        {
                            wdb.mtMasterCardBaggings.Add(model.Bagging);
                        }
                        else
                        {
                            wdb.Entry(find).CurrentValues.SetValues(model.Bagging);
                        }
                    }

                    if (model.Header.Wicketing)
                    {
                        model.Wicketing.Id = model.Header.Id;
                        var find = wdb.mtMasterCardWicketings.Find(model.Header.Id);
                        if (find == null)
                        {
                            wdb.mtMasterCardWicketings.Add(model.Wicketing);
                        }
                        else
                        {
                            wdb.Entry(find).CurrentValues.SetValues(model.Wicketing);
                        }
                    }
                    if (model.Header.Lamination)
                    {
                        model.Lamination.Id = model.Header.Id;
                        var find = wdb.mtMasterCardLaminations.Find(model.Header.Id);
                        if (find == null)
                        {
                            wdb.mtMasterCardLaminations.Add(model.Lamination);
                        }
                        else
                        {
                            wdb.Entry(find).CurrentValues.SetValues(model.Lamination);
                        }
                    }
                    if (model.Header.Other)
                    {
                        model.Other.Id = model.Header.Id;
                        var find = wdb.mtMasterCardOthers.Find(model.Header.Id);
                        if (find == null)
                        {
                            wdb.mtMasterCardOthers.Add(model.Other);
                        }
                        else
                        {
                            wdb.Entry(find).CurrentValues.SetValues(model.Other);
                        }
                    }
                    wdb.SaveChanges();



                }

                GetWorkCentres();
                ViewBag.ValidId = true;
                bool StockCodeExists = false;
                var SysCheck = (from a in wdb.InvMasters where a.StockCode == model.Header.StockCode select a).ToList();
                if (SysCheck.Count > 0)
                {
                    StockCodeExists = true;
                }
                ViewBag.StockCodeExists = StockCodeExists;
                ModelState.AddModelError("", "Saved Successfully");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ViewBag.ValidId = true;
                GetWorkCentres();
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "GetMasterCardDetails")]
        public ActionResult LoadMasterCardDetails(MasterCardViewModel master)
        {
            GetDropDownData();
            try
            {
                ModelState.Clear();
                MasterCardViewModel model = new MasterCardViewModel();
                int Id = master.Header.Id;
                bool StockCodeExists = false;
                if (ModelState.IsValid)
                {
                    if (Id != 0)
                    {
                        var CheckIfIdExists = wdb.mtMasterCardHeaders.Find(Id);
                        if (CheckIfIdExists != null)
                        {
                            model = GetMasterCardDetails(Id);
                            GetWorkCentres();
                            ViewBag.ValidId = true;

                            if (!string.IsNullOrWhiteSpace(CheckIfIdExists.StockCode))
                            {
                                var SysCheck = (from a in wdb.InvMasters where a.StockCode == CheckIfIdExists.StockCode select a).ToList();
                                if (SysCheck.Count > 0)
                                {
                                    StockCodeExists = true;
                                }
                            }
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
                ViewBag.StockCodeExists = StockCodeExists;

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", master);
            }
        }

        public void GetWorkCentres()
        {
            var WorkCentreList = wdb.BomWorkCentres.ToList();
            ViewBag.ExtrusionWorkCentres = (from m in WorkCentreList where m.CostCentre == "EXTR" select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
            ViewBag.PrintingWorkCentres = (from m in WorkCentreList where m.CostCentre == "PRINT" select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
            ViewBag.SlittingWorkCentres = (from m in WorkCentreList where m.CostCentre == "SLIT" select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
            ViewBag.BaggingWorkCentres = (from m in WorkCentreList where m.CostCentre == "BAG" select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
            ViewBag.WickettingWorkCentres = (from m in WorkCentreList where m.CostCentre == "WICKET" select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
            ViewBag.LaminationWorkCentres = (from m in WorkCentreList where m.CostCentre == "LAM" select new { Value = m.WorkCentre, Text = m.WorkCentre + " - " + m.Description }).ToList();
        }

        public ActionResult GetNextStockCode(string customer, string StockCode)
        {
            var result = wdb.sp_MasterCardGetNextStockCode(customer).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);

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
                    model.CustomerName = CustName.Name;
                }
            }
            if (model.Header.Extrusion)
            {
                model.Extrusion = wdb.mtMasterCardExtrusions.Find(Id);
            }

            if (model.Header.Printing)
            {
                model.Printing = wdb.mtMasterCardPrintings.Find(Id);
            }

            if (model.Header.Slitting)
            {
                model.Slitting = wdb.mtMasterCardSlittings.Find(Id);
            }

            if (model.Header.Bagging)
            {
                model.Bagging = wdb.mtMasterCardBaggings.Find(Id);
            }
            if (model.Header.Wicketing)
            {
                model.Wicketing = wdb.mtMasterCardWicketings.Find(Id);
            }
            if (model.Header.Lamination)
            {
                model.Lamination = wdb.mtMasterCardLaminations.Find(Id);
            }
            if (model.Header.Other)
            {
                model.Other = wdb.mtMasterCardOthers.Find(Id);
            }

            if (string.IsNullOrWhiteSpace(model.Header.MultiMediaFilePath))
            {
                var settings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
                if (settings != null)
                {
                    model.CopyTo = settings.MaterCardMultimediaPath;
                }
            }
            else
            {
                model.CopyTo = model.Header.MultiMediaFilePath;
            }

            return model;
        }


        public ActionResult ExtrusionPartial()
        {
            return View();
        }

        public ActionResult PrintingPartial()
        {
            return View();
        }


        public ActionResult SlittingPartial()
        {
            return View();
        }

        public ActionResult BaggingPartial()
        {
            return View();
        }

        public ActionResult WickettingPartial()
        {
            return View();
        }

        public ActionResult LaminationPartial()
        {
            return View();
        }
        public ActionResult OtherPartial()
        {
            return View();
        }
        public ActionResult UnwindDirectionPartial()
        {
            return PartialView();
        }

        public ActionResult BomPartial()
        {
            return PartialView();
        }
        public ActionResult BrowseMasterCards()
        {
            return PartialView();
        }
        public ActionResult CustomerSearch()
        {
            return PartialView();
        }
        public ActionResult AddCustomerIndex()
        {
            return PartialView();
        }
        public ActionResult BrowseHandles()
        {
            return PartialView();
        }

        public ActionResult BrowsePunches()
        {
            return PartialView();
        }
        public ActionResult HandleList(string FilterText)
        {
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult PunchList(string FilterText)
        {
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult MasterCardList(string FilterText)
        {
            var result = wdb.mtMasterCardHeaders.ToList();
            var MasterCards = (from s in result
                               select new
                               {
                                   Id = s.Id
              ,
                                   StockCode = (s.StockCode ?? string.Empty)
              ,
                                   ProductDescription = (s.ProductDescription ?? string.Empty)
              ,
                                   PrintDescription = (s.PrintDescription ?? string.Empty)
              ,
                                   Customer = (s.Customer ?? string.Empty)
              ,
                                   DeliveryDate = (s.DeliveryDate.HasValue ? s.DeliveryDate.Value.Date.ToShortDateString() : null),
                                   Length = (s.Length ?? 0),
                                   Width = (s.Width ?? 0),
                                   Quantity = (s.Quantity ?? 0),
                                   Micron = (s.Micron ?? 0),
                                   Weight = (s.Weight ?? 0),
                                   DateSaved = (s.DateSaved.HasValue ? s.DateSaved.Value.Date.ToShortDateString() : null),
                                   Status = (s.Status ?? 0)
                               }).ToList();
            return Json(MasterCards, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CustomerList(string FilterText)
        {
            var result = wdb.sp_GetStereoCustomers(FilterText).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult MasterCardCustomerList()
        {
            var result = wdb.mtMasterCardCustomers.ToList();
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetPrintingColours(string Id)
        {
            int QuoteId = Convert.ToInt32(Id);
            var result = wdb.mtMasterCardPrintingColours.ToList();
            var Colours = (from s in result
                           where s.Id == QuoteId
                           select new
                           {
                               QuoteId = s.Id,
                               s.ColourId,
                               s.Colour

                           }).ToList();
            return Json(Colours, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SavePrintingColour(string Id, string ColourToSave)
        {
            try
            {
                string Colour = ColourToSave.ToUpper();
                int QuoteId = Convert.ToInt32(Id);
                var CheckIfExists = (from a in wdb.mtMasterCardPrintingColours where a.Id == QuoteId && a.Colour == Colour select a).ToList();
                if (CheckIfExists.Count > 0)
                {
                    return Json("Error: Colour already added.", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    mtMasterCardPrintingColour newcol = new mtMasterCardPrintingColour();
                    newcol.Id = QuoteId;
                    var ColourCount = (from a in wdb.mtMasterCardPrintingColours where a.Id == QuoteId select a).ToList();
                    if (ColourCount.Count == 0)
                    {
                        newcol.ColourId = 1;
                    }
                    else
                    {
                        newcol.ColourId = ColourCount.Max(x => x.ColourId) + 1;
                    }
                    newcol.Colour = Colour;
                    wdb.mtMasterCardPrintingColours.Add(newcol);
                    wdb.SaveChanges();
                }

                var result = wdb.mtMasterCardPrintingColours.ToList();
                var Colours = (from s in result
                               where s.Id == QuoteId
                               select new
                               {
                                   QuoteId = s.Id,
                                   s.ColourId,
                                   s.Colour

                               }).ToList();
                return Json(Colours, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json("Error saving: " + e, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult DeletePrintingColour(string Id, string CoulorId)
        {
            try
            {
                int ColId = Convert.ToInt32(CoulorId);
                int QuoteId = Convert.ToInt32(Id);

                var x = (from y in wdb.mtMasterCardPrintingColours
                         where y.Id == QuoteId && y.ColourId == ColId
                         select y).FirstOrDefault();
                if (x != null)
                {
                    wdb.mtMasterCardPrintingColours.Remove(x);
                    wdb.SaveChanges();

                    var result = wdb.mtMasterCardPrintingColours.ToList();
                    var Colours = (from s in result
                                   where s.Id == QuoteId
                                   select new
                                   {
                                       QuoteId = s.Id,
                                       s.ColourId,
                                       s.Colour

                                   }).ToList();
                    return Json(Colours, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error: Colour not found.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json("Error saving: " + e, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SaveCustomer(string Name, string Contact, string Telephone, string Email, string ShipToAddr1, string ShipToAddr2, string ShipToAddr3, string ShipToAddr4, string ShipToAddr5)
        {
            try
            {

                mtMasterCardCustomer mtMasterCardCustomer = new mtMasterCardCustomer();
                string StripName = Name.Substring(0, 3);
                var CountCust = (from a in wdb.mtMasterCardCustomers select a).Where(oh => oh.Customer.StartsWith(StripName)).ToList();
                if (CountCust.Count == 0)
                {
                    mtMasterCardCustomer.Customer = StripName + "001";
                }
                else
                {
                    var Number = Convert.ToInt32((from b in CountCust select new { Customer = b.Customer, Id = b.Customer.Substring(b.Customer.Length - 3) }).ToList().Max(c => c.Id)) + 1;
                    mtMasterCardCustomer.Customer = StripName + Convert.ToString(Number).PadLeft(3, '0');
                }
                string CustomerCode = mtMasterCardCustomer.Customer;
                mtMasterCardCustomer.Name = Name;
                mtMasterCardCustomer.Contact = Contact;
                mtMasterCardCustomer.Telephone = Telephone;
                mtMasterCardCustomer.Email = Email;
                mtMasterCardCustomer.ShipToAddr1 = ShipToAddr1;
                mtMasterCardCustomer.ShipToAddr2 = ShipToAddr2;
                mtMasterCardCustomer.ShipToAddr3 = ShipToAddr3;
                mtMasterCardCustomer.ShipToAddr4 = ShipToAddr4;
                mtMasterCardCustomer.ShipToAddr5 = ShipToAddr5;

                wdb.mtMasterCardCustomers.Add(mtMasterCardCustomer);
                wdb.SaveChanges();

                return Json(CustomerCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json("Error saving: " + e, JsonRequestBehavior.AllowGet);
            }

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
            var bomstruct = wdb.sp_MasterCardGetBomStructure(KeyId, Parent, "0", "M").ToList().Select(a => new TreeModel { Parent = a.ParentPart, Component = a.Component }).ToList();

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


        [HttpGet]
        public ActionResult GetParentComponents(int KeyId, string ParentPart)
        {
            try
            {
                var result = (from a in wdb.mtMasterCardBomStructures.AsNoTracking() where a.Source == "M" && a.Id == KeyId && a.ParentPart == ParentPart select a).ToList();
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
                obj = (from a in wdb.mtMasterCardBomStructures where a.Source == "M" && a.Id == id && a.ParentPart == ParentPart && a.Component == Component && a.SequenceNum == SequenceNum select new MasterCardComponent { Id = a.Id, ParentPart = a.ParentPart, Component = a.Component, Route = a.Route, SequenceNum = a.SequenceNum, QtyPer = a.QtyPer, LayerPerc = a.LayerPerc, ScrapQuantity = a.ScrapQuantity, ScrapPercentage = a.ScrapPercentage, Mode = Mode }).FirstOrDefault();
            }
            else
            {
                ViewBag.NewComponent = true;
            }
            return PartialView(obj);

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
                        var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "M" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.SequenceNum == bom.SequenceNum && a.Component == bom.Component && a.Route == bom.Route select a).FirstOrDefault();
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
                        string SequenceNum = "00";
                        var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "M" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component select a).ToList();
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
                        obj.Source = "M";
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
                    var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "M" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component && a.Route == bom.Route && a.SequenceNum == bom.SequenceNum select a).FirstOrDefault();
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
                        var delitems = (from a in wdb.mtMasterCardBomStructures where a.Source == "M" && a.Id == KeyId && a.ParentPart == ToStockCode select a).ToList();
                        foreach (var item in delitems)
                        {
                            wdb.Entry(item).State = EntityState.Deleted;
                            wdb.SaveChanges();
                        }

                        foreach (var bom in myDeserializedObject)
                        {

                            string SequenceNum = "00";
                            var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "M" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component select a).ToList();
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
                            obj.Source = "M";
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
                            var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "M" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Component == bom.Component select a).ToList();
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
                            obj.Source = "M";
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
                            var check = (from a in wdb.mtMasterCardBomStructures where a.Source == "M" && a.Id == bom.Id && a.ParentPart == bom.ParentPart && a.Route == "0" && a.Component == bom.Component select a).ToList();
                            if (check.Count == 0)
                            {
                                mtMasterCardBomStructure obj = new mtMasterCardBomStructure();
                                obj.Id = bom.Id;
                                obj.ParentPart = bom.ParentPart;
                                obj.Component = bom.Component;
                                obj.Route = "0";
                                obj.SequenceNum = SequenceNum;
                                obj.Source = "M";
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
                var obj = (from a in wdb.mtMasterCardBomOperations where a.Id == id && a.StockCode == StockCode && a.Operation == Operation select new MasterCardBomOperation { Id = a.Id, StockCode = a.StockCode, Operation = a.Operation, Route = a.Route, WorkCentre = a.WorkCentre, Mode = "Change" }).FirstOrDefault();
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
                        check.WorkCentre = bom.WorkCentre;
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

        public JsonResult StockCodeList()
        {
            var result = wdb.sp_MasterCardGetAllStockCodes("").ToList();
            var Stock = (from a in result select new { MStockCode = a.StockCode, MStockDes = a.Description, MStockingUom = a.StockUom }).Distinct().ToList();
            return Json(Stock, JsonRequestBehavior.AllowGet);
        }
        public ActionResult MasterCardCreation()
        {
            return View();
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadMasterCardData")]
        public ActionResult LoadMasterCardData(MasterCardViewModel master)
        {
            try
            {
                ModelState.Clear();
                int Id = master.Header.Id;
                if (ModelState.IsValid)
                {
                    if (Id != 0)
                    {
                        var CheckIfIdExists = wdb.mtMasterCardHeaders.Find(Id);
                        if (CheckIfIdExists != null)
                        {
                            master.StockCode = wdb.sp_GetMasterCardNewStockCode(CheckIfIdExists.Customer).FirstOrDefault().NewStockCode;
                            master.Departments = GetMasterCardDepartments(Id, master.StockCode);
                            ViewBag.ValidId = true;
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

                return View("MasterCardCreation", master);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("MasterCardCreation", master);
            }
        }
        public List<MasterCardViewModel.DepartmentList> GetMasterCardDepartments(int MasterCardId, string NewStockCode)
        {
            var HeaderInfo = (from a in wdb.mtMasterCardHeaders where a.Id == MasterCardId select a).FirstOrDefault();
            List<MasterCardViewModel.DepartmentList> list = new List<MasterCardViewModel.DepartmentList>();
            if (HeaderInfo.Extrusion == true)
            {
                var Extrusion = (from a in wdb.mtMasterCardExtrusions where a.Id == MasterCardId select a.SysproStockCode).FirstOrDefault();
                list.Add(new MasterCardViewModel.DepartmentList { Department = "EXTRUSION", StockCode = Extrusion ?? NewStockCode });
            }
            if (HeaderInfo.Slitting == true)
            {
                var Slitting = (from a in wdb.mtMasterCardSlittings where a.Id == MasterCardId select a.SysproStockCode).FirstOrDefault();
                list.Add(new MasterCardViewModel.DepartmentList { Department = "SLITTING", StockCode = Slitting ?? NewStockCode });
            }
            if (HeaderInfo.Bagging == true)
            {
                var Bagging = (from a in wdb.mtMasterCardBaggings where a.Id == MasterCardId select a.SysproStockCode).FirstOrDefault();
                list.Add(new MasterCardViewModel.DepartmentList { Department = "BAGGING", StockCode = Bagging ?? NewStockCode });
            }
            if (HeaderInfo.Wicketing == true)
            {
                var Wicketing = (from a in wdb.mtMasterCardWicketings where a.Id == MasterCardId select a.SysproStockCode).FirstOrDefault();
                list.Add(new MasterCardViewModel.DepartmentList { Department = "WICKETING", StockCode = Wicketing ?? NewStockCode });
            }
            if (HeaderInfo.Printing == true)
            {
                var Printing = (from a in wdb.mtMasterCardPrintings where a.Id == MasterCardId select a.SysproStockCode).FirstOrDefault();
                list.Add(new MasterCardViewModel.DepartmentList { Department = "PRINTING", StockCode = Printing ?? NewStockCode });
            }
            if (HeaderInfo.Lamination == true)
            {
                var Lamination = (from a in wdb.mtMasterCardLaminations where a.Id == MasterCardId select a.SysproStockCode).FirstOrDefault();
                list.Add(new MasterCardViewModel.DepartmentList { Department = "LAMINATION", StockCode = Lamination ?? NewStockCode });
            }
            return list;
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveMasterCardStockCodes")]
        public ActionResult SaveMasterCardStockCodes(MasterCardViewModel master)
        {
            try
            {
                ModelState.Clear();
                int Id = master.Header.Id;
                if (ModelState.IsValid)
                {
                    if (Id != 0)
                    {
                        var CheckIfIdExists = wdb.mtMasterCardHeaders.Find(Id);
                        if (CheckIfIdExists != null)
                        {
                            var HeaderInfo = (from a in wdb.mtMasterCardHeaders where a.Id == Id select a).FirstOrDefault();

                            if (HeaderInfo.Extrusion == true)
                            {
                                var Extrusion = wdb.mtMasterCardExtrusions.Find(Id);
                                Extrusion.SysproStockCode = master.Departments.Where(a => a.Department == "EXTRUSION").FirstOrDefault().StockCode;
                                wdb.Entry(Extrusion).CurrentValues.SetValues(Extrusion);
                            }
                            if (HeaderInfo.Slitting == true)
                            {
                                var Slitting = wdb.mtMasterCardSlittings.Find(Id);
                                Slitting.SysproStockCode = master.Departments.Where(a => a.Department == "SLITTING").FirstOrDefault().StockCode;
                                wdb.Entry(Slitting).CurrentValues.SetValues(Slitting);
                            }
                            if (HeaderInfo.Bagging == true)
                            {
                                var Bagging = wdb.mtMasterCardBaggings.Find(Id);
                                Bagging.SysproStockCode = master.Departments.Where(a => a.Department == "BAGGING").FirstOrDefault().StockCode;
                                wdb.Entry(Bagging).CurrentValues.SetValues(Bagging);
                            }
                            if (HeaderInfo.Wicketing == true)
                            {
                                var Wicketing = wdb.mtMasterCardBaggings.Find(Id);
                                Wicketing.SysproStockCode = master.Departments.Where(a => a.Department == "BAGGING").FirstOrDefault().StockCode;
                                wdb.Entry(Wicketing).CurrentValues.SetValues(Wicketing);
                            }
                            if (HeaderInfo.Printing == true)
                            {
                                var Printing = wdb.mtMasterCardPrintings.Find(Id);
                                Printing.SysproStockCode = master.Departments.Where(a => a.Department == "PRINTING").FirstOrDefault().StockCode;
                                wdb.Entry(Printing).CurrentValues.SetValues(Printing);
                            }
                            if (HeaderInfo.Lamination == true)
                            {
                                var Lamination = wdb.mtMasterCardLaminations.Find(Id);
                                Lamination.SysproStockCode = master.Departments.Where(a => a.Department == "LAMINATION").FirstOrDefault().StockCode;
                                wdb.Entry(Lamination).CurrentValues.SetValues(Lamination);
                            }
                            wdb.SaveChanges();
                            ModelState.AddModelError("", "Saved Successfully.");
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

                return View("MasterCardCreation", master);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("MasterCardCreation", master);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "StockCodeSetup")]
        public ActionResult StockCodeCreationIndex(MasterCardViewModel master)
        {
            MasterCardViewModel model = new MasterCardViewModel();
            try
            {
                int Id = master.Header.Id;
                model.MasterCardStockCode = (from a in wdb.mtMasterCardStockCodes where a.Id == Id select a).ToList();
                {
                    foreach (var item in master.Departments)
                    {

                        var checkIfExists = (from a in wdb.mtMasterCardStockCodes where a.Id == Id && a.StockCode == item.StockCode.ToUpper() select a).FirstOrDefault();
                        if (checkIfExists == null)
                        {
                            mtMasterCardStockCode st = new mtMasterCardStockCode();
                            st.Id = Id;
                            st.StockCode = item.StockCode;
                            //Check if stock code exists in syspro
                            var sys = (from a in wdb.InvMasters where a.StockCode == item.StockCode select a).FirstOrDefault();
                            if (sys != null)
                            {
                                st.StockCodeCreated = "Y";
                                st.WarehouseCreated = "Y";
                                st.PriceCodeCreated = "Y";
                            }
                            else
                            {
                                st.StockCodeCreated = "N";
                                st.WarehouseCreated = "N";
                                st.PriceCodeCreated = "N";
                            }
                            st.PriceCategory = "A";
                            st.PriceCategory = "M";

                            wdb.mtMasterCardStockCodes.Add(st);
                            wdb.SaveChanges();
                        }
                    }
                    GetDropDownData();
                    model.MasterCardStockCode = (from a in wdb.mtMasterCardStockCodes where a.Id == Id select a).ToList();
                    return View("StockCodeCreationIndex", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.ToString());
                return View("MasterCardCreation", master);
            }


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
        [HttpPost]
        public ActionResult EditStockCode(int MasterCardId, string StockCode)
        {
            MasterCardViewModel model = new MasterCardViewModel();
            try
            {

                model.MasterCardStockCode = (from a in wdb.mtMasterCardStockCodes where a.Id == MasterCardId select a).ToList();
                return View("StockCodeCreationIndex", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.ToString());
                return View("StockCodeCreationIndex", model);
            }
        }
    }
}

