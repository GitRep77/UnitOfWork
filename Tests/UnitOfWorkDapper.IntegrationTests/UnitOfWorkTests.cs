using Domain.Entities;
using UnitOfWorkDapper.Repositories;

namespace UnitOfWorkDapper.IntegrationTests
{
    public class UnitOfWorkTests : TestBase
    {
        [Fact]
        public void Should_Commit_Transaction_Successfully()
        {
            // Prepare references to newly inserted entities
            Customer customer = null;
            Order order = null;
            OrderDetail orderDetail = null;

            try
            {
                // 1. Begin the transaction
                UnitOfWork.BeginTransaction();

                // 2. Perform the writes
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

                // 3. Commit on success
                UnitOfWork.Commit();
            }
            catch
            {
                // 4. Roll back on failure
                UnitOfWork.Rollback();
                throw;
            }

            // 5. Verification: ensure data is in the database
            using (var verificationConnection = GetNewConnectionForVerification())
            {
                var customerRepository = new CustomerRepository(verificationConnection);
                var orderRepository = new OrderRepository(verificationConnection);
                var orderDetailRepository = new OrderDetailRepository(verificationConnection);

                Assert.NotNull(customerRepository.GetCustomerById(customer.Id));
                Assert.NotNull(orderRepository.GetOrderById(order.Id));
                Assert.NotNull(orderDetailRepository.GetOrderDetailById(orderDetail.Id));
            }
        }

        [Fact]
        public void Should_Rollback_Transaction_On_Failure()
        {
            // Prepare references to newly inserted entities
            Customer customer = null;
            Order order = null;
            OrderDetail orderDetail = null;

            try
            {
                // 1. Begin the transaction
                UnitOfWork.BeginTransaction();

                // 2. Perform the writes, one of which will fail
                customer = new Customer { Name = "John Doe", Email = "johndoe@example.com" };
                customer.Id = UnitOfWork.Customers.AddCustomer(customer);

                order = new Order { CustomerId = customer.Id, OrderDate = DateTime.Now };
                order.Id = UnitOfWork.Orders.AddOrder(order);

                // This is expected to fail due to ProductName being NOT NULL in the schema
                orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductName = null,  // triggers a constraint violation in SQLite
                    Quantity = 1,
                    Price = 99.99m
                };
                orderDetail.Id = UnitOfWork.OrderDetails.AddOrderDetail(orderDetail);

                // If it didn't fail, we'd commit. But we expect an exception above.
                UnitOfWork.Commit();
            }
            catch
            {
                // 3. Rollback on any exception
                UnitOfWork.Rollback();

                // 4. Verify data is not in the database
                using (var verificationConnection = GetNewConnectionForVerification())
                {
                    var customerRepository = new CustomerRepository(verificationConnection);
                    var orderRepository = new OrderRepository(verificationConnection);
                    var orderDetailRepository = new OrderDetailRepository(verificationConnection);

                    Assert.Null(customerRepository.GetCustomerById(customer?.Id ?? 0));
                    Assert.Null(orderRepository.GetOrderById(order?.Id ?? 0));
                    Assert.Null(orderDetailRepository.GetOrderDetailById(orderDetail?.Id ?? 0));
                }
            }
        }
    }
}
