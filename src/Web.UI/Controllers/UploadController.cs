using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.QueryParameter.Upload;
using Ramp.Contracts.ViewModel;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Common.Report;
using Ramp.Contracts.Query.Upload;
using VirtuaCon.Reporting;
using Web.UI.Code.Extensions;
using iTextSharp.text.pdf;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Web.UI.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public class UploadController : RampController
    {
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Get(string id, bool mainContext = false)
        {
            var upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter
            {
                Id = id,
                ExcludeBytes = true,
                MainContext = mainContext
            });
            if (upload != null)
            {
                HttpContext.Response.AddHeader("Content-Disposition", "inline; filename=" + upload.Name.RemoveSpecialCharacters());
                var physicalPathRoot = Server.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
                var virtualPath = AppSettings.Urls.ResolveUrl(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
                var physicalPath = CreateOrGetCachedUpload(upload, mainContext);
                var fileDirectory = physicalPath.Replace(physicalPathRoot, string.Empty).Split('\\')[0];
                var fileName = physicalPath.Replace(physicalPathRoot, string.Empty).Split('\\')[1];

                var completeVirtualPath = $"{virtualPath}/{fileDirectory}/{fileName}";
                return Redirect(completeVirtualPath);
            }
            return new HttpNotFoundResult();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult GetFromCompany(string id, string companyId = null)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                companyId = PortalContext.Current.UserCompany.Id.ToString();

            var upload = ExecuteQuery<FetchUploadFromCompanyQuery, UploadModel>(new FetchUploadFromCompanyQuery
            {
                Id = id,
                ExcludeBytes = true,
                CompanyId = companyId
            });

            if (upload != null)
            {
                HttpContext.Response.AddHeader("Content-Disposition", "inline; filename=" + upload.Name.RemoveSpecialCharacters());
                var physicalPathRoot = Server.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
                var virtualPath = AppSettings.Urls.ResolveUrl(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
                var physicalPath = CreateOrGetCachedUpload(id, companyId);
                var fileDirectory = physicalPath.Replace(physicalPathRoot, string.Empty).Split('\\')[0];
                var fileName = physicalPath.Replace(physicalPathRoot, string.Empty).Split('\\')[1];

                var completeVirtualPath = $"{virtualPath}/{fileDirectory}/{fileName}";
                return Redirect(completeVirtualPath);
            }

            return new HttpNotFoundResult();
        }

        [HttpGet]
        public ActionResult Preview(string id, string companyId = null)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                companyId = PortalContext.Current.UserCompany.Id.ToString();

            var upload = ExecuteQuery<FetchUploadFromCompanyQuery, UploadModel>(new FetchUploadFromCompanyQuery
            {
                Id = id,
                ExcludeBytes = true,
                CompanyId = companyId
            });
            if (upload != null)
            {
                ViewBag.uploadId = id;
                HttpContext.Response.AddHeader("Content-Disposition", "inline; filename=" + upload.Name.RemoveSpecialCharacters());
                var physicalPathRoot = Server.MapPath(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
                var virtualPath = AppSettings.Urls.ResolveUrl(ConfigurationManager.AppSettings["TrainingGuidQuestionFilePath"]);
                var physicalPath = CreateOrGetCachedUpload(upload);
                var fileDirectory = physicalPath.Replace(physicalPathRoot, string.Empty).Split('\\')[0];
                var fileName = physicalPath.Replace(physicalPathRoot, string.Empty).Split('\\')[1];

                var completeVirtualPath = $"{virtualPath}/{fileDirectory}/{fileName}";
                return View(new OpenFileInNewBrowserViewModel { FileName = upload.Name.RemoveSpecialCharacters(), FilePath = completeVirtualPath, Type = upload.Type });
            }
            return new HttpNotFoundResult();
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

        private string CreateOrGetCachedUpload(string id, string companyId)
        {
            var fullUpload = ExecuteQuery<FetchUploadFromCompanyQuery, UploadModel>(new FetchUploadFromCompanyQuery
            {
                Id = id,
                CompanyId = companyId
            });
            return Create(fullUpload.Data, fullUpload.Id, fullUpload.Name);
        }

        public string FillInPdfForm(string filePaths)
        {

            PdfReader pdfReader = new PdfReader(filePaths);
            List<string> fields = new List<string>();
            foreach (var de in pdfReader.AcroFields.Fields)
                fields.Add(de.Key);

            FileInfo info = new FileInfo(filePaths);
            var newfile = info.FullName;
            var newfilePathName = newfile.Replace(info.Extension, "") + "_" + DateTime.Now.Millisecond.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Minute.ToString() + info.Extension;

            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newfilePathName, FileMode.Create));
            AcroFields pdfFormFields = pdfStamper.AcroFields;
            pdfFormFields.GenerateAppearances = true;

            string json = @"[{""templateid"": ""29cf6a79-00fa-41d0-8791-fa46e1f47011""},{""Given Name Text Box"": ""Tan"",""Family Name Text Box"": ""Hynh"",""email"": ""tanhhynh@gmail.com"",""address"": ""ABC""}]";

            dynamic data = JArray.Parse(json) as JArray;

            foreach (dynamic d in data)
            {
                foreach (JProperty p in d)
                {
                    if (fields.Contains(p.Name))
                    {
                        pdfFormFields.SetField(p.Name, p.Value.ToString());
                    }
                }
            }
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();

            return newfilePathName;
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
        
        [AllowAnonymous]
        [HttpGet]
        public ActionResult GetThumbnail(string id, int? height, int? width)
        {
            var upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter
            {
                Id = id
            });
            if (upload != null)
            {
                var result = GetThumbnailData(HttpContext.Request, upload, height, width, true);
                return File(result.Value, MimeMapping.GetMimeMapping(result.Key));
            }
            return new HttpNotFoundResult();
        }
        
        [AllowAnonymous]
        [HttpGet]
        public ActionResult DownloadFile(string uploadId)
        {
            var upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter
            {
                Id = uploadId
            });
            Response.Clear();
            MemoryStream ms = new MemoryStream(upload.Data);
            Response.ContentType = upload.ContentType;
            string filename = upload.Name;
            //Response.AddHeader("content-disposition", $"attachment;filename= {upload.Name}");
            Response.AddHeader("filename", filename);
            Response.Buffer = true;
            ms.WriteTo(Response.OutputStream);
            Response.End();
            return new FileStreamResult(ms, upload.ContentType);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult GetThumbnailFromCompany(string id, int? height, int? width, string companyId = null)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                companyId = PortalContext.Current.UserCompany.Id.ToString();

            var upload = ExecuteQuery<FetchUploadFromCompanyQuery, UploadModel>(new FetchUploadFromCompanyQuery
            {
                Id = id,
                CompanyId = companyId
            });
            if (upload != null)
            {
                var result = GetThumbnailData(HttpContext.Request, upload, height, width, true);
                return File(result.Value, MimeMapping.GetMimeMapping(result.Key));
            }

            return new HttpNotFoundResult();
        }
        
        [HttpPost]
        public JsonResult SaveSignature(string base64image,string section)
        {
            if (string.IsNullOrEmpty(base64image))
                return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            var t = base64image.Substring(22);  // remove data:image/png;base64,

            var username = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            byte[] bytes = Convert.FromBase64String(t);
            var randomFileName = section+"_"+username+ "_signature.png";
            bool mainContext = false;
            var saveCommand = new SaveUploadCommand
            {
                FileUploadV = new UploadModel
                {
                    ContentType = "image/jpeg",
                    Id = Guid.NewGuid().ToString(),
                    Name = randomFileName,
                    Type = CommonHelper.GetDocumentType(randomFileName).ToString(),
                    Data = bytes
                },
                MainContext = mainContext

            };

            var command = ExecuteCommand(saveCommand);
            if (!command.Validation.Any())
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
                    ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(requestId, 115, 115)),
                    Id = requestId
                };
                return new JsonResult() { Data = vm };
            }
            return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        [HttpPost]
        public JsonResult Post(bool mainContext = false)
        {
            if (HttpContext.Request.Files.Count > 0)
            {
                if (HttpContext.Request.Files.AllKeys.Any())
                {
                    var saveCommand = new SaveUploadCommand
                    {
                        FileUploadV = ExecuteQuery<GetFileUploadFromPostedFileQuery, UploadModel>(
                           new GetFileUploadFromPostedFileQuery
                           {
                               Id = Guid.NewGuid(),
                               File = HttpContext.Request.Files[0]
                           }),
                        MainContext = mainContext
                    };

                    var command = ExecuteCommand(saveCommand);
                    if (!command.Validation.Any())
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
                            ThumbnailUrl = Url.ActionLink<UploadController>(a => a.GetThumbnail(requestId, 115, 115)),
                            Id = requestId
                        };
                        return new JsonResult() { Data = vm };
                    }
                }
            }
            return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult PostFromContentToolsInitial(HttpPostedFileBase image, string documentId)
        {
            if (image == null || !string.IsNullOrWhiteSpace(documentId))
                return null;
            var saveCommand = new SaveUploadCommand
            {
                FileUploadV = ExecuteQuery<GetFileUploadFromPostedFileQuery, UploadModel>(
                    new GetFileUploadFromPostedFileQuery
                    {
                        File = image,
                        Id = Guid.NewGuid()
                    })
            };
            var result = ExecuteCommand(saveCommand);
            if (result.Validation.Any())
                return null;
            return new JsonResult { Data = new UploadFromContentToolsResultModel { url = Url.ActionLink<UploadController>(a => a.GetThumbnail(saveCommand.FileUploadV.Id, null, 300)) } };
        }

        [HttpPost]
        public ActionResult PostFromContentToolsCommit(string url, int width, string crop, string trainingGuideId)
        {
            var id = url.Replace(Url.Action("GetThumbnail", "Upload"), string.Empty).Substring(1);
            if (id.IndexOf('?') > -1)
                id = id.Substring(0, id.IndexOf('?'));
            var upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter { Id = id });
            upload.Data = GetThumbnailData(upload.Data, null, width);
            var updateCommand = new UpdateUploadCommand { Model = upload };
            var result = ExecuteCommand(updateCommand);
            if (result.Validation.Any())
                return null;
            upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter { Id = id });
            if (!string.IsNullOrWhiteSpace(crop))
            {
                var cropCoords = crop.Split(',');
                using (var msAfter = new MemoryStream())
                {
                    using (var ms = new MemoryStream(upload.Data))
                    {
                        var image = new Bitmap(ms);
                        var leftC = (int)(image.Width * Convert.ToDouble(cropCoords[1], System.Globalization.CultureInfo.InvariantCulture));
                        var topC = (int)(image.Height * Convert.ToDouble(cropCoords[0], System.Globalization.CultureInfo.InvariantCulture));
                        var rightC = (int)(image.Width * Convert.ToDouble(cropCoords[3], System.Globalization.CultureInfo.InvariantCulture));
                        var bottomC = (int)(image.Height * Convert.ToDouble(cropCoords[2], System.Globalization.CultureInfo.InvariantCulture));
                        var cImage = image.Clone(new Rectangle(leftC, topC, (int)Math.Abs(rightC - leftC), (int)Math.Abs(bottomC - topC)), PixelFormat.DontCare);
                        cImage.Save(msAfter, ImageFormat.Png);
                        upload.Name = $"{Path.GetFileNameWithoutExtension(upload.Name)}.png";
                        upload.ContentType = "image/png";
                        upload.Data = msAfter.ToArray();
                    }
                }
            }
            updateCommand = new UpdateUploadCommand { Model = upload };
            result = ExecuteCommand(updateCommand);
            if (result.Validation.Any())
                return null;
            upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter { Id = id });
            return new JsonResult { Data = new UploadFromContentToolsResultModel { size = new string[] { $"{upload.Width}", $"{upload.Height}" }, url = Url.ActionLink<UploadController>(a => a.GetThumbnail(updateCommand.Model.Id, null, null)) } };
        }

        [HttpPost]
        public ActionResult RotateImage(string url, string direction, string trainingGuideId)
        {
            var id = url.Replace(Url.Action("GetThumbnail", "Upload"), string.Empty).Substring(1);
            if (id.IndexOf('?') > -1)
                id = id.Substring(0, id.IndexOf('?'));
            var upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter { Id = id });
            if (upload.ContentType.Contains("image"))
            {
                using (var msAfter = new MemoryStream())
                {
                    using (var ms = new MemoryStream(upload.Data))
                    {
                        var image = System.Drawing.Image.FromStream(ms);
                        var type = direction.Equals("CW") ? System.Drawing.RotateFlipType.Rotate90FlipNone : System.Drawing.RotateFlipType.Rotate270FlipNone;
                        image.RotateFlip(type);
                        image.Save(msAfter, ImageFormat.Png);
                        upload.Name = $"{Path.GetFileNameWithoutExtension(upload.Name)}.png";
                        upload.ContentType = "image/png";
                        upload.Data = msAfter.ToArray();
                    }
                }
            }
            var updateCommand = new UpdateUploadCommand { Model = upload };
            var result = ExecuteCommand(updateCommand);
            if (result.Validation.Any())
                return null;
            upload = ExecuteQuery<FetchUploadQueryParameter, UploadModel>(new FetchUploadQueryParameter { Id = id });
            return new JsonResult { Data = new UploadFromContentToolsResultModel { size = new string[] { $"{upload.Width}", $"{upload.Height}" }, url = $"{Url.ActionLink<UploadController>(a => a.GetThumbnail(updateCommand.Model.Id, null, null)) }" } };
        }

        [HttpDelete]
        public JsonResult Delete(string id, bool? mainContext = null)
        {
            var command = ExecuteCommand(new DeleteUploadCommand { Id = id, MainContext = mainContext });
            if (!command.Validation.Any())
            {
                return new JsonResult() { Data = true, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                return new JsonResult() { Data = false, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        private KeyValuePair<string, byte[]> GetThumbnailData(HttpRequestBase request, UploadModel upload, int? height, int? width, bool crop = false)
        {
            var ext = Path.GetExtension(upload.Name);
            byte[] data;
            string extention;
            switch (ext.ToLowerInvariant())
            {
                case ".ppsx":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/ppDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".pps":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/ppDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".pdf":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/otherDoc.png")), height, width, crop);
                    extention = ".png";

                    break;

                case ".xls":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/excelDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".xlsx":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/excelDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".ppt":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/ppDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".pptx":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/ppDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".doc":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/wordDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".docx":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/wordDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".mp4":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/VideoDoc.png")), height, width, crop);
                    extention = ".png";
                    break;

                case ".jpeg":
                    data = GetThumbnailData(upload.Data, height, width, crop);
                    extention = ".jpeg";
                    break;

                case ".png":
                    data = GetThumbnailData(upload.Data, height, width, crop);
                    extention = ".png";
                    break;

                case ".gif":
                    data = GetThumbnailData(upload.Data, height, width, crop);
                    extention = ".gif";
                    break;

                case ".bmp":
                    data = GetThumbnailData(upload.Data, height, width, crop);
                    extention = ".bmp";
                    break;

                case ".jpg":
                    data = GetThumbnailData(upload.Data, height, width, crop);
                    extention = ".jpg";
                    break;
                case ".mp3":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/OR_AudioIcon_v3.png")), height, width, crop);
                    extention = ".png";
                    break;
                case ".wav":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/OR_AudioIcon_v2.jpg")), height, width, crop);
                    extention = ".png";
                    break;
                case ".ogg":
                    data = GetThumbnailData(GetStandardImageData(Url.Content("~/Content/images/OR_AudioIcon_v2.jpg")), height, width, crop);
                    extention = ".png";
                    break;
                default:
                    data = upload.Data;
                    extention = ext;
                    break;
            }

            return new KeyValuePair<string, byte[]>(extention, data);
        }

        private byte[] GetThumbnailData(byte[] data, int? height, int? width, bool crop = false, VirtuaCon.Percentage? cropFactor = null)
        {
            try
            {
                return VirtuaCon.Drawing.ImageUtil.Resize(data, width, height, crop, cropFactor);
            }
            catch (OutOfMemoryException)
            {
                return data;
            }
        }

        private byte[] GetStandardImageData(string path)
        {
            var p = path;

            if (p.StartsWith("~") || p.StartsWith("/"))
                p = Server.MapPath(p);

            return Utility.Convertor.BytesFromFilePath(p);
        }
    }

}