using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.EntityFramework.Ninject;
using Xunit;
using Should.Fluent;
using HelperExtensionsLibrary.EntityFramework.Testing;

namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class SharedRepositoriesFixture
    {
        public class DependenciesDBTestModule : RepositoriesNinjectModule
        {
            public override void Load()
            {
                Bind<TestContext>().ToMethod(context => new TestContext(/**/));
                RegisterGeneralRepository<TestModel, TestContext>();
                RegisterGeneralRepository<TestModel2, TestContext>();
                Bind<IRepository<TestModel3>>().To<GeneralRepository<TestModel3, TestContext>>();
                RegisterGeneralRepository<TestModel7, TestContext>();
                RegisterGeneralRepository<TestModel8, TestContext>();
                
                base.Load();

            }
        }

        public class DependenciesTestModule : RepositoriesNinjectModule
        {
            public override void Load()
            {
                Bind<IRepository<TestModel>>().To<TestRepository<TestModel>>();
                Bind<IRepository<TestModel2>>().To<TestRepository<TestModel2>>();
                Bind<IRepository<TestModel3>>().To<TestRepository<TestModel3>>();
                Bind<IRepository<TestModel7>>().To<TestRepository<TestModel7>>();
                Bind<IRepository<TestModel8>>().To<TestRepository<TestModel8>>();
            }
        }

        RepositoriesDispenser repoBuilder;


        public SharedRepositoriesFixture()
        {
            
        }

        SharedRepositories Repositories { get; set; }
        Lazy<IRepository<TestModel>> TestModelRepoLazy { get; set; }
        Lazy<IRepository<TestModel2>> TestModel2RepoLazy { get; set; }
        IRepository<TestModel> TestModelRepo { get { return TestModelRepoLazy.Value; } }
        IRepository<TestModel2> TestModel2Repo { get { return TestModel2RepoLazy.Value; } }

        
        
        [Fact]
        public void SharedRerositaryUsageRealDbFixture()
        {
            RepositoryFixture.InitDb();



            var module = new DependenciesDBTestModule();
            //module.RegisterGeneralRepository<TestModel, TestContext>();
            repoBuilder = new RepositoriesDispenser(module);
            //repoBuilder.RegisterGeneralRepository<TestModel2, TestContext>();

            Repositories = repoBuilder.Share();


            SharedRerositoryMainPartTest();
        }
        [Fact]
        public void SharedRerositaryUsageInMemoryFixture()
        {
            repoBuilder = new RepositoriesDispenser(new DependenciesTestModule());
            Repositories = repoBuilder.Share();
            SharedRerositoryMainPartTest();
        }

        private void SharedRerositoryMainPartTest()
        {
            
            TestModelRepoLazy = new Lazy<IRepository<TestModel>>(() => Repositories.GetRepository<TestModel>());
            TestModel2RepoLazy = new Lazy<IRepository<TestModel2>>(() => Repositories.GetRepository<TestModel2>());


            IDisposable disposable;

            using (disposable = Repositories.Use())
            {
                disposable.Should().Be.OfType<SharedRepositories>();

                TestModelRepo.DeleteRange(TestModelRepo.GetAll()).UpdateAll();
                TestModel2Repo.DeleteRange(TestModel2Repo.GetAll()).UpdateAll();

                TestModelRepo.AddOne(new TestModel() { TestId = 1, TestData = "blabla" }).UpdateAll();

                using (disposable = Repositories.Use())
                {
                    disposable.Should().Be.OfType<SharedRepositories.FakeDisposable>();
                    TestModel2Repo.AddOne(new TestModel2() { TestId = 1, TestData = "blabla" }).UpdateAll();
                }

                ((object)TestModelRepo).Should().Be.SameAs(Repositories.GetRepository<TestModel>())
                    .Should().Be.SameAs(Repositories.GetRepository<TestModel>());


                TestModelRepo.DeleteRange(TestModelRepo.GetAll()).UpdateAll();
                TestModel2Repo.DeleteRange(TestModel2Repo.GetAll()).UpdateAll();
            }
        }
    }
}
