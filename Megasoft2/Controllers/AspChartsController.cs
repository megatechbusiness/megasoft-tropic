using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Megasoft2.Controllers
{
    public class AspChartsController : Controller
    {
        //
        // GET: /AspCharts/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ProductionHistory()
        {

            var myChart = new Chart(width: 600, height: 400)
                .AddTitle("Chart Title")
                .AddSeries(
                    name: "Employee",
                    xValue: new[] { "Jarod", "Andrew", "Julie", "Mary", "Dave" },
                    yValues: new[] { "2", "6", "4", "5", "3" })
                .AddSeries(name: "TEST",
                    xValue: new[] { "Jarod", "Andrew", "Julie", "Mary", "Dave" },
                    yValues: new[] { "3", "5", "2", "4", "1" })
            .GetBytes("png");

            return File(myChart, "image/bytes");
            //string[] xData = new[] { "Peter", "Andrew", "Julie", "Mary", "Dave" };
            //ViewBag.xData = xData;
            //return PartialView();
        }

    }
}
