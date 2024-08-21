using Domain.Entities;
using System.Data;
using Microsoft.Data.Sqlite;

namespace UnitOfWorkAdoNet.Repositories
{
    public interface ICustomerRepository
    {
        Customer GetCustomerById(int id);
        int AddCustomer(Customer customer);
        IEnumerable<Customer> GetAllCustomers();
        int UpdateCustomer(Customer customer);
        int DeleteCustomer(int id);
        void SetTransaction(IDbTransaction transaction);
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction;

        public CustomerRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void SetTransaction(IDbTransaction transaction)
        {
            _transaction = transaction;
        }

        public int AddCustomer(Customer customer)
        {
            string sql = "INSERT INTO Customers (Name, Email) VALUES (@Name, @Email); SELECT last_insert_rowid();";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@Name", customer.Name));
                command.Parameters.Add(new SqliteParameter("@Email", customer.Email));

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public Customer GetCustomerById(int id)
        {
            string sql = "SELECT * FROM Customers WHERE Id = @Id";
            var parameters = new SqliteParameter[] { new SqliteParameter("@Id", id) };

            using (var command = new SqliteCommand(sql, (SqliteConnection)_connection))
            {
                command.Parameters.AddRange(parameters);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Customer
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Email = reader.GetString(2)
                        };
                    }
                }
            }

            return null;
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            string sql = "SELECT * FROM Customers";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                using (var reader = command.ExecuteReader())
                {
                    var customers = new List<Customer>();

                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Email = reader.GetString(2)
                        });
                    }

                    return customers;
                }
            }
        }

        public int UpdateCustomer(Customer customer)
        {
            string sql = "UPDATE Customers SET Name = @Name, Email = @Email WHERE Id = @Id";

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = sql;
                command.Transaction = _transaction;

                command.Parameters.Add(new SqliteParameter("@Name", customer.Name));
                command.Parameters.Add(new SqliteParameter("@Email", customer.Email));
                command.Parameters.Add(new SqliteParameter("@Id", customer.Id));

                return command.ExecuteNonQuery(); // Returns the number of rows affected
            }
        }

        public int DeleteCustomer(int id)
        {
            string sql = "DELETE FROM Customers WHERE Id = @Id";

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
