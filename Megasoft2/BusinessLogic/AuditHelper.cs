using Megasoft2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Megasoft2.BusinessLogic
{
    public static class AuditHelper
    {
        public static List<EntityChanges> EnumeratePropertyDifferences<T>(this T oldModel, T newModel)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            List<EntityChanges> EC = new List<EntityChanges>();
            foreach (PropertyInfo pi in properties)
            {
                object oldValue = typeof(T).GetProperty(pi.Name).GetValue(oldModel, null);
                object newValue = typeof(T).GetProperty(pi.Name).GetValue(newModel, null);
                if(pi.PropertyType.FullName.ToString() != oldModel.GetType().ToString())
                {
                    if (pi.Name != "mtRequisitionDetail1" && pi.Name != "mtRequisitionDetail2")
                    {
                        if (oldValue != newValue && (oldValue == null || !oldValue.Equals(newValue)))
                        {
                            if (pi.PropertyType.FullName.ToString() == "System.DateTime")
                            {
                                //Do nothing
                                if(oldValue.ToString() != newValue.ToString())
                                {
                                    EC.Add(new EntityChanges { KeyField = pi.Name, OldValue = oldValue.ToString(), NewValue = newValue.ToString() });
                                }
                            }
                            else
                            {
                                if (oldValue == null)
                                    oldValue = "";
                                if (newValue == null)
                                    newValue = "";
                                if (pi.Name != "TrnDate")
                                {
                                    EC.Add(new EntityChanges { KeyField = pi.Name, OldValue = oldValue.ToString(), NewValue = newValue.ToString() });
                                }
                            }
                            

                        }
                    }
                    
                }
                
            }
            return EC;
        }
    }
}