using Common.Command;
using Common.Data;
using Common.Query;
using Newtonsoft.Json;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Common.Web;
using VirtuaCon.Reporting;
using Ramp.Services.Helpers;
using System.Configuration;
using VirtuaCon.Reporting.Publishers;
using System.IO;
using Common.Report;
using ikvm.extensions;
using Ramp.Contracts.QueryParameter;
using Ramp.Services.QueryHandler;
using Web.UI.Code.Extensions;
using ICSharpCode.SharpZipLib.Zip;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using Common;
using Ramp.Contracts.QueryParameter.Catagories;
using Ramp.Contracts.Query.DocumentCategory;
using Common.Collections;
using Domain.Customer;
using Domain.Customer.Base;
using Domain.Customer.Models.Test;
using Ramp.Contracts.Command.Document;
using Ramp.Contracts.Command.DocumentUsage;
using Ramp.Contracts.Command.TestSession;
using Ramp.Contracts.CommandParameter.TestManagement;
using Ramp.Contracts.Query.Document;
using Ramp.Contracts.Query.TestSession;
using Ramp.Security.Authorization;
using VirtuaCon;
using System.Net.Http;
using System.Text;
using Web.UI.Helpers;
using Ramp.Contracts.Query.Upload;
using Domain.Customer.Models;
using Domain.Customer.Models.Memo;
using Domain.Customer.Models.TrainingManual;
using Domain.Customer.Models.CheckLists;
using Domain.Customer.Models.Forms;
using Domain.Customer.Models.Policy;
using iTextSharp.text;
using Ramp.Contracts.Query.TrainingManual;
using VirtuaCon.Reporting.Styles;
using Ramp.Contracts.Query.Test;
using VirtuaCon.Net.Dns;
using Ramp.Contracts.Query.CheckListChapterUserResult;
using Domain.Customer.Models.Document;
using Ramp.Contracts.Query.Policy;
using Ramp.Contracts.Query.Memo;

