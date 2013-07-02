using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Activation.Blocks;

namespace HelperExtensionsLibrary.EntityFramework.Ninject
{
    public class NinjectActivationBlock : ActivationBlock
    {

        public IRepository<TUser> GetUserRepository<TUser>() where TUser : class
        {
            return this.Get<IRepository<TUser>>();
        }

        public NinjectActivationBlock(IKernel kernal)
            : base(kernal)
        {
        }


    }
}
