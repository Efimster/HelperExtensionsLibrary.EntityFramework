using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public class TransactionalLock
    {
        LinkedList<KeyValuePair<Transaction, ManualResetEvent>> PendingTransactions { get; set; }

        // Property provides thread-safe access to m_OwningTransaction 
        Transaction OwningTransaction { get; set; }

        public TransactionalLock()
        {
            PendingTransactions = new LinkedList<KeyValuePair<Transaction, ManualResetEvent>>();
        }

        public bool Locked { get { return OwningTransaction != null; } }
        
        public void Lock()
        {
            Lock(Transaction.Current);

        }

        void Lock(Transaction transaction)
        {
            Monitor.Enter(this);
            if (OwningTransaction == null)
            {
                //Acquire the transaction lock if(transaction != null) 
                OwningTransaction = transaction;
                Monitor.Exit(this);
                return;
            }
            else //Some transaction owns the lock 
            {
                //We're done if it's the same one as the method parameter 
                if (OwningTransaction == transaction)
                {
                    Monitor.Exit(this);
                    return;
                } //Otherwise, need to acquire the transaction lock 
                else
                {
                    ManualResetEvent manualEvent = new ManualResetEvent(false);
                    KeyValuePair<Transaction, ManualResetEvent> pair = new KeyValuePair<Transaction, ManualResetEvent>(transaction, manualEvent);
                    PendingTransactions.AddLast(pair);
                    if (transaction != null)
                    {
                        transaction.TransactionCompleted += (sender, e) =>
                        {
                            lock (this)
                            {
                                //Pair may have already been removed if unlocked 
                                PendingTransactions.Remove(pair);
                            }
                            lock (manualEvent)
                            {
                                if (!manualEvent.SafeWaitHandle.IsClosed)
                                {
                                    manualEvent.Set();
                                }
                            }
                        };
                    }
                    Monitor.Exit(this);
                    //Block the transaction or the calling thread
                    manualEvent.WaitOne();
                    lock (manualEvent)
                        manualEvent.Close();
                }
            }
        }


        public void Unlock()
        {
            Debug.Assert(Locked); 
            lock (this)
            {
                OwningTransaction = null;
                LinkedListNode<KeyValuePair<Transaction, ManualResetEvent>> node = null;
                if (PendingTransactions.Count > 0)
                {
                    node = PendingTransactions.First;
                    PendingTransactions.RemoveFirst();
                }
                if (node != null)
                {
                    Transaction transaction = node.Value.Key;
                    ManualResetEvent manualEvent = node.Value.Value;
                    Lock(transaction);
                    lock (manualEvent)
                    {
                        if (!manualEvent.SafeWaitHandle.IsClosed)
                        {
                            manualEvent.Set();
                        }
                    }
                }
            }
        }
    }
}
