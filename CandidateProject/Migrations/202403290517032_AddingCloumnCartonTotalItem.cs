namespace CandidateProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingCloumnCartonTotalItem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Carton", "CartonTotalItem", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Carton", "CartonTotalItem");
        }
    }
}
