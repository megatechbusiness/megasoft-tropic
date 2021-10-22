using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class WhseManDeliveryFileViewModel
    {
        public List<WhseManDeliveryFile> Detail { get; set; }
        public string DeliveryNote { get; set; }
        public string DeliveryDate { get; set; }
        public string PurchaseOrder { get; set; }
        public string FileName { get; set; }
        public DateTime FileDate { get; set; }
        public TimeSpan FileTime { get; set; }
    }
}