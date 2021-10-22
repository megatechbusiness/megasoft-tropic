using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class GrnFilter
    {
        public DateTime DeliveryNoteDate { get; set; }
        public string DeliveryNote { get; set; }
        public string PurchaseOrder { get; set; }
        public DateTime GrnDate { get; set; }
        public int GrnMonth { get; set; }
        public int GrnYear { get; set; }
    }
}