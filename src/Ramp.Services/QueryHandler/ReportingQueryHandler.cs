using Common.Query;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Common.Report;
using Ramp.Contracts.QueryParameter;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Styles;
using System.IO;
using System.Web.Hosting;
using NReco.ImageGenerator;
using Domain.Customer.Models;
using System.Configuration;
using Common.Data;
using Domain.Models;
using Ramp.Services.QueryHandler.Reporting;

namespace Ramp.Services.QueryHandler {
	public abstract class ReportingQueryHandler<TQuery> : IQueryHandler<TQuery, IExportModel>
		where TQuery : class, IContextQuery {
		private readonly ReportDocument _document;
		protected static readonly FontElementStyle smallFont = new FontElementStyle(new Font("Verdana", 8));
		protected static readonly FontElementStyle defaultFont = new FontElementStyle(new Font("Verdana", 9));
		protected static readonly FontElementStyle headerFont = new FontElementStyle(new Font("Verdana", 10));
		protected static readonly FontElementStyle largeFont = new FontElementStyle(new Font("Verdana", 11));
		protected static readonly FontElementStyle largeFontBold = new FontElementStyle(new Font("Verdana", 11, FontStyle.Bold));
		protected static readonly FontElementStyle headingFont = new FontElementStyle(new Font("Verdana", 12));
		protected static readonly FontElementStyle massiveFont = new FontElementStyle(new Font("Verdana", 16));
		protected static readonly FontElementStyle hudgeFont = new FontElementStyle(new Font("Verdana", 18));
		protected static readonly HorizontalAlignmentElementStyle Centered = new HorizontalAlignmentElementStyle(HorizontalAlignment.Center);
		protected static readonly HorizontalAlignmentElementStyle RightAligned = new HorizontalAlignmentElementStyle(HorizontalAlignment.Right);
		protected static readonly HorizontalAlignmentElementStyle LeftAligned = new HorizontalAlignmentElementStyle(HorizontalAlignment.Left);
		public string HeaderStyleColor { get; set; } = "#555";
		protected ReportingQueryHandler() {
			_document = new ReportDocument();
		}
		public abstract void BuildReport(ReportDocument document, out string title,out string RecipRecepients, TQuery data);
		public IExportModel ExecuteQuery(TQuery query) {
			var title = "";
			var _Recepients = "";
			if (query.PortalContext != null)
			{
				HeaderStyleColor = query.PortalContext.UserCompany.CustomColours != null ? query.PortalContext.UserCompany.CustomColours.HeaderColour : "#555";
			}
			BuildReport(_document, out title,out _Recepients ,query);
			return new ExportModel() {
				Title = title,
				Document = _document,
				Recepients= _Recepients
			};
		}

		protected ReportSection CreateSection(string header, PageOrientation orientation = PageOrientation.Landscape, bool? pageBreakAfter = null) {
			var section = new ReportSection(header) { PageOrientation = orientation };
			section.AddHeaderStyle(new HeaderElementStyle(HeaderType.Heading2));
			section.PageBreakAfter = pageBreakAfter.HasValue ? pageBreakAfter.Value : true;
			section.PageBreakBefore = false;
			return section;
		}

		protected GridBlock CreateGrid() {
			var grid = new GridBlock();
			grid.AddStyle(new BorderElementStyle(BorderStyle.Solid, 1, ColorTranslator.FromHtml("#555")));
			return grid;
		}
		protected IEnumerable<ElementStyle> HeaderStyle {
			get {
				return new List<ElementStyle>
				{
					new BackgroundColorElementStyle(ColorTranslator.FromHtml(HeaderStyleColor)),
					new ForeColorElementStyle(Color.White),
					new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold))
				};
			}
		}

		protected void CreateTableHeader(GridBlock grid, params Tuple<string, int>[] columns) {
			var header = new GridRowBlock();
			header.AddDefaultChildStyle(new BackgroundColorElementStyle(ColorTranslator.FromHtml(HeaderStyleColor)));
			header.AddDefaultChildStyle(new ForeColorElementStyle(Color.White));
			header.AddStyle(new FontElementStyle(new Font(headerFont.Font, FontStyle.Bold)));

			columns.ToList().ForEach(delegate (Tuple<string, int> column) {
				grid.ColumnWidths.Add(column.Item2);
				var cell = new GridCellBlock(column.Item1);
				header.AddElement(cell);
			});
			grid.AddElement(header);
		}
		protected GridRowBlock CreateTableDataRow(GridBlock grid, params object[] data) {
			var row = new GridRowBlock();
			row.AddStyle(new FontElementStyle(new Font(defaultFont.Font, FontStyle.Regular)));
			CreateDataRow(row, data);
			grid.AddElement(row);
			return row;
		}
		protected GridRowBlock CreateTableDataRowWithStyles(GridBlock grid, IEnumerable<ElementStyle> styles, params object[] data) {
			var row = new GridRowBlock();
			styles.ToList().ForEach(x => row.AddStyle(x));
			if (!styles.Any(x => x is FontElementStyle))
				row.AddStyle(new FontElementStyle(new Font(defaultFont.Font, FontStyle.Regular)));
			CreateDataRow(row, data);
			grid.AddElement(row);
			return row;
		}
		protected GridRowBlock CreateTableDataRowWithStyles(GridBlock grid, IEnumerable<ElementStyle> styles, params Element[] elements) {
			var row = new GridRowBlock();
			styles.ToList().ForEach(x => row.AddStyle(x));
			if (!styles.Any(x => x is FontElementStyle))
				row.AddStyle(new FontElementStyle(new Font(defaultFont.Font, FontStyle.Regular)));
			CreateDataRow(row, elements);
			grid.AddElement(row);
			return row;
		}
		private void CreateDataRow(GridRowBlock row, params object[] data) {
			data.ToList().ForEach(x => row.AddCell(x == null ? "" : x.ToString()));
		}
		private void CreateDataRow(GridRowBlock row, params Element[] elements) {
			elements.ToList().ForEach(x => row.AddElement(new GridCellBlock(x)));
		}
		protected ParagraphBlock CreateSubHeading(string text) {
			var block = new ParagraphBlock(text);
			block.AddStyle(new FontElementStyle(new Font(headingFont.Font, FontStyle.Bold)));
			return block;
		}
		protected ImageBlock CreateCompanyLogo(PortalContextViewModel portalContext) {
			ImageBlock ib = null;
			if (portalContext != null && portalContext.UserCompany != null) {
				if (!string.IsNullOrEmpty(portalContext.LogoFileName))
					ib = GetServerResource($"~/LogoImages/CustomerLogo/{portalContext.UserCompany.LogoImageUrl}", 1040, 200);
				else if (portalContext.UserCompany != null && portalContext.UserCompany.CustomerConfigurations.Count > 0 && portalContext.UserCompany.CustomerConfigurations.Any(x => x.Type == Domain.Models.CustomerConfigurationType.DashboardLogo && (!x.Deleted.HasValue || (x.Deleted.HasValue && !x.Deleted.Value))))
					ib = GetImage(portalContext.UserCompany.CustomerConfigurations.Where(x => x.Type == Domain.Models.CustomerConfigurationType.DashboardLogo && (!x.Deleted.HasValue || (x.Deleted.HasValue && !x.Deleted.Value))).OrderBy(x => x.Version).LastOrDefault().UploadModel.Data, 570, null);
			}
			if (ib == null)
				ib = GetServerResource("~/Content/images/logo.png", 1040, 200);
			ib.AddStyle(new HorizontalAlignmentElementStyle(HorizontalAlignment.Center));
			return ib;
		}
		protected ParagraphBlock CreateCenteredHorizontalRule() {
			var p = new ParagraphBlock("_____________________________________________");
			p.AddStyle(new HorizontalAlignmentElementStyle(HorizontalAlignment.Center));
			p.AddStyle(new ForeColorElementStyle(Color.LightSlateGray));
			return p;
		}
		protected ImageBlock GetImage(byte[] data, int? maxWidth, int? height) {
			var imageData = VirtuaCon.Drawing.ImageUtil.GetHighQualityImage(data, maxWidth, height);
			if (imageData != null) {
				var block = new ImageBlock(imageData);
				block.AutoScale = false;
				return block;
			}
			return null;
		}
		protected ImageBlock GetServerResource(string virtualPath, int? maxWidth, int? maxHeight) {
			if (Exsists(virtualPath))
				return GetImage(GetRawBytesFromServerResource(virtualPath), maxWidth, maxHeight);
			return null;
		}

		protected byte[] GetRawBytesFromServerResource(string virtualPath) {
			return File.ReadAllBytes(HostingEnvironment.MapPath(virtualPath));
		}
		protected bool Exsists(string virtualPath) {
			return File.Exists(HostingEnvironment.MapPath(virtualPath));
		}
		protected ParagraphBlock CreateParagraph(string text, params ElementStyle[] styles) {
			var p = new ParagraphBlock(text);
			styles.ToList().ForEach(x => p.AddStyle(x));
			return p;
		}
		protected ParagraphBlock AddBreak(float topOffset = 100) {
			var p = new ParagraphBlock();
			p.AddStyle(new PaddingElementStyle(null, null, topOffset, null));
			return p;
		}
		protected HTMLBlock GetImageFromHtml(string html, int width, params ElementStyle[] styles) {
			var block = new HTMLBlock(html);
			styles.ToList().ForEach(x => block.AddStyle(x));
			if (!styles.Any(style => style is FontElementStyle))
				block.AddStyle(defaultFont);
			return block;
		}
		protected ImageBlock GetThumbnail(TrainingDocumentTypeEnum type, byte[] data, string name, int? maxWidth, int? maxHeight, float frameTime = 0f) {
			if (!string.IsNullOrEmpty(name) && Path.GetExtension(name).ToLowerInvariant().Equals(".mp3"))
				type = TrainingDocumentTypeEnum.Audio;
			try {
				switch (type) {
					case TrainingDocumentTypeEnum.Excel:
						return GetServerResource("~/Content/images/excelDoc.png", maxWidth, maxHeight);
					case TrainingDocumentTypeEnum.Audio:
						return GetServerResource("~/Content/images/OR_AudioIcon_v2.jpg", maxWidth, maxHeight);
					case TrainingDocumentTypeEnum.Image:
						return GetImage(data, maxWidth / 2, maxHeight / 2);
					case TrainingDocumentTypeEnum.PowerPoint:
						return GetServerResource("~/Content/images/ppDoc.png", maxWidth, maxHeight);
					case TrainingDocumentTypeEnum.Pdf:
						return GetServerResource("~/Content/images/otherDoc.png", maxWidth, maxHeight);
					case TrainingDocumentTypeEnum.WordDocument:
						return GetServerResource("~/Content/images/wordDoc.png", maxWidth, maxHeight);
					case TrainingDocumentTypeEnum.Video:
						return GetVideoThumbnail(name, data, maxWidth, maxHeight, frameTime);
					default:
						return null;
				}
			}
			catch (Exception) {

			}
			return null;
		}
		protected ImageBlock GetVideoThumbnail(string fileName, byte[] data, int? maxWidth, int? maxHeight, float frameTime = 0f) {
			var videoPath = Path.Combine(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]), $"{Guid.NewGuid()}{Path.GetExtension(fileName)}");
			using (var stream = File.Open(videoPath, FileMode.OpenOrCreate)) {
				stream.Write(data, 0, data.Length);
			}
			var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
			var thumbnailStream = new MemoryStream();
			ffMpeg.GetVideoThumbnail(videoPath, thumbnailStream, frameTime);
			return GetImage(thumbnailStream.ToArray(), maxWidth / 2, maxHeight / 2);
		}
		protected AttachmentModel GetAttachmentModel(TrainingDocumentTypeEnum type, string fileName, string description, byte[] data, string reference = null, int? parent = null, bool includeData = false, int? maxWidth = null, int? maxHeight = null, float frameTime = 0f) {
			return new AttachmentModel {
				Description = EnsureNoExtention(fileName, description),
				Name = EnsureExtention(fileName, description),
				Parent = parent,
				Reference = reference,
				Type = type,
				Thumbnail = GetThumbnail(type, data, fileName, maxWidth, maxHeight, frameTime),
				Data = includeData ? data : null
			};
		}
		protected AttachmentModel GetAttachmentModel(ChapterLinkType type, string url, string reference = null, int? parent = null, int? maxWidth = null, int? maxHeight = null) {
			return new AttachmentModel {
				Description = url,
				Name = url,
				Reference = reference,
				Parent = parent,
				Type = type == ChapterLinkType.Vimeo ? TrainingDocumentTypeEnum.Vimeo : TrainingDocumentTypeEnum.YoutubeVideo,
				Thumbnail = GetThumbnail(type, url, maxWidth, maxHeight)
			};
		}
		protected ImageBlock GetThumbnail(ChapterLinkType type, string url, int? maxWidth = null, int? maxHeight = null) {
			switch (type) {
				case ChapterLinkType.Vimeo:
					var tempVimeoUrl = $"<iframe src='{url}' width='640' height='360' frameborder='0' webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>";
					var contentVimeo = new HtmlToImageConverter();
					var image = contentVimeo.GenerateImage(tempVimeoUrl, NReco.ImageGenerator.ImageFormat.Jpeg);
					return GetImage(image, maxWidth, maxHeight);
				case ChapterLinkType.Youtube:
					var tempUrl = url.Replace("https://", "http://").Replace("http://www.youtube.com/embed/", "http://img.youtube.com/vi/");
					tempUrl = tempUrl + "/0.jpg";
					var content = new HtmlToImageConverter();
					var intro = content.GenerateImage($"<img src='{tempUrl}' />", NReco.ImageGenerator.ImageFormat.Jpeg);
					return GetImage(intro, maxWidth, maxHeight);
				default:
					return null;
			}
		}
		private string EnsureExtention(string name, string description) {
			if (name == null)
				return null;
			if (description == null)
				return name;
			if (string.IsNullOrWhiteSpace(Path.GetExtension(description)))
				description = description + Path.GetExtension(name);
			return description;
		}
		private string EnsureNoExtention(string name, string description) {
			if (name == null)
				return null;
			if (description == null)
				return Path.GetFileNameWithoutExtension(name);
			return Path.GetFileNameWithoutExtension(description);
		}
		protected IList<GridBlock> CreateAttachmentsBlock(IEnumerable<AttachmentModel> attachments) {
			var result = new List<GridBlock>();
			var temp = attachments.ToList();
			for (int i = 0; i < attachments.Count(); i += 4) {
				var group = temp.Take(4);
				var grid = new GridBlock();
				var widths = new List<int>();
				group.ToList().ForEach(x => widths.Add(25));
				for (int j = group.Count(); j < 4; j++)
					grid.ColumnWidths.Add(25);
				grid.ColumnWidths.AddRange(widths);
				grid.AddDefaultChildStyle(Centered);
				grid.AddDefaultChildStyle(largeFont);

				var row = new GridRowBlock();
				foreach (var attachment in group)
					row.AddElement(new GridCellBlock(attachment.Thumbnail));
				for (int j = group.Count(); j < 4; j++)
					row.AddElement(new GridCellBlock(""));
				grid.AddElement(row);

				var descriptionRow = new GridRowBlock();
				foreach (var attachment in group)
					descriptionRow.AddElement(new GridCellBlock(CreateParagraph(attachment.Description, largeFont)));
				for (int j = group.Count(); j < 4; j++)
					descriptionRow.AddElement(new GridCellBlock(""));
				grid.AddElement(descriptionRow);

				result.Add(grid);
				temp.RemoveRange(0, group.Count());
			}
			return result;
		}
		protected void CreateAllImageAttachments(ReportSection section, IEnumerable<AttachmentModel> attachments) {
			attachments.Where(x => x.Thumbnail.Graphic.Width > 200).ToList().ForEach(delegate (AttachmentModel attachment) {
				attachment.Thumbnail.AddStyle(new HorizontalAlignmentElementStyle(HorizontalAlignment.Center));
				section.AddElement(attachment.Thumbnail);

			});

			var sideBySide = attachments.Where(x => x.Thumbnail.Graphic.Width <= 200).ToList();
			var temp = sideBySide;
			for (int i = 0; i < sideBySide.Count(); i++) {
				var grid = new GridBlock();
				sideBySide.ToList().ForEach(x => grid.ColumnWidths.Add(Int32.Parse(Math.Round((double)100 / (double)sideBySide.Count(), 0).ToString())));
				grid.AddDefaultChildStyle(new HorizontalAlignmentElementStyle(HorizontalAlignment.Center));
				var row = new GridRowBlock();
				var group = temp.Take(2);
				if (group.Count() == 1) {
					group.First().Thumbnail.AddStyle(new HorizontalAlignmentElementStyle(HorizontalAlignment.Center));
					var gridBlock = new GridCellBlock(group.First().Thumbnail);
					gridBlock.ColumnSpan = 2;
					row.AddElement(gridBlock);
				} else {
					foreach (var attachment in group) {
						attachment.Thumbnail.AddStyle(new HorizontalAlignmentElementStyle(HorizontalAlignment.Center));
						row.AddElement(new GridCellBlock(attachment.Thumbnail));
					}
				}
				grid.AddElement(row);
				section.AddElement(grid);
				temp.RemoveRange(0, group.Count());
			}
		}
		protected List<string> GetAllByTag(string tag, string html) {
			var occurences = new List<string>();
			var index = -1;
			var tagBegining = $"<{tag}";
			var workingCopy = html;
			do {
				index = workingCopy.IndexOf(tagBegining);
				if (index > -1) {
					workingCopy = workingCopy.Substring(index);
					var endIndex = -1;
					if (tag == "img")
						endIndex = workingCopy.IndexOf(">");
					else {
						endIndex = workingCopy.IndexOf(">", workingCopy.IndexOf(">") + 1);
					}
					if (endIndex > -1) {
						occurences.Add(workingCopy.Substring(0, endIndex + 1));
						workingCopy = workingCopy.Substring(endIndex + 1);
					}
				}
			} while (index > -1);
			return occurences;
		}
	}
}
