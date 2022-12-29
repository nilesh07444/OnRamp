using Common.Command;
using Ramp.Contracts.ViewModel;
using System;

namespace Ramp.Contracts.CommandParameter.TestManagement
{
    public class SaveTestResultCommand : ICommand
    {
        public TestResultViewModel TestResultViewModel { get; set; }
        public Guid TestTakenByUserId { get; set; }
        public Guid Id { get; set; }
        public TestViewModel TestViewModel { get; set; }
        public PortalContextViewModel PortalContext { get; set; }
        public string BasePreviewPath { get; set; }
    }
}