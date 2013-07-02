using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;

namespace HelperExtensionsLibrary.EntityFramework.Ninject
{
    public class RepositoriesDispenser
    {
        /// <summary>
        /// Ninject Kernal
        /// </summary>
        private StandardKernel Kernal { get; set; }
        /// <summary>
        /// Ninject module
        /// </summary>
        protected RepositoriesNinjectModule DependenciesModule;

        public RepositoriesDispenser(RepositoriesNinjectModule module = null)
        {
            DependenciesModule = module ?? new RepositoriesNinjectModule();
            Initialize();
            Kernal = new StandardKernel(DependenciesModule);
        }

        /// <summary>
        /// Additional initialization
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        ///  Register trepository base on entity and db context
        /// </summary>
        /// <typeparam name="TEntity">entity</typeparam>
        /// <typeparam name="TContext">db context</typeparam>
        /// <param name="connectionString"></param>
        public void RegisterGeneralRepository<TEntity, TContext>()
            where TContext : DbContext
            where TEntity : class
        {
            DependenciesModule.RegisterGeneralRepository<TEntity, TContext>();
            Kernal.Dispose();
            Kernal = new StandardKernel(DependenciesModule);
        }
        /// <summary>
        /// Share newly created shared repositories
        /// </summary>
        /// <typeparam name="T">Type of shared repositories</typeparam>
        /// <returns>shared repositories</returns>
        public virtual T Share<T>() where T : SharedRepositories, new()
        {
            var sharedRepos = new T();
            sharedRepos.SetNinjectKernal(Kernal);
            return sharedRepos;
        }

        /// <summary>
        /// Share newly created shared repositories
        /// </summary>
        /// <returns>shared repositories</returns>
        public virtual SharedRepositories Share() 
        {
            var sharedRepos = new SharedRepositories();
            sharedRepos.SetNinjectKernal(Kernal);
            return sharedRepos;
        }
    }
}
