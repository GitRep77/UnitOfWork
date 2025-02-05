using System;
using System.Data;
using UnitOfWorkDapper.Repositories;

namespace UnitOfWorkDapper
{
    public interface IUnitOfWork : IDisposable
    {
        ICustomerRepository Customers { get; }
        IOrderRepository Orders { get; }
        IOrderDetailRepository OrderDetails { get; }

        void BeginTransaction();
        void Commit();
        void Rollback();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        public ICustomerRepository Customers { get; }
        public IOrderRepository Orders { get; }
        public IOrderDetailRepository OrderDetails { get; }

        public UnitOfWork(
            IDbConnection connection,
            ICustomerRepository customerRepository,
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository)
        {
            _connection = connection;
            Customers = customerRepository;
            Orders = orderRepository;
            OrderDetails = orderDetailRepository;
        }

        /// <summary>
        /// Manually begin a new transaction.
        /// </summary>
        public void BeginTransaction()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            _transaction = _connection.BeginTransaction();
            SetTransactionInRepositories(_transaction);
        }

        /// <summary>
        /// Commit the currently active transaction.
        /// </summary>
        public void Commit()
        {
            try
            {
                _transaction?.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during commit. The transaction was not successfully committed.", ex);
            }
            finally
            {
                ResetTransactionInRepositories();
            }
        }

        /// <summary>
        /// Rollback the currently active transaction.
        /// </summary>
        public void Rollback()
        {
            try
            {
                _transaction?.Rollback();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during rollback.", ex);
            }
            finally
            {
                ResetTransactionInRepositories();
            }
        }

        /// <summary>
        /// Dispose the transaction and connection.
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        /// <summary>
        /// Assign the same transaction to all repositories.
        /// </summary>
        private void SetTransactionInRepositories(IDbTransaction transaction)
        {
            Customers.SetTransaction(transaction);
            Orders.SetTransaction(transaction);
            OrderDetails.SetTransaction(transaction);
        }

        /// <summary>
        /// Clear out the transaction from the repositories.
        /// </summary>
        private void ResetTransactionInRepositories()
        {
            _transaction?.Dispose();
            _transaction = null;

            Customers.SetTransaction(null);
            Orders.SetTransaction(null);
            OrderDetails.SetTransaction(null);
        }
    }
}