namespace Web.UI.Controllers
{
    public abstract class KnockoutListController<TSearchQuery, TListModel> : RampController
        where TSearchQuery : class, IPagedQuery
        where TListModel : IdentityModel<string>, new()
    {
        public virtual ActionResult Index([FromUri] TSearchQuery query)
        {
            ViewBag.Links = new Dictionary<string, string>()
                {
                    { "edit" , Url.Action("Edit") },
                    { "delete", Url.Action("Delete")},
                    { "index",Url.Action("Index")},
                    {"index:post", Url.Action("PostIndex") }
                };
            ViewBag.Mode = VirtuaCon.EnumUtility.GetFriendlyName<Mode>(Mode.List);
            var list = ExecuteQuery<TSearchQuery, IPaged<TListModel>>(query);
            Index_PostProcess(list.Items);
            return View(list);
        }
        public virtual void Index_PostProcess(IEnumerable<TListModel> listModel) { }
        public virtual JsonResult PostIndex([FromBody] TSearchQuery query)
        {
            ExecuteCommand(new UpdateConnectionStringCommand());
            if (!query.PageSize.HasValue)
                query.PageSize = 10;
            ViewBag.Mode = VirtuaCon.EnumUtility.GetFriendlyName<Mode>(Mode.List);
            var list = ExecuteQuery<TSearchQuery, IPaged<TListModel>>(query);
            Index_PostProcess(list.Items);
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        protected bool ValidateAndExecute<TCommand>(TCommand command)
        {
            var result = ExecuteCommand(command);
            if (result.Id == null)
            {
                return true;
            }
            if (result.Validation.Any())
            {
                result.Validation.ToList().ForEach(x => ModelState.AddModelError(x.MemberName, x.Message));
                return false;
            }
            return true;
        }

    }
    public abstract class KnockoutPagedListController<TSearchQuery, TListModel> : RampController
        where TSearchQuery : class, IPagedQuery
        where TListModel : IdentityModel<string>, new()
    {
        [System.Web.Mvc.HttpGet]
        public virtual ActionResult Index([FromUri] TSearchQuery query)
        {
            ExecuteCommand(new UpdateConnectionStringCommand());
            if (!query.PageSize.HasValue)
                query.PageSize = 10;
            ViewBag.Links = new Dictionary<string, string>()
                {
                    { "edit" , Url.Action("Edit") },
                    { "delete", Url.Action("Delete")},
                    { "index",Url.Action("Index")},
                    {"index:post", Url.Action("PostIndex") }
                };
            ViewBag.Mode = VirtuaCon.EnumUtility.GetFriendlyName<Mode>(Mode.List);
            query.EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument;
            var list = ExecuteQuery<TSearchQuery, IPaged<TListModel>>(query);
            Index_PostProcess(list.Items);
            return View(list);
        }
        public virtual JsonResult PostIndex([FromBody] TSearchQuery query)
        {
            ExecuteCommand(new UpdateConnectionStringCommand());
            if (!query.PageSize.HasValue)
                query.PageSize = 10;
            ViewBag.Mode = VirtuaCon.EnumUtility.GetFriendlyName<Mode>(Mode.List);
            query.EnableChecklistDocument = PortalContext.Current.UserCompany.EnableChecklistDocument;
            var list = ExecuteQuery<TSearchQuery, IPaged<TListModel>>(query);
            Index_PostProcess(list.Items);
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        public virtual void Index_PostProcess(IEnumerable<TListModel> listModel) { }

    }
    public abstract class KnockoutCRUDController<TSearchQuery, TListModel, TEntity, TModel, TCreateAndUpdateCommand>
        : KnockoutListController<TSearchQuery, TListModel>
        where TSearchQuery : class, IPagedQuery
        where TListModel : IdentityModel<string>, new()
        where TEntity : class
        where TModel : IdentityModel<string>, new()
        where TCreateAndUpdateCommand : IdentityModel<string>, new()
    {
        [System.Web.Mvc.HttpGet]
        public virtual ActionResult Edit(TCreateAndUpdateCommand command)
        {
            ViewBag.Links = new Dictionary<string, string>()
                    {
                        {"index", Url.Action("Index")},
                        {"save",Url.Action("Save")}
                    };
            ViewBag.Mode = VirtuaCon.EnumUtility.GetFriendlyName<Mode>(string.IsNullOrWhiteSpace(command.Id) ? Mode.Create : Mode.Edit);
            TModel model = null;
            if (typeof(TEntity).IsSubclassOf(typeof(CustomerDomainObject)))
                model = ExecuteQuery<FetchByIdQuery, TModel>(new FetchByIdQuery { Id = command.Id.ConvertToGuid() });
            else
                model = ExecuteQuery<FetchByIdQuery, TModel>(new FetchByIdQuery { Id = command.Id });
            if (model != null)
                Edit_PostProcess(model);
            return View(model);
        }
        public virtual void Edit_PostProcess(TModel model) { }

        [System.Web.Mvc.HttpPost]
        public virtual ActionResult Save(TCreateAndUpdateCommand command)
        {
            if (!ModelState.IsValid || !ValidateAndExecute(command))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ModelState.ToDictionary());
            }
            return Json(new { Success = true });
        }
        [System.Web.Mvc.HttpDelete]
        public virtual HttpStatusCodeResult Delete(string id)
        {
            if (!ValidateAndExecute(new DeleteByIdCommand<TEntity> { Id = id }))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
        }
    }
    public abstract class ReportController<TQuery, TDataModel> : ExportController<TQuery>
        where TQuery : class, IContextQuery, new()
    {
        public virtual ActionResult Index(TQuery query)
        {
            ViewBag.Links = new Dictionary<string, string>()
            {
                { "post", Url.Action("Post") },
                { "downloadPDF", Url.Action("DownloadPDF") },
                { "downloadEXCEL", Url.Action("DownloadEXCEL") }
            };
            var r = ExecuteQuery<TQuery, TDataModel>(query);
            PostProcess(r);
            return View(r);
        }
        protected virtual void PostProcess(TDataModel data) { }

        [System.Web.Mvc.HttpPost]
        public virtual ActionResult Post(TQuery query)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ModelState.ToDictionary());
            }
            var r = ExecuteQuery<TQuery, TDataModel>(query);
            PostProcess(r);
            return Json(r, JsonRequestBehavior.AllowGet);
        }
    }
    public abstract class ExportController<TQuery> : RampController where TQuery : class, IContextQuery
    {
        [System.Web.Mvc.HttpGet]
        public virtual ActionResult DownloadPDF([FromUri] TQuery query)
        {
            query.PortalContext = PortalContext.Current;
            var publisher = new PdfReportPublisher();
            if (query.AddOnrampBranding)
            {
                publisher.Page.ImagePosition = HorizontalPosition.BottomRight;
                using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(Server.MapPath("~/Content/images/imgOnrampFooter.png"))))
                using (var image =System.Drawing.Image.FromStream(stream))
                {
                    publisher.Page.Image = VirtuaCon.Drawing.ImageUtil.GetHighQualityImage(image, null, 30);
                }
                publisher.Page.PageNumberPosition = HorizontalPosition.BottomCenter;
            }
            return Publish(ExecuteQuery<TQuery, IExportModel>(query), publisher, "application/pdf", "pdf");
        }
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        [System.Web.Mvc.HttpGet]
        public virtual ActionResult DownloadEXCEL([FromUri] TQuery query)
        {
            query.PortalContext = PortalContext.Current;

            return Publish(ExecuteQuery<TQuery, IExportModel>(query), new ExcelReportPublisher(), "application/vnd.ms-excel", "xls");
        }
        protected virtual ActionResult Publish(IExportModel model, IReportDocumentWriter publisher, string contentType, string fileExtension)
        {
            var stream = new MemoryStream();
            var attachStream = new MemoryStream();

            try
            {
                var portalContext = PortalContext.Current;
                model.Title = model.Title.RemoveSpecialCharacters() + "." + fileExtension;

                publisher.Write(model.Document, stream);
                string filePaths = null;
                stream.Position = 0;
                if (portalContext != null)
                {
                    Response.AddHeader("filename", model.Title);
                    PortalContext.Current = portalContext;
                    filePaths = System.Web.HttpContext.Current.Server.MapPath(Path.Combine("~/Download/", model.Title));
                    System.IO.File.WriteAllBytes(filePaths, stream.ToArray());
                }
                //  stream.CopyTo(attachStream);

                if (!string.IsNullOrEmpty(model.Recepients))
                {
                    var msg = new SendEmail();
                    msg.addAttachmentSendEmail(stream, model.Recepients, model.Title, contentType, filePaths);

                    if (!string.IsNullOrEmpty(filePaths))
                    {
                        FileInfo file = new FileInfo(filePaths);
                        if (file.Exists)//check file exsit or not  
                        {
                            file.Delete();
                        }
                    }
                }
                return new FileStreamResult(stream, contentType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                stream.Dispose();
                attachStream.Dispose();
            }
        }

        public void SaveStreamAsFile(string filePath, Stream inputStream, string fileName, string contentType)
        {
            //DirectoryInfo info = new DirectoryInfo(filePath);
            //if (!info.Exists)
            //{
            //    info.Create();
            //}

            //string path = Path.Combine(filePath, fileName);
            //using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
            //{
            //    inputStream.CopyTo(outputFileStream);
            //}

            //var filePaths = Server.MapPath(Path.Combine(filePath, fileName));
            //var pdfFileBytes = GetBytesFromFile(filePaths);
            // File(pdfFileBytes, contentType, fileName );


        }
        public abstract ActionResult Zip([FromUri] TQuery query);
    }

    //Limited to 2^32 byte files (4.2 GB)
    //public  byte[] GetBytesFromFile(string fullFilePath)
    //{

    //    FileStream fs = null;

    //    try
    //    {
    //        fs = File.OpenRead(fullFilePath);
    //        var bytes = new byte[fs.Length];
    //        fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
    //        return bytes;
    //    }
    //    catch (Exception ex) {
    //        throw ex;
    //    }
    //    finally
    //    {
    //        if (fs != null)
    //        {
    //            fs.Close();
    //            fs.Dispose();
    //        }
    //    }

    //}


    public abstract class KnockoutDocumentController<TSearchQuery, TListModel, TEntity, TModel, TCreateAndUpdateCommand> :
        KnockoutListController<TSearchQuery, TListModel>
        where TSearchQuery : class, IPagedQuery
        where TListModel : IdentityModel<string>, new()
        where TEntity : class
        where TModel : DocumentListModel, new()
        where TCreateAndUpdateCommand : IdentityModel<string>, new()
    {
        [System.Web.Mvc.HttpGet]
        public virtual ActionResult Edit(TCreateAndUpdateCommand command)
        {
            ExecuteCommand(new UpdateConnectionStringCommand());
            ViewBag.Mode = VirtuaCon.EnumUtility.GetFriendlyName<Mode>(string.IsNullOrWhiteSpace(command.Id) ? Mode.Create : Mode.Edit);
            if (string.IsNullOrWhiteSpace(command.Id)) // Create
            {
                var documentsRemaining =
                    ExecuteQuery<DocumentsRemainingQuery, int>(
                        new DocumentsRemainingQuery { CompanyId = PortalContext.Current.UserCompany.Id });
                if (documentsRemaining <= 0)
                {
                    return RedirectToAction("Index", "Document", new { Area = "" });
                }
            }
            ViewBag.Links = new Dictionary<string, string>()
                    {
                        {"index", Url.Action("Index","Document",new { Area = ""})},
                        {"save",Url.Action("Save")},
                        {"clone", Url.Action("Clone")},
                        {"preview", Url.Action("preview")},
                        {"generateId",Url.ActionLink<DefaultController>(a => a.GetGenerateId())},
                        {"contentTools:PostFromContentToolsInitial",Url.Action("PostFromContentToolsInitial", "Upload", new { Area = "" }) },
                        {"contentTools:PostFromContentToolsCommit", Url.Action("PostFromContentToolsCommit", "Upload", new { Area = "" }) },
                        {"contentTools:RotateImage", Url.Action("RotateImage", "Upload", new { Area = "" }) },
                        {"upload:posturl", Url.Action("Post","Upload",new { Area = ""}) },
                        {"category:jsTree",Url.Action("JsTree","Category",new { Area = ""}) }
                    };
            ViewBag.TrainingActivityLogginEnabled = PortalContext.Current?.UserCompany?.EnableTrainingActivityLoggingModule;
            ViewBag.Categories = ExecuteQuery<DocumentCategoryListQuery, IEnumerable<JSTreeViewModel>>(new DocumentCategoryListQuery());
            var model = ExecuteQuery<FetchByIdQuery, TModel>(new FetchByIdQuery { Id = command.Id });
            if (model != null)
            {
                Edit_PostProcess(model);
                if (model.DocumentType == DocumentType.Unknown)
                    model.DocumentType = GetDocumentType();
            }

            return View(model);
        }
        public virtual void Edit_PostProcess(TModel model, string companyId = null, DocumentUsageStatus? status = null,string userid=null) { }

        [System.Web.Mvc.HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult Save(TCreateAndUpdateCommand command)
        {

            ExecuteCommand(new UpdateConnectionStringCommand());
            if (!ModelState.IsValid || !ValidateAndExecute(command))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ModelState.ToDictionary());
            }
            return Json(new { Success = true });
        }
        [System.Web.Mvc.HttpDelete]
        public virtual HttpStatusCodeResult Delete(string id)
        {
            ExecuteCommand(new UpdateConnectionStringCommand());
            if (!ValidateAndExecute(new DeleteByIdCommand<TEntity> { Id = id }))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExecuteCommand(new UnassignDocumentFromAllUsersCommand { DocumentId = id, Type = typeof(TEntity) });
            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
        }

        [System.Web.Mvc.HttpDelete]
        public virtual HttpStatusCodeResult DeleteSection(SectionInfo info)
        {
            ExecuteCommand(new UpdateConnectionStringCommand());
            bool isValidate = false;
            switch (info.Type)
            {
                case DocumentType.AcrobatField:
                    isValidate = ValidateAndExecute(new DeleteByIdCommand<AcrobatFieldContentBox> { Id = info.ID });
                    break;
                case DocumentType.Checklist:
                    isValidate = ValidateAndExecute(new DeleteByIdCommand<CheckListChapter> { Id = info.ID });
                    break;
                case DocumentType.Memo:
                    isValidate = ValidateAndExecute(new DeleteByIdCommand<MemoContentBox> { Id = info.ID });
                    break;
                case DocumentType.Policy:
                    isValidate = ValidateAndExecute(new DeleteByIdCommand<PolicyContentBox> { Id = info.ID });
                    break;
                case DocumentType.Test:
                    isValidate = ValidateAndExecute(new DeleteByIdCommand<TestQuestion> { Id = info.ID });
                    break;
                case DocumentType.TrainingManual:
                    isValidate = ValidateAndExecute(new DeleteByIdCommand<TrainingManualChapter> { Id = info.ID });
                    break;
                case DocumentType.Form:
                    isValidate = ValidateAndExecute(new DeleteByIdCommand<FormChapter> { Id = info.ID });
                    break;
            }

            if (!isValidate)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //ExecuteCommand(new UnassignDocumentFromAllUsersCommand { DocumentId = info.ID, Type = typeof(TEntity) });
            return new HttpStatusCodeResult(HttpStatusCode.Accepted);
        }

        [System.Web.Mvc.HttpPost]
        public virtual ActionResult Clone(CloneCommand<TEntity> command)
        {
            var documentsRemaining =
                ExecuteQuery<DocumentsRemainingQuery, int>(
                    new DocumentsRemainingQuery { CompanyId = PortalContext.Current.UserCompany.Id });
            if (documentsRemaining <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var documentId = command.Id;
            ExecuteCommand(new UpdateConnectionStringCommand());
            command.SourceCompanyId = command.SourceCompanyId ?? PortalContext.Current?.UserCompany?.Id.ToString();
            command.TargetCompanyId = command.TargetCompanyId ?? PortalContext.Current?.UserCompany?.Id.ToString();
            if (!ValidateAndExecute(command))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (command.NewVersion)
                ExecuteCommand(new UnassignDocumentFromAllUsersCommand { DocumentId = documentId, Type = typeof(TEntity) });
            Response.StatusCode = (int)HttpStatusCode.Accepted;
            return new JsonResult { Data = command.Id };
        }

        [OutputCache(NoStore = true, Duration = 0)]
        [System.Web.Mvc.HttpGet]
        public virtual ActionResult Preview([FromUri] object id, string chapterID=null, string companyId = null, string checkUser = null, bool isGlobal = false, DocumentUsageStatus? status = null)
        {
            if (companyId != null)
            {
                ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = companyId });
            }

            ViewBag.Links = new Dictionary<string, string>()
            {
                {"index", Url.Action("MyDocuments", "Document", new {Area = ""})},
                {"learnMore", Url.Action("GlobalDocuments", "Document", new {Area = ""})},
                {"preview",Url.Action("CompleteChaptersUserResult","CustomDocument", new { Area = "" })},
                {"userFeedback:save", Url.Action("Save", "UserFeedback", new {Area = ""})},
                {"save",Url.Action("Save")},
                {"userFeedback:types", Url.Action("Types", "UserFeedback", new {Area = ""})},
                {"userFeedback:contentTypes", Url.Action("ContentTypes", "UserFeedback", new {Area = ""})},
                {"poll", Url.Action("TrackUsage", typeof(TEntity).Name, new {Area = ""})},
                {"inProgress", Url.Action("InProgress", "Test", new {Area = ""})},
                {"upload:posturl", Url.Action("Post","Upload",new { Area = ""}) },
                {"upload:SaveSignature", Url.Action("SaveSignature","Upload",new { Area = ""}) },
                {"category:jsTree",Url.Action("JsTree","Category",new { Area = ""}) },
                {"generateId",Url.ActionLink<DefaultController>(a => a.GetGenerateId())},
                {"contentTools:PostFromContentToolsInitial",Url.Action("PostFromContentToolsInitial", "Upload", new { Area = "" }) },
                {"contentTools:PostFromContentToolsCommit", Url.Action("PostFromContentToolsCommit", "Upload", new { Area = "" }) },
                {"contentTools:RotateImage", Url.Action("RotateImage", "Upload", new { Area = "" }) },
            };
            TModel model = new TModel();
            if (!string.IsNullOrEmpty(chapterID))
            {
                model = ExecuteQuery<FetchByCustomIdQuery , TModel>(new FetchByCustomIdQuery { Id = id , chapterId=chapterID, userId = checkUser});
            }
            else
            {
                model = ExecuteQuery<FetchByIdQuery, TModel>(new FetchByIdQuery { Id = id, userId = checkUser });
            }
            if (model != null)
                Preview_PostProcess(model, companyId, checkUser, isGlobal, status);

            if (Thread.CurrentPrincipal.IsInStandardUserRole())
            {
                var userId = Thread.CurrentPrincipal.GetId().ToString();
                var documentType = GetDocumentType();

                //if (!ExecuteQuery<DocumentAssignedToUserQuery, bool>(new DocumentAssignedToUserQuery {
                //	DocumentId = id.ToString(),
                //	DocumentType = documentType,
                //	UserId = userId
                //}))
                //	return RedirectToAction("AccessDenied", "Account");
                ViewBag.GloballyAccessed = isGlobal;
                var testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
                {
                    UserId = userId
                });
                if (testSession != null)
                {
                    if ((testSession.EnableTimer && testSession.TimeLeft == TimeSpan.Zero) || (testSession.DocumentStatus.HasValue && testSession.DocumentStatus.Value == DocumentStatus.Recalled))
                    {
                        var testResult = ExecuteQuery<FetchByIdQuery<Test>, TestResultModel>(new FetchByIdQuery<Test> { Id = testSession.CurrentTestId });
                        var createTestResultCommand = JsonConvert.DeserializeObject<CreateTestResultCommand>(JsonConvert.SerializeObject(testResult)); // hack
                        createTestResultCommand.PortalContext = PortalContext.Current;
                        createTestResultCommand.UserId = userId;
                        createTestResultCommand.TimeLeft = testSession.TimeLeft;
                        createTestResultCommand.IsGlobalAccessed = isGlobal;
                        ExecuteCommand(createTestResultCommand);
                        ExecuteCommand(new TestSessionEndCommand
                        {
                            UserId = userId
                        });
                    }
                    else if (documentType != DocumentType.Test ? !testSession.OpenTest : testSession.CurrentTestId != id.ToString())
                        return RedirectToAction("InProgress", "Test");
                }

                if (documentType != DocumentType.Test)
                {
                    var viewDate = DateTime.UtcNow;
                    viewDate = DateTime.SpecifyKind(DateTime.ParseExact(viewDate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null), DateTimeKind.Utc); // remove milliseconds
                    ExecuteCommand(new CreateOrUpdateDocumentUsageCommand
                    {
                        DocumentId = id.ToString(),
                        DocumentType = documentType,
                        UserId = userId,
                        ViewDate = viewDate,
                        IsGlobalAccessed = isGlobal
                    });
                    ViewBag.StartTime = viewDate.ToString();
                    ViewBag.TrackingInterval = ConfigurationManager.AppSettings["DocumentTrackingInterval"];
                }
            }

            ExecuteCommand(new UpdateConnectionStringCommand());

            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            return View(model);
        }

        protected virtual void Preview_PostProcess(TModel model, string companyId, string checkUser = null, bool isGlobal = false, DocumentUsageStatus? status = null) { }

        [System.Web.Mvc.HttpGet]
        public virtual ActionResult Print(string id, bool addOnrampBranding = false)
        {
            var userId = Thread.CurrentPrincipal.GetId().ToString();
            var memoryStream = new MemoryStream();
            var zipStream = new ZipOutputStream(memoryStream);
            zipStream.SetLevel(3);

            var publisher = new PdfReportPublisher();

            if (addOnrampBranding)
            {
                publisher.Page.ImagePosition = HorizontalPosition.BottomRight;
                using (var ms = new MemoryStream(System.IO.File.ReadAllBytes(Server.MapPath("~/Content/images/imgOnrampFooter.png"))))
                using (var image =System.Drawing.Image.FromStream(ms))
                {
                    publisher.Page.Image = VirtuaCon.Drawing.ImageUtil.GetHighQualityImage(image, null, 30);
                }
                publisher.Page.PageNumberPosition = HorizontalPosition.BottomCenter;
            }

            IExportModel model;
            try
            {

                 model = ExecuteQuery<PrintDocumentQuery<TEntity>, IExportModel>(new PrintDocumentQuery<TEntity>
                {
                    Id = id,
                    AddOnrampBranding = addOnrampBranding,
                    PortalContext = PortalContext.Current,
                    userId = (PortalContext.Current.UserDetail.CustomerRoles[0].ToString() == "StandardUser") ? PortalContext.Current.UserDetail.Id.ToString() : null
                });
            }
            catch (Exception)
            {
                 model = ExecuteQuery<PrintDocumentQuery<TEntity>, IExportModel>(new PrintDocumentQuery<TEntity>
                {
                    Id = id,
                    AddOnrampBranding = addOnrampBranding,
                    PortalContext = PortalContext.Current,
                    userId = userId
                });
            }
            model.Title = model.Title.RemoveSpecialCharacters() + ".pdf";
            var entry = new ZipEntry(ZipEntry.CleanName(model.Title));
            entry.DateTime = DateTime.Now;
            zipStream.PutNextEntry(entry);
            new PdfReportPublisher().Write(model.Document, zipStream);
            zipStream.CloseEntry();

            var trainingManual = ExecuteQuery<FetchByIdQuery, TrainingManualModel>(new FetchByIdQuery { Id = id });
            var memo = ExecuteQuery<FetchByIdQuery, MemoModel>(new FetchByIdQuery { Id = id });
            var policy = ExecuteQuery<FetchByIdQuery, PolicyModel>(new FetchByIdQuery { Id = id });
            var test = ExecuteQuery<FetchByIdQuery, TestModel>(new FetchByIdQuery { Id = id });
            var checkList = ExecuteQuery<FetchByIdQuery, CheckListModel>(new FetchByIdQuery { Id = id });
            var customDocument = ExecuteQuery<FetchByIdQuery, CustomDocumentModel>(new FetchByIdQuery { Id = id });
           
            var assignedDocument = ExecuteQuery<DocAssignedToUserQuery, AssignedDocumentModel>(new DocAssignedToUserQuery { UserId = userId, DocumentId = id});


            if ((trainingManual != null && trainingManual.ContentModels.Count() > 0) || customDocument.TMContentModels.Any())
            {

                var chapters = customDocument.TMContentModels.Any() ? customDocument.TMContentModels: trainingManual.ContentModels;

                foreach (var item in chapters)
                {
                    if (assignedDocument != null)
                    { 
                        var TrainingManualChapterUserUpload = ExecuteQuery<TrainingManualChapterUserResultQuery, List<UploadResultViewModel>>(new TrainingManualChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TrainingManualChapterId = item.Id, UserId = userId });

                        item.StandardUserAttachments = TrainingManualChapterUserUpload.Where(z => !z.isSignOff).ToList();

                        foreach (var attachment in item.StandardUserAttachments)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }
                        var SigntureData = TrainingManualChapterUserUpload.Where(z => z.isSignOff).ToList();
                        foreach (var attachment in SigntureData)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                    else  if (item.Attachments.Any())
                    {
                        foreach (var attachment in item.Attachments)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }
                    }
                }
            }
            if ((memo != null && memo.ContentModels.Count() > 0)|| customDocument.MemoContentModels.Any())
            {
                var chapters = customDocument.MemoContentModels.Any()? customDocument.MemoContentModels:memo.ContentModels;

                foreach (var item in chapters)
                {
                    if (assignedDocument != null)
                    {
                        var memoChapterUserUpload = ExecuteQuery<MemoChapterUserResultQuery, List<UploadResultViewModel>>(new MemoChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, MemoChapterId = item.Id, UserId = userId });

                        item.StandardUserAttachments =memoChapterUserUpload .Where(z => !z.isSignOff).ToList();

                        foreach (var attachment in item.StandardUserAttachments)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }
                        var SigntureData = memoChapterUserUpload.Where(z => z.isSignOff).ToList();
                        foreach (var attachment in SigntureData)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                    else if(item.Attachments.Any())
                    {
                        foreach (var attachment in item.Attachments)
                        {

                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }

                    }
                }
            }
            if ((policy != null && policy.ContentModels.Count() > 0) || customDocument.PolicyContentModels.Any())
            {
                var chapters = customDocument.PolicyContentModels.Any()? customDocument.PolicyContentModels:policy.ContentModels;

                foreach (var item in chapters)
                {
                    if (assignedDocument != null)
                    {
                        var PolicyContentBoxUserUpload = ExecuteQuery<PolicyContentBoxUserResultQuery, List<UploadResultViewModel>>(new PolicyContentBoxUserResultQuery { AssignedDocumentId = assignedDocument.Id, PolicyContentBoxId = item.Id, UserId = userId });

                        item.StandardUserAttachments =  PolicyContentBoxUserUpload.Where(z => !z.isSignOff).ToList();

                        foreach (var attachment in item.StandardUserAttachments)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }
                        var SigntureData = PolicyContentBoxUserUpload .Where(z => z.isSignOff).ToList();
                        foreach (var attachment in SigntureData)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                    else if (item.Attachments.Any())
                    {
                        foreach (var attachment in item.Attachments)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);
                        }

                    }
                }
            }
            if ((checkList != null && checkList.ContentModels.Count() > 0) || customDocument.CLContentModels.Any())
            {
                var chapters = customDocument.CLContentModels.Any() ? customDocument.CLContentModels : checkList.ContentModels;

                foreach (var item in chapters)
                {
                    if (assignedDocument != null)
                    {
                        var CheckListChapterUserUpload = ExecuteQuery<CheckListChapterUserResultQuery, List<UploadResultViewModel>>(new CheckListChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, CheckListChapterId = item.Id, UserId = userId });

                        item.StandardUserAttachments = CheckListChapterUserUpload.Where(z => !z.isSignOff).ToList();

                        foreach (var attachment in item.StandardUserAttachments)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }
                        var SigntureData = CheckListChapterUserUpload.Where(z => z.isSignOff).ToList();
                        foreach (var attachment in SigntureData)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                    else if (item.Attachments.Any())
                    {
                        foreach (var attachment in item.Attachments)
                        {

                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }

                    }
                }
            }
            if ((test != null && test.ContentModels.Count() > 0) || customDocument.TestContentModels.Any())
            {

                var chapters =customDocument.TestContentModels.Any()? customDocument.TestContentModels:  test?.ContentModels;

                foreach (var item in chapters)
                {
                    if (assignedDocument != null)
                    {
                        var TestChapterUserUpload = ExecuteQuery<TestChapterUserResultQuery, List<UploadResultViewModel>>(new TestChapterUserResultQuery { AssignedDocumentId = assignedDocument.Id, TestChapterId = item.Id, UserId = userId });

                        item.StandardUserAttachments = TestChapterUserUpload.Where(z => !z.isSignOff).ToList();

                        foreach (var attachment in item.StandardUserAttachments)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }
                        var SigntureData = TestChapterUserUpload.Where(z => z.isSignOff).ToList();
                        foreach (var attachment in SigntureData)
                        {
                            ZipFile(attachment.Id, zipStream, attachment.Name);
                        }
                    }
                    else if (item.Attachments.Any())
                    {
                        foreach (var attachment in item.Attachments)
                        {

                            ZipFile(attachment.Id, zipStream, attachment.Name);

                        }

                    }
                }
            }

            zipStream.IsStreamOwner = false;
            zipStream.Close();
            memoryStream.Position = 0;
            Response.AddHeader("filename", $"{model.Title.Replace(".pdf", "")}.zip");
            return new FileStreamResult(memoryStream, "application/octet-stream");
        }
        #region written by ashok to save file in zip and download zip
        public void ZipFile(string id, ZipOutputStream zipStream, string name)
        {
            var upload = ExecuteQuery<FetchUploadByIdQuery, Upload>(new FetchUploadByIdQuery { Id = id });

            if (upload != null && upload.Data.Any())
            {

                using (var stream = new MemoryStream(upload.Data))
                {
                    var attachmentEntry = new ZipEntry(ZipEntry.CleanName(name))
                    {
                        Size = stream.Length
                    };
                    zipStream.PutNextEntry(attachmentEntry);
                    byte[] buffer = new byte[4096];
                    int count = stream.Read(buffer, 0, buffer.Length);
                    while (count > 0)
                    {
                        zipStream.Write(buffer, 0, count);
                        count = stream.Read(buffer, 0, buffer.Length);
                        if (!Response.IsClientConnected)
                        {
                            break;
                        }
                    }
                    zipStream.CloseEntry();
                };
            }

        }
        #endregion

        [System.Web.Mvc.HttpPost]
        public virtual ActionResult TrackUsage(string documentId, DateTime startTime, int duration)
        {

            var userId = Thread.CurrentPrincipal.GetId().ToString();
            var durationTimeSpan = TimeSpan.FromSeconds(duration);
            ExecuteCommand(new CreateOrUpdateDocumentUsageCommand()
            {
                UserId = userId,
                DocumentId = documentId,
                DocumentType = GetDocumentType(),
                ViewDate = startTime,
                Duration = durationTimeSpan
            });

            Response.ContentType = "text/plain";

            var testSession = ExecuteQuery<TestSessionQuery, TestSessionViewModel>(new TestSessionQuery
            {
                UserId = userId
            });
            if (testSession != null && !testSession.OpenTest)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private DocumentType GetDocumentType()
        {
            var documentType = DocumentType.Unknown;
            switch (typeof(TEntity).Name)
            {
                case "TrainingManual":
                    documentType = DocumentType.TrainingManual;
                    break;
                case "Test":
                    documentType = DocumentType.Test;
                    break;
                case "Policy":
                    documentType = DocumentType.Policy;
                    break;
                case "Memo":
                    documentType = DocumentType.Memo;
                    break;
                case "CheckList":
                    documentType = DocumentType.Checklist;
                    break;
            }

            return documentType;
        }
    }
    public abstract class FeedbackController<TQuery, TListModel, TModel, TEntity, TCreateAndUpdateCommand> : KnockoutListController<TQuery, TListModel>
        where TQuery : class, IPagedQuery
        where TListModel : IdentityModel<string>, new()
        where TEntity : class
        where TModel : IdentityModel<string>, new()
        where TCreateAndUpdateCommand : IdentityModel<string>, new()
    {
        [System.Web.Mvc.HttpPost]
        public ActionResult Save([FromBody] TCreateAndUpdateCommand command)
        {
            if (!ModelState.IsValid || !ValidateAndExecute(command))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ModelState.ToDictionary());
            }
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpGet]
        public virtual ActionResult Preview(FetchByIdQuery query)
        {
            ViewBag.Links = new Dictionary<string, string>()
                    {
                        {"index", Url.Action("Index")}
                    };
            var model = ExecuteQuery<FetchByIdQuery, TModel>(query);
            if (model != null)
                Edit_PostProcess(model);
            return View(model);
        }
        public virtual void Edit_PostProcess(TModel model) { }
    }
}