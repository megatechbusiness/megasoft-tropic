using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RequisitionStatusGLViewModel
    {
        public List<sp_GetRequisitionStatusReqListByGL_Result> Requisition { get; set; }
        public List<sp_GetRequisitionStatusGrnListByGL_Result> Grn { get; set; }
        public List<sp_GetRequisitionStatusInvoiceListByGL_Result> Invoice { get; set; }
        public List<sp_GetRequisitionStatusAwaitingAuthByGL_Result> AwaitingAuth { get; set; }
        public List<sp_GetRequisitionStatusPostedInvoiceListByGL_Result> PostedInvoice { get; set; }
        public string GlCode { get; set; }
        public string Description { get; set; }
        public string Job { get; set; }
    }
}