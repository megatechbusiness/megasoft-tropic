using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class PostingViewModel
    {

        public List<sp_GetPostingErrors_Result> PostErrors { get; set; }
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}