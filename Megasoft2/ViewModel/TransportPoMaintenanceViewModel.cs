using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class TransportPoMaintenanceViewModel
    {
        public string PurchaseOrder { get; set; }
        public List<sp_GetTransPoLinesForMaintenance_Result> Detail { get; set; }
        public string Supplier { get; set; }
    }
}