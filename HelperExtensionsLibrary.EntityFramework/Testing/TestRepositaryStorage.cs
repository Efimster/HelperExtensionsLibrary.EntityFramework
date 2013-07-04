using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using HelperExtensionsLibrary.Objects;
using HelperExtensionsLibrary.IEnumerable;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public partial class TestRepository<T> 
           where T : class
    {
        /// <summary>
        /// Represents test storage
        /// </summary>
        public class TestRepositaryStorage
        {
            private class TransactionData
            {
                /// <summary>
                /// The latest transaction commited data set
                /// </summary>
                public IList<Item> CommitedSet { get; set; }
                /// <summary>
                /// Transactional lock. Preserves transaction from being used by more than one repository simultaneously
                /// </summary>
                public TransactionalLock Locker { get; set; }
            }
            /// <summary>
            /// Dictionary of transaction - commited set & transactional lock pairs
            /// </summary>
            IDictionary<Transaction, TransactionData> Transactions { get; set; }
            /// <summary>
            /// Persistant data set
            /// </summary>
            private IList<Item> PersistantSet { get; set; }


            public TestRepositaryStorage()
            {
                PersistantSet = new List<Item>();
                Transactions = new Dictionary<Transaction, TransactionData>();
            }


            /// <summary>
            /// Returns storage shot for given repository object
            /// </summary>
            /// <param name="repository">repository</param>
            /// <returns>storage snapshot</returns>
            public IList<Item> GetSnapshot(TestRepository<T> repository)
            {
                var transaction = EnlistToTransaction(repository);

                if (transaction!=null && transaction.IsolationLevel  == IsolationLevel.ReadCommitted)
                {
                    var value = Transactions[transaction];
                    var commited = value.CommitedSet;
                    
                    if (commited != null)
                    {
                        var commitedSet = commited.DeepCopyByJSON();
                        Persist(commitedSet);
                        return commitedSet;
                    }
                }

                return PersistantSet.DeepCopyByJSON();
            }

            /// <summary>
            /// Process data set to be used as persistant one
            /// </summary>
            /// <param name="dbSet">data set</param>
            /// <param name="prepareOnly">true: doesn't change data set item state</param>
            /// <returns>affected rows</returns>
            private static int Persist(IList<Item> dbSet, bool prepareOnly = false)
            {
                int affectedRows = 0;

                for (int i = dbSet.Count - 1; i >= 0; i--)
                {
                    var item = dbSet[i];
                    if (item.State.IsIn(ItemState.Removed))
                    {
                        dbSet.RemoveAt(i);
                        affectedRows++;
                        continue;
                    }

                    if (item.State.IsNotIn(ItemState.Unchanged))
                    {
                        item.TriggerDbGeneratedActons();
                        item.TriggerMinMaxLengthConstraintActons();
                        if (!prepareOnly)
                            item.State = ItemState.Unchanged;
                        affectedRows++;
                    }
                }


                return affectedRows;
            }


            /// <summary>
            /// Commit repository working set
            /// </summary>
            /// <param name="repository">repository</param>
            /// <param name="dbSet">repository working set</param>
            /// <returns>affected rows</returns>
            public int Commit(TestRepository<T> repository, IList<Item> dbSet)
            {
                try
                {
                    var transaction = Transaction.Current;

                    if (transaction == null)
                        return DbSetToPersistant(dbSet);

                    TransactionData value;
                    if (!Transactions.TryGetValue(transaction, out value))
                    {
                        EnlistToTransaction(repository);
                    }

                    value.CommitedSet = dbSet.DeepCopyByJSON();
                    return Persist(dbSet, prepareOnly: true);
                }
                catch (InvalidOperationException exc)
                {
                    throw new System.Data.Entity.Core.EntityException("The underlying provider failed on open", exc);
                }
            }
            /// <summary>
            /// Persist data set
            /// </summary>
            /// <param name="dbSet">data set</param>
            /// <param name="prepared">true: if data set already prepared to be persist</param>
            /// <returns>affected rows</returns>
            private int DbSetToPersistant(IList<Item> dbSet, bool prepared = false)
            {
                int affectedRows = 0;
                
                if (dbSet == null)
                    return affectedRows;
                
                if (!prepared)
                    affectedRows = Persist(dbSet);
                else
                    dbSet.ForEach(item => item.State = ItemState.Unchanged);

                PersistantSet = dbSet.DeepCopyByJSON();
                return affectedRows;
            }
            /// <summary>
            /// Enlists to transaction
            /// </summary>
            /// <param name="repository">enlisted repositary</param>
            /// <returns>transaction if any</returns>
            protected Transaction EnlistToTransaction(TestRepository<T> repository)
            {
                try
                {
                    var transaction = Transaction.Current;
                    if (transaction == null)
                        return transaction;

                    TransactionalLock tlock;
                    TransactionData value;

                    if (!Transactions.TryGetValue(transaction, out value))
                    {
                        tlock = new TransactionalLock();
                        Transactions.Add(transaction, new TransactionData() { CommitedSet = null, Locker = tlock });
                    }
                    else
                    {
                        tlock = value.Locker;
                    }

                    tlock.Lock();
                    transaction.EnlistVolatile(repository, EnlistmentOptions.EnlistDuringPrepareRequired);
                    transaction.TransactionCompleted += repository.Transaction_TransactionCompleted;


                    return transaction;
                }
                catch (InvalidOperationException exc)
                {
                    throw new System.Data.Entity.Core.EntityException("The underlying provider failed on open", exc);
                }
            }
            /// <summary>
            /// Transaction completed event handler
            /// </summary>
            /// <param name="transaction">completed transaction</param>
            public void Transaction_TransactionCompleted(Transaction transaction)
            {
                if (transaction.TransactionInformation.Status == TransactionStatus.Aborted)
                {
                    Transactions.Remove(transaction);
                    return;
                }
                
                var value = Transactions[transaction];
                Transactions.Remove(transaction);
                DbSetToPersistant(value.CommitedSet, prepared: true);
                value.Locker.Unlock();
            }

        }
    }
}
