using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace HelperExtensionsLibrary.EntityFramework.Fixture
{
    public class TestContext : DbContext
    {
       
        public TestContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<TestModel> Tests { get; set; }
        public DbSet<TestModel2> Tests2 { get; set; }
        public DbSet<TestModel3> Tests3 { get; set; }
        public DbSet<TestModel7> Tests7 { get; set; }
        public DbSet<TestModel8> Tests8 { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            
            
            //if (IgnoreUser)
            //    modelBuilder.Ignore<UserProfile>(); 
        }

    }
}
