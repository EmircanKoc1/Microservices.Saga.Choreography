using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer(MongoDbService _mongoDbService) : IConsumer<PaymentFailedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            IMongoCollection<Models.Stock> stocks = _mongoDbService.GetCollection<Models.Stock>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await (await stocks.FindAsync(stock => stock.ProductId.Equals(orderItem.ProductId))).FirstOrDefaultAsync();

                if (stock is not null)
                {
                    stock.Count += orderItem.Count;
                    await stocks.FindOneAndReplaceAsync(s => s.ProductId.Equals(orderItem.ProductId), stock);

                }


            }


        }
    }
}
