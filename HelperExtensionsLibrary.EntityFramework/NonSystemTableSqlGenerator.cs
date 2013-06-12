using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;

namespace HelperExtensionsLibrary.EntityFramework
{
    /// <summary>
    /// Prevents creating tables in System schema \r\n
    /// Use SetSqlGenerator("System.Data.SqlClient", new NonSystemTableSqlGenerator()); in migration Configuration constructor
    /// </summary>
    public class NonSystemTableSqlGenerator : SqlServerMigrationSqlGenerator
    {
        
        //EF 6.0
        protected override void GenerateMakeSystemTable(CreateTableOperation createTableOperation, System.Data.Entity.Migrations.Utilities.IndentedTextWriter writer)
        {

        }
        ///// <summary>
        ///// Overwritten empty implementation
        ///// </summary>
        ///// <param name="createTableOperation"></param>
        //protected override void GenerateMakeSystemTable(CreateTableOperation createTableOperation)
        //{
        //}

    }
}
