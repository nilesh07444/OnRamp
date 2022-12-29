using Common.Data;
using Domain.Customer.Models.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models
{
    public class StandardUserAdobeFieldValues : IdentityModel<string>
    {
        public string AcrobatFieldChapterId { get; set; }
        public string DocumentId { get; set; }
        public string Field_Name { get; set; }
        public string Field_Value { get; set; }
        public Guid User_ID { get; set; }
        public DateTime CreatedOn { get; set; }


    }
}
