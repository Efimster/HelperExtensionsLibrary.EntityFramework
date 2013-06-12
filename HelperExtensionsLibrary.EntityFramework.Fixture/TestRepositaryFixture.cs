using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.EntityFramework.Testing;
using Should.Fluent;
using Xunit;

namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class TestRepositaryFixture : RepositaryFixture
    {
        protected override void ClearRepositary()
        {
            using (var repositary = GetNewDbRepositary())
            {
                ((ITestRepositary<TestModel>)repositary).Clear();
            }
        }

        protected override IRepositary<TestModel> GetNewDbRepositary()
        {
            return new TestRepositary<TestModel>();
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
        /// Add entity to repositary
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
    }
}
