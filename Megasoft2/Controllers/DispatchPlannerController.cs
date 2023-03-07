using DotNetOpenAuth.Messaging;
using Megasoft2.ViewModel;
using Remotion.Data.Linq.Clauses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;

namespace Megasoft2.Controllers
{
    public class DispatchPlannerController : Controller
    {
        //
        // GET: /DispatchPlanner/
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();

        [CustomAuthorize("DispatchPlanner")]
        public ActionResult Index()
        {
            DispatchPlannerViewModel model = new DispatchPlannerViewModel();
            ViewBag.CanSaveSchedule = CanSaveSchedule();
            try
            {

                System.DateTime dateTime = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd"));
                model.Plans = (from a in wdb.mtDispatchPlans where a.DispatchDate == dateTime && a.DeliveryNo == 1 select a).ToList();
                var transporter = (from a in wdb.ApSuppliers join b in wdb.mtTransporters on a.Supplier equals b.Transporter select a.SupplierName).ToList();

                model.TruckList = new List<string> { "Select Transporter" };
                foreach (var sup in transporter)
                {
                    model.TruckList.Add(sup);
                }
                model.SaveTL = model.TruckList;

                model.DispatchDate = DateTime.Now;
                model.DeliveryNo = 1;

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


        [HttpPost]
        [CustomAuthorize(Activity: "DispatchPlanner")]
        public ActionResult LoadData(DispatchPlannerViewModel model)
        {
            ViewBag.CanSaveSchedule = CanSaveSchedule();
            try
            {
                ModelState.Clear();
                model.OpenOrders = (from a in wdb.mt_DispatchPlanGetOrders() select a).ToList();
                model.TruckList = model.SaveTL;

                var dateTime = Convert.ToDateTime(model.DispatchDate.ToString("yyyy-MM-dd"));
                model.Plans = (from a in wdb.mtDispatchPlans where a.DispatchDate == dateTime && a.DeliveryNo == model.DeliveryNo select a).ToList();
                ViewBag.PlanNo = (from a in wdb.mtDispatchPlans where a.DispatchDate == dateTime select a.DeliveryNo).Distinct().ToList();

                model.OrderPlans = new List<mt_DispatchPlanGetOrders_Result>();
                foreach (var item in model.Plans)
                {
                    if (item.DeliveryNo == model.DeliveryNo)
                    {
                        model.OrderPlans.Add(model.OpenOrders.Find(x => (x.CustCode == item.Customer) && (x.SalesOrder == item.SalesOrder) && (x.SalesOrderLine == item.SalesOrderLine)));
                    }
                }

                if (model.DispatchDate.DayOfYear != DateTime.Now.DayOfYear)
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
        [CustomAuthorize(Activity: "DispatchPlanner")]

        public ActionResult SaveSchedule(string details, decimal mass)
        {
            try
            {
                List<mtDispatchPlan> myDeserializedObjList = (List<mtDispatchPlan>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<mtDispatchPlan>));
                if (myDeserializedObjList.Count > 0)
                {
                    if (mass > myDeserializedObjList[0].VehicleCapacity)
                    {
                        return Json("Cannot Schedule Dispatch!\nMass Balance exceeds Vehicle Capacity", JsonRequestBehavior.AllowGet);
                    }
                    foreach (var item in myDeserializedObjList)
                    {
                        var dateTime = Convert.ToDateTime(item.DispatchDate.ToString("yyyy-MM-dd"));

                        var check = (from a in wdb.mtDispatchPlans where a.DispatchDate == dateTime && a.DeliveryNo == item.DeliveryNo && a.Customer == item.Customer && a.SalesOrder == item.SalesOrder && a.SalesOrderLine == item.SalesOrderLine select a).FirstOrDefault();
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
                            obj.PickComplete = "N";
                            obj.DatePickComplete = DateTime.MinValue;
                            wdb.Entry(obj).State = System.Data.EntityState.Added;
                            wdb.SaveChanges();
                        }
                        else
                        {
                            check.Transporter = item.Transporter;
                            check.VehicleCapacity = item.VehicleCapacity;
                            wdb.Entry(check).State = System.Data.EntityState.Modified;
                            wdb.SaveChanges();
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
        [CustomAuthorize(Activity: "DispatchPlanner")]

        public ActionResult DeleteSchedule(string details)
        {
            try
            {
                List<mtDispatchPlan> myDeserializedObjList = (List<mtDispatchPlan>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<mtDispatchPlan>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        item.DispatchDate = Convert.ToDateTime(item.DispatchDate.ToString("yyyy-MM-dd"));
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

        [CustomAuthorize("DispatchPlanner")]
        public ActionResult PickerSelect(string date, string delNo)
        {
            ViewBag.DisDate = date;
            ViewBag.DelNo = delNo;
            ViewBag.PickersList = new List<string> { "" };
            ViewBag.PickersList.AddRange((from a in mdb.mtUsers where a.Picker == true select a.Username).ToList());
            return PartialView();
        }

        [HttpPost]
        public JsonResult SavePicker(string date, int num, string picker)
        {
            try
            {
                if (picker == "") return Json("Please select a Picker to Release!", JsonRequestBehavior.AllowGet);

                var dateTime = Convert.ToDateTime(Convert.ToDateTime(date).ToString("yyyy-MM-dd"));
                var check = (from a in wdb.mtDispatchPlans where a.DispatchDate == dateTime && a.DeliveryNo == num select a).ToList();
                if (check.Count == 0)
                {
                    return Json("Please Save plan before Releasing", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    foreach (var item in check)
                    {
                        item.Picker = picker;
                        item.Status = "1";
                        wdb.Entry(item).State = System.Data.EntityState.Modified;
                        wdb.SaveChanges();
                    }

                    return Json("Plan Saved", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
