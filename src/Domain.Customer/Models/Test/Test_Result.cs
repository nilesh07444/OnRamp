using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Customer.Models.Test
{
    public class Test_Result : IdentityModel<string>
    {
        public string UserId { get; set; }
        public string TestId { get; set; }
        public virtual Test Test { get; set; }
        public DateTimeOffset Created { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public decimal Percentage { get; set; }
        public int Score { get; set; }
        public int Total { get; set; }
        public bool Deleted { get; set; }
        public bool Passed { get; set; }
        public bool IsGloballyAccessed { get; set; }
        public virtual ICollection<TestQuestion_Result> Questions { get; set; } = new List<TestQuestion_Result>();
        public string CertificateId { get; set; }
        public string CertificateThumbnailId { get; set; }
        public virtual Upload Certificate { get; set; }

        //added by softude
        public string Title { get; set; }

    }
    public class TestQuestion_Result : IdentityModel<string>
    {
        public string QuestionId { get; set; }
        public virtual TestQuestion Question { get; set; }
        public bool Deleted { get; set; }
        public bool UnAnswered { get; set; }
        public bool ViewLater { get; set; }
        public bool Correct { get; set; }
        public virtual ICollection<TestQuestionAnswer_Result> Answers { get; set; } = new List<TestQuestionAnswer_Result>();
    }
    public class TestQuestionAnswer_Result : IdentityModel<string>
    {
        public string AnswerId { get; set; }
        public virtual TestQuestionAnswer Answer { get; set; }
        public bool Selected { get; set; }
        public bool Deleted { get; set; }
    }
}
