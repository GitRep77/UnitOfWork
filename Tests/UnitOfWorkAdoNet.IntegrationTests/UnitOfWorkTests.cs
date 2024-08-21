﻿using Domain.Entities;
using UnitOfWorkAdoNet.Repositories;

namespace UnitOfWorkAdoNet.IntegrationTests
{
    public class UnitOfWorkTests : TestBase
    {
        [Fact]
        public void Should_Commit_Transaction_Successfully()
        {
            Customer customer = null;
            Order order = null;
            OrderDetail orderDetail = null;

            UnitOfWork.ExecuteInTransaction(() =>
            {
                customer = new Customer { Name = "John Doe", Email = "johndoe@example.com" };
                customer.Id = UnitOfWork.Customers.AddCustomer(customer);

                order = new Order { CustomerId = customer.Id, OrderDate = DateTime.Now };
                order.Id = UnitOfWork.Orders.AddOrder(order);

                orderDetail = new OrderDetail { OrderId = order.Id, ProductName = "Product A", Quantity = 1, Price = 99.99m };
                orderDetail.Id = UnitOfWork.OrderDetails.AddOrderDetail(orderDetail);
            });

            using (var verificationConnection = GetNewConnectionForVerification())
            {
                Assert.NotNull(UnitOfWork.Customers.GetCustomerById(customer.Id));
                Assert.NotNull(UnitOfWork.Orders.GetOrderById(order.Id));
                Assert.NotNull(UnitOfWork.OrderDetails.GetOrderDetailById(orderDetail.Id));
            }
        }

        [Fact]
        public void Should_Rollback_Transaction_On_Failure()
        {
            Customer? customer = null;
            Order? order = null;
            OrderDetail? orderDetail = null;

            try
            {
                UnitOfWork.ExecuteInTransaction(() =>
                {
                    customer = new Customer { Name = "John Doe", Email = "johndoe@example.com" };
                    customer.Id = UnitOfWork.Customers.AddCustomer(customer);

                    order = new Order { CustomerId = customer.Id, OrderDate = DateTime.Now };
                    order.Id = UnitOfWork.Orders.AddOrder(order);

                    orderDetail = new OrderDetail { OrderId = order.Id, ProductName = null, Quantity = 1, Price = 99.99m };
                    UnitOfWork.OrderDetails.AddOrderDetail(orderDetail);
                });
            }
            catch
            {
                using (var verificationConnection = GetNewConnectionForVerification())
                {
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
}
