using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Customer.Models.Document;

namespace Domain.Customer.Models.Test
{
    public class TestQuestion : IdentityModel<string>, IContentUploads
    {
        public Test Test { get; set; }
        public string TestId { get; set; }
        public int Number { get; set; }
        public bool Deleted { get; set; }
        public int AnswerWeightage { get; set; }
        public string Question { get; set; }
        public string CorrectAnswerId { get; set; }
        public bool CheckRequired { get; set; }
        public bool AttachmentRequired { get; set; }
        public bool NoteAllow { get; set; }
        public bool dynamicFields { get; set; }
        public bool IsSignOff { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;

        public virtual ICollection<TestQuestionAnswer> Answers { get; set; } = new List<TestQuestionAnswer>();
        public virtual ICollection<Upload> ContentToolsUploads { get; set; } = new List<Upload>();
        public virtual ICollection<Upload> Uploads { get; set; } = new List<Upload>();

        //added by softude
        public string Title { get; set; }

    }
}
