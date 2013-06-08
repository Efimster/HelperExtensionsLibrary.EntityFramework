
namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public interface ITestRepositary<T> : IRepositary<T>
    {
        void Clear();
    }
}
