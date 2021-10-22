using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            Test obj = new Test();
            return View(obj);
        }

        [HttpPost]
        public ActionResult Index(Test model)
        {
            ModelState.AddModelError("", model.StockCode);
            return View(model);
        }

    }
}
