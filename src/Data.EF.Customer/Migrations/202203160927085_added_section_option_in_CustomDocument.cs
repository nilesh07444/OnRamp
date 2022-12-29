namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_section_option_in_CustomDocument : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MemoContentBoxes", "CheckRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.MemoContentBoxes", "IsChecked", c => c.Boolean(nullable: false));
            AddColumn("dbo.MemoContentBoxes", "NoteAllow", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingManualChapters", "CheckRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingManualChapters", "IsChecked", c => c.Boolean(nullable: false));
            AddColumn("dbo.TrainingManualChapters", "NoteAllow", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TrainingManualChapters", "NoteAllow");
            DropColumn("dbo.TrainingManualChapters", "IsChecked");
            DropColumn("dbo.TrainingManualChapters", "CheckRequired");
            DropColumn("dbo.MemoContentBoxes", "NoteAllow");
            DropColumn("dbo.MemoContentBoxes", "IsChecked");
            DropColumn("dbo.MemoContentBoxes", "CheckRequired");
        }
    }
}
