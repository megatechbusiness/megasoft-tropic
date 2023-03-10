using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class DemoCameraController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            Session["val"] = "";
            return View();
        }


        [HttpPost]
        public ActionResult Index(string Imagename)
        {
            ViewBag.pic = "http://localhost:55694/WebImages/" + Session["val"].ToString();
            return View();
        }

        [HttpGet]
        public ActionResult Changephoto()
        {
            if (Convert.ToString(Session["val"]) != string.Empty)
            {
                ViewBag.pic = "http://localhost:55694/WebImages/" + Session["val"].ToString();
            }
            else
            {
                ViewBag.pic = "../../WebImages/person.jpg";
            }
            return View();
        }


        public JsonResult Rebind()
        {
            string path = "http://localhost:55694/WebImages/" + Session["val"].ToString();
            return Json(path, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Capture()
        {
            var stream = Request.InputStream;
            string dump;
            using (var reader = new StreamReader(stream))
            {
                dump = reader.ReadToEnd();
                DateTime nm = DateTime.Now;
                string date = nm.ToString("yyyymmddMMss");
                var path = Server.MapPath("~/WebImages/" + date + "test.jpg");
                System.IO.File.WriteAllBytes(path, String_To_Bytes2(dump));
                ViewData["path"] = date + "test.jpg";
                Session["val"] = date + "test.jpg";
            }
            return View("Index");
        }


        private byte[] String_To_Bytes2(string strInput)
        {
            int numBytes = (strInput.Length) / 2;
            byte[] bytes = new byte[numBytes];
            for (int x = 0; x < numBytes; ++x)
            {
                bytes[x] = Convert.ToByte(strInput.Substring(x * 2, 2), 16);
            }
            return bytes;
        }



        public ActionResult SaveFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveFile(MasterCardViewModel model, IEnumerable<HttpPostedFileBase> FileUpload)
        {
            try
            {
                int filecount = 0;
                foreach (var item in FileUpload)
                {
                    if (item != null)
                    {
                        string filename = item.FileName;
                        ModelState.AddModelError("", item.FileName);

                        string targetpath = HttpContext.Server.MapPath("~/WebImages/").ToString();

                        item.SaveAs(targetpath + filename);
                        filecount++;
                    }
                }

                ModelState.AddModelError("", filecount.ToString() + " file(s) uploaded.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }

    }
}
