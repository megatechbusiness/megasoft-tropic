using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class BranchJobListController : Controller
    {
        //
        // GET: /BranchJobList/
        SysproEntities sys = new SysproEntities("");

        [CustomAuthorize(Activity: "JobsEnquiry")]
        public ActionResult Index()
        {
            var result = sys.sp_GetJobList().ToList();
            return View(result);
        }


        

    }
}
