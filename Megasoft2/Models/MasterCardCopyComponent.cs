using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class MasterCardCopyComponent
    {
        public int KeyId { get; set; }
        public string FromStockCode { get; set; }
        public string ToStockCode { get; set; }
        public string Route { get; set; }
        public string CopyOption { get; set; }
    }
}