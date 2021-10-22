using Megasoft2.BusinessLogic;
using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class StereoSystemCustomFormsController : Controller
    {
        //
        // GET: /StereoSystemCustomForms/
        private WarehouseManagementEntities db = new WarehouseManagementEntities("");
        private SysproCore sys = new SysproCore();

        [CustomAuthorize("StereoCustomFormUpdate")]
        public ActionResult Index()
        {
            return View();
        }

        [MultipleButton(Name = "action", Argument = "LoadDetails")]
        [HttpPost]
        public ActionResult LoadDetails(StereoSystemAddStereoViewModel model)
        {
            ModelState.Clear();
            model.PurchaseOrder = model.PurchaseOrder.PadLeft(15, '0');
            var result = db.sp_GetStereoByPurchaseOrder(model.PurchaseOrder).ToList();
            model.PODetails = result;
            return View("Index", model);
        }

        [MultipleButton(Name = "action", Argument = "UpdateLine")]
        [HttpPost]
        public ActionResult UpdateLine(StereoSystemAddStereoViewModel model)
        {
            string Guid = "";
            try
            {
                Guid = sys.SysproLogin();
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    ModelState.AddModelError("", "Failed To Login To Syspro");
                    return View("Index", model);
                }

                string XmlOut = sys.SysproPost(Guid, BuildCustomFormParameter(), BuildCustomFormDocument(model), "COMTFM");
                string ErrorMessage = XmlOut;

                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    ModelState.AddModelError("", "Failed To Post To Syspro");
                    return View("Index", model);
                }
                else
                {
                    ModelState.AddModelError("", "Updated Successful!");
                }

                foreach (var item in model.PODetails)
                {
                    var check = (from a in db.mtStereoDetails where a.PurchaseOrder == model.PurchaseOrder && a.Line == item.Line select a).FirstOrDefault();
                    check.Width = item.Width;
                    check.Length = item.Length;
                    db.Entry(check).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            finally
            {
                sys.SysproLogoff(Guid);
            }

            return View("Index", model);
        }

        public string BuildCustomFormDocument(StereoSystemAddStereoViewModel model)
        {
            StringBuilder Document = new StringBuilder();

            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("Sample XML for the Custom Form Post Business Object");
            Document.Append("-->");
            Document.Append("<PostCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMTFMDOC.XSD\">");

            foreach (var item in model.PODetails)
            {
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<FormType>PORLIN</FormType>");
                Document.Append("<KeyFields>");
                Document.Append("<PurchaseOrder>" + model.PurchaseOrder + "</PurchaseOrder>");
                Document.Append("<Line>" + item.Line + "</Line>");
                Document.Append("</KeyFields>");
                Document.Append("</Key>");
                Document.Append("<Fields>");
                Document.Append("<Width>" + item.Width + "</Width>");
                Document.Append("<Length>" + item.Length + "</Length>");
                Document.Append("</Fields>");
                Document.Append("</Item>");
            }

            Document.Append("</PostCustomForm>");
            return Document.ToString();
        }

        public string BuildCustomFormParameter()
        {
            StringBuilder Parameter = new StringBuilder();

            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2014 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("Sample XML for the Parameters used in the Custom Form Post Business Object");
            Parameter.Append("-->");
            Parameter.Append("<PostCustomForm xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"COMTFM.XSD\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<Function>U</Function>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</PostCustomForm>");

            return Parameter.ToString();
        }

    }
}
