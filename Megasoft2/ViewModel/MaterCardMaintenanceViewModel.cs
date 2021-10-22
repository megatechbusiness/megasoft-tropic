using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class MaterCardMaintenanceViewModel
    {
        public int Id { get; set; }
        public string StockCode { get; set; }
        public sp_MasterCardGetWarehouseUpdate_Result AddWarehouse { get; set; }
        public mtMasterCardBomOperationsUpdate Oupdate { get; set; }
        public mtMasterCardBomStructureUpdate SUpdate { get; set; }
        public mtMasterCardStockCodeCustomFormUpdate FormUpdate { get; set; }
        public mtMasterCardStockCodeWarehouseUpdate warehouseUpdate { get; set; }
        public mtMasterCardStockCodeUpdate stockCodeUpdate { get; set; }
        public sp_MasterCardGetSysproBomStructure_Result BomStructure { get; set; }
        public sp_MasterCardGetSysproBomOperations_Result BomOperation { get; set; }
        public List<sp_MasterCardGetWarehouseUpdate_Result> warehouseUpdate_Result { get; set; }
        public List<mtMasterCardUpdateAudit> updateAudits { get; set; }


        public List<sp_MasterCardGetAttachmentsByStockCode_Result> AttachmentList;

        public IEnumerable<HttpPostedFileBase> FileUpload { get; set; }
        public List<string> Image { get; set; }

    }
}