using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class InkSystemViewModel
    {
        public string StockCode { get; set; }
        public string Route { get; set; }
        public InkComponets componets { get; set; }
        public InkBomOperation Operation { get; set; }
    }
}