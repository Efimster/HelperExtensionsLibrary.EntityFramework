namespace HelperExtensionsLibrary.EntityFramework.Fixture.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialdatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestModels",
                c => new
                    {
                        TestId = c.Int(nullable: false),
                        TestData = c.String(),
                    })
                .PrimaryKey(t => t.TestId);
            
            CreateTable(
                "dbo.TestModel2",
                c => new
                    {
                        TestId = c.Int(nullable: false, identity: true),
                        TestData = c.String(),
                    })
                .PrimaryKey(t => t.TestId);
            
            CreateTable(
                "dbo.TestModel3",
                c => new
                    {
                        TestId = c.Int(nullable: false),
                        TestId2 = c.Int(nullable: false),
                        TestData = c.String(),
                    })
                .PrimaryKey(t => new { t.TestId, t.TestId2 });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TestModel3");
            DropTable("dbo.TestModel2");
            DropTable("dbo.TestModels");
        }
    }
}
