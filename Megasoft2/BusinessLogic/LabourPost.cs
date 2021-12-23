using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{

    public class LabourPost
    {
        private WarehouseManagementEntities wdb = new WarehouseManagementEntities("");
        clsData data = new clsData();
        //Get List of Departments
        public DataTable GetCostCentre()
        {
            try
            {
                return data.SelectData("EXEC sp_GetCostCentre");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //Operations
        public DataTable GetOperations(string Job, string CostCentre)
        {
            try
            {
                return data.SelectData("EXEC sp_GetOperations '" + Job + "','" + CostCentre + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //Get Last operation
        public DataTable GetPreviousOperation(string Job, int Operation)
        {
            try
            {
                return data.SelectData("EXEC sp_GetPreviousOperation '" + Job + "','" + Operation + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //Get Shifts For ddl
        public DataTable Shifts()
        {
            try
            {
                return data.SelectData("SELECT Shift,ShiftID FROM mtShifts WITH (NOLOCK)");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //Get ShiftDetails
        public DataTable GetShiftDetails(string ShiftID)
        {
            try
            {
                return data.SelectData("EXEC sp_GetAllShifts'" + ShiftID + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //Get Time Code
        public DataTable GetTimeCode(string CostCentre)
        {
            try
            {
                return data.SelectData("EXEC sp_GetTimeCodes'" + CostCentre + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //Get Jobs
        public DataTable GetCheckJobs(string Department, string Job)
        {
            try
            {
                return data.SelectData("EXEC sp_GetDeptJobs'" + Department + "','" + Job + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //GET ScrapCodes
        public DataTable GetScrapCode()
        {
            try
            {
                return data.SelectData("EXEC sp_GetScrapCodes");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //GetScrapData from screen


        //Get Job Details
        public DataTable GetJobDetails(string Job)
        {
            try
            {
                return data.SelectData("EXEC sp_GetJobDetails'" + Job + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //GetWorkCentre
        public DataTable GetWorkCentre(string Department)
        {
            try
            {
                return data.SelectData("EXEC sp_GetWorkCentre'" + Department + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildLabourPostDocument(DataTable dt, DataTable ds)
        {
            try
            {
                string Job = "";
                string Date = "";
                string Operation = "";
                string WorkCentre = "";
                string Shift = "";
                string CloseJob = "";
                string Reference = "";
                string StockCode = "";
                string Department = "";
                string TotalProdQty = "";
                string TimeCodeID = "";


                DataRow[] FirstRow = dt.Select("row = '1'");
                foreach (DataRow row in FirstRow)
                {
                    Job = row["Job"].ToString();
                    Date = row["EntryDate"].ToString();
                    Operation = row["Operation"].ToString();
                    WorkCentre = row["WorkCentre"].ToString();
                    Shift = row["Shift"].ToString();
                    CloseJob = row["CloseJob"].ToString();
                    Reference = row["Reference"].ToString();
                    StockCode = row["StockCode"].ToString();
                    Department = row["Department"].ToString();
                    TimeCodeID = row["TimeCodeID"].ToString();
                    TotalProdQty = row["ProductionMass"].ToString();
                }

                decimal TotalScrap = 0;
                decimal RunTime = 0;
                decimal SetupTime = 0;
                decimal StockMass = GetStockMass(StockCode);
                if (StockMass == 0)
                { StockMass = 1; }
                decimal TotalProduced = 0;



                RunTime = dt.AsEnumerable().Where(y => y.Field<string>("TimeCodeTrnType") == "R").Sum(r => Convert.ToDecimal(r.Field<string>("ElapsedTime")));
                SetupTime = dt.AsEnumerable().Where(y => y.Field<string>("TimeCodeTrnType") == "S").Sum(r => Convert.ToDecimal(r.Field<string>("ElapsedTime")));
                TotalScrap = ds.AsEnumerable().Sum(r => Convert.ToDecimal(r.Field<string>("ScrapAmount"))) / StockMass;

                if (Department.Trim() != "BAG" && Department.Trim() != "WICKET")
                {
                    TotalProduced = Convert.ToDecimal(TotalProdQty) / StockMass;
                }
                else
                {
                    TotalProduced = Convert.ToDecimal(TotalProdQty);
                }


                if (GetCostCentreUnit(Department) == "KG")
                {
                    TotalProduced = Convert.ToDecimal(TotalProduced);
                }
                else
                {
                    TotalProduced = Convert.ToDecimal(TotalProduced) / 1000;
                }

                //Declaration
                StringBuilder Document = new StringBuilder();
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the WIP Labor Posting Business Object");
                Document.Append("-->");
                Document.Append("<PostLabour xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTLPDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Journal/>");
                Document.Append("<ItemTransactionDate>" + Date + "</ItemTransactionDate>");
                Document.Append("<TransactionTime></TransactionTime>");
                Document.Append("<Job>" + Job + "</Job>");
                Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                Document.Append("<LOperation>" + Operation + "</LOperation>");
                Document.Append("<LWorkCentre>" + WorkCentre + "</LWorkCentre>");
                Document.Append("<LWcRateInd>1</LWcRateInd>");
                if (RunTime != 0 || SetupTime != 0)
                {
                    Document.Append("<LRunTimeHours>" + Math.Round(RunTime, 3) + "</LRunTimeHours>");
                    Document.Append("<LSetUpHours>" + Math.Round(SetupTime, 3) + "</LSetUpHours>");
                }
                Document.Append("<ManualWorkCenterRates>N</ManualWorkCenterRates>");



                Document.Append("<Reference>" + GetShiftReference(Shift) + "</Reference>");
                if (Reference != "")
                {
                    Document.Append("<AdditionalReference>" + Reference + "</AdditionalReference>");
                }

                decimal ScrapToPost = 0;

                //If only 1 row of scrap
                if (ds.Rows.Count == 1)
                {
                    foreach (DataRow srow in ds.Rows)
                    {
                        decimal Qty = Convert.ToDecimal(srow["ScrapAmount"].ToString().Trim());

                        Document.Append("<MultipleScrapEntries>N</MultipleScrapEntries>");
                        Document.Append("<ScrapCode>" + srow["ScrapCode"].ToString().Trim() + "</ScrapCode>");

                    }

                    ScrapToPost = TotalScrap;

                }
                //if multiple rows
                else
                {
                    Document.Append("<MultipleScrapEntries>Y</MultipleScrapEntries>");

                    foreach (DataRow srow in ds.Rows)
                    {
                        decimal Qty = Convert.ToDecimal(srow["ScrapAmount"].ToString().Trim());
                        decimal TotalQty = Math.Round(Qty / StockMass, 3);
                        Document.Append("<MultipleScrap>");
                        Document.Append("<MultipleScrapCode>" + srow["ScrapCode"].ToString().Trim() + "</MultipleScrapCode>");
                        Document.Append("<MultipleScrapQty>" + string.Format("{0:#######0.000}", TotalQty) + "</MultipleScrapQty>");
                        Document.Append("</MultipleScrap>");
                        ScrapToPost += TotalQty;
                    }

                }

                if (TotalProduced != 0)
                {
                    Document.Append("<LQtyComplete>" + Math.Round(TotalProduced, 3) + "</LQtyComplete>");
                }
                if (TotalScrap != 0)
                {
                    Document.Append("<LQtyScrapped>" + string.Format("{0:#######0.000}", ScrapToPost) + "</LQtyScrapped>");
                }
                if (CloseJob == "True")
                {
                    Document.Append("<OperCompleted>Y</OperCompleted>");
                }
                else
                {
                    Document.Append("<OperCompleted>N</OperCompleted>");
                }
                Document.Append("</Item>");




                //if Shift Has NonProductive Time
                foreach (DataRow r in dt.AsEnumerable().Where(n => n.Field<string>("TimeCodeTrnType") == "N"))
                {
                    DataTable check = CheckTimeCode(r["TimeCodeID"].ToString().Trim());
                    if (check.Rows.Count == 1)
                    {
                        SetupTime = SetupTime + Convert.ToDecimal(r["ElapsedTime"].ToString());
                    }
                    else
                    {

                        Document.Append("<Item>");
                        Document.Append("<Journal />");
                        Document.Append("<ItemTransactionDate>" + Date + "</ItemTransactionDate>");
                        Document.Append("<Job>" + Job + "</Job>");
                        Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                        Document.Append("<LOperation>" + Operation + "</LOperation>");
                        Document.Append("<LWorkCentre>" + WorkCentre + "</LWorkCentre>");
                        Document.Append("<LWcRateInd>1</LWcRateInd>");
                        Document.Append("<LRunTimeHours>" + Math.Round(Convert.ToDecimal(r["ElapsedTime"].ToString()), 3) + "</LRunTimeHours>");
                        Document.Append("<NonProductiveCode>" + r["TimeCodeID"].ToString().Trim() + "</NonProductiveCode>");
                        Document.Append("<ManualWorkCenterRates>N</ManualWorkCenterRates>");
                        Document.Append("<Reference>" + GetShiftReference(Shift) + "</Reference>");
                        if (Reference != "")
                        {
                            Document.Append("<AdditionalReference>" + Reference + "</AdditionalReference>");
                        }
                        Document.Append("</Item>");
                    }
                }
                Document.Append("</PostLabour>");

                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string BuildLabourPostParameter(string Date)
        {
            try
            {
                //Declaration
                StringBuilder Parameter = new StringBuilder();

                //Building Parameter content
                Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Parameter.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Parameter.Append("<!--");
                Parameter.Append("This is an example XML instance to demonstrate");
                Parameter.Append("use of the parameters for the WIP Labor Posting Business Object");
                Parameter.Append("-->");
                Parameter.Append("<PostLabour xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTLP.XSD\">");
                Parameter.Append("<Parameters>");
                Parameter.Append("<ValidateOnly>N</ValidateOnly>");
                Parameter.Append("<ApplyIfEntireDocumentValid>N</ApplyIfEntireDocumentValid>");
                Parameter.Append("<IgnoreWarnings>Y</IgnoreWarnings>");
                Parameter.Append("<PostingPeriod>C</PostingPeriod>");
                Parameter.Append("<TransactionDate>" + Date + "</TransactionDate>");
                Parameter.Append("<UpdateQtyToMakeWithScrap>N</UpdateQtyToMakeWithScrap>");
                Parameter.Append("<UncompleteNonMile>N</UncompleteNonMile>");
                Parameter.Append("<UseWCRateIfEmpRateZero>N</UseWCRateIfEmpRateZero>");
                Parameter.Append("</Parameters>");
                Parameter.Append("</PostLabour>");


                return Parameter.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetOperatorID(string Username)
        {
            try
            {
                string name = string.Empty;
                DataTable dp = new DataTable();
                dp.Columns.Add("Name", typeof(decimal));

                dp = data.SelectData("SELECT SysproUser FROM tpOpMaster WITH (NOLOCK) WHERE Name = '" + Username.ToUpper() + "'");
                name = dp.Rows[0][0].ToString();
                return name;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public DataTable TimeCode()
        {
            try
            {
                return data.SelectData("SELECT * FROM mtTimeCodes WITH(NOLOCK)");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }

        }
        public DataTable GetTimeCodeTypeID(int Id)
        {
            try
            {
                return data.SelectData("SELECT TimeCodeTypeID FROM mtTimeCodes WITH(NOLOCK) WHERE TimeCodeID = '" + Id + "'");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }
        }
        public DataTable TimeCodeType()
        {
            try
            {
                return data.SelectData("SELECT * FROM mtTimeCodeTypes WITH(NOLOCK)");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }

        }
        public void UpdateTimeCodes(int TimeCodeId, string Description, string SysproCode)
        {

            try
            {
                data.Execute("Update mtTimeCodes Set Description ='" + Description + "', SysproCode = '" + SysproCode + "' WHERE SysproCode = '" + SysproCode + "' ");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public void AddTimeCodes(int TimeCodeID, string Description, string SysproId)
        {

            try
            {
                data.Execute("INSERT INTO mtTimeCodes(TimeCodeID,Description,SysproCode) VALUES ('" + TimeCodeID + "','" + Description + "','" + SysproId + "')");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public void SaveLabour(DataTable dts, DataTable ds)
        {

            try
            {
                string Job = "";
                string Date = "";
                string WorkCentre = "";
                string Shift = "";
                string Department = "";
                string Reference = "";
                string GuidToSave = Guid.NewGuid().ToString();


                DataRow[] FirstRow = dts.Select("row = '1'");
                foreach (DataRow row in FirstRow)
                {
                    Job = row["Job"].ToString();
                    Date = row["EntryDate"].ToString();
                    WorkCentre = row["WorkCentre"].ToString();
                    Shift = row["Shift"].ToString();
                    Department = row["Department"].ToString();
                    Reference = row["Reference"].ToString();

                }

                bool QuantityApplied = false;


                foreach (DataRow row in dts.Rows)
                {

                    decimal StartTime = 0;
                    decimal EndTime = 0;

                    if (row["StartTime"].ToString().Trim() != "")
                    {
                        StartTime = Convert.ToDecimal(TimeSpan.Parse(row["StartTime"].ToString().Trim()).TotalHours);
                    }
                    if (row["EndTime"].ToString().Trim() != "")
                    {
                        EndTime = Convert.ToDecimal(TimeSpan.Parse(row["EndTime"].ToString().Trim()).TotalHours);
                    }


                    string StartDate = row["EntryDate"].ToString().Trim() + " " + row["StartTime"].ToString().Trim();
                    string EndDate = row["EntryDate"].ToString().Trim() + " " + row["EndTime"].ToString().Trim();


                    if (CheckNextDay(Convert.ToInt32(row["Shift"].ToString().Trim())) == true && EndTime < 18)
                    {

                        DateTime endDate = Convert.ToDateTime(row["EntryDate"].ToString().Trim() + " " + row["EndTime"].ToString().Trim());
                        endDate = endDate.AddDays(1);
                        DateTime end = endDate;
                        EndDate = end.ToString("s");
                        //Also next day but same shift
                        if (StartTime < 6 && StartTime >= 0)
                        {
                            DateTime startDate = Convert.ToDateTime(row["EntryDate"].ToString().Trim() + " " + row["StartTime"].ToString().Trim());
                            startDate = startDate.AddDays(1);
                            DateTime start = startDate;
                            StartDate = start.ToString("s");
                        }
                    }

                    //Convert Time from Hours to decimal
                    decimal StockMass = GetStockMass(row["StockCode"].ToString().Trim());
                    if (StockMass == 0)
                    { StockMass = 1; }

                    decimal ElapsedTime = Convert.ToDecimal(row["ElapsedTime"].ToString().Trim()); ;


                    decimal TotalProduced = 0;
                    decimal TotalProdQty = Convert.ToDecimal(row["ProductionMass"].ToString());

                    if (Department.Trim() != "BAG" && Department.Trim() != "WICKET")
                    {
                        TotalProduced = Convert.ToDecimal(TotalProdQty) / StockMass;
                    }
                    else
                    {
                        TotalProduced = Convert.ToDecimal(TotalProdQty);
                    }


                    if (GetCostCentreUnit(Department) == "KG")
                    {
                        TotalProduced = Convert.ToDecimal(TotalProduced);
                    }
                    else
                    {
                        TotalProduced = Convert.ToDecimal(TotalProduced) / 1000;
                    }

                    //JR 21122021 - Run time - Save quantity for runtime only and only for the first entry of the session.
                    if (QuantityApplied == false)
                    {
                        if (row["TimeCodeTrnType"].ToString() == "R")
                        {
                            QuantityApplied = true;
                            data.Execute(" INSERT INTO mtLabourTimes(EntryDate,Shift,WorkCentre,Job,Operation,TimeCodeID,StartTime,EndTime,ElapsedTime,ProductionMass,Operator,ProductionQtyEnt,Guid,AddReference) VALUES ('" + row["EntryDate"].ToString().Trim() + "','" + row["Shift"].ToString().Trim() + "','" + row["WorkCentre"].ToString().Trim() + "','" + row["Job"].ToString().Trim() + "','" + Convert.ToDecimal(row["Operation"].ToString().Trim()) + "','" + row["TimeCodeID"].ToString().Trim() + "','" + StartDate + "','" + EndDate + "','" + ElapsedTime + "','" + TotalProduced + "','" + row["Operator"].ToString().Trim() + "','" + Convert.ToDecimal(row["ProductionMass"].ToString()) + "','" + GuidToSave + "','" + Reference + "')");
                        }
                        else
                        {
                            data.Execute(" INSERT INTO mtLabourTimes(EntryDate,Shift,WorkCentre,Job,Operation,TimeCodeID,StartTime,EndTime,ElapsedTime,ProductionMass,Operator,ProductionQtyEnt,Guid,AddReference) VALUES ('" + row["EntryDate"].ToString().Trim() + "','" + row["Shift"].ToString().Trim() + "','" + row["WorkCentre"].ToString().Trim() + "','" + row["Job"].ToString().Trim() + "','" + Convert.ToDecimal(row["Operation"].ToString().Trim()) + "','" + row["TimeCodeID"].ToString().Trim() + "','" + StartDate + "','" + EndDate + "','" + ElapsedTime + "','" + 0 + "','" + row["Operator"].ToString().Trim() + "','" + 0 + "','" + GuidToSave + "','" + Reference + "')");
                        }
                    }
                    else
                    {
                        data.Execute(" INSERT INTO mtLabourTimes(EntryDate,Shift,WorkCentre,Job,Operation,TimeCodeID,StartTime,EndTime,ElapsedTime,ProductionMass,Operator,ProductionQtyEnt,Guid,AddReference) VALUES ('" + row["EntryDate"].ToString().Trim() + "','" + row["Shift"].ToString().Trim() + "','" + row["WorkCentre"].ToString().Trim() + "','" + row["Job"].ToString().Trim() + "','" + Convert.ToDecimal(row["Operation"].ToString().Trim()) + "','" + row["TimeCodeID"].ToString().Trim() + "','" + StartDate + "','" + EndDate + "','" + ElapsedTime + "','" + 0 + "','" + row["Operator"].ToString().Trim() + "','" + 0 + "','" + GuidToSave + "','" + Reference + "')");
                    }



                }
                string Ref = GetShiftReference(Shift);
                foreach (DataRow srow in ds.Rows)
                {
                    data.Execute(" INSERT INTO mtScrap(Job,Date,WorkCentre,Reference,ScrapCode,Quantity,Guid) VALUES ('" + Job + "','" + Date + "','" + WorkCentre + "','" + Ref.Trim() + "','" + srow["ScrapCode"].ToString().Trim() + "','" + Math.Round(Convert.ToDecimal(srow["ScrapAmount"].ToString().Trim()), 3) + "','" + GuidToSave + "')");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Saving Job: " + ex.Message);
            }

        }

        public void DeleteTimeCode(int TimeCodeID)
        {

            try
            {
                data.Execute("Delete FROM mtTimeCodes WHERE SysproCode = '" + TimeCodeID + "'");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public DataTable GetScrapCodeDesc()
        {
            try
            {
                return data.SelectData("EXEC sp_GetScrapCodeDesc");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public decimal GetStockMass(string StockCode)
        {
            try
            {
                decimal mass;
                DataTable dp = new DataTable();
                dp.Columns.Add("Mass", typeof(decimal));

                dp = data.SelectData("EXEC sp_GetStockMass '" + StockCode + "'");
                mass = Convert.ToDecimal(dp.Rows[0][0].ToString());
                return mass;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string GetShiftReference(string id)
        {

            try
            {
                string Reference;
                DataTable dp = new DataTable();
                dp.Columns.Add("Reference", typeof(decimal));

                dp = data.SelectData("SELECT Reference FROM mtShifts WITH (NOLOCK) WHERE ShiftID = '" + id + "'");
                Reference = dp.Rows[0][0].ToString();
                return Reference;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public string GetCostCentreUnit(string Dept)
        {

            try
            {
                string Department;
                DataTable dp = new DataTable();
                dp.Columns.Add("Dept", typeof(decimal));

                dp = data.SelectData("SELECT CostCentreUnit FROM mtCostCentreUnits WITH (NOLOCK) WHERE CostCentre = '" + Dept + "'");
                Department = dp.Rows[0][0].ToString();
                return Department;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool CheckNextDay(int ShiftID)
        {

            try
            {
                bool val;
                DataTable dp = new DataTable();
                dp.Columns.Add("NextDay", typeof(int));

                dp = data.SelectData("SELECT NextDay FROM mtShifts WITH (NOLOCK) WHERE ShiftID = '" + ShiftID + "'");
                val = Convert.ToBoolean(dp.Rows[0][0].ToString());
                if (val == true)
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

        public DataTable CheckTimeCode(string SysproID)
        {
            try
            {
                return data.SelectData("SELECT * FROM mtTimeCodes WITH(NOLOCK) WHERE SysproCode = '" + SysproID + "' ");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }

        }
        public decimal CheckJobTime(string WorkCentre, string date)
        {

            try
            {
                decimal val;
                DataTable dp = new DataTable();
                dp.Columns.Add("TotalTime", typeof(decimal));

                dp = data.SelectData("EXEC sp_CheckJobTime '" + WorkCentre + "','" + date + "'");
                if (dp.Rows[0][0].ToString() == "")
                {
                    return val = 0;
                }
                else
                {
                    val = Convert.ToDecimal(dp.Rows[0][0].ToString());
                    return val;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public decimal Capacity(string WorkCentre, string Date)
        {

            try
            {
                decimal val;
                DataTable dp = new DataTable();
                dp.Columns.Add("Capacity", typeof(decimal));

                dp = data.SelectData("SELECT Capacity FROM fn_WorkCentreCapacity('" + WorkCentre.Trim() + "','" + Date + "','" + Date + "')");
                if (dp.Rows.Count == 0)
                {
                    return val = 0;
                }
                else
                {
                    val = Convert.ToDecimal(dp.Rows[0][0].ToString());
                    return val;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool CheckIfClosed(string date, string WorkCentre)
        {
            try
            {
                bool val;
                DataTable dp = new DataTable();
                //dp.Columns.Add("Status", typeof(byte));

                dp = data.SelectData("SELECT Status FROM mtLabourPostControl WITH(NOLOCK) WHERE Date = '" + date + "' AND CostCentre='" + WorkCentre + "' ");

                if (dp.Rows.Count > 0)
                {
                    val = Convert.ToBoolean(dp.Rows[0][0].ToString());
                    if (val == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
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
        public void SaveLabourReversal(DataTable dts, DataTable ds, string Username)
        {
            try
            {

                string Job = "";
                string Date = "";
                string WorkCentre = "";
                string Shift = "";
                string GuidToSave = "";

                Job = dts.Rows[0]["Job"].ToString();
                DateTime a = Convert.ToDateTime(dts.Rows[0]["EntryDate"].ToString());
                Date = a.ToString("yyyy/MM/dd");
                WorkCentre = dts.Rows[0]["WorkCentre"].ToString();
                Shift = dts.Rows[0]["Shift"].ToString();
                GuidToSave = dts.Rows[0]["Guid"].ToString().Trim();

                foreach (DataRow row in dts.Rows)
                {
                    //Convert Time from Hours to decimal
                    decimal StockMass = wdb.sp_GetJobStockMass(Job.PadLeft(15, '0')).FirstOrDefault().Mass;
                    if (StockMass == 0)
                    { StockMass = 1; }

                    decimal ElapsedTime = Convert.ToDecimal(row["ElapsedTime"].ToString());
                    DateTime entry = Convert.ToDateTime(Date);
                    Date = entry.ToString("s");
                    string StartTime = row["StartTime"].ToString().Trim();
                    string EndTime = row["EndTime"].ToString().Trim();
                    DateTime start = Convert.ToDateTime(StartTime);
                    StartTime = start.ToString("s");
                    DateTime end = Convert.ToDateTime(EndTime);
                    EndTime = end.ToString("s");

                    data.Execute("INSERT INTO mtLabourTimes(EntryDate,Shift,WorkCentre,Job,Operation,TimeCodeID,StartTime,EndTime,ElapsedTime,ProductionMass,Operator,ProductionQtyEnt,AddReference,Guid) VALUES ('" + Date + "','" + row["Shift"].ToString().Trim() + "','" + row["WorkCentre"].ToString().Trim() + "','" + row["Job"].ToString().Trim() + "','" + Convert.ToDecimal(row["Operation"].ToString().Trim()) + "','" + row["TimeCodeID"].ToString().Trim() + "','" + StartTime + "','" + EndTime + "','" + ElapsedTime + "','" + row["ProductionQuantity"].ToString().Trim() + "','" + Username + "','" + Convert.ToDecimal(row["ProductionQtyEnt"].ToString()) + "','" + row["AddReference"].ToString() + "','" + GuidToSave + "')");
                }
                foreach (DataRow srow in ds.Rows)
                {

                    data.Execute("INSERT INTO mtScrap(Job,Date,WorkCentre,Reference,ScrapCode,Quantity,Guid) VALUES ('" + Job + "','" + Date + "','" + WorkCentre + "','" + srow["Shift"].ToString().Trim() + "','" + srow["ScrapCode"].ToString().Trim() + "','" + Math.Round(Convert.ToDecimal(srow["ScrapQuantity"].ToString().Trim()), 3) + "','" + GuidToSave + "')");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Saving Job: " + ex.Message);
            }

        }
        public string BuildLabourPostReversalDocument(DataTable dt, DataTable ds)
        {
            try
            {
                string Job = "";
                string Date = "";
                string Operation = "";
                string WorkCentre = "";
                string Shift = "";
                string CloseJob = "";
                string Reference = "";
                string AddReference = "";
                string TotalProdQty = "";
                string TimeCodeID = "";


                Job = dt.Rows[0]["Job"].ToString();
                DateTime a = Convert.ToDateTime(dt.Rows[0]["EntryDate"].ToString());
                Date = a.ToString("yyyy-MM-dd");
                Operation = dt.Rows[0]["Operation"].ToString();
                WorkCentre = dt.Rows[0]["WorkCentre"].ToString();
                Shift = dt.Rows[0]["Shift"].ToString();
                Reference = dt.Rows[0]["Reference"].ToString();
                TimeCodeID = dt.Rows[0]["TimeCodeID"].ToString();
                TotalProdQty = dt.Rows[0]["ProductionQuantity"].ToString();
                AddReference = dt.Rows[0]["AddReference"].ToString();
                string TotalProdQtyEnt = dt.Rows[0]["ProductionQtyEnt"].ToString();

                decimal TotalScrap = 0;
                decimal RunTime = 0;
                decimal SetupTime = 0;
                decimal TotalProduced = Convert.ToDecimal(TotalProdQty);

                decimal StockMass = wdb.sp_GetJobStockMass(Job.PadLeft(15, '0')).FirstOrDefault().Mass;
                if (StockMass == 0)
                { StockMass = 1; }

                RunTime = Convert.ToDecimal(dt.AsEnumerable().Where(y => y.Field<string>("TimeCodeTrnType") == "R").Sum(r => Convert.ToDecimal(r.Field<string>("ElapsedTime"))));
                SetupTime = dt.AsEnumerable().Where(y => y.Field<string>("TimeCodeTrnType") == "S").Sum(r => Convert.ToDecimal(r.Field<string>("ElapsedTime")));
                TotalScrap = ds.AsEnumerable().Sum(r => Convert.ToDecimal(r.Field<string>("ScrapQuantity"))) / StockMass;


                //Declaration
                StringBuilder Document = new StringBuilder();
                Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
                Document.Append("<!-- Copyright 1994-2010 SYSPRO Ltd.-->");
                Document.Append("<!--");
                Document.Append("This is an example XML instance to demonstrate");
                Document.Append("use of the WIP Labor Posting Business Object");
                Document.Append("-->");
                Document.Append("<PostLabour xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"WIPTLPDOC.XSD\">");
                Document.Append("<Item>");
                Document.Append("<Journal/>");
                Document.Append("<ItemTransactionDate>" + Date + "</ItemTransactionDate>");
                Document.Append("<TransactionTime></TransactionTime>");
                Document.Append("<Job>" + Job + "</Job>");
                Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                Document.Append("<LOperation>" + Operation + "</LOperation>");
                Document.Append("<LWorkCentre>" + WorkCentre + "</LWorkCentre>");
                Document.Append("<LWcRateInd>1</LWcRateInd>");
                if (RunTime != 0 || SetupTime != 0)
                {
                    Document.Append("<LRunTimeHours>" + Math.Round(RunTime, 3) + "</LRunTimeHours>");
                    Document.Append("<LSetUpHours>" + Math.Round(SetupTime, 3) + "</LSetUpHours>");
                }
                Document.Append("<ManualWorkCenterRates>N</ManualWorkCenterRates>");
                Document.Append("<Reference>" + Reference + "</Reference>");
                if (AddReference != "")
                {
                    Document.Append("<AdditionalReference>" + AddReference + "</AdditionalReference>");
                }

                decimal ScrapToPost = 0;

                //If only 1 row of scrap
                if (ds.Rows.Count == 1)
                {
                    foreach (DataRow srow in ds.Rows)
                    {
                        decimal Qty = Convert.ToDecimal(srow["ScrapQuantity"].ToString().Trim());

                        Document.Append("<MultipleScrapEntries>N</MultipleScrapEntries>");
                        Document.Append("<ScrapCode>" + srow["ScrapCode"].ToString().Trim() + "</ScrapCode>");

                    }

                    ScrapToPost = TotalScrap;

                }
                //if multiple rows
                else
                {
                    Document.Append("<MultipleScrapEntries>Y</MultipleScrapEntries>");

                    foreach (DataRow srow in ds.Rows)
                    {
                        decimal Qty = Convert.ToDecimal(srow["ScrapQuantity"].ToString().Trim());
                        decimal TotalQty = Math.Round(Qty / StockMass, 3);
                        Document.Append("<MultipleScrap>");
                        Document.Append("<MultipleScrapCode>" + srow["ScrapCode"].ToString().Trim() + "</MultipleScrapCode>");
                        Document.Append("<MultipleScrapQty>" + string.Format("{0:#######0.000}", TotalQty) + "</MultipleScrapQty>");
                        Document.Append("</MultipleScrap>");
                        ScrapToPost += TotalQty;
                    }

                }

                if (TotalProduced != 0)
                {
                    Document.Append("<LQtyComplete>" + Math.Round(TotalProduced, 3) + "</LQtyComplete>");
                }
                if (TotalScrap != 0)
                {
                    Document.Append("<LQtyScrapped>" + string.Format("{0:#######0.000}", ScrapToPost) + "</LQtyScrapped>");
                }
                if (CloseJob == "True")
                {
                    Document.Append("<OperCompleted>Y</OperCompleted>");
                }
                else
                {
                    Document.Append("<OperCompleted>N</OperCompleted>");
                }
                Document.Append("</Item>");




                //if Shift Has NonProductive Time
                foreach (DataRow r in dt.AsEnumerable().Where(n => n.Field<string>("TimeCodeTrnType") == "N"))
                {
                    DataTable check = CheckTimeCode(r["TimeCodeID"].ToString().Trim());
                    if (check.Rows.Count == 1)
                    {
                        SetupTime = SetupTime + Convert.ToDecimal(r["ElapsedTime"].ToString());
                    }
                    else
                    {

                        Document.Append("<Item>");
                        Document.Append("<Journal />");
                        Document.Append("<ItemTransactionDate>" + Date + "</ItemTransactionDate>");
                        Document.Append("<Job>" + Job + "</Job>");
                        Document.Append("<UnitOfMeasure>S</UnitOfMeasure>");
                        Document.Append("<LOperation>" + Operation + "</LOperation>");
                        Document.Append("<LWorkCentre>" + WorkCentre + "</LWorkCentre>");
                        Document.Append("<LWcRateInd>1</LWcRateInd>");
                        Document.Append("<LRunTimeHours>" + Math.Round(Convert.ToDecimal(r["ElapsedTime"].ToString()), 3) + "</LRunTimeHours>");
                        Document.Append("<NonProductiveCode>" + r["TimeCodeID"].ToString().Trim() + "</NonProductiveCode>");
                        Document.Append("<ManualWorkCenterRates>N</ManualWorkCenterRates>");
                        Document.Append("<Reference>" + Reference + "</Reference>");
                        if (AddReference != "")
                        {
                            Document.Append("<AdditionalReference>" + AddReference + "</AdditionalReference>");
                        }
                        Document.Append("</Item>");
                    }
                }
                Document.Append("</PostLabour>");

                return Document.ToString();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}