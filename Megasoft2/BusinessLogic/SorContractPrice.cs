using Megasoft2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public class SorContractPrice
    {
        public string BuildExpireDocument(List<sp_GetSalesContractPricingForExpiry_Result> detail)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("");
            Document.Append("This is an example XML instance to demonstrate");
            Document.Append("use of the SO Contract Pricing Maintenance business object");
            Document.Append("-->");
            Document.Append("<SetupSoContractPricing xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORSCPDOC.xsd\">");

            foreach(var item in detail)
            {
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<ContractType>" + item.ContractType + "</ContractType>");
                Document.Append("<CustomerBuygrp>" + item.CustomerBuyGrp + "</CustomerBuygrp>");
                Document.Append("<StockCode>" + item.StockCode + "</StockCode>");
                Document.Append("<Contract>" + item.Contract + "</Contract>");
                Document.Append("</Key>");
                //Document.Append("<StartDate>2009-02-27</StartDate>");
                Document.Append("<ExpiryDate>" + Convert.ToDateTime(item.ExpiryDate).ToString("yyyy-MM-dd") + "</ExpiryDate>");
                Document.Append("<PriceMethod>F</PriceMethod>");
                Document.Append("<FixedUom>" + item.FixedUom + "</FixedUom>");
                //Document.Append("<FixedPriceCode />");
                //Document.Append("<FixedPrice>8000.00</FixedPrice>");
                //Document.Append("<Discount1 />");
                //Document.Append("<Discount2 />");
                //Document.Append("<Discount3 />");
                //Document.Append("<QtyBreak1 />");
                //Document.Append("<QtyBreak2 />");
                //Document.Append("<QtyBreak3 />");
                //Document.Append("<QtyBreak4 />");
                //Document.Append("<QtyBreak5 />");
                //Document.Append("<QtyPrice1 />");
                //Document.Append("<QtyPrice2 />");
                //Document.Append("<QtyPrice3 />");
                //Document.Append("<QtyPrice4 />");
                //Document.Append("<QtyPrice5 />");
                //Document.Append("<CustListPrice />");
                //Document.Append("<DicountPct />");
                //Document.Append("<eSignature>");
                //Document.Append("</eSignature>");
                Document.Append("</Item>");
            }

            
            Document.Append("</SetupSoContractPricing>");

            return Document.ToString();

        }

        

        public string BuildContractPriceDocument(List<sp_GetSalesContractPricingForExpiry_Result> detail)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("");
            Document.Append("This is an example XML instance to demonstrate");
            Document.Append("use of the SO Contract Pricing Maintenance business object");
            Document.Append("-->");
            Document.Append("<SetupSoContractPricing xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORSCPDOC.xsd\">");

            foreach (var item in detail)
            {
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<ContractType>" + item.ContractType + "</ContractType>");
                Document.Append("<CustomerBuygrp>" + item.CustomerBuyGrp + "</CustomerBuygrp>");
                Document.Append("<StockCode>" + item.StockCode + "</StockCode>");
                Document.Append("<Contract>" + item.Contract + "</Contract>");
                Document.Append("</Key>");
                //Document.Append("<StartDate>2009-02-27</StartDate>");
                if (!string.IsNullOrEmpty(item.StartDate))
                {
                    Document.Append("<StartDate>" + Convert.ToDateTime(item.StartDate).ToString("yyyy-MM-dd") + "</StartDate>");
                }
                if (!string.IsNullOrEmpty(item.ExpiryDate))
                {
                    Document.Append("<ExpiryDate>" + Convert.ToDateTime(item.ExpiryDate).ToString("yyyy-MM-dd") + "</ExpiryDate>");
                }
                
                Document.Append("<PriceMethod>F</PriceMethod>");
                Document.Append("<FixedUom>" + item.FixedUom + "</FixedUom>");
                //Document.Append("<FixedPriceCode />");
                Document.Append("<FixedPrice>" + item.NewPrice + "</FixedPrice>");
                //Document.Append("<Discount1 />");
                //Document.Append("<Discount2 />");
                //Document.Append("<Discount3 />");
                //Document.Append("<QtyBreak1 />");
                //Document.Append("<QtyBreak2 />");
                //Document.Append("<QtyBreak3 />");
                //Document.Append("<QtyBreak4 />");
                //Document.Append("<QtyBreak5 />");
                //Document.Append("<QtyPrice1 />");
                //Document.Append("<QtyPrice2 />");
                //Document.Append("<QtyPrice3 />");
                //Document.Append("<QtyPrice4 />");
                //Document.Append("<QtyPrice5 />");
                //Document.Append("<CustListPrice />");
                //Document.Append("<DicountPct />");
                //Document.Append("<eSignature>");
                //Document.Append("</eSignature>");
                Document.Append("</Item>");
            }


            Document.Append("</SetupSoContractPricing>");

            return Document.ToString();

        }

        public string BuildNewContractPriceDocument(SorContractPriceViewModel model)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();
            
            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("");
            Document.Append("This is an example XML instance to demonstrate");
            Document.Append("use of the SO Contract Pricing Maintenance business object");
            Document.Append("-->");
            Document.Append("<SetupSoContractPricing xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORSCPDOC.xsd\">");

            if(model != null)
            {
                Document.Append("<Item>");
                Document.Append("<Key>");
                Document.Append("<ContractType>C</ContractType>");
                Document.Append("<CustomerBuygrp>" + model.Customer + "</CustomerBuygrp>");
                Document.Append("<StockCode>" + model.StockCode + "</StockCode>");
                Document.Append("<Contract>" + model.Contract + "</Contract>");
                Document.Append("</Key>");

                if (!string.IsNullOrEmpty(model.StartDate))
                {
                    Document.Append("<StartDate>" + Convert.ToDateTime(model.StartDate).ToString("yyyy-MM-dd") + "</StartDate>");
                }
                if (!string.IsNullOrEmpty(model.ExpiryDate))
                {
                    Document.Append("<ExpiryDate>" + Convert.ToDateTime(model.ExpiryDate).ToString("yyyy-MM-dd") + "</ExpiryDate>");
                }

                Document.Append("<PriceMethod>F</PriceMethod>");
                Document.Append("<FixedUom>" + model.FixedUom + "</FixedUom>");
                //Document.Append("<FixedPriceCode />");
                Document.Append("<FixedPrice>" + model.Price + "</FixedPrice>");
                //Document.Append("<Discount1 />");
                //Document.Append("<Discount2 />");
                //Document.Append("<Discount3 />");
                //Document.Append("<QtyBreak1 />");
                //Document.Append("<QtyBreak2 />");
                //Document.Append("<QtyBreak3 />");
                //Document.Append("<QtyBreak4 />");
                //Document.Append("<QtyBreak5 />");
                //Document.Append("<QtyPrice1 />");
                //Document.Append("<QtyPrice2 />");
                //Document.Append("<QtyPrice3 />");
                //Document.Append("<QtyPrice4 />");
                //Document.Append("<QtyPrice5 />");
                //Document.Append("<CustListPrice />");
                //Document.Append("<DicountPct />");
                //Document.Append("<eSignature>");
                //Document.Append("</eSignature>");
                Document.Append("</Item>");
            }


            Document.Append("</SetupSoContractPricing>");

            return Document.ToString();

        }
        public string BuildContractPriceParameter()
        {
            //Declaration
            StringBuilder Parameter = new StringBuilder();

            //Building Parameter content
            Parameter.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Parameter.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
            Parameter.Append("<!--");
            Parameter.Append("");
            Parameter.Append("This is an example XML instance to demonstrate");
            Parameter.Append("use of the parameter file for the SO Contract Pricing Maintenance business object");
            Parameter.Append("-->");
            Parameter.Append("<SetupSoContractPricing xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORSCP.xsd\">");
            Parameter.Append("<Parameters>");
            Parameter.Append("<IgnoreWarnings>N</IgnoreWarnings>");
            Parameter.Append("<ApplyIfEntireDocumentValid>Y</ApplyIfEntireDocumentValid>");
            Parameter.Append("<ValidateOnly>N</ValidateOnly>");
            Parameter.Append("</Parameters>");
            Parameter.Append("</SetupSoContractPricing>");

            return Parameter.ToString();
        }



        public string BuildContractPriceXrefDocument(List<sp_GetStockCodesByCustomer_Result> detail, string Contract, string StartDate, string ExpiryDate, string ContractType)
        {
            //Declaration
            StringBuilder Document = new StringBuilder();

            //Building Document content
            Document.Append("<?xml version=\"1.0\" encoding=\"Windows-1252\"?>");
            Document.Append("<!-- Copyright 1994-2011 SYSPRO Ltd.-->");
            Document.Append("<!--");
            Document.Append("");
            Document.Append("This is an example XML instance to demonstrate");
            Document.Append("use of the SO Contract Pricing Maintenance business object");
            Document.Append("-->");
            Document.Append("<SetupSoContractPricing xmlns:xsd=\"http://www.w3.org/2001/XMLSchema-instance\" xsd:noNamespaceSchemaLocation=\"SORSCPDOC.xsd\">");

            foreach (var item in detail)
            {

                if(item.Price != 0)
                {
                    Document.Append("<Item>");
                    Document.Append("<Key>");
                    Document.Append("<ContractType>" + ContractType + "</ContractType>");
                    Document.Append("<CustomerBuygrp>" + item.Customer + "</CustomerBuygrp>");
                    Document.Append("<StockCode>" + item.StockCode + "</StockCode>");
                    Document.Append("<Contract>" + Contract + "</Contract>");
                    Document.Append("</Key>");
                    //Document.Append("<StartDate>2009-02-27</StartDate>");
                    if (!string.IsNullOrEmpty(StartDate))
                    {
                        Document.Append("<StartDate>" + Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd") + "</StartDate>");
                    }
                    if (!string.IsNullOrEmpty(ExpiryDate))
                    {
                        Document.Append("<ExpiryDate>" + Convert.ToDateTime(ExpiryDate).ToString("yyyy-MM-dd") + "</ExpiryDate>");
                    }

                    Document.Append("<PriceMethod>F</PriceMethod>");
                    Document.Append("<FixedUom>" + item.StockUom + "</FixedUom>");
                    //Document.Append("<FixedPriceCode />");
                    Document.Append("<FixedPrice>" + item.Price + "</FixedPrice>");
                    //Document.Append("<Discount1 />");
                    //Document.Append("<Discount2 />");
                    //Document.Append("<Discount3 />");
                    //Document.Append("<QtyBreak1 />");
                    //Document.Append("<QtyBreak2 />");
                    //Document.Append("<QtyBreak3 />");
                    //Document.Append("<QtyBreak4 />");
                    //Document.Append("<QtyBreak5 />");
                    //Document.Append("<QtyPrice1 />");
                    //Document.Append("<QtyPrice2 />");
                    //Document.Append("<QtyPrice3 />");
                    //Document.Append("<QtyPrice4 />");
                    //Document.Append("<QtyPrice5 />");
                    //Document.Append("<CustListPrice />");
                    //Document.Append("<DicountPct />");
                    //Document.Append("<eSignature>");
                    //Document.Append("</eSignature>");
                    Document.Append("</Item>");
                }
                
            }


            Document.Append("</SetupSoContractPricing>");

            return Document.ToString();

        }
    }
}