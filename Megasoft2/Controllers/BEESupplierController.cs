using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class BEESupplierController : Controller
    {
        //
        // GET: /BEESupplier/
        SysproEntities sdb = new SysproEntities("");
        BeeSupplierBL objBee = new BeeSupplierBL();
        MegasoftEntities mdb = new MegasoftEntities();
        Email objEmail = new Email();

        [CustomAuthorize(Activity: "BeeSupplier")]
        public ActionResult Index()
        {
            try
            {
                string username = HttpContext.User.Identity.Name.ToUpper();
                var pref = (from a in sdb.mtBeePreferences where a.Username == username select a).FirstOrDefault();
                if(pref == null)
                {
                    return View();
                }
                else
                {
                    var supplierList = sdb.sp_GetBeeSuppliers(pref.FilterText).ToList();

                    if (!string.IsNullOrEmpty(pref.ExpiryDate))
                    {
                        DateTime ExpDate = Convert.ToDateTime(pref.ExpiryDate);
                        supplierList = (from a in supplierList where a.ExpDate == null || a.ExpDate <= ExpDate select a).ToList();
                    }

                    if (pref.PurchaseValue != 0 && pref.PurchaseValue != null)
                    {
                        supplierList = (from a in supplierList where a.PurchaseValue >= pref.PurchaseValue select a).ToList();
                    }

                    BeeSupplier model = new BeeSupplier();
                    model.PurchaseValue = pref.PurchaseValue;
                    model.ExpiryDate = pref.ExpiryDate;
                    model.FilterText = pref.FilterText;
                    model.Detail = supplierList;
                    return View(model);
                }
                
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }                       
        }

        [CustomAuthorize(Activity: "BeeSupplier")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Index")]
        public ActionResult Index(BeeSupplier model)
        {
            try
            {
                ModelState.Clear();
                if (model.FilterText == null)
                {
                    model.FilterText = "";
                }
                var supplierList = sdb.sp_GetBeeSuppliers(model.FilterText).ToList();



                if (!string.IsNullOrEmpty(model.ExpiryDate))
                {
                    DateTime ExpDate = Convert.ToDateTime(model.ExpiryDate);
                    supplierList = (from a in supplierList where a.ExpDate == null || a.ExpDate <= ExpDate select a).ToList();
                }


                if (model.PurchaseValue != 0 && model.PurchaseValue != null)
                {
                    supplierList = (from a in supplierList where a.PurchaseValue >= model.PurchaseValue select a).ToList();
                }

                BeeSupplier modelOut = new BeeSupplier();
                modelOut.ExpiryDate = model.ExpiryDate;
                modelOut.FilterText = model.FilterText;
                modelOut.PurchaseValue = model.PurchaseValue;
                modelOut.Detail = supplierList;         
                string username = HttpContext.User.Identity.Name.ToUpper();
     
                using(var pdb = new SysproEntities(""))
                {
                    var result = (from a in pdb.mtBeePreferences where a.Username == username select a).FirstOrDefault();
                    if(result != null)
                    {
                        result.ExpiryDate = modelOut.ExpiryDate;
                        result.FilterText = modelOut.FilterText;
                        result.PurchaseValue = modelOut.PurchaseValue;
                        pdb.Entry(result).State = System.Data.EntityState.Modified;
                        pdb.SaveChanges();
                    }
                    else
                    {
                        mtBeePreference pref = new mtBeePreference();
                        pref.Username = username;
                        pref.FilterText = modelOut.FilterText;
                        pref.ExpiryDate = modelOut.ExpiryDate;
                        pref.PurchaseValue = modelOut.PurchaseValue;
                        pdb.mtBeePreferences.Add(pref);
                        pdb.SaveChanges();
                    }
                }

                return View(modelOut);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            
        }

        [CustomAuthorize(Activity: "BeeSupplier")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveForm")]
        public ActionResult SaveForm(BeeSupplier model)
        {
            try
            {
                ModelState.Clear();
                if(model != null)
                {
                    if (model.Detail.Count > 0)
                    {


                        //For Audit Trail
                        if (model.FilterText == null)
                        {
                            model.FilterText = "";
                        }
                        List<sp_GetBeeSuppliers_Result> supplierList;
                        using (var adb = new SysproEntities(""))
                        {
                            supplierList = sdb.sp_GetBeeSuppliers(model.FilterText).ToList();

                        }

                        if (!string.IsNullOrEmpty(model.ExpiryDate))
                        {
                            DateTime ExpDate = Convert.ToDateTime(model.ExpiryDate);
                            supplierList = (from a in supplierList where a.ExpDate == null || a.ExpDate <= ExpDate select a).ToList();
                        }

                        if (model.PurchaseValue != 0 && model.PurchaseValue != null)
                        {
                            supplierList = (from a in supplierList where a.PurchaseValue >= model.PurchaseValue select a).ToList();
                        }

                        List<BeeSupplierClass> newValue = new List<BeeSupplierClass>();


                        foreach(var item in model.Detail)
                        {
                                
                                var check = (from a in sdb.ApSupplier_ where a.Supplier == item.Supplier select a).ToList();
                                if(check.Count > 0)
                                {
                                    using (var udb = new SysproEntities(""))
                                    {
                                        //update custom form
                                        var result = (from a in udb.ApSupplier_ where a.Supplier == item.Supplier select a).FirstOrDefault();
                                        
                                        result.EnterpriseType = item.EnterpriseType;
                                        result.BeeLevel = item.BeeLevel;
                                        result.BlackOwnership = item.BlackOwnership;
                                        result.BlackWomenOwnershi = item.BlackWomenOwnershi;
                                        result.EmpoweringSupplier = item.EmpoweringSupplier;
                                        if(item.ExpiryDate != null)
                                        {
                                            result.ExpiryDate = Convert.ToDateTime(item.ExpiryDate);
                                        }                                        
                                        udb.Entry(result).State = System.Data.EntityState.Modified;
                                        udb.SaveChanges();
                                    }
                                }
                                else
                                {
                                    using (var idb = new SysproEntities(""))
                                    {
                                        //insert custom form
                                        ApSupplier_ ap = new ApSupplier_();
                                        ap.Supplier = item.Supplier;
                                        ap.EnterpriseType = item.EnterpriseType;
                                        ap.BeeLevel = item.BeeLevel;
                                        ap.BlackOwnership = item.BlackOwnership;
                                        ap.BlackWomenOwnershi = item.BlackWomenOwnershi;
                                        ap.EmpoweringSupplier = item.EmpoweringSupplier;
                                        if(item.ExpiryDate != null)
                                        {
                                            ap.ExpiryDate = Convert.ToDateTime(item.ExpiryDate);
                                        }
                                        
                                        idb.Entry(ap).State = System.Data.EntityState.Added;
                                        idb.SaveChanges();
                                    }
                                }

                                if (item.ExpiryDate != null)
                                {
                                    item.ExpDate = Convert.ToDateTime(item.ExpiryDate); //for tracking changes. ExpDate stores actual date, ExpiryDate stores formatted date
                                }

                                //Build list for Audit trail purposes
                                BeeSupplierClass newItem = new BeeSupplierClass();
                                newItem.Supplier = item.Supplier;
                                newItem.EnterpriseType = item.EnterpriseType;
                                newItem.BeeLevel = item.BeeLevel;
                                newItem.BlackOwnership = item.BlackOwnership;
                                newItem.BlackWomenOwnershi = item.BlackWomenOwnershi;
                                newItem.EmpoweringSupplier = item.EmpoweringSupplier;
                                newItem.ExpiryDate = item.ExpiryDate;
                                newValue.Add(newItem);
                                
                        }

                        
                        
                        
                        

                        foreach(var item in supplierList)
                        {
                            BeeSupplierClass oldItem = new BeeSupplierClass();
                            oldItem.Supplier = item.Supplier;
                            oldItem.EnterpriseType = item.EnterpriseType;
                            oldItem.BeeLevel = item.BeeLevel;
                            oldItem.BlackOwnership = item.BlackOwnership;
                            oldItem.BlackWomenOwnershi = item.BlackWomenOwnershi;
                            oldItem.EmpoweringSupplier = item.EmpoweringSupplier;
                            oldItem.ExpiryDate = item.ExpiryDate;

                            var newItem = (from a in newValue where a.Supplier == item.Supplier select a).FirstOrDefault();

                            List<EntityChanges> changes = AuditHelper.EnumeratePropertyDifferences(oldItem, newItem);
                            if(changes.Count > 0)
                            {
                                foreach(var ch in changes)
                                {
                                    if(ch.OldValue != ch.NewValue)
                                    {
                                        objBee.AuditBeeSupplier(item.Supplier, ch.KeyField, ch.OldValue, ch.NewValue);
                                    }
                                   
                                }
                            }
                        }


                        if (model.FilterText == null)
                        {
                            model.FilterText = "";
                        }
                        List<sp_GetBeeSuppliers_Result> NewsupplierList;
                        using (var adb = new SysproEntities(""))
                        {
                            NewsupplierList = sdb.sp_GetBeeSuppliers(model.FilterText).ToList();

                        }

                        if (!string.IsNullOrEmpty(model.ExpiryDate))
                        {
                            DateTime ExpDate = Convert.ToDateTime(model.ExpiryDate);
                            NewsupplierList = (from a in NewsupplierList where a.ExpDate == null || a.ExpDate <= ExpDate select a).ToList();
                        }

                        if (model.PurchaseValue != 0 && model.PurchaseValue != null)
                        {
                            NewsupplierList = (from a in NewsupplierList where a.PurchaseValue >= model.PurchaseValue select a).ToList();
                        }

                        model.Detail = NewsupplierList;
                        

                        ModelState.AddModelError("", "Saved Successfully!");
                        
                        
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


        public JsonResult GetGridData(string FilterText, string ExpiryDate, string PurchaseValue, string FromPeriod, string ToPeriod)
        {
            try
            {
                var supplierList = sdb.sp_GetBeeSuppliers(FilterText).ToList();

               

                if(!string.IsNullOrEmpty(ExpiryDate))
                {
                    DateTime ExpDate = Convert.ToDateTime(ExpiryDate);
                    supplierList = (from a in supplierList where a.ExpDate != null && a.ExpDate <= ExpDate select a).ToList();
                }
                return Json(supplierList, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(ex.Message);
            }
        }


        public ActionResult Email()
        {
            var username = HttpContext.User.Identity.Name.ToUpper();
            var sett = (from a in mdb.mtUsers where a.Username == username select a.EmailAddress).FirstOrDefault();
            BeeEmail model = new BeeEmail();
            model.FromEmail = sett;
            return PartialView(model);
        }


        [HttpPost]
        public ActionResult EmailSupplier(string details)
        {
            try
            {
                List<BeeEmail> myDeserializedObjList = (List<BeeEmail>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<BeeEmail>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach(var item in myDeserializedObjList)
                    {
                        Mail _mail = new Mail();
                        _mail.From = item.FromEmail;
                        _mail.To = item.ToEmail;
                        _mail.Subject = item.Subject;
                        _mail.Body = item.MessageBody;

                        objEmail.SendEmail(_mail, new List<string>());

                        mtBeeEmailLog log = new mtBeeEmailLog();
                        log.FromEmail = item.FromEmail;
                        log.Email = item.ToEmail;
                        log.Contact = "";
                        log.Subject = item.Subject;
                        log.Body = item.MessageBody;
                        log.Username = HttpContext.User.Identity.Name.ToUpper();
                        log.TrnDate = DateTime.Now;
                        log.Supplier = item.Supplier;
                        sdb.mtBeeEmailLogs.Add(log);
                        sdb.SaveChanges();
                    }
                    return Json("Email(s) sent successfully!", JsonRequestBehavior.AllowGet);
                }
                return Json("No data found!", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
