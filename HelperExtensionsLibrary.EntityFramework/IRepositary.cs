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
        IList<T> GetAll(Expression<Func<T, bool>> predicate);
        T GetOne(Expression<Func<T, bool>> predicate, bool single = true);
        void AddOne(T one, bool autoupdate = true);
        void UpdateAll();
        void DeleteOne(T one, bool autoupdate = true);

    }
}
