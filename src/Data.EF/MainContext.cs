using Domain.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace Data.EF
{
    public class MainContext : DbContext, IMainContext
    {
        static MainContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MainContext, Migrations.Configuration>());
        }

        public MainContext()
            : base("Name=MainContext")
        {
        }
        public IDbSet<CustomColour> CustomColours { get; set; }
        public virtual IDbSet<Group> GroupSet { get; set; }
        public virtual IDbSet<Package> PackageSet { get; set; }
        public virtual IDbSet<Bundle> BundleSet { get; set; }
        public virtual IDbSet<Role> RoleSet { get; set; }
        public virtual IDbSet<User> UserSet { get; set; }
        public virtual IDbSet<Company> CompanySet { get; set; }
        public virtual IDbSet<Setting> SettingSet { get; set; }
        public virtual IDbSet<ReportFile> ReportFileSet { get; set; }
        public virtual IDbSet<ErrorLogs> ErrorLogsSet { get; set; }
        public virtual IDbSet<UserLoginStats> UserLoginStatsSet { get; set; }
        public virtual IDbSet<UserCorrespondenceLog> UserCorrespondenceLogSet { get; set; }
        public virtual IDbSet<UserActivityLog> UserActivityLogSet { get; set; }
        public virtual IDbSet<FileUpload> FileUploadSet { get; set; }
        public virtual IDbSet<DefaultConfiguration> DefaultConfigurationSet { get; set; }
        public virtual IDbSet<RaceCodes> RaceCodesSet { get; set; }
        public virtual IDbSet<CustomerConfiguration> CustomerConfigurationSet { get; set; }
        public virtual IDbSet<Icon> Icon { get; set; }
        public virtual IDbSet<IconSet> IconSet { get; set; }
        public virtual IDbSet<CommandAuditTrail> CommandAuditTrail { get; set; }
        public IQueryable<FileUpload> FileUploads
        {
            get { return FileUploadSet; }
            set { FileUploadSet = (DbSet<FileUpload>)value; }
        }
        public IQueryable<Icon> Icons
        {
            get { return Icon; }
            set { Icon = (DbSet<Icon>)value; }
        }
        public IQueryable<IconSet> IconSets
        {
            get { return IconSet; }
            set { IconSet = (DbSet<IconSet>)value; }
        }

        public IQueryable<DefaultConfiguration> DefaultConfiguration
        {
            get { return DefaultConfigurationSet; }
            set { DefaultConfigurationSet = value as IDbSet<DefaultConfiguration>; }
        }
        public IQueryable<CustomerConfiguration> CustomerConfigutrationSet
        {
            get { return CustomerConfigutrationSet; }
            set { CustomerConfigutrationSet = value as IDbSet<CustomerConfiguration>; }
        }
        public IQueryable<UserLoginStats> UserLoginStatistics
        {
            get { return UserLoginStatsSet; }
            set { UserLoginStatsSet = (DbSet<UserLoginStats>)value; }
        }

        public IQueryable<UserActivityLog> UserActivityLogs
        {
            get { return UserActivityLogSet; }
            set { UserActivityLogSet = (DbSet<UserActivityLog>)value; }
        }

        public IQueryable<UserCorrespondenceLog> UserCorrespondenceLogs
        {
            get { return UserCorrespondenceLogSet; }
            set { UserCorrespondenceLogSet = (DbSet<UserCorrespondenceLog>)value; }
        }

        public IQueryable<Package> Packages
        {
            get { return PackageSet; }
            set { PackageSet = (DbSet<Package>)value; }
        }

        public IQueryable<Bundle> Bundles
        {
            get { return BundleSet; }
            set { BundleSet = (DbSet<Bundle>) value; }
        }

        public IQueryable<Group> Groups
        {
            get { return GroupSet; }
            set { GroupSet = (DbSet<Group>)value; }
        }

        public IQueryable<Role> Roles
        {
            get { return RoleSet; }
            set { RoleSet = (IDbSet<Role>)value; }
        }

        public IQueryable<User> Users
        {
            get { return UserSet; }
            set { UserSet = (IDbSet<User>)value; }
        }

        public IQueryable<Company> Company
        {
            get { return CompanySet; }
            set { CompanySet = (IDbSet<Company>)value; }
        }

        public IQueryable<Setting> Settings
        {
            get { return SettingSet; }
            set { SettingSet = (IDbSet<Setting>)value; }
        }

        public IQueryable<ReportFile> ReportFiles
        {
            get { return ReportFileSet; }
            set { ReportFileSet = (IDbSet<ReportFile>)value; }
        }

        public IQueryable<ErrorLogs> ErrorLogses
        {
            get { return ErrorLogsSet; }
            set { ErrorLogsSet = (IDbSet<ErrorLogs>)value; }
        }

        public IQueryable<DefaultConfiguration> DefualtConfigurations
        {
            get { return DefaultConfigurationSet; }
            set { DefaultConfigurationSet = value as IDbSet<DefaultConfiguration>; }
        }

        public IQueryable<RaceCodes> RaceCodes
        {
            get { return RaceCodesSet; }
            set { RaceCodesSet = value as IDbSet<RaceCodes>; }
        }

        public void Add<T>(T obj) where T : DomainObject
        {
            Set<T>().Add(obj);
        }

        public void Save()
        {
            base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //TODO : For Reference only

            modelBuilder.Entity<Group>().HasKey(x => x.Id).Property(x => x.Id)
               .HasColumnName("GroupId");

            modelBuilder.Entity<Company>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("CompanyId");

            modelBuilder.Entity<Package>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("PackageId");

            modelBuilder.Entity<Role>().HasKey(x => x.Id).Property(x => x.Id)
                .HasColumnName("RoleId");

            modelBuilder.Entity<User>().HasKey(x => x.Id)
                .Property(x => x.Id)
                .HasColumnName("UserId");

            modelBuilder.Entity<User>()
                .HasMany(x => x.Roles)
                .WithMany(x => x.Users)
                .Map(x =>
                {
                    x.ToTable("UsersInRole");
                    x.MapLeftKey("Id");
                    x.MapRightKey("RoleId");
                });

            modelBuilder.Entity<Group>().HasRequired<Company>(s => s.Company)
                    .WithMany(s => s.GroupList).HasForeignKey(s => s.CompanyId);

            modelBuilder.Entity<User>().HasRequired<Company>(s => s.Company)
                      .WithMany(s => s.UserList).HasForeignKey(s => s.CompanyId);

            modelBuilder.Entity<User>().HasOptional<Group>(s => s.Group)
                     .WithMany(s => s.UserList).HasForeignKey(s => s.GroupId);

            modelBuilder.Entity<Setting>().HasKey(x => x.Id);

            modelBuilder.Entity<ReportFile>().HasKey(x => x.Id);

            modelBuilder.Entity<ErrorLogs>().HasKey(x => x.Id);

            modelBuilder.Entity<UserLoginStats>().HasKey(x => x.Id);
            modelBuilder.Entity<UserLoginStats>().HasRequired(x => x.LoggedInUser).WithMany().WillCascadeOnDelete();

            modelBuilder.Entity<UserActivityLog>().HasKey(x => x.Id);
            modelBuilder.Entity<UserActivityLog>().Property(t => t.UserId).IsRequired();
            modelBuilder.Entity<UserActivityLog>().HasRequired(x => x.User).WithMany().WillCascadeOnDelete();
            modelBuilder.Entity<UserActivityLog>().Property(t => t.Description).IsOptional();

            modelBuilder.Entity<UserCorrespondenceLog>().HasKey(x => x.Id);
            modelBuilder.Entity<UserCorrespondenceLog>().HasRequired(x => x.User).WithMany().WillCascadeOnDelete();
            modelBuilder.Entity<DefaultConfiguration>().HasMany(x => x.Trophys).WithMany().Map(
                x =>
                {
                    x.ToTable("DefaultConfigurationIdTrophyId");
                    x.MapLeftKey("DefaultConfigurationId");
                    x.MapRightKey("Upload_Id");
                });
            modelBuilder.Entity<Company>().HasOptional(x => x.CustomColours).WithRequired(x => x.Company);
            base.OnModelCreating(modelBuilder);
        }

        public IQueryable<TEntity> GetSet<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }
    }
}