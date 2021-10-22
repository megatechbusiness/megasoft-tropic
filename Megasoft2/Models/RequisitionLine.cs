using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class RequisitionLine
    {
        public string LineType { get; set; }
        public string Requisition { get; set; }
        public string Branch { get; set; }
        public int Line { get; set; }
        public string StockCode { get; set; }
        public string Description { get; set; }
        public string GlCode { get; set; }
        public decimal Quantity { get; set; }
        public string Supplier { get; set; }
        public decimal Price { get; set; }
        public bool Emergency { get; set; }

    }
}