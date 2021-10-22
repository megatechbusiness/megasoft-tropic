using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Megasoft2.BusinessLogic
{
    public class SysproCore
    {
        SYSPROWCFServicesClientLibrary40.SYSPROWCFServicesPrimitiveClient SysproWcf = new SYSPROWCFServicesClientLibrary40.SYSPROWCFServicesPrimitiveClient(System.Web.Configuration.WebConfigurationManager.AppSettings["WcfBaseAddress"].ToString(), SYSPROWCFServicesClientLibrary40.SYSPROWCFBinding.NetTcp);

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

        public string SysproLogin(string Username = null, string DbName = null)
        {
            try
            {
                string Guid;
                if (Username == null)
                {
                    Username = HttpContext.Current.User.Identity.Name.ToUpper();
                }
                var LoginCred = (from a in db.mtUsers where a.Username.Equals(Username) select new { SysproUsername = a.SysproUsername, SysproPassword = a.SysproPassword ?? "" }).FirstOrDefault();
                if (string.IsNullOrEmpty(LoginCred.SysproUsername))
                {
                    throw new Exception("Syspro Username not found in User Setup Table.");
                }

                string DatabaseName;
                if (DbName != null)
                {
                    DatabaseName = DbName;
                }
                else
                {
                    DatabaseName = HttpContext.Current.Request.Cookies["SysproDatabase"].Value;
                }


                var CompanyCred = (from a in db.mtSysproAdmins where a.DatabaseName.Equals(DatabaseName) select new { Company = a.Company, CompanyPassword = a.CompanyPassword ?? "" }).FirstOrDefault();

                Guid = SysproWcf.Logon(LoginCred.SysproUsername, LoginCred.SysproPassword, CompanyCred.Company, CompanyCred.CompanyPassword);
                //Guid = SysproWcf.Logon(LoginCred.SysproUsername, LoginCred.SysproPassword, CompanyCred.Company, CompanyCred.CompanyPassword, "5", "1", "1", "");
                //Guid = SysproWcf.Logon(LoginCred.SysproUsername, LoginCred.SysproPassword, "0", "TEST");
                if (Guid != "")
                {
                    return Guid;
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


        public string SysproSetupDelete(string Guid, string Parameter, string Document, string BusinessObject)
        {
            try
            {
                return SysproWcf.SetupDelete(Guid, BusinessObject, Parameter, Document);
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
    }
}