using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Should.Fluent;
using HelperExtensionsLibrary.EntityFramework.Testing;

namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class TestRepositaryHelperFixture
    {
        
        [Fact]
        public void ConstructForeignKeySetterFixture()
        {
            var action = TestRepositaryHelper.ConstructForeignKeySetter<TestModel6>("TestModelObject", "TestId");
            var model = new TestModel6() { TestId = 2 };
            action(model);
            model.TestId.Should().Equal(2);
            model.TestModelObject.Should().Not.Be.Null();
            model.TestModelObject.TestId.Should().Equal(2);

            model = new TestModel6() { TestModelObject = new TestModel5() { TestId = 3 } };
            action(model);
            model.TestId.Should().Equal(3);

        }

        [Fact]
        public void ConstructDbGeneratedIdentityFixture()
        {
            var action = TestRepositaryHelper.ConstructDbGeneratedIdentity<TestModel5>("TestId");
            var model = new TestModel5();
            action(model, 5);
            model.TestId.Should().Equal(5);


        }
    }
}
