using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class LabourPostReversal
    {
        clsData data = new clsData();
        public DataTable GetPostedJobs(string Date, string WC, string Shift)
        {
            try
            {
                return data.SelectData("sp_GetReversalJobs '" + WC + "','" + Date + "','" + Shift + "'");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }
        }
        public DataTable GetSelectedJobInfo(string Guid)
        {
            try
            {
                return data.SelectData("sp_GetReversalJobsInfo '" + Guid + "'");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }
        }
        public DataTable GetScrapPosted(string Guid)
        {
            try
            {
                return data.SelectData("sp_GetReversalScrap '" + Guid + "'");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }

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
                decimal StockMass = GetStockMass(Job);
                if (StockMass == 0)
                { StockMass = 1; }

                RunTime = dt.AsEnumerable().Where(y => y.Field<string>("TimeCodeID") == "01").Sum(r => Convert.ToDecimal(r.Field<decimal>("ElapsedTime")));
                SetupTime = dt.AsEnumerable().Where(y => y.Field<string>("TimeCodeID") == "08").Sum(r => Convert.ToDecimal(r.Field<decimal>("ElapsedTime")));
                TotalScrap = ds.AsEnumerable().Sum(r => Convert.ToDecimal(r.Field<decimal>("ScrapQuantity"))) / StockMass;


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
                foreach (DataRow r in dt.AsEnumerable().Where(n => n.Field<string>("TimeCodeID") != "08").Where(n => n.Field<string>("TimeCodeID") != "01"))
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


        public decimal GetStockMass(string Job)
        {
            try
            {
                decimal mass;
                DataTable dp = new DataTable();
                dp.Columns.Add("Mass", typeof(decimal));

                dp = data.SelectData("EXEC sp_GetReversalStockMass '" + Job + "'");
                mass = Convert.ToDecimal(dp.Rows[0][0].ToString());
                return mass;
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
                return data.SelectData("SELECT * FROM tpTimeCodes WITH(NOLOCK) WHERE SysproCode = '" + SysproID + "' ");
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }

        }
        public void SaveLabour(DataTable dts, DataTable ds, string Username)
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
                    decimal StockMass = GetStockMass(Job);
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

                    data.Execute("INSERT INTO tpLabourTimes(EntryDate,Shift,WorkCentre,Job,Operation,TimeCodeID,StartTime,EndTime,ElapsedTime,ProductionMass,Operator,ProductionQtyEnt,AddReference,Guid) VALUES ('" + Date + "','" + row["Shift"].ToString().Trim() + "','" + row["WorkCentre"].ToString().Trim() + "','" + row["Job"].ToString().Trim() + "','" + Convert.ToDecimal(row["Operation"].ToString().Trim()) + "','" + row["TimeCodeID"].ToString().Trim() + "','" + StartTime + "','" + EndTime + "','" + ElapsedTime + "','" + row["ProductionQuantity"].ToString().Trim() + "','" + Username + "','" + Convert.ToDecimal(row["ProductionQtyEnt"].ToString()) + "','" + row["AddReference"].ToString() + "','" + GuidToSave + "')");
                }
                foreach (DataRow srow in ds.Rows)
                {

                    data.Execute("INSERT INTO tpScrap(Job,Date,WorkCentre,Reference,ScrapCode,Quantity,Guid) VALUES ('" + Job + "','" + Date + "','" + WorkCentre + "','" + srow["Shift"].ToString().Trim() + "','" + srow["ScrapCode"].ToString().Trim() + "','" + Math.Round(Convert.ToDecimal(srow["ScrapQuantity"].ToString().Trim()), 3) + "','" + GuidToSave + "')");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error Saving Job: " + ex.Message);
            }

        }
        //Get Shifts For ddl
        public DataTable Shifts()
        {
            try
            {
                return data.SelectData("SELECT Shift,ShiftID FROM tpShifts WITH (NOLOCK)");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}