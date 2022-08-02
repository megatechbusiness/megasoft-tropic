using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TransportSystemCustomerEmailController : Controller
    {
        WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        //
        // GET: /TransportSystemCustomerEmail/
        [CustomAuthorize(Activity: "TransportSystemCustomerEmail")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadLocation")]
        public ActionResult LoadLocations(TransportSystemCustomerEmail model)
        {
            ModelState.Clear();
            try
            {
                DateTime ReportDate = Convert.ToDateTime(model.Date);
                var locations = (from m in wdb.sp_GetTransportSystemCustomerTowns(ReportDate, model.Customer) select new { Value = m.Town, Text = m.Town }).ToList();
                if (locations.Count == 0)
                {
                    ModelState.AddModelError("", "No customer locations found for. " + model.Customer + " on " + model.Date);
                    return View("Index", model);
                }
                if (locations.Count == 1)
                {
                    model.Location = locations.FirstOrDefault().Value;
                }
                ViewBag.CustomerLocations = locations;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error getting customer locations. " + ex.Message);
            }

            return View("Index", model);
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadCustomer")]
        public ActionResult LoadCustomer(TransportSystemCustomerEmail model)
        {
            ModelState.Clear();
            var DefaultEmailSettings = (from a in wdb.mtTransportCustomerEmailSettings where a.Customer == model.Customer select a).FirstOrDefault();
            var CustName = wdb.sp_GetStereoCustomerName(model.Customer).FirstOrDefault();
            if (DefaultEmailSettings != null)
            {

                model.EmailTo = DefaultEmailSettings.ToEmail;
                model.EmailFrom = DefaultEmailSettings.FromEmail;
                model.CC = DefaultEmailSettings.CC;
                model.BCC = DefaultEmailSettings.BCC;
            }
            else
            {
                ModelState.AddModelError("", "Customer Email Settings not found");

            }
            if (CustName != null)
            {
                model.CustomerName = CustName.Name;
                model.Subject = model.CustomerName.ToUpper() + " - " + model.Location + " DESPATCHES FOR " + model.Date;
                string MessageText = "Good day, Trust you are well,Kindly find attached dispatches for " + model.Date + "," + model.CustomerName;
                MessageText = MessageText.Replace(",", "," + System.Environment.NewLine + System.Environment.NewLine);
                model.Message = MessageText;
            }
            DateTime ReportDate = Convert.ToDateTime(model.Date);
            ViewBag.CustomerLocations = (from m in wdb.sp_GetTransportSystemCustomerTowns(ReportDate, model.Customer) select new { Value = m.Town, Text = m.Town }).ToList();
            return View("Index", model);
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "SendMail")]
        public ActionResult SendEmail(TransportSystemCustomerEmail model)
        {
            ModelState.Clear();
            //var emailSettings = (from a in mdb.mtSystemSettings select a).FirstOrDefault();
            var emailSettings = (from a in mdb.mtEmailSettings where a.EmailProgram == "TransportAutomationSystem" select a).FirstOrDefault();

            Mail objMail = new Mail();
            objMail.From = emailSettings.FromAddress;
            objMail.To = model.EmailTo;
            objMail.Subject = model.Subject;
            objMail.Body = model.Message;
            objMail.CC = model.CC;
            objMail.BCC = model.BCC;

            List<string> attachments = new List<string>();
            try
            {
                attachments.Add(ExportPdf(model.Customer, model.Date, model.Location));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error generating report : " + ex.Message);
                return View("Index", model);
            }

            try
            {
                Email(objMail, attachments, emailSettings.FromAddress, emailSettings.FromAddressPassword, emailSettings.SmtpHost, emailSettings.SmtpPort ?? 0);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error sending email : " + ex.Message);
                return View("Index", model);
            }
            ModelState.AddModelError("", "Email Sent Successfully to " + model.Customer);
            TransportSystemCustomerEmail newModel = new TransportSystemCustomerEmail();
            return View("Index", newModel);
        }

        public void Email(Mail mail, List<string> Files, string FromAddress, string FromAddressPassword, string Host, int Port)
        {
            //function to send email
            MailMessage email = new MailMessage();
            foreach (var address in mail.To.Trim().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                email.To.Add(address);
            }

            if (!string.IsNullOrEmpty(mail.CC))
            {
                foreach (var address in mail.CC.Trim().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    email.CC.Add(address);
                }

            }
            if (!string.IsNullOrEmpty(mail.BCC))
            {
                foreach (var address in mail.BCC.Trim().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    email.Bcc.Add(address);
                }
            }

            email.From = new MailAddress(FromAddress);
            email.Subject = mail.Subject;
            mail.Body = mail.Body.Replace(",", "," + "<br/><br/>"); ;
            email.Body = mail.Body;
            email.IsBodyHtml = true;

            //SmtpClient smtp = new SmtpClient();
            //smtp.Host = Host;
            //smtp.Port = Port;
            //smtp.UseDefaultCredentials = false;
            //smtp.Credentials = new NetworkCredential(FromAddress, FromAddressPassword);
            //smtp.EnableSsl = true;
            ////smtp.UseDefaultCredentials = true;
            ////smtp.EnableSsl = true;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = Host;
            smtp.Port = Port;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(FromAddress, FromAddressPassword);
            smtp.EnableSsl = true;



            foreach (var path in Files)
            {
                Attachment data = new Attachment(path, MediaTypeNames.Application.Pdf);
                // Add time stamp information for the file.
                ContentDisposition disposition = data.ContentDisposition;
                disposition.CreationDate = System.IO.File.GetCreationTime(path);
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(path);
                disposition.ReadDate = System.IO.File.GetLastAccessTime(path);

                // Add the file attachment to this e-mail message.
                email.Attachments.Add(data);

            }
            smtp.Send(email);
            smtp.Dispose();
            email.Attachments.Dispose();


        }

        public string ExportPdf(string Customer, string Date, string Location)
        {
            try
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePathRelativeToAssembly = Path.Combine(assemblyPath, @"D:\wwwroot\Megasoft\Reports\TransportSystem\Pdf\");
                string normalizedPath = Path.GetFullPath(filePathRelativeToAssembly);
                string[] files = Directory.GetFiles(normalizedPath);

                foreach (string delFile in files)
                {
                    FileInfo fi = new FileInfo(delFile);
                    fi.Delete();
                }
            }
            catch (Exception err)
            {

            }

            try

            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePathRelativeToAssembly = Path.Combine(assemblyPath, @"D:\wwwroot\Megasoft\Reports\TransportSystem\Pdf\");
                string FilePath = Path.GetFullPath(filePathRelativeToAssembly);
                var ReportDet = (from a in wdb.mtReportMasters where a.Report == "TransportScheduleEmailDocument" select a).FirstOrDefault();

                ReportDocument rpt = new ReportDocument();
                rpt.Load(ReportDet.ReportPath);

                ConnectionStringSettings sysproSettings = ConfigurationManager.ConnectionStrings["WarehouseManagementEntities"];
                if (sysproSettings == null || string.IsNullOrEmpty(sysproSettings.ConnectionString))
                {
                    throw new Exception("Fatal error: Missing connection string 'WarehouseManagementEntities' in web.config file");
                }
                string sysproConnectionString = sysproSettings.ConnectionString;
                EntityConnectionStringBuilder entityConnectionStringBuilder = new EntityConnectionStringBuilder(sysproConnectionString);
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(entityConnectionStringBuilder.ProviderConnectionString);

                string password = sqlConnectionStringBuilder.Password;
                string userId = sqlConnectionStringBuilder.UserID;

                rpt.SetDatabaseLogon(userId, password);
                rpt.SetParameterValue("@FromDate", Date);
                rpt.SetParameterValue("@ToDate", Date);
                rpt.SetParameterValue("Customer", Customer);
                rpt.SetParameterValue("Location", Location);
                var CustName = wdb.sp_GetStereoCustomerName(Customer).FirstOrDefault();
                string FileName = Customer + " " + Convert.ToDateTime(Date).ToString("dd.MM.yyyy") + ".pdf";

                string OutputPath = Path.Combine(FilePath, FileName);

                rpt.ExportToDisk(ExportFormatType.PortableDocFormat, OutputPath);

                rpt.Close();
                rpt.Dispose();
                GC.Collect();

                return OutputPath;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class ExportFile
        {
            public string FilePath { get; set; }
            public string FileName { get; set; }
        }

        public ActionResult CustomerSearch()
        {
            return PartialView();
        }
        public ActionResult CustomerList(string FilterText)
        {
            var result = wdb.mtTransportCustomerEmailSettings.ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SysproCustomerSearch()
        {
            return PartialView();
        }
        public ActionResult SysproCustomerList(string FilterText)
        {
            var result = wdb.sp_GetStereoCustomers(FilterText).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize(Activity: "CustomerEmailSettings")]
        public ActionResult CustomerSetup()
        {
            return View(wdb.mtTransportCustomerEmailSettings.AsEnumerable());
        }
        [CustomAuthorize(Activity: "CustomerEmailSettings")]
        public ActionResult CustomerSetupCreate(string Customer)
        {
            try
            {
                mtTransportCustomerEmailSetting model = new mtTransportCustomerEmailSetting();
                if (Customer != null)
                {

                    model = wdb.mtTransportCustomerEmailSettings.Find(Customer);
                    return View(model);
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", "Error Loading details:  " + ex.Message);
                return View();
            }
        }

        [CustomAuthorize(Activity: "CustomerEmailSettings")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [MultipleButton(Name = "action", Argument = "AddNewCustomer")]
        public ActionResult AddNewCustomer(mtTransportCustomerEmailSetting model)
        {
            try
            {
                var CheckIfExists = wdb.mtTransportCustomerEmailSettings.Find(model.Customer);
                if (CheckIfExists != null)
                {
                    wdb.Entry(CheckIfExists).CurrentValues.SetValues(model);
                    wdb.SaveChanges();
                    ModelState.AddModelError("", "Customer Mailing Updated.");
                }
                else
                {
                    //model.FromEmail = mdb.mtSystemSettings.Find(1).FromAddress;
                    model.FromEmail = mdb.mtEmailSettings.Find("TransportAutomationSystem").FromAddress;
                    wdb.mtTransportCustomerEmailSettings.Add(model);
                    wdb.SaveChanges();
                    ModelState.AddModelError("", "Customer mailing list created.");
                }


                return View("CustomerSetupCreate", model);

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error Saving data:  " + ex.Message);
                return View("CustomerSetupCreate", model);
            }
        }

        [CustomAuthorize(Activity: "CustomerEmailSettings")]
        public ActionResult CustomerSetupDelete(string Customer)
        {
            try
            {
                mtTransportCustomerEmailSetting model = new mtTransportCustomerEmailSetting();
                model = wdb.mtTransportCustomerEmailSettings.Find(Customer);
                if (model != null)
                {
                    wdb.Entry(model).State = System.Data.EntityState.Deleted;
                    wdb.SaveChanges();
                    ModelState.AddModelError("", "Deleted successfully.");
                    return View("CustomerSetup", wdb.mtTransportCustomerEmailSettings.AsEnumerable());
                }
                else
                {
                    ModelState.AddModelError("", "Customer not found.");
                    return View("CustomerSetup", wdb.mtTransportCustomerEmailSettings.AsEnumerable());
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error Deleting Customer:  " + ex.Message);
                return View("CustomerSetup", wdb.mtTransportCustomerEmailSettings.AsEnumerable());
            }
        }


    }
}
