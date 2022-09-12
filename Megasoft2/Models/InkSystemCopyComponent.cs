using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Models
{
    public class InkSystemCopyComponent 
    {
        public int KeyId { get; set; }
        public string FromStockCode { get; set; }
        public string ToStockCode { get; set; }
        public string Route { get; set; }
        public string CopyOption { get; set; }

    }
}
