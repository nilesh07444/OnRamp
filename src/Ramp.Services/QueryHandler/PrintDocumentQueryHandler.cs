using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using com.sun.org.apache.bcel.@internal.generic;
using Common;
using Common.Data;
using Common.Query;
using Data.EF;
using Data.EF.Customer;
using Data.EF.Migrations;
using Domain.Customer.Models;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Document;
using Domain.Customer.Models.Forms;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.Policy;
using Domain.Customer.Models.Test;
using Domain.Customer.Models.TrainingManual;
using Domain.Enums;
using Domain.Models;
using iTextSharp.text;
using javax.xml.crypto;
using NReco.VideoConverter;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.Memo;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.Query.Test;
using Ramp.Contracts.Query.TrainingManual;
using Ramp.Contracts.QueryParameter.Upload;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using VirtuaCon;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;

namespace Ramp.Services.QueryHandler
{
    public class PrintDocumentQueryHandler<TEntity> :
        ReportingQueryHandler<PrintDocumentQuery<TEntity>>
        where TEntity : class, IDocument
    {
        private readonly IRepository<TEntity> _documentRepository;
        private readonly IQueryExecutor _executor;

        public PrintDocumentQueryHandler(

        IRepository<TEntity> documentRepository)
        {

            _documentRepository = documentRepository;
            _executor = DependencyResolver.Current.GetService<IQueryExecutor>();

        }

        public override void BuildReport(ReportDocument document, out string title, out string recepitent, PrintDocumentQuery<TEntity> data)
        {
            var entity = _documentRepository.Find(data.Id);
            title = entity.Title;
            recepitent = string.Empty;
            document.AddStyle(new PaddingElementStyle(5, 5, 5, 5));
            CreateCoverPage(document, entity, data.PortalContext);
            if (typeof(TEntity) == typeof(TrainingManual))
            {
                var trainingManual = entity as TrainingManual;
                CreateChapters(document, trainingManual.Chapters.Where(x => !x.Deleted).OrderBy(x => x.Number),
                    trainingManual.Chapters.Min(x => x.Number) == 0 ? 1 : 0);
            }
            else if (typeof(TEntity) == typeof(Test))
            {
                var test = entity as Test;
                CreateIntroductionSection(document, test);
                CreateQuestionSection(document, test,null,null);
            }
            else if (typeof(TEntity) == typeof(Memo))
            {
                var memo = entity as Memo;
                CreateContentBoxes(document, memo.ContentBoxes.Where(x => !x.Deleted).OrderBy(c => c.Number).Cast<IContentBox>().ToList());
            }
            else if (typeof(TEntity) == typeof(Policy))
            {
                var policy = entity as Policy;
                CreateContentBoxes(document, policy.ContentBoxes.Where(x => !x.Deleted).OrderBy(c => c.Number).Cast<IContentBox>().ToList());
            }
            else if (typeof(TEntity) == typeof(CheckList))
            {

                var checkList = entity as CheckList;
                CreateChecklistChapters(document, checkList.Chapters.Where(x => !x.Deleted).OrderBy(x => x.Number),
                    checkList.Chapters.Min(x => x.Number) == 0 ? 1 : 0);
            }
            else if (typeof(TEntity) == typeof(Domain.Customer.Models.CustomDocument))
            {

                var customDocumentList = entity as Domain.Customer.Models.CustomDocument;
                CreateCustomDocumentlistChapters(document, customDocumentList, 1, data.userId);
            }
        }
        private void CreateCustomDocumentlistChapters(ReportDocument document, Domain.Customer.Models.CustomDocument query, int chapterNumberOffset, string userId)
        {
           
            var customDocumentList = _executor.Execute<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery
            {
                Id = query.Id,
            });
            var Memomanual = query.Id == null ? null : _executor.Execute<FetchByCustomDocumentIdQuery, Memo>(new FetchByCustomDocumentIdQuery { Id = query.Id, }); ;
            var Trainingmanual = query.Id == null ? null : _executor.Execute<FetchByCustomDocumentIdQuery, TrainingManual>(new FetchByCustomDocumentIdQuery { Id = query.Id, });
            var PolicyData = query.Id == null ? null : _executor.Execute<FetchByCustomDocumentIdQuery, Policy>(new FetchByCustomDocumentIdQuery { Id = query.Id, });
            var TestData = query.Id == null ? null : _executor.Execute<FetchByCustomDocumentIdQuery, Test>(new FetchByCustomDocumentIdQuery { Id = query.Id, });
            var checklistData = query.Id == null ? null : _executor.Execute<FetchByCustomDocumentIdQuery, CheckList>(new FetchByCustomDocumentIdQuery { Id = query.Id, });

            var assignedDocument = _executor.Execute<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = userId, DocumentId = query.Id });

            #region ["Training Manual"]
            if (Trainingmanual != null)
            {
                if (assignedDocument != null)
                    CreateCustomTrainingChapters(document, customDocumentList.TMContentModels, assignedDocument, userId);
                else
                    CreateChapters(document, Trainingmanual.Chapters.Where(x => !x.Deleted).OrderBy(x => x.Number),
                      Trainingmanual.Chapters.Min(x => x.Number) == 0 ? 1 : 0);
            }
            #endregion

            #region ["Memo"]
            if (Memomanual != null)
            {
                 if (assignedDocument != null)
                    CreateCustomMemoChapters(document, customDocumentList.MemoContentModels, assignedDocument, userId);
                else
                    CreateContentBoxes(document, Memomanual.ContentBoxes.Where(x => !x.Deleted).OrderBy(c => c.Number).Cast<IContentBox>().ToList());
            }
            #endregion

            #region ["Policy"]
            if (PolicyData != null)
            { if (assignedDocument != null)
                    CreateCustomPolicyChapters(document, customDocumentList.PolicyContentModels, assignedDocument, userId);
                else
                CreateContentBoxes(document, PolicyData.ContentBoxes.Where(x => !x.Deleted).OrderBy(c => c.Number).Cast<IContentBox>().ToList());
            }
            #endregion

            #region ["Form"]
            if (customDocumentList.FormContentModels.Any())
            {
                CreateCustomFormChapters(document, customDocumentList.FormContentModels.Where(x => !x.Deleted).OrderBy(x => x.Number));
            }
            #endregion

            #region ["CheckList/Activity Book"]
            if (checklistData != null)
            { 
                if (assignedDocument != null)
                    CreateCustomChecklistChapters(document, customDocumentList.CLContentModels, assignedDocument, userId);
                else
                CreateChecklistChapters(document, checklistData.Chapters.Where(x => !x.Deleted).OrderBy(x => x.Number),
                   checklistData.Chapters.Min(x => x.Number) == 0 ? 1 : 0);
            }
            #endregion

            #region ["Test"]
            if (TestData != null)
            {
                CreateQuestionSection(document, TestData, assignedDocument, userId);
            }
            #endregion
        }
        private void CreateCustomTrainingChapters(ReportDocument document, IEnumerable<TrainingManualChapterModel> chapters, AssignedDocumentModel assignedDocument,string userId)
        {
            var index = 1;
            foreach (var chapter in chapters.ToList())
            {
                var section = CreateSection("", PageOrientation.Portrait, false);

                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();
                chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                    $"{index++}.{chapter.Title}",
                    new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));
                contentTable.AddElement(chapterHeadingRow);
                if (chapter.Content != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(chapter.Content)));
                    contentTable.AddElement(contentImageRow);
                }

                section.AddElement(contentTable);
                document.AddElement(section);

                if (assignedDocument != null)
                {

                    var TrainingManualChapterUserResult = _executor.Execute<TrainingManualChapterUserResultQuery, TrainingManualChapterUserResultViewModel>(new TrainingManualChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TrainingManualChapterId = chapter.Id });
                    if (TrainingManualChapterUserResult != null)
                    {
                        if (!string.IsNullOrEmpty(TrainingManualChapterUserResult.IssueDiscription))
                        {
                            var additionalContentTable = new GridBlock();
                            additionalContentTable.ColumnWidths.Add(100);
                            var contentAdditionalRow = new GridRowBlock();
                            section.AddElement(CreateParagraph($"Additional notes :",
                             new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                             new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            contentAdditionalRow.AddElement(new GridCellBlock(CreateChapterContentBlock(TrainingManualChapterUserResult.IssueDiscription)));
                            additionalContentTable.AddElement(contentAdditionalRow);
                            section.AddElement(additionalContentTable);
                            // document.AddElement(section);
                        }
                    }

                    // To Display Standeruser Attachment and Additional Notes
                    var TrainingManualChapterUserUpload = _executor.Execute<TrainingManualChapterUserResultQuery, List<UploadResultViewModel>>(new TrainingManualChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TrainingManualChapterId = chapter.Id, UserId = userId });

                    chapter.StandardUserAttachments = TrainingManualChapterUserUpload.Where(z => !z.isSignOff).ToList();


                    var attachments = GetAttachments(chapter.StandardUserAttachments).OrderBy(x => x.Reference);
                    section = CreateSection("", PageOrientation.Portrait, false);
                    if (attachments.Any(x => x.Thumbnail != null))
                    {
                        section.AddElement(CreateParagraph($"Resources:",
                            new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                            new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                        CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                    }
                    document.AddElement(section);

                    var SigntureData = TrainingManualChapterUserUpload.Where(z => z.isSignOff).ToList();
                    if (SigntureData != null)
                    {
                        var signatureAttachments = GetAttachments(SigntureData).OrderBy(x => x.Reference);
                        section = CreateSection("", PageOrientation.Portrait, false);
                        if (signatureAttachments.Any(x => x.Thumbnail != null))
                        {
                            section.AddElement(CreateParagraph($"Signature :",
                                new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                                new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            CreateAttachmentsBlock(signatureAttachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                        }
                    }
                    document.AddElement(section);

                }


                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }
        private void CreateCustomTestChapters(ReportDocument document, IEnumerable<TestQuestionModel> test)
        {
            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Questions", headingFont, Centered));

            var questions = test.Where(x => !x.Deleted).OrderBy(q => q.Number).ToList();
            var index = 1;
            foreach (var question in questions)
            {
                question.Number = index++;
                //   section.AddElement(CreateQuestionAnswerBlock(question, test.HighlightAnswersOnSummary));

                var attachments = GetAttachments(question.Attachments).OrderBy(x => x.Reference);

                if (attachments.Any(x => x.Thumbnail != null))
                {
                    section.AddElement(CreateParagraph($"Resources:",
                        new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                        new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                    CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                }

                section.AddElement(CreateCenteredHorizontalRule());
            }

            document.AddElement(section);
        }
        private void CreateCustomFormChapters(ReportDocument document, IEnumerable<FormChapterModel> chapters)
        {
            var index = 1;
            foreach (var chapter in chapters.ToList())
            {
                var section = CreateSection("", PageOrientation.Portrait, false);

                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();
                chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                    $"{index++}.{chapter.Title}",
                    new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));
                contentTable.AddElement(chapterHeadingRow);
                if (chapter.Content != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(chapter.Content)));
                    contentTable.AddElement(contentImageRow);
                }
                section.AddElement(contentTable);
                var contentTablefield = new GridBlock();
                contentTablefield.ColumnWidths.Add(100);
                contentTablefield.ColumnWidths.Add(100);
                foreach (var frm in chapter.FormFields)
                {
                    var contentfrmRow = new GridRowBlock();
                    contentfrmRow.AddElement(new GridCellBlock(CreateChapterContentBlock(frm.FormFieldName)));
                    contentfrmRow.AddElement(new GridCellBlock(CreateChapterContentBlock(!string.IsNullOrEmpty(frm.FormFieldDescription)? frm.FormFieldDescription:string.Empty)));
                    contentTablefield.AddElement(contentfrmRow);
                }
                section.AddElement(contentTablefield);
                document.AddElement(section);

          


                document.AddElement(section);
                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }
        private void CreateCustomPolicyChapters(ReportDocument document, IEnumerable<PolicyContentBoxModel> contentBoxes, AssignedDocumentModel assignedDocument, string userId)
        {
            var index = 1;
            foreach (var contentBox in contentBoxes)
            {
                var section = CreateSection("", PageOrientation.Portrait, false);

                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();
                chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                    $"{index++}.{contentBox.Title}",
                    new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));
                contentTable.AddElement(chapterHeadingRow);
                if (contentBox.Content != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(contentBox.Content)));
                    contentTable.AddElement(contentImageRow);
                }

                section.AddElement(contentTable);
                document.AddElement(section);

                section = CreateSection("", PageOrientation.Portrait, false);
                document.AddElement(section);
                if (assignedDocument != null)
                {

                    var PolicyContentBoxUserResult =_executor.Execute <PolicyContentBoxUserResultQuery, PolicyContentBoxUserResultViewModel>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = contentBox.Id });
                    if (PolicyContentBoxUserResult != null)
                    {
                        if (!string.IsNullOrEmpty(PolicyContentBoxUserResult.IssueDiscription))
                        {
                            var additionalContentTable = new GridBlock();
                            additionalContentTable.ColumnWidths.Add(100);
                            var contentAdditionalRow = new GridRowBlock();
                            section.AddElement(CreateParagraph($"Additional notes :",
                             new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                             new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            contentAdditionalRow.AddElement(new GridCellBlock(CreateChapterContentBlock(PolicyContentBoxUserResult.IssueDiscription)));
                            additionalContentTable.AddElement(contentAdditionalRow);
                            section.AddElement(additionalContentTable);
                            // document.AddElement(section);
                        }
                    }

                    // To Display Standeruser Attachment and Additional Notes
                    var PolicyContentBoxUserUpload = _executor.Execute <PolicyContentBoxUserResultQuery, List<UploadResultViewModel>>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = contentBox.Id, UserId = userId });

                    contentBox.StandardUserAttachments = PolicyContentBoxUserUpload.Where(z => !z.isSignOff).ToList();


                    var attachments = GetAttachments(contentBox.StandardUserAttachments).OrderBy(x => x.Reference);
                    section = CreateSection("", PageOrientation.Portrait, false);
                    if (attachments.Any(x => x.Thumbnail != null))
                    {
                        section.AddElement(CreateParagraph($"Resources:",
                            new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                            new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                        CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                    }
                    document.AddElement(section);

                    var SigntureData =  PolicyContentBoxUserUpload.Where(z => z.isSignOff).ToList();
                    if (SigntureData != null)
                    {
                        var signatureAttachments = GetAttachments(SigntureData).OrderBy(x => x.Reference);
                        section = CreateSection("", PageOrientation.Portrait, false);
                        if (signatureAttachments.Any(x => x.Thumbnail != null))
                        {
                            section.AddElement(CreateParagraph($"Signature :",
                                new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                                new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            CreateAttachmentsBlock(signatureAttachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                        }
                    }
                    document.AddElement(section);

                }
                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }
        private void CreateCustomMemoChapters(ReportDocument document, IEnumerable<MemoContentBoxModel> chapters, AssignedDocumentModel assignedDocument, string userId)
        {
            var index = 1;
            foreach (var chapter in chapters.ToList())
            {
                var section = CreateSection("", PageOrientation.Portrait, false);

                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();
                chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                    $"{index++}.{chapter.Title}",
                    new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));
                contentTable.AddElement(chapterHeadingRow);
                if (chapter.Content != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(chapter.Content)));
                    contentTable.AddElement(contentImageRow);
                }

                section.AddElement(contentTable);
                document.AddElement(section);

                section = CreateSection("", PageOrientation.Portrait, false);
                document.AddElement(section);
                if (assignedDocument != null)
                {

                    var MemoChapterUserResult =  _executor.Execute<MemoChapterUserResultQuery, MemoChapterUserResultViewModel>(new MemoChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, MemoChapterId = chapter.Id, UserId = userId });
                    if (MemoChapterUserResult != null)
                    {
                        if (!string.IsNullOrEmpty(MemoChapterUserResult.IssueDiscription))
                        {
                            var additionalContentTable = new GridBlock();
                            additionalContentTable.ColumnWidths.Add(100);
                            var contentAdditionalRow = new GridRowBlock();
                            section.AddElement(CreateParagraph($"Additional notes :",
                             new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                             new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            contentAdditionalRow.AddElement(new GridCellBlock(CreateChapterContentBlock(MemoChapterUserResult.IssueDiscription)));
                            additionalContentTable.AddElement(contentAdditionalRow);
                            section.AddElement(additionalContentTable);
                            // document.AddElement(section);
                        }
                    }

                    // To Display Standeruser Attachment and Additional Notes
                    var memoChapterUserUpload = _executor.Execute<MemoChapterUserResultQuery, List<UploadResultViewModel>>(new MemoChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, MemoChapterId = chapter.Id, UserId = userId });

                    chapter.StandardUserAttachments = memoChapterUserUpload.Where(z => !z.isSignOff).ToList();


                    var attachments = GetAttachments(chapter.StandardUserAttachments).OrderBy(x => x.Reference);
                    section = CreateSection("", PageOrientation.Portrait, false);
                    if (attachments.Any(x => x.Thumbnail != null))
                    {
                        section.AddElement(CreateParagraph($"Resources:",
                            new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                            new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                        CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                    }
                    document.AddElement(section);

                    var SigntureData =  memoChapterUserUpload.Where(z => z.isSignOff).ToList();
                    if (SigntureData != null)
                    {
                        var signatureAttachments = GetAttachments(SigntureData).OrderBy(x => x.Reference);
                        section = CreateSection("", PageOrientation.Portrait, false);
                        if (signatureAttachments.Any(x => x.Thumbnail != null))
                        {
                            section.AddElement(CreateParagraph($"Signature :",
                                new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                                new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            CreateAttachmentsBlock(signatureAttachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                        }
                    }
                    document.AddElement(section);

                }
                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }

        private void CreateCustomChecklistChapters(ReportDocument document, IEnumerable<CheckListChapterModel> chapters, AssignedDocumentModel assignedDocument, string userId)
        {
            var index = 1;
            foreach (var chapter in chapters.ToList())
            {
                var section = CreateSection("", PageOrientation.Portrait, false);

                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();

                if (chapter != null && chapter.IsChecked)
                {
                    chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                        $"{index++}. {chapter.Title}                                        [ X ]",
                        new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                        new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));

                }
                else
                {
                    chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                    $"{index++}. {chapter.Title}                                             [ ]",
                    new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));
                }

                contentTable.AddElement(chapterHeadingRow);
                if (chapter.Content != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(chapter.Content)));
                    contentTable.AddElement(contentImageRow);
                }

                section.AddElement(contentTable);
                document.AddElement(section);

                section = CreateSection("", PageOrientation.Portrait, false);
                document.AddElement(section);
                if (assignedDocument != null)
                {

                    var CheckListChapterUserResult =  _executor.Execute<CheckListChapterUserResultQuery, CheckListChapterUserResultViewModel>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = chapter.Id, UserId = userId });
                    if ( CheckListChapterUserResult != null)
                    {
                        if (!string.IsNullOrEmpty(CheckListChapterUserResult .IssueDiscription))
                        {
                            var additionalContentTable = new GridBlock();
                            additionalContentTable.ColumnWidths.Add(100);
                            var contentAdditionalRow = new GridRowBlock();
                            section.AddElement(CreateParagraph($"Additional notes :",
                             new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                             new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            contentAdditionalRow.AddElement(new GridCellBlock(CreateChapterContentBlock( CheckListChapterUserResult.IssueDiscription)));
                            additionalContentTable.AddElement(contentAdditionalRow);
                            section.AddElement(additionalContentTable);
                            // document.AddElement(section);
                        }
                    }

                    // To Display Standeruser Attachment and Additional Notes
                    var CheckListChapterUserUpload =  _executor.Execute <CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = chapter.Id, UserId = userId });

                    chapter.StandardUserAttachments = CheckListChapterUserUpload.Where(z => !z.isSignOff).ToList();


                    var attachments = GetAttachments(chapter.StandardUserAttachments).OrderBy(x => x.Reference);
                    section = CreateSection("", PageOrientation.Portrait, false);
                    if (attachments.Any(x => x.Thumbnail != null))
                    {
                        section.AddElement(CreateParagraph($"Resources:",
                            new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                            new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                        CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                    }
                    document.AddElement(section);

                    var SigntureData =  CheckListChapterUserUpload.Where(z => z.isSignOff).ToList();
                    if (SigntureData != null)
                    {
                        var signatureAttachments = GetAttachments(SigntureData).OrderBy(x => x.Reference);
                        section = CreateSection("", PageOrientation.Portrait, false);
                        if (signatureAttachments.Any(x => x.Thumbnail != null))
                        {
                            section.AddElement(CreateParagraph($"Signature :",
                                new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                                new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            CreateAttachmentsBlock(signatureAttachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                        }
                    }
                    document.AddElement(section);

                }

                section = CreateSection("", PageOrientation.Portrait, false);
                //section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }
        private void CreateChecklistChapters(ReportDocument document, IOrderedEnumerable<CheckListChapter> chapters, int chapterNumberOffset)
        {
            var index = 1;
            foreach (var chapter in chapters.ToList())
            {
                var section = CreateSection("", PageOrientation.Portrait, false);

                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();

                if (chapter != null && chapter.IsChecked)
                {
                    chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                        $"{index++}. {chapter.Title}                                        [ X ]",
                        new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                        new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));

                }
                else
                {
                    chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                    $"{index++}. {chapter.Title}                                             [ ]",
                    new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));
                }

                contentTable.AddElement(chapterHeadingRow);
                if (chapter.Content != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(chapter.Content)));
                    contentTable.AddElement(contentImageRow);
                }

                section.AddElement(contentTable);
                document.AddElement(section);

                section = CreateSection("", PageOrientation.Portrait, false);
                document.AddElement(section);
                var attachments = GetAttachments(chapter).OrderBy(x => x.Reference);

                section = CreateSection("", PageOrientation.Portrait, false);
                if (attachments.Any(x => x.Thumbnail != null))
                {
                    section.AddElement(CreateParagraph($"Resources:",
                        new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                        new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                    CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                }

                document.AddElement(section);
                section = CreateSection("", PageOrientation.Portrait, false);
                //section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }

        private void CreateContentBoxes(ReportDocument document, IEnumerable<IContentBox> contentBoxes)
        {
            var index = 1;
            foreach (var contentBox in contentBoxes)
            {
                var section = CreateSection("", PageOrientation.Portrait, false);

                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();
                chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                    $"{index++}.{contentBox.Title}",
                    new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));
                contentTable.AddElement(chapterHeadingRow);
                if (contentBox.Content != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(contentBox.Content)));
                    contentTable.AddElement(contentImageRow);
                }

                section.AddElement(contentTable);
                document.AddElement(section);

                section = CreateSection("", PageOrientation.Portrait, false);
                document.AddElement(section);
                var attachments = GetAttachments(contentBox).OrderBy(x => x.Reference);

                section = CreateSection("", PageOrientation.Portrait, false);
                if (attachments.Any(x => x.Thumbnail != null))
                {
                    section.AddElement(CreateParagraph($"Resources:",
                        new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                        new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                    CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                }

                document.AddElement(section);
                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }

        private void CreateQuestionSection(ReportDocument document, Test test, AssignedDocumentModel assignedDocument, string userId)
        {
            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Questions", headingFont, Centered));

            var questions = test.Questions.Where(x => !x.Deleted).OrderBy(q => q.Number).ToList();
            var index = 1;
            foreach (var question in questions)
            {
                question.Number = index++;
                section.AddElement(CreateQuestionAnswerBlock(question, test.HighlightAnswersOnSummary));

                if (assignedDocument != null)
                {
                    var TestChapterUserResult = _executor.Execute<TestChapterUserResultQuery, TestChapterUserResultViewModel>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = question.Id, UserId = userId });
                    if (TestChapterUserResult != null)
                    {
                        if (!string.IsNullOrEmpty(TestChapterUserResult.IssueDiscription))
                        {
                            var additionalContentTable = new GridBlock();
                            additionalContentTable.ColumnWidths.Add(100);
                            var contentAdditionalRow = new GridRowBlock();
                            section.AddElement(CreateParagraph($"Additional notes :",
                             new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                             new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            contentAdditionalRow.AddElement(new GridCellBlock(CreateChapterContentBlock(TestChapterUserResult.IssueDiscription)));
                            additionalContentTable.AddElement(contentAdditionalRow);
                            section.AddElement(additionalContentTable);
                            // document.AddElement(section);
                        }
                    }

                    // To Display Standeruser Attachment and Additional Notes
                    var TestChapterUserUpload = _executor.Execute<TestChapterUserResultQuery, List<UploadResultViewModel>>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = question.Id, UserId = userId });

                    //question.StandardUserAttachments = TestChapterUserUpload.Where(z => !z.isSignOff).ToList();


                    //var attachments = GetAttachments(chapter.StandardUserAttachments).OrderBy(x => x.Reference);
                    //section = CreateSection("", PageOrientation.Portrait, false);
                    //if (attachments.Any(x => x.Thumbnail != null))
                    //{
                    //    section.AddElement(CreateParagraph($"Resources:",
                    //        new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    //        new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                    //    CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                    //}
                    //document.AddElement(section);

                    var SigntureData = TestChapterUserUpload .Where(z => z.isSignOff).ToList();
                    if (SigntureData != null)
                    {
                        var signatureAttachments = GetAttachments(SigntureData).OrderBy(x => x.Reference);
                        section = CreateSection("", PageOrientation.Portrait, false);
                        if (signatureAttachments.Any(x => x.Thumbnail != null))
                        {
                            section.AddElement(CreateParagraph($"Signature :",
                                new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                                new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                            CreateAttachmentsBlock(signatureAttachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                        }
                    }
                    document.AddElement(section);
                }
                else
                {
                    var attachments = GetAttachments(question).OrderBy(x => x.Reference);

                    if (attachments.Any(x => x.Thumbnail != null))
                    {
                        section.AddElement(CreateParagraph($"Resources:",
                            new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                            new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                        CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                    }
                }
                section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }

         
        }

        private GridBlock CreateQuestionAnswerBlock(TestQuestion question, bool? highlightAnswersOnSummary)
        {
            var block = new GridBlock();

            block.ColumnWidths.AddRange(new[] { 5, 80 });
            CreateTableDataRowWithStyles(block, new[] { new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Bold)) },
                question.Number, question.Question);

            var answers = question.Answers.Where(x => !x.Deleted).OrderBy(a => a.Number).ToList();
            var index = 0;
            foreach (var answer in answers)
            {
                var answerRow = CreateTableDataRowWithStyles(block, new[] { largeFont }, $"({index++})",
                    $"       {answer.Option?.Trim() ?? ""}");
                if (highlightAnswersOnSummary.HasValue && highlightAnswersOnSummary.Value && question.CorrectAnswerId == answer.Id)
                {
                    answerRow.LastElement().AddStyle(new BackgroundColorElementStyle(Color.Yellow));
                }
            }

            return block;
        }

        private void CreateIntroductionSection(ReportDocument document, Test test)
        {
            var section = CreateSection("", PageOrientation.Portrait);
            section.AddElement(CreateParagraph("Introduction", headingFont, Centered));
            section.AddElement(CreateParagraph(test.IntroductionContent,
                new HorizontalAlignmentElementStyle(HorizontalAlignment.Center)));
            section.AddElement(CreateCenteredHorizontalRule());

            document.AddElement(section);
        }

        private void CreateCoverPage(ReportDocument document, IDocument entity, PortalContextViewModel portalContext)
        {
            var section = CreateSection("", PageOrientation.Portrait, false);
            section.AddElement(CreateCompanyLogo(portalContext));
            section.AddElement(AddBreak());
            section.AddElement(CreateParagraph(entity.Title,
                new VerticalAlignmentElementStyle(VerticalAlignment.Center),
                new HorizontalAlignmentElementStyle(HorizontalAlignment.Center),
                new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold))));
            section.AddElement(AddBreak(20));
            section.AddElement(CreateParagraph(entity.Description,
                new VerticalAlignmentElementStyle(VerticalAlignment.Center),
                new HorizontalAlignmentElementStyle(HorizontalAlignment.Center),
                new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Regular))));

            document.AddElement(section);

            if (entity.CoverPicture != null)
            {
                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(AddBreak());
                var pic = GetImage(entity.CoverPicture.Data, 800, 400);
                if (pic != null)
                {
                    pic.AddStyle(new VerticalAlignmentElementStyle(VerticalAlignment.Bottom));
                    pic.AddStyle(new HorizontalAlignmentElementStyle(HorizontalAlignment.Center));
                    section.AddElement(pic);
                }

                document.AddElement(section);
            }

            document.AddElement(CreateSection("", PageOrientation.Portrait));
        }


        private void CreateChapters(ReportDocument document, IOrderedEnumerable<TrainingManualChapter> chapters, int chapterNumberOffset)
        {
            var index = 1;
            foreach (var chapter in chapters.ToList())
            {
                var section = CreateSection("", PageOrientation.Portrait, false);

                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();
                chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph(
                    $"{index++}.{chapter.Title}",
                    new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                    new FontElementStyle(new System.Drawing.Font(massiveFont.Font, FontStyle.Bold)))));
                contentTable.AddElement(chapterHeadingRow);
                if (chapter.Content != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(chapter.Content)));
                    contentTable.AddElement(contentImageRow);
                }

                section.AddElement(contentTable);
                document.AddElement(section);

                section = CreateSection("", PageOrientation.Portrait, false);
                document.AddElement(section);
                var attachments = GetAttachments(chapter).OrderBy(x => x.Reference);

                section = CreateSection("", PageOrientation.Portrait, false);
                if (attachments.Any(x => x.Thumbnail != null))
                {
                    section.AddElement(CreateParagraph($"Resources:",
                        new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                        new FontElementStyle(new System.Drawing.Font(largeFont.Font, FontStyle.Underline))));
                    CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(block => section.AddElement(block));
                }

                document.AddElement(section);
                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }

        private HTMLBlock CreateChapterContentBlock(string chapterContentHtml)
        {
            return GetImageFromHtml(GetChapterContent(chapterContentHtml), 1000,
                new HorizontalAlignmentElementStyle(HorizontalAlignment.Center), massiveFont);
        }

        private string GetChapterContent(string html)
        {
            var basePath = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}";
            var result = html;
            var listOfImages = GetAllByTag("img", html);
            var listOfIFrames = GetAllByTag("iframe", html);
            var listOfAnchors = GetAllByTag("a", html);

            listOfImages.ForEach(tag =>
            {
                var srcSubstring = tag.Substring(tag.IndexOf("src="));
                var src = srcSubstring.Substring(0, srcSubstring.IndexOf("\" ") + 1);
                if (srcSubstring.IndexOf("\" ") + 1 > 0)
                {
                    var urlToReplace = string.Empty;
                    var found = false;
                    if (src.IndexOf("/Upload/") - 6 > 6)
                        urlToReplace = src.Substring(6, src.IndexOf("/Upload/") - 6);
                    if (string.IsNullOrWhiteSpace(urlToReplace))
                    {
                        var uploadPath = GetUpload(src, basePath, out found);
                        if (found)
                            result = result.Replace(tag, tag.Replace(src, uploadPath));
                        else
                            result = result.Replace(tag, string.Empty);
                    }
                    else
                    {
                        var uploadPath = GetUpload(tag.Replace(urlToReplace, basePath), basePath, out found);
                        if (found)
                            result = result.Replace(tag, uploadPath);
                        else
                            result = result.Replace(tag, string.Empty);
                    }
                }
            });
            listOfIFrames.ForEach(tag =>
            {
                var url = string.Empty;
                var srcIndex = tag.IndexOf("src");
                if (srcIndex > -1)
                {
                    url = tag.Substring(tag.IndexOf("src=\""));
                    if (url.IndexOf("\"", 5) > -1)
                        url = url.Substring(5, url.IndexOf("\"", 5) - 5);
                }

                if (!string.IsNullOrWhiteSpace(url))
                    result = result.Replace(tag, $"<br><a>Video link : {url}</a><br>");
            });
            listOfAnchors.ForEach(tag =>
            {
                var href = string.Empty;
                var hrefIndex = tag.IndexOf("href");
                if (hrefIndex > 0)
                {
                    href = tag.Substring(hrefIndex);
                    if (href.IndexOf("\"", 6) > -1)
                        href = href.Substring(6, href.IndexOf("\"", 6) - 6);
                }

                if (!string.IsNullOrWhiteSpace(href))
                    result = result.Replace(tag, $"{tag}({href})");
            });
            return result.Replace("<hr />", "<br>");
        }

        private string GetUpload(string src, string basePath, out bool found)
        {
            var path = string.Empty;
            Guid? id = null;
            string url = string.Empty;
            if (src.Contains("Upload/"))
            {
                url = src.Substring(src.IndexOf("src=\"")).Replace("src=\"", "");
                url = url.Substring(1, url.IndexOf("\""));
                if (src.Contains("Upload/GetFromCompany/"))
                {
                    var uploadId = src.Substring(src.IndexOf("Upload/GetFromCompany/"));
                    uploadId = uploadId.Replace("Upload/GetFromCompany/", "");
                    uploadId = uploadId.Substring(0, 36);
                    id = uploadId.ConvertToGuid();
                }
                else if (src.Contains("Upload/GetThumbnailFromCompany/"))
                {
                    var uploadId = src.Substring(src.IndexOf("Upload/GetThumbnailFromCompany/"));
                    uploadId = uploadId.Replace("Upload/GetThumbnailFromCompany/", "");
                    uploadId = uploadId.Substring(0, 36);
                    id = uploadId.ConvertToGuid();
                }
                else if (src.Contains("Upload/GetThumbnail/"))
                {
                    var uploadId = src.Substring(src.IndexOf("Upload/GetThumbnail/"));
                    uploadId = uploadId.Replace("Upload/GetThumbnail/", "");
                    uploadId = uploadId.Substring(0, 36);
                    id = uploadId.ConvertToGuid();
                }
            }

            if (id.HasValue)
            {
                var upload = _executor.Execute<FetchUploadQueryParameter, UploadModel>(
                    new FetchUploadQueryParameter
                    {
                        Id = id.Value.ToString(),
                        ExcludeBytes = false
                    });
                if (upload != null)
                {
                    var physicalPath = Create(upload.Data, id.Value.ToString(), upload.Name);
                    path = $"{physicalPath}\"";
                }
            }

            found = !string.IsNullOrWhiteSpace(path);
            return found ? src.Replace(url, path) : src;
        }

        private string Create(byte[] data, string uniqueId, string filename)
        {
            var path = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["DocumentUploadFilePath"]);
            Directory.CreateDirectory(Path.Combine(path, uniqueId));
            path = Path.Combine(path, uniqueId, filename.RemoveSpecialCharacters());
            if (!File.Exists(path) && data != null)
            {
                Utility.Convertor.CreateFileFromBytes(data, path);
            }

            return path;
        }

        //private IEnumerable<AttachmentModel> GetAttachments(TrainingManual trainingManual, bool data = false)
        //{
        //    var attachments = new List<AttachmentModel>();
        //    trainingManual.Chapters.OrderBy(x => x.Number).ToList().ForEach(chapter =>
        //    {
        //        attachments.AddRange(GetAttachments(chapter, data));
        //    });
        //    return attachments;
        //}

        private IEnumerable<AttachmentModel> GetAttachments(IContentUploads content, bool data = false)
        {
            var attachments = new List<AttachmentModel>();
            content.Uploads.Where(x => !x.Deleted).ToList().ForEach(upload =>
            {
                if (Path.GetExtension(upload.Name).ToLower().Equals(".mp3"))
                    upload.Type = TrainingDocumentTypeEnum.Audio.ToString();
                if (Enum.TryParse<TrainingDocumentTypeEnum>(upload.Type, true, out var type))
                {
                    if (data)
                        attachments.Add(GetAttachmentModel(type, upload.Name, upload.Description, upload.Data,
                            includeData: data));
                    else
                        attachments.Add(GetAttachmentModel(type, upload.Name, upload.Description, upload.Data,
                            includeData: data, maxWidth: 240, maxHeight: 240, reference: upload.Order.ToString()));
                }
            });

            return attachments;
        }

        private IEnumerable<AttachmentModel> GetAttachments(IEnumerable<UploadResultViewModel> content, bool data = false)
        {

            var attachments = new List<AttachmentModel>();
            content.ToList().ForEach(upload =>
            {
                if (Path.GetExtension(upload.Name).ToLower().Equals(".mp3"))
                    upload.Type = TrainingDocumentTypeEnum.Audio.ToString();
                if (Enum.TryParse<TrainingDocumentTypeEnum>(upload.Type, true, out var type))
                {
                    if (data)
                        attachments.Add(GetAttachmentModel(type, upload.Name, upload.Description, upload.Data,
                            includeData: data));
                    else
                        attachments.Add(GetAttachmentModel(type, upload.Name, upload.Description, upload.Data,
                            includeData: data, maxWidth: 240, maxHeight: 240, reference: upload.ToString()));
                }
            });

            return attachments;
        }

    }

    public class PrintMemoQueryHandler : PrintDocumentQueryHandler<Memo>
    {
        public PrintMemoQueryHandler(IRepository<Memo> memoRepository) : base(memoRepository)
        {
        }
    }

    public class PrintPolicyQueryHandler : PrintDocumentQueryHandler<Policy>
    {
        public PrintPolicyQueryHandler(IRepository<Policy> policyRepository) : base(policyRepository)
        {
        }
    }

    public class PrintTestQueryHandler : PrintDocumentQueryHandler<Test>
    {
        public PrintTestQueryHandler(IRepository<Test> testRepository) : base(testRepository)
        {
        }
    }

    public class PrintTrainingManualQueryHandler : PrintDocumentQueryHandler<TrainingManual>
    {
        public PrintTrainingManualQueryHandler(IRepository<TrainingManual> trainingManualRepository) : base(trainingManualRepository)
        {
        }
    }

    public class PrintCheckListQueryHandler : PrintDocumentQueryHandler<CheckList>
    {
        public PrintCheckListQueryHandler(IRepository<CheckList> checkListRepository) : base(checkListRepository)
        {
        }
    }
    public class PrintCustomDocumentQueryHandler : PrintDocumentQueryHandler<Domain.Customer.Models.CustomDocument>
    {
        public PrintCustomDocumentQueryHandler(IRepository<Domain.Customer.Models.CustomDocument> customDocumentRepository) : base(customDocumentRepository)
        {
        }
    }
}