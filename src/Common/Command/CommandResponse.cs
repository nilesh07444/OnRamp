using System.Collections.Generic;

namespace Common.Command
{
    public class CommandResponse
    {
        public string ErrorMessage { get; set; }
		public string Id { get; set; }
		public IEnumerable<IValidationResult> Validation { get; set; } = new List<IValidationResult>();
    }
}