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
            wdb = new WarehouseManagementEntities("");
            mdb = new MegasoftEntities();
            sys = new SysproEntities();
        }

        public PalletMatIssueViewModel LoadJob(PalletMatIssueViewModel model)
        {
            try
            {
                if (model.Job == null || model.Pallet == null)
                {
                    model.Messages = "Please Enter values into Both Fields";
                    return model;
                }
                else
                {
                    //make sure Job is a 15-digit number
                    var jobPad = (from a in wdb.mtWhseManSettings select a.JobNumberPadZeros).ToList();
                    var padding = jobPad[0];
                    var jobCode = model.Job;
                    if (padding == true)
                    {
                        jobCode = model.Job.PadLeft(15, '0');
                    }

                    //retrieving and populating fields regarding the Job
                    var JobList = wdb.mt_PalletMatIssueGetJobDetails(jobCode).FirstOrDefault();
                    var palletList = wdb.mt_PalletMatIssueGetPalletDetails(model.Pallet).ToList();
                    var noPallet = (from a in wdb.CusLot_ where a.PalletId == model.Pallet select a).FirstOrDefault();

                    if (JobList == null)
                    {
                        model.Messages = "Job not Found.";
                    }
                    else if(noPallet == null)
                    {
                        model.Messages = "Pallet not Found.";
                    }
                    else if(palletList.Count == 0)
                    {
                        model.Messages = "Pallet Quantity is Zero.";
                    }
                    else
                    {
                        model.StockCode = JobList.StockCode;
                        model.StockDescription = JobList.StockDescription;
                        model.JobDescription = JobList.JobDescription;
                        model.QtyToMake = JobList.QtyToMake;
                        model.QtyManufactured = JobList.QtyManufactured;
                        model.Job = jobCode;
                        model.PalletList = palletList;
                    }

                    return model;
                }
            }
            catch (Exception ex)
            {
                model.Messages = ex.Message;
                return model;
            }

        }


    }
}