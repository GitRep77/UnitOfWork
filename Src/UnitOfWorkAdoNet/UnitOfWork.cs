using System;
using System.Data;
using UnitOfWorkAdoNet.Repositories;

namespace UnitOfWorkAdoNet
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        IOrderDetailRepository OrderDetails { get; }
        IOrderRepository Orders { get; }

        void BeginTransaction();
        void Commit();
        void Dispose();
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
        /// Explicitly begin a new transaction.
        /// </summary>
        public void BeginTransaction()
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                _transaction = _connection.BeginTransaction();
                SetTransactionInRepositories(_transaction);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while beginning the transaction.", ex);
            }
        }

        /// <summary>
        /// Commit the current transaction.
        /// </summary>
        public void Commit()
        {
            try
            {
                _transaction?.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred during commit. The transaction has not been successfully committed.", ex);
            }
            finally
            {
                ResetTransactionInRepositories();
            }
        }

        /// <summary>
        /// Rollback the current transaction.
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
        /// Dispose of the transaction and connection.
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        private void SetTransactionInRepositories(IDbTransaction transaction)
        {
            Customers.SetTransaction(transaction);
            Orders.SetTransaction(transaction);
            OrderDetails.SetTransaction(transaction);
        }

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
