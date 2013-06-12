using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HelperExtensionsLibrary.EntityFramework
{
    /// <summary>
    /// General Repositary Interface
    /// </summary>
    /// <typeparam name="T">Type of repositary items</typeparam>
    public interface IRepositary<T> : IDisposable
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
        /// <param name="single">true- repositary should contain at most one suited entity </param>
        /// <returns>entity</returns>
        T GetOne(Expression<Func<T, bool>> predicate, bool single = true);
        /// <summary>
        /// Add entity to repositary
        /// </summary>
        /// <param name="one">entity</param>
        /// <param name="autoupdate">commit changes</param>
        void AddOne(T one, bool autoupdate = true);
        /// <summary>
        /// Commit changes
        /// </summary>
        void UpdateAll();
        /// <summary>
        ///  Remove  entity
        /// </summary>
        /// <param name="one">Entity</param>
        /// <param name="autoupdate">Commit changes</param>
        void DeleteOne(T one, bool autoupdate = true);
        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>list of entities</returns>
        IList<T> GetAll(string filterStr);
        /// <summary>
        /// Remove entities range
        /// </summary>
        /// <param name="range">range of entities</param>
        /// <param name="autoupdate">Commit changes</param>
        void DeleteRange(IEnumerable<T> range, bool autoupdate = true);
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

    }
}
