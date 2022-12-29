using Common;
using Domain.Customer.Models;
using Domain.Customer.Models.Feedback;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.QueryParameter;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static UserViewModel UserViewModelFrom(StandardUser domainModel)
        {
            if (domainModel != null)
            {
                var model = new UserViewModel()
                {
                    ParentUserId = domainModel.ParentUserId,
                    CompanyId = domainModel.CompanyId,
                    Id = domainModel.Id,
                    EmailAddress = domainModel.EmailAddress,
                    EmployeeNo = domainModel.EmployeeNo,
                    FirstName = domainModel.FirstName,
                    LastName = domainModel.LastName,
                    Password = domainModel.Password,
                    ContactNumber = domainModel.ContactNumber,
                    MobileNumber = domainModel.MobileNumber,
                    CreatedOn = domainModel.CreatedOn,
                    IsUserExpire = domainModel.IsUserExpire,
                    ExpireDays = domainModel.ExpireDays,
                    IsFromSelfSignUp = domainModel.IsFromSelfSignUp,
                    IsActive = domainModel.IsActive,
                    IsConfirmEmail = domainModel.IsConfirmEmail,
                    Status = domainModel.IsActive,
                    IDNumber = domainModel.IDNumber,
                    RaceCodeId = domainModel.RaceCodeId,
                    Department = domainModel.Department,
                    Gender = GenderEnum.GetDescription(domainModel.Gender)
                };
                if (domainModel.FirstName.Equals(domainModel.LastName))
                {
                    model.FullName = domainModel.FirstName;
                }
                else
                {
                    model.FullName = $"{domainModel.FirstName} {domainModel.LastName}";
                }
                foreach (var customerRole in domainModel.Roles)
                {
                    model.CustomerRoles.Add(customerRole.RoleName);
                }
                if (domainModel.Group != null)
                {
                    model.GroupList.Add(new GroupViewModelShort()
                    {
                        Id = domainModel.Group.Id,
                        Name = domainModel.Group.Title
                    });
                    model.SelectedGroupId = domainModel.Group.Id;
                }
                model.UserDisclaimerActivity = domainModel.DisclaimerActivityLog.AsQueryable().Select(Project.ToUserDisclaimerActivityLogEntryModelShort).ToList();
                return model;
            }
            return null;
        }
        public static UserViewModel UserViewModelFrom(Domain.Models.User domainModel)
        {
            var model = new UserViewModel()
            {
                Id = domainModel.Id,
                ParentUserId = domainModel.ParentUserId,
                FirstName = domainModel.FirstName,
                ContactNumber = domainModel.ContactNumber,
                CompanyId = domainModel.CompanyId,
                LastName = domainModel.LastName,
                FullName = $"{domainModel.FirstName} {domainModel.LastName}",
                EmailAddress = domainModel.EmailAddress,
                Password = domainModel.Password,
                MobileNumber = domainModel.MobileNumber,
                CreatedOn = domainModel.CreatedOn,
                IsUserExpire = domainModel.IsUserExpire,
                ExpireDays = domainModel.ExpireDays,
                IsFromSelfSignUp = domainModel.IsFromSelfSignUp,
                EmployeeNo = domainModel.EmployeeNo,
                IsActive = domainModel.IsActive,
                IsConfirmEmail = domainModel.IsConfirmEmail,
                Status = domainModel.IsActive,
                Department = domainModel.Department,
                IDNumber = domainModel.IDNumber
            };

            foreach (var role in domainModel.Roles)
            {
                model.Roles.Add(new RoleViewModel()
                {
                    RoleName = role.RoleName,
                    Description = role.Description,
                    RoleId = role.Id
                });
                if (role.RoleName.Equals(Helpers.EnumHelper.GetDescription(UserRole.Admin)))
                    model.UserRoles.Add(UserRole.Admin);
                else if (role.RoleName.Equals(Helpers.EnumHelper.GetDescription(UserRole.Reseller)))
                    model.UserRoles.Add(UserRole.Reseller);
                else if (role.RoleName.Equals(Helpers.EnumHelper.GetDescription(UserRole.CustomerAdmin)))
                    model.UserRoles.Add(UserRole.CustomerAdmin);
                else if (role.RoleName.Equals(Helpers.EnumHelper.GetDescription(UserRole.CustomerStandardUser)))
                    model.UserRoles.Add(UserRole.CustomerStandardUser);
            }
            if (domainModel.Group != null)
                model.GroupList.Add(new GroupViewModelShort()
                {
                    Id = domainModel.Group.Id,
                    Name = domainModel.Group.Title
                });

            return model;
        }
        public static FeedbackViewModel FeedbackViewModelFrom(Feedback domainModel)
        {
            return new FeedbackViewModel()
            {
                FeedbackDate = domainModel.FeedbackDate,
                Message = domainModel.Message,
                Option = domainModel.Option,
                Subject = domainModel.Subject,
                Type = domainModel.Type,
                UserId = domainModel.UserId,
                Reads = domainModel.Reads.ToList().Select(s => FeedbackReadViewModelFrom(s)).ToList(),
                Id = domainModel.Id
            };
        }

        public static FeedbackReadViewModel FeedbackReadViewModelFrom(FeedbackRead domainModel)
        {
            return new FeedbackReadViewModel()
            {
                Id = domainModel.Id,
                ReadOn = domainModel.ReadOn,
                UserId = domainModel.UserId
            };
        }

        public static CompanyViewModel CompanyViewModelFrom(Company domainModel)
        {
			var result = new CompanyViewModel() {
				Id = domainModel.Id,
				CompanyName = domainModel.CompanyName,
				PhysicalAddress = domainModel.PhysicalAddress,
				PostalAddress = domainModel.PostalAddress,
				TelephoneNumber = domainModel.TelephoneNumber,
				WebsiteAddress = domainModel.WebsiteAddress,
				LayerSubDomain = domainModel.LayerSubDomain,
				ProvisionalAccountLink = domainModel.ProvisionalAccountLink,
				CompanyConnectionString = domainModel.CompanyConnectionString,
				LogoImageUrl = domainModel.LogoImageUrl,
				CompanyCreatedBy = domainModel.CreatedBy,
				ClientSystemName = domainModel.ClientSystemName,
				CreatedOn = domainModel.CreatedOn,
				IsChangePasswordFirstLogin = domainModel.IsChangePasswordFirstLogin,
				IsSendWelcomeSMS = domainModel.IsSendWelcomeSMS,
				IsForSelfProvision = domainModel.IsForSelfProvision,
				IsSelfCustomer = domainModel.IsSelfCustomer,
				ApplyCustomCss = domainModel.ApplyCustomCss,
				IsForSelfSignUp = domainModel.IsForSelfSignUp,
				IsSelfSignUpApprove = domainModel.IsSelfSignUpApprove,
				IsEmployeeCodeReq=domainModel.IsEmployeeCodeReq,
				IsEnabledEmployeeCode=domainModel.IsEnabledEmployeeCode,
				CompanySiteTitle=domainModel.CompanySiteTitle,
				DefaultUserExpireDays = domainModel.DefaultUserExpireDays,
				ShowCompanyNameOnDashboard = domainModel.ShowCompanyNameOnDashboard,
				ShowCompanyLogoOnDashboard = !domainModel.HideDashboardLogo,
				TestExpiryNotificationInterval = domainModel.TestExpiryNotificationInterval,
				DashboardVideoFile = domainModel.DashboardVideoFile,
				DashboardVideoTitle = domainModel.DashboardVideoTitle,
				DashboardVideoDescription = domainModel.DashboardVideoDescription,
				DashboardQuoteAuthor = domainModel.DashboardQuoteAuthor,
				DashboardQuoteText = domainModel.DashboardQuoteText,
				EnableChecklistDocument = domainModel.EnableChecklistDocument,
				EnableCategoryTree = domainModel.EnableCategoryTree,
				EnableGlobalAccessDocuments = domainModel.EnableGlobalAccessDocuments,
				EnableVirtualClassRoom = domainModel.EnableVirtualClassRoom,
				IsActive=domainModel.IsActive,
                ActiveDirectoryEnabled = domainModel.ActiveDirectoryEnabled,

                JitsiServerName =domainModel.JitsiServerName
            };
            if (domainModel.TestExpiryNotificationInterval == NotificationInterval.Arbitrary)
                result.ArbitraryTestExpiryIntervalInDaysBefore = domainModel.ArbitraryTestExpiryIntervalInDaysBefore;
            if (domainModel.ExpiryDate.HasValue)
                result.ExpiryDate = domainModel.ExpiryDate.Value;
            if (domainModel.YearlySubscription.HasValue)
                result.YearlySubscription = domainModel.YearlySubscription.Value;
            if (domainModel.AutoExpire.HasValue)
                result.AutoExpire = domainModel.AutoExpire.Value;
            if (domainModel.CompanyType == CompanyType.CustomerCompany)
                result.CompanyType = Domain.Enums.CompanyType.CustomerCompany;
            if (domainModel.CompanyType == CompanyType.ProvisionalCompany)
                result.CompanyType = Domain.Enums.CompanyType.ProvisionalCompany;
            //if (domainModel.Package != null)
            //{
            //    result.PackageName = domainModel.Package.Title;
            //    result.PackageList = new List<PackageViewModel>()
            //    {
            //        new PackageViewModel()
            //        {
            //            PackageViewModelShort = new PackageViewModelShort()
            //            {
            //                Id = domainModel.Package.Id,
            //                IsForSelfProvision = domainModel.Package.IsForSelfProvision,
            //                Description = domainModel.Package.Description,
            //                Title = domainModel.Package.Title,
            //                MaxNumberOfChaptersPerGuide = domainModel.Package.MaxNumberOfChaptersPerGuide,
            //                MaxNumberOfGuides = domainModel.Package.MaxNumberOfGuides
            //            }
            //        }
            //    };
            //}

            if (domainModel.Bundle != null)
            {
                result.BundleName = domainModel.Bundle.Title;
                result.BundleList = new List<BundleViewModel>
                {
                    new BundleViewModel
                    {
                        BundleViewModelShort = new BundleViewModelShort
                        {
                            Id = domainModel.Bundle.Id,
                            Title = domainModel.Bundle.Title,
                            Description = domainModel.Bundle.Description,
                            MaxNumberOfDocuments = domainModel.Bundle.MaxNumberOfDocuments,
                            IsForSelfProvision = domainModel.Bundle.IsForSelfProvision
                        }
                    }
                };
                result.BundleSize = domainModel.Bundle.MaxNumberOfDocuments;
            }
            result.CustomerConfigurations = domainModel.CustomerConfigurations.AsQueryable().Select(ToCutomerConfigurationModel).ToList();
            result.LegalDisclaimer = domainModel.LegalDisclaimer;
            result.LegalDisclaimerActivationType = LegalDisclaimerActivationType.Disabled;
            if (domainModel.ShowLegalDisclaimerOnLogin)
                result.LegalDisclaimerActivationType = LegalDisclaimerActivationType.ShowOnLogin;
            if (domainModel.ShowLegalDisclaimerOnLoginOnlyOnce)
                result.LegalDisclaimerActivationType = LegalDisclaimerActivationType.ShowOnLoginOnce;
            result.EnableTrainingActivityLoggingModule = domainModel.EnableTrainingActivityLoggingModule;
            result.EnableRaceCode = domainModel.EnableRaceCode;
            return result;
        }
        public static Expression<Func<CustomerConfiguration, CustomerConfigurationModel>> ToCutomerConfigurationModel
            = x => new CustomerConfigurationModel
            {
                Version = x.Version,
                Deleted = x.Deleted,
                Company = CompanyModelShortFrom(x.Company),
                UploadModel = ToUploadModel_Domain.Compile().Invoke(x.Upload),
                Type = x.Type
            };
        public static Expression<Func<FileUpload, FileUploadViewModel>> ToUploadModel_Domain
            = x => new FileUploadViewModel
            {
                Id = x.Id,
                Data = x.Data,
                Name = x.Name,
                Type = x.FileType.ToString(),
                ContentType = x.MIMEType
            };
         public static Expression<Func<FileUpload, UploadModel>> FileUpload_UploadModel
            = x => new UploadModel
            {
                Id = x.Id.ToString(),
                Data = x.Data,
                Name = x.Name,
                Type = x.FileType.ToString(),
                ContentType = x.MIMEType
            };
        public static CompanyModelShort CompanyModelShortFrom(Company domainModel)
        {
            return new CompanyModelShort
            {
                Id = domainModel.Id,
                Name = domainModel.CompanyName
            };
        }
        public static CategoryViewModel CategoryViewModelFrom(Domain.Customer.Models.Categories domainModel)
        {
            var result = new CategoryViewModel()
            {
                Id = domainModel.Id,
                Description = domainModel.Description,
                CategorieTitle = domainModel.CategoryTitle,
                ParentCategoryId = domainModel.ParentCategoryId
            };
            return result;
        }
        public static readonly Expression<Func<Domain.Customer.Models.Categories, CategoryViewModel>> ToCategoryModel
            = x => new CategoryViewModel
            {
                CategorieTitle = x.CategoryTitle,
                Description = x.Description,
                Id = x.Id,
                ParentCategoryId = x.ParentCategoryId
            };
        public static EditUserProfileViewModel EditUserProfileViewModelFrom(StandardUser domainModel)
        {
            var result = new EditUserProfileViewModel();
            result.Id = domainModel.Id;
            result.FullName = $"{domainModel.FirstName} {domainModel.LastName}";
            result.EmailAddress = domainModel.EmailAddress;
            result.MobileNumber = domainModel.MobileNumber;
            result.EmployeeNo = domainModel.EmployeeNo;
            result.IDNumber = domainModel.IDNumber;
            return result;
        }
        public static EditUserProfileViewModel EditUserProfileViewModelFrom(Domain.Models.User domainModel)
        {
            var result = new EditUserProfileViewModel();
            result.Id = domainModel.Id;
            result.FullName = $"{domainModel.FirstName} {domainModel.LastName}";
            result.EmailAddress = domainModel.EmailAddress;
            result.MobileNumber = domainModel.MobileNumber;
            result.EmployeeNo = domainModel.EmployeeNo;
            result.IDNumber = domainModel.IDNumber;
            return result;
        }
        public static FileUploadResultViewModel FileUploadResultViewModelFrom(ChapterUpload domainModel)
        {
            if (domainModel != null && domainModel.Upload != null)
            {
                var result = new FileUploadResultViewModel
                {
                    Id = domainModel.Upload.Id,
                    Name = domainModel.Upload.Name,
                    Description = domainModel.Upload.Description,
                    Size = domainModel.Upload.Data == null ? 0 : domainModel.Upload.Data.Length,
                    Type = domainModel.Upload.ContentType,
                    Number = domainModel.ChapterUploadSequence
                };
                return result;
            }
            return null;
        }
        public static FileUploadResultViewModel FileUploadResultViewModelFrom(ChapterLink domainModel)
        {
            if (domainModel != null)
            {
                var result = new FileUploadResultViewModel
                {
                    Id = domainModel.Id,
                    Url = domainModel.Url,
                    Type = domainModel.Type.ToString(),
                    Embeded = true,
                    Number = domainModel.ChapterUploadSequence
                };
                return result;
            }
            return null;
        }
        public static FileUploadResultViewModel FileUploadResultViewModelFrom(Domain.Customer.Models.FileUploads domainModel)
        {
            if (domainModel != null)
            {
                var result = new FileUploadResultViewModel
                {
                    Id = domainModel.Id,
                    Name = domainModel.Name,
                    Description = domainModel.Description,
                    Size = domainModel.Data.Length,
                    Type = domainModel.ContentType,
                    Number = 0
                };
                return result;
            }
            return null;
        }
        public static readonly Expression<Func<FileUploads, FileUploadResultViewModel>> ToFileUploadResultModel
            = x => new FileUploadResultViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Size = x.Data.Length,
                Type = x.ContentType,
                Number = 0,
            };
        public static readonly Expression<Func<FileUploads, FileUploadViewModel>> FileUploads_FileUploadModel
            = x => new FileUploadViewModel
            {
                ContentType = x.ContentType,
                Data = x.Data,
                Description = x.Description,
                FileUploadType = (FileUploadType)Enum.Parse(typeof(FileUploadType), x.Type, true),
                Id = x.Id,
                Name = !string.IsNullOrWhiteSpace(x.Description) ? x.Description : x.Name,
            };
        public static TrainingGuideViewModel TrainingGuideViewModelFrom(TrainingGuide domainModel)
        {
            var result = new TrainingGuideViewModel
            {
                Description = domainModel.Description,
                CreatedOn = domainModel.CreatedOn,
                TrainingGuidId = domainModel.Id,
                ReferenceId = domainModel.ReferenceId,
                Title = domainModel.Title,
                IsActive = domainModel.IsActive,
                CoverPictureVM = FileUploadResultViewModelFrom(domainModel.CoverPicture),
                PlaybookPreviewMode = domainModel.PlaybookPreviewMode,
                Printable = domainModel.Printable
            };
            domainModel.ChapterList.ForEach(
                chapter => result.TraningGuideChapters.Add(Project.TrainingGuideChapterViewModelFrom(chapter)));

            domainModel.Categories.ForEach(
                category => result.TraningGuideCategories.Add(Project.CategoryViewModelFrom(category)));

            domainModel.Collaborators.ForEach(
                collaborator => result.Collaborators.Add(Project.UserViewModelFrom(collaborator)));
            
            
            return result;
        }
        public static TraningGuideChapterViewModel TrainingGuideChapterViewModelFrom(TraningGuideChapter domainModel)
        {
            var result = new TraningGuideChapterViewModel
            {
                ChapterContent = domainModel.ChapterContent,
                ChapterName = domainModel.ChapterName,
                ChapterNumber = domainModel.ChapterNumber,
                TraningGuidId = domainModel.TraningGuidId,
                TraningGuideChapterId = domainModel.Id
            };
            domainModel.ChapterUploads.ForEach(
                upload => result.Attachments.Add(FileUploadResultViewModelFrom(upload)));

            domainModel.ChapterLinks.ForEach(
                link => result.Attachments.Add(FileUploadResultViewModelFrom(link)));
            domainModel.CKEUploads.Where(x => x.Upload != null).ToList().ForEach(
                x => result.CKEUploads.Add(new CKEditorUploadResultViewModel
                {
                    Id = x.Upload.Id,
                    uploaded  = uploadResult.success,
                    fileName = x.Upload.Name
                }));
            result.Attachments = result.Attachments.OrderBy(n => n.Number).ToList();

            return result;
        }
        public static TrainingTestViewModel TrainingTestViewModelFrom(TrainingTest domainModel)
        {
            var result = new TrainingTestViewModel
            {
                ActivePublishDate = domainModel.ActivePublishDate,
                ActiveStatus = domainModel.ActiveStatus,
                CreateDate = domainModel.CreateDate,
                CreatedBy = domainModel.CreatedBy,
                DraftStatus = domainModel.DraftStatus,
                DraftEditDate = domainModel.DraftEditDate,
                IntroductionContent = domainModel.IntroductionContent,
                IsTestExpiryDate = !domainModel.TestExpiryDate.HasValue,
                ParentTrainingTestId = domainModel.ParentTrainingTestId,
                PassMarks = domainModel.PassMarks,
                ReferenceId = domainModel.ReferenceId,
                TestDuration = domainModel.TestDuration,
                TestExpiryDate = domainModel.TestExpiryDate,
                TestStatus = domainModel.DraftStatus,
                TestTitle = domainModel.TestTitle,
                TestTrophy = domainModel.TestTrophy,
                TrophyName = domainModel.TrophyName,
                TrainingTestId = domainModel.Id,
                AssignMarksToQuestions = domainModel.AssignMarksToQuestions,
                PassPoints = domainModel.PassPoints,
                TestReview = domainModel.TestReview,
                Version = domainModel.Version ?? 0,
                EnableMaximumTestRewriteFunction = domainModel.MaximumNumberOfRewites.HasValue,
                MaximumRewrites = domainModel.MaximumNumberOfRewites,
                DisableQuestionRandomization = domainModel.DisableQuestionRandomization
            };
            if (domainModel.TrainingGuide != null)
                result.TrainingGuideName = domainModel.TrainingGuide.Title;
            if (domainModel.QuestionList != null && domainModel.QuestionList.Count > 0)
                domainModel.QuestionList.ForEach(q => result.QuestionsList.Add(TrainingTestQuestionViewModelFrom(q)));

            return result;
        }
        public static TrainingTestQuestionViewModel TrainingTestQuestionViewModelFrom(TrainingQuestion domainModel)
        {
            var result = new TrainingTestQuestionViewModel
            {
                AnswerWeightage = domainModel.AnswerWeightage,
                CorrectAnswerId = string.IsNullOrWhiteSpace(domainModel.CorrectAnswer) ? Guid.Empty : System.Guid.Parse(domainModel.CorrectAnswer),
                IsDeleted = false,
                TestQuestionNumber = domainModel.TestQuestionNumber,
                TrainingTestQuestionId = domainModel.Id,
                TestQuestion = domainModel.TestQuestion,
                TrainingTestId = domainModel.TrainingTestId
            };
            if (domainModel.Image != null)
            {
                result.ImageContainer = FileUploadResultViewModelFrom(domainModel.Image.Upload);
            }
            if (domainModel.Video != null)
            {
                result.VideoContainer = FileUploadResultViewModelFrom(domainModel.Video.Upload);
            }
            if(domainModel.Audio != null)
            {
                result.AudioContainer = FileUploadResultViewModelFrom(domainModel.Audio.Upload);
            }
            domainModel.TestAnswerList.ForEach(a => result.TestAnswerList.Add(TestAnswerViewModelFrom(a)));
            return result;
        }
        public static TestAnswerViewModel TestAnswerViewModelFrom(TestAnswer domainModel)
        {
            var result = new TestAnswerViewModel
            {
                Option = domainModel.Option,
                TestAnswerId = domainModel.Id,
                TrainingQuestionId = domainModel.TrainingQuestionId,
                Correct = domainModel.Correct,
                Position = domainModel.Position
            };
            return result;
        }
        public static UserCorrespondenceLogViewModel UserCorrespondenceLogViewModelFrom(
            UserCorrespondenceLog domainModel)
        {
            var result = new UserCorrespondenceLogViewModel
            {
                CorrespondenceDate = domainModel.CorrespondenceDate,
                CorrespondenceId = domainModel.Id,
                CorrespondenceType = domainModel.CorrespondenceType,
                Description = domainModel.Description,
                MessageContent = domainModel.Content,
                UserId = domainModel.UserId
            };
            if (domainModel.User != null)
            {
                result.UserViewModel = new UserViewModel
                {
                    Id = domainModel.User.Id,
                    EmailAddress = domainModel.User.EmailAddress,
                    FirstName = domainModel.User.FirstName,
                    LastName = domainModel.User.LastName,
                    MobileNumber = domainModel.User.MobileNumber,
                    Status = domainModel.User.IsActive,
                    ContactNumber = domainModel.User.ContactNumber,
                    ParentUserId = domainModel.User.ParentUserId
                };
                if (domainModel.User.FirstName.Equals(domainModel.User.LastName))
                    result.UserViewModel.FullName = domainModel.User.FirstName;
                else
                {
                    result.UserViewModel.FullName = $"{domainModel.User.FirstName} {domainModel.User.LastName}";
                }
                if (domainModel.User.Roles.Count > 0)
                {
                    var isGlobalAdmin = domainModel.User.Roles.FirstOrDefault(r => r.RoleName == "CustomerAdmin") != null;
                    var isManagingAdmin = !isGlobalAdmin &&
                                          domainModel.User.Roles.Any(r =>
                                              r.RoleName.Contains("Admin") || r.RoleName.Contains("Reporter"));
                    result.UserViewModel.RoleName = isGlobalAdmin ? "Global Admin" :
                        isManagingAdmin ? "Managing Admin" : "Standard User";
                }
            }

            return result;
        }
        public static UserCorrespondenceLogViewModel UserCorrespondenceLogViewModelFrom(
            StandardUserCorrespondanceLog domainModel)
        {
            var result = new UserCorrespondenceLogViewModel
            {
                Description = domainModel.Description,
                MessageContent = domainModel.Content,
                UserId = domainModel.UserId,
                CorrespondenceId = domainModel.Id,
                CorrespondenceDate = domainModel.CorrespondenceDate
            };
            if (domainModel.CorrespondenceType == StandardUserCorrespondenceEnum.Email)
                result.CorrespondenceType = UserCorrespondenceEnum.Email;
            else if (domainModel.CorrespondenceType == StandardUserCorrespondenceEnum.Sms)
                result.CorrespondenceType = UserCorrespondenceEnum.Sms;
            if (domainModel.User != null)
            {
                result.UserViewModel = new UserViewModel
                {
                    Id = domainModel.User.Id,
                    EmailAddress = domainModel.User.EmailAddress,
                    FirstName = domainModel.User.FirstName,
                    LastName = domainModel.User.LastName,
                    MobileNumber = domainModel.User.MobileNumber,
                    Status = domainModel.User.IsActive,
                    ContactNumber = domainModel.User.ContactNumber,
                    ParentUserId = domainModel.User.ParentUserId,
                };
                if (domainModel.User.FirstName.Equals(domainModel.User.LastName))
                    result.UserViewModel.FullName = domainModel.User.FirstName;
                else
                {
                    result.UserViewModel.FullName = $"{domainModel.User.FirstName} {domainModel.User.LastName}";
                }
                if (domainModel.User.Roles.Count > 0)
                {
                    var isGlobalAdmin = domainModel.User.Roles.FirstOrDefault(r => r.RoleName == "CustomerAdmin") != null;
                    var isManagingAdmin = !isGlobalAdmin &&
                                          domainModel.User.Roles.Any(r =>
                                              r.RoleName.Contains("Admin") || r.RoleName.Contains("Reporter"));
                    result.UserViewModel.RoleName = isGlobalAdmin ? "Global Admin" :
                        isManagingAdmin ? "Managing Admin" : "Standard User";
                }
            }

            return result;
        }
       
        public static UserActivityLogViewModel UserActivityLogViewModelFrom(UserActivityLog domainModel)
        {
            var result = new UserActivityLogViewModel
            {
                UserId = domainModel.UserId,
                ActivityDate = domainModel.ActivityDate,
                ActivityId = domainModel.Id,
                ActivityType = domainModel.ActivityType.ToString(),
                Description = domainModel.Description,
            };
            if (domainModel.User != null)
            {
                result.User = new UserViewModel
                {
                    Id = domainModel.User.Id,
                    EmailAddress = domainModel.User.EmailAddress,
                    FirstName = domainModel.User.FirstName,
                    LastName = domainModel.User.LastName,
                    MobileNumber = domainModel.User.MobileNumber,
                    Status = domainModel.User.IsActive,
                    ContactNumber = domainModel.User.ContactNumber,
                    ParentUserId = domainModel.User.ParentUserId
                };
                if (domainModel.User.Roles.Count > 0)
                {
                    foreach (
                        var userRole in domainModel.User.Roles.Select(role => UserRoleHelper.GetUserRole(role.RoleName))
                        )
                    {
                        result.User.CustomerRoles.Add(userRole.ToString());
                    }
                }
            }

            return result;
        }
        public static UserActivityLogViewModel UserActivityLogViewModelFrom(StandardUserActivityLog domainModel)
        {
            var result = new UserActivityLogViewModel
            {
                ActivityDate = domainModel.ActivityDate,
                ActivityId = domainModel.Id,
                ActivityType = domainModel.ActivityType,
                Description = domainModel.Description,
            };
            if (domainModel.User != null)
            {
                result.UserId = domainModel.User.Id;
                result.User = new UserViewModel
                {
                    Id = domainModel.User.Id,
                    EmailAddress = domainModel.User.EmailAddress,
                    FirstName = domainModel.User.FirstName,
                    LastName = domainModel.User.LastName,
                    MobileNumber = domainModel.User.MobileNumber,
                    Status = domainModel.User.IsActive,
                    ContactNumber = domainModel.User.ContactNumber,
                    ParentUserId = domainModel.User.ParentUserId
                };
                if (domainModel.User.Roles.Count > 0)
                {
                    foreach (var userRole in domainModel.User.Roles.Select(role => role.RoleName))
                    {
                        result.User.CustomerRoles.Add(userRole);
                    }
                }
            }

            return result;
        }
        public static UserActivityLogViewModel UserActivityLogViewModelFrom(StandardUserDisclaimerActivityLog domainModel)
        {
            var result = new UserActivityLogViewModel
            {
                ActivityDate = domainModel.Stamp,
                ActivityId = domainModel.Id,
                ActivityType = StandardUserActivityLog.LegalDisclaimer.ToString(),
                Description = string.Format("{0} the Company Legal Disclaimer from IP Address : {1}", domainModel.Accepted ? "Accepted" : "Declined", domainModel.IPAddress),
            };
            if (domainModel.User != null)
            {
                result.UserId = domainModel.User.Id;
                result.User = new UserViewModel
                {
                    Id = domainModel.User.Id,
                    EmailAddress = domainModel.User.EmailAddress,
                    FirstName = domainModel.User.FirstName,
                    LastName = domainModel.User.LastName,
                    MobileNumber = domainModel.User.MobileNumber,
                    Status = domainModel.User.IsActive,
                    ContactNumber = domainModel.User.ContactNumber,
                    ParentUserId = domainModel.User.ParentUserId
                };
                if (domainModel.User.Roles.Count > 0)
                {
                    foreach (var userRole in domainModel.User.Roles.Select(role => role.RoleName))
                    {
                        result.User.CustomerRoles.Add(userRole);
                    }
                }
            }

            return result;
        }
    }
   
}
namespace Ramp.Services
{
    public static partial class Extensions
    {
        public static TestAnswer toDomainModel(this TestAnswerViewModel model)
        {
            return Projection.Project.TestAnswerFromViewModel.Compile().Invoke(model);
        }
    }
}
namespace Ramp.Services.Projection
{
    public static partial class Project
    {
        public static readonly Expression<Func<TrainingTestQuestionViewModel, TrainingQuestion>> TrainingQuestionFromViewModel =
           x => new TrainingQuestion
           {
               AnswerWeightage = x.AnswerWeightage,
               TestQuestion = x.TestQuestion,
               CorrectAnswer = x.TestAnswerList.Where(a => a.Correct).Single().TestAnswerId.ToString(),
               Id = x.TrainingTestQuestionId,
               TestQuestionNumber = x.TestQuestionNumber,
               TrainingTestId = x.TrainingTestId,
               TestAnswerList = x.TestAnswerList.AsQueryable().Select(TestAnswerFromViewModel).ToList(),

           };
        public static readonly Expression<Func<TestAnswerViewModel, TestAnswer>> TestAnswerFromViewModel =
            x => new TestAnswer
            {
                Correct = x.Correct,
                Id = x.TestAnswerId,
                Option = x.Option,
                Position = x.Position,
                TrainingQuestionId = x.TrainingQuestionId
            };

        
        public static readonly Expression<Func<StandardUserDisclaimerActivityLog, UserDisclaimerActivityLogEntryModelShort>> ToUserDisclaimerActivityLogEntryModelShort
            = x => new UserDisclaimerActivityLogEntryModelShort
            {
                Accepted = x.Accepted,
                Deleted = x.Deleted,
                Id = x.Id,
                IPAddress = x.IPAddress,
                Stamp = x.Stamp
            };
    }
}