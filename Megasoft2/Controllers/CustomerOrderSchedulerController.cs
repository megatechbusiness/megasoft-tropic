using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class CustomerOrderSchedulerController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();

        [CustomAuthorize(Activity: "CustomerOrderScheduler")]
        public ActionResult Index()
        {
            CustomerOrderScheduleViewModel model = new CustomerOrderScheduleViewModel();
            ViewBag.CanSaveSchedule = CanSaveSchedule();
            return View(model);
        }


        [CustomAuthorize(Activity: "CustomerOrderScheduler")]
        public ActionResult ActiveSchedule()
        {
            CustomerOrderScheduleViewModel model = new CustomerOrderScheduleViewModel();
            var result = (from a in wdb.sp_GetSorOrderSchedule() select a).ToList();
            model.Planned = (from a in result where a.ScheduleItem == "Y" select a).OrderBy(a => a.CustCode).ToList();
            return View("ActiveSchedule", model);
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
        [MultipleButton(Name = "action", Argument = "LoadData")]
        public ActionResult LoadData(CustomerOrderScheduleViewModel model)
        {
            ViewBag.CanSaveSchedule = CanSaveSchedule();
            try
            {
                ModelState.Clear();

                var result = (from a in wdb.sp_GetSorOrderSchedule() where a.CustCode == model.Customer select a).ToList();
                model.Unplanned = (from a in result where a.ScheduleItem == "N" select a).ToList();
                model.Planned = (from a in result where a.ScheduleItem == "Y" select a).ToList();
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }
        }

        public JsonResult SalesOrderList()
        {
            var result = wdb.sp_GetSorOrderSchedule().Take(2).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Comments(string SalesOrder, decimal Line)
        {
            ViewBag.SalesOrder = SalesOrder;
            ViewBag.Line = Line;
            return PartialView();
        }

        public JsonResult CommentsData(string SalesOrder, decimal Line)
        {
            var result = wdb.sp_GetSorOrderSchedulerComments(SalesOrder, Line).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveComment(string SalesOrder, decimal Line, string Comment)
        {
            mtSorSchedulerComment _comm = new mtSorSchedulerComment();
            _comm.SalesOrder = SalesOrder;
            _comm.Line = Line;
            _comm.Comment = Comment;
            _comm.Username = HttpContext.User.Identity.Name.ToUpper();
            _comm.TrnDate = DateTime.Now;
            wdb.mtSorSchedulerComments.Add(_comm);
            wdb.SaveChanges();
            return Json("Saved", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveSchedule(string details)
        {
            try
            {
                List<sp_GetSorOrderSchedule_Result> myDeserializedObjList = (List<sp_GetSorOrderSchedule_Result>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<sp_GetSorOrderSchedule_Result>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var check = (from a in wdb.mtSorSchedulers where a.SalesOrder == item.SalesOrder && a.Line == item.SalesOrderLine select a).FirstOrDefault();
                        if (check == null)
                        {
                            //Add item to schedule
                            mtSorScheduler obj = new mtSorScheduler();
                            obj.SalesOrder = item.SalesOrder;
                            obj.Line = (decimal)item.SalesOrderLine;
                            obj.StockDays = item.StockDays;
                            obj.DateSaved = DateTime.Now;
                            obj.Username = HttpContext.User.Identity.Name.ToUpper();
                            wdb.Entry(obj).State = System.Data.EntityState.Added;
                            wdb.SaveChanges();

                            mtSorSchedulerComment comm = new mtSorSchedulerComment();
                            comm.SalesOrder = item.SalesOrder;
                            comm.Line = item.SalesOrderLine;
                            comm.Comment = "Sales Order added to schedule.";
                            comm.Username = HttpContext.User.Identity.Name.ToUpper();
                            comm.TrnDate = DateTime.Now;
                            wdb.Entry(comm).State = System.Data.EntityState.Added;
                            wdb.SaveChanges();

                        }
                        else
                        {
                            check.StockDays = item.StockDays;
                            check.DateSaved = DateTime.Now;
                            check.Username = HttpContext.User.Identity.Name.ToUpper();
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
        public ActionResult DeleteSchedule(string details)
        {
            try
            {
                List<sp_GetSorOrderSchedule_Result> myDeserializedObjList = (List<sp_GetSorOrderSchedule_Result>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<sp_GetSorOrderSchedule_Result>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var check = (from a in wdb.mtSorSchedulers where a.SalesOrder == item.SalesOrder && a.Line == item.SalesOrderLine select a).FirstOrDefault();
                        if (check == null)
                        {

                            return Json("Order line not found on schedule!", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            wdb.Entry(check).State = System.Data.EntityState.Deleted;
                            wdb.SaveChanges();

                            mtSorSchedulerComment comm = new mtSorSchedulerComment();
                            comm.SalesOrder = item.SalesOrder;
                            comm.Line = item.SalesOrderLine;
                            comm.Comment = "Sales Order removed from schedule.";
                            comm.Username = HttpContext.User.Identity.Name.ToUpper();
                            comm.TrnDate = DateTime.Now;
                            wdb.Entry(comm).State = System.Data.EntityState.Added;
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


        public ActionResult ExportSchedule(string Customer)
        {
            var model = new CustomerOrderScheduleViewModel();
            List<sp_GetSorOrderSchedule_Result> result;
            if (string.IsNullOrWhiteSpace(Customer))
            {
                result = (from a in wdb.sp_GetSorOrderSchedule() select a).ToList();
            }
            else
            {
                result = (from a in wdb.sp_GetSorOrderSchedule() where a.CustCode == Customer select a).ToList();
            }
            model.Planned = (from a in result where a.ScheduleItem == "Y" select a).ToList();
            return PartialView(model);
        }
    }
}
