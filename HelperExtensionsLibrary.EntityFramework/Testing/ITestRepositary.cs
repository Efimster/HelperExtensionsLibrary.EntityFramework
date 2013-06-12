
namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public interface ITestRepositary<T> : IRepositary<T>
    {
        /// <summary>
        /// Clear all repositary data
        /// </summary>
        void Clear();
    }
}
