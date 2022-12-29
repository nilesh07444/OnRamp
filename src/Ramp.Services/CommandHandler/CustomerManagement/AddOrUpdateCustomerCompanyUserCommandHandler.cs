using System;
using System.Collections.Generic;
using System.Linq;
using Common.Data;
using Common.Command;
using Ramp.Services.Helpers;
using Domain.Enums;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;

namespace Ramp.Services.CommandHandler.CustomerManagement
{
    public class AddOrUpdateCustomerCompanyUserCommandHandler :
        CommandHandlerBase<AddOrUpdateCustomerCompanyUserCommand>
    {
        private readonly IRepository<Domain.Models.Group> _groupRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<User> _userRepository;

        public AddOrUpdateCustomerCompanyUserCommandHandler(IRepository<Domain.Models.Group> groupRepository, IRepository<Role> roleRepository, IRepository<User> userRepository)
        {
            _groupRepository = groupRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public override CommandResponse Execute(AddOrUpdateCustomerCompanyUserCommand command)
        {
            command.HasCreated = false;

            List<User> userList = _userRepository.List.ToList();
            if (!userList.Any(u => u.EmailAddress.Equals(command.EmailAddress) && u.CompanyId == command.CompanyId))
            {
                User userModel = _userRepository.Find(command.UserId);

                //To get the Guid of CustomerStandardUser for UserRoleId

                Role customerRole =
                    _roleRepository.Find(command.UserRoleId) ??
                    _roleRepository.List.First(u => u.RoleName
                                                        == EnumHelper.GetDescription(UserRole.CustomerStandardUser));

                var group = new Domain.Models.Group();

                if (command.GroupName != null)
                {
                    List<Domain.Models.Group> groupRole =
                        _groupRepository.List
                            .Where(em => em.Title.ToLower().Trim() == command.GroupName.ToLower().Trim())
                            .ToList();

                    if (groupRole.Count > 0)
                    {
                        group.Id = groupRole[0].Id;
                    }
                    else
                    {
                        //TODO: Create new Group
                        group.Id = Guid.NewGuid();
                        group.Title = command.GroupName;
                        group.CompanyId = command.GroupCreatedForCompanyId;
                        _groupRepository.Add(group);
                        _groupRepository.SaveChanges();
                    }
                }

                if (userModel == null)
                {
                    var customer = new User
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = command.CompanyId,
                        EmailAddress = command.EmailAddress,
                        FirstName = command.FirstName,
                        LastName = command.LastName,
                        IsActive = true,
                        MobileNumber = command.MobileNumber,
                        ParentUserId = command.ParentUserId,
                        ContactNumber = command.ContactNumber,
                        Password = new EncryptionHelper().Encrypt(command.Password),
                        IsConfirmEmail = true,
                        IsUserExpire = false,
                        CreatedOn = DateTime.UtcNow,
                        EmployeeNo = command.EmployeeNo,
                        //ExpireDays =command.ExpireDays,

                    };

                    if (command.SelectedGroupId != Guid.Empty)
                    {
                        customer.GroupId = command.SelectedGroupId;
                    }

                    if (command.GroupName != null)
                    {
                        customer.GroupId = group.Id;
                    }
                    customerRole.Users.Add(customer);
                    _roleRepository.SaveChanges();
                    _userRepository.SaveChanges();
                }
                else
                {
                    userModel.CompanyId = command.CompanyId;
                    userModel.EmailAddress = command.EmailAddress;
                    userModel.FirstName = command.FirstName;
                    userModel.LastName = command.LastName;
                    userModel.ContactNumber = command.ContactNumber;
                    userModel.MobileNumber = command.MobileNumber;

                    if (!string.IsNullOrEmpty(command.Password))
                        userModel.Password = new EncryptionHelper().Encrypt(command.Password);

                    userModel.IsConfirmEmail = true;
                    userModel.CreatedOn = DateTime.UtcNow;
                    userModel.IsUserExpire = false;
                    userModel.EmployeeNo = command.EmployeeNo;


                    // userModel.ExpireDays =command.ExpireDays;
                    _userRepository.SaveChanges();
                }
                command.HasCreated = true;
            }
            else
            {
                User userModel = _userRepository.Find(command.UserId);


                if (userModel != null)
                {
                    //userModel.Id = Guid.NewGuid();
                    userModel.CompanyId = command.CompanyId;
                    userModel.EmailAddress = command.EmailAddress;
                    userModel.FirstName = command.FirstName;
                    userModel.LastName = command.LastName;
                    userModel.IsActive = true;
                    userModel.ParentUserId = command.ParentUserId;
                    userModel.ContactNumber = command.ContactNumber;
                    if (!string.IsNullOrEmpty(command.Password))
                        userModel.Password = new EncryptionHelper().Encrypt(command.Password);
                    userModel.CompanyId = command.CompanyId;
                    userModel.MobileNumber = command.MobileNumber;

                    if (command.UserRoleId != null)
                    {
                        var selectedRole = _roleRepository.Find(command.UserRoleId);

                        if (selectedRole != null)
                        {
                            userModel.Roles.Clear();
                            userModel.Roles.Add(selectedRole);
                        }

                        _roleRepository.SaveChanges();
                    }

                    if (userModel.Roles.FirstOrDefault().RoleName == "CustomerAdmin")
                    {
                        userModel.GroupId = null;
                    }
                    else
                    {
                        userModel.GroupId = command.SelectedGroupId;
                    }
                    //userModel.GroupId = command.SelectedGroupId;
                    userModel.CreatedOn = userModel.CreatedOn;
                    userModel.IsUserExpire = userModel.IsUserExpire;
                    // userModel.ExpireDays = command.ExpireDays;
                    userModel.IsConfirmEmail = true;
                    userModel.EmployeeNo = command.EmployeeNo;

                    _userRepository.SaveChanges();
                    command.HasCreated = true;
                }
            }
            return null;
        }
    }
}