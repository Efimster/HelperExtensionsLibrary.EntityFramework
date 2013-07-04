using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.EntityFramework.Ninject;
using HelperExtensionsLibrary.EntityFramework.Testing;
using Should.Fluent;
using Xunit;

namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class TestRepositoryFixture : RepositoryFixture
    {
        protected override void InitializeRepoDispenser()
        {
            RepoBuilder = new RepositoriesDispenser(new SharedRepositoriesFixture.DependenciesTestModule());
            Repositories = RepoBuilder.Share();
        }
        
        
        
        protected override void ClearRepository()
        {
            using (var repository = GetNewDbRepository())
            {
                ((ITestRepository<TestModel>)repository).Clear();
                repository.UpdateAll();
            }
        }

        protected override IRepository<TestModel> GetNewDbRepository()
        {
            return new TestRepository<TestModel>();
        }

        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        [Fact]
        protected override void GetAllFixture()
        {
            base.GetAllFixture();
        }
        /// <summary>
        /// Get entity by predicate
        /// </summary>
        [Fact]
        protected override void GetOneFixture()
        {
            base.GetOneFixture();
        }
        /// <summary>
        /// Add entity to repository
        /// </summary>
        [Fact]
        protected override void AddOneFixture()
        {
            base.AddOneFixture();
        }
        /// <summary>
        ///  Remove  entity
        /// </summary>
        [Fact]
        protected override void DeleteOneFixture()
        {
            base.DeleteOneFixture();
        }
        /// <summary>
        /// Remove entities range
        /// </summary>
        [Fact]
        protected override void DeleteRangeFixture()
        {
            base.DeleteRangeFixture();
        }

        [Fact]
        protected override void IsAttachedFixture()
        {
            base.IsAttachedFixture();
        }

        [Fact]
        protected override void AddRangeFixture()
        {
            base.AddRangeFixture();
        }

        [Fact]
        protected override void MinMaxConstraintsFixture()
        {
            base.MinMaxConstraintsFixture();
        }

        [Fact]
        protected override void IncludeFixture()
        {
            base.IncludeFixture();
        }

        [Fact]
        protected override void TransactionScopeFixture()
        {
            base.TransactionScopeFixture();
        }
    }
}