using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.UI.WebControls;

namespace Megasoft2.Controllers
{
    public class MaterialIssueController : Controller
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        BusinessLogic.SysproMaterialIssue objMat = new BusinessLogic.SysproMaterialIssue();
        private LabelPrint objPrint = new LabelPrint();
        [CustomAuthorize(Activity: "MaterialIssue-Scan")]
        public ActionResult Index()
        {
            // ViewBag.Warehouse = (from a in db.vw_InvWhControl select new { Warehouse = a.Warehouse, Description = a.Description }).ToList();
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.PrinterList = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Printer = a.PrinterName, Description = a.PrinterName }).ToList();
            var PrintLabel = (from a in db.mtWhseManSettings where a.SettingId == 1 select a.PrintLabelOnMaterialIssueReturn).FirstOrDefault();
            ViewBag.DepartmentList = new List<SelectListItem>();
            if (PrintLabel == null)
            {
                ViewBag.PrinterList = new List<SelectListItem>();
                ViewBag.PrinterList = false;
            }
            else
            {
                ViewBag.PrintLabel = PrintLabel;
            }
            return View();
        }
        [MultipleButton(Name ="action",Argument ="LoadJob")]
        [HttpPost]
        public ActionResult LoadJob(MaterialIssue model)
        {
            string Job = model.Job.PadLeft(15, '0');
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.PrinterList = (from a in mdb.mtUserPrinterAccesses where a.Username == Username && a.Company == Company select new { Printer = a.PrinterName, Description = a.PrinterName }).ToList();
            var PrintLabel = (from a in db.mtWhseManSettings where a.SettingId == 1 select a.PrintLabelOnMaterialIssueReturn).FirstOrDefault();
            try
            {
                var DeptList = (from a in db.sp_GetProductionDepartments(Job, Company, Username).ToList() select new { Value = a.CostCentre, Text = a.CostCentre }).ToList();
                ViewBag.DepartmentList = DeptList;
                return View("Index",model);
            }
            catch (Exception)
            {
                return View("Index", model);
            }
        }
        //[HttpPost]
        //public JsonResult CalcNoOfLabels(string StockCode,string Job, decimal IssueQty)
        //{
        //    try
        //    {
        //        var LabelsToPrint = (from a in db.mt_ProductionLabelCalc(StockCode, Job, IssueQty) select a).FirstOrDefault();
        //        if (LabelsToPrint!= null)
        //        {
        //            return Json(LabelsToPrint.NumberofLabels, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json("Error: Calc No of lalbels", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return Json("", JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]
        public ActionResult PostMaterialIssue(string details)
        {
            try
            {
                var resultPostMaterialIssue = objMat.PostMaterialIssue(details);
                return Json(resultPostMaterialIssue, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomAuthorize(Activity: "MaterialIssue")]
        public ActionResult MaterialIssue()
        {
            return View();
        }

        [HttpPost]
        // [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public JsonResult LoadGrid(string details)
        {
            try
            {
                clsMaterialIssue objMat = new clsMaterialIssue();

                List<MaterialAllocation> myDeserializedObjList = (List<MaterialAllocation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MaterialAllocation>));
                string Job = myDeserializedObjList.FirstOrDefault().Job;
                DataTable dt = objMat.GetMaterialAllocationList(Job);
                if (dt.Rows.Count > 0)
                {
                    List<MaterialAllocation> objList = new List<MaterialAllocation>();

                    objList = (from DataRow row in dt.Rows
                               select new MaterialAllocation
                               {
                                   Job = row["Job"].ToString().Trim(),
                                   StockCode = row["StockCode"].ToString().Trim(),
                                   Description = row["StockDescription"].ToString().Trim(),
                                   Line = row["Line"].ToString().Trim(),
                                   QtyRequired = (decimal)row["UnitQtyReqd"],
                                   QtyIssued = (decimal)row["QtyIssued"],
                                   QtyOnHand = (decimal)row["QtyOnHand"],
                                   Warehouse = row["Warehouse"].ToString().Trim(),
                                   JobStockCode = row["JobStockCode"].ToString().Trim(),
                                   JobDescription = row["JobDescription"].ToString().Trim(),
                                   Traceable = row["Traceable"].ToString().Trim()

                               }).ToList();
                    return Json(objList.ToArray(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class MaterialAllocation
        {
            public string Job { get; set; }
            public string StockCode { get; set; }
            public string Description { get; set; }
            public string Line { get; set; }
            public decimal QtyRequired { get; set; }
            public decimal QtyIssued { get; set; }
            public decimal Quantity { get; set; }
            public string Shift { get; set; }
            public string WorkCentre { get; set; }
            public string Warehouse { get; set; }
            public string JobStockCode { get; set; }
            public string JobDescription { get; set; }
            public decimal QtyOnHand { get; set; }
            public string FilterText { get; set; }

            public string Traceable { get; set; }
            public string Lot { get; set; }

            public string Guid { get; set; }




        }

        [HttpPost]
        public JsonResult GetWorkCentres()
        {
            try
            {
                clsMaterialIssue objMat = new clsMaterialIssue();
                DataTable dt = objMat.GetWorkCentre();
                if (dt.Rows.Count > 0)
                {
                    List<ListItem> objList = new List<ListItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        objList.Add(new ListItem
                        {
                            Text = row["Description"].ToString().Trim(),
                            Value = row["WorkCentre"].ToString().Trim()
                        });
                    }
                    return Json(objList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult GetWarehouses()
        {
            try
            {
                clsMaterialIssue objMat = new clsMaterialIssue();
                DataTable dt = objMat.GetWarehouse();
                if (dt.Rows.Count > 0)
                {
                    List<ListItem> objList = new List<ListItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        objList.Add(new ListItem
                        {
                            Text = row["Description"].ToString().Trim(),
                            Value = row["Warehouse"].ToString().Trim()
                        });
                    }
                    return Json(objList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [HttpPost]
        public JsonResult GetShifts()
        {
            try
            {
                clsMaterialIssue objMat = new clsMaterialIssue();
                DataTable dt = objMat.GetShifts();
                if (dt.Rows.Count > 0)
                {
                    List<ListItem> objList = new List<ListItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        objList.Add(new ListItem
                        {
                            Text = row["Description"].ToString().Trim(),
                            Value = row["Shift"].ToString().Trim()
                        });
                    }
                    return Json(objList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [HttpPost]
        public JsonResult PostJobMaterialIssue(string details)
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToString().ToUpper();
                clsMaterialIssue objMat = new clsMaterialIssue();
                string PostReturn = objMat.PostMaterialIssue(details, Username);
                return Json(PostReturn, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public JsonResult GetStockCodes(string details)
        {
            try
            {
                clsMaterialIssue objMat = new clsMaterialIssue();

                List<MaterialAllocation> myDeserializedObjList = (List<MaterialAllocation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MaterialAllocation>));
                string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse;
                string FilterText = myDeserializedObjList.FirstOrDefault().FilterText.ToUpper();
                DataTable dt = objMat.GetStockCodesByWarehouse(Warehouse, FilterText);
                if (dt.Rows.Count > 0)
                {
                    List<MaterialAllocation> objList = new List<MaterialAllocation>();

                    objList = (from DataRow row in dt.Rows
                               select new MaterialAllocation
                               {
                                   StockCode = row["StockCode"].ToString().Trim(),
                                   Description = row["Description"].ToString().Trim(),
                                   QtyOnHand = (decimal)row["QtyOnHand"],
                                   Warehouse = row["Warehouse"].ToString().Trim()
                               }).ToList();
                    return Json(objList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        [HttpPost]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public JsonResult PostMaterialAllocation(string details)
        {
            try
            {
                clsMaterialIssue objMat = new clsMaterialIssue();
                List<MaterialAllocation> myDeserializedObjList = (List<MaterialAllocation>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<MaterialAllocation>));
                string Job = myDeserializedObjList.FirstOrDefault().Job;
                string StockCode = myDeserializedObjList.FirstOrDefault().StockCode;
                string Descrition = objMat.GetStockCodeDescription(StockCode);
                string Warehouse = myDeserializedObjList.FirstOrDefault().Warehouse;
                string Traceable = objMat.GetTraceable(StockCode);
                decimal QtyOnHand = objMat.GetQtyOnHand(StockCode, Warehouse);
                string Username = HttpContext.User.Identity.Name.ToString().ToUpper();
                DataTable dt = objMat.AddMaterialAllocation(Job, StockCode, Warehouse, Username);
                if (dt.Rows.Count > 0)
                {
                    List<MaterialAllocation> objList = new List<MaterialAllocation>();

                    objList = (from DataRow row in dt.Rows
                               select new MaterialAllocation
                               {
                                   Job = row["Job"].ToString().Trim(),
                                   Line = row["Line"].ToString().Trim(),
                                   StockCode = row["StockCode"].ToString().Trim(),
                                   Description = Descrition,
                                   QtyOnHand = QtyOnHand,
                                   QtyRequired = 0,
                                   QtyIssued = 0,
                                   Warehouse = row["Warehouse"].ToString().Trim(),
                                   Traceable = Traceable
                               }).ToList();
                    return Json(objList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("Failed to post Material allocation.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult GetLots(string Warehouse, string StockCode)
        {
            try
            {
                clsMaterialIssue objMat = new clsMaterialIssue();
                DataTable dt = objMat.GetLots(Warehouse, StockCode);
                List<MaterialAllocation> objList = new List<MaterialAllocation>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        MaterialAllocation ID = new MaterialAllocation();
                        ID.Lot = row["Lot"].ToString();
                        ID.Description = row["Lot"].ToString() + "  (" + Math.Round(Convert.ToDecimal(row["QtyOnHand"].ToString()), 2) + ")";

                        objList.Add(ID);
                    }
                    return Json(objList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public ActionResult TempReversal()
        {
            return View();
        }
        [HttpPost]
        public ActionResult TempReversal(mtTmpLotsToReverse model)
        {
            try
            {
                ModelState.Clear();

                string returnmsg = objMat.PostTempMaterialIssue();

                ModelState.AddModelError("", returnmsg);

                return View("TempReversal", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("TempReversal", model);
            }

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
                List<WarehouseTransfer> myDeserializedObjList = (List<WarehouseTransfer>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WarehouseTransfer>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var StockCodeCheck = db.InvMasters.Where(a => a.StockCode.Equals(item.StockCode)).FirstOrDefault();
                        if (StockCodeCheck == null)
                        {
                            return "StockCode not found!.";
                        }

                        var StockWarehouseCheck = db.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.SourceWarehouse)).FirstOrDefault();
                        if (StockWarehouseCheck == null)
                        {
                            return "StockCode not stocked in Warehouse " + item.SourceWarehouse + "!.";
                        }

                        if (item.Quantity == 0)
                        {
                            return "Quantity cannot be zero!";
                        }

                        var TraceableCheck = db.InvMasters.Where(a => a.StockCode.Equals(item.StockCode) && a.TraceableType.Equals("T")).FirstOrDefault();
                        if (TraceableCheck != null)
                        {
                            //StockCode is Traceable -- Lot number required
                            if (string.IsNullOrEmpty(item.LotNumber))
                            {
                                return "StockCode is Lot Traceable. Lot number required";
                            }
                            else
                            {
                                var LotCheck = db.LotDetails.Where(a => a.StockCode.Equals(item.StockCode) && a.Warehouse.Equals(item.SourceWarehouse) && a.Lot.Equals(item.LotNumber)).FirstOrDefault();
                                if (LotCheck == null)
                                {
                                    return "Lot " + item.LotNumber + " not found for StockCode " + item.StockCode + " in Warehouse " + item.SourceWarehouse + "!";
                                }
                                else
                                {
                                    //var LotQtyCheck = db.LotDetails.Where(a => a.StockCode.Equals(item.StockCode)
                                    //                                        && a.Warehouse.Equals(item.SourceWarehouse)
                                    //                                        && a.Lot.Equals(item.LotNumber)
                                    //                                      ).Select(a => a.QtyOnHand).Sum();
                                    //if (LotQtyCheck < item.Quantity)
                                    //{
                                    //    return "Quantity on hand is less than Quantity to transfer for Lot " + item.LotNumber + "!";
                                    //}
                                    //else
                                    //{
                                    return "";
                                    //}
                                }
                            }
                        }
                        else
                        {
                            //StockCode is not Traceable -- Check Quantity
                            //var QtyCheck = db.InvWarehouses.Where(a => a.StockCode.Equals(item.StockCode)
                            //                                              && a.Warehouse.Equals(item.SourceWarehouse)
                            //                                              ).Select(a => a.QtyOnHand).Sum();
                            //if (QtyCheck < item.Quantity)
                            //{
                            //    return "Quantity on hand is less than Quantity to transfer for StockCode " + item.StockCode + "!";
                            //}
                            //else
                            //{
                            return "";
                            //}
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

    }
}
