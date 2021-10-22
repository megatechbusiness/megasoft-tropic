using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class SupplierContractPricingController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        SysproCore objSys = new SysproCore();
        SupplierContractsBL objPost = new SupplierContractsBL();

        [CustomAuthorize(Activity: "SupplierContractListing")]
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadXRef")]
        public ActionResult LoadXRef(SupplierContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                string Supplier = "";
                if (!string.IsNullOrEmpty(model.Supplier))
                {
                    Supplier = model.Supplier;
                }


                var detail = wdb.sp_GetSupContractsStockCodesBySupplier(Supplier).ToList();
                SupplierContractPriceViewModel modelOut = new SupplierContractPriceViewModel();
                modelOut.Supplier = model.Supplier;
                modelOut.SupplierListing = detail;
                modelOut.Contract = model.Contract;
                modelOut.StartDate = model.StartDate;
                modelOut.ExpiryDate = model.ExpiryDate;
                return View("Index", modelOut);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }

        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostContractsXref")]
        public ActionResult PostContractsXref(SupplierContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (model.SupplierListing.Count > 0)
                {
                    string Guid = objSys.SysproLogin();
                    string Document = objPost.BuildContractPriceXrefDocument(model.SupplierListing, model.Contract, model.StartDate, model.ExpiryDate);
                    string Parameter = objPost.BuildContractPriceParameter();

                    string XmlOut = objSys.SysproPost(Guid, Parameter, Document, "PORTSC");

                    objSys.SysproLogoff(Guid);

                    string ErrorMessage = objSys.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        ModelState.AddModelError("", "Posted Successfully");
                        return View("Index", model);
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                        return View("Index", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No data found to post!");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }

        }





        [CustomAuthorize(Activity: "MaintainContracts")]
        public ActionResult MaintainContracts()
        {
            return View();
        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadMaintainContracts")]
        public ActionResult LoadMaintainContracts(SupplierContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                string Supplier = ""; string Contract = ""; string StockCode = "";
                if (!string.IsNullOrEmpty(model.Supplier))
                {
                    Supplier = model.Supplier;
                }

                if (!string.IsNullOrEmpty(model.Contract))
                {
                    Contract = model.Contract;
                }

                if (!string.IsNullOrEmpty(model.StockCode))
                {
                    StockCode = model.StockCode;
                }

                var detail = wdb.sp_GetSupplierContractsPricingData(Supplier, Contract, StockCode).ToList();
                SupplierContractPriceViewModel modelOut = new SupplierContractPriceViewModel();
                modelOut.Supplier = model.Supplier;
                modelOut.Contract = model.Contract;
                modelOut.StockCode = model.StockCode;
                modelOut.Detail = detail;
                return View("MaintainContracts", modelOut);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("MaintainContracts", model);
            }

        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostUpdate")]
        public ActionResult PostUpdate(SupplierContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (model.Detail.Count > 0)
                {
                    string Guid = objSys.SysproLogin();
                    string Document = objPost.BuildContractPriceDocument(model.Detail);
                    string Parameter = objPost.BuildContractPriceParameter();

                    string XmlOut = objSys.SysproPost(Guid, Parameter, Document, "PORTSC");

                    objSys.SysproLogoff(Guid);

                    string ErrorMessage = objSys.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        ModelState.AddModelError("", "Posted Successfully");
                        return View("MaintainContracts", model);
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                        return View("MaintainContracts", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No data found to post!");
                    return View("MaintainContracts", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("MaintainContracts", model);
            }
        }

    }
}
