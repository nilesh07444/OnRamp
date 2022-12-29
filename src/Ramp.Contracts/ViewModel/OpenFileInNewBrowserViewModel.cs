using Domain.Customer.Models;
using System;

namespace Ramp.Contracts.ViewModel
{
    public class OpenFileInNewBrowserViewModel : IViewModel
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
    }
}