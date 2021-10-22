using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class TransportSystemWaybillEntryViewModel
    {
        public int TrackId { get; set; }
        public string Transporter { get; set; }
        public string RegNo { get; set; }
        public string Driver { get; set; }
        public List<sp_GetTransWaybillDetailByTrackId_Result> Detail { get; set; }
        public string Waybill { get; set; }
        public string DispatchNote { get; set; }
        public int DispatchNoteLine { get; set; }
        public string Customer { get; set; }
        public string Province { get; set; }
        public string Town { get; set; }
        public string StockCode { get; set; }
        public string StockDesc { get; set; }
        public decimal DispatchQty { get; set; }
        public string DispatchUom { get; set; }
        public decimal LoadQty { get; set; }
        public string LoadUom { get; set; }
        public decimal Pallets { get; set; }
        public decimal Weight { get; set; }
        public string Notes { get; set; }
        public List<sp_GetTransporterPoLines_Result> PoLines { get; set; }
        public string PurchaseOrder { get; set; }
        public string Supplier { get; set; }
        public string SupplierName { get; set; }
        public string Invoice { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        //public List<sp_GetTransWaybillDetailByTrackId_Result> TransDetail { get; set; }
        public DateTime DeliveryDate { get; set; }
        public bool WaybillReturn { get; set; }
        public Nullable<DateTime> PODDate { get; set; }
        public string PODComment { get; set; }
        public ExportFile PrintPdf { get; set; }

    }
}