using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace MegasoftService
{
    class SysproCore
    {
        SYSPROWCFServicesClientLibrary40.SYSPROWCFServicesPrimitiveClient SysproWcf = new SYSPROWCFServicesClientLibrary40.SYSPROWCFServicesPrimitiveClient(Properties.Settings.Default.WcfAddress, SYSPROWCFServicesClientLibrary40.SYSPROWCFBinding.NetTcp);

        MegasoftEntities db = new MegasoftEntities();
        public string GetXmlErrors(string XmlOut)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(XmlOut.ToString());

                XmlNodeList ErrorIdTags = doc.GetElementsByTagName("ErrorDescription");
                if (ErrorIdTags.Count > 0)
                {
                    string ErrorMessage = "";
                    foreach (XmlNode Error in ErrorIdTags)
                    {
                        if (Error.InnerText.Trim() != "")
                            ErrorMessage += Error.InnerText + ";";
                    }
                    return ErrorMessage;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string GetXmlValue(string XmlOut, string TagName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(XmlOut.ToString());

                XmlNodeList ErrorIdTags = doc.GetElementsByTagName(TagName);
                if (ErrorIdTags.Count > 0)
                {
                    string ErrorMessage = "";
                    foreach (XmlNode Error in ErrorIdTags)
                    {
                        if (Error.InnerText.Trim() != "")
                            ErrorMessage += Error.InnerText + ";";
                    }
                    return ErrorMessage;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetGlJournal(string XmlOut)
        {
            try
            {
                var xDoc = XDocument.Parse(XmlOut);
                //Get All Operators - First Level Elements
                var DetailList = (from a in xDoc.Descendants("postapinvoice").Descendants("StatusOfItems").Descendants("GlJournal")
                                  select new GlJournal
                                  {
                                      GlJournalNo = a.Element("GlJournal").Value
                                  }).ToList();

                string Journal = "";
                foreach(var item in DetailList)
                {
                    Journal += item.GlJournalNo + " ";
                }
                return Journal;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public class GlJournal
        {
            public string GlJournalNo { get; set; }
        }


        public string GetFirstXmlValue(string XmlOut, string TagName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(XmlOut.ToString());

                XmlNodeList ErrorIdTags = doc.GetElementsByTagName(TagName);
                if (ErrorIdTags.Count > 0)
                {
                    string ErrorMessage = "";
                    foreach (XmlNode Error in ErrorIdTags)
                    {
                        if (Error.InnerText.Trim() != "")
                            return Error.InnerText;
                    }
                    return ErrorMessage;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public string SysproLogin(string Username)
        {
            try
            {
                string Guid;
                //var LoginCred = (from a in db.mtUsers where a.Username.Equals(Username.ToUpper()) select new { SysproUsername = a.SysproUsername, SysproPassword = a.SysproPassword ?? "" }).FirstOrDefault();
                //if (string.IsNullOrEmpty(LoginCred.SysproUsername))
                //{
                //    throw new Exception("Syspro Username not found in User Setup Table.");
                //}

                //string DatabaseName = HttpContext.Current.Request.Cookies["SysproDatabase"].Value;

                //var CompanyCred = (from a in db.mtSysproAdmins where a.DatabaseName.Equals(DatabaseName) select new { Company = a.Company, CompanyPassword = a.CompanyPassword ?? "" }).FirstOrDefault();

                Guid = SysproWcf.Logon(Properties.Settings.Default.SysproUser, Properties.Settings.Default.SysproPassword, Properties.Settings.Default.CompanyId, Properties.Settings.Default.CompanyPassword);
                if (Guid != "")
                {
                    return Guid;
                }
                else
                {
                    throw new Exception("Login Error : Failed to login to Syspro as User : " + Properties.Settings.Default.SysproUser);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Login Error :  " + ex.Message);
            }
        }

        public void SysproLogoff(string Guid)
        {
            try
            {
                SysproWcf.Logoff(Guid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string SysproQuery(string Guid, string Document, string BusinessObject)
        {
            try
            {
                return SysproWcf.QueryQuery(Guid, BusinessObject, Document);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string SysproSetupUpdate(string Guid, string Parameter, string Document, string BusinessObject)
        {
            try
            {
                return SysproWcf.SetupUpdate(Guid, BusinessObject, Parameter, Document);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string SysproSetupAdd(string Guid, string Parameter, string Document, string BusinessObject)
        {
            try
            {
                return SysproWcf.SetupAdd(Guid, BusinessObject, Parameter, Document);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string SysproPost(string Guid, string Parameter, string Document, string BusinessObject)
        {
            try
            {
                return SysproWcf.TransactionPost(Guid, BusinessObject, Parameter, Document);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string SysproBuild(string Guid, string Document, string BusinessObject)
        {
            try
            {
                return SysproWcf.TransactionBuild(Guid, BusinessObject, Document);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
