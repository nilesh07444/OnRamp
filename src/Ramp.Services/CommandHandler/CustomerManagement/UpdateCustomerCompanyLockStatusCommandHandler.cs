using Common.Command;
using Common.Data;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class UpdateCustomerCompanyLockStatusCommandHandler : CommandHandlerBase<UpdateCustomerCompanyLockStatusCommandParameter>
    {
         private readonly IRepository<Company> _companyRepository;

         public UpdateCustomerCompanyLockStatusCommandHandler(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

         public override CommandResponse Execute(UpdateCustomerCompanyLockStatusCommandParameter command)
        {
            Company companyModel = _companyRepository.Find(command.CompanyId);
             // check is  selfProvision company
            if (command.LockStatus == false) 
            {
                // Add expiry date add 1 month
                if (companyModel.IsSelfCustomer == true)
                {

                    DateTime currentDate = DateTime.Now;
                    DateTime ExpiryDate = currentDate.AddMonths(1);
                    companyModel.ExpiryDate = ExpiryDate;
                    companyModel.IsLock = command.LockStatus;
                }
                else {

                    if (companyModel.YearlySubscription != null) { 
                    // check yearly is null
                        if (companyModel.YearlySubscription == true)
                        {
                            // company is yearly
                            DateTime currentDate = DateTime.Now;
                            DateTime ExpiryDate = currentDate.AddYears(1);
                            companyModel.ExpiryDate = ExpiryDate;
                            companyModel.IsLock = command.LockStatus;

                        }
                        else 
                        { 
                            // company is monthly
                            DateTime currentDate = DateTime.Now;
                            DateTime ExpiryDate = currentDate.AddMonths(1);
                            companyModel.ExpiryDate = ExpiryDate;
                            companyModel.IsLock = command.LockStatus;
                        
                        } 
                    }
                
                }
            } 
            else 
            {
                companyModel.IsLock = command.LockStatus; 
            
            } 
           
            _companyRepository.SaveChanges();
            return null;
        }
    }
}
