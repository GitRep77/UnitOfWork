using Domain.Entities;
using System.Data;
using Microsoft.Data.Sqlite;

namespace UnitOfWorkAdoNet.Repositories
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
            var orders = new List<Order>();
            string sql = "SELECT * FROM Orders";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            OrderDate = reader.GetDateTime(2)
                        });
                    }
                }
            }

            return orders;
        }

        public Order GetOrderById(int id)
        {
            string sql = "SELECT * FROM Orders WHERE Id = @Id";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@Id", id));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Order
                        {
                            Id = reader.GetInt32(0),
                            CustomerId = reader.GetInt32(1),
                            OrderDate = reader.GetDateTime(2)
                        };
                    }
                }
            }

            return null;
        }

        public int AddOrder(Order order)
        {
            string sql = "INSERT INTO Orders (CustomerId, OrderDate) VALUES (@CustomerId, @OrderDate); SELECT last_insert_rowid();";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@CustomerId", order.CustomerId));
                command.Parameters.Add(new SqliteParameter("@OrderDate", order.OrderDate));

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public int UpdateOrder(Order order)
        {
            string sql = "UPDATE Orders SET CustomerId = @CustomerId, OrderDate = @OrderDate WHERE Id = @Id";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@CustomerId", order.CustomerId));
                command.Parameters.Add(new SqliteParameter("@OrderDate", order.OrderDate));
                command.Parameters.Add(new SqliteParameter("@Id", order.Id));

                return command.ExecuteNonQuery(); // Returns the number of rows affected
            }
        }

        public int DeleteOrder(int id)
        {
            string sql = "DELETE FROM Orders WHERE Id = @Id";

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
