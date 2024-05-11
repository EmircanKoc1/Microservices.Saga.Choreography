using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;
using SAM = Stock.API.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();


    configurator.UsingRabbitMq((_context, _configurator) =>
    {
        _configurator.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        _configurator.ReceiveEndpoint(
            queueName: RabbitMQSettings.Stock_OrderCreatedEventQueue,
            configureEndpoint: e =>
            {
                e.ConfigureConsumer<OrderCreatedEventConsumer>(_context);
            });

        _configurator.ReceiveEndpoint(
          queueName: RabbitMQSettings.Stock_PaymentFailedEventQueue,
          configureEndpoint: e =>
          {
              e.ConfigureConsumer<PaymentFailedEventConsumer>(_context);
          });

    });

});

builder.Services.AddSingleton<MongoDbService>();




var app = builder.Build();

using IServiceScope scope = app.Services.CreateScope();
MongoDbService mongoDbService = scope.ServiceProvider.GetService<MongoDbService>();



var stockCollection = mongoDbService.GetCollection<SAM.Stock>();

if (!stockCollection.FindSync(s => true).Any())
{
    await stockCollection.InsertOneAsync(new SAM.Stock() { Id = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Count = 100, });
    await stockCollection.InsertOneAsync(new SAM.Stock() { Id = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Count = 200, });
    await stockCollection.InsertOneAsync(new SAM.Stock() { Id = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Count = 50, });
    await stockCollection.InsertOneAsync(new SAM.Stock() { Id = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Count = 30, });
    await stockCollection.InsertOneAsync(new SAM.Stock() { Id = Guid.NewGuid().ToString(), ProductId = Guid.NewGuid().ToString(), Count = 5, });
    

}

app.Run();
