//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Megasoft2
{
    using System;
    using System.Collections.Generic;
    
    public partial class mtDispatchPlan
    {
        public System.DateTime DispatchDate { get; set; }
        public int DeliveryNo { get; set; }
        public string Customer { get; set; }
        public string SalesOrder { get; set; }
        public decimal SalesOrderLine { get; set; }
        public string MStockCode { get; set; }
        public string MStockDesc { get; set; }
        public string Size { get; set; }
        public decimal MOrderQty { get; set; }
        public decimal MBackOrderQty { get; set; }
        public decimal MQtyToDispatch { get; set; }
        public string Transporter { get; set; }
        public Nullable<decimal> VehicleCapacity { get; set; }
        public string Picker { get; set; }
        public string Status { get; set; }
    }
}
