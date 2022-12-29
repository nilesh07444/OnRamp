using Common.Data;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Forms;
using Domain.Enums;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.Command.Form
{
    public class CreateOrUpdateFormCommand : IdentityModel<string>
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastEditDate { get; set; }
        public string LastEditedBy { get; set; }
        public Guid CustomDocummentId { get; set; }
        public string CategoryId { get; set; }
        public bool Printable { get; set; }
        public int Points { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public virtual Upload CoverPicture { get; set; }
        public string CoverPictureId { get; set; }
        public bool Deleted { get; set; }
        public string TrainingLabels { get; set; }
        public bool CheckRequired { get; set; }

        public virtual ICollection<FormChapter> FormChapters { get; set; } = new List<FormChapter>();
    }
}
