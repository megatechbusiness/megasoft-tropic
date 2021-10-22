using Megasoft2.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class UserGlCodeImportController : Controller
    {
        //
        // GET: /UserGlCodeImport/
        CsvUpload csv = new CsvUpload();
        SysproEntities sdb = new SysproEntities("");
        MegasoftEntities mdb = new MegasoftEntities();


        [CustomAuthorize(Activity: "GlAccessImport")]
        public ActionResult Index()
        {
            return View();
        }



        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Submit")]
        public ActionResult Submit(HttpPostedFileBase nfile)
        {
            try
            {
                // Set up DataTable place holder 
                DataTable dt = new DataTable();

                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();

                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/"), fileName);
                    file.SaveAs(path);

                    dt = csv.csvToDataTable(path, true);

                    if (dt.Rows.Count > 0)
                    {
                        for (int r = 0; r < dt.Rows.Count; r++)
                        {
                            for (int i = 1; i < dt.Columns.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(dt.Rows[r][i].ToString().Trim()))
                                {
                                    string ReportIndex2 = dt.Rows[r][0].ToString().Trim();
                                    
                                    string User = dt.Rows[r][i].ToString().Trim();
                                    
                                    var userCheck = (from a in mdb.mtUsers where a.Username == User select a).ToList();
                                    if(userCheck.Count > 0)
                                    {
                                        sdb.sp_SaveGlCodeAccess(Company, User, ReportIndex2);
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("", "User " + User + " not found.");
                                    }
                                    
                                }
                                
                            }
                        }
                    }

                    if(System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    ModelState.AddModelError("", "Saved Successfully.");
                }

                ////check we have a file
                //if (fileUpload.ContentLength > 0)
                //{
                //    //Workout our file path
                //    string fileName = Path.GetFileName(fileUpload.FileName);
                //    string path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);

                //    //Try and upload
                //    try
                //    {
                //        fileUpload.SaveAs(path);

                //        //Process the CSV file and capture the results to our DataTable place holder

                //    }
                //    catch (Exception ex)
                //    {
                //        //Catch errors
                //        ViewData["Feedback"] = ex.Message;
                //    }
                //}
                //else
                //{
                //    //Catch errors
                //    ViewData["Feedback"] = "Please select a file";
                //}
                
                return View("Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index");
            }
            

        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "Save")]
        public ActionResult Save()
        {
            try
            {
                if (TempData["datatable"] == null)
                {
                    ModelState.AddModelError("", "Something went wrong. No data found!");
                }
                else
                {
                    DataTable dt = (DataTable)TempData["datatable"];
                    if(dt.Rows.Count > 0)                    
                    {
                        for(int r = 1; r < dt.Rows.Count ; r++)
                        {
                            for (int i = 1; i < dt.Columns.Count ; i++)
                            {
                                string GlCode = dt.Rows[r][0].ToString().Trim();
                                string Company = "X";
                                string User = dt.Rows[r][i].ToString().Trim();
                                sdb.sp_SaveGlCodeAccess(Company, User, GlCode);
                            }
                        }
                    }
                }
                return View("Index");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index");
            }
        }

    }
}
