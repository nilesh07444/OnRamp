using Common.Command;
using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.QueryParameter.FileUploads;
using Ramp.Contracts.QueryParameter.TestManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.TestManagement
{
    public class TestCertificateForResultQueryHandler : IQueryHandler<TestCertificateForResultQueryParameter, FileUploadResultViewModel>
    {
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<TestCertificate> _testCertificateRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private const string _certificate = "certificate";

        public TestCertificateForResultQueryHandler(
            IRepository<TestResult> testResultRepository,
            IRepository<TestCertificate> testCertificateRepository,
            IRepository<StandardUser> userRepository)
        {
            _testResultRepository = testResultRepository;
            _testCertificateRepository = testCertificateRepository;
            _userRepository = userRepository;
        }

        public FileUploadResultViewModel ExecuteQuery(TestCertificateForResultQueryParameter query)
        {
            var user = _userRepository.Find(query.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            var result = _testResultRepository.Find(query.ResultId);
            if (result != null && result.TestResultStatus)
            {
                TestCertificate cert = null;
                cert = _testCertificateRepository.List.SingleOrDefault(c => c.TestId.Equals(result.TrainingTestId) && c.User.Id.Equals(result.TestTakenByUserId) && c.Passed);
                if (cert == null)
                {
                    var id = Guid.NewGuid();
                    new CommandDispatcher().Dispatch(new CreateTestCertificateCommandParameter
                    {
                        PortalContext = query.PortalContext,
                        ResultId = result.Id,
                        UserId = query.UserId,
                        Id = id
                    });
                    cert = _testCertificateRepository.Find(id);
                }
                if (cert != null && cert.Upload != null)
                {
                    return new FileUploadResultViewModel
                    {
                        Id = cert.Upload.Id,
                        Name = cert.Upload.Name,
                        Description = cert.Upload.Description,
                        Size = cert.Upload.Data.Length,
                        PreviewPath = query.BasePreviewPath?.Replace($"{Guid.Empty}", $"{cert.Upload.Id}")
                    };
                }
            }
            return null;
        }
    }
}