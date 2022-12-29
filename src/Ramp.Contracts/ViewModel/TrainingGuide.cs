using Domain.Customer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TrainingGuideModel
    {

    }
    public class TrainingGuideListModel
    {
        public Guid? Id { get; set; }
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? LastEditDate { get; set; }
        public bool? Assignable { get; set; }
        public bool? HasTest { get; set; }
        public IList<UserModelShort> Colaborators { get; set; }
        public bool? Published { get; set; }
        public IEnumerable<Guid> TestIds { get; set; }
    }
}
