using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Contracts.ViewModel
{
    public class StandardUserProgressViewModel
    {
        public int TotalGuides { get; set; }
        public int TotalTests { get; set; }
        public int ViewedGuides { get; set; }
        public int ViewedTests { get; set; }
        public int PassedTests { get; set; }
        public double OverallProgress { get; set; }
        public double GuideProgress { get; set; }
        public double TestProgress { get; set; }
    }
}
