using Common.Command;
using Common.Data;
using Common.Events;
using Common.Query;
using Data.EF.Events;
using Domain.Customer.Models;
using Domain.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.QueryParameter.FileUploads;
using Ramp.Contracts.QueryParameter.Settings;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Web.UI.Code.Extensions;

namespace Web.UI.Code.Handlers
{
    public class CreateTestCertificateCommandHandler : ICommandHandlerBase<CreateTestCertificateCommandParameter>, IEventHandler<CreateTestCertificatesEvent>
    {
        private readonly IRepository<StandardUser> _standarUserRepository;
        private readonly IRepository<TestResult> _testResultRepository;
        private readonly IRepository<Domain.Customer.Models.TestCertificate> _testCertificateRepository;
        private readonly IRepository<DefaultConfiguration> _defaultConfigRepository;
        private readonly IQueryExecutor _queryExecutor;
        private const string _certificate = "application/pdf";

        public CreateTestCertificateCommandHandler(
            IRepository<StandardUser> standarUserRepository,
            IRepository<TestResult> testResultRepository,
            IRepository<Domain.Customer.Models.TestCertificate> testCertificateRepository,
            IRepository<DefaultConfiguration> defaultConfigRepository,
            IQueryExecutor queryExecutor)
        {
            _standarUserRepository = standarUserRepository;
            _testResultRepository = testResultRepository;
            _testCertificateRepository = testCertificateRepository;
            _defaultConfigRepository = defaultConfigRepository;
            _queryExecutor = queryExecutor;
        }

        public CommandResponse Execute(CreateTestCertificateCommandParameter command)
        {
            if (command.Migration)
            {
                var results = _testResultRepository.List.Where(x => x.TestResultStatus).ToList();

                foreach (var r in results)
                {
                    var user = _standarUserRepository.List.SingleOrDefault(u => u.Id.Equals(r.TestTakenByUserId));
                    if (user != null)
                    {
                        if (r.TrainingGuideTitle != null)
                        {
                            CreateCertificate(user, r, PortalContext.Current);
                        }
                    }
                }
                return null;
            }
            var re = _testResultRepository.Find(command.ResultId);
            var ruser = _standarUserRepository.List.SingleOrDefault(u => u.Id.Equals(re.TestTakenByUserId));
            if (ruser != null)
            {
                if (re.TrainingGuideTitle != null)
                {
                    //if (re.TestDate.Date != DateTime.Now.Date)
                    //    re.TestDate = DateTime.Now;
                    CreateCertificate(ruser, re, PortalContext.Current,command.Id);
                }
            }
            return null;
        }

        public void Handle(CreateTestCertificatesEvent @event)
        {
            PortalContext.Override(@event.CompanyId);
            new CommandDispatcher().Dispatch(new CreateTestCertificateCommandParameter
            {
                Migration = true
            });
        }

        private void CreateCertificate(StandardUser user, TestResult r, PortalContextViewModel context, Guid? Id = null)
        {
            try
            {
                byte[] cert = null;
                var customCert = _queryExecutor.Execute<GetCustomConfigurationQueryParameter, CustomConfigurationViewModel>(new GetCustomConfigurationQueryParameter());

                if (customCert != null && customCert.Certificate != null)
                {
                    cert = customCert.Certificate.Data;
                }
                else
                {
                    var defaultC = _defaultConfigRepository.List.FirstOrDefault();
                    cert = defaultC.Certificate.Data;
                }
                //make it a bmp first
                System.Drawing.Image image = null;
                using (MemoryStream stream = new MemoryStream(cert))
                {
                    var temp = Bitmap.FromStream(stream, true, true);
                    using (var tempStream = new MemoryStream())
                    {
                        temp.Save(tempStream, ImageFormat.Bmp);
                        image = Bitmap.FromStream(tempStream, true, true);
                    }
                }
                using (var graphics = Graphics.FromImage(image))
                {

                    System.Drawing.Font font = new System.Drawing.Font("Times New Roman", 20.0f);
                    System.Drawing.Font fontlogo = new System.Drawing.Font("Times New Roman", 40.0f);
                    // Create text position
                    var name = user.FirstName.RemoveSpecialCharacters().Contains(" ") ? user.FirstName.RemoveSpecialCharacters() : $"{user.FirstName} {user.LastName}".RemoveSpecialCharacters();
                    int intWidth = (int)graphics.MeasureString(name, font).Width;
                    PointF pointUserName = new PointF(1250 - (intWidth / 2), 1150);
                    // Draw text User Name
                    graphics.DrawString(name, font, Brushes.Black, pointUserName);

                    int intWidthPlaybook = (int)graphics.MeasureString(r.TrainingGuideTitle.RemoveSpecialCharacters(), font).Width;
                    PointF pointPlaybookName = new PointF(1250 - (intWidthPlaybook / 2), 1420);
                    // Draw text
                    graphics.DrawString(r.TrainingGuideTitle.RemoveSpecialCharacters(), font, Brushes.Black, pointPlaybookName);

                    string score = string.Format("{0} %", ((int)decimal.Round(((decimal)r.TestScore / (decimal)r.Total) * 100, 2)).ToString());
                    int intWidthMarks = (int)graphics.MeasureString(score, font).Width;
                    PointF pointMarks = new PointF(1250 - (intWidthMarks / 2), 1700);
                    // Draw text total marks scored
                    graphics.DrawString(score, font, Brushes.Black, pointMarks);

                    int intWidthDate = (int)graphics.MeasureString(r.TestDate.ToString("dd-MMMM-yyyy"), font).Width;
                    PointF pointDate = new PointF(1250 - (intWidthDate / 2), 1900);

                    graphics.DrawString(r.TestDate.ToString("dd-MMMM-yyyy"), font, Brushes.Black, pointDate);
                    var document = new Document(PageSize.A4, 0, 0, 0, 0);
                    var pdfS = new MemoryStream();

                    PdfWriter.GetInstance(document, pdfS);
                    document.Open();
                    var images = iTextSharp.text.Image.GetInstance(image, ImageFormat.Bmp);
                    images.ScaleToFit(iTextSharp.text.PageSize.A4);
                    document.Add(images);
                    document.Close();
                    var certE = new TestCertificate
                    {
                        Id = Id.HasValue ? Id.Value : Guid.NewGuid(),
                        TestId = r.TrainingTestId.Value,
                        User = user,
                        Upload = new FileUploads
                        {
                            Id = Guid.NewGuid(),
                            ContentType = _certificate,
                            Name = $"{user.Id}-{r.TrainingTestId}.pdf",
                            Description = $"{user.Id}-{r.TrainingTestId}.pdf",
                            Data = pdfS.ToArray(),
                            Type = TrainingDocumentTypeEnum.Pdf.ToString()
                        },
                        DateCreated = r.TestDate,
                        Passed = r.TestResultStatus
                    };
                    user.TestCertificates.Add(certE);
                    _standarUserRepository.SaveChanges();
                    pdfS.Dispose();
                }
            }
            catch (DocumentException de)
            {
                //throw de;
                var docmsg = de.Message;
                throw de;
            }
            catch (IOException ex)
            {
                //throw ex;
                var msg = ex.Message;
                throw ex;
            }
        }
    }
}