using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class RepairsAndMaintenanceController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore objSyspro = new SysproCore();
        RepairsAndMaintenanceBL BL = new RepairsAndMaintenanceBL();

        [CustomAuthorize(Activity: "RepairsAndMaintenance")]
        public ActionResult Index(string ProgramMode = "")
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.WarehouseList = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            var Employees = wdb.sp_BaggingLabelEmployees().ToList();
            ViewBag.EmployeeList = (from a in Employees where a.ProcessTask == "MAINT" select new { Employee = a.Employee, Description = a.Employee }).ToList();
            var CostCentreList = (from a in mdb.mtUserDepartments where a.Company == Company && a.Username == Username select new { CostCentre = a.CostCentre, Description = a.CostCentre }).ToList();
            ViewBag.CostCentreList = CostCentreList;
            ViewBag.WorkCentreList = new List<SelectListItem>();
            ViewBag.ProgramMode = ProgramMode;
            RepairsAndMaintenanceViewModel model = new RepairsAndMaintenanceViewModel();
            model.ProgramMode = ProgramMode;
            return View(model);
        }



        [HttpPost]
        public ActionResult CheckWarehouseMultiBin(string details)
        {
            try
            {
                List<MultiBin> myDeserializedObjList = (List<MultiBin>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MultiBin>));
                if (myDeserializedObjList.Count > 0)
                {
                    string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse.Trim();
                    var result = (from a in wdb.vw_InvWhControl where a.Warehouse == Warehouse select a).ToList();
                    if (result.Count > 0)
                    {
                        return Json(result.FirstOrDefault().UseMultipleBins, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error. Warehouse : " + Warehouse + " not found in Syspro.", JsonRequestBehavior.AllowGet);
                    }
                }
                return Json("Error - No Data. Warehouse not found.", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public class MultiBin
        {
            public string Warehouse { get; set; }
            public string Source { get; set; }
        }

        [HttpPost]
        public ActionResult ValidateDetails(string details)
        {
            try
            {
                return Json(ValidateBarcode(details), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public string ValidateBarcode(string details)
        {
            try
            {
                List<RepairsAndMaintenanceViewModel> myDeserializedObjList = (List<RepairsAndMaintenanceViewModel>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<RepairsAndMaintenanceViewModel>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var StockCodeCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                        if (StockCodeCheck == null)
                        {
                            return "StockCode not found!.";
                        }

                        var StockWarehouseCheck = wdb.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse)).FirstOrDefault();
                        if (StockWarehouseCheck == null)
                        {
                            return "StockCode not stocked in Warehouse " + item.Warehouse + "!.";
                        }
                        if (item.Quantity == 0)
                        {
                            return "Quantity cannot be zero!";
                        }


                        var TraceableCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(item.StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                        if (TraceableCheck != null)
                        {
                            //StockCode is Traceable -- Lot number required
                            if (string.IsNullOrEmpty(item.Lot))
                            {
                                return "StockCode is Lot Traceable. Lot number required";
                            }
                            else
                            {
                                var LotCheck = wdb.LotDetails.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.Warehouse) && a.Lot.Equals(item.Lot)).FirstOrDefault();
                                if (LotCheck == null)
                                {
                                    return "Lot " + item.Lot + " not found for StockCode " + item.StockCode + " in Warehouse " + item.Warehouse + "!";
                                }
                                else
                                {
                                    return "";
                                }
                            }
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public ActionResult GetWorkCentres(string CostCentre)
        {
            try
            {
                return Json((from a in wdb.BomWorkCentres where a.CostCentre == CostCentre select new { WorkCentre = a.WorkCentre, Description = a.WorkCentre + " - " + a.Description }).ToList());
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult StockCodeSearch()
        {
            return PartialView();
        }

        public JsonResult StockCodeList(string Warehouse)
        {
            var result = (from a in wdb.InvWarehouses.AsNoTracking() join b in wdb.InvMasters on a.StockCode equals b.StockCode where a.Warehouse == Warehouse select new { MStockCode = a.StockCode, MStockDes = b.Description, MStockingUom = b.StockUom, ProductClass = b.ProductClass, WarehouseToUse = b.WarehouseToUse }).Distinct().ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostSysproJob")]
        public ActionResult PostSysproJob(RepairsAndMaintenanceViewModel model)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var WhList = wdb.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.WarehouseList = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                var Employees = wdb.sp_BaggingLabelEmployees().ToList();
                ViewBag.EmployeeList = (from a in Employees where a.ProcessTask == "MAINT" select new { Employee = a.Employee, Description = a.Employee }).ToList();
                var CostCentreList = (from a in mdb.mtUserDepartments where a.Company == Company && a.Username == Username select new { CostCentre = a.CostCentre, Description = a.CostCentre }).ToList();
                ViewBag.CostCentreList = CostCentreList;
                ViewBag.WorkCentreList = new List<SelectListItem>();


                if(model.Warehouse != "**")
                {
                    var TraceableType = (from a in wdb.InvMasters where a.StockCode.Equals(model.StockCode) select new { TraceableType = a.TraceableType, SerialMethod = a.SerialMethod, ProductClass = a.ProductClass }).FirstOrDefault();
                    //var GlCode = (from a in wdb.mtExpenseIssueMatrices where a.CostCentre == model.CostCentre && a.WorkCentre == model.WorkCentre && a.ProductClass == TraceableType.ProductClass select a.GlCode).FirstOrDefault();
                    //if (string.IsNullOrWhiteSpace(GlCode))
                    //{
                    //    ModelState.AddModelError("", "No GL code found in Matrix for Cost Centre: " + model.CostCentre + " WorkCentre: " + model.WorkCentre + " Product Cass: " + TraceableType.ProductClass);
                    //    return View("Index", model);
                    //}

                    var StockCodeCheck = wdb.InvMasters.Where(a => a.StockCode.Equals(model.StockCode)).FirstOrDefault();
                    if (StockCodeCheck == null)
                    {
                        ModelState.AddModelError("", "StockCode not found!.");
                        return View("Index", model);

                    }

                    var StockWarehouseCheck = wdb.InvWarehouses.Where(a => a.StockCode.Equals(model.StockCode) && a.Warehouse.Equals(model.Warehouse)).FirstOrDefault();
                    if (StockWarehouseCheck == null)
                    {
                        ModelState.AddModelError("", "StockCode not stocked in Warehouse " + model.Warehouse + "!.");
                        return View("Index", model);
                    }
                }

                

                if (model.Quantity == 0)
                {
                    ModelState.AddModelError("", "Quantity cannot be zero!");
                    return View("Index", model);
                }

                string Guid = objSyspro.SysproLogin();
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    ModelState.AddModelError("", "Failed to login to Syspro.");
                    return View("Index", model);
                }


                string PostError = BL.PostJobCreation(Guid, model.StockCode, model.Quantity, model.Warehouse, model.Bin, model.Lot, model.Employee, model.WorkCentre, model.Notes, model.CostCentre,Company);
                objSyspro.SysproLogoff(Guid);
                model.StockCode = "";
                model.Lot = "";
                model.Quantity = 0;
                model.Notes = "";
                ModelState.AddModelError("", PostError);
                return View("Index", model);


            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }

        }



    }
}
