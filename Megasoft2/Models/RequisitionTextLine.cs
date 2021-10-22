using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.Models
{
    public class RequisitionTextLine
    {
        public string Requisition { get; set; }
        public int TextLine { get; set; }
        public SelectList Lines { get; set; }
        public int SelectedLine { get; set; }
        public string NText { get; set; }
        public bool LinkedToLine { get; set; }
        public string PurchaseOrder { get; set; }
    }
}