using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class PurchaseOrderEmail
    {
        public string PurchaseOrder { get; set; }
        public string Supplier { get; set; }
        public string ToEmail { get; set; }
        public string CCEmail { get; set; }
        public string FromEmail { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public string AttachmentPath { get; set; }
        public string FileName { get; set; }
    }
}