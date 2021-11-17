using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class StereoSystemAddStereoController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        private StereoSystem BL = new StereoSystem();
        //
        // GET: /StereoSystemAddStereo/

        [CustomAuthorize(Activity: "StereoEntry")]
        public ActionResult Index(int TrackId = 0)
        {
            ViewBag.IsValidTrackId = false;

            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadTrackId")]
        public ActionResult LoadTrackId(StereoSystemAddStereoViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (model.ReqNo == 0)
                {
                    ViewBag.IsValidTrackId = false;
                }
                else
                {
                    var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == model.ReqNo select a).ToList();
                    if (Header.Count > 0)
                    {
                        ViewBag.IsValidTrackId = true;
                        model.ReqNo = model.ReqNo;
                        model.Customer = Header.FirstOrDefault().Customer;
                        model.CustomerInvoice = Convert.ToChar(Header.FirstOrDefault().CustomerInvoice);
                        model.Invoice = Header.FirstOrDefault().Invoice;
                        model.StockCode = Header.FirstOrDefault().StockCode;
                        model.SalesOrder = Header.FirstOrDefault().SalesOrder;
                        model.SupplierReference = Header.FirstOrDefault().SupplierReference;
                        model.PlateCategory = Header.FirstOrDefault().PlateCategory;
                        model.DesignReference = Header.FirstOrDefault().DesignReference;
                        model.PrintDescription = Header.FirstOrDefault().PrintDescription;
                        model.BagSize = Convert.ToDecimal(Header.FirstOrDefault().BagSize);
                        model.CylSlvSize = Convert.ToDecimal(Header.FirstOrDefault().CylSlvSize);
                        model.Surface = Header.FirstOrDefault().Surface;
                        model.NumberAcross = Convert.ToDecimal(Header.FirstOrDefault().NumberAcross);
                        model.NumberAround = Convert.ToDecimal(Header.FirstOrDefault().NumberAround);
                        model.NumberSetsRequired = Convert.ToDecimal(Header.FirstOrDefault().NumberSetsRequired);
                        model.MaterialType = Header.FirstOrDefault().MaterialType;
                        model.Thickness = Convert.ToDecimal(Header.FirstOrDefault().Thickness);
                        model.NumberOfColoursFront = Convert.ToDecimal(Header.FirstOrDefault().NumberOfColoursFront);
                        model.NumberOfColoursReverse = Convert.ToDecimal(Header.FirstOrDefault().NumberOfColoursReverse);
                        model.SpecialInstructions = Header.FirstOrDefault().SpecialInstructions;
                        model.Quotation = Header.FirstOrDefault().Quotation;
                        model.Date = Convert.ToDateTime(Header.FirstOrDefault().QuotationDate);
                        model.Barcode = Header.FirstOrDefault().Barcode;
                        model.BarcodeColour = Header.FirstOrDefault().BarcodeColour;
                        model.EyeMark = Convert.ToChar(Header.FirstOrDefault().Eyemark);
                        model.Size = Convert.ToDecimal(Header.FirstOrDefault().Size);
                        model.NumberColours = Convert.ToInt32(Header.FirstOrDefault().NumberColours);
                        model.Position = Header.FirstOrDefault().Position;
                        model.DateProofRequired = Convert.ToDateTime(Header.FirstOrDefault().DateProofRequired);
                        model.DateStereosRequired = Convert.ToDateTime(Header.FirstOrDefault().DateStereosRequired);
                        model.Taxable = Convert.ToChar(Header.FirstOrDefault().Taxable);
                        model.ChargeCustomer = Convert.ToChar(Header.FirstOrDefault().ChargeCustomer);
                        model.ChargeTropic = Convert.ToChar(Header.FirstOrDefault().ChargeTropic);
                        model.ChangePlate = Convert.ToInt32(Header.FirstOrDefault().ChangePlate);
                        model.StereoType = Header.FirstOrDefault().StereoType;
                        var Authorize = (from a in mdb.mtOpFunctions where a.Username == User.Identity.Name.ToUpper() && a.Program == "Stereo" && a.ProgramFunction == "Authorize" select a).ToList();
                        if (Authorize.Count > 0)
                        {
                            model.Authorize = "Y";
                        }
                        else
                        {
                            model.Authorize = "N";
                        }
                    }
                    else
                    {
                        ViewBag.IsValidTrackId = false;
                        ModelState.AddModelError("", "Requisition not found!");
                    }
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ViewBag.IsValidTrackId = false;
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveStereoHdr")]
        public ActionResult SaveStereoHdr(StereoSystemAddStereoViewModel model)
        {
            try
            {
                ModelState.Clear();
                var NewTrackId = 0;
                using (var hdb = new WarehouseManagementEntities(""))
                {
                    var TrackId = (from a in hdb.mtStereoHdrs where a.ReqNo == model.ReqNo select a).ToList();
                    if (TrackId.Count == 0)
                    {
                        //ADD NEW
                        mtStereoHdr hdr = new mtStereoHdr();
                        hdr.Customer = model.Customer;
                        hdr.EntryDate = DateTime.Now;
                        hdr.CustomerInvoice = Convert.ToString(model.CustomerInvoice);
                        hdr.Invoice = model.Invoice;
                        hdr.StockCode = model.StockCode;
                        hdr.SalesOrder = model.SalesOrder;
                        hdr.PlateCategory = model.PlateCategory;
                        hdr.SupplierReference = model.SupplierReference;
                        hdr.DesignReference = model.DesignReference;
                        hdr.PrintDescription = model.PrintDescription;
                        hdr.BagSize = model.BagSize;
                        hdr.CylSlvSize = model.CylSlvSize;
                        hdr.Surface = model.Surface;
                        hdr.NumberAcross = model.NumberAcross;
                        hdr.NumberAround = model.NumberAround;
                        hdr.NumberSetsRequired = model.NumberSetsRequired;
                        hdr.MaterialType = model.MaterialType;
                        hdr.Thickness = model.Thickness;
                        hdr.NumberOfColoursFront = model.NumberOfColoursFront;
                        hdr.NumberOfColoursReverse = model.NumberOfColoursReverse;
                        hdr.SpecialInstructions = model.SpecialInstructions;
                        hdr.Quotation = model.Quotation;
                        hdr.QuotationDate = Convert.ToDateTime(model.Date);
                        hdr.Barcode = model.Barcode;
                        hdr.BarcodeColour = model.BarcodeColour;
                        hdr.Eyemark = Convert.ToString(model.EyeMark);
                        hdr.Size = model.Size;
                        hdr.NumberColours = model.NumberColours;
                        hdr.Position = model.Position;
                        hdr.DateProofRequired = Convert.ToDateTime(model.DateProofRequired);
                        hdr.DateStereosRequired = Convert.ToDateTime(model.DateStereosRequired);
                        hdr.Taxable = Convert.ToString(model.Taxable);
                        hdr.ChargeCustomer = Convert.ToString(model.ChargeCustomer);
                        hdr.ChargeTropic = Convert.ToString(model.ChargeTropic);
                        hdr.ChangePlate = model.ChangePlate;
                        hdr.StereoType = model.StereoType;
                        hdr.Variant = "N";
                        hdr.PoCreated = "N";
                        hdr.PurchaseOrder = "";
                        if(model.StereoType =="Customer")
                        {
                            hdr.PurchaseOrderRequired = "N";
                        }
                        else
                        {
                            hdr.PurchaseOrderRequired = "Y";
                        }
                        if (model.Approved == "Y")
                        {
                            hdr.Approved = model.Approved;
                            hdr.ApprovedBy = User.Identity.Name.ToUpper();
                            hdr.ApprovedDate = DateTime.Now;
                        }
                        hdb.Entry(hdr).State = EntityState.Added;
                        hdb.SaveChanges();

                        NewTrackId = hdr.ReqNo;
                    }
                    else
                    {
                        NewTrackId = model.ReqNo;
                        var check = (from a in hdb.mtStereoHdrs where a.ReqNo == NewTrackId select a).FirstOrDefault();
                        if (check != null)
                        {
                            //Update required
                            check.ReqNo = check.ReqNo; //Primary Key
                            check.Customer = model.Customer;
                            check.CustomerInvoice = Convert.ToString(model.CustomerInvoice);
                            check.Invoice = model.Invoice;
                            check.StockCode = model.StockCode;
                            check.SalesOrder = model.SalesOrder;
                            check.SupplierReference = model.SupplierReference;
                            check.PlateCategory = model.PlateCategory;
                            check.DesignReference = model.DesignReference;
                            check.PrintDescription = model.PrintDescription;
                            check.BagSize = model.BagSize;
                            check.CylSlvSize = model.CylSlvSize;
                            check.Surface = model.Surface;
                            check.NumberAcross = model.NumberAcross;
                            check.NumberAround = model.NumberAround;
                            check.NumberSetsRequired = model.NumberSetsRequired;
                            check.MaterialType = model.MaterialType;
                            check.Thickness = model.Thickness;
                            check.NumberOfColoursFront = model.NumberOfColoursFront;
                            check.NumberOfColoursReverse = model.NumberOfColoursReverse;
                            check.SpecialInstructions = model.SpecialInstructions;
                            check.Quotation = model.Quotation;
                            check.QuotationDate = Convert.ToDateTime(model.Date);
                            check.Barcode = model.Barcode;
                            check.BarcodeColour = model.BarcodeColour;
                            check.Eyemark = Convert.ToString(model.EyeMark);
                            check.Size = model.Size;
                            check.NumberColours = model.NumberColours;
                            check.Position = model.Position;
                            check.DateProofRequired = Convert.ToDateTime(model.DateProofRequired);
                            check.DateStereosRequired = Convert.ToDateTime(model.DateStereosRequired);
                            check.Taxable = Convert.ToString(model.Taxable);
                            check.ChargeCustomer = Convert.ToString(model.ChargeCustomer);
                            check.ChargeTropic = Convert.ToString(model.ChargeTropic);
                            check.ChangePlate = model.ChangePlate;
                            check.StereoType = model.StereoType;
                            if (model.StereoType == "Customer")
                            {
                                check.PurchaseOrderRequired = "N";
                            }
                            else
                            {
                                check.PurchaseOrderRequired = "Y";
                            }
                            if (model.Approved == "Y")
                            {
                                check.Approved = model.Approved;
                                check.ApprovedBy = User.Identity.Name.ToUpper();
                                check.ApprovedDate = DateTime.Now;
                            }
                            else
                            {
                                check.Approved = "";
                                check.ApprovedBy = "";
                                check.ApprovedDate = null;
                            }
                            hdb.Entry(check).State = EntityState.Modified;
                            hdb.SaveChanges();
                        }
                    }
                }
                var Authorize = (from a in mdb.mtOpFunctions where a.Username == User.Identity.Name.ToUpper() && a.Program == "Stereo" && a.ProgramFunction == "Authorize" select a).ToList();
                if (Authorize.Count > 0)
                {
                    model.Authorize = "Y";
                }
                else
                {
                    model.Authorize = "N";
                }
                model.ReqNo = NewTrackId;
                ViewBag.IsValidTrackId = true;
                ModelState.AddModelError("", "Saved Successfully");
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                if (model.ReqNo == 0)
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

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Detail")]
        public ActionResult Detail(StereoSystemAddStereoViewModel model)
        {
            var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == model.ReqNo select a).FirstOrDefault();
            if (Header != null)
            {
                model.Approved = null;
                var Detail = wdb.sp_GetStereoDetails(model.ReqNo).ToList();
                model.StereoDetails = Detail;
                var supplier = (from a in wdb.mtStereoSuppliers where a.Supplier == model.SupplierReference select a).ToList();
                model.TotalColours = model.NumberColours + model.ChangePlate;
                model.Quantity = 1;
                model.GlCode = supplier.FirstOrDefault().CustomerExpenseGlCode;
                model.Approved = Header.Approved;
                model.TaxCode = Convert.ToChar(supplier.FirstOrDefault().TaxCode.Trim());
                if(Detail.Count > 0 )
                {
                    string PurchaseOrder = Detail.FirstOrDefault().PurchaseOrder;
                    model.TotalSquareM = Detail.Sum(a => a.SquareM).Value;
                    if (!string.IsNullOrEmpty(PurchaseOrder))
                    {
                        var Total = (from b in wdb.mtStereoDetails where b.PurchaseOrder == PurchaseOrder select b).ToList();
                        if (Total != null)
                        {
                            model.TotalPoValue = Total.Sum(a => a.PoPrice).Value;
                        }
                        else
                        {
                            model.TotalPoValue = 0;
                        }
                    }      
                }
                return View(model);
            }
            else
            {
                ViewBag.IsValidTrackId = false;
                ModelState.AddModelError("", "Please enter a valid TrackId");
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveStereoDetail")]
        public ActionResult SaveStereoDetail(StereoSystemAddStereoViewModel model)
        {
           try
            {
                ModelState.Clear();
                var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == model.ReqNo select a).ToList().FirstOrDefault();
                var checkgrn = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.Line == model.Line && a.Grn!="" select a).ToList();
                if(checkgrn.Count == 0)
                {            
                    using (var hdb = new WarehouseManagementEntities(""))
                    {
                        //Check if ReqNo and Line Exists.
                        var Exist = (from a in hdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.Line == model.Line select a).ToList();                     
                        if (Exist.Count == 0)
                        {

                            int LineNo;
                            var CountLine = (from a in hdb.mtStereoDetails where a.ReqNo == model.ReqNo select a).ToList();
                            if (CountLine.Count == 0)
                            {
                                LineNo = 1;
                            }
                            else
                            {
                                var LastLine = (from a in hdb.mtStereoDetails where a.ReqNo == model.ReqNo select a.Line).ToList().Last();
                                LineNo = LastLine + 1;
                            }
                            mtStereoDetail det = new mtStereoDetail();
                            det.ReqNo = model.ReqNo;
                            det.Line = LineNo;
                            det.StockCode = model.Colour.ToUpper() +"-" + Convert.ToString(LineNo);
                            if (!string.IsNullOrEmpty(model.PrintDescription))
                            {
                                det.StockDescription = model.PrintDescription;
                            }
                            else
                            {
                                det.StockDescription = model.Colour.ToUpper();
                            }
                            det.Colour = model.Colour.ToUpper();
                            det.Quantity = model.Quantity;
                            det.UnitPrice = model.UnitPrice;
                            det.PurchaseOrder = "";
                            det.Grn = "";
                            det.ApJournal = "";
                            det.GlCode = model.GlCode;
                            det.TaxCode = Convert.ToString(model.TaxCode).Trim();
                            det.Width = model.Width;
                            det.Length = model.Length;
                            hdb.Entry(det).State = EntityState.Added;
                            hdb.SaveChanges();
                            model.Line = LineNo;
                        }
                        else
                        {
                            //Update required
                            var check = (from a in hdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.Line == model.Line select a).FirstOrDefault();
                            if (check != null)
                            {
                                check.StockCode = model.Colour.ToUpper() + "-" + Convert.ToString(model.Line);
                                if (!string.IsNullOrEmpty(model.PrintDescription))
                                {
                                    check.StockDescription = model.PrintDescription;
                                }
                                else
                                {
                                    check.StockDescription = model.Colour.ToUpper();
                                }
                                check.Colour = model.Colour.ToUpper();
                                check.Quantity = model.Quantity;
                                check.UnitPrice = model.UnitPrice;
                                check.GlCode = model.GlCode;
                                check.TaxCode = Convert.ToString(model.TaxCode);
                                check.Width = model.Width;
                                check.Length = model.Length;
                                hdb.Entry(check).State = EntityState.Modified;
                                hdb.SaveChanges();
                            }
                        }
                    }                   
                    model.Colour = "";
                    model.Line = 0;
                    model.Quantity = 1;
                    model.Width = 0;
                    model.Length = 0;
                    model.UnitPrice = 0;         
                    //UPDATE EXISTING LINES
                    if (!string.IsNullOrEmpty(Header.PurchaseOrder))
                    {
                        ModelState.AddModelError("", BL.PostPurchaseOrder(model));                     
                    }
                    else
                    {
                        ModelState.AddModelError("", "Saved Successfully");
                    }
                    var Detail = wdb.sp_GetStereoDetails(model.ReqNo).ToList();
                    model.StereoDetails = Detail;
                    if (Detail.Count > 0)
                    {
                        model.TotalSquareM = Detail.Sum(a => a.SquareM).Value;
                        string PurchaseOrder = Detail.FirstOrDefault().PurchaseOrder;
                        if (!string.IsNullOrEmpty(PurchaseOrder))
                        {
                            var Total = (from b in wdb.mtStereoDetails where b.PurchaseOrder == PurchaseOrder select b).ToList();
                            if (Total != null)
                            {
                                model.TotalPoValue = Total.Sum(a => a.PoPrice).Value;
                            }
                            else
                            {
                                model.TotalPoValue = 0;
                            }
                        }
                    }
                    return View("Detail", model);

                }
                else
                {
                    ModelState.AddModelError("", "Cannot post, This purchase order line is already receipted Purchase Order: " + checkgrn.FirstOrDefault().PurchaseOrder + " Grn:" + checkgrn.FirstOrDefault().Grn);
                        return View("Detail", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Detail", model);
            }
        }

        public ActionResult EditDetailLine(int ReqNo, int Line)
        {
            try
            {
                ModelState.Clear();
                StereoSystemAddStereoViewModel model = new StereoSystemAddStereoViewModel();
                var Detail = (from a in wdb.mtStereoDetails where a.ReqNo == ReqNo && a.Line == Line select a).ToList();
                var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == ReqNo select a).ToList();
                if(Detail.Count>0)
                {
                    if(string.IsNullOrEmpty(Detail.FirstOrDefault().Grn))
                    {
                        model.Colour = Detail.FirstOrDefault().Colour;
                        model.Quantity = Convert.ToDecimal(Detail.FirstOrDefault().Quantity);
                        model.UnitPrice = Convert.ToDecimal(Detail.FirstOrDefault().UnitPrice);
                        model.Width = Convert.ToDecimal(Detail.FirstOrDefault().Width);
                        model.Length = Convert.ToDecimal(Detail.FirstOrDefault().Length);
                    }
                    else
                    {
                        ModelState.AddModelError("", "This line has already been receipted.You cannot amend it");
                    }
                }
                model.ReqNo = ReqNo;
                model.Line = Detail.FirstOrDefault().Line;
                model.DetStockCode = Detail.FirstOrDefault().StockCode;
                model.StockDescription = Header.FirstOrDefault().DesignReference + "-" + Detail.FirstOrDefault().Colour;
                model.GlCode = Detail.FirstOrDefault().GlCode;
                model.TaxCode = Convert.ToChar(Detail.FirstOrDefault().TaxCode.Trim());
                model.PrintDescription = Header.FirstOrDefault().PrintDescription;
                model.DateStereosRequired = Convert.ToDateTime(Header.FirstOrDefault().DateStereosRequired);
                model.TotalColours = Convert.ToInt32(Header.FirstOrDefault().NumberColours) + Convert.ToInt32(Header.FirstOrDefault().ChangePlate);
                model.Approved = Header.FirstOrDefault().Approved;

                var AllDetailLines = wdb.sp_GetStereoDetails(model.ReqNo).ToList();
                model.StereoDetails = AllDetailLines;
                if (AllDetailLines.Count > 0)
                {
                    model.TotalSquareM = AllDetailLines.Sum(a => a.SquareM).Value;
                    string PurchaseOrder = Detail.FirstOrDefault().PurchaseOrder;
                    if (!string.IsNullOrEmpty(PurchaseOrder))
                    {
                        var Total = (from b in wdb.mtStereoDetails where b.PurchaseOrder == PurchaseOrder select b).ToList();
                        if (Total != null)
                        {
                            model.TotalPoValue = Total.Sum(a => a.PoPrice).Value;
                        }
                        else
                        {
                            model.TotalPoValue = 0;
                        }
                    }
                }
                return View("Detail", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Detail");
            }
        }

        public ActionResult DeleteDetailLine(int ReqNo, int Line)
        {
            try
            {
                ModelState.Clear();
                var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == ReqNo select a).FirstOrDefault();
                StereoSystemAddStereoViewModel model = new StereoSystemAddStereoViewModel();
                model.ReqNo = Header.ReqNo;
                model.DesignReference = Header.DesignReference;
                model.PrintDescription = Header.PrintDescription;
                model.DateStereosRequired = Convert.ToDateTime(Header.DateStereosRequired);
                model.NumberColours = Convert.ToInt32(Header.NumberColours);
                var supplier = (from a in wdb.mtStereoSuppliers where a.Supplier == Header.SupplierReference select a).ToList();
                model.GlCode = supplier.FirstOrDefault().CustomerExpenseGlCode;
                model.Approved = Header.Approved;
                model.TaxCode = Convert.ToChar(supplier.FirstOrDefault().TaxCode.Trim());

                var checkgrn = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.Line == Line && a.Grn!="" select a).ToList();
                if (checkgrn.Count == 0)
                {
                    ModelState.AddModelError("", BL.DeleteLine(ReqNo, Line));
                }
                else
                {
                    ModelState.AddModelError("", "Cannot delete, This line has already been receipted.");
                }


                var AllDetailLines = wdb.sp_GetStereoDetails(ReqNo).ToList();
                model.StereoDetails = AllDetailLines;

                return View("Detail", model);
            }
            catch (Exception ex)
            {
                var Header = (from a in wdb.mtStereoHdrs where a.ReqNo == ReqNo select a).FirstOrDefault();
                StereoSystemAddStereoViewModel model = new StereoSystemAddStereoViewModel();
                model.ReqNo = Header.ReqNo;
                model.DesignReference = Header.DesignReference;
                model.DateStereosRequired = Convert.ToDateTime(Header.DateStereosRequired);
                model.NumberColours = Convert.ToInt32(Header.NumberColours);
                var supplier = (from a in wdb.mtStereoSuppliers where a.Supplier == Header.SupplierReference select a).ToList();
                model.GlCode = supplier.FirstOrDefault().CustomerExpenseGlCode;
                model.Approved = Header.Approved;
                model.TaxCode = Convert.ToChar(supplier.FirstOrDefault().TaxCode.Trim());
                var AllDetailLines = wdb.sp_GetStereoDetails(ReqNo).ToList();
                model.StereoDetails = AllDetailLines;
                ModelState.AddModelError("", ex.Message);
                return View("Detail", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "CreatePurchaseOrder")]
        public ActionResult PostPurchaseOrder(StereoSystemAddStereoViewModel model)
        {
            try
            {
                var Detail = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo select a).ToList();
                var CountGrn = (from a in wdb.mtStereoDetails where a.ReqNo == model.ReqNo && a.Grn !="" select a).ToList();
                if (Detail.Count == CountGrn.Count)
                {
                    ViewBag.IsValidTrackId = true;
                    ModelState.AddModelError("", "Cannot post, this purchase order has been fully receipted.");
                    return View("Index", model);
                }
                else
                {
                    ModelState.AddModelError("", BL.PostPurchaseOrder(model));
                    ViewBag.IsValidTrackId = true;
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.IsValidTrackId = true;
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        public ActionResult CustomerSearch()
        {
            return PartialView();
        }

        public ActionResult CustomerList(string FilterText)
        {
            var result = wdb.sp_GetStereoCustomers(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult StockCodeSearch()
        {
            return PartialView();
        }
        public ActionResult StockCodeQuerySearch()
        {
            return PartialView();
        }
        public ActionResult StockCodeList(string FilterText)
        {
            var result = wdb.sp_GetStereoStockCodes(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SupplierSearch()
        {
            return PartialView();
        }

        public ActionResult SupplierList(string FilterText)
        {
            var result = wdb.sp_GetStereoSupplier(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PlateCategorySearch()
        {
            return PartialView();
        }

        public ActionResult PlateCategoryList(string FilterText)
        {
            var result = wdb.sp_GetStereoPlateCategory(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize(Activity: "StereoQuery")]
        public ActionResult StereoQuery()
        {
            StereoSystemAddStereoViewModel model = new StereoSystemAddStereoViewModel();
            return View(model);
        }
        public ActionResult RequisitionSearch()
        {         
            return PartialView();
        }
        public ActionResult RequisitionList()
        {
            var result = (from a in wdb.mtStereoHdrs select new { a.ReqNo, a.PrintDescription, a.SupplierReference, a.Customer, a.NumberColours, a.ChangePlate, a.PlateCategory }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SearchDecription")]
        public ActionResult SearchDecription(StereoSystemAddStereoViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (!string.IsNullOrEmpty(model.PrintDescription))
                {
                    string Description = model.PrintDescription.Trim().ToString().ToUpper();
                    var list = wdb.mtStereoDetails.Where(h => h.StockDescription.Contains("" + Description + "")).ToList();
                    if (list.Count > 0)
                    {
                        model.DetailList = list;
                    }
                    else
                    {
                        model.DetailList = null;
                        ModelState.AddModelError("", "No data found.");
                    }
                }
                else
                {
                    model.DetailList = null;
                    ModelState.AddModelError("", "Please enter a Description");
                }
                return View("StereoQuery", model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("StereoQuery", model);
            }
           
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SearchStockCode")]
        public ActionResult SearchStockCode(StereoSystemAddStereoViewModel model)
        {

            try
            {
                ModelState.Clear();
                if(!string.IsNullOrEmpty(model.StockCode))
                {
                    string StockCode = model.StockCode.ToString().ToUpper();
                    var Header = wdb.mtStereoHdrs.Where(h => h.StockCode.Contains(StockCode)).Select(a => a.ReqNo).ToList();
                    if (Header.Count > 0)
                    {
                        model.DetailList = wdb.mtStereoDetails.Where(h => Header.Contains(h.ReqNo)).ToList();
                    }
                    else
                    {
                        model.DetailList = null;
                        ModelState.AddModelError("", "No data found");
                    }
                }
                else
                {
                    model.DetailList = null;
                    ModelState.AddModelError("", "Please enter a Stock Code.");
                }
                
                
                return View("StereoQuery", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("StereoQuery", model);
            }
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SearchReqno")]
        public ActionResult SearchReqNo(StereoSystemAddStereoViewModel model)
        {

            try
            {
                ModelState.Clear();
                int ReqNo = model.ReqNo;          
                var data =  wdb.mtStereoDetails.Where(h => h.ReqNo == ReqNo).ToList();
                mtStereoHdr st = new mtStereoHdr();
                st = wdb.mtStereoHdrs.Find(ReqNo);
                if (data.Count > 0)
                {
                    model.DetailList = data;
                    model.hdr = st;
                }
                else
                {
                    model.DetailList = null;
                    ModelState.AddModelError("", "No data found.");
                }
                

                return View("StereoQuery", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("StereoQuery", model);
            }
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SearchSupplier")]
        public ActionResult SearchSupplier(StereoSystemAddStereoViewModel model)
        {

            try
            {
                ModelState.Clear();
                if (!string.IsNullOrEmpty(model.SupplierReference))
                {
                    string Supplier = model.SupplierReference.ToString();
                    var Header = wdb.mtStereoHdrs.Where(h => h.SupplierReference.Contains(Supplier)).Select(a => a.ReqNo).ToList();
                    if (Header.Count > 0)
                    {
                        model.DetailList = wdb.mtStereoDetails.Where(h => Header.Contains(h.ReqNo)).ToList();
                    }
                    else
                    {
                        model.DetailList = null;
                        ModelState.AddModelError("", "No data found");
                    }
                }
                else
                {
                    model.DetailList = null;
                    ModelState.AddModelError("", "Please enter a supplier");
                }

                return View("StereoQuery", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("StereoQuery", model);
            }
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SearchCustomer")]
        public ActionResult SearchCustomer(StereoSystemAddStereoViewModel model)
        {

            try
            {
                ModelState.Clear();
                if (!string.IsNullOrEmpty(model.Customer))
                {
                    string Customer = model.Customer.ToString();
                    var Header = wdb.mtStereoHdrs.Where(h => h.Customer.Contains(Customer)).Select(a => a.ReqNo).ToList();
                    if (Header.Count > 0)
                    {
                        model.DetailList = wdb.mtStereoDetails.Where(h => Header.Contains(h.ReqNo)).ToList();
                    }
                    else
                    {
                        model.DetailList = null;
                        ModelState.AddModelError("", "No data found");
                    }
                }
                else
                {
                    model.DetailList = null;
                    ModelState.AddModelError("", "Please enter a customer");
                }

                return View("StereoQuery", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("StereoQuery", model);
            }
        }
        [CustomAuthorize(Activity: "StereoQueryDuplicate")]
        public ActionResult DuplicateStereo(int ReqNo)
        {
            try
            {
                StereoSystemAddStereoViewModel model = new StereoSystemAddStereoViewModel();
                
                ModelState.Clear();
                var NewTrackId = 0;
                var TrackId = (from a in wdb.mtStereoHdrs where a.ReqNo == ReqNo select a).ToList().FirstOrDefault();
              
                if (TrackId != null)
                {
                        using (var hdb = new WarehouseManagementEntities(""))
                    {
                   
                        //ADD NEW
                        mtStereoHdr hdr = new mtStereoHdr();
                        hdr.Customer = TrackId.Customer;
                        hdr.EntryDate = DateTime.Now;
                        hdr.CustomerInvoice = "";
                        hdr.Invoice = "";
                        hdr.StockCode = TrackId.StockCode;
                        hdr.SalesOrder = "";
                        hdr.PlateCategory = TrackId.PlateCategory;
                        hdr.SupplierReference = TrackId.SupplierReference;
                        hdr.DesignReference = TrackId.DesignReference;
                        hdr.PrintDescription = TrackId.PrintDescription;
                        hdr.BagSize = TrackId.BagSize;
                        hdr.CylSlvSize = TrackId.CylSlvSize;
                        hdr.Surface = TrackId.Surface;
                        hdr.NumberAcross = TrackId.NumberAcross;
                        hdr.NumberAround = TrackId.NumberAround;
                        hdr.NumberSetsRequired = TrackId.NumberSetsRequired;
                        hdr.MaterialType = TrackId.MaterialType;
                        hdr.Thickness = TrackId.Thickness;
                        hdr.NumberOfColoursFront = TrackId.NumberOfColoursFront;
                        hdr.NumberOfColoursReverse = TrackId.NumberOfColoursReverse;
                        hdr.SpecialInstructions = TrackId.SpecialInstructions;
                        hdr.Quotation = "";
                        hdr.Barcode = TrackId.Barcode;
                        hdr.BarcodeColour = TrackId.BarcodeColour;
                        hdr.Eyemark = Convert.ToString(TrackId.Eyemark);
                        hdr.Size = TrackId.Size;
                        hdr.NumberColours = TrackId.NumberColours;
                        hdr.Position = TrackId.Position;
                        hdr.Taxable = Convert.ToString(TrackId.Taxable);
                        hdr.ChargeCustomer = Convert.ToString(TrackId.ChargeCustomer);
                        hdr.ChargeTropic = Convert.ToString(TrackId.ChargeTropic);
                        hdr.ChangePlate = TrackId.ChangePlate;
                        hdr.StereoType = TrackId.StereoType;
                        hdr.PoCreated = "N";
                        hdr.PurchaseOrderRequired = TrackId.PurchaseOrderRequired;

                        if (TrackId.Approved == "Y")
                        {
                            hdr.Approved = TrackId.Approved;
                            hdr.ApprovedBy = User.Identity.Name.ToUpper();
                            hdr.ApprovedDate = DateTime.Now;
                        }
                        hdb.Entry(hdr).State = EntityState.Added;
                        hdb.SaveChanges();

                        NewTrackId = hdr.ReqNo;
                    
                    var Detail = (from a in hdb.mtStereoDetails where a.ReqNo == ReqNo select a).ToList();
                    foreach(var item in Detail)
                    {
                        mtStereoDetail det = new mtStereoDetail();
                        det.ReqNo = NewTrackId;
                        det.Line = item.Line;
                        det.Colour = item.Colour;
                        det.StockCode = item.StockCode;
                        det.StockDescription = item.StockDescription;
                        det.Quantity = item.Quantity;
                        det.UnitPrice = item.UnitPrice;
                        det.PurchaseOrder = "";
                        det.Grn = "";
                        det.ApJournal = "";
                        det.GlCode = item.GlCode;
                        det.TaxCode = Convert.ToString(item.TaxCode).Trim();
                        det.Width = item.Width;
                        det.Length = item.Length;
                        hdb.Entry(det).State = EntityState.Added;
                        hdb.SaveChanges();
                    }  
                }
                    model.Customer = TrackId.Customer;
                    model.CustomerInvoice = 'N';
                    model.Invoice = "";
                    model.StockCode = TrackId.StockCode;
                    model.SalesOrder = "";
                    model.PlateCategory = TrackId.PlateCategory;
                    model.SupplierReference = TrackId.SupplierReference;
                    model.DesignReference = TrackId.DesignReference;
                    model.PrintDescription = TrackId.PrintDescription;
                    model.BagSize = (decimal)TrackId.BagSize;
                    model.CylSlvSize = (decimal)TrackId.CylSlvSize;
                    model.Surface = TrackId.Surface;
                    model.NumberAcross = (decimal)TrackId.NumberAcross;
                    model.NumberAround = (decimal)TrackId.NumberAround;
                    model.NumberSetsRequired = (decimal)TrackId.NumberSetsRequired;
                    model.MaterialType = TrackId.MaterialType;
                    model.Thickness = (decimal)TrackId.Thickness;
                    model.NumberOfColoursFront = (decimal)TrackId.NumberOfColoursFront;
                    model.NumberOfColoursReverse = (decimal)TrackId.NumberOfColoursReverse;
                    model.SpecialInstructions = TrackId.SpecialInstructions;
                    model.Quotation = "";
                    //hdr.QuotationDate = ;
                    model.Barcode = TrackId.Barcode;
                    model.BarcodeColour = TrackId.BarcodeColour;
                    model.EyeMark = Convert.ToChar(TrackId.Eyemark);
                    model.Size = (decimal)TrackId.Size;
                    model.NumberColours =(int) TrackId.NumberColours;
                    model.Position = TrackId.Position;
                    //hdr.DateProofRequired = Convert.ToDateTime(model.DateProofRequired);
                    //hdr.DateStereosRequired = Convert.ToDateTime(model.DateStereosRequired);
                    model.Taxable = Convert.ToChar(TrackId.Taxable);
                    model.ChargeCustomer = Convert.ToChar(TrackId.ChargeCustomer);
                    model.ChargeTropic = Convert.ToChar(TrackId.ChargeTropic);
                    model.ChangePlate = (int)TrackId.ChangePlate;
                    model.StereoType = TrackId.StereoType;
                    ViewBag.IsValidTrackId = true;
                    model.ReqNo = NewTrackId;
                return View("Index",model);
             }
            else 
             {
                    return View("StereoQuery", model);

             }
        }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex);
                return View();
            }
        }

        [HttpPost]
        [CustomAuthorize(Activity: "CreateVariant")]
        [MultipleButton(Name = "action", Argument = "CreateVariant")]
        public ActionResult CreateVariant(StereoSystemAddStereoViewModel model)
        {
            try
            {
                ModelState.Clear();
                var NewTrackId = 0;
                var TrackId = (from a in wdb.mtStereoHdrs where a.ReqNo == model.ReqNo select a).ToList().FirstOrDefault();

                if (TrackId != null)
                {
                    if(TrackId.PoCreated =="Y")
                    {
                        using (var hdb = new WarehouseManagementEntities(""))
                        {

                            //ADD NEW
                            mtStereoHdr hdr = new mtStereoHdr();
                            hdr.Customer = model.Customer;
                            hdr.EntryDate = DateTime.Now;
                            hdr.CustomerInvoice = Convert.ToString(model.CustomerInvoice);
                            hdr.Invoice = model.Invoice ;
                            hdr.StockCode ="";
                            hdr.SalesOrder =model.SalesOrder;
                            hdr.PlateCategory = model.PlateCategory;
                            hdr.SupplierReference = model.SupplierReference;
                            hdr.DesignReference = model.DesignReference;
                            hdr.PrintDescription = "";
                            hdr.BagSize = model.BagSize;
                            hdr.CylSlvSize = model.CylSlvSize;
                            hdr.Surface = model.Surface;
                            hdr.NumberAcross = model.NumberAcross;
                            hdr.NumberAround = model.NumberAround;
                            hdr.NumberSetsRequired = model.NumberSetsRequired;
                            hdr.MaterialType = model.MaterialType;
                            hdr.Thickness = model.Thickness;
                            hdr.NumberOfColoursFront = model.NumberOfColoursFront;
                            hdr.NumberOfColoursReverse = model.NumberOfColoursReverse;
                            hdr.SpecialInstructions = model.SpecialInstructions;
                            hdr.Quotation = model.Quotation;
                            hdr.QuotationDate = model.Date;
                            hdr.DateProofRequired = Convert.ToDateTime(model.DateProofRequired);
                            hdr.DateStereosRequired = Convert.ToDateTime(model.DateStereosRequired);
                            hdr.Barcode = model.Barcode;
                            hdr.BarcodeColour = model.BarcodeColour;
                            hdr.Eyemark = Convert.ToString(model.EyeMark);
                            hdr.Size = model.Size;
                            //hdr.NumberColours = model.NumberColours;
                            hdr.Position = model.Position;
                            hdr.Taxable = Convert.ToString(model.Taxable);
                            hdr.ChargeCustomer = Convert.ToString(model.ChargeCustomer);
                            hdr.ChargeTropic = Convert.ToString(model.ChargeTropic);
                            hdr.ChangePlate = model.ChangePlate;
                            hdr.StereoType = model.StereoType;
                            hdr.PoCreated = "Y";                     
                            hdr.PurchaseOrderRequired = TrackId.PurchaseOrderRequired;
                            hdr.PurchaseOrder = TrackId.PurchaseOrder;
                            hdr.Variant = "Y";
                            if (TrackId.Approved == "Y")
                            {
                                hdr.Approved = TrackId.Approved;
                                hdr.ApprovedBy = User.Identity.Name.ToUpper();
                                hdr.ApprovedDate = DateTime.Now;
                            }
                            hdb.Entry(hdr).State = EntityState.Added;
                            hdb.SaveChanges();
                            NewTrackId = hdr.ReqNo;

                            //UPDATE EXISTING REQNO TO VARIANT
                            mtStereoHdr h = new mtStereoHdr();
                            h = hdb.mtStereoHdrs.Find(model.ReqNo);
                            h.Variant = "Y";
                            hdb.Entry(h).State = EntityState.Modified;
                            hdb.SaveChanges();
                        }
                       
                        model.PrintDescription = "";                       
                        ViewBag.IsValidTrackId = true;
                        model.ReqNo = NewTrackId;
                        return View("Index", model);
                    }
                    else
                    {
                        ViewBag.IsValidTrackId = true;
                        ModelState.AddModelError("", "Purchase Order isnt created. Please create purchase order to continue.");
                        return View("Index", model);
                    }
                    
                }
                else
                {
                    ViewBag.IsValidTrackId = true;
                    ModelState.AddModelError("", "Requisition not found.");
                    return View("Index", model);

                }
            }
            catch (Exception ex)
            {
                ViewBag.IsValidTrackId = true;
                ModelState.AddModelError("", ex);
                return View();
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveStockCode")]
        public ActionResult SaveStockCode(StereoSystemAddStereoViewModel model)
        {

            try
            {
                ModelState.Clear();
                if (model.hdr != null)
                {
                    mtStereoHdr st = new mtStereoHdr();
                    st = wdb.mtStereoHdrs.Find(model.hdr.ReqNo);
                    if(st != null)
                    {
                        st.StockCode = model.hdr.StockCode;
                        wdb.Entry(st).State = EntityState.Modified;
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Stock Code saved successfully");
                    }
                }
                else
                {
                    model.DetailList = null;
                    ModelState.AddModelError("", "Please select a ReqNo");
                }

                return View("StereoQuery", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("StereoQuery", model);
            }
        }

        public ActionResult LoadStereoHeader(int ReqNo)
        {
            StereoSystemAddStereoViewModel model = new StereoSystemAddStereoViewModel();
            try
            {
                    ModelState.Clear();
                    var data = wdb.mtStereoDetails.Where(h => h.ReqNo == ReqNo).ToList();
                    mtStereoHdr st = new mtStereoHdr();
                    st = wdb.mtStereoHdrs.Find(ReqNo);
                    if (st != null)
                    {                
                        model.hdr = st;
                    }  
                    if (data != null)
                    {
                        model.DetailList = data;
                    }

                return View("StereoQuery",model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("StereoQuery",model);
            }
        }
    }
}