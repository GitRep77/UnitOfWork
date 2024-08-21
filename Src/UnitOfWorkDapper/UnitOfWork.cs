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

        void ExecuteInTransaction(Action action);

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

        public void ExecuteInTransaction(Action action)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                using (_transaction = _connection.BeginTransaction())
                {
                    SetTransactionInRepositories(_transaction);

                    try
                    {
                        action();
                        _transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            _transaction.Rollback();
                        }
                        catch (Exception rollbackEx)
                        {
                            throw new Exception("Rollback failed after an exception occurred during the transaction.", rollbackEx);
                        }

                        throw new Exception("An error occurred during the transaction. The transaction has been rolled back.", ex);
                    }
                    finally
                    {
                        ResetTransactionInRepositories();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error if needed
                throw new Exception("An error occurred while executing the transaction.", ex);
            }
        }

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

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}
