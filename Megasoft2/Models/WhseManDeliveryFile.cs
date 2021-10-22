using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class WhseManDeliveryFile
    {
        public string RECORD_TYPE { get; set; }
        public string RECORD_FUNCTION { get; set; }
        public string SUPPLIERSTKCODE { get; set; }
        public string FS_STKCODE { get; set; }
        public decimal QUANTITY { get; set; }
        public string UOM { get; set; }
        public decimal QTYMTR { get; set; }
        public decimal GRAMMAGE { get; set; }
        public string BATCH_NUMBER { get; set; }
        public string SUPPLIERCODE { get; set; }
        public string PURCHASEORDER { get; set; }
        public int LINENO { get; set; }
        public string DELIVERY_NUMBER { get; set; }
    }
}