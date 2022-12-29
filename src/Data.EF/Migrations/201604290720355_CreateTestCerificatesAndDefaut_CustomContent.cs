namespace Data.EF.Migrations
{
    using Domain.Enums;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public partial class CreateTestCerificatesAndDefaut_CustomContent : DbMigration
    {
        public override void Up()
        {
            MainContext mainDb = new MainContext();
            new Common.Events.EventPublisher().Publish(new Events.CreateDefaultConfigurationContent());
            var customerCompanies =
                mainDb.CompanySet.Where(c => c.CompanyType == CompanyType.CustomerCompany).Select(c => new { Name = c.CompanyName, CompanyId = c.Id, CompanyConnectionString = c.CompanyConnectionString }).ToList();
            foreach (var company in customerCompanies)
            {
                new Common.Events.EventPublisher().Publish(new Data.EF.Events.CreateCustomConfigurationContent
                {
                    CompanyId = company.CompanyId
                });
                new Common.Events.EventPublisher().Publish(new Data.EF.Events.CreateTestCertificatesEvent
                {
                    CompanyId = company.CompanyId
                });
            }
        }

        public override void Down()
        {
        }
    }
}