using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer(
        MongoDbService _mongoDbService,
        ISendEndpointProvider _sendEndpointProvider,
        IPublishEndpoint _publishEndpoint) : IConsumer<OrderCreatedEvent>
    {

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new List<bool>();
            IMongoCollection<API.Models.Stock> collection = _mongoDbService.GetCollection<Models.Stock>();

            foreach (var orderItem in context.Message.OrderItems)
            {
                stockResult.Add(await (await collection.FindAsync(x => x.ProductId.Equals(orderItem.ProductId.ToString()) && x.Count > orderItem.Count)).AnyAsync());
            }

            if (stockResult.TrueForAll(s => s.Equals(true)))
            {
                //stock güncellemesi 
                //paymenti uyaracak olan eventin fırlatılması

                foreach (var orderItem in context.Message.OrderItems)
                {
                    var stock = await (await collection.FindAsync(s => s.ProductId.Equals(orderItem.ProductId.ToString()))).FirstOrDefaultAsync();

                    stock.Count -= orderItem.Count;

                    await collection.FindOneAndReplaceAsync(x => x.ProductId.Equals(orderItem.ProductId.ToString()), stock);

                }

                var sendEnpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEvent}"));

                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    TotalPrice = context.Message.TotalPrice,
                    OrderItems = context.Message.OrderItems
                };

                await sendEnpoint.Send<StockReservedEvent>(stockReservedEvent);
            }
            else
            {

                //stok işlemi başarısız
                //orderi uyarıcak olan event fırlatılacaktır

                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    Message = "Stok miktarı yetersiz"
                };

                await _publishEndpoint.Publish<StockNotReservedEvent>(stockNotReservedEvent);



            }



        }
    }
}
