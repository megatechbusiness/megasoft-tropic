using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class LabourPostControl
    {
        clsData data = new clsData();
        public DataTable GetTimes(string date)
        {
            try
            {
                return data.SelectData("SELECT * FROM tpLabourPostControl WITH(NOLOCK) WHERE Date = '" + date + "' ");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }

        }
        public DataTable GetCostCentre()
        {
            try
            {
                return data.SelectData("EXEC sp_GetCostCentreControl");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CheckTimes(string date)
        {
            try
            {
                // bool val;
                DataTable dp = new DataTable();
                //dp.Columns.Add("Status", typeof(byte));

                dp = data.SelectData("SELECT * FROM tpLabourPostControl WITH(NOLOCK) WHERE Date = '" + date + "' ");

                if (dp.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}