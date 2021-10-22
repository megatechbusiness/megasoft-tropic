using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class WhseManProductionReceiptController : Controller
    {
        //
        // GET: /WhseManProductionReceipt/

        WhseManProductionReceipt objPost = new WhseManProductionReceipt();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Activity: "Immediate")]
        public ActionResult PostJobReceipt(string details)
        {
            try
            {
                List<WhseManJobReceipt> myDeserializedObjList = (List<WhseManJobReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WhseManJobReceipt>));
                if (myDeserializedObjList.Count > 0)
                {
                    return Json(objPost.PostJobReceipt(myDeserializedObjList), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Data found.", JsonRequestBehavior.AllowGet);
                }                
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        

    }
}
