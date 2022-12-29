using Domain.Customer;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Query.Report
{
   public class ReportParam
    {
       public static DateTime FromDate = DateTime.MinValue;
       public static DateTime ToDate = DateTime.MinValue;

        public void AssignDateTime(DateTime _FromDate, DateTime _ToDate)
        {
            FromDate = _FromDate;
            ToDate = _ToDate;
        }

        public static void ReturnDocumentType(List<Parameters> Params, out DocumentType[] documentsType)
        {
            var parmeters = Params.Where(z => z.Name == "DocumentType").ToList();
            List<int> list = new List<int>();
            foreach (var d in parmeters)
            {
                foreach (var v in d.Value.Split(','))
                {
                    list.Add(int.Parse(v));
                }
            }
            documentsType = Enum.GetValues(typeof(DocumentType)).Cast<DocumentType>().Where(z => list.Contains((int)z)).ToArray();
        }

        public static string ReturnDocumentTypeCopy(List<Parameters> Params)
        {
            var parmeters = Params.Where(z => z.Name == "DocumentType").FirstOrDefault();
            var paramSplit = parmeters.Value.Split(',');
            var NewString = "[";
            NewString += string.Join(",", paramSplit);
            NewString += "]";
            return NewString;
        }

        public static List<string> ReturnParams(List<Parameters> Params, string Key)
        {
            var parmeters = Params.Where(z => z.Name == Key).Select(z => z.Value).ToList();
            List<string> list = new List<string>();
            foreach (var d in parmeters)
            {
                {
                    foreach (var v in d.Split(','))
                    {
                        list.Add(v);
                    }
                }
            }
            return list;
        }

        public static string ReturnSingleParams(List<Parameters> Params, string Key)
        {
            var parmeters = Params.Where(z => z.Name == Key).Select(z => z.Value).FirstOrDefault();
            return parmeters;
        }

    }
}
