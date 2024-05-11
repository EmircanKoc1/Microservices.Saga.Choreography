using MassTransit;
using Order.API.Enums;
using Order.API.Models.Context;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer(OrderAPIDbContext _context) : IConsumer<PaymentFailedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = await _context.FindAsync<global::Order.API.Models.Order>(context.Message.OrderId);

            if (order is null)
                throw new NotImplementedException();

            order.OrderStatus = OrderStatu.Fail;

            await _context.SaveChangesAsync();


        }
    }
}
