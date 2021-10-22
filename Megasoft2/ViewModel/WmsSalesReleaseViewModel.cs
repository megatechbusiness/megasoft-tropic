using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class WmsSalesReleaseViewModel
    {
        public string SalesOrderSelection { get; set; }

        public string SalesOrder { get; set; }

        public string StockCodeSelection { get; set; }

        public string StockCode { get; set; }

        public string ShipDateSelection { get; set; }

        public DateTime StartShipDate { get; set; }

        public DateTime EndShipDate { get; set; }

        public List<sp_GetWmsOrdersForRelease_Result> OrderLines { get; set; }
    }
}