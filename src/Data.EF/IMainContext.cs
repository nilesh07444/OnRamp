using Domain.Models;
using System.Linq;

namespace Data.EF
{
    public interface IMainContext
    {
        IQueryable<Package> Packages { get; set; }
        IQueryable<Group> Groups { get; set; }
        IQueryable<Role> Roles { get; set; }
        IQueryable<User> Users { get; set; }
        IQueryable<Company> Company { get; set; }
        IQueryable<Setting> Settings { get; set; }
        IQueryable<ReportFile> ReportFiles { get; set; }
        IQueryable<ErrorLogs> ErrorLogses { get; set; }
        IQueryable<UserLoginStats> UserLoginStatistics { get; set; }
        IQueryable<UserActivityLog> UserActivityLogs { get; set; }
        IQueryable<UserCorrespondenceLog> UserCorrespondenceLogs { get; set; }
        IQueryable<FileUpload> FileUploads { get; set; }
        IQueryable<DefaultConfiguration> DefualtConfigurations { get; set; }
        IQueryable<RaceCodes> RaceCodes { get; set; }

        void Add<T>(T obj) where T : DomainObject;

        void Save();
    }
}