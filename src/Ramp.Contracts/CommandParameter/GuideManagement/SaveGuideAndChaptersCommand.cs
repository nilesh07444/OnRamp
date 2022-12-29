using Common.Command;
using Ramp.Contracts;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;

namespace Ramp.Contracts.CommandParameter.GuideManagement
{
    public class SaveGuideAndChaptersCommand : ICommand
    {
        public Guid CreatedBy { get; set; }
        public CompanyViewModel CompanyViewModel { get; set; }
        public string PathToSaveUploadedFiles { get; set; }
    }
}