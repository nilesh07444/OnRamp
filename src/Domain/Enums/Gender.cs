using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon;
namespace Domain.Enums
{
    public class GenderEnum
    {
        public const string NotSpecified = "Not Specified",
        Male = "Male",
        Female = "Female";

        public enum Gender
        {
            [EnumFriendlyName(GenderEnum.NotSpecified)]
            NotSpecified,
            [EnumFriendlyName(GenderEnum.Male)]
            Male,
            [EnumFriendlyName(GenderEnum.Female)]
            Female
        }

        public static string GetDescription(Gender? g)
        {
            if (!g.HasValue)
                return NotSpecified;
            else
            {
                switch (g)
                {
                    case Gender.Female: return Female;
                    case Gender.Male: return Male;
                    case Gender.NotSpecified: return NotSpecified;
                    default: return NotSpecified;
                }
            }
        }

        public static Gender GetGenderFromString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Gender.NotSpecified;
            if (s.Equals(Gender.Female.ToString()) || s.Equals(Female))
                return Gender.Female;
            else if (s.Equals(Gender.Male.ToString()) || s.Equals(Male))
                return Gender.Male;
            else if (s.Equals(Gender.NotSpecified.ToString()) || s.Equals(NotSpecified))
                return Gender.NotSpecified;
            else
                return Gender.NotSpecified;
        }
    }
}