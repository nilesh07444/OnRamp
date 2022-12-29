using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Common.Command;
using Common.Web;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.Certificate;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.QueryParameter.Certificates;
using Ramp.Contracts.QueryParameter.Upload;
using Ramp.Contracts.ViewModel;
using Web.UI.Code.ActionFilters;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers
{
    [PortalContextActionFilter]
    public class AchievementController : KnockoutCRUDController<CertificateListQuery, CertificateListModel, Certificate,
        CertificateModel, CreateOrUpdateCertificateCommand>
    {
        public override void Index_PostProcess(IEnumerable<CertificateListModel> listModel)
        {
			foreach (var certificateListModel in listModel) {
				certificateListModel.ThumbnailUrl =
					Url.ActionLink<UploadController>(a =>
						a.GetThumbnail(certificateListModel.UploadId, 842, 595)); // 72 dpi
			}
		}

        public override void Edit_PostProcess(CertificateModel model)
        {
            if (model.Upload != null)
            {
                model.Upload.DeleteUrl = Url.ActionLink<UploadController>(m => m.Delete(model.Upload.Id, null));
                model.Upload.Url = Url.ActionLink<UploadController>(m => m.Get(model.Upload.Id, false));
            }
        }

		[HttpPost]
		public HttpStatusCodeResult AddCertificate(CertificateModel model) {
			var certificateAdd = new CreateOrUpdateCertificateCommand {
				Id = model.Id,
				Upload = model.Upload,
				Title = model.Title
			};
			var response = ExecuteCommand(certificateAdd);
			if (model.Upload != null) {
				model.Upload.DeleteUrl = Url.ActionLink<UploadController>(m => m.Delete(model.Upload.Id, null));
				model.Upload.Url = Url.ActionLink<UploadController>(m => m.Get(model.Upload.Id, false));
			}
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		[HttpPost]
        public ActionResult RemoveCertificateUpload(RemoveCertificateUploadCommand command)
        {
            if (!ModelState.IsValid || !base.ValidateAndExecute(command))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest,
                    ModelState.ModelStateToDictionary().ToString());
            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
        }

        [HttpPost]
        public JsonResult Upload(bool mainContext = false)
        {
            if (HttpContext.Request.Files.Count > 0 && HttpContext.Request.Files.AllKeys.Any())
            {
                var file = HttpContext.Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    using (var fileStream = file.InputStream)
                    using (var memoryStream = new MemoryStream())
                    {
                        memoryStream.SetLength(fileStream.Length);
                        fileStream.Read(memoryStream.GetBuffer(), 0, (int) fileStream.Length);
                        memoryStream.Seek(0, SeekOrigin.Begin);

						// Certificate image validation
						//using (var img = System.Drawing.Image.FromStream(memoryStream)) {
						//	if (img.Width != 2480 || img.Height != 3508) {
						//		ModelState.AddModelError("Upload",
						//			"Uploaded certificate should be A4 size ie 2480x3508.");
						//	}

						//	if (img.HorizontalResolution != 300f || img.VerticalResolution != 300f) {
						//		ModelState.AddModelError("Upload",
						//			"Uploaded certificate resolution should be 300 dpi.");
						//	}
						//}

						if (ModelState.IsValid)
                        {
                            file.InputStream.Seek(0, SeekOrigin.Begin);
                            var saveCommand = new SaveUploadCommand
                            {
                                FileUploadV = ExecuteQuery<GetFileUploadFromPostedFileQuery, UploadModel>(
                                    new GetFileUploadFromPostedFileQuery
                                    {
                                        Id = Guid.NewGuid(),
                                        File = file
                                    }),
                                MainContext = mainContext
                            };

                            var commandResponse = ExecuteCommand(saveCommand);
                            if (!commandResponse.Validation.Any())
                            {
                                var requestId = saveCommand.FileUploadV.Id;
                                var vm = new UploadResultViewModel
                                {
                                    DeleteType = "DELETE",
                                    DeleteUrl = Url.ActionLink<UploadController>(a => a.Delete(requestId, mainContext)),
                                    Name = saveCommand.FileUploadV.Name,
                                    Description = saveCommand.FileUploadV.Name,
                                    Type = saveCommand.FileUploadV.ContentType,
                                    InProcess = false,
                                    Progress = "100%",
                                    Size = saveCommand.FileUploadV.Data.Length,
                                    Url = Url.ActionLink<UploadController>(a => a.Get(requestId, mainContext)),
                                    ThumbnailUrl =
                                        Url.ActionLink<UploadController>(a => a.GetThumbnail(requestId, 842, 595)),
                                    Id = requestId
                                };

                                return new JsonResult {Data = vm};
                            }
                        }
                        else
                        {
                            var errorModel =
                                from x in ModelState.Keys
                                where ModelState[x].Errors.Count > 0
                                select new
                                {
                                    key = x,
                                    errors =
                                        (from e in ModelState[x].Errors
                                            select e.ErrorMessage).ToArray()
                                };

                            Response.StatusCode = (int) HttpStatusCode.BadRequest;
                            return new JsonResult {Data = errorModel};
                        }
                    }
                }
            }

            return new JsonResult {Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpGet]
        public JsonResult List(CertificateListQuery query)
        {
            var models = ExecuteQuery<CertificateListQuery, IEnumerable<UploadResultViewModel>>(query).ToList();
            models.ForEach(m =>
            {
                m.ThumbnailUrl = Url.ActionLink<UploadController>(x => x.GetThumbnail(m.Id, 424, 300));
            });
            var @default = models.First(m => m.Description == "Default");
            models.Remove(@default);
            var ordered = models.OrderBy(m => m.Description).ToList();
            ordered.Insert(0, @default);
            return new JsonResult {Data = ordered, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        [HttpDelete]
        public override HttpStatusCodeResult Delete(string id)
        {
            var result = ExecuteCommand(new DeleteByIdCommand<Certificate> {Id = id});
            if (result.Validation.Any())
            {
                if (result.Validation.Any(v =>
                    v.MemberName == nameof(Certificate.UploadId) && v.Message.Contains("linked to active Tests.")))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
        }

		[HttpGet]
		public ActionResult GetDocument(string id) {
			var documents = new List<DocumentListModel>();
			var query = new DocumentListQuery();
			var documentList = ExecuteQuery<DocumentListQuery, IEnumerable<DocumentListModel>>(query).ToList();
			var certificateList = documentList.Where(x => x.Certificate != null).ToList();
			documents = certificateList.Where(x => x.Certificate.Id == id).ToList();
			return PartialView("_AssociateDocument", documents);
		}

		[HttpGet]
		public ActionResult DownloadCertificate(string id) {

			var upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter {
				Id = id
			});
			Response.Clear();
			MemoryStream ms = new MemoryStream(upload.Data);
			Response.ContentType = upload.ContentType;
			string filename = upload.Name;
			Response.AddHeader("filename", filename);
			Response.Buffer = true;
			ms.WriteTo(Response.OutputStream);
			Response.End();
			return new FileStreamResult(ms, upload.ContentType);
		}

		[HttpGet]
		public PartialViewResult GetAchievement() {			
			return PartialView("_AddAchievement");
		}

	}
	}