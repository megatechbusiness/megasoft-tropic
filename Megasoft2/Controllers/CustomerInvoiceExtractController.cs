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
    public class CustomerInvoiceExtractController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

        InvoiceExtractBL BL = new InvoiceExtractBL();
        //
        // GET: /CustomerInvoiceExtract/

        [CustomAuthorize("CustomerInvoiceExtract")]
        public ActionResult Index()
        {
            var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
            ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
            CustomerInvoiceViewModel model = new CustomerInvoiceViewModel();
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadInvoices")]
        public ActionResult LoadInvoices(CustomerInvoiceViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (string.IsNullOrEmpty(model.FromDate))
                {
                    ModelState.AddModelError("", "Please select a From Date.");
                    return View("Index", model);
                }
                else if (string.IsNullOrEmpty(model.ToDate))
                {
                    ModelState.AddModelError("", "Please select a To Date.");
                    return View("Index", model);
                }
                else
                {
                    var InvoiceList = wdb.sp_GetInvoices(Convert.ToDateTime(model.FromDate), Convert.ToDateTime(model.ToDate), model.CustomerClass, "N").ToList();
                    //var InvoiceList = wdb.sp_GetInvoices(Convert.ToDateTime("2021-09-01"), Convert.ToDateTime("2021-09-30"), model.CustomerClass, "N").ToList();
                    model.Invoices = InvoiceList;
                    var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DownloadFile")]
        public ActionResult DownloadFile(CustomerInvoiceViewModel model)
        {
            try
            {
                ModelState.Clear();

                var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
                if (string.IsNullOrEmpty(model.FromDate))
                {
                    ModelState.AddModelError("", "Please select a From Date.");
                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }
                else if (string.IsNullOrEmpty(model.ToDate))
                {
                    ModelState.AddModelError("", "Please select a To Date.");
                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }
                else if (model.Invoices.Count == 0)
                {
                    ModelState.AddModelError("", "No Invoices found.");
                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }
                else
                {
                    if (model.CustomerClass == "NG")
                    {
                        ModelState.AddModelError("", this.BL.DownloadNestleInvoice(model, false, false));
                    }
                    else if (model.CustomerClass == "UG")
                    {
                        ModelState.AddModelError("", this.BL.DownloadUnileverInvoice(model, false, false));
                    }
                    else
                    {
                        ModelState.AddModelError("", "No Xml Defined for selected customer class.");
                    }

                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                return View("Index", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SFTPFile")]
        public ActionResult SFTPFile(CustomerInvoiceViewModel model)
        {
            try
            {
                ModelState.Clear();
                var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
                if (string.IsNullOrEmpty(model.FromDate))
                {
                    ModelState.AddModelError("", "Please select a From Date.");
                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }
                else if (string.IsNullOrEmpty(model.ToDate))
                {
                    ModelState.AddModelError("", "Please select a To Date.");
                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }
                else if (model.Invoices.Count == 0)
                {
                    ModelState.AddModelError("", "No Invoices found.");
                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }
                else
                {
                    if (model.CustomerClass == "NG")
                    {
                        ModelState.AddModelError("", this.BL.DownloadNestleInvoice(model, true, false));
                    }
                    else if (model.CustomerClass == "UG")
                    {
                        ModelState.AddModelError("", this.BL.DownloadUnileverInvoice(model, true, false));
                    }
                    else
                    {
                        ModelState.AddModelError("", "No Xml Defined for selected customer class.");
                    }


                    ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                    return View("Index", model);
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                return View("Index", model);
            }
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SendEmail")]
        public ActionResult SendEmail(CustomerInvoiceViewModel model)
        {
            try
            {
                ModelState.Clear();



                BL.SendEmail(model);

                ModelState.AddModelError("", "Sent.");
                var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var result = wdb.sp_GetInvoiceExtractCustomerClass().ToList();
                ViewBag.CustomerClassList = (from a in result select new SelectListItem { Value = a.Class.Trim(), Text = a.Description }).ToList();
                return View("Index", model);
            }
        }
    }
}
