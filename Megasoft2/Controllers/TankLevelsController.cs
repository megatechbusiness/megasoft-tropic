using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.Models;
using Megasoft2.BusinessLogic;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;

namespace Megasoft2.Controllers
{
    public class TankLevelsController : Controller
    {
        MegasoftEntities mdb = new MegasoftEntities();

        Adr_LoggingEntities adb = new Adr_LoggingEntities();
        TankLevelsBL BL = new TankLevelsBL();
        Email emailBl = new Email();
        CrystalReportsBL _crystal = new CrystalReportsBL();
        //
        // GET: /TankLevels/
        [CustomAuthorize(Activity: "TankLevels")]
        public ActionResult Index(bool reprint = false)
        {
            try
            {
                               
                var tdv = (TankDataViewModel)TempData["tankDataViewModel"];
                TempData["tankDataViewModel"] = tdv;

                if (tdv != null)
                {
                    if(tdv.lstTankLevels != null)
                    {
                        string Guid;
                        if (reprint == true)
                        {
                            string Blend = tdv.lstTankLevels.FirstOrDefault().BlendNo.ToString();
                            List<mtTankLevelStaging> blend = (from a in mdb.mtTankLevelStagings where a.BlendNo == Blend select a).ToList();
                            Guid = blend.FirstOrDefault().GUID.ToString();
                        }
                        else
                        {
                            Guid = BL.SaveTankData(tdv.lstTankLevels, HttpContext.User.Identity.Name.ToUpper());
                        }
                        tdv.tankData = BL.GetTankLevelsData(Guid);
                        System.Web.HttpContext.Current.Session["Guid"] = Guid;
                    }
                    else
                    {
                        ModelState.AddModelError("", "No tanks selected!"); 
                    }
                }

                return View(tdv);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            
        }

        [CustomAuthorize(Activity: "TankLevels")]
        public ActionResult TankFilter(TankDataViewModel TankList)
        {
            var AllTanks = (from a in mdb.mtTankMasters select new SelectListItem { Value = a.Tank, Text = a.Description }).ToList();
            ViewBag.Tanks = AllTanks;

            ViewBag.BlendNo = "";

            var tdv = (TankDataViewModel)TempData["tankDataViewModel"];
            if(tdv != null)
            {
                foreach (var item in tdv.lstTankLevels)
                {
                    if (string.IsNullOrEmpty(item.AlumSilic))
                    {
                        item.AlumSilic = "";
                    }

                    if(!string.IsNullOrEmpty(item.BlendNo))
                    {
                        ViewBag.BlendNo = item.BlendNo;
                    }
                }
            }
            

            return PartialView(tdv);
        }
        [CustomAuthorize(Activity: "TankLevels")]
        [HttpPost, ActionName("LoadDetails")]
        [ValidateAntiForgeryToken]
        public ActionResult LoadDetails(TankDataViewModel TankList)
        {
            var tdv = new TankDataViewModel();
           
            tdv.lstTankLevels = TankList.lstTankLevels;
            TempData["tankDataViewModel"] = tdv;

            return RedirectToAction("Index", new { reprint = false });
        }

        [CustomAuthorize(Activity: "TankLevels")]
        public ActionResult TankMovement()
        {
            try
            {
                var tdv = (TankMovementsViewModel)TempData["tankMovementsViewModel"];
                TempData["tankMovementsViewModel"] = tdv;

                if (tdv != null)
                {
                    if (tdv.lstTankLevels != null)
                    {
                        string Guid = BL.SaveMovements(tdv.lstTankLevels);
                        tdv.tankData = BL.GetTankMovementsData(Guid);
                        System.Web.HttpContext.Current.Session["Guid"] = Guid;
                    }
                    else
                    {
                        ModelState.AddModelError("", "No tanks selected!");
                    }
                }

                return View(tdv);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [CustomAuthorize(Activity: "TankLevels")]
        public ActionResult MovementsFilter(TankMovementsViewModel TankList)
        {
            var AllTanks = (from a in mdb.mtTankMasters select new SelectListItem { Value = a.Tank, Text = a.Description }).ToList();
            ViewBag.Tanks = AllTanks;

            var tdv = (TankMovementsViewModel)TempData["tankMovementsViewModel"];


            return PartialView(tdv);
        }

        [CustomAuthorize(Activity: "TankLevels")]
        [HttpPost, ActionName("LoadMovements")]
        [ValidateAntiForgeryToken]
        public ActionResult LoadMovements(TankMovementsViewModel TankList)
        {
            var tdv = new TankMovementsViewModel();

            tdv.lstTankLevels = TankList.lstTankLevels;
            TempData["tankMovementsViewModel"] = tdv;

            return RedirectToAction("TankMovement");
        }

        [CustomAuthorize(Activity: "TankLevels")]
        public ActionResult SendEmail(string Program)
        {
            Mail _mail = new Mail();
            _mail.Program = Program;
            return PartialView(_mail);
        }


        [CustomAuthorize(Activity: "TankLevels")]
        [HttpPost, ActionName("SendEmail")]
        [ValidateAntiForgeryToken]
        public ActionResult SendEmail(Mail mail, string Program)
        {
            try
            {
                var settings = (from a in mdb.mtSystemSettings select a).FirstOrDefault();

                string FilePath = settings.ReportExportPath + "\\TankSystem" + "\\" + HttpContext.User.Identity.Name.ToUpper();

                //bool exists = System.IO.Directory.Exists(FilePath);

                //if (!exists)
                //    System.IO.Directory.CreateDirectory(FilePath);

                //DirectoryInfo dir = new DirectoryInfo(FilePath);

                //foreach (FileInfo fi in dir.GetFiles())
                //{
                //    try
                //    {
                //        fi.Delete();
                //    }
                //    catch (Exception)
                //    {
                //        //do nothing. Clean up function can be done at a later stage.
                //    }
                //}

                List<string> Files = new List<string>();

                string Guid = System.Web.HttpContext.Current.Session["Guid"].ToString();
                if (Program == "Movements")
                {
                    Files.Add(_crystal.PrintToPdf(Guid, "Movements", HttpContext.User.Identity.Name.ToUpper(), FilePath));
                }
                else
                {
                    Files.Add(_crystal.PrintToPdf(Guid, "StockSheet", HttpContext.User.Identity.Name.ToUpper(), FilePath));
                    Files.Add(_crystal.PrintToPdf(Guid, "BlendSheet", HttpContext.User.Identity.Name.ToUpper(), FilePath));
                }

                //emailBl.SendEmail(mail, Files);

                //foreach (var item in Files)
                //{
                //    FileInfo file = new FileInfo(item);
                //    if (file.Exists)
                //    {
                //        Response.Clear();

                //        Response.ClearHeaders();

                //        Response.ClearContent();

                //        Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);

                //        Response.AddHeader("Content-Length", file.Length.ToString());

                //        Response.ContentType = "application/pdf";

                //        Response.Flush();

                //        Response.TransmitFile(file.FullName);

                //        Response.End();
                //    }
                //}
                return Json("Email Sent Successfully.", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json("Error : " + ex.StackTrace.ToString() + " --- " + ex.Message + " --- " + ex.InnerException, JsonRequestBehavior.AllowGet);
            }
            
        }

        [CustomAuthorize(Activity: "TankLevels")]
        public ActionResult Reprint()
        {
            return PartialView();
        }


        [CustomAuthorize(Activity: "TankLevels")]
        [HttpPost, ActionName("Reprint")]
        [ValidateAntiForgeryToken]
        public ActionResult Reprint(BlendReprint rep)
        {
            List<TankLevels> tl = new List<TankLevels>();
            tl = (from a in mdb.mtTankLevelStagings
                               where a.BlendNo == rep.BlendNo
                               select new TankLevels
                               {
                                   Tank = a.Tank,
                                   FromDate = (DateTime)a.FromDate,
                                   ToDate = (DateTime)a.ToDate,
                                   FromTemperature = a.FromTemperature,
                                   ToTemperature = (Decimal)a.ToTemperature,
                                   TankType = a.TankType,
                                   BlendNo = a.BlendNo,
                                   SGMethod = a.SGMethod,
                                   ReportingTemperature = (decimal)a.ReportingTemperature

                               }).ToList();

            var tdv = new TankDataViewModel();

            tdv.lstTankLevels = tl;
            TempData["tankDataViewModel"] = tdv;

            return RedirectToAction("Index", new { reprint = true });
        }
    }
}
