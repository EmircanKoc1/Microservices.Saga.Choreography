using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Enums;
using Order.API.Models;
using Order.API.Models.Context;
using Order.API.ViewModels;
using Shared;
using Shared.Events;
using Shared.Messages;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<OrderAPIDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddMassTransit(configurator =>
{

    configurator.AddConsumer<PaymentCompletedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.AddConsumer<StockNotReservedEventConsumer>();

    configurator.UsingRabbitMq((_context, _configurator) =>
    {

        _configurator.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentCompletedEventQueue, e =>
        {
            e.ConfigureConsumer<PaymentCompletedEventConsumer>(_context);
        });

        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventQueue, e =>
        {
            e.ConfigureConsumer<PaymentFailedEventConsumer>(_context);
        });

        _configurator.ReceiveEndpoint(RabbitMQSettings.Order_StockNotReservedEventQueue, e =>
        {
            e.ConfigureConsumer<StockNotReservedEventConsumer>(_context);
        });

    });



});




var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async ([FromBody] CreateOrderVM model, [FromServices] OrderAPIDbContext _context, [FromServices] IPublishEndpoint _publishEndpoint) =>
{
    global::Order.API.Models.Order order = new()
    {
        BuyerId = Guid.TryParse(model.BUyerId, out Guid _buyerId) ? _buyerId : Guid.NewGuid(),
        OrderItems = model.OrderItems.Select(oi => new OrderItem()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = Guid.Parse(oi.ProductId)
        }).ToList(),
        OrderStatus = OrderStatu.Suspend,
        CreatedDate = DateTime.UtcNow,
        TotalPrice = model.OrderItems.Sum(oi => oi.Price * oi.Count)
    };

    await _context.Orders.AddAsync(order);
    await _context.SaveChangesAsync();

    var orderCreatedEvent = new OrderCreatedEvent()
    {
        BuyerId = order.BuyerId,
        OrderId = order.Id,
        TotalPrice = order.TotalPrice,
        OrderItems = order.OrderItems.Select(oi => new OrderItemMessage
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = oi.ProductId,
        }).ToList()

    };

    await _publishEndpoint.Publish<OrderCreatedEvent>(orderCreatedEvent);


});


app.Run();
