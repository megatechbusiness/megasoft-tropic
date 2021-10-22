using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class StockTakeOn
    {
        public int Id { get; set; }
        public string Warehouse { get; set; }
        public string Bin { get; set; }
        public string StockCode { get; set; }     
        public decimal Quantity { get; set; }
        public string Lot { get; set; }
        public string Uom { get; set; }
    }
}