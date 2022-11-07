using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class MasterCardArtworkController : Controller
    {
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        // GET: /MasterCardArtwork/
        [CustomAuthorize("MasterCardArtworkUpload")]
        public ActionResult Index()
        {
            return View();
        }

        [CustomAuthorize("MasterCardArtworkUpload")]
        [HttpGet]
        public ActionResult FileUpload(string Customer, string StockCode)
        {
            mtMasterCardArtwork model = new mtMasterCardArtwork();
            model.StockCode = StockCode;
            model.Customer = Customer;
            return View(model);
        }

        [CustomAuthorize("MasterCardArtworkUpload")]
        [HttpPost]
        public ActionResult FileUpload(mtMasterCardArtwork model, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (file.ContentLength > 0 && file.ContentType == "application/pdf")
                    {
                        string FileName = Path.GetFileName(file.FileName);
                        var ArtWorkPath = (from a in db.mtWhseManSettings where a.SettingId == 1 select a.MaterCardMultimediaPath).FirstOrDefault();
                        string FilePath = Path.Combine(ArtWorkPath, FileName);
                        file.SaveAs(FilePath);

                        var update = (from a in db.mtMasterCardHeaders where a.StockCode == model.StockCode && a.Customer == model.Customer select a).FirstOrDefault();
                        //check if file exists in path: 2022/08/18-SR
                        var check = (from a in db.mtMasterCardArtworks where a.StockCode == model.StockCode && a.Customer == model.Customer select a).FirstOrDefault();
                        if (check !=null)
                        {
                            FileInfo delFile = new FileInfo(check.FilePath);
                            delFile.Delete();
                            check.FilePath = FilePath;
                            check.Username = User.Identity.Name;
                            check.DateSaved = DateTime.Now;

                            db.Entry(check).State = System.Data.EntityState.Modified;
                            db.SaveChanges();



                            ModelState.AddModelError("", "File uploaded successfully.");

                            return RedirectToAction("Index", "MasterCardMultimediaViewer", new { Customer = model.Customer });

                        }
                        if (file.ContentType != "application/pdf")
                        {
                            ModelState.AddModelError("", " File needs to be in Pdf format.");
                        }
                        else if (update != null)
                        {
                            update.MultiMediaFilePath = FilePath;
                            db.Entry(update).State = System.Data.EntityState.Modified;
                            db.SaveChanges();


                            ModelState.AddModelError("", "File uploaded successfully.");


                            return RedirectToAction("Index", "MasterCardMultimediaViewer", new { Customer = model.Customer });

                        }
                        else
                        {
                            model.FilePath = FilePath;
                            model.Username = User.Identity.Name;
                            model.DateSaved = DateTime.Now;

                            db.mtMasterCardArtworks.Add(model);
                            db.SaveChanges();



                            ModelState.AddModelError("", "File uploaded successfully.");

                            return RedirectToAction("Index", "MasterCardMultimediaViewer", new { Customer = model.Customer });
                        }

                    }

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }

            }
            return View();
        }
    }
}
