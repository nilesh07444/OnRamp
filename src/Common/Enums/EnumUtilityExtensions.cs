using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtuaCon;

namespace Common.Enums
{
    public static class EnumUtilityExtensions
    {
        public static IDictionary<int,string> GetEnumFriendlyNamesDictionary(Type enumType)
        {
            var result = new Dictionary<int, string>();
            foreach (var value in Enum.GetValues(enumType))
            {
                try
                {
					
					var iValue = (int)(Enum.Parse(enumType, value.ToString(), true));
                    if (!result.ContainsKey(iValue) ) {
						result.Add(iValue, EnumUtility.GetFriendlyName(enumType, value));
					}
                }
                catch (Exception) { }
            }
            return result;
        }
    }
}
