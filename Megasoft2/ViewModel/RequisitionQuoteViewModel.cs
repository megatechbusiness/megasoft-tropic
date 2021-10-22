using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class RequisitionQuoteViewModel
    {
        public sp_GetRequisitionQuoteByLine_Result Detail { get; set; }
        public List<sp_GetRequisitionQuoteLines_Result> Quote { get; set; }

        public RequisitionQuoteViewModel()
        {
            File1 = new List<HttpPostedFileBase>();
            File2 = new List<HttpPostedFileBase>();
            File3 = new List<HttpPostedFileBase>();
        }

        public List<HttpPostedFileBase> File1 { get; set; }
        public List<HttpPostedFileBase> File2 { get; set; }
        public List<HttpPostedFileBase> File3 { get; set; }

        public mtRequisitionHeader Header { get; set; }


    }
}