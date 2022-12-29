namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocumentTrainingLabelsCommaSeparatedString : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MemoLabels", "Memo_Id", "dbo.Memos");
            DropForeignKey("dbo.MemoLabels", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.PolicyLabels", "Policy_Id", "dbo.Policies");
            DropForeignKey("dbo.PolicyLabels", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.TestLabels", "Test_Id", "dbo.Tests");
            DropForeignKey("dbo.TestLabels", "Label_Id", "dbo.Labels");
            DropForeignKey("dbo.TrainingManualLabels", "TrainingManual_Id", "dbo.TrainingManuals");
            DropForeignKey("dbo.TrainingManualLabels", "Label_Id", "dbo.Labels");
            DropIndex("dbo.MemoLabels", new[] { "Memo_Id" });
            DropIndex("dbo.MemoLabels", new[] { "Label_Id" });
            DropIndex("dbo.PolicyLabels", new[] { "Policy_Id" });
            DropIndex("dbo.PolicyLabels", new[] { "Label_Id" });
            DropIndex("dbo.TestLabels", new[] { "Test_Id" });
            DropIndex("dbo.TestLabels", new[] { "Label_Id" });
            DropIndex("dbo.TrainingManualLabels", new[] { "TrainingManual_Id" });
            DropIndex("dbo.TrainingManualLabels", new[] { "Label_Id" });
            AddColumn("dbo.Memos", "TrainingLabels", c => c.String());
            AddColumn("dbo.Policies", "TrainingLabels", c => c.String());
            AddColumn("dbo.Tests", "TrainingLabels", c => c.String());
            AddColumn("dbo.TrainingManuals", "TrainingLabels", c => c.String());
            DropTable("dbo.MemoLabels");
            DropTable("dbo.PolicyLabels");
            DropTable("dbo.TestLabels");
            DropTable("dbo.TrainingManualLabels");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TrainingManualLabels",
                c => new
                    {
                        TrainingManual_Id = c.String(nullable: false, maxLength: 128),
                        Label_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TrainingManual_Id, t.Label_Id });
            
            CreateTable(
                "dbo.TestLabels",
                c => new
                    {
                        Test_Id = c.String(nullable: false, maxLength: 128),
                        Label_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Test_Id, t.Label_Id });
            
            CreateTable(
                "dbo.PolicyLabels",
                c => new
                    {
                        Policy_Id = c.String(nullable: false, maxLength: 128),
                        Label_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Policy_Id, t.Label_Id });
            
            CreateTable(
                "dbo.MemoLabels",
                c => new
                    {
                        Memo_Id = c.String(nullable: false, maxLength: 128),
                        Label_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Memo_Id, t.Label_Id });
            
            DropColumn("dbo.TrainingManuals", "TrainingLabels");
            DropColumn("dbo.Tests", "TrainingLabels");
            DropColumn("dbo.Policies", "TrainingLabels");
            DropColumn("dbo.Memos", "TrainingLabels");
            CreateIndex("dbo.TrainingManualLabels", "Label_Id");
            CreateIndex("dbo.TrainingManualLabels", "TrainingManual_Id");
            CreateIndex("dbo.TestLabels", "Label_Id");
            CreateIndex("dbo.TestLabels", "Test_Id");
            CreateIndex("dbo.PolicyLabels", "Label_Id");
            CreateIndex("dbo.PolicyLabels", "Policy_Id");
            CreateIndex("dbo.MemoLabels", "Label_Id");
            CreateIndex("dbo.MemoLabels", "Memo_Id");
            AddForeignKey("dbo.TrainingManualLabels", "Label_Id", "dbo.Labels", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrainingManualLabels", "TrainingManual_Id", "dbo.TrainingManuals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TestLabels", "Label_Id", "dbo.Labels", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TestLabels", "Test_Id", "dbo.Tests", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PolicyLabels", "Label_Id", "dbo.Labels", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PolicyLabels", "Policy_Id", "dbo.Policies", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MemoLabels", "Label_Id", "dbo.Labels", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MemoLabels", "Memo_Id", "dbo.Memos", "Id", cascadeDelete: true);
        }
    }
}
