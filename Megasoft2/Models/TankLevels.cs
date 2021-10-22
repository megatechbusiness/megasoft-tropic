using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class TankLevels
    {
        public string Tank { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal FromTemperature { get; set; }
        public decimal ToTemperature { get; set; }
        public string TankType { get; set; }
        public string SGMethod { get; set; }
        public string BlendNo { get; set; }
        public string AlumSilic { get; set; }
        public decimal Kinematic { get; set; }
        public decimal DensitySpec { get; set; }
        public decimal ReportingTemperature { get; set; }

    }
}