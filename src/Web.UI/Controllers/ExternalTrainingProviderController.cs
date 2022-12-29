using Common.Query;
using Domain.Customer.Models;
using Ramp.Contracts.CommandParameter.ExternalTrainingProvider;
using Ramp.Contracts.CommandParameter.Upload;
using Ramp.Contracts.QueryParameter.ExternalTrainingProvider;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.UI.Code.ActionFilters;
using Web.UI.Code.Extensions;

namespace Web.UI.Controllers
{
    [PortalContextActionFilter]
    public class ExternalTrainingProviderController : KnockoutCRUDController<ExternalTrainingProviderListQuery,ExternalTrainingProviderListModel,ExternalTrainingProvider,ExternalTrainingProviderModel,CreateAndUpdateExternalTrainingProviderCommand>
    {

		public override void Index_PostProcess(IEnumerable<ExternalTrainingProviderListModel> listModel) {
			var companyId = PortalContext.Current.UserCompany.Id.ToString();
			listModel.ToList().ForEach(m => {
				m.Url = Url.ActionLink<UploadController>(a => a.GetFromCompany(m.CertificateUploadId, companyId));
			});
			var test = listModel.ToList();
		}

		public ActionResult AddUpdateExternalTrainingProvider(ExternalTrainingProviderModel externalTrainingProviderModel) {

			var result = ExecuteCommand(new CreateAndUpdateExternalTrainingProviderCommand {
				Id = externalTrainingProviderModel.Id,
				CompanyName = externalTrainingProviderModel.CompanyName,
				ContactNumber = externalTrainingProviderModel.ContactNumber,
				ContactPerson = externalTrainingProviderModel.ContactPerson,
				MobileNumber = externalTrainingProviderModel.MobileNumber,
				BEEStatusLevel = externalTrainingProviderModel.BEEStatusLevel,
				Address = externalTrainingProviderModel.Address,
				CertificateUploadId= externalTrainingProviderModel.CertificateUploadId,
				EmailAddress = externalTrainingProviderModel.EmailAddress
			});
			return RedirectToAction("Index", "ExternalTrainingProvider");
		}

		public JsonResult GetExternalTrainingProvider(string Id) {
			FetchByIdQuery query = new FetchByIdQuery();
			query.Id = Id;
			var result = ExecuteQuery<FetchByIdQuery, ExternalTrainingProviderModel>(
					   query);
			return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		[HttpPost]
		public JsonResult SaveImage(string base64image, string name, string type) {
			if (base64image != null) {
				string x = base64image.Replace("data:" + type + ";base64,", "");
				byte[] data = Convert.FromBase64String(x);
				var upload = new UploadModel {
					Type = type.Split('/')[1],
					Name = name,
					ContentType = type,
					Data = data,
					Id = Guid.NewGuid().ToString()

				};
				var updateCommand = new UploadLogTrainingCommand { Model = upload };
				ExecuteCommand(updateCommand);
				return new JsonResult { Data = upload.Id, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}
			return null;
		}

		public JsonResult GetUploadedCertificate(string Id) {
			var certificate = ExecuteQuery<FetchByIdQuery, Upload>(new FetchByIdQuery {
				Id = Id
			});
			return new JsonResult { Data = certificate, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		public JsonResult GetSearchExternalTrainingProviders(string search) {
			
			var documents = ExecuteQuery<ExternalTrainingProviderListQuery, IEnumerable<ExternalTrainingProviderListModel>>(new ExternalTrainingProviderListQuery {
				SearchText = search
			});

			return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}


		public JsonResult GetFilteredExternalTrainingProviders(string[] data) {

			var documents = ExecuteQuery<ExternalTrainingProviderListQuery, IEnumerable<ExternalTrainingProviderListModel>>(new ExternalTrainingProviderListQuery {
				ExternalTrainingFilter = data
			});

			return new JsonResult { Data = documents, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public JsonResult DoesEmailAlreadyPresent( string EmailAddress,  string Id) {
			var userViewModel =
					ExecuteQuery<ExternalTrainingProviderQueryParameter, ExternalTrainingProviderModel>(new ExternalTrainingProviderQueryParameter {
						Email = EmailAddress,
						Id = Id
					});
			var checkEmail = userViewModel == null ? true : false;
			return Json(checkEmail, JsonRequestBehavior.AllowGet);
		}
	}
}