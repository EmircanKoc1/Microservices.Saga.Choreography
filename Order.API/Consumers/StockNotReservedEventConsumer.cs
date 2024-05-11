using MassTransit;
using Order.API.Enums;
using Order.API.Models.Context;
using Shared.Events;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer(OrderAPIDbContext _context) : IConsumer<StockNotReservedEvent>
    {
        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);

            if (order is null)
                throw new NullReferenceException();

            order.OrderStatus = OrderStatu.Fail;

            await _context.SaveChangesAsync();

        }
    }
}
