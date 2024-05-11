using MassTransit;
namespace Payment.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMassTransit(configurator =>
            {

                configurator.UsingRabbitMq((_context, _configurator) =>
                {
                    _configurator.Host(builder.Configuration.GetConnectionString("RabbitMQ"));




                });

            });



            var app = builder.Build();




            app.Run();
        }
    }
}
