using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Megasoft2.Controllers
{
    public class ReportController : Controller
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        //
        // GET: /Report/
        clsData Data = new clsData();
        public ActionResult Index()
        {
            ViewBag.HtmlStr = BuildReportHtmlGroups();
            return View();
        }
        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }
        public class Reports
        {
            public string Report { get; set; }
            public string ReportPath { get; set; }
            public string DisplayName { get; set; }
            public string ReportGroup { get; set; }
        }
        public string BuildReportHtmlGroups()
        {
            try
            {
                string Username = HttpContext.User.Identity.Name.ToUpper();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                StringBuilder strHtml = new StringBuilder();
                DataTable dt = ToDataTable(wdb.sp_GetMegasoftReportsByUser(Username,Company).ToList());
                if (dt.Rows.Count > 0)
                {
                    List<Reports> objList = new List<Reports>();
                    objList = (from DataRow row in dt.Rows
                               select new Reports
                               {
                                   Report = row["Report"].ToString().Trim(),
                                   DisplayName = row["DisplayName"].ToString().Trim(),
                                   ReportGroup = row["ReportGroup"].ToString().Trim()

                               }).ToList();

                    var Groups = (from a in objList select a.ReportGroup).Distinct().ToList();
                    //strHtml.Append("'");
                    int j = 1;
                    var Reports = wdb.sp_GetMegasoftReportsByUser(Username, Company).ToList();
                    foreach (var gr in Groups)
                    {

                        strHtml.Append("<div class=\"form-horizontal row\"><div class=\"col-sm-3\"></div><div class=\"col-sm-6\">");
                        strHtml.Append("<div class=\"panel panel-primary\"><div class=\"panel-heading\"><h3 class=\"panel-title\"><a data-toggle=\"collapse\" data-target=\"#collapse" + j + "\" href=\"#collapse" + j + "\" class=\"\">" + gr + "</a></h3></div><div class=\"panel-body\">");
                        strHtml.Append("<div id=\"collapse" + j + "\" class=\"panel-body collapse\">");

                        int i = 0;
                       // dt = ToDataTable(wdb.sp_GetReportsByUserAndGroup(Username, gr).ToList());
                       // dt = GetReportsByGroup(HttpContext.User.Identity.Name, gr);
                       dt = ToDataTable(Reports.Where(r => r.ReportGroup.Contains(gr)).ToList());
                        if (dt.Rows.Count < 2)
                        {
                            strHtml.Append("<div class=\"centered text-center\" id=\"div0\" runat=\"server\" visible=\"false\">");
                            strHtml.Append("<button type=\"button\" id=\"btn0\" class=\"btn btn-info\" style=\"width:286px\" onclick=\"ShowReport('" + dt.Rows[0]["Report"].ToString().Trim() + "');\">");
                            strHtml.Append("<span class=\"glyphicon glyphicon-list-alt\"></span> " + dt.Rows[0]["DisplayName"].ToString().Trim());
                            strHtml.Append("</button>");
                            strHtml.Append("</div><BR/>");
                        }
                        else
                        {
                            int NumOfLoops = 0;
                            bool IsOddN = false;
                            if (IsOdd(dt.Rows.Count))
                            {
                                NumOfLoops = (dt.Rows.Count - 1) / 2;
                                IsOddN = true;
                            }
                            else
                            {
                                NumOfLoops = dt.Rows.Count / 2;
                            }


                            while (NumOfLoops > 0)
                            {
                                strHtml.Append("<div class=\"centered text-center\" id=\"div" + i + "\" runat=\"server\" visible=\"false\">");

                                strHtml.Append("<button type=\"button\" id=\"btn" + i + "\" class=\"btn btn-info\" style=\"width:286px\" onclick=\"ShowReport('" + dt.Rows[i]["Report"].ToString().Trim() + "');\">");
                                strHtml.Append("<span class=\"glyphicon glyphicon-list-alt\"></span> " + dt.Rows[i]["DisplayName"].ToString().Trim());
                                strHtml.Append("</button>&nbsp&nbsp&nbsp&nbsp");
                                i++;
                                strHtml.Append("<button type=\"button\" id=\"btn1\" class=\"btn btn-info\" style=\"width:286px\" onclick=\"ShowReport('" + dt.Rows[i]["Report"].ToString().Trim() + "');\">");
                                strHtml.Append("<span class=\"glyphicon glyphicon-list-alt\"></span> " + dt.Rows[i]["DisplayName"].ToString().Trim());
                                strHtml.Append("</button>");
                                i++;

                                strHtml.Append("</div><BR/>");
                                NumOfLoops--;
                            }
                            if (IsOddN)
                            {
                                strHtml.Append("<div class=\"centered text-center\" id=\"div" + i + "\" runat=\"server\" visible=\"false\">");
                                strHtml.Append("<button type=\"button\" id=\"btn" + i + "\" class=\"btn btn-info\" style=\"width:286px\" onclick=\"ShowReport('" + dt.Rows[i]["Report"].ToString().Trim() + "');\">");
                                strHtml.Append("<span class=\"glyphicon glyphicon-list-alt\"></span> " + dt.Rows[i]["DisplayName"].ToString().Trim());
                                strHtml.Append("</button>");
                                strHtml.Append("</div><BR/>");
                            }
                        }
                        j++;
                        strHtml.Append("</div>");

                        strHtml.Append("</div></div></div><div class=\"col-sm-3\"></div></div>");
                    }
                    // strHtml.Append("'");
               return strHtml.ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
        [CustomAuthorize(Activity: "ReportSetup")]
        public ActionResult ReportSetup()
        {
            return View(wdb.mtReportMasters.AsEnumerable());
        }
        [CustomAuthorize(Activity: "ReportSetup")]
        public ActionResult Create(string Program,string Report)
        {
            try
            {
                ViewBag.ReportGroup = (from a in wdb.mtReportGroups select new { Value = a.GroupName, Text = a.GroupName }).ToList();
                ReportViewModel report = new ReportViewModel();
                report.ReportMaster = wdb.mtReportMasters.Find(Program,Report);
                return View(report);
            }
            catch (Exception ex)
            {
                ViewBag.ReportGroup = (from a in wdb.mtReportGroups select new { Value = a.GroupName, Text = a.GroupName }).ToList();
                ModelState.AddModelError("", "Error Loading report details:  " + ex.InnerException.Message);
                return View();
            }
        }


        [CustomAuthorize(Activity: "ReportSetup")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReportViewModel ReportModel)
        {

            ModelState.Clear();
            ViewBag.ReportGroup = (from a in wdb.mtReportGroups select new { Value = a.GroupName, Text = a.GroupName }).ToList();
            if (ModelState.IsValid)
            {
                //Check update or add
                var checkReport = (from a in wdb.mtReportMasters.AsEnumerable() where a.Program == ReportModel.ReportMaster.Program && a.Report == ReportModel.ReportMaster.Report select a).ToList();
                if (checkReport.Count > 0)
                {
                    try
                    {
                        var v = wdb.mtReportMasters.Find(ReportModel.ReportMaster.Program, ReportModel.ReportMaster.Report);
                        mtReportMaster obj = new mtReportMaster()
                        {
                            Program= ReportModel.ReportMaster.Program,
                            Report= ReportModel.ReportMaster.Report,
                            DisplayName=ReportModel.ReportMaster.DisplayName,
                            ReportGroup= ReportModel.ReportMaster.ReportGroup,
                            ReportPath= ReportModel.ReportMaster.ReportPath
                        };
                        wdb.Entry(v).CurrentValues.SetValues(obj);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Updated Successfully.");
                        return View(ReportModel);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error Updating: " + ex.InnerException);
                        return View(ReportModel);
                    }

                }
                else
                {
                    //Add New Entry
                    try
                    {
                        wdb.mtReportMasters.Add(ReportModel.ReportMaster);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Saved Successfully.");
                        return View(ReportModel);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error Saving: " + ex.InnerException);
                        return View(ReportModel);
                    }
                }
            }
            else
            { return View(ReportModel); }
        }
        public ActionResult Delete(string Program,string Report)
        {
            mtReportMaster report = wdb.mtReportMasters.Find(Program, Report);
            if (report == null)
            {
                return HttpNotFound();
            }
            return PartialView(report);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Program, string Report)
        {
            mtReportMaster report = wdb.mtReportMasters.Find(Program, Report);
            wdb.mtReportMasters.Remove(report);
            wdb.SaveChanges();
            return RedirectToAction("ReportSetup");
        }
        [CustomAuthorize(Activity: "ReportGroupSetup")]
        public ActionResult ReportGroupSetup()
        {
            return View(wdb.mtReportGroups.AsEnumerable());
        }
        [CustomAuthorize(Activity: "ReportGroupSetup")]
        public ActionResult CreateReportGroup(string GroupId)
        {
            try
            {
                ViewBag.ReportGroup = (from a in wdb.mtReportGroups select new { Value = a.GroupName, Text = a.GroupName }).ToList();
                ReportViewModel report = new ReportViewModel();
                if (!string.IsNullOrEmpty(GroupId))
                {
                    int Id = Convert.ToInt32(GroupId);
                    report.ReportGroup = wdb.mtReportGroups.Find(Id);
                }

                return View(report);
            }
            catch (Exception ex)
            {
                ViewBag.ReportGroup = (from a in wdb.mtReportGroups select new { Value = a.GroupName, Text = a.GroupName }).ToList();
                ModelState.AddModelError("", "Error Loading report group details:  " + ex.InnerException.Message);
                return View();
            }
        }
        [CustomAuthorize(Activity: "ReportGroupSetup")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReportGroup(ReportViewModel ReportModel)
        {

            ModelState.Clear();
            ViewBag.ReportGroup = (from a in wdb.mtReportGroups select new { Value = a.GroupName, Text = a.GroupName }).ToList();
            if (ModelState.IsValid)
            {
                //Check update or add
                var checkReport = (from a in wdb.mtReportGroups.AsEnumerable() where a.GroupId == ReportModel.ReportGroup.GroupId select a).ToList();
                if (checkReport.Count > 0)
                {
                    try
                    {
                        var v = wdb.mtReportGroups.Find(ReportModel.ReportGroup.GroupId);
                        wdb.Entry(v).CurrentValues.SetValues(ReportModel.ReportGroup);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Updated Successfully.");
                        return View(ReportModel);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error Updating: " + ex.InnerException);
                        return View(ReportModel);
                    }

                }
                else
                {
                    //Add New Entry
                    try
                    {
                       // var countGroup = (from a in wdb.mtReportGroups.AsEnumerable()  select a).ToList();
                     //   ReportModel.ReportGroup.GroupId = countGroup.Count + 1;
                        wdb.mtReportGroups.Add(ReportModel.ReportGroup);
                        wdb.SaveChanges();
                        ModelState.AddModelError("", "Saved Successfully.");
                        return View(ReportModel);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error Saving: " + ex.InnerException);
                        return View(ReportModel);
                    }
                }
            }
            else
            { return View(ReportModel); }
        }
        public ActionResult DeleteReportGroup(int GroupId)
        {
            mtReportGroup report = wdb.mtReportGroups.Find(GroupId);
            if (report == null)
            {
                return HttpNotFound();
            }
            return PartialView(report);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DeleteGroup")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReportGroupConfirmed(int GroupId)
        {
            mtReportGroup report = wdb.mtReportGroups.Find(GroupId);
            wdb.mtReportGroups.Remove(report);
            wdb.SaveChanges();
            return RedirectToAction("ReportGroupSetup");
        }


    }
}
