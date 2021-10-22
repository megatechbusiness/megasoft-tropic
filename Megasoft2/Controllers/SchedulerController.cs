using Megasoft2.BusinessLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Megasoft2.Controllers
{
    public class SchedulerController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        //
        // GET: /Scheduler/

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetDepartment()
        {
            try
            {
                Scheduler objSch = new Scheduler();
                DataTable dt = objSch.GetCostCentres();
                if (dt.Rows.Count > 0)
                {
                    List<ListItem> objList = new List<ListItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        objList.Add(new ListItem
                        {
                            Text = row["Description"].ToString().Trim(),
                            Value = row["CostCentre"].ToString().Trim()
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
        public JsonResult GetWorkCentres(string Department)
        {
            try
            {
                Scheduler objSch = new Scheduler();
                DataTable dt = objSch.GetWorkCentres(Department);
                if (dt.Rows.Count > 0)
                {
                    List<ListItem> objList = new List<ListItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        objList.Add(new ListItem
                        {
                            Text = row["WorkCentre"].ToString().Trim(),
                            Value = row["WorkCentre"].ToString().Trim()
                        });
                    }
                    return Json(objList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult SaveSchedule(string details)
        {
            try
            {
                Scheduler Sched = new Scheduler();

                List<Schedule> myDeserializedObjList = (List<Schedule>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<Schedule>));
                string Workcentre = myDeserializedObjList.First().WorkCentre.ToString().Trim();
                string AppUser = HttpContext.User.Identity.Name.ToString().ToUpper();
                string ScheduleGuid = Sched.BackupScheduleGetGuid(Workcentre, AppUser);


                foreach (Schedule item in myDeserializedObjList)
                {
                    if (item.Job.Trim() != "")
                    {
                        int SchedComplete = 0;
                        if (item.ScheduleComplete.ToUpper().Trim() == "TRUE")
                        {
                            SchedComplete = 1;
                        }
                        Sched.SaveSchedule(ScheduleGuid.ToUpper().Trim(), Workcentre, item.Job.Trim(), item.StockCode.Trim(), Convert.ToDecimal(item.QtyToMake), Convert.ToDecimal(item.QtyToPlan), Convert.ToDecimal(item.Produced), Convert.ToDecimal(item.Balance), item.JobDeliveryDate.Trim(), SchedComplete, AppUser);
                    }
                }
                return Json("Schedule Saved Successfully. Click Refresh.",JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class Schedule
        {
            public string AppUser { get; set; }
            public string WorkCentre { get; set; }
            public string Customer { get; set; }
            public string Job { get; set; }
            public string StockCode { get; set; }
            public string MasterJob { get; set; }
            public string Mass { get; set; }
            public decimal QtyToMake { get; set; }
            public string QtyToPlan { get; set; }
            public string Produced { get; set; }
            public string Balance { get; set; }
            public string JobDeliveryDate { get; set; }
            public string ScheduleComplete { get; set; }

        }

        [HttpPost]
        public JsonResult LoadActiveSchedule(string WorkCentre)
        {
            try
            {
                var objList = (from a in wdb.sp_GetActiveSchedule(WorkCentre) select a).ToList();
                return Json(objList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult LoadJobsToPlan(string CostCentre,string WorkCentre,string Date,string Sort)
        {
            try
            {
                var objList = (from a in wdb.sp_Schedule_JobsToSchedule(CostCentre, WorkCentre, Date, Sort) select a).ToList();
                return Json(objList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
