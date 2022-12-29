using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using Common.Command;
using Common.Query;
using ICSharpCode.SharpZipLib.Zip;
using Ramp.Contracts.Query.Reporting;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter.User;
using Ramp.Contracts.ViewModel;
using Domain.Customer.Models.ScheduleReport;
using VirtuaCon.Reporting;
using VirtuaCon.Reporting.Publishers;
using Web.UI.Code.Extensions;
using Web.UI.Controllers;
using Ramp.Security.Authorization;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.Query.Report;
using Domain.Customer;

namespace Web.UI.Areas.Reporting.Controllers {
	public class PointsStatementController : ExportController<PointsStatementExportQuery>
    {
        [System.Web.Mvc.HttpGet]
        public ActionResult Index()
        {

			var customerCompanyUserQueryParameter = new CustomerCompanyUserQueryParameter
			{
				CompanyId = PortalContext.Current.UserCompany.Id,
				CompanyName = PortalContext.Current.UserCompany.CompanyName,
				UserId = Guid.Empty,
				LoggedInUserId = SessionManager.GetCurrentlyLoggedInUserId()
			};
			CompanyUserViewModel companyUserViewModel =
				ExecuteQuery<CustomerCompanyUserQueryParameter, CompanyUserViewModel>(customerCompanyUserQueryParameter);
			var datas = companyUserViewModel.UserList.Where(z => z.Department != null).Select(z => z.Department).Distinct().ToList();
			ViewBag.Departments = datas;

			var query = new PointsStatementQuery { WithData = false };
            query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole()
                ? (Guid?) null
                : PortalContext.Current.UserCompany.Id;
			query.EnableGlobalAccessDocuments = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
			var vm = ExecuteQuery<PointsStatementQuery, PointsStatementViewModel>(query);

			ViewBag.Groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(
				new AllGroupsByCustomerAdminQueryParameter {
					CompanyId = PortalContext.Current.UserCompany.Id
				}).Select(g => new SerializableSelectListItem {
					Value = g.GroupId.ToString(),
					Text = g.Title
				}).OrderBy(x => x.Text);

			vm.Data = new List<PointsStatementViewModel.DataItem>();
            return View(vm);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Index(PointsStatementQuery query)
        {
            query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole()
                ? (Guid?) null
                : PortalContext.Current.UserCompany.Id;

            if (query.CompanyId != null)
                ExecuteCommand(new UpdateConnectionStringCommand { CompanyId = query.CompanyId.Value.ToString() });
			var GlobalAccess = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
			query.EnableGlobalAccessDocuments = GlobalAccess;
			var vm = ExecuteQuery<PointsStatementQuery, PointsStatementViewModel>(query);

            vm.SelectedDocumentTypes = query.DocumentTypes?.Cast<int>().ToArray() ?? new int[] {};

			var records = vm.Data.GroupBy(c => new { c.User.Id, c.User.FullName }).Select(c=>c.FirstOrDefault()).ToList();
			ViewBag.Records =records;

			if (query.GroupId != null) {

				var groups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(
				new AllGroupsByCustomerAdminQueryParameter {
					CompanyId = PortalContext.Current.UserCompany.Id
				}).Select(g => new SerializableSelectListItem {
					Value = g.GroupId.ToString(),
					Text = g.Title
				}).OrderBy(f => f.Text);

				//List<string> ng = new List<string>();

				//foreach (var q in query.GroupId) {
				//	foreach (var r in groups) {
				//		if (r.Value == q.ToString()) {
				//			ng.Add(r.Text);
				//		}
				//	}
				//}

				var t = String.Join(",", query.GroupId);

				var x = vm.Data.Where(u => u.User.GroupTitle.Contains(t)).ToList();
				vm.Data = x;
			}


			ExecuteCommand(new UpdateConnectionStringCommand());

            return PartialView("_PointsStatementPartialView", vm);
        }

        public override ActionResult DownloadEXCEL([FromUri]PointsStatementExportQuery query)
        {
            query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole()
                ? (Guid?)null
                : PortalContext.Current.UserCompany.Id;
			var GlobalAccess = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
			query.EnableGlobalAccessDocuments = GlobalAccess;
			
			return base.DownloadEXCEL(query);
        }


		[System.Web.Mvc.HttpGet]
		[System.Web.Mvc.AllowAnonymous]
		public ActionResult DownloadEXCELZip([FromUri]PointsStatementExportQuery query) {
			query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole()
				   ? (Guid?)null
				   : PortalContext.Current.UserCompany.Id;
			var GlobalAccess = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
			query.EnableGlobalAccessDocuments = GlobalAccess;

			var memoryStream = new MemoryStream();
			var zipStream = new ZipOutputStream(memoryStream);
			zipStream.SetLevel(3);

			foreach (var userId in query.UserIds) {

				var userDetail = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = userId });

				#region for xls point
				
				var pointReport = query;
				pointReport.AddOnrampBranding = true;
				pointReport.PortalContext = PortalContext.Current;
				pointReport.UserIds = new List<string> { userId };
				var model = ExecuteQuery<PointsStatementExportQuery, Common.Report.IExportModel>(pointReport);

				if (string.IsNullOrEmpty(query.ToggleFilter))
					query.ToggleFilter = model.Title;

				model.Title = userDetail.UserName.Trim() + "/" + userDetail.UserName.Trim().Replace("/", "") + ".xls";
				var stream = new MemoryStream();
				IReportDocumentWriter publisher = new ExcelReportPublisher();
				publisher.Write(model.Document, stream);
				stream.Position = 0;

				stream.Position = 0;
				var t = stream.ToArray();

				SaveFile(zipStream, model.Title, t);

				#endregion
							
			}

