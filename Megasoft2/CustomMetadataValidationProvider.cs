using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;
//using System.Web.Http.Validation.Providers;
using System.Web.Mvc;

namespace Megasoft2
{
    public class CustomMetadataValidationProvider : DataAnnotationsModelValidatorProvider
    {

        //This is a Custom Validation Class used to add validation of fields at runtime based on user/system/company preferences
        //Used in Customer create, to dynamicaly add required validation at runtime.
        protected override IEnumerable<System.Web.Mvc.ModelValidator> GetValidators(System.Web.Mvc.ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes)
        {
            //go to db if you want
            //var repository = ((MyBaseController) context.Controller).RepositorySomething;

            //find user if you need it
            var user = context.HttpContext.User;
            if(metadata.ContainerType != null)
            {
                if (!string.IsNullOrWhiteSpace(metadata.ContainerType.FullName) && metadata.ContainerType.FullName == "Megasoft2.Models.CustomerViewModel")
                {
                    //SysproEntities db = new SysproEntities("");
                    //if (!string.IsNullOrEmpty((from a in db.mtMasterFileRequiredFields where a.MasterFile == "CustomerCreate" && a.FieldName == metadata.PropertyName && (a.RequiredField == "Y" || a.SysproRequired == "Y") select a.DisplayName).FirstOrDefault()))                    
                    //{
                    //    attributes = new List<Attribute>() { new RequiredAttribute() };
                    //}

                    
                    
                }

                
            }

            return base.GetValidators(metadata, context, attributes);
        }
    }
}