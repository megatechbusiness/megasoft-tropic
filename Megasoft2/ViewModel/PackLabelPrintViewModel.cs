using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.ViewModel
{
    public class PackLabelPrintViewModel 
    {
        public string Job { get; set; }
        public int NoOfLabels { get; set; }
        public string ExtruderNo { get; set; }
        public string ExtRoll { get; set; }
        public string PrintRoll { get; set; }
        public string PrintToWorkCentre { get; set; }
        public string OpCode { get; set; }
        public string Printer { get; set; }
        public string BatchId { get; set; }
        public string BaggingWorkCentre { get; set; }
        public string OpNo { get; set; }
        public decimal? PackSize { get; set; }
        public string Packer { get; set; }
        public string Barcode { get; set; }
        public string BatchNo { get; set; }
        public string Username { get; set; }
        public DateTime TrnDate { get; set; }
        public int PackNo { get; set; }
        public string ErrorMessage { get; set; }

    }
}
