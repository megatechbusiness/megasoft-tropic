using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class SalesContractPricingController : Controller
    {
        //
        // GET: /SalesContractPricing/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        SorContractPrice objPost = new SorContractPrice();
        SysproCore objSys = new SysproCore();

        [CustomAuthorize(Activity: "MaintainContracts")]
        public ActionResult Index()
        {
            return View();
        }

        [CustomAuthorize(Activity: "MaintainContracts")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Index")]
        public ActionResult Index(SorContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                string Customer = ""; string Contract = ""; string StockCode = "";
                if (!string.IsNullOrEmpty(model.Customer))
                {
                    Customer = model.Customer;
                }

                if (!string.IsNullOrEmpty(model.Contract))
                {
                    Contract = model.Contract;
                }

                if (!string.IsNullOrEmpty(model.StockCode))
                {
                    StockCode = model.StockCode;
                }

                var detail = wdb.sp_GetSalesContractPricingForExpiry(Customer, Contract, StockCode).ToList();
                SorContractPriceViewModel modelOut = new SorContractPriceViewModel();
                modelOut.Customer = model.Customer;
                modelOut.Contract = model.Contract;
                modelOut.StockCode = model.StockCode;
                modelOut.Detail = detail;
                return View("Index", modelOut);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }

        }


        [CustomAuthorize(Activity: "MaintainContracts")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostUpdate")]
        public ActionResult PostUpdate(SorContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (model.Detail.Count > 0)
                {
                    string Guid = objSys.SysproLogin();
                    string Document = objPost.BuildContractPriceDocument(model.Detail);
                    string Parameter = objPost.BuildContractPriceParameter();

                    string XmlOut = objSys.SysproSetupUpdate(Guid, Parameter, Document, "SORSCP");

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
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        [CustomAuthorize(Activity: "ExpireContracts")]
        public ActionResult ExpireContracts()
        {
            return View(new SorContractPriceViewModel());
        }


        [CustomAuthorize(Activity: "ExpireContracts")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ExpireContracts")]
        public ActionResult ExpireContracts(SorContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                string Customer = ""; string Contract = "";string StockCode = "";
                if(!string.IsNullOrEmpty(model.Customer))
                {
                    Customer = model.Customer;
                }

                if (!string.IsNullOrEmpty(model.Contract))
                {
                    Contract = model.Contract;
                }

                if (!string.IsNullOrEmpty(model.StockCode))
                {
                    StockCode = model.StockCode;
                }

                var detail = wdb.sp_GetSalesContractPricingForExpiry(Customer, Contract, StockCode).ToList();
                SorContractPriceViewModel modelOut = new SorContractPriceViewModel();
                modelOut.Customer = model.Customer;
                modelOut.Contract = model.Contract;
                modelOut.StockCode = model.StockCode;
                modelOut.Detail = detail;
                return View("ExpireContracts", modelOut);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("ExpireContracts", model);
            }
            
        }

        [CustomAuthorize(Activity: "ExpireContracts")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostForm")]
        public ActionResult PostExpireContracts(SorContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                if(model.Detail.Count > 0)
                {
                    string Guid = objSys.SysproLogin();
                    string Document = objPost.BuildExpireDocument(model.Detail);
                    string Parameter = objPost.BuildContractPriceParameter();

                    string XmlOut = objSys.SysproSetupUpdate(Guid, Parameter, Document, "SORSCP");

                    objSys.SysproLogoff(Guid);

                    string ErrorMessage = objSys.GetXmlErrors(XmlOut);

                    if(string.IsNullOrEmpty(ErrorMessage))
                    {
                        ModelState.AddModelError("", "Posted Successfully");
                        return View("ExpireContracts", model);
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                        return View("ExpireContracts", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No data found to post!");
                    return View("ExpireContracts", model);
                }
                

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("ExpireContracts", model);
            }
        }


        [CustomAuthorize(Activity: "NewContracts")]
        public ActionResult NewContract()
        {
            return View();
        }

        [CustomAuthorize(Activity: "NewContracts")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostNewContract")]
        public ActionResult PostNewContract(SorContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (model != null)
                {
                    string Guid = objSys.SysproLogin();
                    string Document = objPost.BuildNewContractPriceDocument(model);
                    string Parameter = objPost.BuildContractPriceParameter();

                    string XmlOut = objSys.SysproSetupAdd(Guid, Parameter, Document, "SORSCP");

                    objSys.SysproLogoff(Guid);

                    string ErrorMessage = objSys.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        ModelState.AddModelError("", "Posted Successfully");
                        return View("NewContract", model);
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                        return View("NewContract", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No data found to post!");
                    return View("NewContract", model);
                }


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("NewContract", model);
            }
        }
        
        [CustomAuthorize(Activity: "ContractsByCustList")]
        public ActionResult ContractsXref()
        {
            return View();
        }

        [CustomAuthorize(Activity: "ContractsByCustList")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadXRef")]
        public ActionResult LoadXRef(SorContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                string Customer = "";
                if (!string.IsNullOrEmpty(model.Customer))
                {
                    Customer = model.Customer;
                }


                var detail = wdb.sp_GetStockCodesByCustomer(Customer).ToList();
                SorContractPriceViewModel modelOut = new SorContractPriceViewModel();
                modelOut.Customer = model.Customer;
                modelOut.CustomerListing = detail;
                return View("ContractsXref", modelOut);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("ContractsXref", model);
            }

        }

        [CustomAuthorize(Activity: "ContractsByCustList")]
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostContractsXref")]
        public ActionResult PostContractsXref(SorContractPriceViewModel model)
        {
            try
            {
                ModelState.Clear();
                if (model.CustomerListing.Count > 0)
                {
                    string Guid = objSys.SysproLogin();
                    string Document = objPost.BuildContractPriceXrefDocument(model.CustomerListing, model.Contract, model.StartDate, model.ExpiryDate, "C");
                    string Parameter = objPost.BuildContractPriceParameter();

                    string XmlOut = objSys.SysproSetupAdd(Guid, Parameter, Document, "SORSCP");

                    objSys.SysproLogoff(Guid);

                    string ErrorMessage = objSys.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(ErrorMessage))
                    {
                        ModelState.AddModelError("", "Posted Successfully");
                        return View("ContractsXref", model);
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorMessage);
                        return View("ContractsXref", model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No data found to post!");
                    return View("ContractsXref", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("ContractsXref", model);
            }

        }
    }
}
