namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Modified_section_option_in_CustomDocument : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MemoContentBoxes", "AttachmentRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.MemoContentBoxes", "IsAttached", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingManualChapters", "AttachmentRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingManualChapters", "IsAttached", c => c.Boolean(nullable: false));
            DropColumn("dbo.MemoContentBoxes", "CheckRequired");
            DropColumn("dbo.MemoContentBoxes", "IsChecked");
            DropColumn("dbo.TrainingManualChapters", "CheckRequired");
            DropColumn("dbo.TrainingManualChapters", "IsChecked");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TrainingManualChapters", "IsChecked", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingManualChapters", "CheckRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.MemoContentBoxes", "IsChecked", c => c.Boolean(nullable: false));
            AddColumn("dbo.MemoContentBoxes", "CheckRequired", c => c.Boolean(nullable: false));
            DropColumn("dbo.TrainingManualChapters", "IsAttached");
            DropColumn("dbo.TrainingManualChapters", "AttachmentRequired");
            DropColumn("dbo.MemoContentBoxes", "IsAttached");
            DropColumn("dbo.MemoContentBoxes", "AttachmentRequired");
        }
    }
}
