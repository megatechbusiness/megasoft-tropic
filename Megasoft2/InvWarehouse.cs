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
    
    public partial class InvWarehouse
    {
        public InvWarehouse()
        {
            this.LotDetails = new HashSet<LotDetail>();
            this.PorMasterDetails = new HashSet<PorMasterDetail>();
            this.WipMasters = new HashSet<WipMaster>();
            this.WipJobAllMats = new HashSet<WipJobAllMat>();
            this.InvStockTakes = new HashSet<InvStockTake>();
            this.GrnDetails = new HashSet<GrnDetail>();
        }
    
        public string StockCode { get; set; }
        public string Warehouse { get; set; }
        public decimal QtyOnHand { get; set; }
        public decimal MtdQtyReceived { get; set; }
        public decimal MtdQtyAdjusted { get; set; }
        public decimal MtdQtyTrf { get; set; }
        public decimal MtdQtyIssued { get; set; }
        public decimal MtdQtySold { get; set; }
        public decimal QtyAllocated { get; set; }
        public decimal QtyOnOrder { get; set; }
        public decimal QtyOnBackOrder { get; set; }
        public decimal MinimumQty { get; set; }
        public decimal MaximumQty { get; set; }
        public decimal YtdQtySold { get; set; }
        public decimal PrevYearQtySold { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal YtdUsageValue { get; set; }
        public string AbcClass { get; set; }
        public decimal UnitCost { get; set; }
        public string DefaultBin { get; set; }
        public Nullable<System.DateTime> DateLastSale { get; set; }
        public Nullable<System.DateTime> DateLastStockMove { get; set; }
        public Nullable<System.DateTime> DateLastCostChange { get; set; }
        public Nullable<System.DateTime> DateLastStockCnt { get; set; }
        public Nullable<System.DateTime> DateLastPurchase { get; set; }
        public decimal QtyInTransit { get; set; }
        public decimal TransferValue { get; set; }
        public decimal SafetyStockQty { get; set; }
        public decimal ReOrderQty { get; set; }
        public decimal QtyAllocatedWip { get; set; }
        public string InterfaceFlag { get; set; }
        public decimal CostMultiplier { get; set; }
        public decimal LastCostEntered { get; set; }
        public decimal MtdSalesValue { get; set; }
        public decimal YtdSalesValue { get; set; }
        public decimal PrevYtdSalesVal { get; set; }
        public decimal QtyInInspection { get; set; }
        public decimal PalletQty { get; set; }
        public decimal NumMonthsHistory { get; set; }
        public decimal YtdQtyIssued { get; set; }
        public string RequisitionFlag { get; set; }
        public decimal SalesQty1 { get; set; }
        public decimal SalesQty2 { get; set; }
        public decimal SalesQty3 { get; set; }
        public decimal SalesQty4 { get; set; }
        public decimal SalesQty5 { get; set; }
        public decimal SalesQty6 { get; set; }
        public decimal SalesQty7 { get; set; }
        public decimal SalesQty8 { get; set; }
        public decimal SalesQty9 { get; set; }
        public decimal SalesQty10 { get; set; }
        public decimal SalesQty11 { get; set; }
        public decimal SalesQty12 { get; set; }
        public string UserField1 { get; set; }
        public string UserField2 { get; set; }
        public string UserField3 { get; set; }
        public string TrfSuppliedItem { get; set; }
        public string DefaultSourceWh { get; set; }
        public decimal TrfLeadTime { get; set; }
        public string TrfCostGlCode { get; set; }
        public decimal TrfCostMultiply { get; set; }
        public decimal QtyDispatched { get; set; }
        public decimal OpenBalQty1 { get; set; }
        public decimal OpenBalQty2 { get; set; }
        public decimal OpenBalQty3 { get; set; }
        public decimal OpenBalQty4 { get; set; }
        public decimal OpenBalQty5 { get; set; }
        public decimal OpenBalQty6 { get; set; }
        public decimal OpenBalQty7 { get; set; }
        public decimal OpenBalQty8 { get; set; }
        public decimal OpenBalQty9 { get; set; }
        public decimal OpenBalQty10 { get; set; }
        public decimal OpenBalQty11 { get; set; }
        public decimal OpenBalQty12 { get; set; }
        public decimal OpenBalCost1 { get; set; }
        public decimal OpenBalCost2 { get; set; }
        public decimal OpenBalCost3 { get; set; }
        public decimal OpenBalCost4 { get; set; }
        public decimal OpenBalCost5 { get; set; }
        public decimal OpenBalCost6 { get; set; }
        public decimal OpenBalCost7 { get; set; }
        public decimal OpenBalCost8 { get; set; }
        public decimal OpenBalCost9 { get; set; }
        public decimal OpenBalCost10 { get; set; }
        public decimal OpenBalCost11 { get; set; }
        public decimal OpenBalCost12 { get; set; }
        public decimal AgedQty1 { get; set; }
        public decimal AgedQty2 { get; set; }
        public decimal AgedQty3 { get; set; }
        public decimal AgedQty4 { get; set; }
        public decimal AgedQty5 { get; set; }
        public decimal AgedQty6 { get; set; }
        public decimal TrfReplenishWh { get; set; }
        public string TrfBuyingRule { get; set; }
        public decimal TrfDockToStock { get; set; }
        public decimal TrfFixTimePeriod { get; set; }
        public decimal LabourCost { get; set; }
        public decimal MaterialCost { get; set; }
        public decimal FixedOverhead { get; set; }
        public decimal VariableOverhead { get; set; }
        public decimal StdLabCostsBill { get; set; }
        public decimal SubContractCost { get; set; }
        public Nullable<System.DateTime> DateWhAdded { get; set; }
        public decimal RmaQtyIssued { get; set; }
        public decimal LastExtendedCost { get; set; }
        public string OrderPolicy { get; set; }
        public decimal MajorOrderMult { get; set; }
        public decimal MinorOrderMult { get; set; }
        public decimal OrderMinimum { get; set; }
        public decimal OrderMaximum { get; set; }
        public decimal OrderFixPeriod { get; set; }
        public string ManualCostFlag { get; set; }
        public decimal LeadTime { get; set; }
        public decimal ManufLeadTime { get; set; }
        public decimal TransferCost { get; set; }
        public decimal ImplosionNum { get; set; }
        public string ExcludeFromSched { get; set; }
        public decimal QtyWipReserved { get; set; }
        public string Supplier { get; set; }
        public byte[] TimeStamp { get; set; }
        public string BoughtOutWhsLvl { get; set; }
        public decimal DockToStock { get; set; }
        public string UsesPrefSupplier { get; set; }
        public decimal QtyAllocatedToPick { get; set; }
    
        public virtual ICollection<LotDetail> LotDetails { get; set; }
        public virtual ICollection<PorMasterDetail> PorMasterDetails { get; set; }
        public virtual ICollection<WipMaster> WipMasters { get; set; }
        public virtual ICollection<WipJobAllMat> WipJobAllMats { get; set; }
        public virtual ICollection<InvStockTake> InvStockTakes { get; set; }
        public virtual ICollection<GrnDetail> GrnDetails { get; set; }
        public virtual InvMaster InvMaster { get; set; }
        public virtual InvWhControl InvWhControl { get; set; }
        public virtual ApSupplier ApSupplier { get; set; }
        public virtual PorSupStkInfo PorSupStkInfo { get; set; }
    }
}
