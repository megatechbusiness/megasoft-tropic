using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Threading;
using System.Text;
using Megasoft2.BusinessLogic;
using Megasoft2.Models;

namespace Megasoft2.Controllers
{
    public class RequisitionScanController : Controller
    {
        //
        // GET: /RequisitionScan/
        WarehouseManagementEntities db = new WarehouseManagementEntities("");
        MegasoftEntities mdb = new MegasoftEntities();
        SysproCore sys = new SysproCore();
        MegasoftAlertsBL ABL = new MegasoftAlertsBL();
        Email _email = new Email();


        [CustomAuthorize(Activity: "RequisitionScan")]
        public ActionResult Index()
        {
            //Check and Populate drop down lists
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            var CostCentreList = db.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
            ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
            return View();
        }
        [MultipleButton(Name = "action", Argument = "Index")]
        [HttpPost]
        public ActionResult Index(RequsitionScanViewModel model, IEnumerable<HttpPostedFileBase> FileUpload)
        {
            //Clearing Model State.
            ModelState.Clear();
            //Check and Populate drop down lists
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            var CostCentreList = db.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
            ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
            var requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            ViewBag.Holder = from a in db.sp_mtReqGetRequisitionList(requser, Company) select new { Holder = a.Holder };
            try
            {


                if (FileUpload != null)
                {
                    if (FileUpload.Count() > 0)
                    {
                        using (WarehouseManagementEntities ndb = new WarehouseManagementEntities(""))
                        {
                            var Images = (from a in ndb.mtInvMasterImages where a.StockCode == model.StockCode select a).ToList();
                            foreach (var img in Images)
                            {
                                ndb.mtInvMasterImages.Remove(img);
                                ndb.SaveChanges();
                            }
                        }

                    }
                }

                foreach (var file in FileUpload)
                {
                    if (file != null)
                    {
                        //Convert HttpPostedFileBase to Byte.
                        byte[] ImapgeUpload = new byte[file.InputStream.Length];
                        file.InputStream.Read(ImapgeUpload, 0, ImapgeUpload.Length);
                        //Save Byte array to Table

                        using (WarehouseManagementEntities adb = new WarehouseManagementEntities(""))
                        {
                            mtInvMasterImage obj = new mtInvMasterImage()
                            {
                                StockCode = model.StockCode,
                                Image = ImapgeUpload
                            };
                            adb.mtInvMasterImages.Add(obj);
                            adb.SaveChanges();
                        }

                    }
                }


                string Guid = sys.SysproLogin();//Login to Syspro

                var Requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();//get the prefix from megadb
                string ActionType = "";//Initialize ActionType

                //Declaration Xml 
                StringBuilder Document = new StringBuilder();

                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the Requisition Entry Post Business Object");
                Document.Append("-->");
                Document.Append("<PostRequisition xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRQDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<User>" + Requser + "</User>");
                Document.Append("<UserPassword/>");
                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    //Updating existing Requisition
                    Document.Append("<RequisitionNumber>" + model.Requisition + "</RequisitionNumber>");
                    ActionType = "C";
                    //db.sp_mtReqUpdateReqUserWarehouse(Requser, model.Line.Warehouse);
                }
                else
                {
                    ActionType = "A";
                }

                Document.Append("<RequisitionLine>0</RequisitionLine>");
                Document.Append("<StockCode><![CDATA[" + model.StockCode + "]]></StockCode>");
                Document.Append("<Quantity><![CDATA[" + model.Quantity + "]]></Quantity>");
                Document.Append("<Reason>" + model.ReqComment + "</Reason>");
                Document.Append("<Warehouse><![CDATA[" + model.WareHouse + "]]></Warehouse>");
                Document.Append("<RouteToUser>" + Requser + "</RouteToUser>");
                Document.Append("<Price><![CDATA[0]]></Price>");
                Document.Append("<Job/>");
                Document.Append("<Buyer>IS</Buyer>");
                Document.Append("</Item>");
                Document.Append("</PostRequisition>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("This is an example XML instance to demonstrate");
                Parameter.Append("use of the Requisition Entry Post Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostRequisition xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRQ.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<AllowNonStockedItems>Y</AllowNonStockedItems>");
                Parameter.Append("<AcceptGLCodeforStocked>N</AcceptGLCodeforStocked>");
                Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
                Parameter.Append("<ActionType>A</ActionType>");
                Parameter.Append("<GiveErrorWhenDuplicateFound>N</GiveErrorWhenDuplicateFound>");
                Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostRequisition>");

                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "PORTRQ");

                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {

                    string Requisition = sys.GetFirstXmlValue(XmlOut, "Requisition");
                    if (ActionType == "A")
                    {
                        ABL.SaveMegasoftAlert("Requisition : " + Requisition + " created in Syspro.");
                    }


                    var Branch = (from a in db.mtReqBranches select a).FirstOrDefault();
                    string Request = HttpContext.User.Identity.Name;
                    model.Requisition = Requisition;
                    PostReqCustomForm(Guid, model.CostCentre, Requisition, Branch.Branch, 0, ActionType);
                    PostReqCustomFormUpdate(Guid, Requisition, Request, model.Urgent);
                    Thread.Sleep(3000); //Wait 3 seconds for replication

                    var detail = db.sp_mtReqGetRequisitionLines(Requisition, requser, Username, Company).ToList();
                    model.Lines = detail;

                    sys.SysproLogoff(Guid);
                    ModelState.AddModelError("", "Posted successfully");

                    if (!string.IsNullOrWhiteSpace(model.ReqComment))
                    {
                        SaveComment(Requisition, model.ReqComment);
                    }

                    model.ReqComment = "";
                    model.StockCode = "";
                    model.StockCodeDescription = "";
                    model.Quantity = 0;

                    return View("Index", model);
                }
                else
                {
                    sys.SysproLogoff(Guid);
                    ModelState.AddModelError("", "Syspro Error: " + ErrorMessage);
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", model);
            }

        }

