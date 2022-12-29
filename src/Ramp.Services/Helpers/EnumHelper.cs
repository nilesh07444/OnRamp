using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;

namespace Ramp.Services.Helpers
{
    public static class EnumHelper
    {
        public static string GetDescription(Enum value)
        {
            if (value == null)
            {
                return null;
            }

            string description = value.ToString();
            FieldInfo fieldInfo = value.GetType().GetField(description);
            var attributes = (RampEnumDescriptionAttribute[])
                fieldInfo.GetCustomAttributes(typeof(RampEnumDescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                description = attributes[0].Description;
            }

            return description;
        }

        public static string GetDescriptionString(this System.Enum enumObject)
        {
            if (enumObject == null)
            {
                throw new ArgumentNullException("enumObject");
            }

            var da = (DescriptionAttribute[])enumObject
                                                  .GetType()
                                                  .GetField(enumObject.ToString())
                                                  .GetCustomAttributes(typeof(DescriptionAttribute), true);

            string description = null;
            if (da.Length > 0)
            {
                description = da[0].Description;
            }

            if (string.IsNullOrEmpty(description))
            {
                description = enumObject.ToString().ToUpper();
            }

            return description;
        }

        public static string GetUserRoleName(Enum value)
        {
            if (value == null)
            {
                return null;
            }
            string description = value.ToString();
            if (description == "Admin")
            {
                description = "Admin";
            }
            else if (description == "Reseller")
            {
                description = "Reseller";
            }
            else if (description == "CustomerAdmin")
            {
                description = "Customer Admin User";
            }
            else if (description == "CustomerStandardUser")
            {
                description = "Customer Standard User";
            }
            return description;
        }

        public static string GetUserRoleNameForString(string value)
        {
            if (value == null)
            {
                return null;
            }
            string description = value;
            if (description == "Admin")
            {
                description = "Admin";
            }
            else if (description == "Reseller")
            {
                description = "Reseller";
            }
            else if (description == "CustomerAdmin")
            {
                description = "Customer Admin User";
            }
            else if (description == "CustomerStandardUser")
            {
                description = "Customer Standard User";
            }
            return description;
        }

        public static IList<SerializableSelectListItem> ToList(Type type)
        {
            if (type == null)
            {
                return null;
            }

            Array enumValues = Enum.GetValues(type);

            return (from Enum value in enumValues
                    select new SerializableSelectListItem
                    {
                        Text = GetDescription(value),
                        Value = value.ToString()
                    }).ToList();
        }
    }
}