﻿using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class DispatchPlannerController : Controller
    {
        //
        // GET: /DispatchPlanner/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "CustomerOrderScheduler")]
        public ActionResult Index()
        {
            DispatchPlannerViewModel model = new DispatchPlannerViewModel();
            ViewBag.CanSaveSchedule = CanSaveSchedule();
            try
            {
                var result = (from a in wdb.mt_DispatchPlanGetOrders() select a).ToList();
                model.OpenOrders = result;
                System.DateTime dateTime = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd"));
                model.Plans = (from a in wdb.mtDispatchPlans where a.DispatchDate == dateTime select a).ToList();
                var transporter = (from a in wdb.mtTransporters select new { Name = a.VehicleReg + " - " + a.Transporter, Capacity = a.VehicleCapacity }).ToList();
                model.TruckList = new List<string> { "Select Transporter" };
                model.Capacity = new List<string> { "" };
                foreach (var truck in transporter)
                {
                    model.TruckList.Add(truck.Name);
                    model.Capacity.Add(truck.Capacity.ToString());
                }
                model.TruckList.Add("!");
                model.SaveTL = model.TruckList;
                model.Capacity.Add("14");

                model.DispatchDate = DateTime.Now;
                model.DeliveryNo = "1";
                LoadData(model);
                model.Messages = "Index";
                return View(model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        public bool CanSaveSchedule()
        {
            try
            {
                var Admin = (from a in mdb.mtUsers where a.Username == HttpContext.User.Identity.Name.ToUpper() && a.Administrator == true select a).ToList();
                if (Admin.Count > 0)
                {
                    return true;
                }
                var Emergency = (from a in mdb.mtOpFunctions where a.Username == HttpContext.User.Identity.Name.ToUpper() && a.Program == "OrderScheduler" && a.ProgramFunction == "SaveSchedule" select a).ToList();
                if (Emergency.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult SaveSchedule(string details)
        {
            try
            {
                List<mtDispatchPlan> myDeserializedObjList = (List<mtDispatchPlan>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<mtDispatchPlan>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var check = (from a in wdb.mtDispatchPlans where a.DispatchDate == item.DispatchDate && a.DeliveryNo == item.DeliveryNo && a.Customer == item.Customer && a.SalesOrder == item.SalesOrder && a.SalesOrderLine == item.SalesOrderLine select a).FirstOrDefault();
                        if (check == null)
                        {
                            //Add item to schedule
                            mtDispatchPlan obj = new mtDispatchPlan();
                            obj.DispatchDate = item.DispatchDate;
                            obj.DeliveryNo = item.DeliveryNo;
                            obj.Customer = item.Customer;
                            obj.SalesOrder = item.SalesOrder;
                            obj.SalesOrderLine = item.SalesOrderLine;
                            obj.MStockCode = item.MStockCode;
                            obj.MStockDesc = item.MStockDesc;
                            obj.Size = item.Size;
                            obj.MOrderQty = item.MOrderQty;
                            obj.MBackOrderQty = item.MBackOrderQty;
                            obj.MQtyToDispatch = item.MQtyToDispatch;
                            obj.Transporter = item.Transporter;
                            obj.VehicleCapacity = item.VehicleCapacity;
                            obj.Picker = item.Picker;
                            obj.Status = item.Status;
                            wdb.Entry(obj).State = System.Data.EntityState.Added;

                        }
                        else
                        {
                            //check.StockDays = item.StockDays;
                            //check.DateSaved = DateTime.Now;
                            //check.Username = HttpContext.User.Identity.Name.ToUpper();
                            //wdb.Entry(check).State = System.Data.EntityState.Modified;
                            //wdb.SaveChanges();
                        }
                    }

                    wdb.SaveChanges();
                    return Json("Schedule saved!", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No data found!", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadData")]
        public ActionResult LoadData(DispatchPlannerViewModel model)
        {
            ViewBag.CanSaveSchedule = CanSaveSchedule();
            try
            {
                ModelState.Clear();
                model.TruckList = model.SaveTL;
                var dateTime = Convert.ToDateTime(model.DispatchDate.ToString("yyyy-MM-dd"));
                model.Plans = (from a in wdb.mtDispatchPlans where a.DispatchDate == dateTime select a).ToList();
                ViewBag.PlanNo = (from a in wdb.mtDispatchPlans where a.DispatchDate == dateTime select a.DeliveryNo).Distinct().ToList();
                ViewBag.Capacity = model.Capacity;
                model.OrderPlans = new List<mt_DispatchPlanGetOrders_Result>();
                foreach (var item in model.Plans)
                {
                    if (item.DeliveryNo == Convert.ToInt32(model.DeliveryNo))
                    {
                        model.OrderPlans.Add(model.OpenOrders.Find(x => (x.CustCode == item.Customer) && (x.SalesOrder == item.SalesOrder) && (x.SalesOrderLine == item.SalesOrderLine)));
                    }
                }

                if (model.DispatchDate.DayOfYear != System.DateTime.Now.DayOfYear)
                {
                    model.Messages = "";
                }
               
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        [HttpPost]
        public ActionResult DeleteSchedule(string details)
        {
            try
            {
                List<mtDispatchPlan> myDeserializedObjList = (List<mtDispatchPlan>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<mtDispatchPlan>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var check = (from a in wdb.mtDispatchPlans where a.DispatchDate == item.DispatchDate && a.DeliveryNo == item.DeliveryNo && a.Customer == item.Customer && a.SalesOrder == item.SalesOrder && a.SalesOrderLine == item.SalesOrderLine select a).FirstOrDefault();
                        if (check == null)
                        {

                            return Json("Order line not found on schedule!", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            wdb.Entry(check).State = System.Data.EntityState.Deleted;
                            wdb.SaveChanges();

                        }
                    }

                    return Json("Schedule line deleted!", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No data found!", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }


    }
}
