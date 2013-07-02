using System;
using System.Data.Entity;
using System.Diagnostics.Contracts;
using Ninject;
using Ninject.Activation.Blocks;

namespace HelperExtensionsLibrary.EntityFramework.Ninject
{
    
    /// <summary>
    /// Represents the set of repositories
    /// </summary>
    public class SharedRepositories : IDisposable
    {
        /// <summary>
        /// Fake empty disposable object
        /// </summary>
        public class FakeDisposable : IDisposable { public void Dispose() { } };
        /// <summary>
        /// Ninject Kernal
        /// </summary>
        private IKernel Kernal { get; set; }
        /// <summary>
        /// Ninject activation block
        /// </summary>
        ActivationBlock Block { get; set; }
        /// <summary>
        /// Returns repository 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            return Block.Get<IRepository<TEntity>>();
        }


        /// <summary>
        /// set ninject kernal
        /// </summary>
        /// <param name="kernal">ninject kernal</param>
        public void SetNinjectKernal(IKernel kernal)
        {
            Kernal = kernal;
        }

        /// <summary>
        /// Creates disposable object for <using> block 
        /// </summary>
        /// <returns>disposable object</returns>
        public IDisposable Use()
        {
            if (Block != null && !Block.IsDisposed)
                return new FakeDisposable();

            Contract.Requires(Kernal!=null, "Ninject kernal should not be null");
            
            Block = new NinjectActivationBlock(Kernal);
            ExtendUsageInitialization();

            return this;
        }
        /// <summary>
        /// Perform additional initialization actions
        /// </summary>
        protected virtual void ExtendUsageInitialization()
        {
            
        }


        /// <summary>
        /// Dispose Repositories
        /// </summary>
        public void Dispose()
        {
            if (Block == null)
                return;

            Block.Dispose();
            Block = null;

        }
    }
}