        public ActionResult CheckStockCode(string StockCode)
        {
            RequsitionScanViewModel model = new RequsitionScanViewModel();
            try
            {
                var result = db.sp_GetStockCodeDescriptionByStockCode(StockCode).ToList();
                model.Image = new List<string>();
                foreach (var item in result)
                {
                    if (item.Image != null)
                    {
                        string imreBase64Data = Convert.ToBase64String(item.Image);
                        string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
                        model.Image.Add(imgDataURL);
                    }

                }
                if (result.Count > 0)
                {
                    model.StockCode = result.FirstOrDefault().StockCode;
                    model.StockCodeDescription = result.FirstOrDefault().Description;
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public string PostReqCustomFormUpdate(string Guid, string Requisition, string Requestedby, string Urgent)
        {
            try
            {
                StringBuilder Document = new StringBuilder();
                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Custom Form Setup Business Object");
                Document.Append("-->");
                Document.Append("<SetupCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMSFMDOC.XSD\">");

                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<FormType>REQ</FormType>");
                Document.Append("<KeyField><![CDATA[" + Requisition + "]]></KeyField>");
                Document.Append("<FieldName>Requ1</FieldName>");
                Document.Append("</Key>");
                Document.Append("<AlphaValue><![CDATA[" + Requestedby + "]]></AlphaValue>");//Requested by
                Document.Append("</Item>");

                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<FormType>REQ</FormType>");
                Document.Append("<KeyField><![CDATA[" + Requisition + "]]></KeyField>");
                Document.Append("<FieldName>URG001</FieldName>");
                Document.Append("</Key>");
                Document.Append("<AlphaValue><![CDATA[" + Urgent + "]]></AlphaValue>");//Urgent
                Document.Append("</Item>");
                Document.Append("</SetupCustomForm>");
                //Declaration
                StringBuilder Parameter = new StringBuilder();
                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Custom Form Setup Business Object");
                Parameter.Append("-->");
                Parameter.Append("<SetupCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMSFM.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</SetupCustomForm>");

                string XmlOut = sys.SysproSetupUpdate(Guid, Parameter.ToString(), Document.ToString(), "COMSFM");
                return sys.GetXmlErrors(XmlOut);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }


        public string PostReqCustomForm(string Guid, string CostCentre, string Requisition, string Branch, decimal ExchangeRate, string ActionType)
        {
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();
                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("Sample XML for the Custom Form Setup Business Object");
                Document.Append("-->");
                Document.Append("<SetupCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMSFMDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<FormType>REQ</FormType>");
                Document.Append("<KeyField><![CDATA[" + Requisition + "]]></KeyField>");
                Document.Append("<FieldName>COS001</FieldName>");
                Document.Append("</Key>");
                Document.Append("<AlphaValue><![CDATA[" + CostCentre + "]]></AlphaValue>");
                Document.Append("</Item>");

                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<FormType>REQ</FormType>");
                Document.Append("<KeyField><![CDATA[" + Requisition + "]]></KeyField>");
                Document.Append("<FieldName>BRA001</FieldName>");
                Document.Append("</Key>");
                Document.Append("<AlphaValue><![CDATA[" + Branch + "]]></AlphaValue>");
                Document.Append("</Item>");
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<FormType>REQ</FormType>");
                Document.Append("<KeyField><![CDATA[" + Requisition + "]]></KeyField>");
                Document.Append("<FieldName>EXC001</FieldName>");
                Document.Append("</Key>");
                Document.Append("<NumericValue><![CDATA[" + ExchangeRate + "]]></NumericValue>");
                Document.Append("</Item>");
                Document.Append("</SetupCustomForm>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();
                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("Sample XML for the Custom Form Setup Business Object");
                Parameter.Append("-->");
                Parameter.Append("<SetupCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMSFM.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</SetupCustomForm>");

                string XmlOut = sys.SysproSetupAdd(Guid, Parameter.ToString(), Document.ToString(), "COMSFM");
                return sys.GetXmlErrors(XmlOut);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void SaveComment(string Requisition, string Comment)
        {
            using (var cdb = new WarehouseManagementEntities(""))
            {
                if (!string.IsNullOrWhiteSpace(Requisition))
                {
                    HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                    var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                    mtReqRequisitionComment _comm = new mtReqRequisitionComment();
                    _comm.Company = Company;
                    _comm.Requisition = Requisition;
                    _comm.Comment = Comment;
                    _comm.Username = HttpContext.User.Identity.Name.ToUpper();
                    _comm.TrnDate = DateTime.Now;
                    cdb.mtReqRequisitionComments.Add(_comm);
                    cdb.SaveChanges();

                }
            }

        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "RequisitionRouting")]
        public ActionResult RequisitionRouting(RequsitionScanViewModel model)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            try
            {

                string Username = HttpContext.User.Identity.Name.ToUpper();
                bool OkToApprove = true;
                var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
                ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
                var CostCentreList = db.sp_GetUserDepartments(Company, Username).Where(a => a.Allowed == true).ToList();
                ViewBag.CostCentreList = new SelectList(CostCentreList.ToList(), "CostCentre", "Description");
                var requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                ViewBag.Holder = from a in db.sp_mtReqGetRequisitionList(requser, Company) select new { Holder = a.Holder };
                var code = (from a in db.sp_mtReqGetRouteOnUsers(Username, Company, null, null)
                            where a.Username == Username
                            select a).ToList();

                if (code.Count == 0)
                {
                    ModelState.AddModelError("", "No routing found for " + Username);
                    return View("Index", model);
                }
                var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == model.Requisition && a.Company == Company && a.GuidActive == "Y" select a).ToList();
                if (Tracking.Count > 0)
                {
                    if (Tracking.FirstOrDefault().NoOfApprovals <= 1)
                    {
                        OkToApprove = true;
                        foreach (var tr in Tracking)
                        {
                            tr.Approved = "Y";
                            tr.DateApproved = DateTime.Now;
                            tr.GuidActive = "N";
                            mdb.Entry(tr).State = System.Data.EntityState.Modified;
                            mdb.SaveChanges();
                        }
                    }
                    else if (Tracking.FirstOrDefault().NoOfApprovals > 1)
                    {
                        var ApprovalsOutstanding = (from a in Tracking where a.Approved == "N" select a).ToList();
                        if (ApprovalsOutstanding.Count == 1)
                        {
                            OkToApprove = true; // only 1 approval outstanding which is the current approval
                        }
                        else
                        {
                            OkToApprove = false;
                        }

                        var ItemToFlag = (from a in ApprovalsOutstanding where a.RoutedTo == Username select a).FirstOrDefault();
                        ItemToFlag.Approved = "Y";
                        ItemToFlag.DateApproved = DateTime.Now;
                        ItemToFlag.GuidActive = "N";
                        mdb.Entry(ItemToFlag).State = System.Data.EntityState.Modified;
                        mdb.SaveChanges();
                    }

                }


                if (OkToApprove)
                {
                    var reqheader = db.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    if (reqheader != null)
                    {
                        string sysGuid = sys.SysproLogin();

                        //Declaration
                        StringBuilder Document = new StringBuilder();

                        //Building Document content
                        Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                        Document.Append("<!--");
                        Document.Append("This is an example XML instance to demonstrate");
                        Document.Append("use of the Requisition Route To User Posting Business Object");
                        Document.Append("-->");
                        Document.Append("<PostReqRoute xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRRDOC.XSD\">");
                        Document.Append("<Item>");
                        Document.Append("<User><![CDATA[" + reqheader.OriginatorCode + "]]></User>");
                        Document.Append("<UserPassword/>");
                        Document.Append("<RequisitionNumber><![CDATA[" + model.Requisition + "]]></RequisitionNumber>");
                        Document.Append("<RequisitionLine>0</RequisitionLine>");
                        Document.Append("<RouteToUser><![CDATA[" + code.FirstOrDefault().UserCode + "]]></RouteToUser>");
                        Document.Append("<RouteNotation><![CDATA[Please Approve]]></RouteNotation>");
                        //Document.Append("<eSignature/>");
                        Document.Append("</Item>");
                        Document.Append("</PostReqRoute>");


                        //Declaration
                        StringBuilder Parameter = new StringBuilder();

                        //Building Parameter content
                        Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                        Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                        Parameter.Append("<!--");
                        Parameter.Append("This is an example XML instance to demonstrate");
                        Parameter.Append("use of the Requisition Route To User Posting Business Object");
                        Parameter.Append("There are no parameters required");
                        Parameter.Append("-->");
                        Parameter.Append("<PostReqRoute xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRR.XSD\">");
                        Parameter.Append("<Parameters>");
                        Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                        Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
                        Parameter.Append("</Parameters>");
                        Parameter.Append("</PostReqRoute>");


                        string XmlOut = sys.SysproPost(sysGuid, Parameter.ToString(), Document.ToString(), "PORTRR");
                        sys.SysproLogoff(sysGuid);
                        string ErrorMessage = sys.GetXmlErrors(XmlOut);
                        if (string.IsNullOrWhiteSpace(ErrorMessage))
                        {
                            Thread.Sleep(5000);
                            ClearActiveTracking(model.Requisition, Company);
                            using (var edb = new MegasoftEntities())
                            {

                                model.RouteOn = code;
                                foreach (var item in model.RouteOn)
                                {

                                    Guid eGuid = Guid.NewGuid();
                                    mtReqRoutingTracking obj = new mtReqRoutingTracking();
                                    obj.MegasoftGuid = eGuid;
                                    obj.Company = Company;
                                    obj.Requisition = model.Requisition;
                                    obj.Originator = reqheader.OriginatorCode.Trim();
                                    obj.RoutedTo = item.UserCode.Trim();
                                    obj.DateRouted = DateTime.Now;
                                    obj.Username = Username;
                                    obj.RouteNote = "Please Approve";
                                    obj.GuidActive = "Y";
                                    obj.NoOfApprovals = item.NoOfApprovals;
                                    obj.Approved = "N";
                                    obj.ProcessApiRequest = "N";
                                    edb.Entry(obj).State = System.Data.EntityState.Added;
                                    edb.SaveChanges();

                                    SendEmail(model.Requisition, reqheader.OriginatorCode.Trim(), item.UserCode.Trim(), eGuid);

                                }
                            }


                            ModelState.AddModelError("", "Requistion routed.");
                        }
                        else
                        {
                            string GuidActive = Tracking.FirstOrDefault().GuidActive;
                            //Approval failed so we need last tracking to become active again
                            using (var ldb = new MegasoftEntities())
                            {
                                var LastTracking = (from a in ldb.mtReqRoutingTrackings where a.Requisition == model.Requisition && a.Company == Company && a.GuidActive == "N" select a).OrderByDescending(a => a.Id).FirstOrDefault();
                                LastTracking.GuidActive = "Y";
                                LastTracking.Approved = "N";
                                LastTracking.DateApproved = null;
                                ldb.Entry(LastTracking).State = System.Data.EntityState.Modified;
                                ldb.SaveChanges();
                            }

                            ModelState.AddModelError("", ErrorMessage);
                        }


                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to get requisition details.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Routing request denied due to invalid tracking information found.");
                }


                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = db.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = db.sp_mtReqGetRequisitionLines(model.Requisition, requser, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }
                return View("Index", model);
            }
            catch (Exception ex)
            {
                var Username = HttpContext.User.Identity.Name.ToUpper();
                var ReqName = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                ModelState.AddModelError("", ex.Message);
                if (!string.IsNullOrWhiteSpace(model.Requisition))
                {
                    var header = db.sp_mtReqGetRequisitionHeader(model.Requisition).FirstOrDefault();
                    var detail = db.sp_mtReqGetRequisitionLines(model.Requisition, ReqName, Username, Company).ToList();
                    model.Header = header;
                    model.Lines = detail;
                }

                return View("Index", model);
            }
        }

        public void ClearActiveTracking(string Requisition, string Company)
        {
            try
            {
                var tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == Requisition && a.Company == Company select a).ToList();
                foreach (var tr in tracking)
                {
                    tr.GuidActive = "N";
                    mdb.Entry(tr).State = System.Data.EntityState.Modified;
                    mdb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendEmail(string Requisition, string RoutedBy, string RoutedTo, Guid RouteGuid)
        {
            try
            {
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.FriendlyName).FirstOrDefault();
                var ToUser = (from a in db.sp_mtReqGetRequisitionUsers() where a.UserCode == RoutedTo select a).FirstOrDefault();
                var FromAddress = (from a in mdb.mtEmailSettings where a.EmailProgram == "RequisitionSystem" select a.FromAddress).FirstOrDefault();
                Mail objMail = new Mail();
                objMail.From = FromAddress;
                objMail.To = ToUser.Email;
                objMail.Subject = "Requisition for " + Company;
                objMail.Body = GetEmailTemplate(Requisition, RoutedBy, RoutedTo, RouteGuid, Company);

                List<string> attachments = new List<string>();
                _email.SendEmail(objMail, attachments, "RequisitionSystem");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool RoutedToCanApprove(string Requisition, string RoutedTo)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(Requisition))
                {
                    return false;
                }

                //string Username = HttpContext.User.Identity.Name.ToUpper();

                //var User = (from a in mdb.mtUsers where a.Username == RoutedTo select a).FirstOrDefault();
                //var requser = User.ReqPrefix;

                var header = db.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
                HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
                var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
                var Tracking = (from a in mdb.mtReqRoutingTrackings where a.Requisition == Requisition && a.RoutedTo == RoutedTo && a.Company == Company && a.GuidActive == "Y" select a).ToList();



                if (header.ReqnStatus != "R")
                {
                    if (Tracking.Count > 0 && header.ReqHolder != header.OriginatorCode)
                    {
                        return true;
                    }
                    else return false;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetEmailTemplate(string Requisition, string RoutedBy, string RoutedTo, Guid RouteGuid, string Company)
        {
            var Username = HttpContext.User.Identity.Name.ToUpper();
            var ReqName = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            var Header = db.sp_mtReqGetRequisitionHeader(Requisition).FirstOrDefault();
            var detail = db.sp_mtReqGetRequisitionLines(Requisition, RoutedTo, Username, Company).ToList();

            bool CanApprove = RoutedToCanApprove(Requisition, RoutedTo);
            var RoutedByName = (from a in db.sp_mtReqGetRequisitionUsers() where a.UserCode == RoutedBy select a).FirstOrDefault().UserName;

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
            //Document.Append("border-left-width: 0 !important;");
            //Document.Append("border-radius: 0 !important;");
            //Document.Append("border-right-width: 0 !important;");
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
            Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top;\">");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">Hi there,</p>");
            Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">The below requisition has been routed for your attention.</p>");
            Document.Append("");
            Document.Append("						");
            Document.Append("						<table class=\"grtable\" style=\"width:100%\">");
            Document.Append("						  <caption style=\"font-weight:bold;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Requisition Details</caption>");
            Document.Append("						  <thead>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Company</td>");
            Document.Append("							  <td>" + Company + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\">Site</td>");
            Document.Append("							  <td>" + Header.CostCentre + "</td>");
            Document.Append("							</tr>");
            Document.Append("						  </thead>");
            Document.Append("						  <tbody>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Requisition</td>");
            Document.Append("							  <td>" + Requisition + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\"></td>");
            Document.Append("							  <td></td>");
            Document.Append("							</tr>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Supplier</td>");
            Document.Append("							  <td>" + detail.FirstOrDefault().SupplierName + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\">Currency</td>");
            Document.Append("							  <td>" + detail.FirstOrDefault().Currency + "</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Requisition Value</td>");
            Document.Append("							  <td>" + string.Format("{0:##,###,##0.00}", Header.ReqnValue) + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\">Local Currency Value</td>");
            Document.Append("							  <td>" + string.Format("{0:##,###,##0.00}", Header.ReqnValue) + "</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr style=\"text-align:left\">");
            Document.Append("							  <td style=\"font-weight:bold;\">Originator</td>");
            Document.Append("							  <td>" + Header.Originator + "</td>");
            Document.Append("							  <td style=\"font-weight:bold;\">Routed By</td>");
            Document.Append("							  <td>" + RoutedBy + " - " + RoutedByName + "</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr>");
            //Document.Append("							  <td style=\"font-weight:bold;\">Route Note</td>");
            //Document.Append("							  <td colspan=\"3\">" + RouteNote + "</td>");
            Document.Append("							</tr>");
            Document.Append("							<tr>");
            Document.Append("							</tr>");
            Document.Append("							<tr>");
            Document.Append("								<td colspan=\"4\">");
            Document.Append("									<table class=\"grtable\" style=\"width:100%\">");
            Document.Append("										<tr>");
            Document.Append("											<th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Line</th>");
            Document.Append("											<th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">StockCode</th>");
            Document.Append("											<th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Desc</th>");
            Document.Append("											<th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Qty</th>");
            Document.Append("											<th style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Uom</th>");
            Document.Append("											<th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Price</th>");
            Document.Append("											<th style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">Value</th>");
            Document.Append("										</tr>");

            foreach (var item in detail)
            {
                Document.Append("										<tr>");
                Document.Append("											<td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.Line + "</td>");
                Document.Append("											<td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.StockCode + "</td>");
                Document.Append("											<td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.StockDescription + "</td>");
                Document.Append("											<td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.000}", item.OrderQty) + "</td>");
                Document.Append("											<td style=\"text-align:left;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + item.OrderUom + "</td>");
                Document.Append("											<td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.00}", item.Price) + "</td>");
                Document.Append("											<td style=\"text-align:right;border-collapse: collapse; border-bottom: 1px solid yellowgreen;padding: 10px;\">" + string.Format("{0:##,###,##0.00}", item.OrderQty * item.Price) + "</td>");
                Document.Append("										</tr>");
            }

            Document.Append("									</table>");
            Document.Append("								</td>");
            Document.Append("							</tr>");
            Document.Append("						  </tbody>");
            Document.Append("						</table>");
            Document.Append("						");
            Document.Append("						<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">For more information click the \"View\" button below</p>");
            //Document.Append("<p style=\"font-family: sans-serif; font-size: 14px; font-weight: normal; margin: 0; Margin-bottom: 15px;\">or click the \"Approve\" button.</p>");
            //Document.Append("						");
            Document.Append("						<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"btn btn-primary\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%; box-sizing: border-box;\">");
            Document.Append("<tbody>");
            Document.Append("<tr>");
            Document.Append("<td align=\"left\" style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; padding-bottom: 15px;\">");
            Document.Append("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;\">");
            Document.Append("<tbody>");
            Document.Append("<tr>");


            string HostUrl = Request.Url.Host;
            //if (HostUrl == "localhost")
            //{
            //    HostUrl = "localhost:52696";
            //}
            string ViewUrl = @"http://" + HostUrl + "//Requisition/Create?Requisition=" + Requisition;
            Document.Append("									<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #3498db; border-radius: 5px; text-align: center;\"> <a href=\"" + ViewUrl + "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #3498db; border: solid 1px #3498db; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: #3498db;\">View</a> </td>");
            Document.Append("									<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>");
            if (CanApprove)
            {
                //string ApproveUrl = @"http://" + HostUrl + "/api/RequisitionApi/" + RouteGuid;
                string ApproveUrl = "http://192.168.0.22/MegasoftApi/api/RequisitionApi/" + RouteGuid;
                Document.Append("<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #5cb85c; border-radius: 5px; text-align: center;\"> <a href=\"" + ApproveUrl + "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #5cb85c; border: solid 1px #5cb85c; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 12px 25px; text-transform: capitalize; border-color: #5cb85c;\">Approve</a> </td>");

            }
            Document.Append("</tr>");
            Document.Append("</tbody>");
            Document.Append("</table>");
            Document.Append("</td>");
            Document.Append("</tr>");
            Document.Append("</tbody>");
            Document.Append("</table>");
            Document.Append("						");
            Document.Append("						");
            Document.Append("");
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






            return Document.ToString();
        }


        public ActionResult DeleteLine(string Requisition, decimal Line)
        {
            HttpCookie database = HttpContext.Request.Cookies.Get("SysproDatabase");
            var Company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            string Guid = sys.SysproLogin();
            string Username = HttpContext.User.Identity.Name.ToUpper();
            var Requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            string ActionType = "";
            var WhList = db.sp_GetUserWarehouses(Company, Username).ToList();
            ViewBag.Warehouse = (from a in WhList where a.Allowed == true select new { Warehouse = a.Warehouse, Description = a.Warehouse + " - " + a.Description }).ToList();
            ViewBag.CostCentreList = (from a in db.sp_mtReqGetUserCostCentres(Company).ToList() select new { CostCentre = a.CostCentre, Description = a.Description }).ToList();
            //var requser = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
            RequsitionScanViewModel model = new RequsitionScanViewModel();
            try
            {
                //Declaration
                StringBuilder Document = new StringBuilder();
                //Building Document content
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the Requisition Cancel Business Object");
                Document.Append("-->");
                Document.Append("<PostReqCancel xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRCDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<User>" + Requser + "</User>");
                Document.Append("<UserPassword/>");


                if (!string.IsNullOrWhiteSpace(Requisition))
                {
                    //Updating existing Requisition
                    Document.Append("<RequisitionNumber>" + Requisition + "</RequisitionNumber>");
                    ActionType = "D";

                }
                else
                {
                    ActionType = "D";
                }
                Document.Append("<RequisitionLine>" + Line + "</RequisitionLine>");
                Document.Append("<eSignature/>");
                Document.Append("</Item>");
                Document.Append("</PostReqCancel>");

                //Declaration
                StringBuilder Parameter = new StringBuilder();
                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("This is an example XML instance to demonstrate");
                Parameter.Append("use of the Requisition Cancel Posting Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostReqCancel xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"PORTRC.XSD\">");
                Parameter.Append("<Parameters/>");
                Parameter.Append("</PostReqCancel>");
                string XmlOut = sys.SysproPost(Guid, Parameter.ToString(), Document.ToString(), "PORTRCDOC");
                string ErrorMessage = sys.GetXmlErrors(XmlOut);
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {

                    string Req = sys.GetFirstXmlValue(XmlOut, "Requisition");
                    if (ActionType == "D")
                    {
                        ABL.SaveMegasoftAlert("Requisition : " + Req + " Deleted in Syspro.");
                    }
                    var detail = db.sp_mtReqGetRequisitionLines(Requisition, Requser, Username, Company).ToList();
                    model.Lines = detail;
                    sys.SysproLogoff(Guid);
                    ModelState.AddModelError("", "Deleted successfully");
                    return View("Index", model);
                }
                else
                {
                    var detail = db.sp_mtReqGetRequisitionLines(Requisition, Requser, Username, Company).ToList();
                    model.Lines = detail;
                    sys.SysproLogoff(Guid);
                    ModelState.AddModelError("", "Syspro Error: " + ErrorMessage);
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                var ReqName = (from a in mdb.mtUsers where a.Username == Username select a.ReqPrefix).FirstOrDefault();
                var detail = db.sp_mtReqGetRequisitionLines(Requisition, Requser, Username, Company).ToList();
                model.Lines = detail;
                ModelState.AddModelError("", ex.Message);
                return View("Index");
            }
        }

    }
}
