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
    public class TestRepositaryItemFixture
    {
        [Fact]
        public void GetDatabaseGeneratedIdentitysFixture()
        {
            var list2 = TestRepositary<TestModel2>.Item.GetDatabaseGeneratedIdentitys();
            list2.Should().Count.Exactly(1);
            var list1 = TestRepositary<TestModel>.Item.GetDatabaseGeneratedIdentitys();
            list1.Should().Count.Exactly(0);
            var list3 = TestRepositary<TestModel3>.Item.GetDatabaseGeneratedIdentitys();
            list3.Should().Count.Exactly(0);
        }

        [Fact]
        public void GetKeyDataFixture()
        {
            var list = TestRepositary<TestModel2>.Item.GetKeyData();
            list.Should().Count.Exactly(1);
            list = TestRepositary<TestModel>.Item.GetKeyData();
            list.Should().Count.Exactly(1);
            list = TestRepositary<TestModel3>.Item.GetKeyData();
            list.Should().Count.Exactly(2);

            list = TestRepositary<TestModel4>.Item.GetKeyData();
            list.Should().Count.Exactly(0);
        }

        [Fact]
        public void EquelsFixture()
        {
            var item1 = new TestRepositary<TestModel3>.Item() { Value = new TestModel3() { TestId = 1, TestId2 = 2, TestData = "TestData1" } };
            var item2 = new TestRepositary<TestModel3>.Item() { Value = new TestModel3() { TestId = 1, TestId2 = 2, TestData = "TestData2" } };
            var item3 = new TestRepositary<TestModel3>.Item() { Value = new TestModel3() { TestId = 2, TestId2 = 2, TestData = "TestData1" } };

            item1.Should().Equal(item2);
            item1.Should().Not.Equal(item3);

            var item4 = new TestRepositary<TestModel4>.Item() { Value = new TestModel4() { TestId = 1, TestId2 = 2, TestData = "TestData1" } };
            var item5 = new TestRepositary<TestModel4>.Item() { Value = new TestModel4() { TestId = 1, TestId2 = 2, TestData = "TestData2" } };
            var item6 = new TestRepositary<TestModel4>.Item() { Value = new TestModel4() { TestId = 1, TestId2 = 2, TestData = "TestData1" } };

            item4.Should().Not.Equal(item5);
            item4.Should().Equal(item6);
        }

    }
}
