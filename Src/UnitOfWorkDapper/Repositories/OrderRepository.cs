using Dapper;
using Domain.Entities;
using System.Data;

namespace UnitOfWorkDapper.Repositories
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetAllOrders();
        Order GetOrderById(int id);
        int AddOrder(Order order);
        int UpdateOrder(Order order);
        int DeleteOrder(int id);
        void SetTransaction(IDbTransaction transaction);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        public OrderRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void SetTransaction(IDbTransaction transaction)
        {
            _transaction = transaction;
        }

        public IEnumerable<Order> GetAllOrders()
        {
            string sql = "SELECT * FROM Orders";
            return _connection.Query<Order>(sql);
        }

        public Order GetOrderById(int id)
        {
            string sql = "SELECT * FROM Orders WHERE Id = @Id";
            return _connection.QuerySingleOrDefault<Order>(sql, new { Id = id });
        }

        public int AddOrder(Order order)
        {
            string sql = "INSERT INTO Orders (CustomerId, OrderDate) VALUES (@CustomerId, @OrderDate); SELECT last_insert_rowid();";
            return _connection.ExecuteScalar<int>(sql, order, _transaction); // Consistently return the inserted ID
        }

        public int UpdateOrder(Order order)
        {
            string sql = "UPDATE Orders SET CustomerId = @CustomerId, OrderDate = @OrderDate WHERE Id = @Id";
            return _connection.Execute(sql, order, _transaction); // Return the number of rows affected
        }

        public int DeleteOrder(int id)
        {
            string sql = "DELETE FROM Orders WHERE Id = @Id";
            return _connection.Execute(sql, new { Id = id }, _transaction); // Return the number of rows affected
        }
    }
}
