using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.EntityFramework.Fixture.Migrations;
using HelperExtensionsLibrary.EntityFramework.Fixture.Models;
using Should.Fluent;
using Xunit;

namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class RepositaryFixture
    {
        public RepositaryFixture()
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
        
        protected virtual IRepositary<TestModel> GetNewDbRepositary()
        {
            return new TestModelDbRepositary(new TestContext());
        }
        
        
        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        [Fact]
        protected virtual void GetAllFixture()
        {
            ClearRepositary();
            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                repositary.AddOne(new TestModel() { TestId = 1, TestData = "data1" }, false);
                repositary.AddOne(new TestModel() { TestId = 2, TestData = "data2" }, false);
                repositary.AddOne(new TestModel() { TestId = 3, TestData = "data3" }, false);
                repositary.UpdateAll();
            }

            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                var list = repositary.GetAll(entity => entity.TestId == 1);
                list.Should().Count.Exactly(1);
                list[0].TestId.Should().Equal(1);
            }

            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                var list = repositary.GetAll("TestId=1");
                list.Should().Count.Exactly(1);
                list[0].TestId.Should().Equal(1);
            }

            ClearRepositary();
        }
        /// <summary>
        /// Get entity by predicate
        /// </summary>
        [Fact]
        protected virtual void GetOneFixture()
        {
            ClearRepositary();
            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                repositary.AddOne(new TestModel() { TestId = 1, TestData = "data1" }, false);
                repositary.AddOne(new TestModel() { TestId = 2, TestData = "data2" }, false);
                repositary.AddOne(new TestModel() { TestId = 3, TestData = "data3" }, false);
                repositary.UpdateAll();
            }

            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                var one = repositary.GetOne(entity => entity.TestId == 1);
                one.Should().Not.Be.Null();
                one.TestId.Should().Equal(1);
            }

            ClearRepositary();
        }
        /// <summary>
        /// Add entity to repositary
        /// </summary>
        [Fact]
        protected virtual void AddOneFixture()
        {
            ClearRepositary();
            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                repositary.AddOne(new TestModel() { TestId = 1, TestData = "data1" });
            }

            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                var one = repositary.GetOne(entity => entity.TestId == 1);
                one.Should().Not.Be.Null();
                one.TestId.Should().Equal(1);
            }

            ClearRepositary();
        }
        /// <summary>
        ///  Remove  entity
        /// </summary>
        [Fact]
        protected virtual void DeleteOneFixture()
        {
            ClearRepositary();
            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                repositary.AddOne(new TestModel() { TestId = 1, TestData = "data1" });
            }

            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                var one = new TestModel() { TestId = 1}; 
                Assert.Throws<InvalidOperationException>(() => repositary.DeleteOne(one));
                repositary.AttachOne(one);
                repositary.DeleteOne(one);
            }

            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                var one = repositary.GetOne(entity => entity.TestId == 1);
                one.Should().Be.Null();
            }
            ClearRepositary();
        }
        /// <summary>
        /// Remove entities range
        /// </summary>
        [Fact]
        protected virtual void DeleteRangeFixture()
        {
            ClearRepositary();
            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                repositary.AddOne(new TestModel() { TestId = 1, TestData = "data1" }, false);
                repositary.AddOne(new TestModel() { TestId = 2, TestData = "data2" }, false);
                repositary.AddOne(new TestModel() { TestId = 3, TestData = "data3" }, false);
                repositary.UpdateAll();
            }

            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                repositary.DeleteRange(repositary.GetAll(x => true), true);
            }

            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                var list = repositary.GetAll(entity => entity.TestId == 1);
                list.Should().Count.Zero();
            }
        }

        protected virtual void ClearRepositary()
        {
            using (IRepositary<TestModel> repositary = GetNewDbRepositary())
            {
                repositary.DeleteRange(repositary.GetAll(x => true), true);
            }
        }
    }
}
