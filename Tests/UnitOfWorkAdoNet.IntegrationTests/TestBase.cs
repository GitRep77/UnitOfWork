using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace UnitOfWorkAdoNet.IntegrationTests
{
    public abstract class TestBase : IDisposable
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected IUnitOfWork UnitOfWork { get; private set; }
        protected IDbConnection Connection { get; private set; }

        private readonly string _databasePath = $"test_{Guid.NewGuid()}.db";

        public TestBase()
        {
            ServiceProvider = ServiceConfigurator.ConfigureServices(_databasePath);

            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }

            Connection = ServiceProvider.GetRequiredService<IDbConnection>();

            CreateDatabaseSchema();

            UnitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();

            var test = $"Test Setup - Connection: {Connection.GetHashCode()}";
            var test2 = string.Empty ;
        }

        private void CreateDatabaseSchema()
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Customers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Email TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Orders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerId INTEGER NOT NULL,
                        OrderDate TEXT NOT NULL,
                        FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
                    );

                    CREATE TABLE IF NOT EXISTS OrderDetails (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER NOT NULL,
                        ProductName TEXT NOT NULL,
                        Quantity INTEGER NOT NULL,
                        Price REAL NOT NULL,
                        FOREIGN KEY (OrderId) REFERENCES Orders(Id)
                    );
                ";

                command.ExecuteNonQuery();
            }
        }

        protected IDbConnection GetNewConnectionForVerification()
        {
            var newConnection = new SqliteConnection($"Data Source={_databasePath}");
            newConnection.Open();
            return newConnection;
        }

        public void Dispose()
        {
            try
            {
                if (Connection != null && Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
                Connection?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to close and dispose of the connection: {ex.Message}");
            }

            try
            {
                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Failed to delete the database file: {ex.Message}");
            }

            (ServiceProvider as IDisposable)?.Dispose();
        }
    }
}
