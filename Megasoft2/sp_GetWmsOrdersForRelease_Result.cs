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
    
    public partial class sp_GetWmsOrdersForRelease_Result
    {
        public string SalesOrder { get; set; }
        public Nullable<int> SalesOrderLine { get; set; }
        public string MStockCode { get; set; }
        public string MStockDes { get; set; }
        public string MWarehouse { get; set; }
        public string MStockingUom { get; set; }
        public Nullable<decimal> MOrderQty { get; set; }
        public Nullable<decimal> MBackOrderQty { get; set; }
        public Nullable<System.DateTime> MLineShipDate { get; set; }
        public Nullable<decimal> ReleaseQty { get; set; }
        public string TraceableType { get; set; }
        public string Comment { get; set; }
        public string Customer { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public Nullable<decimal> QtyOnHand { get; set; }
        public Nullable<decimal> PickingQty { get; set; }
        public int WmsId { get; set; }
        public string Picker { get; set; }
    }
}
