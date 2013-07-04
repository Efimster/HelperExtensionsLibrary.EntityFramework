using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;


namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public partial class TestRepository<T> : IEnlistmentNotification
    {
        /// <summary>
        /// Precess transaction commit action
        /// </summary>
        /// <param name="enlistment">current repositary enlistment</param>
        public void Commit(Enlistment enlistment)
        {
            enlistment.Done();
            if (IsDisposed)
                Dispose();
        }
        /// <summary>
        /// Process transaction 'in doubt' status
        /// </summary>
        /// <param name="enlistment">current repositary enlistment</param>
        public void InDoubt(Enlistment enlistment)
        {
            Commit(enlistment);
        }
        /// <summary>
        /// Prepares for transaction commit or rollback actions
        /// </summary>
        /// <param name="preparingEnlistment">current repositary enlistment</param>
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }
        /// <summary>
        /// Precess transaction rollback action
        /// </summary>
        /// <param name="enlistment">current repositary enlistment</param>
        public void Rollback(Enlistment enlistment)
        {
            enlistment.Done();
            if (IsDisposed)
                Dispose();
        }
        /// <summary>
        /// Transaction comleted event handler
        /// </summary>
        /// <param name="sender">transaction</param>
        /// <param name="e">event arguments</param>
        public void Transaction_TransactionCompleted(object sender, TransactionEventArgs e)
        {
            
            Storage.Transaction_TransactionCompleted((Transaction)sender);
        }
    }


    
}
