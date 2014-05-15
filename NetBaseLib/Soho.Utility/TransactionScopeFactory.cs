using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Soho.Utility
{
    public class TransactionScopeFactory
    {
        public static TransactionScope CreateTransactionScope(TransactionScopeOption scopeOpion, IsolationLevel level)
        {
            if (Transaction.Current != null)
            {
                level = Transaction.Current.IsolationLevel;
            }
            return new TransactionScope(scopeOpion, new TransactionOptions { IsolationLevel = level });
        }

        public static TransactionScope CreateTransactionScope(TransactionScopeOption scopeOpion)
        {
            return CreateTransactionScope(scopeOpion, IsolationLevel.Serializable);
        }

        public static TransactionScope CreateTransactionScope()
        {
            return CreateTransactionScope(TransactionScopeOption.Required);
        }

        public static void TransactionAction(Action action, TransactionScopeOption scopeOpion, IsolationLevel level)
        {
            if (action != null)
            {
                using (TransactionScope scope = CreateTransactionScope(scopeOpion, level))
                {
                    action();
                    scope.Complete();
                }
            }
        }

        public static void TransactionAction(Action action, TransactionScopeOption scopeOpion)
        {
            if (action != null)
            {
                using (TransactionScope scope = CreateTransactionScope(scopeOpion))
                {
                    action();
                    scope.Complete();
                }
            }
        }

        public static void TransactionAction(Action action)
        {
            if (action != null)
            {
                using (TransactionScope scope = CreateTransactionScope())
                {
                    action();
                    scope.Complete();
                }
            }
        }
    }
}
