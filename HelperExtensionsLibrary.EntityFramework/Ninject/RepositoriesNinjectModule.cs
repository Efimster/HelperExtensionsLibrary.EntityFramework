using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;

namespace HelperExtensionsLibrary.EntityFramework.Ninject
{
    /// <summary>
    /// Ninject module with general repository registration feature
    /// </summary>
    public class RepositoriesNinjectModule : NinjectModule
    {

        private List<Action> BindGeneralRepositoriesList {get; set;}

        public void RegisterGeneralRepository<TUser, TContext>()
            where TContext : DbContext
            where TUser : class
        {

            if (BindGeneralRepositoriesList == null)
                BindGeneralRepositoriesList = new List<Action>(1);
            
            BindGeneralRepositoriesList.Add(() =>
            {
                Bind<IRepository<TUser>>().To<GeneralRepository<TUser, TContext>>();
            });
        }


        public override void Load()
        {
            if (BindGeneralRepositoriesList != null)
            {
                foreach (var action in BindGeneralRepositoriesList)
                    action();
            }

        }
    }
}
