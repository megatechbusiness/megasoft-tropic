using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class SuppDeliveryLogViewModel
    {
        public mtSuppDeliveryLog SuppLog { get; set; }
        public DateTime Date { get; set; }
        public DateTime? Time { get; set; }
        public List<sp_GetPoLabelLines_Result> ReelLines { get; set; }
        public string PurchaseOrder { get; set; }
        public string Employee { get; set; }
        public string Supplier { get; set; }

        public List<sp_SupplierDeliveryGetLogByReciever_Result> GetLogReciever { get; set; }

        public ExportFile PrintPdf { get; set; }
        public List<mt_SupplierDeliveryLogReport_Result> ReportList { get; set; }
    }
}