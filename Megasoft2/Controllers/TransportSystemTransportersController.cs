using Megasoft2.ViewModel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransportSystemTransportersController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        //
        // GET: /TransportSystemTransporters/
        [CustomAuthorize(Activity: "Transporters")]
        public ActionResult TransporterIndex()
        {
            return View(wdb.mtTransporters.AsEnumerable());
        }

        //
        // GET: /TransportSystemTransporters/Create
        [CustomAuthorize(Activity: "Transporters")]
        public ActionResult Create(string Transporter, string VehicleReg)
        {
            try
            {
                if (VehicleReg == null)
                {
                    TransportSystemTransportersViewModel trans = new TransportSystemTransportersViewModel();
                    return View(trans);
                }
                else
                {
                    mtTransporter mtTransporters = new mtTransporter();
                    if (mtTransporters == null)
                    {
                        return HttpNotFound();
                    }
                    TransportSystemTransportersViewModel trans = new TransportSystemTransportersViewModel();
                    trans.Transport = wdb.mtTransporters.Find(Transporter, VehicleReg);

                    if(trans.Transport.Taxable == "Y")
                    {
                        trans.Taxable = true;
                    }
                    else
                    {
                        trans.Taxable = false;
                    }

                    return View(trans);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        // POST: /Transporter/Create
        [CustomAuthorize(Activity: "Transporters")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TransportSystemTransportersViewModel TransModel)
        {
            try
            {
                ModelState.Clear();
                if (ModelState.IsValid)
                {
                    var checkUser = (from a in wdb.mtTransporters.AsEnumerable() where a.Transporter == TransModel.Transport.Transporter && a.VehicleReg == TransModel.Transport.VehicleReg select a).ToList();
                    if (checkUser.Count > 0)
                    {

                        string Taxable = "";
                        if (TransModel.Taxable == true)
                        {
                            Taxable = "Y";
                            if (string.IsNullOrEmpty(TransModel.Transport.TaxCode))
                            {
                                ModelState.AddModelError("", "Please select a Tax Code");
                                return View(TransModel);
                            }
                        }
                        else
                        {
                            Taxable = "N";
                            TransModel.Transport.TaxCode = "E";
                        }

                        TransModel.Transport.Taxable = Taxable;

                        var v = wdb.mtTransporters.Find(TransModel.Transport.Transporter, TransModel.Transport.VehicleReg);
                        wdb.Entry(v).CurrentValues.SetValues(TransModel.Transport);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Updated Successfully.");
                        return View(TransModel);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TransModel.Transport.Transporter))
                        {
                            ModelState.AddModelError("", "Please select a Transporter");
                            return View(TransModel);
                        }
                        if (string.IsNullOrEmpty(TransModel.Transport.VehicleReg))
                        {
                            ModelState.AddModelError("", "Please enter a vehicle registration number");
                            return View(TransModel);
                        }
                        if (string.IsNullOrEmpty(TransModel.Transport.Driver))
                        {
                            ModelState.AddModelError("", "Please enter driver name");
                            return View(TransModel);
                        }
                        if (string.IsNullOrEmpty(TransModel.Transport.GLCode))
                        {
                            ModelState.AddModelError("", "Please select a GL Code");
                            return View(TransModel);
                        }

                        string Taxable = "";
                        if(TransModel.Taxable == true)
                        {
                            Taxable = "Y";
                            if (string.IsNullOrEmpty(TransModel.Transport.TaxCode))
                            {
                                ModelState.AddModelError("", "Please select a Tax Code");
                                return View(TransModel);
                            }
                        }
                        else
                        {
                            Taxable = "N";
                            TransModel.Transport.TaxCode = "E";
                        }







                        wdb.mtTransporters.Add(new mtTransporter
                        {
                            Transporter = TransModel.Transport.Transporter,
                            VehicleReg = TransModel.Transport.VehicleReg,
                            LinkReg1 = TransModel.Transport.LinkReg1,
                            LinkReg2 = TransModel.Transport.LinkReg2,
                            LinkReg3 = TransModel.Transport.LinkReg3,
                            Driver = TransModel.Transport.Driver,
                            GLCode = TransModel.Transport.GLCode,
                            VehicleCapacity = TransModel.Transport.VehicleCapacity,
                            Taxable = Taxable,
                            TaxCode = TransModel.Transport.TaxCode
                        });
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Saved Successfully.");
                        return View(TransModel);
                    }
                }

                return View(TransModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View(TransModel);
            }
        }

        public ActionResult SupplierSearch()
        {
            return PartialView();
        }

        public JsonResult SupplierList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            var result = wdb.sp_GetTransSuppliers(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GlCodeSearch()
        {
            return PartialView();
        }

        public ActionResult GlCodeList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }

            var result = wdb.sp_GetTransGlCodes(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string Transporter, string VehicleReg)
        {
            mtTransporter mtTransporters = wdb.mtTransporters.Find(Transporter, VehicleReg);
            if (mtTransporters == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtTransporters);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Transporter, string VehicleReg)
        {
            mtTransporter mtTransporters = wdb.mtTransporters.Find(Transporter, VehicleReg);
            wdb.mtTransporters.Remove(mtTransporters);
            wdb.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /Transport/Index/5
        [CustomAuthorize(Activity: "TransporterRates")]
        public ActionResult RateIndex()
        {
            return View(wdb.mtTransporterRates.ToList());
        }

        public ActionResult AddRate(string Transporter, string RateCode)
        {
            try
            {
                if (RateCode == null)
                {
                    TransportSystemTransportersViewModel trans = new TransportSystemTransportersViewModel();
                    return View(trans);
                }
                else
                {
                    mtTransporterRate mtTransporterRates = new mtTransporterRate();
                    if (mtTransporterRates == null)
                    {
                        return HttpNotFound();
                    }
                    TransportSystemTransportersViewModel trans = new TransportSystemTransportersViewModel();
                    trans.Rates = wdb.mtTransporterRates.Find(Transporter, RateCode);

                    return View(trans);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRate(TransportSystemTransportersViewModel Rate)
        {
            try
            {
                ModelState.Clear();
                if (ModelState.IsValid)
                {
                    var checkRate = (from a in wdb.mtTransporterRates.AsEnumerable() where a.Transporter == Rate.Rates.Transporter && a.RateCode == Rate.Rates.RateCode select a).ToList();
                    if (checkRate.Count > 0)
                    {
                        var v = wdb.mtTransporters.Find(Rate.Rates.Transporter, Rate.Rates.RateCode);
                        wdb.Entry(v).CurrentValues.SetValues(Rate.Rates);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Updated Successfully.");
                        return View(Rate);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Rate.Rates.Transporter))
                        {
                            ModelState.AddModelError("", "Please select a Transporter");
                            return View(Rate);
                        }
                        if (string.IsNullOrEmpty(Rate.Rates.RateCode))
                        {
                            ModelState.AddModelError("", "Please select a Rate Code");
                            return View(Rate);
                        }
                        if (string.IsNullOrEmpty((Convert.ToString(Rate.Rates.Rate))))
                        {
                            ModelState.AddModelError("", "Please enter rate amount");
                            return View(Rate);
                        }
                        wdb.mtTransporterRates.Add(new mtTransporterRate
                        {
                            Transporter = Rate.Rates.Transporter,
                            RateCode = Rate.Rates.RateCode,
                            Rate = Rate.Rates.Rate
                        });
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Saved Successfully.");
                        return View(Rate);
                    }
                }

                return View(Rate);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View(Rate);
            }
        }


        [CustomAuthorize(Activity: "TransporterRateCodes")]
        public ActionResult RateCodeIndex()
        {
            return View(wdb.mtTransporterRateCodes.ToList());
        }

        //GET
        public ActionResult CreateRateCode(string RateCode)
        {
            try
            {
                if (RateCode == null)
                {
                    TransportSystemTransportersViewModel tst = new TransportSystemTransportersViewModel();
                    return View(tst);
                }
                else
                {
                    mtTransporterRateCode rates = new mtTransporterRateCode();
                    if (rates == null)
                    {
                        return HttpNotFound();
                    }
                    TransportSystemTransportersViewModel tst = new TransportSystemTransportersViewModel();
                    tst.RateCodes = wdb.mtTransporterRateCodes.Find(RateCode);

                    return View(tst);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRateCode(TransportSystemTransportersViewModel TransModel)
        {
            try
            {
                ModelState.Clear();
                if (ModelState.IsValid)
                {
                    var checkRateCode = (from a in wdb.mtTransporterRateCodes.AsEnumerable() where a.RateCode == TransModel.RateCodes.RateCode select a).ToList();
                    if (checkRateCode.Count > 0)
                    {
                        var v = wdb.mtTransporterRateCodes.Find(TransModel.RateCodes.RateCode);
                        wdb.Entry(v).CurrentValues.SetValues(TransModel.RateCodes);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Updated Successfully.");
                        return View(TransModel);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TransModel.RateCodes.RateCode))
                        {
                            ModelState.AddModelError("", "Please enter a Rate");
                            return View(TransModel);
                        }
                        if (string.IsNullOrEmpty(TransModel.RateCodes.Description))
                        {
                            ModelState.AddModelError("", "Please enter a Description for Rate Code");
                            return View(TransModel);
                        }

                        wdb.mtTransporterRateCodes.Add(new mtTransporterRateCode
                        {
                            RateCode = TransModel.RateCodes.RateCode,
                            Description = TransModel.RateCodes.Description
                        });
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Saved Successfully.");
                        return View(TransModel);
                    }
                }

                return View(TransModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.InnerException.Message);
                return View(TransModel);
            }
        }
      

        public ActionResult DeleteRateCode(string RateCode)
        {
            mtTransporterRateCode mtTransporterRateCodes = wdb.mtTransporterRateCodes.Find(RateCode);
            if (mtTransporterRateCodes == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtTransporterRateCodes);
        }

        
        [HttpPost, ActionName("DeleteRateCode")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRateCodeConfirmed(string RateCode)
        {
            mtTransporterRateCode mtTransporterRateCodes = wdb.mtTransporterRateCodes.Find(RateCode);
            wdb.mtTransporterRateCodes.Remove(mtTransporterRateCodes);
            wdb.SaveChanges();
            return RedirectToAction("RateCodeIndex");

        }

        public ActionResult RateCodeSearch()
        {
            return PartialView();
        }

        public JsonResult RateCodeList(string FilterText)
        {
            if (FilterText == "")
            {
                FilterText = "NULL";
            }
            var result = wdb.sp_GetTransRateCodes(FilterText.ToUpper());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteRate(string Transporter, string RateCode)
        {
            mtTransporterRate mtTransportersRate = wdb.mtTransporterRates.Find(Transporter, RateCode);
            if (mtTransportersRate == null)
            {
                return HttpNotFound();
            }
            return PartialView(mtTransportersRate);
        }

        
        [HttpPost, ActionName("DeleteRate")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRateConfirmed(string Transporter, string RateCode)
        {
            mtTransporterRate mtTransportersRate = wdb.mtTransporterRates.Find(Transporter, RateCode);
            wdb.mtTransporterRates.Remove(mtTransportersRate);
            wdb.SaveChanges();
            return RedirectToAction("RateIndex");
        }

        
        public ActionResult TaxCode()
        {
            return PartialView();
        }

        

        public ActionResult TaxCodeList()
        {

            var result = (from a in wdb.AdmTaxes select new { TaxCode = a.TaxCode, Description = a.Description }).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}