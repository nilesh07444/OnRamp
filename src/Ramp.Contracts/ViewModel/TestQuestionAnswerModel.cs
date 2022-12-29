using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class TestQuestionAnswerModel : IdentityModel<string>
    {
        public string Option { get; set; } = "";
        public int Number { get; set; }
        public bool Deleted { get; set; }
    }
    public class TestQuestionAnswerResultModel : TestQuestionAnswerModel
    {
        public TestQuestionAnswerStateModel State { get; set; } = new TestQuestionAnswerStateModel();
    }
    public class TestQuestionAnswerStateModel
    {
        public bool Selected { get; set; }
    }
}
