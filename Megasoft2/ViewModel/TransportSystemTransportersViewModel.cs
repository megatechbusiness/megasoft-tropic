using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Megasoft2.ViewModel
{
    public class TransportSystemTransportersViewModel
    {

        public mtTransporter Transport { get; set; }

        public mtTransporterRate Rates { get; set; }

        public mtTransporterRateCode RateCodes { get; set; }

        //public List<sp_GetTransSuppliers_Result> Suppliers { get; set; }

        public List<sp_GetTransGlCodes_Result> GlCodes { get; set; }

        //public List<sp_GetTransporters_Result> Transporters { get; set; }

        public bool Taxable { get; set; }

    }
}