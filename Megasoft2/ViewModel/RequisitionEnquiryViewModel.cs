using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RequisitionEnquiryViewModel
    {
        public string ReqOrPONumber { get; set; }
        public List<sp_mtReqGetRequisitionEnquiry_Result> Enquiry { get; set; }
        public string Holder { get; set; }
    }
}