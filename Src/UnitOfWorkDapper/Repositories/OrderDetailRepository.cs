using Dapper;
using Domain.Entities;
using System.Data;

namespace UnitOfWorkDapper.Repositories
{
    public interface IOrderDetailRepository
    {
        IEnumerable<OrderDetail> GetAllOrderDetails();
        OrderDetail GetOrderDetailById(int id);
        int AddOrderDetail(OrderDetail orderDetail);
        int UpdateOrderDetail(OrderDetail orderDetail);
        int DeleteOrderDetail(int id);
        void SetTransaction(IDbTransaction transaction);
    }

    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        public OrderDetailRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void SetTransaction(IDbTransaction transaction)
        {
            _transaction = transaction;
        }

        public IEnumerable<OrderDetail> GetAllOrderDetails()
        {
            string sql = "SELECT * FROM OrderDetails";
            return _connection.Query<OrderDetail>(sql);
        }

        public OrderDetail GetOrderDetailById(int id)
        {
            string sql = "SELECT * FROM OrderDetails WHERE Id = @Id";
            return _connection.QuerySingleOrDefault<OrderDetail>(sql, new { Id = id });
        }

        public int AddOrderDetail(OrderDetail orderDetail)
        {
            string sql = "INSERT INTO OrderDetails (OrderId, ProductName, Quantity, Price) VALUES (@OrderId, @ProductName, @Quantity, @Price); SELECT last_insert_rowid();";
            return _connection.ExecuteScalar<int>(sql, orderDetail, _transaction); // Consistently return the inserted ID
        }

        public int UpdateOrderDetail(OrderDetail orderDetail)
        {
            string sql = "UPDATE OrderDetails SET ProductName = @ProductName, Quantity = @Quantity, Price = @Price WHERE Id = @Id";
            return _connection.Execute(sql, orderDetail, _transaction); // Return the number of rows affected
        }

        public int DeleteOrderDetail(int id)
        {
            string sql = "DELETE FROM OrderDetails WHERE Id = @Id";
            return _connection.Execute(sql, new { Id = id }, _transaction); // Return the number of rows affected
        }
    }
}
