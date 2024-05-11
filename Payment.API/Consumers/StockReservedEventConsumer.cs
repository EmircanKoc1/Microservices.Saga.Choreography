using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer(IPublishEndpoint _publishEndpoint) : IConsumer<StockReservedEvent>
    {
        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            if (true)
            {
                //ödeme başarılı
                PaymentCompletedEvent paymentCompletedEvent = new PaymentCompletedEvent()
                {
                    OrderId = context.Message.OrderId
                };

                await _publishEndpoint.Publish<PaymentCompletedEvent>(paymentCompletedEvent);

            }
            else
            {
                //ödeme başarısız

                PaymentFailedEvent payFailedEvent = new PaymentFailedEvent()
                {
                    OrderId = context.Message.OrderId,
                    Message = "Yetersiz bakiye",
                    OrderItems = context.Message.OrderItems
                };

                await _publishEndpoint.Publish<PaymentFailedEvent>(payFailedEvent);

            }


        }
    }
}
