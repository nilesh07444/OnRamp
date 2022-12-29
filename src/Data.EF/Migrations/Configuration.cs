using System.Collections.Generic;
using Domain.Models;

namespace Data.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.IO;
    using Domain.Enums;

    internal sealed class Configuration : DbMigrationsConfiguration<MainContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Data.EF.MainContext context)
        {
            //Code for Configure ELMAH for error log
            if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "/ElmahSP")))
            {
                var sqlFiles =
                    Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/ElmahSP", "*.sql").OrderBy(x => x);
                foreach (string file in sqlFiles)
                {
                    //get the script
                    string scriptText = File.ReadAllText(file);
                    //split the script on "GO" commands
                    string[] splitter = new string[] { "\r\nGO\r\n" };
                    string[] commandTexts = scriptText.Split(splitter,
                        StringSplitOptions.RemoveEmptyEntries);
                    foreach (string commandText in commandTexts)
                    {
                        //execute commandText
                        context.Database.ExecuteSqlCommand(commandText);
                    }
                }
            }

            // setup default roles
            var role = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Admin",
                    Description = "Admin",
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Reseller",
                    Description = "Reseller",
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "CustomerAdmin",
                    Description = "CustomerAdmin",
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "CustomerStandardUser",
                    Description = "CustomerStandardUser",
                }
            };

            role.ForEach(i => context.RoleSet.AddOrUpdate(r => r.RoleName, i));
            context.Save();

            //Run to insert default data into blank db
            if (!context.PackageSet.Any() && !context.CompanySet.Any())
            {
                // create a dummy package and company if there are no packages or companies in the database
                var bundle = new List<Bundle>
                {
                    new Bundle
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Dummy",
                        Description = "Dummy",
                        MaxNumberOfDocuments = 5
                    }
                };

                bundle.ForEach(i => context.BundleSet.AddOrUpdate(p => p.Title, i));
                context.Save();

                Guid companyId = Guid.NewGuid();
                var companyList = new List<Company>
                {
                    new Company
                    {
                        Id = companyId,
                        CompanyName = "Dummy company",
                        CompanyType = CompanyType.ProvisionalCompany,
                        LayerSubDomain = "Dummy",
                        PhysicalAddress = "Dummy",
                        PostalAddress = "Dummy",
                        ProvisionalAccountLink = Guid.Empty,
                        TelephoneNumber = "123",
                        WebsiteAddress = "Dummy",
                        IsActive = true,
                        Bundle = bundle.First(),
                        CreatedOn = DateTime.Now,
                    }
                };

                companyList.ForEach(i => context.CompanySet.AddOrUpdate(c => c.CompanyName, i));
                context.Save();

                if (!context.UserSet.Any(c => c.Roles.Any(r => r.RoleName == "Admin")))
                {
                    // only create a default "admin@admin.com" user if there are no users in the "Admin" role
                    var user = new List<User>
                    {
                        new User
                        {
                            Id = Guid.NewGuid(),
                            ParentUserId = Guid.Empty,
                            FirstName = "Administrator",
                            LastName = "Admin",
                            EmailAddress = "admin@admin.com",
                            Password = "192092051083166081220072193235210111082178035057", // 'password'
                            MobileNumber = "",
                            Company = companyList.First(),
                            IsActive = true,
                            IsConfirmEmail = true,
                            CreatedOn = DateTime.Now,
                            Roles = new []{context.RoleSet.First(c=>c.RoleName == "Admin")}

                        }
                    };

                    user.ForEach(i => context.UserSet.AddOrUpdate(u => u.EmailAddress, i));
                    context.Save();
                }
            }
        }
    }
}
