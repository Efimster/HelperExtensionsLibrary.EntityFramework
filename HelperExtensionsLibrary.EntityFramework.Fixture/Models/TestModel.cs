using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class TestModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TestId { get; set; }
        public string TestData { get; set; }
    }

    public class TestModel2
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TestId { get; set; }
        public string TestData { get; set; }
    }
    public class TestModel3
    {
        [Key, Column(Order = 0)]
        public int TestId { get; set; }
        [Key, Column(Order = 1)]
        public int TestId2 { get; set; }
        public string TestData { get; set; }
    }

    public class TestModel4
    {
        public int TestId { get; set; }
        public int TestId2 { get; set; }
        public string TestData { get; set; }
    }

    public class TestModel5
    {
        [Key]
        public int TestId { get; set; }
        public string TestData { get; set; }
    }

    public class TestModel6
    {
        [Key]
        public int Id { get; set; }
        public int TestId { get; set; }
        public string TestData { get; set; }
        [ForeignKey("TestId")]
        public TestModel5 TestModelObject { get; set; }
    }
}
