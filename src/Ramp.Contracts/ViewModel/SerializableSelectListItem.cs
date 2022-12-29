using System;

namespace Ramp.Contracts.ViewModel
{
    [Serializable]
    public class SerializableSelectListItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}