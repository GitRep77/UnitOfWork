
# UnitOfWork

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)![C# Version](https://img.shields.io/badge/C%23-7.0-blue)![.NET Version](https://img.shields.io/badge/.NET-4.7.2-blue)![License](https://img.shields.io/badge/license-MIT-blue)


## Project Overview

UnitOfWork is an experimental project designed to explore and implement effective transaction handling strategies using ADO.NET and Dapper. The project revolves around the Unit of Work design pattern, which helps maintain consistency across multiple repository operations by coordinating the execution of commands within a single database transaction.

The aim of this project is to provide a practical example of managing transactions across multiple repositories in a clean and efficient way, leveraging ADO.NET and Dapper for database interactions.

## Features

- **Transaction Handling:** Implementing the Unit of Work pattern to manage transactions across multiple repositories.
- **Repository Pattern:** Structured repository classes for managing Customers, Orders, and OrderDetails.
- **Flexible Transaction Management:** Both automatic and manual transaction handling with commit, rollback, and fine-grained control.
- **Support for ADO.NET and Dapper:** The project demonstrates two different approaches, one using raw ADO.NET and the other using Dapper, to showcase different ways of handling transactions in .NET.
- **Exception Handling:** Comprehensive exception handling, ensuring that transaction failures are rolled back correctly, with custom exceptions where necessary.
- **Testing:** Integration tests for both ADO.NET and Dapper implementations to ensure that the UnitOfWork pattern works correctly across both database interaction technologies.

## Technologies Used

- **.NET Core**: This project is built using .NET Core, leveraging the latest features and best practices of the framework.
- **Dapper**: Lightweight ORM for .NET, providing fast database access with less overhead.
- **ADO.NET**: Standard database access layer for .NET, allowing for low-level database interaction.
- **XUnit**: Testing framework for unit and integration tests.
- **SQLite**: Used as the database for local testing and integration purposes.

## Setup and Installation

To run this project locally, follow these steps:

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/UnitOfWork.git
   ```

2. Navigate to the project directory:
   ```bash
   cd UnitOfWork
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the tests:
   ```bash
   dotnet test
   ```

## Project Structure

The project follows a clean architecture with separate layers for different concerns:

- **Repositories**: Responsible for database access and CRUD operations.
- **UnitOfWork**: Manages transactions across repositories, ensuring atomic operations.
- **IntegrationTests**: Ensures that the UnitOfWork implementation works as expected for both ADO.NET and Dapper.

### Folder Structure

```
UnitOfWork/
│
├── Src/
│   ├── UnitOfWorkAdoNet/
│   ├── UnitOfWorkDapper/
│   └── Domain/
├── Tests/
│   ├── UnitOfWorkAdoNet.IntegrationTests/
│   └── UnitOfWorkDapper.IntegrationTests/
└── README.md
```

## How It Works

The project demonstrates how to handle transactions for multiple repositories using the Unit of Work pattern. This pattern helps coordinate database operations to ensure that they either all succeed or all fail, avoiding data inconsistency.

In both the ADO.NET and Dapper implementations, the UnitOfWork class ensures that all database operations are wrapped in a transaction. If an operation fails, the transaction is rolled back to maintain data integrity.

### Example Code

Here is a simplified example of how to use the UnitOfWork:

```csharp
using (var unitOfWork = new UnitOfWork(connection, customerRepository, orderRepository, orderDetailRepository))
{
    unitOfWork.ExecuteInTransaction(() =>
    {
        var customer = new Customer { Name = "John Doe", Email = "johndoe@example.com" };
        customer.Id = unitOfWork.Customers.AddCustomer(customer);
        
        var order = new Order { CustomerId = customer.Id, OrderDate = DateTime.Now };
        order.Id = unitOfWork.Orders.AddOrder(order);

        var orderDetail = new OrderDetail { OrderId = order.Id, ProductName = "Product A", Quantity = 1, Price = 99.99m };
        unitOfWork.OrderDetails.AddOrderDetail(orderDetail);
    });
}
```

## Author

This project was created by [Gitrep77](https://github.com/Gitrep77). If you find it helpful, feel free to give a star on the repository!

## Support

For issues or questions, open an issue on the [GitHub repository](https://github.com/Gitrep77/Unit_Of_Work/issues).

## Contributing

We welcome contributions! Please read our [contributing guidelines](CONTRIBUTING.md) for more details.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
