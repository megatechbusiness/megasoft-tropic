using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class StereoSystemAddStereoViewModel
    {
        public decimal TotalPoValue { get; set; }
        public decimal TotalSquareM { get; set; }

        public int TotalColours { get; set; }
        public int ReqNo { get; set; }
        public string Customer { get; set; }
        public string StockDescription { get; set; }
        public string PurchaseOrder { get; set; }
        public string PlateCategory { get; set; }
        public char CustomerInvoice { get; set; }
        public string Invoice { get; set; }
        public string SalesOrder { get; set; }
        public string StockCode { get; set; }
        public string SupplierReference { get; set; }
        public string DesignReference { get; set; }
        public char Taxable { get; set; }
        public string Approved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string PrintDescription { get; set; }
        public decimal BagSize { get; set; }
        public decimal CylSlvSize { get; set; }
        public string Surface { get; set; }
        public decimal NumberAround { get; set; }
        public decimal NumberAcross { get; set; }
        public decimal NumberSetsRequired { get; set; }
        public string MaterialType { get; set; }
        public decimal Thickness { get; set; }
        public decimal NumberOfColoursFront { get; set; }
        public decimal NumberOfColoursReverse { get; set; }
        public string SpecialInstructions { get; set; }
        public string Quotation { get; set; }
        public DateTime Date { get; set; }
        public string Barcode { get; set; }
        public string BarcodeColour { get; set; }
        public char EyeMark { get; set; }
        public decimal Size { get; set; }
        public int NumberColours { get; set; }
        public string Position { get; set; }
        public DateTime DateProofRequired { get; set; }
        public DateTime DateStereosRequired { get; set; }
        public char ChargeCustomer { get; set; }
        public char ChargeTropic { get; set; }
        public string Authorize { get; set; }
        public int Line { get; set; }
        //DetailStockCode
        public string DetStockCode { get; set; }
        public string Colour { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int ChangePlate { get; set; }
        public string StereoType { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public string GlCode { get; set; }
        public string ProductClass { get; set; }
        public char TaxCode { get; set; }
        public mtStereoSupplier tblSupplier { get; set; }
        public mtStereoDetail detail { get; set; }
        public List<mtStereoDetail> DetailList { get; set; }
        public List<sp_GetStereoDetails_Result> StereoDetails { get; set; }
        public mtStereoHdr hdr { get; set; }

        //
        public List<sp_GetStereoByPurchaseOrder_Result> PODetails { get; set; }


    }
}