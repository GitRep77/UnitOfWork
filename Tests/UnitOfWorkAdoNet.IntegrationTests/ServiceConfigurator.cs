using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using UnitOfWorkAdoNet.Repositories;

namespace UnitOfWorkAdoNet.IntegrationTests
{
    public static class ServiceConfigurator
    {
        public static IServiceProvider ConfigureServices(string databasePath)
        {
            var services = new ServiceCollection();

            services.AddScoped<IDbConnection>(sp =>
            {
                var connection = new SqliteConnection($"Data Source={databasePath}");
                connection.Open();
                return connection;
            });

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services.BuildServiceProvider();
        }
    }
}
