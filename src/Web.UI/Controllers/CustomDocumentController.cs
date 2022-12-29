
using Common.Events;
using Common.Query;
using Data.EF.Customer;
using Domain.Customer;
using Domain.Customer.Models;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Ramp.Contracts.Command;
using Ramp.Contracts.Command.AcrobatField;
using Ramp.Contracts.Command.CheckList;
using Ramp.Contracts.Command.CustomDocument;
using Ramp.Contracts.Command.DocumentUsage;
using Ramp.Contracts.Command.Form;
using Ramp.Contracts.Command.Memo;
using Ramp.Contracts.Command.Policy;
using Ramp.Contracts.Command.Test;
using Ramp.Contracts.Command.TrainingManual;
using Ramp.Contracts.CommandParameter;
using Ramp.Contracts.CommandParameter.CustomDocument;
using Ramp.Contracts.Events.DocumentWorkflow;
using Ramp.Contracts.Query.AcrobatField;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Ramp.Contracts.Query.CustomDocument;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.DocumentCategory;
using Ramp.Contracts.Query.Label;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.Query.Upload;
using Ramp.Contracts.QueryParameter.Upload;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using Ramp.Services.Projection;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Web.UI.Code.Extensions;
using Web.UI.Helpers;

namespace Web.UI.Controllers
{
    public class CustomDocumentController : KnockoutDocumentController<CustomDocumentListQuery, CustomDocumentListModel, CustomDocument, CustomDocumentModel, AddOrUpdateCustomDocumentCommand>
    {
        private readonly ITransientRepository<AssignedDocument> _assignedDocumentRepository;
        private readonly ITransientRepository<StandardUser> _userRepository;
        public CustomDocumentController(ITransientRepository<AssignedDocument> assignedDocumentRepository,ITransientRepository<StandardUser> userRepository)
        {
            _assignedDocumentRepository = assignedDocumentRepository;
            _userRepository = userRepository;
        }
        public override void Edit_PostProcess(CustomDocumentModel model, string companyId = null, DocumentUsageStatus? status = null, string userId = null)
        {

            if (model.CoverPicture != null)
            {
                model.CoverPicture.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(model.CoverPicture.Id, 300, 300, companyId));
                model.CoverPicture.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.CoverPicture.Id, companyId));
            }

            if (model.Certificate != null)
            {
                model.Certificate.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(model.Certificate.Id, companyId));
            }

            model.DocumentType = DocumentType.custom;

            if (string.IsNullOrEmpty(userId))
            {
                userId = Thread.CurrentPrincipal.GetId().ToString();
            }

            var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = userId, DocumentId = model.Id });

            #region ["Training Manual"]
            // Training Manual
            model.TMContentModels.ToList().ForEach(content =>
            {
                content.Attachments.ToList().ForEach(attachment =>
                {
                    attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                    attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                    attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));


                });
                content.ContentToolsUploads.ToList().ForEach(attachment =>
                {
                    attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
                });

                if (assignedDocument != null)
                {
                    // To Display Standeruser Attachment and Additional Notes
                    var TrainingManualChapterUserUpload = ExecuteQuery<TrainingManualChapterUserResultQuery, List<UploadResultViewModel>>(new TrainingManualChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TrainingManualChapterId = content.Id, UserId = userId });

                    content.StandardUserAttachments = TrainingManualChapterUserUpload.Where(z => !z.isSignOff).ToList();
                    var SigntureData = TrainingManualChapterUserUpload.Where(z => z.isSignOff).FirstOrDefault();
                    if (SigntureData != null)
                    {
                        content.SignatureUploadId = SigntureData.Id;
                        content.SignatureThumbnail = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(SigntureData.Id, 300, 500, companyId));
                    }
                    content.StandardUserAttachments.ToList().ForEach(attachment =>
                    {
                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                        attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                    });

                    var TrainingManualChapterUserResult = ExecuteQuery<TrainingManualChapterUserResultQuery, TrainingManualChapterUserResultViewModel>(new TrainingManualChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TrainingManualChapterId = content.Id });
                    if (TrainingManualChapterUserResult != null)
                    {
                        content.IsChecked = TrainingManualChapterUserResult.IsChecked;
                        content.IssueDiscription = TrainingManualChapterUserResult.IssueDiscription;
                    }
                    else
                        content.IsChecked = false;
                }
            });

            if (model.TMContentModels.Count() <= 0)
            {
                var TM = new TrainingManualChapterModel();
                TM.IsAttached = false;
                TM.IsSignOff = false;
                model.TMContentModels.ToList().Add(TM);
                //model.TMContentModels.FirstOrDefault().IsAttached = false;
            }
            #endregion

            #region ["Policy"]
            // Policy /*added by softude*/
            if (model.PolicyContentModels.Count() > 0)
            {
                model.PolicyContentModels.ToList().ForEach(PolicyContentModel =>
                {
                    PolicyContentModel.Attachments.ToList().ForEach(attachment =>
                    {

                        //********************* This Block Has Been Modified By Softude *******************************

                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                        attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));

                        //**************************************** Ended***************************************************

                    });
                    PolicyContentModel.ContentToolsUploads.ToList().ForEach(attachment =>
                    {
                        attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
                    });

                    if (assignedDocument != null)
                    {
                        // To Display Standeruser Attachment and Additional Notes
                        var PolicyContentBoxUserUpload = ExecuteQuery<PolicyContentBoxUserResultQuery, List<UploadResultViewModel>>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = PolicyContentModel.Id, UserId = userId });

                        PolicyContentModel.StandardUserAttachments = PolicyContentBoxUserUpload.Where(z => !z.isSignOff).ToList();

                        //var PolicyContentBoxUserUploadSignature = ExecuteQuery<policycontent, List<UploadResultViewModel>>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = PolicyContentModel.Id, UserId = userId });

                        var SigntureData = PolicyContentBoxUserUpload.Where(z => z.isSignOff).FirstOrDefault();      /*.OrderByDescending(z => z.CreatedDate)*/

                        if (SigntureData != null)
                        {
                            PolicyContentModel.SignatureUploadId = SigntureData.Id;
                            PolicyContentModel.SignatureThumbnail = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(SigntureData.Id, 300, 500, companyId));
                        }
                        PolicyContentModel.StandardUserAttachments.ToList().ForEach(attachment =>
                        {
                            attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                            attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                            attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                        });

                        var PolicyContentBoxUserResult = ExecuteQuery<PolicyContentBoxUserResultQuery, PolicyContentBoxUserResultViewModel>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = PolicyContentModel.Id });
                        if (PolicyContentBoxUserResult != null)
                        {
                            PolicyContentModel.IsChecked = PolicyContentBoxUserResult.IsChecked;
                            PolicyContentModel.IssueDiscription = PolicyContentBoxUserResult.IssueDiscription;
                            PolicyContentModel.IsActionNeeded = PolicyContentBoxUserResult.IsActionNeeded;
                        }
                        else
                            PolicyContentModel.IsChecked = false;
                    }

                });
            }


            //********************* This Block Has Been Modified By Softude *******************************
            if (model.PolicyContentModels.Count() <= 0)
            {
                var Policy = new PolicyContentBoxModel();
                Policy.IsAttached = false;
                Policy.IsSignOff = false;
                model.PolicyContentModels.ToList().Add(Policy);
            }
            //**************************************** Ended ********************************************

            #endregion

            #region ["Memo"]
            // Memo
            model.MemoContentModels.ToList().ForEach(content =>
            {
                content.Attachments.ToList().ForEach(attachment =>
                {
                    attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                    attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                    attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                });
                content.ContentToolsUploads.ToList().ForEach(attachment =>
                {
                    attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
                });
                if (assignedDocument != null)
                {
                    // To Display Standeruser Attachment and Additional Notes
                    var memoChapterUserUpload = ExecuteQuery<MemoChapterUserResultQuery, List<UploadResultViewModel>>(new MemoChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, MemoChapterId = content.Id, UserId = userId });

                    content.StandardUserAttachments = memoChapterUserUpload.Where(z => !z.isSignOff).ToList();
                    var SigntureData = memoChapterUserUpload.Where(z => z.isSignOff).FirstOrDefault();
                    if (SigntureData != null)
                    {
                        content.SignatureUploadId = SigntureData.Id;
                        content.SignatureThumbnail = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(SigntureData.Id, 300, 500, companyId));
                    }
                    content.StandardUserAttachments.ToList().ForEach(attachment =>
                    {
                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                        attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                    });

                    var MemoChapterUserResult = ExecuteQuery<MemoChapterUserResultQuery, MemoChapterUserResultViewModel>(new MemoChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, MemoChapterId = content.Id, UserId = userId });
                    if (MemoChapterUserResult != null)
                    {
                        content.IsChecked = MemoChapterUserResult.IsChecked;
                        content.IssueDiscription = MemoChapterUserResult.IssueDiscription;
                    }
                    else
                        content.IsChecked = false;

                }

            });

            #endregion

            #region ["AcrobatField"]
            // AcrobatField

            model.AcrobatFieldContentModels.ToList().ForEach(content =>
            {
                content.Attachments.ToList().ForEach(attachment =>
                {
                    attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                    attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                    attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                    attachment.AcrofieldJson = fetchAcroFields(attachment.Id, companyId);
                });
                content.ContentToolsUploads.ToList().ForEach(attachment =>
                {
                    attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));

                });
                if (assignedDocument != null)
                {
                    // To Display Standeruser Attachment and Additional Notes
                    var AcrobatFieldChapterUserUpload = ExecuteQuery<AcrobatFieldChapterUserResultQuery, List<UploadResultViewModel>>(new AcrobatFieldChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, AcrobatFieldChapterId = content.Id, UserId = userId });

                    content.StandardUserAttachments = AcrobatFieldChapterUserUpload.Where(z => !z.isSignOff).ToList();
                    var SigntureData = AcrobatFieldChapterUserUpload.Where(z => z.isSignOff).FirstOrDefault();
                    if (SigntureData != null)
                    {
                        content.SignatureUploadId = SigntureData.Id;
                        content.SignatureThumbnail = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(SigntureData.Id, 300, 500, companyId));
                    }
                    content.StandardUserAttachments.ToList().ForEach(attachment =>
                    {
                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                        attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                    });

                    var AcrobatFieldChapterUserResult = ExecuteQuery<AcrobatFieldChapterUserResultQuery, AcrobatFieldChapterUserResultViewModel>(new AcrobatFieldChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, AcrobatFieldChapterId = content.Id, UserId = userId });
                    if (AcrobatFieldChapterUserResult != null)
                    {
                        content.IsChecked = AcrobatFieldChapterUserResult.IsChecked;
                        content.IssueDiscription = AcrobatFieldChapterUserResult.IssueDiscription;
                    }
                    else
                        content.IsChecked = false;

                }



            });

            #endregion

            #region ["Activity Book/CheckList"]
            // Activity Book
            model.CLContentModels.ToList().ForEach(content =>
            {
                content.Attachments.ToList().ForEach(attachment =>
                {
                    attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                    attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                    attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                });
                content.ContentToolsUploads.ToList().ForEach(attachment =>
                {
                    attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
                });

                if (assignedDocument != null)
                {
                    // To Display Standeruser Attachment and Additional Notes
                    var CheckListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = content.Id, UserId = userId });

                    content.StandardUserAttachments = CheckListChapterUserUpload.Where(z => !z.isSignOff).ToList();
                    var SigntureData = CheckListChapterUserUpload.Where(z => z.isSignOff).FirstOrDefault();
                    if (SigntureData != null)
                    {
                        content.SignatureUploadId = SigntureData.Id;
                        content.SignatureThumbnail = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(SigntureData.Id, 300, 500, companyId));
                    }
                    content.StandardUserAttachments.ToList().ForEach(attachment =>
                    {
                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                        attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                    });

                    var CheckListChapterUserResult = ExecuteQuery<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = content.Id, UserId = userId });
                    if (CheckListChapterUserResult != null)
                    {
                        content.IsChecked = CheckListChapterUserResult.IsChecked;
                        content.IssueDiscription = CheckListChapterUserResult.IssueDiscription;
                    }

                    //commented by softude

                    //else
                    //    content.IsChecked = false;

                    //commented by softude
                }
            });

            #endregion

            #region   ["Form Content"]

            if (model.FormContentModels.Count() > 0)
            {
                model.FormContentModels.ToList().ForEach(FormContentModel =>
                {
                    if (assignedDocument != null)
                    {
                        //var PolicyContentBoxUserResult = ExecuteQuery<PolicyContentBoxUserResultQuery, PolicyContentBoxUserResultViewModel>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = PolicyContentModel.Id });
                        //if (PolicyContentBoxUserResult != null)
                        //{
                        //    PolicyContentModel.IsChecked = PolicyContentBoxUserResult.IsChecked;
                        //    PolicyContentModel.IssueDiscription = PolicyContentBoxUserResult.IssueDiscription;
                        //    PolicyContentModel.IsActionNeeded = PolicyContentBoxUserResult.IsActionNeeded;
                        //}
                        //else
                        //    PolicyContentModel.IsChecked = false;
                    }

                });
            }

            #endregion

            #region ["Test Content"]
            //Test 
            if (model.TestContent.ContentModels != null)
            {
                model.TestContent.ContentModels.ToList().ForEach(e =>
                {
                    var d = model.TestContentModels.Where(z => z.Id == e.Id).FirstOrDefault();
                    if (d != null)
                    {

                        e.NoteAllow = d.NoteAllow;
                        e.CheckRequired = d.CheckRequired;
                        e.AttachmentRequired = d.AttachmentRequired;
                        e.dynamicFields = d.dynamicFields;
                        e.IsSignOff = d.IsSignOff;
                        e.CustomDocumentOrder = d.CustomDocumentOrder;
                    }
                });

                model.TestContent.ContentModels.ToList().ForEach(content =>
                {
                    content.Attachments.ToList().ForEach(attachment =>
                    {
                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                    });
                    content.ContentToolsUploads.ToList().ForEach(attachment =>
                    {
                        attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
                    });
                    if (assignedDocument != null)
                    {
                        // To Display Standeruser Attachment and Additional Notes
                        var TestChapterUserUpload = ExecuteQuery<TestChapterUserResultQuery, List<UploadResultViewModel>>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = content.Id, UserId = userId });

                        content.StandardUserAttachments = TestChapterUserUpload.Where(z => !z.isSignOff).ToList();
                        var SigntureData = TestChapterUserUpload.Where(z => z.isSignOff).FirstOrDefault();
                        if (SigntureData != null)
                        {
                            content.SignatureUploadId = SigntureData.Id;
                            content.SignatureThumbnail = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(SigntureData.Id, 300, 500, companyId));
                        }
                        content.StandardUserAttachments.ToList().ForEach(attachment =>
                        {
                            attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                            attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                            attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                        });

                        var TestChapterUserResult = ExecuteQuery<TestChapterUserResultQuery, TestChapterUserResultViewModel>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = content.Id, UserId = userId });
                        if (TestChapterUserResult != null)
                        {
                            content.IsChecked = TestChapterUserResult.IsChecked;
                            content.IssueDiscription = TestChapterUserResult.IssueDiscription;
                        }
                        else
                            content.IsChecked = false;
                    }
                });
            }

            #endregion

            #region ["Test Content"]

            model.TestContentModels.ToList().ForEach(content =>
            {
                content.Attachments.ToList().ForEach(attachment =>
                {
                    attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                    attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                });
                content.ContentToolsUploads.ToList().ForEach(attachment =>
                {
                    attachment.url = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.url, null, null, companyId));
                });

                if (assignedDocument != null)
                {
                    // To Display Standeruser Attachment and Additional Notes
                    var TestChapterUserUpload = ExecuteQuery<TestChapterUserResultQuery, List<UploadResultViewModel>>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = content.Id, UserId = userId });

                    content.StandardUserAttachments = TestChapterUserUpload;
                    content.StandardUserAttachments.ToList().ForEach(attachment =>
                    {
                        attachment.ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnailFromCompany(attachment.Id, 300, 300, companyId));
                        attachment.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(attachment.Id, companyId));
                        attachment.PreviewPath = Url.ActionLink<UploadController>(a => a.Preview(attachment.Id, companyId));
                    });

                    var TestChapterUserResult = ExecuteQuery<TestChapterUserResultQuery, TestChapterUserResultViewModel>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = content.Id, UserId = userId });
                    if (TestChapterUserResult != null)
                    {
                        content.IsChecked = TestChapterUserResult.IsChecked;
                        content.IssueDiscription = TestChapterUserResult.IssueDiscription;
                    }
                    else
                        content.IsChecked = false;
                }
            });
            #endregion

            var trainigLabels = new List<string>();

            foreach (var item in model.TrainingLabels.Split(','))
            {
                var label = ExecuteQuery<FetchByNameQuery, TrainingLabelModel>(new FetchByNameQuery() { Name = item });
                trainigLabels.Add(label.Id);
            }

            model.LabelIds = string.Join(",", trainigLabels);

            var labels = ExecuteQuery<TrainingLabelListQuery, IEnumerable<TrainingLabelListModel>>(new TrainingLabelListQuery());

            ViewBag.Labels = labels.OrderBy(c => c.Name).ToList();
            ((IDictionary<string, string>)ViewBag.Links).Add("certificates", Url.ActionLink<AchievementController>(a => a.List(null))); //Added by Softude

            //neeraj
            var x = PortalContext.Current.UserCompany.Id;

            var ca = ExecuteQuery<FetchAllRecordsQuery,
                IEnumerable<StandardUser>>(new FetchAllRecordsQuery());

            var l = new List<StandardUser>();

            foreach (var c in ca.ToList())
            {
                if (c.Id != User.GetId())
                {
                    l.Add(c);
                }
            }

            ViewBag.ContentApprovers = l;

            //get all approver name from document Id
            if (model.Approver != null)
            {
                List<string> names = new List<string>();

                string[] Ids = model.Approver.Split(',');

                foreach (var id in Ids)
                {
                    var userDetail = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = id });

                    names.Add(userDetail.UserName);
                }

                ViewBag.DocumentApprovers = string.Join(",", names);
            }
        }

        [HttpGet]
        public ActionResult ChapterStack(string doc)
        {
            return PartialView("partial/_" + doc);
        }

        protected override void Preview_PostProcess(CustomDocumentModel model, string companyId = null, string checkUser = null, bool isGlobal = false, DocumentUsageStatus? status = null)
        {
            var x = PortalContext.Current.UserCompany.Id;

            var ca = ExecuteQuery<FetchAllRecordsQuery, IEnumerable<StandardUser>>(new FetchAllRecordsQuery());
            var l = new List<StandardUser>();
            foreach (var c in ca.ToList())
            {
                if (c.Id != User.GetId())
                    if (c.Id != User.GetId())
                    {
                        l.Add(c);
                    }
            }

            ViewBag.ContentApprovers = l;

            if (model.Status.ToString() == "Viewed") { ViewBag.Show = false; }
            else { ViewBag.Show = true; }

            // Test
            var test = ExecuteQuery<FetchByIdQuery<Test>, TestResultModel>(new FetchByIdQuery<Test> { Id = model.Id });
            test.IsGlobalAccessed = isGlobal;
            model.TestContent = test;

            //Policy 
            var policyData = ExecuteQuery<FetchByIdQuery<Domain.Customer.Models.Policy.Policy>, PolicyModel>(new FetchByIdQuery<Domain.Customer.Models.Policy.Policy> { Id = model.Id });
            policyData.IsGlobalAccessed = isGlobal;
            model.PolicyContent = policyData;


            Edit_PostProcess(model, companyId, null, checkUser);

        }


        public JsonResult ResourceCenter(CustomDocumentModel custom)
        {
            var documentUploadParent = Guid.NewGuid().ToString();
            var myUploadParent = Guid.NewGuid().ToString();
            var parentIds = Guid.NewGuid().ToString();
            var root = new List<dynamic>
            {
                new
                {
                    text = "Document Upload",
                    id =documentUploadParent,
                    parent = "#",
                    isParentNode = true,
                    state = new
                    {
                        opened = true
                    },
                },
                new
                {
                    text = "My Upload",
                    id = myUploadParent,
                    parent = "#",
                    isParentNode = true,
                    state = new
                    {
                        opened = true
                    },
                }
            };
            List<JSTreeViewModel> ParentTree = new List<JSTreeViewModel>();
            List<JSTreeViewModel> ChildTree = new List<JSTreeViewModel>();
            var TrainingModule = custom.TMContentModels.ToList();
            var TestContentModule = custom.TestContentModels.ToList();
            var CLContentModule = custom.CLContentModels.ToList();
            var MemoContentModule = custom.MemoContentModels.ToList();
            var PolicyContentModule = custom.PolicyContentModels.ToList();
            var AcrobatContentModule = custom.AcrobatFieldContentModels.ToList();
            var FormContentModule = custom.FormContentModels.ToList();
            string SectionName = string.Empty;

            #region [Training Module]
            if (TrainingModule.Count > 0)
            {
                TrainingModule.ForEach(p =>
                {

                    if (p.IsConditionalLogic == false)
                    {
                        if (p.Attachments.Count() > 0)
                        {
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = documentUploadParent,
                                order = p.CustomDocumentOrder

                            });
                            p.Attachments.ForEach(c =>
                            {
                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }

                        if (p.StandardUserAttachments.Count() > 0)
                        {
                            parentIds = Guid.NewGuid().ToString();
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = myUploadParent,
                                order = p.CustomDocumentOrder

                            });
                            p.StandardUserAttachments.ForEach(c =>
                            {
                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }
                    }
                });
            }
            #endregion

            #region [Test Content Module]
            if (TestContentModule.Count > 0)
            {
                TestContentModule.ForEach(p =>
                {
                    parentIds = Guid.NewGuid().ToString();
                    if (p.Attachments.Count() > 0)
                    {
                        ParentTree.Add(new JSTreeViewModel
                        {
                            id = parentIds,
                            isParentNode = true,
                            text = p.Title,
                            parent = documentUploadParent,
                            order = p.CustomDocumentOrder
                        });
                        p.Attachments.ForEach(c =>
                        {

                            SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                            ChildTree.Add(new JSTreeViewModel { id = c.Id, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                        });
                    }
                    if (p.StandardUserAttachments.Count() > 0)
                    {
                        parentIds = Guid.NewGuid().ToString();
                        ParentTree.Add(new JSTreeViewModel
                        {
                            id = parentIds,
                            isParentNode = true,
                            text = p.Title,
                            parent = myUploadParent,
                            order = p.CustomDocumentOrder
                        });
                        p.StandardUserAttachments.ForEach(c =>
                        {
                            SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                            ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                        });
                    }
                });
            }
            #endregion

            #region ["Checklist/ActivityBook"]
            if (CLContentModule.Count > 0)
            {
                CLContentModule.ForEach(p =>
                {
                    if (p.IsConditionalLogic == false)
                    {
                        parentIds = Guid.NewGuid().ToString();
                        if (p.Attachments.Count() > 0)
                        {
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = documentUploadParent,
                                order = p.CustomDocumentOrder

                            });
                            p.Attachments.ForEach(c =>
                            {
                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }
                        if (p.StandardUserAttachments.Count() > 0)
                        {
                            parentIds = Guid.NewGuid().ToString();
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = myUploadParent,

                            });
                            p.StandardUserAttachments.ForEach(c =>
                            {
                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }
                    }
                });

            }
            #endregion

            #region ["Memo"]
            if (MemoContentModule.Count > 0)
            {
                MemoContentModule.ForEach(p =>
                {
                    if (p.IsConditionalLogic == false)
                    {
                        parentIds = Guid.NewGuid().ToString();
                        if (p.Attachments.Count() > 0)
                        {
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = documentUploadParent,
                                order = p.CustomDocumentOrder

                            });
                        }
                        p.Attachments.ForEach(c =>
                        {
                            SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                            ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                        });
                        if (p.StandardUserAttachments.Count() > 0)
                        {
                            parentIds = Guid.NewGuid().ToString();
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = myUploadParent,

                            });
                            p.StandardUserAttachments.ForEach(c =>
                            {
                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }
                    }
                });

            }
            #endregion

            #region ["Policy"]
            if (PolicyContentModule.Count > 0)
            {
                PolicyContentModule.ForEach(p =>
                {
                    if (p.IsConditionalLogic == false)
                    {
                        parentIds = Guid.NewGuid().ToString();
                        if (p.Attachments.Count() > 0)
                        {
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = documentUploadParent,
                                order = p.CustomDocumentOrder

                            });
                            p.Attachments.ForEach(c =>
                            {
                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";

                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }
                        if (p.StandardUserAttachments.Count() > 0)
                        {
                            parentIds = Guid.NewGuid().ToString();
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = myUploadParent,

                            });
                            p.StandardUserAttachments.ForEach(c =>
                            {
                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }
                    }
                });

            }
            #endregion

            #region ["AcrobatField"]
            if (AcrobatContentModule.Count > 0)
            {
                AcrobatContentModule.ForEach(p =>
                {
                    if (p.IsConditionalLogic == false)
                    {
                        parentIds = Guid.NewGuid().ToString();
                        if (p.Attachments.Count() > 0)
                        {
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = documentUploadParent,
                                order = p.CustomDocumentOrder

                            });
                            p.Attachments.ForEach(c =>
                            {

                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }
                        if (p.StandardUserAttachments.Count() > 0)
                        {
                            parentIds = Guid.NewGuid().ToString();
                            ParentTree.Add(new JSTreeViewModel
                            {
                                id = parentIds,
                                isParentNode = true,
                                text = p.Title,
                                parent = myUploadParent,

                            });
                            p.StandardUserAttachments.ForEach(c =>
                            {
                                SectionName = "<a href=\"" + c.Url + "\" title=\"" + c.Description + "\" class=\"fancybox.image\">" + c.Name + "</a>";
                                ChildTree.Add(new JSTreeViewModel { id = c.Id, isParentNode = true, parent = parentIds, text = SectionName, type = c.Type, url = c.Url });
                            });
                        }
                    }
                });
            }
            #endregion


            IEnumerable<dynamic> result = ParentTree.OrderBy(z => z.order).Concat(ChildTree).Concat(root);

            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [System.Web.Http.HttpPost]
        [ValidateInput(false)]
        public JsonResult CompleteChaptersUserResult(CustomDocumentModel dataModel, DocumentUsageStatus? status = null, bool IsGlobalAccessed = false, string DeclineMessage = null)
        {
            string[] Ids = null;
            var userId = Thread.CurrentPrincipal.GetId().ToString();
            var userRole = Thread.CurrentPrincipal.IsInStandardUserRole();
            var isMultipleRoles = Thread.CurrentPrincipal.IsInAdminRole() && Thread.CurrentPrincipal.IsInStandardUserRole();

            var viewDate = DateTime.UtcNow;

            #region CreateOrUpdateDocumentUsageCommand

            ExecuteCommand(new CreateOrUpdateDocumentUsageCommand
            {
                DocumentId = dataModel.Id.ToString(),
                DocumentType = DocumentType.custom,
                UserId = userId,
                ViewDate = viewDate,
                IsGlobalAccessed = IsGlobalAccessed,
                Status = status,
                AssignedDocumentId = dataModel.AssignedDocumentId
            });
            #endregion

            #region CreateOrUpdateCustomDocumentMessageCenterCommand
            if (!string.IsNullOrWhiteSpace(DeclineMessage))
            {
                ExecuteCommand(new CreateOrUpdateCustomDocumentMessageCenterCommand
                {
                    DocumentId = dataModel.Id.ToString(),
                    DocumentType = DocumentType.custom,
                    UserId = userId,
                    Messages = DeclineMessage,
                    IsGlobalAccessed = IsGlobalAccessed,
                    Status = status,
                    AssignedDocumentId = dataModel.AssignedDocumentId
                });
            }
            var model = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = userId, DocumentId = dataModel.Id });
            #endregion

            #region Training Manual
            foreach (var item in dataModel.TMContentModels)
            {
                ExecuteCommand(new CreateOrUpdateTrainingManualChapterUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, TrainingManualChapterId = item.Id, IsChecked = item.IsChecked, IssueDiscription = item.IssueDiscription });

                foreach (var upload in item.StandardUserAttachments)
                {
                    if (!dataModel.IsReportView)
                    {
                        ExecuteCommand(new CreateOrUpdateTrainingManualChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, TrainingManualChapterId = item.Id, UploadId = upload.Id });  //done  changes by softude 
                    }

                }
                if (!string.IsNullOrEmpty(item.SignatureUploadId) && item.IsSignOff)

                    ExecuteCommand(new CreateOrUpdateTrainingManualChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, TrainingManualChapterId = item.Id, UploadId = item.SignatureUploadId, isSignOff = item.IsSignOff }); //done  changes by softude
            }
            #endregion

            #region Policy
            foreach (var item in dataModel.PolicyContentModels)
            {
                ExecuteCommand(new CreateOrUpdatePolicyContentBoxUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, PolicyContentBoxId = item.Id, IsChecked = item.IsChecked, IssueDiscription = item.IssueDiscription, IsActionNeeded = item.IsActionNeeded });

                foreach (var upload in item.StandardUserAttachments)
                {
                    if (!dataModel.IsReportView)
                    {
                        ExecuteCommand(new CreateOrUpdatePolicyContentBoxUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, PolicyContentBoxId = item.Id, UploadId = upload.Id });
                    }

                }
                // upload Signature
                if (!string.IsNullOrEmpty(item.SignatureUploadId) && item.IsSignOff)
                {
                    ExecuteCommand(new CreateOrUpdatePolicyContentBoxUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, PolicyContentBoxId = item.Id, UploadId = item.SignatureUploadId, isSignOff = item.IsSignOff });
                }
            }
            #endregion

            #region Checklist
            foreach (var item in dataModel.CLContentModels)
            {
                ExecuteCommand(new CreateOrUpdateCheckListChapterUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, CheckListChapterId = item.Id, IsChecked = item.IsChecked, IssueDiscription = item.IssueDiscription });

                foreach (var upload in item.StandardUserAttachments)
                {
                    if (!dataModel.IsReportView)
                    {
                        ExecuteCommand(new CreateOrUpdateCheckListChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, CheckListChapterId = item.Id, UploadId = upload.Id }); //done  changes by softude 
                    }

                }

                // upload Signature
                if (!string.IsNullOrEmpty(item.SignatureUploadId) && item.IsSignOff)
                    ExecuteCommand(new CreateOrUpdateCheckListChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, CheckListChapterId = item.Id, UploadId = item.SignatureUploadId, isSignOff = item.IsSignOff }); //done  changes by softude 
            }
            #endregion

            #region Memo
            foreach (var item in dataModel.MemoContentModels)
            {
                ExecuteCommand(new CreateOrUpdateMemoChapterUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, MemoChapterId = item.Id, IsChecked = item.IsChecked, IssueDiscription = item.IssueDiscription });

                foreach (var upload in item.StandardUserAttachments)
                {
                    if (!dataModel.IsReportView)
                    {
                        ExecuteCommand(new CreateOrUpdateMemoChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, MemoChapterId = item.Id, UploadId = upload.Id });
                    }

                }
                // upload Signature
                if (!string.IsNullOrEmpty(item.SignatureUploadId) && item.IsSignOff)
                    ExecuteCommand(new CreateOrUpdateMemoChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, MemoChapterId = item.Id, UploadId = item.SignatureUploadId, isSignOff = item.IsSignOff });  //done  changes by softude 
            }
            #endregion

            #region AcrobatField
            foreach (var item in dataModel.AcrobatFieldContentModels)
            {
                ExecuteCommand(new CreateOrUpdateAcrobatFieldChapterUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, AcrobatFieldChapterId = item.Id, IsChecked = item.IsChecked, IssueDiscription = item.IssueDiscription });

                foreach (var upload in item.StandardUserAttachments)
                {
                    ExecuteCommand(new CreateOrUpdateAcrobatFieldChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, AcrobatFieldChapterId = item.Id, UploadId = upload.Id });
                }
                // upload Signature
                if (!string.IsNullOrEmpty(item.SignatureUploadId) && item.IsSignOff)
                    ExecuteCommand(new CreateOrUpdateAcrobatFieldChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, AcrobatFieldChapterId = item.Id, UploadId = item.SignatureUploadId, isSignOff = item.IsSignOff }); //done  changes by softude 

                List<StandardUserAdobeFieldValues> AcroFieldkeyValues = new List<StandardUserAdobeFieldValues>();
                if (item.AcrofieldValue != null)
                {
                    var strArr = item.AcrofieldValue.Split(',');

                    foreach (var ar in strArr)
                    {
                        var keyVal = ar.Split(':');
                        if (keyVal.Length > 1)
                        {
                            var fillData = new StandardUserAdobeFieldValues
                            {
                                AcrobatFieldChapterId = item.Id,
                                DocumentId = model?.DocumentId,
                                Field_Name = (keyVal[0].Replace("{", "").Replace("}", "").Replace("'", "")).Trim(),
                                Field_Value = (keyVal[1].Replace("{", "").Replace("}", "").Replace("'", "")).Trim()
                            };
                            AcroFieldkeyValues.Add(fillData);
                        }
                    }

                    ExecuteCommand(new CreateOrUpdateAcrobatFieldValueCommand { AcrobatFieldList = AcroFieldkeyValues });
                }
            }
            #endregion

            #region Form
            if (dataModel.FormContentModels != null)
            {
                foreach (var formcontentmodel in dataModel.FormContentModels)
                {
                    ExecuteCommand(new CreateOrUpdateFormChapterUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, FormChapterId = formcontentmodel.Id });

                    foreach (var formfielddata in formcontentmodel.FormFields)
                    {
                        ExecuteCommand(new CreateOrUpdateFormFieldUserResultCommand
                        {
                            FormFieldName = formfielddata.FormFieldName,
                            FormFieldDescription = formfielddata.FormFieldDescription,
                            FormChapterId = formcontentmodel.Id,
                            Id = formfielddata.Id,
                            Number = formcontentmodel.Number,
                            FormFiledId = formfielddata.Id,
                            AssignedId = dataModel.AssignedDocumentId
                        });
                    }
                }
            }
            #endregion

            #region Test
            if (dataModel.TestContent.ContentModels != null)
            {
                //Test
                foreach (var item in dataModel.TestContent.ContentModels)
                {
                    ExecuteCommand(new CreateOrUpdateTestChapterUserResultCommand { UserId = userId, AssignedDocumentId = model?.Id, TestChapterId = item.Id, IsChecked = item.IsChecked, IssueDiscription = item.IssueDiscription });

                    foreach (var upload in item.StandardUserAttachments)
                    {
                        if (!dataModel.IsReportView)
                        {
                            ExecuteCommand(new CreateOrUpdateTestChapterUserUploadResultsCommand { UserId = userId, AssignedDocumentId = model?.Id, TestChapterId = item.Id, UploadId = upload.Id });  //done  changes by softude 
                        }

                    }
                }
            }

            if (dataModel.TestContentModels != null)
            {
                foreach (var item in dataModel.TestContentModels)
                {
                    ExecuteCommand(new CustomDocumentAnswerSubmission { StandarduserID = Guid.Parse(userId), TestQuestionID = Guid.Parse(item.Id), documentType = DocumentType.custom, CreatedOn = DateTime.Now, TestSelectedAnswer = item.SelectedAnswer, CustomDocumentID = Guid.Parse(dataModel.Id), Id = Guid.NewGuid() });
                }
            }
            #endregion

            if (dataModel.Approver != null)
            {
                List<string> names = new List<string>();

                Ids = dataModel.Approver.Split(',');

                foreach (var id in Ids)
                {
                    var userDetail = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = id });

                    names.Add(userDetail.UserName);
                }


            }
            // Mail functionality for user when click on submit for review buton   on the basis of status 

            if (Thread.CurrentPrincipal.IsInStandardUserRole() && dataModel.Approver == null && DeclineMessage == "" && status == DocumentUsageStatus.UnderReview)
            {
                string userIds = Convert.ToString(userId);
                string[] userIdsApproverIds = new string[] { userIds };
                sendMail(dataModel.CreatedBy, DeclineMessage, dataModel.Id, userIdsApproverIds, true, false, false, "Submit", dataModel, status);
            }

            // for Admin Approve  Status = Complete

            else
            {
                string approverIds = Convert.ToString(dataModel.ApproverId);
                string[] adminApproverIds = new string[] { approverIds };
                if (status == DocumentUsageStatus.Complete)
                {
                    sendMail(dataModel.CreatedBy, DeclineMessage, dataModel.Id, adminApproverIds, false, true, true, "accept", dataModel, status);
                }
                // for Admin Decline Status = Decline
                else
                {
                    sendMail(dataModel.CreatedBy, DeclineMessage, dataModel.Id, adminApproverIds, false, true, true, "decline", dataModel, status);
                }
            }

            return new JsonResult { Data = "done", JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        public List<acroField> fetchAcroFields(string id, string companyId = null)
        {
            List<acroField> fields = new List<acroField>();
            try
            {
                var upload = ExecuteQuery<FetchUploadFromCompanyQuery, UploadModel>(new FetchUploadFromCompanyQuery
                {
                    Id = id,
                    ExcludeBytes = true,
                    CompanyId = companyId
                });
                if (upload != null)
                {
                    var physicalPath = CreateOrGetCachedUpload(upload);
                    PdfReader pdfReader = new PdfReader(physicalPath);

                    foreach (var de in pdfReader.AcroFields.Fields)
                        fields.Add(new acroField { fields = de.Key });
                }

            }
            catch (Exception ex)
            {

            }
            // var jsonStr = JsonConvert.SerializeObject(fields);
            return fields;
        }

        private string CreateOrGetCachedUpload(UploadModel upload, bool mainContext = false)
        {
            var fullUpload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter
            {
                Id = upload.Id,
                MainContext = mainContext
            });
            return Create(fullUpload.Data, fullUpload.Id, fullUpload.Name);
        }

        string Create(byte[] data, string uniqueId, string filename)
        {
            var path = Server.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
            Directory.CreateDirectory(Path.Combine(path, uniqueId));
            path = Path.Combine(path, uniqueId, filename.RemoveSpecialCharacters());
            if (!System.IO.File.Exists(path))
            {
                if (data != null)
                    Utility.Convertor.CreateFileFromBytes(data, path);
            }
            return path;
        }

        [System.Web.Http.HttpPost]
        [ValidateInput(false)]
        public object CompleteTrainingMannual(CustomDocumentModel model, DocumentUsageStatus? status = null, bool IsGlobalAccessed = false)
        {
            var userId = Thread.CurrentPrincipal.GetId().ToString();

            var viewDate = DateTime.UtcNow;
            //viewDate = DateTime.SpecifyKind(DateTime.ParseExact(viewDate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc); // remove milliseconds

            ExecuteCommand(new CreateOrUpdateDocumentUsageCommand
            {
                DocumentId = model.Id.ToString(),
                DocumentType = DocumentType.TrainingManual,

                UserId = userId,
                ViewDate = viewDate,
                IsGlobalAccessed = IsGlobalAccessed,
                Status = status
            });

            return null;

        }

        [HttpPost]
        [ValidateInput(false)]
        public object DocumentWorkFlowMessageSave(CustomDocumentModel model, string message, bool creator, bool? approver, bool? admin, string[] approvers, string action)
        {
            var userId = Thread.CurrentPrincipal.GetId();

            if (Thread.CurrentPrincipal.IsInGlobalAdminRole())
            {
                approver = false;
                creator = false;
                admin = true;
            }

            if (creator)
            {
                model.CreatedBy = userId.ToString();
            }

            if (admin != true && message != "")
            {
                var cs = new SaveOrUpdateDocumentWorkflowAuditMessagesCommand
                {
                    DocumentId = model.Id == null ? null : model.Id,
                    CreatorId = model.CreatedBy == "null" ? userId : Guid.Parse(model.CreatedBy),
                    ApproverId = creator ? Guid.Empty : userId,
                    Message = message
                };

                ExecuteCommand(cs);
            }
            if (approvers != null && message != "")
            {
                sendMail(model.CreatedBy, message, model.Id, approvers, creator, approver, admin, action, model,null);
            }
            else
            {
                string[] approverIds = model.Approver.Split(',');
                sendMail(model.CreatedBy, message, model.Id, approverIds, creator, approver, admin, action, model,null);
            }


            return null;
        }

        private void sendMail(string creatorId, string message, string documentId, string[] approverIds, bool? isCreator, bool? isApprover, bool? isAdmin, string action, CustomDocumentModel model, DocumentUsageStatus? status = null)
        {
            string subject = null;
            //if admin is delining or accepting document
            if (isAdmin == true)
            {
                if (status == DocumentUsageStatus.Complete)
                {
                    //send notification to all approver and content creator
                    message = "A document has been " + action + " by the global Administrator";

                    subject = "A document has been approved";
                    addNotification(documentId, creatorId, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());


                    //send notification to all approvaers
                    foreach (var a in approverIds)
                    {
                        mail(a, documentId, message, subject, model,status);

                        addNotification(documentId, a, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
                    }

                    //send mail to creator
                   // mail(creatorId, documentId, message, subject, model,status);
                }
                else 
                {
                    //send notification to all approver and content creator
                    message = "A document has been " + action + " by the global Administrator";

                    subject = "A document has been Decline";
                    addNotification(documentId, creatorId, "A document has been Declined", DocumentNotificationType.Declined.GetDescription());


                    //send notification to all approvaers
                    foreach (var a in approverIds)
                    {
                        mail(a, documentId, message, subject, model,status);

                        addNotification(documentId, a, "A document has been Declined", DocumentNotificationType.Declined.GetDescription());
                    }

                    //send mail to creator
                   // mail(creatorId, documentId, message, subject, model);
                }
            }

            //if creator submits document for approval
            else if (isCreator == true)
            {

                subject = "A document has been submitted for approval";
                //send notification to all approvers
                foreach (var a in approverIds)
                {
                    mail(a, documentId, message, subject, model,status);
                    addNotification(documentId, a, "A document has been submitted for approval", DocumentNotificationType.Assign.GetDescription());
                }

            }
            //if approver accepts or declines document
            else if (isApprover == true)
            {

                if (action == "decline")
                {
              
                    subject = "A document has been declined";
                    addNotification(documentId, creatorId, "A document has been declined", DocumentNotificationType.Declined.GetDescription());
                }
                else if (action == "accept")
                {
                    //mail(creatorId, documentId, message, subject, model, status);
                    subject = "Document Approval";
                    addNotification(documentId, creatorId, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
                }

                //send notification to all approvaers
                foreach (var a in approverIds)
                {
                   // mail(a, documentId, message, subject, model);
                    if (action == "decline")
                    {
                        addNotification(documentId, a, "A document has been declined", DocumentNotificationType.Declined.GetDescription());
                    }
                    else if (action == "accept")
                    {
                        addNotification(documentId, a, "A document has been approved", DocumentNotificationType.Accepted.GetDescription());
                    }
                }

                //send mail to creator
                mail(creatorId, documentId, message, subject, model);

            }
        }

        private void mail(string userId, string documentId, string message, string subject, CustomDocumentModel model,DocumentUsageStatus? status = null)
        {
            var approver = new UserViewModel();
            var creator = new UserViewModel();
            var getUserEmailWithUserId= new StandardUser();
            var getUserIdWithAssignedDocument = new AssignedDocument();
            var userName= "";
            // Get User Id on the basis of AssignedDocumentId
            if (model.AssignedDocumentId != null)
            {
                getUserIdWithAssignedDocument = _assignedDocumentRepository.List.Where(x => x.Id == model.AssignedDocumentId && x.Deleted == false).FirstOrDefault();
                // Get Email and UserFullName 
                getUserEmailWithUserId = _userRepository.List.Where(x => x.Id == Guid.Parse(getUserIdWithAssignedDocument.UserId)).FirstOrDefault();
                userName = getUserEmailWithUserId.FirstName + " " + getUserEmailWithUserId.LastName;
            }
            else
            {
                if(!string.IsNullOrEmpty(message) && status != null)
                    getUserEmailWithUserId = _userRepository.List.Where(x => x.Id == Guid.Parse(model.Approver)).FirstOrDefault();
                else
                    getUserEmailWithUserId = _userRepository.List.Where(x => x.Id == Guid.Parse(userId)).FirstOrDefault();

                userName = getUserEmailWithUserId.FirstName + " " + getUserEmailWithUserId.LastName;
            }

            var userQueryParameter = new UserQueryParameter
            {
                UserId = Guid.Parse(userId)
            };
            var user = ExecuteQuery<UserQueryParameter, UserViewModel>(userQueryParameter);
            var name = user.FirstName + " " + user.LastName;
            var author = ExecuteQuery<UserQueryParameter, UserViewModel>(new UserQueryParameter
            {
                UserId = Guid.Parse(model.CreatedBy)
            });

            var documentTitles = new List<DocumentTitlesAndTypeQuery>();
            documentTitles.Add(new DocumentTitlesAndTypeQuery
            {
                DocumentTitle = model.Title,
                AdditionalMsg = model.Description,
                DocumentType = model.DocumentType,
                DocumentId = model.Id,
                Author = author.FirstName + " " + author.LastName,
                Points = model.Points,
                Passmark = model.PassMarks
            });

            //Sending mail to Admin 
            if (status == DocumentUsageStatus.UnderReview)
            {
                var eventPublisher = new EventPublisher();
                eventPublisher.Publish(new DocumentWorkflowEvent
                {
                    UserViewModel = new UserViewModel() { FirstName = author.FullName, EmailAddress = author.EmailAddress },
                    CompanyViewModel = PortalContext.Current.UserCompany,
                    DocumentTitles = documentTitles,
                    Subject = subject,
                    Message = message
                });
            }
            // Sending mail to User
            else
            {
                var eventPublisher = new EventPublisher();
                eventPublisher.Publish(new DocumentWorkflowEvent
                {
                    UserViewModel = new UserViewModel() { FirstName = userName, EmailAddress = getUserEmailWithUserId.EmailAddress },
                    CompanyViewModel = PortalContext.Current.UserCompany,
                    DocumentTitles = documentTitles,
                    Subject = subject,
                    Message = message
                });
            }
        }

        private void addNotification(string DocumentId, string UserId, string AdditionalMsg, string notiType)
        {
            var documentNotifications = new List<DocumentNotificationViewModel>();


            var notificationModel = new DocumentNotificationViewModel
            {
                AssignedDate = DateTime.Now,
                IsViewed = false,
                DocId = DocumentId,
                UserId = UserId,
                NotificationType = notiType,
                Message = AdditionalMsg
            };


            documentNotifications.Add(notificationModel);

            ExecuteCommand(documentNotifications);
        }
    }
}