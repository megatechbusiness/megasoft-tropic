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
    
    public partial class PorSupStkInfo
    {
        public PorSupStkInfo()
        {
            this.GrnDetails = new HashSet<GrnDetail>();
            this.InvMasters = new HashSet<InvMaster>();
            this.InvWarehouses = new HashSet<InvWarehouse>();
        }
    
        public string Supplier { get; set; }
        public string StockCode { get; set; }
        public string SupCatalogueNum { get; set; }
        public decimal DemoLeadTime { get; set; }
        public decimal LastPricePaid { get; set; }
        public Nullable<System.DateTime> LastReceiptDate { get; set; }
        public decimal LastReceiptQty { get; set; }
        public string OrderQtyUom { get; set; }
        public string SupStockDesc { get; set; }
        public string SupLongDesc { get; set; }
        public string PoText1 { get; set; }
        public string PoText2 { get; set; }
        public string PoText3 { get; set; }
        public string PoText4 { get; set; }
        public decimal LctMerchPrice { get; set; }
        public decimal DefaultPrice { get; set; }
        public string StockItemFlag { get; set; }
        public string DefaultPrcUom { get; set; }
        public string LastPrcUom { get; set; }
        public decimal AltDocPrice { get; set; }
        public byte[] TimeStamp { get; set; }
    
        public virtual ApSupplier ApSupplier { get; set; }
        public virtual ICollection<GrnDetail> GrnDetails { get; set; }
        public virtual ICollection<InvMaster> InvMasters { get; set; }
        public virtual InvMaster InvMaster { get; set; }
        public virtual ICollection<InvWarehouse> InvWarehouses { get; set; }
    }
}
