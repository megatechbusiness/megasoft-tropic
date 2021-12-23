using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Megasoft2.BusinessLogic
{
    public class WickettLabelDeleteBL
    {
        private SysproCore objSyspro = new SysproCore();
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        private MegasoftEntities mdb = new MegasoftEntities();
        private LabelPrint objPrint = new LabelPrint();

        public string GetCompany()
        {
            HttpCookie database = HttpContext.Current.Request.Cookies.Get("SysproDatabase");
            var company = (from a in mdb.mtSysproAdmins where a.DatabaseName == database.Value select a.Company).FirstOrDefault();
            return company;
        }

        public string GetUsername()
        {
            var Username = HttpContext.Current.User.Identity.Name.ToUpper();
            return Username;
        }

        public mtWhseManSetting GetSettings()
        {
            var settings = (from a in wdb.mtWhseManSettings where a.SettingId == 1 select a).FirstOrDefault();
            return settings;
        }
        

        public MegasoftEntities GetMegasoftEntities()
        {
            return mdb;
        }

        public WarehouseManagementEntities GetWarehouseManagementEntities()
        {
            return wdb;
        }

        public string ValidateBarcode(string details)
        {
            try
            {
                List<WickettLabelDelete> myDeserializedObjList = (List<WickettLabelDelete>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WickettLabelDelete>));
                if (myDeserializedObjList.Count > 0)
                {
                    foreach (var item in myDeserializedObjList)
                    {
                        var Job = item.Job.PadLeft(15, '0');
                        var postOk = true;
                        var check = (from a in wdb.mtProductionLabels where a.Job == Job select a).FirstOrDefault();
                        var lot = (from b in wdb.LotDetails where b.Lot == check.BatchId select b).ToList();

                        //Check if lot No matches barcode lot
                        if (lot.Count > 0)
                        {
                            postOk = false;
                            return "Error: Label cannot be deleted. " + check.Job;
                        }
                        //check if scanned ="Y"
                        else if (check.Scanned == "Y")
                        {
                            postOk = false;
                            return "Error: Batch is already scanned. " + check.Job;
                        }
                        //Check if label is receipted
                        else if (check.LabelReceipted == "Y")
                        {
                            postOk = false;
                            return "Error: Job is already receipted. " + check.Job;
                        }
                        else
                        {
                            postOk = true;
                        }

                        if (postOk)
                        {
                            List<WhseManJobReceipt> detail = (List<WhseManJobReceipt>)Newtonsoft.Json.JsonConvert.DeserializeObject(details, typeof(List<WhseManJobReceipt>));
                            if (detail.Count > 0)
                            {
                                string Jobs = detail.FirstOrDefault().Job.PadLeft(15, '0');
                                string Pallet = detail.FirstOrDefault().Lot;
                                wdb.sp_DeleteProductionLabel(Jobs, Pallet, HttpContext.Current.User.Identity.Name.ToUpper());
                                return "Label " + Jobs + " deleted successfully";
                            }
                            else
                            {
                                return "No data found.";
                            }
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }
        }
    }
}