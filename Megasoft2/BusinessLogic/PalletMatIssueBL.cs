using Megasoft2.ViewModel;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.UI.WebControls.WebParts;

namespace Megasoft2.BusinessLogic
{
    public class PalletMatIssueBL
    {
        private WarehouseManagementEntities wdb;
        private MegasoftEntities mdb;
        private SysproEntities sys;
        public PalletMatIssueBL()
        {
            wdb = new WarehouseManagementEntities();
            mdb = new MegasoftEntities();
            sys = new SysproEntities();
        }

        public PalletMatIssueViewModel LoadJob(PalletMatIssueViewModel model)
        {
            try
            {
                //amke sure Job is a 15-digit number
                var jobPad = (from a in wdb.mtWhseManSettings select a.JobNumberPadZeros).ToList();
                var padding = jobPad[0];
                var jobCode = model.Job;
                if (padding == true && jobCode != null )
                {
                    jobCode = model.Job.PadLeft(15, '0');
                }
                else
                {
                    model.Messages = "Please Enter values into Both Fields";
                    return model;
                }

                //retrieving and populating fields regarding the Job
                var JobList = wdb.mt_PalletMatIssueGetJobDetails(jobCode).FirstOrDefault();
                var palletList = wdb.mt_PalletMatIssueGetPalletDetails(model.Pallet).ToList();

                if (JobList != null && palletList.Count != 0)
                {
                    model.StockCode = JobList.StockCode;
                    model.StockDescription = JobList.StockDescription;
                    model.JobDescription = JobList.JobDescription;
                    model.QtyToMake = JobList.QtyToMake;
                    model.QtyManufactured = JobList.QtyManufactured;
                    model.Job = jobCode;
                    model.PalletList = palletList;
                }
                else
                {
                    model.Messages = "Please Enter values into Both Fields";
                }

                
                return model;
            }
            catch (Exception ex)
            {
                model.Messages = ex.Message;
                return model;
            }

        }

        
    }
}