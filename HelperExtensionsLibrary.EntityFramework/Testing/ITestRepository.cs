
namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public interface ITestRepository<T> : IRepository<T>
    {
        /// <summary>
        /// Clear all repository data
        /// </summary>
        void Clear();
    }
}
