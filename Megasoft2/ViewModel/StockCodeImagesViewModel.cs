using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class StockCodeImagesViewModel
    {
        public string StockCode { get; set; }
        public string Description { get; set; }
        public string LongDesc { get; set; }
        public List<string> ImageList { get; set; }

    }
}