using Domain.Entities;
using UnitOfWorkAdoNet.Repositories;

namespace UnitOfWorkAdoNet.IntegrationTests
{
    public class UnitOfWorkTests : TestBase
    {
        [Fact]
        public void Should_Commit_Transaction_Successfully()
        {
            // Entities to insert and verify later
            Customer customer = null;
            Order order = null;
            OrderDetail orderDetail = null;

            try
            {
                // 1. Begin the transaction
                UnitOfWork.BeginTransaction();

                // 2. Perform repository operations
                customer = new Customer { Name = "John Doe", Email = "johndoe@example.com" };
                customer.Id = UnitOfWork.Customers.AddCustomer(customer);

                order = new Order { CustomerId = customer.Id, OrderDate = DateTime.Now };
                order.Id = UnitOfWork.Orders.AddOrder(order);

                orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductName = "Product A",
                    Quantity = 1,
                    Price = 99.99m
                };
                orderDetail.Id = UnitOfWork.OrderDetails.AddOrderDetail(orderDetail);

                // 3. Commit
                UnitOfWork.Commit();
            }
            catch
            {
                // 4. Rollback on failure
                UnitOfWork.Rollback();
                throw;
            }

            // 5. Verify data was committed
            using (var verificationConnection = GetNewConnectionForVerification())
            {
                // We can reuse the same repository logic (or create new repos for verification if desired)
                var insertedCustomer = UnitOfWork.Customers.GetCustomerById(customer.Id);
                var insertedOrder = UnitOfWork.Orders.GetOrderById(order.Id);
                var insertedOrderDetail = UnitOfWork.OrderDetails.GetOrderDetailById(orderDetail.Id);

                Assert.NotNull(insertedCustomer);
                Assert.NotNull(insertedOrder);
                Assert.NotNull(insertedOrderDetail);
            }
        }

        [Fact]
        public void Should_Rollback_Transaction_On_Failure()
        {
            Customer customer = null;
            Order order = null;
            OrderDetail orderDetail = null;

            try
            {
                // 1. Begin transaction
                UnitOfWork.BeginTransaction();

                // 2. Perform repository operations that will throw or fail
                customer = new Customer { Name = "John Doe", Email = "johndoe@example.com" };
                customer.Id = UnitOfWork.Customers.AddCustomer(customer);

                order = new Order { CustomerId = customer.Id, OrderDate = DateTime.Now };
                order.Id = UnitOfWork.Orders.AddOrder(order);

                // Suppose ProductName is disallowed to be null and this leads to an exception or constraint violation.
                orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductName = null, // triggers an error or constraint failure
                    Quantity = 1,
                    Price = 99.99m
                };
                UnitOfWork.OrderDetails.AddOrderDetail(orderDetail);

                // If we never reach here (due to an exception), then we won't commit.
                UnitOfWork.Commit();
            }
            catch
            {
                // 3. Rollback
                UnitOfWork.Rollback();
            }

            // 4. Verify that nothing was inserted (all changes rolled back).
            using (var verificationConnection = GetNewConnectionForVerification())
            {
                // We can create new repository objects pointing to the new verification connection
                // Or we can read from the existing ones — but typically you want a separate connection for a fresh read.
                var customerRepository = new CustomerRepository(verificationConnection);
                var orderRepository = new OrderRepository(verificationConnection);
                var orderDetailRepository = new OrderDetailRepository(verificationConnection);

                var insertedCustomer = customerRepository.GetCustomerById(customer?.Id ?? 0);
                var insertedOrder = orderRepository.GetOrderById(order?.Id ?? 0);
                var insertedOrderDetail = orderDetailRepository.GetOrderDetailById(orderDetail?.Id ?? 0);

                Assert.Null(insertedCustomer);
                Assert.Null(insertedOrder);
                Assert.Null(insertedOrderDetail);
            }
        }
    }
}
