using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace Megasoft2.Controllers
{
    public class LabourPostController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        private LabourPost objLabourPost = new LabourPost();
        //
        // GET: /LabourPost/
        [CustomAuthorize(Activity: "LabourPost")]
        public ActionResult Index()
        {
            ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult GetDepartment()
        {
            try
            {
                LabourPost objLab = new LabourPost();
                DataTable dt = objLab.GetCostCentre();
                if (dt.Rows.Count > 0)
                {
                    List<ListItem> objList = new List<ListItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        objList.Add(new ListItem
                        {
                            Text = row["CostCentre"].ToString().Trim(),
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
        public JsonResult GetTimeCodes(string Department)
        {
            try
            {
                LabourPost objLab = new LabourPost();
                DataTable dt = objLab.GetTimeCode(Department);
                if (dt.Rows.Count > 0)
                {
                    List<ListItem> objList = new List<ListItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        objList.Add(new ListItem
                        {
                            Text = row["Description"].ToString().Trim(),
                            Value = row["TimeCodeID"].ToString().Trim()
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
        public JsonResult GetScrapCodes()
        {
            try
            {
                LabourPost objLab = new LabourPost();
                DataTable dt = objLab.GetScrapCode();
                if (dt.Rows.Count > 0)
                {
                    List<ListItem> objList = new List<ListItem>();
                    foreach (DataRow row in dt.Rows)
                    {
                        objList.Add(new ListItem
                        {
                            Text = row["Description"].ToString().Trim(),
                            Value = row["NonProdScrap"].ToString().Trim()
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

        public class JobDetail
        {
            public string StockCode { get; set; }
            public string StockDescription { get; set; }
            public string JobUom { get; set; }
            public string QtyToMake { get; set; }
            public string JobComplete { get; set; }
        }
        public class Operation
        {
            public string OperationNo { get; set; }
            public string Milestone { get; set; }
            public string Complete { get; set; }


        }
        public class ShiftDetail
        {
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public bool NextDay { get; set; }
        }
        //Get Job Details
        [HttpPost]
        public JsonResult GetJobDetails(string Job, string Department)
        {
            try
            {
                LabourPost objLab = new LabourPost();
                DataTable da = objLab.GetCheckJobs(Department, Job);
                if (da.Rows.Count > 0)
                {
                    DataTable dt = objLab.GetJobDetails(Job);
                    List<JobDetail> objList = new List<JobDetail>();
                    if (dt.Rows.Count > 0)
                    {

                        foreach (DataRow row in dt.Rows)
                        {
                            JobDetail detail = new JobDetail();
                            detail.StockCode = row["StockCode"].ToString();
                            detail.StockDescription = row["StockDescription"].ToString();
                            detail.JobUom = row["StockUom"].ToString();
                            detail.QtyToMake = row["QtyToMake"].ToString();
                            detail.JobComplete = row["Complete"].ToString();
                            objList.Add(detail);
                        }
                        return Json(objList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("", JsonRequestBehavior.AllowGet);
                    }
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
        public JsonResult GetShiftDetails(string ShiftID)
        {
            try
            {
                LabourPost objLab = new LabourPost();
                DataTable dt = objLab.GetShiftDetails(ShiftID);
                List<ShiftDetail> objList = new List<ShiftDetail>();
                if (dt.Rows.Count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        ShiftDetail detail = new ShiftDetail();
                        detail.StartTime = row["StartTime"].ToString();
                        detail.EndTime = row["EndTime"].ToString();
                        detail.NextDay = Convert.ToBoolean(row["NextDay"]);
                        objList.Add(detail);
                    }
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
        //Get Work Centres
        [HttpPost]
        public JsonResult GetWorkCentres(string Department)
        {
            try
            {
                LabourPost objLab = new LabourPost();
                DataTable dt = objLab.GetWorkCentre(Department);
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


        //Get Operation
        [HttpPost]
        public JsonResult GetOperation(string Job, string Dept)
        {
            try
            {
                LabourPost objLab = new LabourPost();
                DataTable dt = objLab.GetOperations(Job, Dept);
                List<Operation> objList = new List<Operation>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Operation ID = new Operation();
                        ID.OperationNo = row["Operation"].ToString();
                        ID.Milestone = row["Milestone"].ToString();
                        objList.Add(ID);
                    }
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

        //Get Last Operation
        [HttpPost]
        public JsonResult GetPreviousOperation(string Job, string Operation)
        {
            try
            {
                LabourPost objLab = new LabourPost();
                DataTable dt = objLab.GetPreviousOperation(Job, Convert.ToInt32(Operation));
                List<Operation> objList = new List<Operation>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Operation ID = new Operation();
                        ID.OperationNo = row["Operation"].ToString();
                        ID.Milestone = row["Milestone"].ToString();
                        ID.Complete = row["Complete"].ToString();
                        objList.Add(ID);
                    }
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

        public class FormDetail
        {
            public string row { get; set; }
            public string Department { get; set; }
            public string Job { get; set; }
            public string JobUom { get; set; }
            public string Date { get; set; }
            public string WorkCentre { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string ElapsedTime { get; set; }
            public string Shift { get; set; }
            public string ProductionQuantity { get; set; }
            public string TimeCode { get; set; }
            public string StockCode { get; set; }
            public string Operation { get; set; }
            public string CloseJob { get; set; }
            public string Reference { get; set; }

        }

        [HttpPost]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public JsonResult SaveForm(string details)
        {
            try
            {
                LabourPost objLb = new LabourPost();
                List<FormDetail> myDeserializedObjList = (List<FormDetail>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<FormDetail>));
                SysproCore objSyspro = new SysproCore();
                string Username = System.Web.HttpContext.Current.User.Identity.Name.ToString().ToUpper();
                string Date = "";

                string Guid = objSyspro.SysproLogin(Username);

                DataTable dt = new DataTable();
                dt.Columns.Add("row", typeof(string));
                dt.Columns.Add("Job", typeof(string));
                dt.Columns.Add("StockCode", typeof(string));
                dt.Columns.Add("Department", typeof(string));
                dt.Columns.Add("WorkCentre", typeof(string));
                dt.Columns.Add("StartTime", typeof(string));
                dt.Columns.Add("EndTime", typeof(string));
                dt.Columns.Add("ElapsedTime", typeof(string));
                dt.Columns.Add("ProductionMass", typeof(string));
                dt.Columns.Add("JobUom", typeof(string));
                dt.Columns.Add("Operation", typeof(string));
                dt.Columns.Add("EntryDate", typeof(string));
                dt.Columns.Add("TimeCodeID", typeof(string));
                dt.Columns.Add("Operator", typeof(string));
                dt.Columns.Add("Shift", typeof(string));
                dt.Columns.Add("CloseJob", typeof(string));
                dt.Columns.Add("Reference", typeof(string));
                dt.Columns.Add("TimeCodeTrnType", typeof(string));

                if (objLb.CheckIfClosed(myDeserializedObjList.First().Date.Trim(), myDeserializedObjList.First().Department.Trim()) == false)
                {
                    if (myDeserializedObjList.First().Operation != "")
                    {
                        var TimeCodes = wdb.sp_GetTimeCodes(myDeserializedObjList.First().Department).ToList();
                        foreach (FormDetail item in myDeserializedObjList)
                        {
                            if (Date == "")
                            {
                                Date = item.Date;
                            }
                            var TrnType = (from a in TimeCodes where a.TimeCodeID == item.TimeCode select a.TrnType).FirstOrDefault();
                            dt.Rows.Add(new Object[] { item.row, item.Job, item.StockCode, item.Department, item.WorkCentre, item.StartTime, item.EndTime, item.ElapsedTime, item.ProductionQuantity, item.JobUom, item.Operation, item.Date.ToString(), item.TimeCode, Username, item.Shift, item.CloseJob, item.Reference, TrnType });

                        }
                        decimal JobTime = dt.AsEnumerable().Sum(r => Convert.ToDecimal(r.Field<string>("ElapsedTime")));
                        decimal JobTimePosted = objLb.CheckJobTime(myDeserializedObjList.First().WorkCentre, myDeserializedObjList.First().Date);
                        decimal Scrap = ds.AsEnumerable().Sum(r => Convert.ToDecimal(r.Field<string>("ScrapAmount")));
                        decimal TotalJobTime = JobTimePosted + JobTime;
                        decimal Capacity = objLb.Capacity(myDeserializedObjList.First().WorkCentre, myDeserializedObjList.First().Date);

                        if (Capacity > 0)
                        {
                            if (Math.Round(TotalJobTime, 2) <= Math.Round(Capacity, 2))
                            {

                                string XmlOut = objSyspro.SysproPost(Guid, objLb.BuildLabourPostParameter(Date), objLb.BuildLabourPostDocument(dt, ds), "WIPTLP");
                                objSyspro.SysproLogoff(Guid);
                                string XmlErrors = objSyspro.GetXmlErrors(XmlOut);

                                if (string.IsNullOrEmpty(XmlErrors))
                                {
                                    //Save To Table
                                    objLb.SaveLabour(dt, ds);
                                    return Json("Posted Successfully.", JsonRequestBehavior.AllowGet);
                                }
                                return Json(XmlErrors + " " + objLb.BuildLabourPostDocument(dt, ds), JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                decimal remainder = Capacity - JobTimePosted;
                                return Json("You have exceeded the daily limit of " + Math.Round(Capacity, 2) + " hours for this Work Centre.Total Hours for this Work Centre today is " + Math.Round(JobTimePosted, 2) + ", You have " + Math.Round(remainder, 2) + " hours remaining.Total Job Time is '" + Math.Round(TotalJobTime) + "' ", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("No machine capacity available for this day. Contact Administrator.", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else return Json("Operation could not be found for Department.", JsonRequestBehavior.AllowGet);
                }
                else return Json("Posting for this Department is Closed. Contact administrator", JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public class ScrapDetail
        {
            public string row { get; set; }
            public string ScrapCode { get; set; }
            public string ScrapAmount { get; set; }
        }
        public static DataTable ds
        {
            get;
            set;
        }

        [HttpPost]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public JsonResult SaveScrap(string details)
        {
            try
            {
                LabourPost objLb = new LabourPost();
                List<ScrapDetail> myDeserializedObjList = (List<ScrapDetail>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<ScrapDetail>));

                DataTable dsr = new DataTable();
                dsr.Columns.Add("row");
                dsr.Columns.Add("ScrapCode");
                dsr.Columns.Add("ScrapAmount");

                foreach (ScrapDetail item in myDeserializedObjList)
                {
                    DataRow dr = dsr.NewRow();
                    dr["row"] = Convert.ToInt32(item.row);
                    dr["ScrapCode"] = item.ScrapCode;
                    dr["ScrapAmount"] = Convert.ToDecimal(item.ScrapAmount);
                    dsr.Rows.Add(dr);
                }
                ds = dsr;
                return Json("Saved Successfully", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomAuthorize(Activity: "LabourPostReversal")]
        public ActionResult LabourPostReversal()
        {
            ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
            return View();
        }
        [CustomAuthorize(Activity: "LabourPostControl")]
        public ActionResult LabourPostControl()
        {
            return View();
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadGrid")]
        public ActionResult LoadCostCentreGrid(LabourPostViewModel model)
        {
            ModelState.Clear();
            DateTime DateToSearch = Convert.ToDateTime(model.Date);
            model.CostCentres = wdb.sp_GetCostCentreControl(DateToSearch).ToList();
            return View("LabourPostControl", model);
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SaveGrid")]
        public ActionResult SaveCostCentreGrid(LabourPostViewModel model)
        {
            try
            {
                ModelState.Clear();
                foreach (var item in model.CostCentres)
                {
                    DateTime DateToSave = Convert.ToDateTime(model.Date);
                    wdb.sp_SaveCostCentreControl(item.CostCentre, DateToSave, item.Status);
                }
                ModelState.AddModelError("", "Saved Successfully!");
                return View("LabourPostControl", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error Saving: " + ex.Message);
                return View("LabourPostControl", model);
            }

        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadJobs")]
        public ActionResult GetReversalJobs(LabourPostViewModel model)
        {
            try
            {
                ModelState.Clear();
                var Job = wdb.sp_GetReversalJobs(model.WorkCentre, model.Date, model.Shift).ToList();
                ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                if (Job != null)
                {
                    ViewBag.JobList = (from a in Job select new { Text = a.Job, Value = a.Guid }).ToList();
                    return View("LabourPostReversal", model);
                }
                else
                {
                    ModelState.AddModelError("", "No Jobs found.");
                    return View("LabourPostReversal", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.ToString());
                return View("LabourPostReversal", model);
            }
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "ReversalJobInfo")]
        public ActionResult GetReversalJobInfo(LabourPostViewModel model)
        {
            try
            {
                ModelState.Clear();
                Guid guid = new Guid(model.Job);
                var JobInfo = wdb.sp_GetReversalJobsInfo(guid).ToList();
                var JobList = wdb.sp_GetReversalJobs(model.WorkCentre, model.Date, model.Shift).ToList();
                ViewBag.JobList = (from a in JobList select new { Text = a.Job, Value = a.Guid }).ToList();
                if (JobInfo != null)
                {
                    model.Job = Convert.ToString(JobInfo.FirstOrDefault().Guid);
                    model.ProductionQty = JobInfo.FirstOrDefault().ProductionQuantity;
                    model.TotalScrap = JobInfo.FirstOrDefault().TotalScrap;
                    model.JobDescription = JobInfo.FirstOrDefault().StockDescription;
                    model.JobLines = JobInfo;
                    ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                    return View("LabourPostReversal", model);
                }
                else
                {
                    ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                    ModelState.AddModelError("", "Job Details Not Found.");
                    return View("LabourPostReversal", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                ModelState.AddModelError("", "Error: " + ex.ToString());
                return View("LabourPostReversal", model);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "PostReversalJob")]
        public ActionResult PostReversalJob(LabourPostViewModel model)
        {
            try
            {
                ModelState.Clear();
                ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                string Username = System.Web.HttpContext.Current.User.Identity.Name.ToString().ToUpper();
                SysproCore objSyspro = new SysproCore();
                string Guid = objSyspro.SysproLogin(Username);
                string Date = string.Empty;
                string Job = string.Empty;
                string WorkCentre = string.Empty;
                string Shift = string.Empty;
                string Quantity = string.Empty;

                DataTable dt = new DataTable();
                DataTable ds = new DataTable();

                string GuidToSearch = model.Job;
                Guid guid = new Guid(model.Job);
                dt = ToDataTable(wdb.sp_GetReversalJobsInfo(guid).ToList());
                // var Department = dt.AsEnumerable().Select(dr => dr["Department"].ToString()).FirstOrDefault();
                var Department = dt.Rows[0]["CostCentre"].ToString();
                var TimeCodes = wdb.sp_GetTimeCodes(Department).ToList();
                foreach (DataRow r in dt.AsEnumerable())
                {
                    r["TimeCodeTrnType"] = (from a in TimeCodes where a.TimeCodeID == r["TimeCodeID"].ToString() select a.TrnType).FirstOrDefault();
                }
                ds = ToDataTable(wdb.sp_GetReversalScrap(guid).ToList());
                Date = dt.Rows[0]["EntryDate"].ToString();
                DateTime start = Convert.ToDateTime(Date);
                Date = start.ToString("yyyy/MM/dd");

                DateTime UserEntryDate = Convert.ToDateTime(model.Date);

                //check CostCentre and Date combo
                var CostCentre = (from a in wdb.BomWorkCentres where a.WorkCentre == model.WorkCentre select a.CostCentre).FirstOrDefault();
                var LabourPostControlCheck = (from a in wdb.mtLabourPostControls
                                   where (a.CostCentre == CostCentre && a.Date == UserEntryDate)
                                   select a).FirstOrDefault();

                //do not allow post if already closed
                if (LabourPostControlCheck != null)
                {
                    if (LabourPostControlCheck.Status == true)
                    {
                        ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                        ModelState.AddModelError("", "Posting for this Department is Closed. Contact administrator.");
                        return View("LabourPostReversal", model);
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    string XmlOut = objSyspro.SysproPost(Guid, objLabourPost.BuildLabourPostParameter(Date), objLabourPost.BuildLabourPostReversalDocument(dt, ds), "WIPTLP");
                    objSyspro.SysproLogoff(Guid);
                    string XmlErrors = objSyspro.GetXmlErrors(XmlOut);

                    if (string.IsNullOrEmpty(XmlErrors))
                    {
                        LabourPostViewModel newmodel = new LabourPostViewModel();
                        try
                        {
                            objLabourPost.SaveLabourReversal(dt, ds, Username);
                        }
                        catch (Exception ex)
                        {
                            ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                            ModelState.AddModelError("", "Reversal Successful, Error Saving: " + ex.Message);
                            return View("LabourPostReversal", newmodel);
                        }

                        ModelState.AddModelError("", "Reversal Successful.");
                        return View("LabourPostReversal", newmodel);
                    }
                    else
                    {
                        ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                        ModelState.AddModelError("", "Posting Error:" + XmlErrors);
                        return View("LabourPostReversal", model);
                    }
                }
                else
                {
                    ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                    ModelState.AddModelError("", "Error Posting: Failed to get job details.");
                    return View("LabourPostReversal", model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ShiftList = (from a in wdb.mtShifts select new { Text = a.Shift, Value = a.ShiftID }).ToList();
                ModelState.AddModelError("", "Error Posting: " + ex.Message);
                return View("LabourPostReversal", model);
            }
        }
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);

                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;

        }
    }
}
