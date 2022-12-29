using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Ramp.Contracts.ViewModel;
using Common.Command;
using Common.Data;
using Domain.Models;
using Domain.Customer.Models;
using Ramp.Services.Projection;
using Ramp.Services.Helpers;
using System.Security.Claims;
using System.Web.Mvc;
using Domain.Enums;
using ikvm.extensions;
using Ramp.Contracts.Security;

namespace Web.UI.Code
{
    public class OnRampUserStore : IUserStore<OnRampUserModel>, IUserLockoutStore<OnRampUserModel,string>, IUserPasswordStore<OnRampUserModel>, IUserTwoFactorStore<OnRampUserModel,string>
    {
        private IRepository<User> _repository;
        private IRepository<StandardUser> _customerRepository;
        public Task CreateAsync(OnRampUserModel user)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(OnRampUserModel user)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
         
        }
        private void ResolveCustomerRepository()
        {
            _customerRepository = DependencyResolver.Current.GetService<IRepository<StandardUser>>();
            _repository = DependencyResolver.Current.GetService<IRepository<User>>();
        }
        private OnRampUserModel GetUser(string userId)
        {
            ResolveCustomerRepository();
            UserViewModel model = null;
            Guid id = Guid.Empty;
            if (string.IsNullOrWhiteSpace(userId))
                return null;
            if (!Guid.TryParse(userId, out id))
                return null;
            if (_repository.Find(id) != null)
                model = Project.UserViewModelFrom(_repository.Find(id));
            if (_customerRepository.Find(id) != null)
                model = Project.UserViewModelFrom(_customerRepository.Find(id));
            if (model != null)
                return new OnRampUserModel
                {
                    Id = model.Id.ToString(),
                    UserName = model.EmailAddress,
                    Model = model
                };
            return null;
        }
        public Task<OnRampUserModel> FindByIdAsync(string userId)
        {
            return Task.FromResult(GetUser(userId));
        }
        public Task<OnRampUserModel> FindByNameAsync(string userName)
        {
            OnRampUserModel r = null;
            ResolveCustomerRepository();
            if (!string.IsNullOrWhiteSpace(userName)) {
                userName = userName.TrimAllCastToLowerInvariant();
                Guid? id = new Guid?();
                id = _repository.List.AsQueryable().FirstOrDefault(user => user.EmailAddress == userName)?.Id;
                if (!id.HasValue)
                    id = _customerRepository.List.AsQueryable().FirstOrDefault(user => user.EmailAddress == userName)?.Id;
                if (id.HasValue)
                    r = GetUser(id.Value.ToString());
            }
            return Task.FromResult(r);
        }
        public Task UpdateAsync(OnRampUserModel user)
        {
            throw new NotImplementedException();
        }
        public Task<DateTimeOffset> GetLockoutEndDateAsync(OnRampUserModel user)
        {
            return Task.FromResult(new DateTimeOffset(DateTime.MinValue));
        }
        public Task SetLockoutEndDateAsync(OnRampUserModel user, DateTimeOffset lockoutEnd)
        {
            return Task.Run(() => { return; });
        }
        public Task<int> IncrementAccessFailedCountAsync(OnRampUserModel user)
        {
            return Task.FromResult(0);
        }
        public Task ResetAccessFailedCountAsync(OnRampUserModel user)
        {
            return Task.Run(() => { return; });
        }
        public Task<int> GetAccessFailedCountAsync(OnRampUserModel user)
        {
            return Task.FromResult(0);
        }
        public Task<bool> GetLockoutEnabledAsync(OnRampUserModel user)
        {
            return Task.FromResult(!user.Model.IsActive);
        }
        public Task SetLockoutEnabledAsync(OnRampUserModel user, bool enabled)
        {
            return Task.Run(() => {
                user.Model.IsActive = enabled;
            });
        }
        public Task SetPasswordHashAsync(OnRampUserModel user, string passwordHash)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(OnRampUserModel user)
        {
            return Task.FromResult(user.Model.Password);
        }

        public Task<bool> HasPasswordAsync(OnRampUserModel user)
        {
            return Task.FromResult(true);
        }

        public Task SetTwoFactorEnabledAsync(OnRampUserModel user, bool enabled)
        {
            return Task.Run(() => { return; });
        }

        public Task<bool> GetTwoFactorEnabledAsync(OnRampUserModel user)
        {
            return Task.FromResult(false);
        }
    }
    public class OnRampUserModel : IUser
    {
        private string MapDomainRoleEnumToContractRole(UserRole role)
        {
            switch (role)
            {
                case UserRole.Admin:
                    return Ramp.Contracts.Security.Role.Admin;

                case UserRole.Reseller:
                    return Ramp.Contracts.Security.Role.Reseller;

                case UserRole.CustomerAdmin:
                    return Ramp.Contracts.Security.Role.CustomerAdmin;

                case UserRole.CustomerStandardUser:
                    return Ramp.Contracts.Security.Role.StandardUser;
            }
            return null;
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<OnRampUserModel> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            this.Model.UserRoles.ForEach(role => userIdentity.AddClaim(new Claim(ClaimTypes.Role, MapDomainRoleEnumToContractRole(role))));
            this.Model.CustomerRoles.ForEach(role => userIdentity.AddClaim(new Claim(ClaimTypes.Role, role)));
            userIdentity.AddClaim(new Claim(ClaimTypes.System, this.Model.CompanyId.ToString()));
            userIdentity.AddClaim(new Claim(OnrampClaimTypes.StatsIdentifier, Guid.NewGuid().ToString()));
            userIdentity.AddClaim(new Claim(ClaimTypes.GivenName, this.Model.FullName));
            userIdentity.AddClaim(new Claim(ClaimTypes.PrimarySid, this.Model.Id.ToString()));
            return userIdentity;
        }
        public string Id { get; set; }
        public string UserName { get; set; }
        public UserViewModel Model { get; set; }
    }
}