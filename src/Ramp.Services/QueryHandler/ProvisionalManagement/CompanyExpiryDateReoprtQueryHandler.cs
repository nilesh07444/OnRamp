using Common.Data;
using Common.Query;
using Domain.Models;
using Ramp.Contracts;
using Ramp.Contracts.QueryParameter.ProvisionalManagement;
using Ramp.Contracts.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.QueryHandler.ProvisionalManagement
{
    public class CompanyExpiryDateReoprtQueryHandler : QueryHandlerBase<CompanyExpiryDateReoprtQueryParameter, CompanyViewModelLong>
    {
        private readonly IRepository<Domain.Models.Company> _companyRepository;
        private readonly IRepository<Domain.Models.User> _userRepository;

        public CompanyExpiryDateReoprtQueryHandler(IRepository<Domain.Models.Company> companyRepository, IRepository<Domain.Models.User> userRepository)
        {
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }

        public override CompanyViewModelLong ExecuteQuery(CompanyExpiryDateReoprtQueryParameter queryParameters)
        {
            var compamyModel = new CompanyViewModelLong();

            List<Company> company1 = new List<Company>();

            if (queryParameters.IsReseller != Guid.Empty)
            {
                var user = _userRepository.GetAll().Where(u => u.Id == queryParameters.IsReseller).FirstOrDefault();

                company1 = _companyRepository.GetAll().Where(c => c.ProvisionalAccountLink == user.CompanyId).ToList();
            }
            else
            {
                company1 = _companyRepository.GetAll().ToList();
            }

            if (queryParameters.IsMonthly == false && queryParameters.IsYearly == false && queryParameters.CompanyId == Guid.Empty && queryParameters.ExpireInXDays == 0 && queryParameters.AutoExpire == false)
            {
                company1 = null;
            }
            else
            {
                if (queryParameters.CompanyId != Guid.Empty)
                {
                    company1 = company1.Where(c => c.Id == queryParameters.CompanyId).ToList();
                }
                if (queryParameters.IsYearly && queryParameters.IsMonthly)
                {
                    company1 = company1.Where(c => c.YearlySubscription == true || c.YearlySubscription == false).ToList();
                }
                else
                    if (queryParameters.IsYearly)
                {
                    company1 = company1.Where(c => c.YearlySubscription == true).ToList();
                }
                else
                        if (queryParameters.IsMonthly)
                {
                    company1 = company1.Where(c => c.YearlySubscription == false).ToList();
                }
                if (queryParameters.ExpireInXDays != 0)
                {
                    DateTime currentDate = DateTime.Now;
                    DateTime SelectedDate = currentDate.AddDays(queryParameters.ExpireInXDays);

                    company1 = company1.Where(c => (c.ExpiryDate >= currentDate && c.ExpiryDate <= SelectedDate)).ToList();
                }

                if (queryParameters.AutoExpire)
                {
                    company1 = company1.Where(c => c.AutoExpire == queryParameters.AutoExpire).ToList();
                }
            }

            if (company1 != null)
            {
                foreach (Company comp in company1)
                {
                    if (comp.IsLock == true)
                    {
                        // already lock
                        DateTime Exppirydate = Convert.ToDateTime(comp.ExpiryDate);

                        CompanyViewModel companyViewModel = new CompanyViewModel();

                        companyViewModel.Id = comp.Id;
                        companyViewModel.CompanyName = comp.CompanyName;
                        companyViewModel.ExpireMessage = Exppirydate.ToString("dd-MMMM-yyyy");
                        companyViewModel.ReportExpiryDate = Convert.ToDateTime(Exppirydate);
                        companyViewModel.IsExpireReport = true;
                        companyViewModel.CreatedOn = Convert.ToDateTime(comp.CreatedOn);
                        companyViewModel.createdDate = comp.CreatedOn.ToString("dd-MMMM-yyyy");
                        companyViewModel.AutoExpire = (bool)comp.AutoExpire;
                        if (comp.YearlySubscription == true)
                        {
                            companyViewModel.MontlyOrAnual = "Yearly";
                        }
                        else
                        {
                            companyViewModel.MontlyOrAnual = "Monthly";
                        }

                        compamyModel.CompanyList.Add(companyViewModel);
                    }
                    else
                    {
                        // will lock

                        CompanyViewModel companyViewModel = new CompanyViewModel();
                        DateTime Exppirydate = Convert.ToDateTime(comp.ExpiryDate);
                        companyViewModel.Id = comp.Id;
                        companyViewModel.CompanyName = comp.CompanyName;
                        companyViewModel.ExpireMessage = Exppirydate.ToString("dd-MMMM-yyyy");
                        companyViewModel.ReportExpiryDate = Convert.ToDateTime(Exppirydate.ToString("dd-MMMM-yyyy"));
                        companyViewModel.IsExpireReport = false;
                        companyViewModel.CreatedOn = Convert.ToDateTime(comp.CreatedOn.ToString("dd-MMMM-yyyy"));
                        companyViewModel.createdDate = comp.CreatedOn.ToString("dd-MMMM-yyyy");
                        companyViewModel.AutoExpire = (bool)comp.AutoExpire;
                        if (comp.YearlySubscription == true)
                        {
                            companyViewModel.MontlyOrAnual = "Yearly";
                        }
                        else
                        {
                            companyViewModel.MontlyOrAnual = "Monthly";
                        }

                        compamyModel.CompanyList.Add(companyViewModel);
                    }
                }
            }
            else
            {
                compamyModel = null;
            }

            //// If All are not selected
            //if (queryParameters.IsMonthly == false && queryParameters.IsYearly == false && queryParameters.CompanyId == Guid.Empty && queryParameters.ExpireInXDays == 0)
            //{
            //    company = null;

            //    IsQueryExicuted = true;

            //}

            //// if  monthly, yearly, company selected
            //if (queryParameters.IsMonthly == true && queryParameters.IsYearly == true && queryParameters.CompanyId != Guid.Empty)
            //{
            //    company = null;
            //    IsQueryExicuted = true;
            //}

            //// if  monthly, yearly, ExpireInXDays selected
            //if (queryParameters.IsMonthly == true && queryParameters.IsYearly == true && queryParameters.ExpireInXDays != 0)
            //{
            //    company = null;
            //    IsQueryExicuted = true;

            //}

            //// if  CompanyId, yearly, ExpireInXDays selected

            //if (queryParameters.IsYearly == true && queryParameters.CompanyId != Guid.Empty && queryParameters.ExpireInXDays != 0)
            //{
            //    company = null;
            //    IsQueryExicuted = true;

            //}

            //// if  IsMonthly, yearly, ExpireInXDays selected
            //if (queryParameters.IsMonthly == true && queryParameters.CompanyId != Guid.Empty && queryParameters.ExpireInXDays != 0)
            //{
            //    company = null;
            //    IsQueryExicuted = true;

            //}
            //// If All Selected

            //if (queryParameters.IsMonthly == true && queryParameters.IsYearly == true && queryParameters.CompanyId != Guid.Empty && queryParameters.ExpireInXDays != 0)
            //{
            //    DateTime currentDate = DateTime.Now;
            //    DateTime SelectedDate = currentDate.AddDays(queryParameters.ExpireInXDays);

            //    company = _companyRepository.GetAll().Where(c => (c.ExpiryDate >= currentDate && c.ExpiryDate <= SelectedDate)).ToList();
            //    IsQueryExicuted = true;

            //}

            //// if Monthly, Yearly selected
            //if (queryParameters.IsMonthly == true && queryParameters.IsYearly == true)
            //{
            //    company = _companyRepository.GetAll().ToList();
            //    IsQueryExicuted = true;

            //}

            //// if Monthly, ExpireInXDays selected
            //if (queryParameters.IsMonthly == true && queryParameters.ExpireInXDays != 0)
            //{
            //    DateTime currentDate = DateTime.Now;
            //    DateTime SelectedDate = currentDate.AddDays(queryParameters.ExpireInXDays);

            //    company = _companyRepository.GetAll().Where(c => (c.ExpiryDate >= currentDate && c.ExpiryDate <= SelectedDate) && c.YearlySubscription == false).ToList();
            //    IsQueryExicuted = true;
            //}

            //// if Monthly, ExpireInXDays selected
            //if (queryParameters.ExpireInXDays != 0 && queryParameters.IsYearly == true)
            //{
            //    DateTime currentDate = DateTime.Now;
            //    DateTime SelectedDate = currentDate.AddDays(queryParameters.ExpireInXDays);

            //    company = _companyRepository.GetAll().Where(c => (c.ExpiryDate >= currentDate && c.ExpiryDate <= SelectedDate) && c.YearlySubscription == true).ToList();
            //    IsQueryExicuted = true;
            //}

            //// if IsYearly, CompanyId selected
            //if (queryParameters.CompanyId != Guid.Empty && queryParameters.IsYearly == true)
            //{
            //    company = _companyRepository.GetAll().Where(c => c.Id == queryParameters.CompanyId && c.YearlySubscription == true).ToList();
            //    IsQueryExicuted = true;

            //}

            //// if CompanyId, ExpireInXDays selected
            //if (queryParameters.CompanyId != Guid.Empty && queryParameters.ExpireInXDays != 0)
            //{
            //    company = null;
            //    IsQueryExicuted = true;
            //}

            //// if CompanyId, IsMonthly selected
            //if (queryParameters.CompanyId != Guid.Empty && queryParameters.IsMonthly == true)
            //{
            //    company = _companyRepository.GetAll().Where(c => c.Id == queryParameters.CompanyId && c.YearlySubscription == false).ToList();
            //    IsQueryExicuted = true;
            //}

            //// if CompanyId selected
            //if (IsQueryExicuted == false)
            //{
            //    if (queryParameters.CompanyId != Guid.Empty)
            //    {
            //        company = _companyRepository.GetAll().Where(c => c.Id == queryParameters.CompanyId).ToList();
            //        IsQueryExicuted = true;
            //    }
            //}

            //// if IsMonthly selected
            //if (IsQueryExicuted == false)
            //{
            //    if (queryParameters.IsMonthly == true)
            //    {
            //        company = _companyRepository.GetAll().Where(c => c.YearlySubscription == false).ToList();
            //        IsQueryExicuted = true;
            //    }
            //}

            //// if IsYearly selected
            //if (IsQueryExicuted == false)
            //{
            //    if (queryParameters.IsYearly == true)
            //    {
            //        company = _companyRepository.GetAll().Where(c => c.YearlySubscription == true).ToList();
            //        IsQueryExicuted = true;
            //    }
            //}
            //// if ExpireInXDays selected

            //if (IsQueryExicuted == false)
            //{
            //    if (queryParameters.ExpireInXDays != 0)
            //    {
            //        DateTime currentDate = DateTime.Now;
            //        DateTime SelectedDate = currentDate.AddDays(queryParameters.ExpireInXDays);

            //        company = _companyRepository.GetAll().Where(c => (c.ExpiryDate >= currentDate && c.ExpiryDate <= SelectedDate)).ToList();
            //        IsQueryExicuted = true;
            //    }
            //}

            //if (company != null)
            //{
            //    foreach (Company comp in company)
            //    {
            //        if (comp.IsSelfCustomer == true)
            //        {
            //            // selfProvision customer company will expire "+ month + 1day " of created date

            //            DateTime CreatedDate = comp.CreatedOn;
            //            DateTime findExpireDate = CreatedDate.AddMonths(1);
            //            DateTime ExpireDate = findExpireDate.AddDays(1);
            //            DateTime CurrentDate = DateTime.UtcNow;
            //            if ((ExpireDate > queryParameters.FromDate) && (ExpireDate < queryParameters.ToDate))
            //            {
            //                //match found here have to add the customer company to view model

            //                int totaldays = Convert.ToInt32((CurrentDate - CreatedDate).TotalDays);

            //                if (totaldays >= 31)
            //                {
            //                    // checked is expire
            //                    CompanyViewModel companyViewModel = new CompanyViewModel();

            //                    companyViewModel.Id = comp.Id;
            //                    companyViewModel.CompanyName = comp.CompanyName;
            //                    companyViewModel.ExpireMessage = "Expired on " + ExpireDate.ToString("dd-MMMM-yyyy");
            //                    companyViewModel.ReportExpiryDate = ExpireDate;
            //                    companyViewModel.IsExpireReport = true;
            //                    companyViewModel.MontlyOrAnual = "Monthly";
            //                    compamyModel.CompanyList.Add(companyViewModel);

            //                }
            //                else
            //                {
            //                    // not expire
            //                    CompanyViewModel companyViewModel = new CompanyViewModel();

            //                    companyViewModel.Id = comp.Id;
            //                    companyViewModel.CompanyName = comp.CompanyName;
            //                    companyViewModel.ExpireMessage = "Will Expire on " + ExpireDate.ToString("dd-MMMM-yyyy");
            //                    companyViewModel.ReportExpiryDate = ExpireDate;
            //                    companyViewModel.IsExpireReport = false;
            //                    companyViewModel.MontlyOrAnual = "Monthly";
            //                    compamyModel.CompanyList.Add(companyViewModel);

            //                }

            //            }

            //        }
            //        else
            //        {
            //            // Normal customer company will expire " +1year + 1day " of created date
            //            DateTime CreatedDate = comp.CreatedOn;
            //            DateTime findExpireDate = CreatedDate.AddYears(1);
            //            DateTime ExpireDate = findExpireDate.AddDays(1);

            //            DateTime CurrentDate = DateTime.UtcNow;
            //            if ((ExpireDate > queryParameters.FromDate) && (ExpireDate < queryParameters.ToDate))
            //            {
            //                //match found here have to add the customer company to view model

            //                int totaldays = Convert.ToInt32((CurrentDate - CreatedDate).TotalDays);

            //                if (totaldays >= 366)
            //                {
            //                    // checked is expire
            //                    CompanyViewModel companyViewModel = new CompanyViewModel();

            //                    companyViewModel.Id = comp.Id;
            //                    companyViewModel.CompanyName = comp.CompanyName;
            //                    companyViewModel.ExpireMessage = "Expired on " + ExpireDate.ToString("dd-MMMM-yyyy");
            //                    companyViewModel.ReportExpiryDate = ExpireDate;
            //                    companyViewModel.IsExpireReport = true;
            //                    companyViewModel.MontlyOrAnual = "Yearly";
            //                    compamyModel.CompanyList.Add(companyViewModel);

            //                }
            //                else
            //                {
            //                    // not expire
            //                    CompanyViewModel companyViewModel = new CompanyViewModel();
            //                    companyViewModel.Id = comp.Id;
            //                    companyViewModel.CompanyName = comp.CompanyName;
            //                    companyViewModel.ExpireMessage = "Will Expire on " + ExpireDate.ToString("dd-MMMM-yyyy");
            //                    companyViewModel.ReportExpiryDate = ExpireDate;
            //                    companyViewModel.IsExpireReport = false;
            //                    companyViewModel.MontlyOrAnual = "Yearly";

            //                    compamyModel.CompanyList.Add(companyViewModel);

            //                }

            //            }

            //        }

            //    }
            //}

            return compamyModel;
        }
    }
}