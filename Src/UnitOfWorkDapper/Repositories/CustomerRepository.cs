using Dapper;
using Domain.Entities;
using System.Data;

namespace UnitOfWorkDapper.Repositories
{
    public interface ICustomerRepository
    {
        IEnumerable<Customer> GetAllCustomers();
        Customer GetCustomerById(int id);
        int AddCustomer(Customer customer);
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

        public IEnumerable<Customer> GetAllCustomers()
        {
            string sql = "SELECT * FROM Customers";
            return _connection.Query<Customer>(sql);
        }

        public Customer GetCustomerById(int id)
        {
            string sql = "SELECT * FROM Customers WHERE Id = @Id";
            return _connection.QuerySingleOrDefault<Customer>(sql, new { Id = id });
        }

        public int AddCustomer(Customer customer)
        {
            string sql = "INSERT INTO Customers (Name, Email) VALUES (@Name, @Email); SELECT last_insert_rowid();";
            return _connection.ExecuteScalar<int>(sql, customer, _transaction); // Returns the inserted ID
        }
        public int UpdateCustomer(Customer customer)
        {
            string sql = "UPDATE Customers SET Name = @Name, Email = @Email WHERE Id = @Id";
            return _connection.Execute(sql, customer, _transaction); // Returns the number of rows affected
        }

        public int DeleteCustomer(int id)
        {
            string sql = "DELETE FROM Customers WHERE Id = @Id";
            return _connection.Execute(sql, new { Id = id }, _transaction); // Returns the number of rows affected
        }
    }
}
