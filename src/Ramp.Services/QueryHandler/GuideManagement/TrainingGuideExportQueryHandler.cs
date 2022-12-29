using Common.Data;
using Common.Query;
using Domain.Customer.Models;
using NReco.ImageGenerator;
using Ramp.Contracts.QueryParameter.GuideManagement;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;
using System.IO;
using System.Web.Hosting;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Web;
using Domain.Models;
using Common;
using Ramp.Contracts.QueryParameter.Upload;
using Ramp.Services.Helpers;

namespace Ramp.Services.QueryHandler.GuideManagement
{
    public class TrainingGuideExportQueryHandler : ReportingQueryHandler<TrainingGuideExportQuery>,IQueryHandler<TrainingGuideExportQuery,IEnumerable<AttachmentModel>>
    {
        private readonly IRepository<TrainingGuide> _guideRepository;
        private readonly IRepository<StandardUser> _userRepository;
        private readonly IQueryExecutor _executor;
        public TrainingGuideExportQueryHandler(IRepository<TrainingGuide> guideRepository, IRepository<StandardUser> userRepository,IQueryExecutor executor)
        {
            _guideRepository = guideRepository;
            _userRepository = userRepository;
            _executor = executor;
        }
      
        public override void BuildReport(ReportDocument document, out string title,out string recepitent, TrainingGuideExportQuery data)
        {
            var guide = _guideRepository.Find(data.TrainingGuideId);
            var user = _userRepository.Find(data.UserId);
            if (guide == null || user == null)
                throw new ArgumentNullException();
            title = guide.Title;
            recepitent = data.Recepients;
            document.AddStyle(new PaddingElementStyle(5, 5, 5, 5));
            CreateCoverPage(document, guide, data.PortalContext);
            CreateChapters(document, guide.ChapterList.OrderBy(x => x.ChapterNumber), guide.ChapterList.Min(x => x.ChapterNumber) == 0 ? 1 : 0);
        }
        private void CreateCoverPage(ReportDocument document, TrainingGuide guide,PortalContextViewModel portalContext)
        {
            var section = CreateSection("", PageOrientation.Portrait,false);
            section.AddElement(CreateCompanyLogo(portalContext));
            section.AddElement(AddBreak());
            section.AddElement(CreateParagraph(guide.Title, new VerticalAlignmentElementStyle(VerticalAlignment.Center),
                                                           new HorizontalAlignmentElementStyle(HorizontalAlignment.Center),
                                                           new FontElementStyle(new Font(hudgeFont.Font, FontStyle.Bold))));
            section.AddElement(CreateParagraph(guide.ReferenceId, new VerticalAlignmentElementStyle(VerticalAlignment.Center),
                                                           new HorizontalAlignmentElementStyle(HorizontalAlignment.Center),
                                                           new FontElementStyle(new Font(massiveFont.Font, FontStyle.Bold))));
            section.AddElement(AddBreak(20));
            section.AddElement(CreateParagraph(guide.Description, new VerticalAlignmentElementStyle(VerticalAlignment.Center),
                                                           new HorizontalAlignmentElementStyle(HorizontalAlignment.Center),
                                                           new FontElementStyle(new Font(massiveFont.Font, FontStyle.Regular))));
            document.AddElement(section);

            if (guide.CoverPicture != null)
            {
                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(AddBreak());
                var pic = GetImage(guide.CoverPicture.Data, 800, 400);
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
       
        private void CreateChapters(ReportDocument document,IOrderedEnumerable<TraningGuideChapter> chapters,int chapterNumberOffset)
        {
            foreach (var chapter in chapters.ToList())
            {
                var section = CreateSection("", PageOrientation.Portrait, false);
                
                var contentTable = new GridBlock();
                contentTable.ColumnWidths.Add(100);
                var chapterHeadingRow = new GridRowBlock();
                chapterHeadingRow.AddElement(new GridCellBlock(CreateParagraph($"{chapter.ChapterNumber + chapterNumberOffset}. {chapter.ChapterName}",
                                                         new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                                                         new FontElementStyle(new Font(massiveFont.Font, FontStyle.Bold)))));
                contentTable.AddElement(chapterHeadingRow);
                if (chapter.ChapterContent != null)
                {
                    var contentImageRow = new GridRowBlock();
                    contentImageRow.AddElement(new GridCellBlock(CreateChapterContentBlock(chapter.ChapterContent)));
                    contentTable.AddElement(contentImageRow);
                }
                section.AddElement(contentTable);
                document.AddElement(section);
                section = CreateSection("", PageOrientation.Portrait, false);
                var attachments = GetAttachments(chapter).OrderBy(x => x.Reference);
                document.AddElement(section);
                section = CreateSection("", PageOrientation.Portrait, false);
                if (attachments.Any(x => x.Thumbnail != null))
                {
                    section.AddElement(CreateParagraph($"Resources:",
                                                             new HorizontalAlignmentElementStyle(HorizontalAlignment.Left),
                                                             new FontElementStyle(new Font(largeFont.Font, FontStyle.Underline))));
                    CreateAttachmentsBlock(attachments.Where(x => x.Thumbnail != null)).ToList().ForEach(delegate (GridBlock block)
                    {
                        section.AddElement(block);
                    });
                }
                document.AddElement(section);
                section = CreateSection("", PageOrientation.Portrait, false);
                section.AddElement(CreateCenteredHorizontalRule());
                document.AddElement(section);
            }
        }
        private HTMLBlock CreateChapterContentBlock(string chapterContentHTML)
        {
            return GetImageFromHtml(GetChapterContent(chapterContentHTML), 1000, new HorizontalAlignmentElementStyle(HorizontalAlignment.Center),massiveFont);
        }
       
        private string GetChapterContent(string html)
        {
            var basePath = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}";
            var result = html;
            var listOfImages = GetAllByTag("img", html);
            var listOfIFrames = GetAllByTag("iframe", html);
            var listOfAnchors = GetAllByTag("a", html);

            listOfImages.ForEach(delegate (string tag)
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
            listOfIFrames.ForEach((tag) =>
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
            listOfAnchors.ForEach((tag) =>
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
            return result.Replace("<hr />","<br>");
        }
        private string GetUpload(string src, string basePath,out bool found)
        {
            var path = string.Empty;
            Guid? id = null;
            string url = string.Empty;
            if (src.Contains("Upload/"))
            {
                url = src.Substring(src.IndexOf("src=\"")).Replace("src=\"", "");
                url = url.Substring(1, url.IndexOf("\""));
                if (src.Contains("Upload/Get/"))
                {
                    var uploadId = src.Substring(src.IndexOf("Upload/Get/"));
                    uploadId = uploadId.Replace("Upload/Get/", "");
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
                var upload = _executor.Execute<FetchUploadQueryParameter, FileUploadViewModel>(new FetchUploadQueryParameter
                {
                    Id = id.Value.ToString(),
                    ExcludeBytes = false
                });
                if (upload != null)
                {
                    var physicalPathRoot = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
                    var virtualPath = ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"].Replace("~", basePath);
                    var physicalPath = Create(upload.Data, Guid.NewGuid().ToString(), upload.Name);
                    var fileDirectory = physicalPath.Replace(physicalPathRoot, string.Empty).Split('\\')[0];
                    var fileName = physicalPath.Replace(physicalPathRoot, string.Empty).Split('\\')[1];
                    path = $"{physicalPath}\"";
                }
            }
            found = !string.IsNullOrWhiteSpace(path);
            return found ? src.Replace(url, path) : src;
        }
        string Create(byte[] data, string uniqueId, string filename)
        {
            var path = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
            Directory.CreateDirectory(Path.Combine(path, uniqueId));
            path = Path.Combine(path, uniqueId, filename.RemoveSpecialCharacters());
            if (!System.IO.File.Exists(path))
            {
                if (data != null)
                    Utility.Convertor.CreateFileFromBytes(data, path);
            }
            return path;
        }
        private IEnumerable<AttachmentModel> GetAttachments(TrainingGuide guide,bool data = false)
        {
            var attachments = new List<AttachmentModel>();
            guide.ChapterList.OrderBy(x => x.ChapterNumber).ToList().ForEach(delegate (TraningGuideChapter chapter)
            {
                attachments.AddRange(GetAttachments(chapter,data));
            });
            return attachments;
        }
        private IEnumerable<AttachmentModel> GetAttachments(TraningGuideChapter chapter,bool data = false,bool includeLinks = false)
        {
            var attachments = new List<AttachmentModel>();
            chapter.ChapterUploads.Where(x => x.Upload != null).ToList().ForEach(delegate (ChapterUpload upload)
                 {
                     if (Path.GetExtension(upload.Upload.Name).ToLower().Equals(".mp3"))
                         upload.Upload.Type = TrainingDocumentTypeEnum.Audio.ToString();
                     TrainingDocumentTypeEnum type = default(TrainingDocumentTypeEnum);
                     if (Enum.TryParse<TrainingDocumentTypeEnum>(upload.Upload.Type, true, out type))
                     {
                         if (data)
                             attachments.Add(GetAttachmentModel(type, upload.Upload.Name, upload.Upload.Description, upload.Upload.Data, includeData: data));
                         else
                             attachments.Add(GetAttachmentModel(type, upload.Upload.Name, upload.Upload.Description, upload.Upload.Data, includeData: data, maxWidth: 240, maxHeight: 240, reference: upload.ChapterUploadSequence.ToString()));
                     }
                 });
            if (includeLinks)
                chapter.ChapterLinks.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList().ForEach(delegate (ChapterLink link)
                {
                    attachments.Add(GetAttachmentModel(link.Type, link.Url, reference: link.ChapterUploadSequence.ToString()));
                });
            return attachments;
        }
        IEnumerable<AttachmentModel> IQueryHandler<TrainingGuideExportQuery, IEnumerable<AttachmentModel>>.ExecuteQuery(TrainingGuideExportQuery query)
        {
            var guide = _guideRepository.Find(query.TrainingGuideId);
            if (guide == null)
                throw new ArgumentNullException();
            return GetAttachments(guide,true);
        }
       
    }
}
