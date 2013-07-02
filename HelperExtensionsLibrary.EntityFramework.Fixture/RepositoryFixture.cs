using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.EntityFramework.Fixture.Migrations;
using HelperExtensionsLibrary.EntityFramework.Fixture.Models;
using HelperExtensionsLibrary.EntityFramework.Ninject;
using Should.Fluent;
using Xunit;
using Xunit.Extensions;
using HelperExtensionsLibrary.EntityFramework.Testing;

namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class RepositoryFixture
    {
        public RepositoryFixture()
        {
            InitDb();
            InitializeRepoDispenser();
        }


        public static void InitDb()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\App_Data");
            path = Path.GetFullPath(path);
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
            
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestContext, Configuration>());
            using (var context = new TestContext())
            {
                context.Database.Initialize(force:false);
            }
        }
        
        protected virtual IRepository<TestModel> GetNewDbRepository()
        {
            return new TestModelDbRepository(new TestContext());
        }

        protected SharedRepositories Repositories { get; set; }
        protected RepositoriesDispenser RepoBuilder { get; set; }
        IRepository<TestModel> TestModelRepo { get { return Repositories.GetRepository<TestModel>(); } }
        IRepository<TestModel7> TestModel7Repo { get { return Repositories.GetRepository<TestModel7>(); } }
        IRepository<TestModel8> TestModel8Repo { get { return Repositories.GetRepository<TestModel8>(); } }

        protected virtual void InitializeRepoDispenser()
        {
            RepoBuilder = new RepositoriesDispenser(new SharedRepositoriesFixture.DependenciesDBTestModule());
            Repositories = RepoBuilder.Share();
        }
        
        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        [Fact]
        [Trace]
        protected virtual void GetAllFixture()
        {
            ClearRepository();
            var models = new[] {
                    new TestModel() { TestId = 1, TestData = "data1" },
                    new TestModel() { TestId = 2, TestData = "data2" } ,
                    new TestModel() { TestId = 3, TestData = "data3" }
                };

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.AddRange(models).UpdateAll();
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var list = repository.GetAll(entity => entity.TestId == 1);
                list.Should().Count.Exactly(1);
                list[0].TestId.Should().Equal(1);

                list[0].Should().Not.Be.SameAs(models[0]);
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var list = repository.GetAll("TestId=1");
                list.Should().Count.Exactly(1);
                list[0].TestId.Should().Equal(1);

                list = repository.GetAll("TestData='data2'");
                list.Should().Count.Exactly(1);
                list[0].TestId.Should().Equal(2);

                list = repository.GetAll("TestData like 'data%'");
                list.Should().Count.Exactly(3);
            }

            ClearRepository();
        }
        /// <summary>
        /// Get entity by predicate
        /// </summary>
        [Fact]
        [Trace]
        protected virtual void GetOneFixture()
        {
            ClearRepository();
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.AddOne(new TestModel() { TestId = 1, TestData = "data1" })
                .AddOne(new TestModel() { TestId = 2, TestData = "data2" })
                .AddOne(new TestModel() { TestId = 3, TestData = "data3" })
                .UpdateAll();
            }

            TestModel one;
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                one = repository.GetOne(entity => entity.TestId == 1);
                one.Should().Not.Be.Null();
                one.TestId.Should().Equal(1);
                repository.GetOne(entity => entity.TestId == 1).Should().Be.SameAs(one);
           }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.GetOne(entity => entity.TestId == 1).Should().Not.Be.SameAs(one);
            }

            ClearRepository();
        }
        /// <summary>
        /// Add entity to repository
        /// </summary>
        [Fact]
        [Trace]
        protected virtual void AddOneFixture()
        {
            ClearRepository();
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.AddOne(new TestModel() { TestId = 1, TestData = "data1" }).UpdateAll();
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var one = repository.GetOne(entity => entity.TestId == 1);
                one.Should().Not.Be.Null();
                one.TestId.Should().Equal(1);
            }

            ClearRepository();
        }
        /// <summary>
        ///  Remove  entity
        /// </summary>
        [Fact]
        [Trace]
        protected virtual void DeleteOneFixture()
        {
            ClearRepository();
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.AddOne(new TestModel() { TestId = 1, TestData = "data1" }).UpdateAll();
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var one = new TestModel() { TestId = 1}; 
                Assert.Throws<InvalidOperationException>(() => repository.DeleteOne(one));
                //repository.AttachOne(one);
                repository.DeleteOne(one, attache: true, autoupdate: true);
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var one = repository.GetOne(entity => entity.TestId == 1);
                one.Should().Be.Null();
            }
            ClearRepository();
        }
        /// <summary>
        /// Remove entities range
        /// </summary>
        [Fact]
        [Trace]
        protected virtual void DeleteRangeFixture()
        {
            ClearRepository();
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.AddOne(new TestModel() { TestId = 1, TestData = "data1" }, false);
                repository.AddOne(new TestModel() { TestId = 2, TestData = "data2" }, false);
                repository.AddOne(new TestModel() { TestId = 3, TestData = "data3" }, false);
                repository.UpdateAll();
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.DeleteRange(repository.GetAll(x => true), autoupdate: true);
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var list = repository.GetAll(entity => entity.TestId == 1);
                list.Should().Count.Zero();
            }
            //---------------------- attache -----------------------------------
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.AddOne(new TestModel() { TestId = 1, TestData = "data1" })
                .AddOne(new TestModel() { TestId = 2, TestData = "data2" })
                .AddOne(new TestModel() { TestId = 3, TestData = "data3" })
                .UpdateAll().Should().Equal(3);
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var models = new List<TestModel> { new TestModel() { TestId = 1 },
                new TestModel() { TestId = 2 },
                new TestModel() { TestId = 3 }};

                repository.DeleteRange(models, attache: true).UpdateAll();
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var list = repository.GetAll();
                list.Should().Count.Zero();
            }

            //-------------------- predicate -------------------

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.AddOne(new TestModel() { TestId = 1, TestData = "data1" })
                .AddOne(new TestModel() { TestId = 2, TestData = "data2" })
                .AddOne(new TestModel() { TestId = 3, TestData = "data3" })
                .UpdateAll().Should().Equal(3);
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.DeleteRange(x=> x.TestId == 1 || x.TestId == 2).UpdateAll();
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var list = repository.GetAll();
                list.Should().Count.One();
                list[0].TestId.Should().Equal(3);
                repository.DeleteRange(x=>true).UpdateAll();
                list = repository.GetAll();
                list.Should().Count.Zero();
            }

        }

        protected virtual void ClearRepository()
        {
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                repository.DeleteRange(repository.GetAll(x => true), autoupdate: true);
            }
        }

        protected virtual void ClearSharedRepositories()
        {
            using (Repositories.Use())
            {
                TestModelRepo.DeleteRange(TestModelRepo.GetAll(x => true), autoupdate: true);
                TestModel8Repo.DeleteRange(TestModel8Repo.GetAll(x => true), autoupdate: true);
                TestModel7Repo.DeleteRange(TestModel7Repo.GetAll(x => true), autoupdate: true);
                
            }
        }

        [Fact]
        [Trace]
        protected virtual void IsAttachedFixture()
        {
            ClearRepository();
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var model = new TestModel() { TestId = 1, TestData = "data1" };

                repository.IsAttached(model).Should().Be.False();
                repository.AttachOne(model);
                repository.IsAttached(model).Should().Be.True();
                
            }

            ClearRepository();

        }

        [Fact]
        protected virtual void AddRangeFixture()
        {
            ClearRepository();
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var models = new List<TestModel> { new TestModel() { TestId = 1, TestData = "data1" },
                new TestModel() { TestId = 2, TestData = "data1" },
                new TestModel() { TestId = 3, TestData = "data1" }};

                repository.AddRange(models).UpdateAll();
            }

            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var one = repository.GetOne(entity => entity.TestId == 1);
                one.Should().Not.Be.Null();
                one.TestId.Should().Equal(1);
            }
            
            ClearRepository();
        }

        [Fact]
        protected virtual void MinMaxConstraintsFixture()
        {
            ClearRepository();
            using (IRepository<TestModel> repository = GetNewDbRepository())
            {
                var model = new TestModel() { TestId = 1, TestData = "MaxLength constraint exceded" };
                
                Assert.Throws<UpdateException>(()=>repository.AddOne(model, autoupdate: true));
                repository.DeleteOne(model).UpdateAll();
                repository.GetOne(x => x.TestId == 1).Should().Be.Null();
                model.TestData = "M";
                repository.AddOne(model).UpdateAll();//Min length constraint hasn't been checked!!!!!
                repository.DeleteOne(model).UpdateAll();
                model.TestData = null;
                repository.AddOne(model).UpdateAll();
                repository.DeleteOne(model, attache: true).UpdateAll();
            }
            ClearRepository();
        }


        [Fact]
        protected void IncludeFixture()
        {
            ClearSharedRepositories();

            using (Repositories.Use())
            {
                var model7 = new TestModel7() { Model7Id = 7, DataString = "data7", DataInt = 7, DataDateTime = DateTime.Now, NullableDateTime = DateTime.Now };
                TestModel7Repo.AddOne(model7).UpdateAll();


                var model8 = new TestModel8() { Id = 8, Model7Id = 7, DataString = "data8", DataInt = 8, DataDateTime = DateTime.Now, NullableDateTime = DateTime.Now };
                TestModel8Repo.AddOne(model8).UpdateAll();
            }
            using (Repositories.Use())
            {
                var res = TestModel8Repo.GetAllAndInclude(m8 => m8.Id == 8, m8 => m8.TestModel7Object, TestModel7Repo).FirstOrDefault();
                res.Should().Not.Be.Null();
                res.TestModel7Object.DataString.Should().Equal("data7");
                res.TestModel7Object.DataInt.Should().Equal(7);
            }
            
            using (Repositories.Use())
            {
                var res = TestModel8Repo.AsQueryable().Where(m8 => m8.Id == 8).Include(m8 => m8.TestModel7Object, TestModel7Repo).FirstOrDefault();
                res.Should().Not.Be.Null();
                res.TestModel7Object.DataString.Should().Equal("data7");
                res.TestModel7Object.DataInt.Should().Equal(7);
                
            }

            ClearSharedRepositories();
        }

    }
}
