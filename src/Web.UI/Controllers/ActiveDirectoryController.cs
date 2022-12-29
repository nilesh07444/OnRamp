using Domain.Customer.Models;
using Ramp.Contracts.QueryParameter.User;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices.Protocols;
using System.Net;
using System;
using Ramp.Contracts.QueryParameter.CustomerManagement;
using Ramp.Contracts.ViewModel;
using Common.Query;
using System.DirectoryServices;
using System.IO;
using Ramp.Security.Authorization;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Contracts.CommandParameter.Group;
using System.DirectoryServices.AccountManagement;
using Ramp.Contracts.QueryParameter.Group;
using Ramp.Contracts.QueryParameter;
using Domain.Customer.Models.CustomRole;
using Role = Ramp.Contracts.Security.Role;
using System.Text.RegularExpressions;
using Ramp.Contracts.CommandParameter.Login;
using System.Security.Cryptography.X509Certificates;

namespace Web.UI.Controllers
{
    public class ActiveDirectoryController : RampController
    {

        Guid CompanyId;

        // GET: ActiveDirectory latest
        public bool CheckUserEmail(string email, string password, string companyId)
        {


            try
            {
                var res = ExecuteQuery<UserListQuery, IEnumerable<StandardUser>>(new UserListQuery
                {
                    Emails = new List<string> { email }
                });


                if (res.ToList().Count > 0)
                {
                    var company = ExecuteQuery<FindCompanyQueryParameter, Domain.Models.Company>(new FindCompanyQueryParameter
                    {
                        Id = Guid.Parse(companyId)
                    });
                    if (company.ActiveDirectoryEnabled)
                    {
                        bool exist = false;

                        var domains = Regex.Replace(company.Domain, @"\s+", "").Split(',');
                        var ports = Regex.Replace(company.Port, @"\s+", "").Split(',');
                        var userNames = Regex.Replace(company.UserName, @"\s+", "").Split(',');
                        var adpass = company.Password;
                        var passwords = password;

                        foreach (var domain in domains.Select((value, i) => new { i, value }))
                        {
                            var value = domain.value;
                            var index = domain.i;

                            exist = new LdapManager().Validate(email, passwords, value, userNames[index], adpass);



                            if (exist)
                            {
                                return exist;
                            }
                        }

                        return exist;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void ImportAllUsersFromAD()
        {
            List<ADUser> users = new List<ADUser>();

            try
            {
                CompanyId = PortalContext.Current.UserCompany.Id;

                var result = ExecuteQuery<FetchByCategoryIdQuery, List<CustomerRole>>(new FetchByCategoryIdQuery()); ;

                var RoleId = result.Where(x => x.RoleName == "StandardUser").FirstOrDefault();

                var company = ExecuteQuery<CustomerCompanyQueryParameter, CompanyViewModelLong>(new CustomerCompanyQueryParameter
                {
                    Id = CompanyId
                });


                if (company.CompanyViewModel.ActiveDirectoryEnabled)
                {
                    bool exist = false;

                    var domains = Regex.Replace(company.CompanyViewModel.Domain, @"\s+", "").Split(',');
                    var ports = Regex.Replace(company.CompanyViewModel.Port, @"\s+", "").Split(',');
                    var userNames = Regex.Replace(company.CompanyViewModel.UserName, @"\s+", "").Split(',');
                    var passwords = company.CompanyViewModel.Password;

                    foreach (var domain in domains.Select((value, i) => new { i, value }))
                    {
                        var value = domain.value;
                        var index = domain.i;

                        var u = GetAllUsers(value, userNames[index], company.CompanyViewModel.Password);


                        users.AddRange(u);
                    }
                }
                //get list of all users for a copmany from all the active directory servers

                

                var allGroups = ExecuteQuery<AllGroupsByCustomerAdminQueryParameter, List<GroupViewModel>>(new AllGroupsByCustomerAdminQueryParameter
                {
                });
                //save update users from ad to database
                foreach (var user in users)
                {
                    if ((user.Email != null && user.Email.Contains("boxer.co.za")))
                    {


                        //call save update command handler to save/update the user etails to database
                        if (user.Email != null)
                        {
                            List<string> groups = new List<string>();

                            var tempG = String.Join(",", user.Groups);
                            foreach (var g in tempG.Split(','))
                            {
                                //if (g.Contains("CN"))
                                //{
                                var groupinfo = allGroups.Where(z => z.Title == g.ToString()).FirstOrDefault();
                                if (groupinfo != null)
                                {
                                    groups.Add(groupinfo.GroupId.ToString());
                                }
                                //foreach (var ag in allGroups)
                                //{
                                //    if (ag.Title == g.ToString())
                                //    {
                                //        //if (ag.Title == g.Replace("CN=", "") && !g.Contains("CN=department"))
                                //        //{
                                //        groups.Add(ag.GroupId.ToString());
                                //        //}
                                //    }
                                //}

                                //}
                            }


                            var customerCompanyUserCommand = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand
                            {
                                UserId = Guid.Empty,
                                CompanyId = PortalContext.Current.UserCompany.Id,
                                //SelectedGroupId = model.SelectedGroups,
                                EmailAddress = user.Email.ToLower(),
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                SelectedGroupId = groups,
                                Roles = new List<string> { Role.StandardUser },
                                //MobileNumber = model.MobileNumber,
                                ParentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                                Password = user.FirstName + new Random().Next(1000),
                                IsFromDataMigration = true,
                                AdUser = true,
                                IsActive = user.IsActive,

                                EmployeeNo = user.EmployeeNo,

                                //IsConfirmEmail = true,
                                //IDNumber = model.IDNumber,
                                //Gender = model.Gender,
                                //RaceCodeId = model.RaceCodeId,
                                //TrainingLabels = model.TrainingLabels,
                                //SelectedCustomUserRole = model.SelectedCustomUserRole,,
                                ADSync = true,
                                Department = user.Department,


                            };
                            ExecuteCommand(customerCompanyUserCommand);
                            //}

                        }
                    }
                    else
                    {
                        if (user.Email != null)
                        {
                            List<string> groups = new List<string>();

                            var tempG = String.Join(",", user.Groups);
                            foreach (var g in tempG.Split(','))
                            {
                                if (g.Contains("CN"))
                                {

                                    foreach (var ag in allGroups)
                                    {
                                        if (ag.Title == g.Replace("CN=", "") && !g.Contains("CN=Groups"))
                                        {
                                            groups.Add(ag.GroupId.ToString());
                                        }
                                    }

                                }
                            }


                            var customerCompanyUserCommand = new AddOrUpdateCustomerCompanyUserByCustomerAdminCommand
                            {
                                UserId = Guid.Empty,
                                CompanyId = PortalContext.Current.UserCompany.Id,
                                //SelectedGroupId = model.SelectedGroups,
                                EmailAddress = user.Email.ToLower(),
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                SelectedGroupId = groups,
                                Roles = new List<string> { Role.StandardUser },
                                //MobileNumber = model.MobileNumber,
                                ParentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                                Password = user.FirstName + new Random().Next(1000),
                                IsFromDataMigration = true,
                                AdUser = true,
                                IsActive = user.IsActive,

                                EmployeeNo = user.EmployeeNo,

                                //IsConfirmEmail = true,
                                //IDNumber = model.IDNumber,
                                //Gender = model.Gender,
                                //RaceCodeId = model.RaceCodeId,
                                //TrainingLabels = model.TrainingLabels,
                                //SelectedCustomUserRole = model.SelectedCustomUserRole,,
                                ADSync = true,
                                Department = user.Department,


                            };
                            ExecuteCommand(customerCompanyUserCommand);
                            //}

                        }
                    }
                }


                UpdateStandardUserByAdUser(users);


            }

            catch (Exception ex)
            {
                throw ex;
            }


        }


        private void UpdateStandardUserByAdUser(List<ADUser> Aduserlist)
        {
          
                 var AllUser = ExecuteQuery<FetchAllScheduleReportQuery, IEnumerable<StandardUser>>(new FetchAllScheduleReportQuery
                 {
                      
                 }).Where(z=>z.AdUser).ToList();
            
            var missingAdUser = AllUser.Where(z => !Aduserlist.Where(c=>c.Email!=null).Select(a=>a.Email.ToLower()).Contains(z.EmailAddress.ToLower())).ToList();
            foreach (var user in missingAdUser)
            {
                var customerCompanyUserCommand = new UpdateStandardUserByAdUser
                {
                    UserId =user.Id,
                    EmailAddress = user.EmailAddress.ToLower(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,                    
                    Roles = new List<string> { Role.StandardUser },                    
                    ParentUserId = SessionManager.GetCurrentlyLoggedInUserId(),
                    IsActive = false,
                };
                ExecuteCommand(customerCompanyUserCommand);
            }
        }




        public void ImportAllGroupsFromAD()
        {
            List<ADGroups> groups = new List<ADGroups>();

            try
            {
                CompanyId = PortalContext.Current.UserCompany.Id;

                var company = ExecuteQuery<CustomerCompanyQueryParameter, CompanyViewModelLong>(new CustomerCompanyQueryParameter
                {
                    Id = CompanyId
                });
                if (company.CompanyViewModel.ActiveDirectoryEnabled)
                {
                    bool exist = false;

                    var domains = Regex.Replace(company.CompanyViewModel.Domain, @"\s+", "").Split(',');
                    var ports = Regex.Replace(company.CompanyViewModel.Port, @"\s+", "").Split(',');
                    var userNames = Regex.Replace(company.CompanyViewModel.UserName, @"\s+", "").Split(',');
                    var passwords = company.CompanyViewModel.Password;

                    foreach (var domain in domains.Select((value, i) => new { i, value }))
                    {
                        var value = domain.value;
                        var index = domain.i;

                        var g = GetlAllGroups(value, userNames[index], company.CompanyViewModel.Password);

                        groups.AddRange(g);
                    }
                }


                foreach (var group in groups)
                {
                  
                        var result = ExecuteCommand(new SaveOrUpdateGroupCommand
                        {
                            GroupId = Guid.NewGuid(),
                            ParentId = null,
                            Title = group.Title,
                            Description = group.Desription
                        });

                    
                   
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private List<ADUser> GetAllUsers(string Domain, string UserName, string Password)
        {

            List<ADUser> usersList = new List<ADUser>();

            string domainpath = "LDAP://" + Domain;
            if (Domain == "172.16.8.1:389" || Domain == "boxer.co.za:389" || Domain == "bohodc01.boxer.local:636")
            {
                var OU1 = "/OU=Users,OU=Information Technology,OU=Boxer Head Office,DC=boxer,DC=local";
                var OU2 = "/OU=Staff,OU=Boxer Head Office,DC=boxer,DC=local";
                domainpath = "LDAP://" + Domain + OU1;
                usersList = ADUsers(domainpath, UserName, Password);
                domainpath = "LDAP://" + Domain + OU2;
                var usersListOU2 = ADUsers(domainpath, UserName, Password);
                usersList.AddRange(usersListOU2);
            }
            else
            {
                usersList = ADUsers(domainpath, UserName, Password);
            }
            return usersList;


        }
        private List<ADUser> ADUsers(string domainpath, string UserName, string Password)
        {

            List<ADUser> usersList = new List<ADUser>();
            DirectoryEntry entry = new DirectoryEntry(domainpath, UserName, Password);
            DirectorySearcher mySearcher = new DirectorySearcher(entry);
            mySearcher.PageSize = 100;
            mySearcher.Filter = ("(&(objectCategory=person)(objectClass=user))");

            if (domainpath.Contains("boxer.co.za") || domainpath.Contains("172.16.8.1") || domainpath.Contains("bohodc01.boxer.local:636"))
            {
                try
                {


                    foreach (SearchResult result in mySearcher.FindAll())
                    {
                        var adUsers = new ADUser();
                        adUsers.Groups = new List<string>();


                        foreach (string myKey in result.Properties.PropertyNames)
                        {
                            //x = x + myKey + ":";
                            foreach (Object myCollection in result.Properties[myKey])
                            {
                                if (myKey == "cn")
                                    adUsers.UserName = myCollection.ToString();
                                else if (myKey == "mail")
                                    adUsers.Email = myCollection.ToString();
                                else if (myKey == "whencreated")
                                    adUsers.CreatedOn = myCollection.ToString();
                                else if (myKey == "department")
                                    adUsers.Groups.Add(myCollection.ToString());
                                else if (myKey == "givenname")
                                    adUsers.FirstName = myCollection.ToString();
                                else if (myKey == "sn")
                                    adUsers.LastName = myCollection.ToString();
                                else if (myKey == "useraccountcontrol")
                                    adUsers.IsActive = ((myCollection.ToString() == "512") || (myCollection.ToString() == "66048") || (myCollection.ToString() == "	262656")) ? true : false;
                                else if (myKey == "employeeid")
                                    adUsers.EmployeeNo = myCollection.ToString();

                            }


                        }

                        if (adUsers.FirstName != null && adUsers.Groups != null)
                        {
                            usersList.Add(adUsers);
                        }
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("exceotion ", ex);
                }
            }
            else
            {
                try
                {


                    foreach (SearchResult result in mySearcher.FindAll())
                    {
                        var adUsers = new ADUser();
                        adUsers.Groups = new List<string>();


                        foreach (string myKey in result.Properties.PropertyNames)
                        {
                            //x = x + myKey + ":";
                            foreach (Object myCollection in result.Properties[myKey])
                            {
                                if (myKey == "cn")
                                    adUsers.UserName = myCollection.ToString();
                                else if (myKey == "mail")
                                    adUsers.Email = myCollection.ToString();
                                else if (myKey == "whencreated")
                                    adUsers.CreatedOn = myCollection.ToString();
                                else if (myKey == "memberof")
                                    adUsers.Groups.Add(myCollection.ToString());
                                else if (myKey == "givenname")
                                    adUsers.FirstName = myCollection.ToString();
                                else if (myKey == "sn")
                                    adUsers.LastName = myCollection.ToString();
                                else if (myKey == "useraccountcontrol")
                                    adUsers.IsActive = ((myCollection.ToString() == "512") || (myCollection.ToString() == "66048") || (myCollection.ToString() == "	262656")) ? true : false;
                                else if (myKey == "employeeid")
                                    adUsers.EmployeeNo = myCollection.ToString();

                            }


                        }

                        if (adUsers.FirstName != null)
                        {
                            usersList.Add(adUsers);
                        }
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("exceotion ", ex);
                }
            }


            return usersList;
        }

        private List<ADGroups> GetlAllGroups(string Domain, string UserName, string Password)
        {


            //get list of all groups for a active directory
            //var groups = new List<string>();

            List<ADGroups> groupList = new List<ADGroups>();


           

            //List<string> lista1 = new List<string>();
            DirectoryEntry entry = new DirectoryEntry("LDAP://" + Domain, UserName, Password);

            DirectorySearcher mySearcher = new DirectorySearcher(entry);
            mySearcher.PageSize = 100;
            if (Domain == "172.16.8.1:636" || Domain == "boxer.co.za:389" || Domain == "bohodc01.boxer.local:636")
            {
                mySearcher.Filter = ("(&(objectCategory=person)(objectClass=user))");
            }
            else
            {
                mySearcher.Filter = ("(&(objectCategory=Group)(!(groupType:1.2.840.113556.1.4.803:=2147483648))(!(groupType:1.2.840.113556.1.4.803:=1))(!(groupType:1.2.840.113556.1.4.803:=4)))");
            }
            try
            {
                //string tab = " ";
                //using (StreamWriter writer = new StreamWriter(@"C:\Users\cis\Desktop\ads\groups.txt"))
                //{
                if (Domain == "172.16.8.1:389" || Domain == "bohodc01.boxer.local:636" || Domain == "boxer.local:389" || Domain == "boxer.co.za:389")
                {

                    foreach (SearchResult result in mySearcher.FindAll())
                    {
                        var adGroups = new ADGroups();
                        //string x = null;
                        //foreach (string myKey in result.Properties.PropertyNames)
                        foreach (string myKey in new List<string> { "department" })
                        {
                            //x = x + myKey + ":";
                            foreach (Object myCollection in result.Properties[myKey])
                            {

                                if (myKey == "department")
                                    adGroups.Title = myCollection.ToString();
                                else if (myKey == "description")
                                    adGroups.Desription = myCollection.ToString();
                                else if (myKey == "whencreated")
                                    adGroups.CreatedOn = myCollection.ToString();


                                //x = x + tab + "'" + myCollection + "', ";
                            }
                        }
                        if (adGroups.Title != null && adGroups.Title.ToString() != "internal Serivice Account" && adGroups.Title.ToString() != "Service Account" && adGroups.Title.ToString() != "BCX" && adGroups.Title.ToString() != "BCTechnologies" && adGroups.Title.ToString() != "Altron" && adGroups.Title.ToString() != "External Service Account" && adGroups.Title.ToString() != "First Technologies" && adGroups.Title.ToString() != "IWMS service account" && adGroups.Title.ToString() != "Internal Service Account" && adGroups.Title.ToString() != "Training" && adGroups.Title.ToString() != "Concom")
                        {
                            
                                groupList.Add(adGroups);
                           
                        
                        }

                        //writer.WriteLine(x);
                        //writer.WriteLine();
                    }
                }
                else
                {
                    foreach (SearchResult result in mySearcher.FindAll())
                    {
                        var adGroups = new ADGroups();
                        //string x = null;
                        //foreach (string myKey in result.Properties.PropertyNames)
                        foreach (string myKey in new List<string> { "name", "whencreated", "description", "memberof" })
                        {
                            //x = x + myKey + ":";
                            foreach (Object myCollection in result.Properties[myKey])
                            {

                                if (myKey == "name")
                                    adGroups.Title = myCollection.ToString();
                                else if (myKey == "description")
                                    adGroups.Desription = myCollection.ToString();
                                else if (myKey == "whencreated")
                                    adGroups.CreatedOn = myCollection.ToString();


                                //x = x + tab + "'" + myCollection + "', ";
                            }
                        }
                        groupList.Add(adGroups);
                        //writer.WriteLine(x);
                        //writer.WriteLine();
                    }
                }

                //writer.Flush();
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("exceotion ", ex);
            }

            return groupList;


        }


    }

    public class LdapManager
    {

        public bool Validate(string userId, string password, string domain, string adusername, string adpassword)
        {

            try
            {
                List<ADUser> usersList = new List<ADUser>();
                DirectoryEntry entry = new DirectoryEntry("LDAP://" + domain, adusername, adpassword);
                DirectorySearcher mySearcher = new DirectorySearcher(entry);
                mySearcher.PageSize = 100;
                mySearcher.Filter = ("(&(objectCategory=person)(objectClass=user)(mail=" + userId + "))");
                var ADusername = string.Empty;
                foreach (SearchResult result in mySearcher.FindAll())
                {
                    ADusername = string.Empty;
                    foreach (string myKey in result.Properties.PropertyNames)
                    {
                        if (myKey == "samaccountname")
                        {
                            foreach (Object myCollection in result.Properties[myKey])
                            {
                                ADusername = myCollection.ToString();
                            }
                            break;
                        }

                    }


                }

                LdapConnection connection = new LdapConnection(domain);
                NetworkCredential credential = new NetworkCredential(ADusername, password);
                connection.Credential = credential;
                connection.Bind();

                return true;
            }
            catch (LdapException lexc)
            {
                try
                {
                    int index = userId.IndexOf("@");
                    if (index >= 0)
                        userId = userId.Substring(0, index);
                    LdapConnection connection2 = new LdapConnection(domain);
                    NetworkCredential credential2 = new NetworkCredential(userId, password);
                    connection2.Credential = credential2;
                    connection2.Bind();
                    return true;
                }
                catch (Exception)
                {
                    var error = lexc.ServerErrorMessage;
                    return false;
                }


            }
            catch (Exception exc)
            {
                var error = exc;
                return false;
            }
        }
    }

}