using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Megasoft2.BusinessLogic;
using Megasoft2.Models;
using Megasoft2.ViewModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

namespace Megasoft2.Controllers
{
    public class PalletMatIssueController : Controller
    {
        PalletMatIssueBL bl = new PalletMatIssueBL();
        WarehouseManagementEntities wdb = new WarehouseManagementEntities();
        BusinessLogic.SysproMaterialIssue objMat = new BusinessLogic.SysproMaterialIssue();

        [CustomAuthorize(Activity: "PalletMatIssue")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "LoadJob")]

        public ActionResult LoadJob(PalletMatIssueViewModel model)
        {
            ModelState.Clear();

            var result = bl.LoadJob(model);
            //result = bl.LoadPallet(model);
            ShowMessage(model);
            //vm = result;
            //var entriesThatAreChecked = model.TestList.Where(a => a.checkedField == true).ToList();

            return View("Index", result);    
        }

        [HttpPost]
        public ActionResult GetSelectedRows(string palletDetails)
        {
            try
            {
                var postPalletIssue = objMat.PostMaterialIssue(palletDetails);
                return Json(postPalletIssue, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet); 
            }
        }

        //public ActionResult LoadPalletDetails(PalletMatIssueViewModel model)
        //{
            
        //    ShowMessage(model);
        //    return View("Index", result);
        //}

        public void ShowMessage(PalletMatIssueViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Messages))
            {
                ModelState.AddModelError("", model.Messages);
            }
        }

    }
}