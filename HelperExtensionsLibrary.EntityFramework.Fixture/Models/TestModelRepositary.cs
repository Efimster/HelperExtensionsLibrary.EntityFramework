using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperExtensionsLibrary.EntityFramework.Fixture.Models
{
    public class TestModelDbRepository : RepositoryBase<TestModel>
    {
        public TestModelDbRepository(TestContext context)
            : base(context, context.Tests)
        {
            
        }
    }

    
}