			zipStream.IsStreamOwner = false;
			zipStream.Close();
			memoryStream.Position = 0;
			var fileName =  "PointStatement.zip";
			Response.AddHeader("filename", fileName);

			var filePaths = Server.MapPath(Path.Combine("~/Download/", fileName));
			System.IO.File.WriteAllBytes(filePaths, memoryStream.ToArray());
			new Ramp.Services.Helpers.SendEmail().addAttachmentSendEmail(memoryStream, query.Recepients, fileName, "application/octet-stream", filePaths);
			FileInfo file = new FileInfo(filePaths);
			if (file.Exists)
			{
				file.Delete();
			}
			
			return new FileStreamResult(memoryStream, "application/octet-stream");
		}




		[System.Web.Http.HttpGet]
		[System.Web.Mvc.AllowAnonymous]
		public void SchedulePointStatement(string Occurance)
		{
			var getAllReport = ExecuteQuery<FetchAllScheduleReportQuery, List<ScheduleReportModel>>(new FetchAllScheduleReportQuery { }).Where(z => z.Occurences == Occurance && z.ReportAssignedId == "4").ToList();
			foreach (var d in getAllReport)
			{
				var data = ExecuteQuery<FetchByIdQuery, ScheduleReportVM>(new FetchByIdQuery
				{
					Id = d.Id
				});
				DocumentType[] documentsType = null;
				ReportParam.ReturnDocumentType(data.Params, out documentsType);
				PointsStatementExportQuery query = new PointsStatementExportQuery
				{
					ToggleFilter = "Employee Code,User,Category,Document Type,Title,Date,Access,Result,Points",
					UserIds = ReportParam.ReturnParams(data.Params, "Users"),
					//GroupId =  ReportParam.ReturnParams(data.Params, "Groups"),                    
					DocumentTypes = documentsType,
					ScheduleName = data.ScheduleName,
					Recepients = data.RecipientsList,
					FromDate = ReportParam.FromDate,
					ToDate = ReportParam.ToDate,
				};

				ExportEXCELZip(query);
			}
		}








		[System.Web.Mvc.AllowAnonymous]
		public void ExportEXCELZip(PointsStatementExportQuery query)
		{
			//query.ProvisionalCompanyId = Thread.CurrentPrincipal.IsInGlobalAdminRole()
			//       ? (Guid?)null
			//       : PortalContext.Current.UserCompany.Id;
			//var GlobalAccess = PortalContext.Current.UserCompany.EnableGlobalAccessDocuments;
			//query.EnableGlobalAccessDocuments = GlobalAccess;
			string filePaths = null;
			var memoryStream = new MemoryStream();
			var zipStream = new ZipOutputStream(memoryStream);
			zipStream.SetLevel(3);

			foreach (var userId in query.UserIds)
			{

				var userDetail = ExecuteQuery<FindUserByIdQuery, UserModelShort>(new FindUserByIdQuery { Id = userId });

				#region for xls point

				var pointReport = query;
				pointReport.AddOnrampBranding = true;
				//   pointReport.PortalContext = PortalContext.Current;
				pointReport.UserIds = new List<string> { userId };
				var model = ExecuteQuery<PointsStatementExportQuery, Common.Report.IExportModel>(pointReport);

				if (string.IsNullOrEmpty(query.ToggleFilter))
					query.ToggleFilter = model.Title;

				model.Title = userDetail.UserName.Trim() + "/" + userDetail.UserName.Trim().Replace("/", "") + ".xls";
				var stream = new MemoryStream();
				IReportDocumentWriter publisher = new ExcelReportPublisher();
				publisher.Write(model.Document, stream);
				stream.Position = 0;

				stream.Position = 0;
				var t = stream.ToArray();

				SaveFile(zipStream, model.Title, t);

				#endregion

			}

			zipStream.IsStreamOwner = false;
			zipStream.Close();
			memoryStream.Position = 0;
			var fileName = "PointStatement.zip";
			if (PortalContext.Current != null)
			{
				Response.AddHeader("filename", fileName);

				filePaths = Server.MapPath(Path.Combine("~/Download/", fileName));
				System.IO.File.WriteAllBytes(filePaths, memoryStream.ToArray());
			}
			if (!string.IsNullOrEmpty(query.Recepients))
			{
				new Ramp.Services.Helpers.SendEmail().addAttachmentSendEmail(memoryStream, query.Recepients, fileName, "application/octet-stream", filePaths);
				if (!string.IsNullOrEmpty(filePaths))
				{
					FileInfo file = new FileInfo(filePaths);

					if (file.Exists)
					{
						file.Delete();
					}
				}
			}


		}

		#region written by ashok to save file in zip and download zip
		[System.Web.Mvc.AllowAnonymous]
		public void SaveFile(ZipOutputStream zipStream, string name, byte[] data) {
			using (var stream = new MemoryStream(data)) {
				var attachmentEntry = new ZipEntry(ZipEntry.CleanName(name)) {
					Size = stream.Length
				};
				zipStream.PutNextEntry(attachmentEntry);
				byte[] buffer = new byte[4096];
				int count = stream.Read(buffer, 0, buffer.Length);
				while (count > 0) {
					zipStream.Write(buffer, 0, count);
					count = stream.Read(buffer, 0, buffer.Length);
					if (!Response.IsClientConnected) {
						break;
					}
				}
				zipStream.CloseEntry();
			};
		}
		#endregion

		public override ActionResult Zip(PointsStatementExportQuery query)
        {
            throw new NotImplementedException();
        }
    }
}