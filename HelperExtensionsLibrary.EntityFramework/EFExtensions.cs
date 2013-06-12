using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using HelperExtensionsLibrary.IEnumerable;

namespace HelperExtensionsLibrary.EntityFramework
{
    /// <summary>
    /// Entity Framework extesions
    /// </summary>
    public static partial class EFExtensions
    {
        #region GetTableName
        /// <summary>
        /// Returns Entity table name
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TContext">DataBase context type</typeparam>
        /// <param name="shemaIncluded">true: with schema</param>
        /// <returns>table name</returns>
        public static string GetTableName<T, TContext>(bool shemaIncluded = true)
            where T : class
            where TContext : DbContext, new()
        {
            return GetTableName<T>((IObjectContextAdapter)new TContext(), shemaIncluded);
        }
        /// <summary>
        /// Returns Entity table name
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="objectContext"></param>
        /// <param name="shemaIncluded">>true: with schema</param>
        /// <returns>table name</returns>
        public static string GetTableName<T>(IObjectContextAdapter objectContext, bool shemaIncluded = true)
            where T : class
        {
            var entitySet = GetEntitySet<T>(objectContext);
            if (entitySet == null)
                throw new Exception("Unable to find entity set '{0}' in edm metadata " + (typeof(T).Name));

            var tableName = GetStringProperty(entitySet, "Table");
            if (shemaIncluded)
                tableName = GetStringProperty(entitySet, "Schema") + "." + tableName;
            return tableName;
        }

        /// <summary>
        /// Get entity set metadata property value
        /// </summary>
        /// <param name="entitySet">metadata</param>
        /// <param name="propertyName">name of property</param>
        /// <returns>string property value</returns>
        private static string GetStringProperty(MetadataItem entitySet, string propertyName)
        {
            MetadataProperty property;
            if (entitySet == null)
                throw new ArgumentNullException("entitySet");

            if (entitySet.MetadataProperties.TryGetValue(propertyName, false, out property))
            {
                string str = null;
                if (((property != null) &&
                    (property.Value != null)) &&
                    (((str = property.Value as string) != null) &&
                    !string.IsNullOrEmpty(str)))
                {
                    return str;
                }
            }
            return string.Empty;
        }


        /// <summary>
        /// Resolve entity set from database context
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="objectContext">object context adapter</param>
        /// <returns>entity set</returns>
        private static EntitySet GetEntitySet<T>(IObjectContextAdapter objectContext)
        {
            var type = typeof(T);
            var entityName = type.Name;
            var metadata = objectContext.ObjectContext.MetadataWorkspace;

            IEnumerable<EntitySet> entitySets;
            entitySets = metadata.GetItemCollection(DataSpace.SSpace)
                             .GetItems<EntityContainer>()
                             .Single()
                             .BaseEntitySets
                             .OfType<EntitySet>()
                             .Where(s => !s.MetadataProperties.Contains("Type")
                                         || s.MetadataProperties["Type"].ToString() == "Tables");
            var entitySet = entitySets.FirstOrDefault(t => t.Name == entityName);
            return entitySet;
        }

        #endregion
        /// <summary>
        /// Add filter to sql query
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="set">Entity set</param>
        /// <param name="tableName">Entity database table name</param>
        /// <param name="filterStr">Sql query "Where" clause</param>
        /// <returns>Sql query</returns>
        public static DbSqlQuery<TEntity> SqlFilterQuery<TEntity>(this DbSet<TEntity> set, string tableName, string filterStr)
            where TEntity : class
        {
            var str = string.Concat("select * from ", tableName);
            if (!string.IsNullOrWhiteSpace(filterStr))
                str = string.Concat(str, " where ", filterStr);

            return set.SqlQuery(str);
        }
        /// <summary>
        /// Add filter to sql query
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="set"></param>
        /// <param name="tableName"></param>
        /// <param name="op"></param>
        /// <param name="filters"></param>
        /// <returns>Sql query</returns>
        public static DbSqlQuery<TEntity> SqlFilterQuery<TEntity, TKey, TValue>(this DbSet<TEntity> set, string tableName, string op = "and", params KeyValuePair<TKey, TValue>[] filters)
            where TEntity : class
        {
            string filter = string.Empty;

            switch (typeof(TValue).Name.ToLower())
            {
                case "string": filter = BuildSqlFilterQuery((x => string.Concat("'", x, "'")), op, filters); break;
                case "datetime": filter = BuildSqlFilterQuery((x => string.Concat("'", x.ToString(), "'")), op, filters); break;
                default: filter = BuildSqlFilterQuery((x => x.ToString()), op, filters); break;

            }

            return set.SqlFilterQuery(tableName, filter);
        }

        private static string BuildSqlFilterQuery<TKey, TValue>(
            Func<TValue, string> valuefomat,
            string op,
            params KeyValuePair<TKey, TValue>[] filters)
        {
            var sb = new StringBuilder(filters.Length << 4);

            filters.Take(1).ForEach(filter => sb.Append(string.Concat(filter.Key, "=", valuefomat(filter.Value))));
            filters.Skip(1).ForEach(filter => sb.Append(string.Concat(" ", op, " ", filter.Key, "=", valuefomat(filter.Value))));

            return sb.ToString();
        }
        /// <summary>
        /// Returns connection string of database context
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <returns></returns>
        public static string GetConnectionString<TContext>()
            where TContext : DbContext, new()
        {
            return new TContext().Database.Connection.ConnectionString;

        }

    }
}
