using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class Scheduler
    {
        clsData Data = new clsData();

        public DataTable GetCostCentres()
        {
            try
            {
                return Data.SelectData("SELECT CostCentre, Description FROM BomCostCentre (NOLOCK)");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetWorkCentres(string CostCentre)
        {
            try
            {
                return Data.SelectData("SELECT WorkCentre FROM BomWorkCentre (NOLOCK) WHERE CostCentre = '" + CostCentre + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetOutstandingJobs(string CostCentre, string WorkCentre, string DeliveryDate, string SortBy)
        {
            try
            {
                return Data.SelectData("exec sp_Schedule_JobsToSchedule '" + CostCentre + "','" + WorkCentre + "','" + DeliveryDate + "','" + SortBy + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetActiveSchedule(string Workcentre)
        {
            try
            {
                return Data.SelectData("exec sp_GetActiveSchedule '" + Workcentre + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BackupScheduleGetGuid(string Workcentre, string AppUser)
        {
            try
            {
                return Data.SelectData("exec sp_BackupSchedule '" + Workcentre + "','" + AppUser + "'").Rows[0][0].ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SaveSchedule(string ScheduleGuid, string WorkCentre, string Job, string StockCode, decimal QtytoMake, decimal QtyToPlan, decimal Produced, decimal Balance, string JobDeliveryDate, int ScheduleComplete, string AppUser)
        {
            try
            {
                Data.Execute("exec sp_SaveActiveSchedule '" + ScheduleGuid + "','" + WorkCentre + "','" + Job + "','" + StockCode + "'," + QtytoMake + "," + QtyToPlan + "," + Produced + "," + Balance + ",'" + JobDeliveryDate + "'," + ScheduleComplete + ",'" + AppUser + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}