using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class PoGrn
    {
        public List<sp_GetPurchaseOrderLinesForGrn_Result> GrnLines { get; set; }
        public string PurchaseOrder { get; set; }
        public string Grn { get; set; }
        public string DeliveryNote { get; set; }
        public DateTime? DeliveryNoteDate { get; set; }
        public string ReceivedBy { get; set; }

        public ExportFile GrnReport { get; set; }
    }
}