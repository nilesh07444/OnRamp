using System.ComponentModel;

namespace Ramp.Services.Helpers
{
    public class RampEnumDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _description;

        public RampEnumDescriptionAttribute(string description)
        {
            _description = description;
        }

        public override string Description
        {
            get { return _description; }
        }
    }
}