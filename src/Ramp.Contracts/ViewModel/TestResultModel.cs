using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TestResultModel  : TestModel
    {
        public new IEnumerable<TestQuestionResultModel> ContentModels { get; set; }
        public decimal Percentage { get; set; }
        public int Score { get; set; }
        public int Total { get; set; }
        public bool Passed { get; set; }
        public int CustomDocumentOrder { get; set; } = 0;
        public int NumberOfRightAnswers { get; set; }
        public int NumberOfWrongAnswers { get; set; }
        public int NumberOfUnattemptedQuestions { get; set; }
    }
}
