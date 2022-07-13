using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class SuppDeliveryLogViewModel
    {
        public string Supplier { get; set; }
        public string DeliveryNote { get; set; }
        public mtSuppDeliveryLog SuppLog { get; set; }
        public List<sp_GetPoLabelLines_Result> ReelLines { get; set; }
        public List<sp_BaggingLabelEmployees_Result> EmployeeList { get; set; }
        public ExportFile PrintPdf { get; set; }
        public List<mt_SupplierDeliveryLogReport_Result> ReportList { get; set; }
        public List<mtSuppDeliveryLog> DeliveryLogList { get; set; }
    }
}