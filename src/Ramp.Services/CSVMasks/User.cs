using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CSVMasks
{
    public class User
    {
        [CsvColumn(Name ="FullName",FieldIndex =1)]
        public string FullName { get; set; }
        
        [CsvColumn(Name ="IDNumber",FieldIndex =2)]
        public string IDNumber { get; set; }

        [CsvColumn(Name ="EmailAddress",FieldIndex =3)]
        public string EmailAddress { get; set; }

        [CsvColumn(Name ="Password",FieldIndex =4)]
        public string Password { get; set; }

        [CsvColumn(Name = "MobileNumber",FieldIndex =5)]
        public string MobileNumber { get; set; }

        [CsvColumn(Name ="Group",FieldIndex =6)]
        public string Group { get; set;}

        [CsvColumn(Name ="EmployeeNo",FieldIndex = 7)]
        public string EmployeeNo { get; set; }

        [CsvColumn(Name ="Gender",FieldIndex = 8)]
        public string Gender { get; set; }

        [CsvColumn(Name ="RaceCode",FieldIndex = 9)]
        public string RaceCode { get; set; }
    }
}
