using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Domain.Data
{
    public interface IContext
    {
        IQueryable<Person> People { get; set; }
        IQueryable<Address> Addresses { get; set; }

        IQueryable<Role> Roles { get; set; }
        IQueryable<User> Users { get; set; }
        IQueryable<UsersInRole> UsersInRoles { get; set; }
        IQueryable<Package> Packages { get; set; }
        IQueryable<Training> Trainings { get; set; }
        IQueryable<TrainingTest> TrainingTests { get; set; }
        IQueryable<ScoreCard> ScoreCards { get; set; }
        IQueryable<Setting> Settings { get; set; }
        IQueryable<ReportFile> ReportFiles { get; set; }

        void Add<T>(T obj) where T : DomainObject;
        void Save();
        IQueryable<TEntity> GetSet<TEntity>() where TEntity : class;
    }
}
