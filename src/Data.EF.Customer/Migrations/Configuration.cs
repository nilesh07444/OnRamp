namespace Data.EF.Customer.Migrations
{
    using Domain.Customer;
    using Ramp.Contracts.ViewModel;
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;

    internal sealed class Configuration : DbMigrationsConfiguration<CustomerContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
      
        }

        protected override void Seed(Data.EF.Customer.CustomerContext context)
        {
            if (!context.DocumentCategories.Any())
                context.DocumentCategories.Add(new Domain.Customer.Models.DocumentCategory
                {
                    Id = Guid.NewGuid().ToString(),
                    ParentId = null,
                    Title = "Default"
                });
            if (!context.Certificates.Any() && File.Exists(HostingEnvironment.MapPath("~/Content/images/Certificate.jpg")))
                context.Certificates.Add(new Domain.Customer.Models.Certificate
                {
                    Id = Guid.NewGuid().ToString(),
                    Upload = new Domain.Customer.Models.Upload
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = DocumentType.Certificate.ToString(),
                        ContentType = "image/jpg",
                        Description = "Default",
                        Name = Path.GetFileName(HostingEnvironment.MapPath("~/Content/images/Certificate.jpg")),
                        Data = File.ReadAllBytes(HostingEnvironment.MapPath("~/Content/images/Certificate.jpg"))
                    }
                });
            context.SaveChanges();
        }
    }
}