using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HelperExtensionsLibrary.EntityFramework
{
    /// <summary>
    /// General Repository Interface
    /// </summary>
    /// <typeparam name="T">Type of repository items</typeparam>
    public interface IRepository<T> : IDisposable, IEnumerable<T>
    {
        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>list of entities</returns>
        IList<T> GetAll(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Get entity by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <param name="single">true- repository should contain at most one suited entity </param>
        /// <returns>entity</returns>
        T GetOne(Expression<Func<T, bool>> predicate, bool single = true);
        /// <summary>
        /// Add entity to repository
        /// </summary>
        /// <param name="one">entity</param>
        /// <param name="autoupdate">commit changes</param>
        IRepository<T> AddOne(T one, bool autoupdate = false);
        /// <summary>
        /// Add entities range to repository
        /// </summary>
        /// <param name="range">range of entities</param>
        /// <param name="autoupdate">commit changes</param>
        IRepository<T> AddRange(IEnumerable<T> range, bool autoupdate = false);
        /// <summary>
        /// Commit changes
        /// </summary>
        /// <returns>affected rows count</returns>
        int UpdateAll();
        /// <summary>
        ///  Remove  entity
        /// </summary>
        /// <param name="one">Entity</param>
        /// <param name="autoupdate">Commit changes</param>
        /// <param name="attache">true:attache entity. Default false.</param>
        IRepository<T> DeleteOne(T one, bool autoupdate = false, bool attache = false);
        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        /// <param name="filterString">filter string</param>
        /// <returns>list of entities</returns>
        IList<T> GetAll(string filterStr);
        /// <summary>
        /// Remove entities range
        /// </summary>
        /// <param name="range">range of entities</param>
        /// <param name="autoupdate">commit changes</param>
        /// <param name="attache">true:attaches entity. Default false.</param>
        /// <returns>repository</returns>
        IRepository<T> DeleteRange(IEnumerable<T> range, bool autoupdate = false, bool attache = false);
        /// <summary>
        /// Removes entities filtered by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <param name="autoupdate">commit changes</param>
        /// <returns>repository</returns>
        IRepository<T> DeleteRange(Expression<Func<T, bool>> predicate, bool autoupdate = false);
        /// <summary>
        /// Get list of all entities 
        /// </summary>
        /// <returns>list of entities</returns>
        IList<T> GetAll();
        /// <summary>
        /// Attach entity to context
        /// </summary>
        /// <param name="one">entity</param>
        ///<returns>entity</returns>
        T AttachOne(T one);
        /// <summary>
        /// Checks whether entity is attached to context 
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true: attached, false: not attached</returns>
        bool IsAttached(T entity);
        /// <summary>
        /// Indicates whether repository has been disposed
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// Makes repository queryable
        /// </summary>
        /// <returns>queryable interface</returns>
        IQueryable<T> AsQueryable();
        /// <summary>
        /// Makes repository queryable and filter by filter string
        /// </summary>
        /// <param name="filterString">filtering string</param>
        /// <returns>queryable interface</returns>
        IQueryable<T> AsQueryable(string filterString);
        /// <summary>
        /// Makes repository queryable and filter by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>queryable interface</returns>
        IQueryable<T> AsQueryable(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Enumerates repository items filterd by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>enumerable list</returns>
        IEnumerable<T> AsEnumerable(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Enumerates repository items filtered by filter string
        /// </summary>
        /// <param name="filterString">filtering string</param>
        /// <returns>enumerable list</returns>
        IEnumerable<T> AsEnumerable(string filterString);
        /// <summary>
        /// Enumerates repository items
        /// </summary>
        /// <returns>enumerable list</returns>
        IEnumerable<T> AsEnumerable();
        /// <summary>
        /// Filter the sequence of values base on a predicate
        /// and Specifies the related objects to include in the query results
        /// </summary>
        /// <typeparam name="TProperty">entity refernced by foreign key</typeparam>
        /// <param name="predicate">predicate</param>
        /// <param name="includePath">The related object to return in the query results.</param>
        /// <param name="foreignSource">source of foreign type entities</param>
        /// <returns>A new System.Linq.IQueryable<T> with the defined query path.</returns>
        IQueryable<T> GetAllAndInclude<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> includePath, IEnumerable<TProperty> foreignSource) where TProperty : class;

    }
}
