using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class MasterCardViewModel
    {
        public mtMasterCardHeader Header { get; set; }
        public string CustomerName { get; set; }
        public int MasterCardId { get; set; }
        public mtMasterCardExtrusion Extrusion { get; set; }
        public mtMasterCardPrinting Printing { get; set; }
        public List<mtMasterCardPrintingColour> PrintingColour { get; set; }
        public mtMasterCardSlitting Slitting { get; set; }
        public mtMasterCardBagging Bagging { get; set; }
        public mtMasterCardWicketing Wicketing { get; set; }
        public mtMasterCardLamination Lamination { get; set; }
        public mtMasterCardOther Other { get; set; }
        public List<mtMasterCardStockCode> MasterCardStockCode { get; set; }
        public mtMasterCardStockCode stkobj { get; set; }
        public mtMasterCardStockCodeCustomForm CustomForms { get; set; }
        public string FileName { get; set; }
        public string CopyTo { get; set; }


        public string StockCode { get; set; }
        public string StockDesc { get; set; }
        public string LongDescription { get; set; }
        public List<string> AddWarehouse { get; set; }
        public string PartCategory { get; set; }
        public string ProductClass { get; set; }
        public string StockUom { get; set; }
        public decimal Mass { get; set; }
        public string AltUom { get; set; }
        public decimal AltUomFact { get; set; }
        public decimal AltUomMethod { get; set; }
        public string OtherUom { get; set; }
        public decimal OtherUomFact { get; set; }
        public decimal OtherUomMethod { get; set; }
        public decimal MaxNoOfDecimals { get; set; }
        public string Dimensions { get; set; }
        public string Micron { get; set; }
        public string JobClassification { get; set; }
        public string PricingCategory { get; set; }
        public string ListPriceCode { get; set; }
        public string TaxCode { get; set; }
        public decimal SellingPrice { get; set; }
        public string JobNarrations { get; set; }
        public string SalesOrderAddText { get; set; }

        public class DepartmentList
        {
            public string Department { get; set; }
            public string StockCode { get; set; }
        }
        public List<DepartmentList> Departments { get; set; }


        public List<sp_MasterCardGetStockCodeArtwork_Result> Multimedia { get; set; }
    }
}