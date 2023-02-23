using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using System.Security.Cryptography;
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
                model.TruckList = new List<string> { "" };
                model.Capacity = new List<string> { "" };
                foreach (var truck in transporter)
                {
                    model.TruckList.Add(truck.Name);
                    model.Capacity.Add(truck.Capacity.ToString());
                }
                model.TruckList.Add("!");
                model.Capacity.Add("14");
                ViewBag.TruckList = model.TruckList;
                ViewBag.Capacity = model.Capacity;
                model.DispatchDate = dateTime;
                model.DeliveryNo = "1";
                LoadData(model);
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
        //[MultipleButton(Name = "action", Argument = "SaveSchedule")]
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
                            wdb.SaveChanges();

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
                model.OrderPlans = new List<mt_DispatchPlanGetOrders_Result>();
                foreach (var item in model.Plans)
                {
                    if( (item.DispatchDate == model.DispatchDate) && (item.DeliveryNo == Convert.ToInt32(model.DeliveryNo)) )
                    {
                        model.OrderPlans.Add(model.OpenOrders.Find(x => (x.CustCode == item.Customer) && (x.SalesOrder == item.SalesOrder) && (x.SalesOrderLine == item.SalesOrderLine)));
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

    }
}
