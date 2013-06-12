using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using HelperExtensionsLibrary.EntityFramework.Fixture.Migrations;
using HelperExtensionsLibrary.IEnumerable;
using Should.Fluent;
using Xunit;


namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class EFExtentionsFixture
    {
        
        public EFExtentionsFixture()
        {
            var path =  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\App_Data");
            path = Path.GetFullPath(path);
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
            InitDb();
        }


        public void InitDb()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestContext, Configuration>());
            using (var context = new TestContext())
            {
                context.Database.Initialize(force:false);
            }
        }
         
        [Fact]
        public void GetTableNameFixture()
        {
            EFExtensions.GetTableName<TestModel, TestContext>(false).Should().Equal("TestModels");
            EFExtensions.GetTableName<TestModel, TestContext>(true).Should().Equal("dbo.TestModels");

        }

        [Fact]
        public void SqlFilterQueryFixture()
        {
            ClearRepositary();
            using (var context = new TestContext())
            {
                context.Tests.Add(new TestModel() {TestId = 1, TestData = "data1"});
                context.Tests.Add(new TestModel() { TestId = 2, TestData = "data2" });
                context.SaveChanges();
            }

            using (var context = new TestContext())
            {
                var list = context.Tests.SqlFilterQuery(EFExtensions.GetTableName<TestModel, TestContext>(true), "TestId=1").ToList();
                list.Should().Count.Exactly(1);
                list[0].TestId.Should().Equal(1);

            }
            ClearRepositary();
        }

        private void ClearRepositary()
        {
            using (var context = new TestContext())
            {
                context.Tests.Where(entity => true).ForEach(entity => context.Tests.Remove(entity));
                context.SaveChanges();
            }
        }


    }
}
