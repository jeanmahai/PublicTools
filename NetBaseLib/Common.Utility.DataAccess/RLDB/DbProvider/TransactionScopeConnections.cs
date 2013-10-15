using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Transactions;
using System.Threading;
using System.Configuration;

namespace Common.Utility.DataAccess.Database.DbProvider
{
    internal static class TransactionScopeConnections
    {
        [ThreadStatic]
        private static Dictionary<Transaction, Dictionary<string, DbConnection>> s_Dictionary = null;
        private static Dictionary<Transaction, Dictionary<string, DbConnection>> GetTransactionConnectionDictionary()
        {
            string name = ConfigurationManager.AppSettings["Transaction_Scope_Name"];
            if (name == null || name.Trim().Length <= 0)
            {
                name = "Transaction_Scope";
            }
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot(name);
            if (slot == null)
            {
                slot = Thread.AllocateNamedDataSlot(name);
            }
            var transactionConnections = Thread.GetData(slot) as Dictionary<Transaction, Dictionary<string, DbConnection>>;
            if (transactionConnections == null)
            {
                transactionConnections = new Dictionary<Transaction, Dictionary<string, DbConnection>>();
                Thread.SetData(slot, transactionConnections);
            }
            return transactionConnections;
        }

        /// <summary>
        ///		Returns a connection for the current transaction. This will be an existing <see cref="DbConnection"/>
        ///		instance or a new one if there is a <see cref="TransactionScope"/> active. Otherwise this method
        ///		returns null.
        /// </summary>
        /// <param name="db"></param>
        /// <returns>Either a <see cref="DbConnection"/> instance or null.</returns>
        private static DbConnection GetConnection(string connectionString, Func<DbConnection> creator)
        {
            Transaction currentTransaction = Transaction.Current;

            if (currentTransaction == null)
            {
                return null;
            }

            if (s_Dictionary == null)
            {
                s_Dictionary = GetTransactionConnectionDictionary();
            }
            Dictionary<string, DbConnection> connectionList;
            s_Dictionary.TryGetValue(currentTransaction, out connectionList);

            DbConnection connection;
            if (connectionList != null)
            {
                connectionList.TryGetValue(connectionString, out connection);
                if (connection != null)
                {
                    return connection;
                }
            }
            else
            {
                // We don't have a list for this transaction, so create a new one
                connectionList = new Dictionary<string, DbConnection>();
                s_Dictionary.Add(currentTransaction, connectionList);
            }

            //
            // Next we'll see if there is already a connection. If not, we'll create a new connection and add it
            // to the transaction's list of connections.
            //
            if (connectionList.ContainsKey(connectionString))
            {
                connection = connectionList[connectionString];
            }
            else
            {
                connection = creator();
                connection.ConnectionString = connectionString;
                connection.Open();
                currentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(OnTransactionCompleted);
                connectionList.Add(connectionString, connection);
            }

            return connection;
        }

        /// <summary>
        ///		This event handler is called whenever a transaction is about to be disposed, which allows
        ///		us to remove the transaction from our list and dispose the connection instance we created.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnTransactionCompleted(object sender, TransactionEventArgs e)
        {
            Dictionary<string, DbConnection> connectionList; // = connections[e.Transaction];
            s_Dictionary.TryGetValue(e.Transaction, out connectionList);
            if (connectionList == null)
            {
                return;
            }
            s_Dictionary.Remove(e.Transaction);
            foreach (DbConnection conneciton in connectionList.Values)
            {
                try
                {
                    conneciton.Dispose();
                }
                catch { }
            }
        }

        public static ConnectionWrapper<DbConnection> GetOpenConnection(string connectionString, Func<DbConnection> creator,
            bool disposeInnerConnection = true)
        {
            DbConnection connection = TransactionScopeConnections.GetConnection(connectionString, creator);
            if (connection != null)
            {
                return new ConnectionWrapper<DbConnection>(connection, false);
            }
            else
            {
                try
                {
                    connection = creator();
                    connection.ConnectionString = connectionString;
                    connection.Open();
                }
                catch
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                    throw;
                }
                return new ConnectionWrapper<DbConnection>(connection, disposeInnerConnection);
            }
        }

        public static ConnectionWrapper<T> GetOpenConnection<T>(string connectionString, bool disposeInnerConnection = true)
            where T : DbConnection, new()
        {
            T connection = TransactionScopeConnections.GetConnection(connectionString, () => new T()) as T;
            if (connection != null)
            {
                return new ConnectionWrapper<T>(connection, false);
            }
            else
            {
                try
                {
                    connection = new T();
                    connection.ConnectionString = connectionString;
                    connection.Open();
                }
                catch
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                    throw;
                }
                return new ConnectionWrapper<T>(connection, disposeInnerConnection);
            }
        }
    }

    internal class ConnectionWrapper<T> : IDisposable where T : DbConnection
    {
        private T m_Connection;
        private bool m_DisposeConnection;

        /// <summary>
        ///		Create a new "lifetime" container for a <see cref="DbConnection"/> instance.
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="disposeConnection">
        ///		Whether or not to dispose of the connection when this class is disposed.
        ///	</param>
        public ConnectionWrapper(T connection, bool disposeConnection)
        {
            this.m_Connection = connection;
            this.m_DisposeConnection = disposeConnection;
        }

        /// <summary>
        ///		Gets the actual connection.
        /// </summary>
        public T Connection
        {
            get { return m_Connection; }
        }

        #region IDisposable Members

        /// <summary>
        ///		Dispose the wrapped connection, if appropriate.
        /// </summary>
        public void Dispose()
        {
            if (m_DisposeConnection)
            {
                try
                {
                    m_Connection.Close();
                    m_Connection.Dispose();
                }
                catch { }
            }
        }

        #endregion
    }
}
