using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace Megasoft2.Controllers
{
    public class RequisitionApiController : ApiController
    {

        public HttpResponseMessage Get(Guid id)
        {

            MegasoftEntities mdb = new MegasoftEntities();
            WarehouseManagementEntities wdb = new WarehouseManagementEntities("");

            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<!doctype html>");
            Document.Append("<html>");
            Document.Append("<head>");
            Document.Append("<meta name=\"viewport\" content=\"width=device-width\">");
            Document.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">");
            Document.Append("<title>Requisition powered by Megasoft</title>");
            Document.Append("<style>");
            Document.Append("/* -------------------------------------");
            Document.Append("INLINED WITH htmlemail.io/inline");
            Document.Append("------------------------------------- */");
            Document.Append("/* -------------------------------------");
            Document.Append("RESPONSIVE AND MOBILE FRIENDLY STYLES");
            Document.Append("------------------------------------- */");
            Document.Append("@media only screen and (max-width: 720px) {");
            Document.Append("table[class=body] h1 {");
            Document.Append("font-size: 28px !important;");
            Document.Append("margin-bottom: 10px !important;");
            Document.Append("}");
            Document.Append("table[class=body] p,");
            Document.Append("table[class=body] ul,");
            Document.Append("table[class=body] ol,");
            Document.Append("table[class=body] td,");
            Document.Append("table[class=body] span,");
            Document.Append("table[class=body] a {");
            Document.Append("font-size: 16px !important;");
            Document.Append("}");
            Document.Append("table[class=body] .wrapper,");
            Document.Append("table[class=body] .article {");
            Document.Append("padding: 10px !important;");
            Document.Append("}");
            Document.Append("table[class=body] .content {");
            Document.Append("padding: 0 !important;");
            Document.Append("}");
            Document.Append("table[class=body] .container {");
            Document.Append("padding: 0 !important;");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .main {");
            Document.Append("border-left-width: 0 !important;");
            Document.Append("border-radius: 0 !important;");
            Document.Append("border-right-width: 0 !important;");
            Document.Append("}");
            Document.Append("table[class=body] .btn table {");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .btn a {");
            Document.Append("width: 100% !important;");
            Document.Append("}");
            Document.Append("table[class=body] .img-responsive {");
            Document.Append("height: auto !important;");
            Document.Append("max-width: 100% !important;");
            Document.Append("width: auto !important;");
            Document.Append("}");
            Document.Append("	    table.bottomBorder {");
            Document.Append("border-collapse: collapse;");
            Document.Append("}");
            Document.Append("table.bottomBorder td,");
            Document.Append("table.bottomBorder th {");
            Document.Append("border-bottom: 1px solid yellowgreen;");
            Document.Append("padding: 10px;");
            Document.Append("text-align: left;");
            Document.Append("}");
            Document.Append("}");
            Document.Append("/* -------------------------------------");
            Document.Append("PRESERVE THESE STYLES IN THE HEAD");
            Document.Append("------------------------------------- */");
            Document.Append("@media all {");
            Document.Append(".ExternalClass {");
            Document.Append("width: 100%;");
            Document.Append("}");
            Document.Append(".ExternalClass,");
            Document.Append(".ExternalClass p,");
            Document.Append(".ExternalClass span,");
            Document.Append(".ExternalClass font,");
            Document.Append(".ExternalClass td,");
            Document.Append(".ExternalClass div {");
            Document.Append("line-height: 100%;");
            Document.Append("}");
            Document.Append(".apple-link a {");
            Document.Append("color: inherit !important;");
            Document.Append("font-family: inherit !important;");
            Document.Append("font-size: inherit !important;");
            Document.Append("font-weight: inherit !important;");
            Document.Append("line-height: inherit !important;");
            Document.Append("text-decoration: none !important;");
            Document.Append("}");
            Document.Append("#MessageViewBody a {");
            Document.Append("color: inherit;");
            Document.Append("text-decoration: none;");
            Document.Append("font-size: inherit;");
            Document.Append("font-family: inherit;");
            Document.Append("font-weight: inherit;");
            Document.Append("line-height: inherit;");
            Document.Append("}");
            Document.Append(".btn-primary table td:hover {");
            Document.Append("background-color: #34495e !important;");
            Document.Append("}");
            Document.Append(".btn-primary a:hover {");
            Document.Append("background-color: #34495e !important;");
            Document.Append("border-color: #34495e !important;");
            Document.Append("}");
            Document.Append("}");
            Document.Append("	");
            Document.Append("	");
            Document.Append("</style>");
            Document.Append("</head>");
            Document.Append("<body class=\"\" style=\"background-color: #f6f6f6; font-family: sans-serif; -webkit-font-smoothing: antialiased; font-size: 14px; line-height: 1.4; margin: 0; padding: 0; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"body\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background-color: #f6f6f6;\">");
            Document.Append("<tr>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">&nbsp;</td>");
            Document.Append("<td class=\"container\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; display: block; Margin: 0 auto; max-width: 780px; padding: 10px; width: 780px;\">");
            Document.Append("<div class=\"content\" style=\"box-sizing: border-box; display: block; Margin: 0 auto; max-width: 780px; padding: 10px;\">");
            Document.Append("<!-- START CENTERED WHITE CONTAINER -->");
            Document.Append("<span class=\"preheader\" style=\"color: transparent; display: none; height: 0; max-height: 0; max-width: 0; opacity: 0; overflow: hidden; mso-hide: all; visibility: hidden; width: 0;\"></span>");
            Document.Append("<table class=\"main\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; background: #ffffff; border-radius: 3px;\">");
            Document.Append("<!-- START MAIN CONTENT AREA -->");
            Document.Append("<tr>");
            Document.Append("<td class=\"wrapper\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; box-sizing: border-box; padding: 20px;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<tr>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">                       					");
            Document.Append("						<table class=\"grtable\" style=\"width:100%\">");
            Document.Append("						  <caption style=\"font-weight:bold;\">###TEXTGOESHERE###</caption>						");
            Document.Append("						</table>");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("<!-- END MAIN CONTENT AREA -->");
            Document.Append("</table>");
            Document.Append("<!-- START FOOTER -->");
            Document.Append("<div class=\"footer\" style=\"clear: both; Margin-top: 10px; text-align: center; width: 100%;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<td class=\"content-block powered-by\" style=\"font-family: sans-serif; vertical-align: top; padding-bottom: 10px; padding-top: 10px; font-size: 12px; color: #999999; text-align: center;\">");
            Document.Append("Powered by <a href=\"http://www.mega-tech.co.za\" style=\"color: #999999; font-size: 12px; text-align: center; text-decoration: none;\">Megasoft</a>.");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</div>");
            Document.Append("<!-- END FOOTER -->");
            Document.Append("<!-- END CENTERED WHITE CONTAINER -->");
            Document.Append("</div>");
            Document.Append("</td>");
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">&nbsp;</td>");
            Document.Append("</tr>");
            Document.Append("</table>");
            Document.Append("</body>");
            Document.Append("</html>");


            try
            {
                string ReturnHtml = "";
                var result = (from a in mdb.mtReqRoutingTrackings where a.MegasoftGuid == id select a).FirstOrDefault();
                if (result == null)
                {
                    ReturnHtml = Document.ToString().Replace("###TEXTGOESHERE###", "Cannot find approval request. Please contact your administrator. Reference : " + id.ToString());
                }
                else
                {
                    if (result.Approved == "Y")
                    {
                        ReturnHtml = Document.ToString().Replace("###TEXTGOESHERE###", "Your approval request has already been processed.");

                    }
                    else
                    {
                        if (result.GuidActive == "Y")
                        {
                            ReturnHtml = Document.ToString().Replace("###TEXTGOESHERE###", "Your approval request has been received and will be processed shortly.");
                            result.GuidActive = "N";
                            result.Approved = "Y";
                            result.DateApproved = DateTime.Now;
                            result.ProcessApiRequest = "Y";
                            mdb.Entry(result).State = System.Data.EntityState.Modified;
                            mdb.SaveChanges();
                        }
                        else
                        {
                            ReturnHtml = Document.ToString().Replace("###TEXTGOESHERE###", "Your approval request is no longer active. Please contact your system administrator. Reference : " + id.ToString());
                        }
                    }

                }

                var response = new HttpResponseMessage();
                response.Content = new StringContent(ReturnHtml);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                return response;
            }
            catch (Exception ex)
            {
                var response = new HttpResponseMessage();


                string ReturnHtml = Document.ToString().Replace("###TEXTGOESHERE###", "An error has occured. " + ex.Message);

                response.Content = new StringContent(ReturnHtml);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                return response;
            }
        }
    }
}
