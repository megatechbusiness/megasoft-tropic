//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MegasoftService
{
    using System;
    using System.Collections.Generic;
    
    public partial class WipJobAllMat
    {
        public string Job { get; set; }
        public string StockCode { get; set; }
        public string Warehouse { get; set; }
        public string Line { get; set; }
        public string StockDescription { get; set; }
        public decimal UnitQtyReqd { get; set; }
        public decimal UnitCost { get; set; }
        public decimal OperationOffset { get; set; }
        public string OpOffsetFlag { get; set; }
        public string Uom { get; set; }
        public string Bin { get; set; }
        public decimal QtyIssued { get; set; }
        public decimal ValueIssued { get; set; }
        public decimal QtyBilled { get; set; }
        public decimal ValueBilled { get; set; }
        public decimal Decimals { get; set; }
        public string AllocCompleted { get; set; }
        public string SequenceNum { get; set; }
        public decimal AutoNarrCode { get; set; }
        public string PhantomParent { get; set; }
        public string NonConformFlag { get; set; }
        public string ApplyCostUom { get; set; }
        public string CostUom { get; set; }
        public string BulkIssueItem { get; set; }
        public decimal ScrapPercentage { get; set; }
        public decimal ScrapQuantity { get; set; }
        public string InclScrapOnDoc { get; set; }
        public decimal DockToStock { get; set; }
        public string HierarchyJob1 { get; set; }
        public string HierarchyJob2 { get; set; }
        public string HierarchyJob3 { get; set; }
        public string HierarchyJob4 { get; set; }
        public string HierarchyJob5 { get; set; }
        public string KitIssueItem { get; set; }
        public string CompletedJobFlag { get; set; }
        public string Version { get; set; }
        public string Release { get; set; }
        public string EccConsumption { get; set; }
        public string SubJob { get; set; }
        public string FixedQtyPerFlag { get; set; }
        public decimal FixedQtyPer { get; set; }
        public decimal NetUnitQtyReqd { get; set; }
        public string ReservedLotSerFlag { get; set; }
        public string RollUpCostFlag { get; set; }
        public string CoProductCostVal { get; set; }
        public decimal ReservedLotQty { get; set; }
        public decimal ReservedSerQty { get; set; }
        public decimal ConvFactUom { get; set; }
        public string ConvMulDiv { get; set; }
        public decimal UnitQtyReqdEnt { get; set; }
        public decimal QtyIssuedEnt { get; set; }
        public decimal QtyBilledEnt { get; set; }
        public decimal ScrapQuantityEnt { get; set; }
        public decimal NetUnitQtyReqdEnt { get; set; }
        public decimal FixedQtyPerEnt { get; set; }
        public string RefDesignator { get; set; }
        public string AssemblyPlace { get; set; }
        public string ItemNumber { get; set; }
        public string ComponentType { get; set; }
        public decimal QtyTotRequired { get; set; }
        public decimal QtyTotRequiredEnt { get; set; }
        public decimal QtyOutstanding { get; set; }
        public decimal QtyOutstandingEnt { get; set; }
        public decimal QtyToIssue { get; set; }
        public decimal QtyToIssueEnt { get; set; }
        public decimal QtyReserved { get; set; }
        public decimal QtyReservedEnt { get; set; }
        public string LineStatus { get; set; }
        public decimal SalesOrderInitLine { get; set; }
        public string CreatedBy { get; set; }
        public string ProductCode { get; set; }
        public string LibraryCode { get; set; }
        public decimal FirstSeq { get; set; }
        public decimal SecondSeq { get; set; }
        public string OvrEccSpecIss { get; set; }
        public byte[] TimeStamp { get; set; }
    
        public virtual WipMaster WipMaster { get; set; }
    }
}
