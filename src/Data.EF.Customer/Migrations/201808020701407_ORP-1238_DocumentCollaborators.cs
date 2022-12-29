namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ORP1238_DocumentCollaborators : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MemoStandardUsers",
                c => new
                    {
                        Memo_Id = c.String(nullable: false, maxLength: 128),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Memo_Id, t.StandardUser_Id })
                .ForeignKey("dbo.Memos", t => t.Memo_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.Memo_Id)
                .Index(t => t.StandardUser_Id);
            
            CreateTable(
                "dbo.PolicyStandardUsers",
                c => new
                    {
                        Policy_Id = c.String(nullable: false, maxLength: 128),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Policy_Id, t.StandardUser_Id })
                .ForeignKey("dbo.Policies", t => t.Policy_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.Policy_Id)
                .Index(t => t.StandardUser_Id);
            
            CreateTable(
                "dbo.TestStandardUsers",
                c => new
                    {
                        Test_Id = c.String(nullable: false, maxLength: 128),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Test_Id, t.StandardUser_Id })
                .ForeignKey("dbo.Tests", t => t.Test_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.Test_Id)
                .Index(t => t.StandardUser_Id);
            
            CreateTable(
                "dbo.TrainingManualStandardUsers",
                c => new
                    {
                        TrainingManual_Id = c.String(nullable: false, maxLength: 128),
                        StandardUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.TrainingManual_Id, t.StandardUser_Id })
                .ForeignKey("dbo.TrainingManuals", t => t.TrainingManual_Id, cascadeDelete: true)
                .ForeignKey("dbo.StandardUsers", t => t.StandardUser_Id, cascadeDelete: true)
                .Index(t => t.TrainingManual_Id)
                .Index(t => t.StandardUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrainingManualStandardUsers", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.TrainingManualStandardUsers", "TrainingManual_Id", "dbo.TrainingManuals");
            DropForeignKey("dbo.TestStandardUsers", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.TestStandardUsers", "Test_Id", "dbo.Tests");
            DropForeignKey("dbo.PolicyStandardUsers", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.PolicyStandardUsers", "Policy_Id", "dbo.Policies");
            DropForeignKey("dbo.MemoStandardUsers", "StandardUser_Id", "dbo.StandardUsers");
            DropForeignKey("dbo.MemoStandardUsers", "Memo_Id", "dbo.Memos");
            DropIndex("dbo.TrainingManualStandardUsers", new[] { "StandardUser_Id" });
            DropIndex("dbo.TrainingManualStandardUsers", new[] { "TrainingManual_Id" });
            DropIndex("dbo.TestStandardUsers", new[] { "StandardUser_Id" });
            DropIndex("dbo.TestStandardUsers", new[] { "Test_Id" });
            DropIndex("dbo.PolicyStandardUsers", new[] { "StandardUser_Id" });
            DropIndex("dbo.PolicyStandardUsers", new[] { "Policy_Id" });
            DropIndex("dbo.MemoStandardUsers", new[] { "StandardUser_Id" });
            DropIndex("dbo.MemoStandardUsers", new[] { "Memo_Id" });
            DropTable("dbo.TrainingManualStandardUsers");
            DropTable("dbo.TestStandardUsers");
            DropTable("dbo.PolicyStandardUsers");
            DropTable("dbo.MemoStandardUsers");
        }
    }
}
