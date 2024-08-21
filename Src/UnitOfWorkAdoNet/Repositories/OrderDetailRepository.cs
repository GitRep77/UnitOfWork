using Domain.Entities;
using System.Data;
using Microsoft.Data.Sqlite;

namespace UnitOfWorkAdoNet.Repositories
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
            var orderDetails = new List<OrderDetail>();
            string sql = "SELECT * FROM OrderDetails";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orderDetails.Add(new OrderDetail
                        {
                            Id = reader.GetInt32(0),
                            OrderId = reader.GetInt32(1),
                            ProductName = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            Price = reader.GetDecimal(4)
                        });
                    }
                }
            }

            return orderDetails;
        }

        public OrderDetail GetOrderDetailById(int id)
        {
            string sql = "SELECT * FROM OrderDetails WHERE Id = @Id";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@Id", id));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new OrderDetail
                        {
                            Id = reader.GetInt32(0),
                            OrderId = reader.GetInt32(1),
                            ProductName = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            Price = reader.GetDecimal(4)
                        };
                    }
                }
            }

            return null;
        }

        public int AddOrderDetail(OrderDetail orderDetail)
        {
            string sql = "INSERT INTO OrderDetails (OrderId, ProductName, Quantity, Price) VALUES (@OrderId, @ProductName, @Quantity, @Price); SELECT last_insert_rowid();";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@OrderId", orderDetail.OrderId));
                command.Parameters.Add(new SqliteParameter("@ProductName", orderDetail.ProductName));
                command.Parameters.Add(new SqliteParameter("@Quantity", orderDetail.Quantity));
                command.Parameters.Add(new SqliteParameter("@Price", orderDetail.Price));

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int UpdateOrderDetail(OrderDetail orderDetail)
        {
            string sql = "UPDATE OrderDetails SET ProductName = @ProductName, Quantity = @Quantity, Price = @Price WHERE Id = @Id";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@ProductName", orderDetail.ProductName));
                command.Parameters.Add(new SqliteParameter("@Quantity", orderDetail.Quantity));
                command.Parameters.Add(new SqliteParameter("@Price", orderDetail.Price));
                command.Parameters.Add(new SqliteParameter("@Id", orderDetail.Id));

                return command.ExecuteNonQuery(); // Returns the number of rows affected
            }
        }

        public int DeleteOrderDetail(int id)
        {
            string sql = "DELETE FROM OrderDetails WHERE Id = @Id";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@Id", id));

                return command.ExecuteNonQuery(); // Returns the number of rows affected
            }
        }
    }
}
