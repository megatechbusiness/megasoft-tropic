using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class Email
    {
        MegasoftEntities mdb = new MegasoftEntities();
        public void SendEmail(Mail mail, List<string> Files)
        {
            //function to send email
            MailMessage email = new MailMessage();
            email.To.Add(mail.To);
            if (!string.IsNullOrEmpty(mail.CC))
            {
                email.CC.Add(mail.CC);
            }

            email.From = new MailAddress(mail.From);
            email.Subject = mail.Subject;
            string Body = mail.Body;
            email.Body = Body;
            email.IsBodyHtml = true;

            var emailSettings = (from a in mdb.mtSystemSettings select a).FirstOrDefault();

            //SmtpClient smtp = new SmtpClient();
            //smtp.Host = emailSettings.SmtpHost;
            //smtp.Port = (int)emailSettings.SmtpPort;
            //smtp.UseDefaultCredentials = true;
            ////smtp.Credentials = new NetworkCredential("dineshr@ffs.co.za", "");
            //smtp.EnableSsl = false;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = emailSettings.SmtpHost;
            smtp.Port = (int)emailSettings.SmtpPort;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(emailSettings.FromAddress, emailSettings.FromAddressPassword);
            smtp.EnableSsl = true;



            foreach (var path in Files)
            {
                // Create the file attachment for this e-mail message.
                Attachment data = new Attachment(HttpContext.Current.Server.MapPath(path), MediaTypeNames.Application.Pdf);

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
        }
    }
}