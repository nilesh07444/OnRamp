namespace Data.EF.Customer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_section_option_in_TestQuestionTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TestQuestions", "CheckRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.TestQuestions", "AttachmentRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.TestQuestions", "NoteAllow", c => c.Boolean(nullable: false));
            AddColumn("dbo.TestQuestions", "dynamicFields", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TestQuestions", "dynamicFields");
            DropColumn("dbo.TestQuestions", "NoteAllow");
            DropColumn("dbo.TestQuestions", "AttachmentRequired");
            DropColumn("dbo.TestQuestions", "CheckRequired");
        }
    }
}
