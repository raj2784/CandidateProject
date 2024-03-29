namespace CandidateProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartonDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CartonId = c.Int(nullable: false),
                        EquipmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Carton", t => t.CartonId)
                .ForeignKey("dbo.Equipment", t => t.EquipmentId)
                .Index(t => t.CartonId)
                .Index(t => t.EquipmentId);
            
            CreateTable(
                "dbo.Carton",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CartonNumber = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Equipment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ModelTypeId = c.Int(nullable: false),
                        SerialNumber = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ModelType", t => t.ModelTypeId)
                .Index(t => t.ModelTypeId);
            
            CreateTable(
                "dbo.ModelType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TypeName = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Equipment", "ModelTypeId", "dbo.ModelType");
            DropForeignKey("dbo.CartonDetail", "EquipmentId", "dbo.Equipment");
            DropForeignKey("dbo.CartonDetail", "CartonId", "dbo.Carton");
            DropIndex("dbo.Equipment", new[] { "ModelTypeId" });
            DropIndex("dbo.CartonDetail", new[] { "EquipmentId" });
            DropIndex("dbo.CartonDetail", new[] { "CartonId" });
            DropTable("dbo.ModelType");
            DropTable("dbo.Equipment");
            DropTable("dbo.Carton");
            DropTable("dbo.CartonDetail");
        }
    }
}
