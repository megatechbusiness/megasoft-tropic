using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RequisitionStatusViewModel
    {
        public List<sp_GetRequisitionStatusReqList_Result> Requisition { get; set; }
        public List<sp_GetRequisitionStatusGrnList_Result> Grn { get; set; }
        public List<sp_GetRequisitionStatusInvoiceList_Result> Invoice { get; set; }

        public List<sp_GetRequisitionStatusAwaitingAuthList_Result> AwaitingAuth { get; set; }
        public List<sp_GetRequisitionStatusPostedInvoiceList_Result> PostedInvoice { get; set; }
        public string GlCode { get; set; }
        public string FilterText { get; set; }
    }
}