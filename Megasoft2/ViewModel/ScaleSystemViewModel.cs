using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class ScaleSystemViewModel
    {
        public string Job { get; set; }
        public string JobDescription { get; set; }
        public string StockDescription { get; set; }
        public string StockCode { get; set; }
        public string Customer { get; set; }
        public string Dimensions { get; set; }
        public string CustStockCode { get; set; }
        public int Scale { get; set; }
        public int ScaleVal { get; set; }
        public decimal QtyToMake { get; set; }
        public decimal QtyOutstanding { get; set; }
        public decimal QtyManufactured { get; set; }
        public decimal Core { get; set; }
        public decimal Tare { get; set; }
        public decimal Gross { get; set; }
        public decimal Net { get; set; }
        public decimal Weight { get; set; }
        public string Pallet { get; set; }
        public List<mtProductionLabel> TblProductionLabel { get; set; }
        public List<sp_GetScaleRolls_Result> Rolls { get; set; }
        public string Department { get; set; }
        public string ExtruderNo { get; set; }
        public string PrinterNo { get; set; }
        public string Operator { get; set; }

        public int CurrScale { get; set; }
        public int DestScale { get; set; }
        public ExportFile PalletReport { get; set; }
        public ExportFile PalletInformation { get; set; }
        public string Barcode { get; set; }
        //PRINTER OP REFERENCE 20210311
        public string PrintOpReference { get; set; }
        public string PrintOperator { get; set; }
        //Warehouse 20210319
        public string Warehouse { get; set; }
        //Pallet Scan 20210415
        public string BatchId { get; set; }
        public mt_GetPalletDetailsByPalletId_Result PalletList { get; set; }
        public string BoxNo { get; set; }

    }
}