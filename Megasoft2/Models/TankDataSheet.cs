using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class TankDataSheet
    {
        public string Tank { get; set; }
        public string Description { get; set; }
        public string TagName { get; set; }
        public decimal Temperature { get; set; }
        public decimal TankStartValue { get; set; }
        public decimal TankEndValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}