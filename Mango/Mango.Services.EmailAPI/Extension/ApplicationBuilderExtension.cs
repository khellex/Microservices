using Mango.Services.EmailAPI.Messaging;

namespace Mango.Services.EmailAPI.Extension
{
    public static class ApplicationBuilderExtension
    {
        private static IAzureServiceBusConsumer ServiceBusConsumer { get; set; }

        public static IApplicationBuilder UseAzureServiceBusConsumer(this IApplicationBuilder app)
        {
            //here we request for an implementation of the IAzureServiceBusConsumer,
            //we register this interface in the program.cs as Singleton since for
            //throughout the lifetime of the application we only want to make one
            //request to the service bus
            ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();

            //we call this to fetch the application lifetime, so we can determine
            //when to start and stop the AzureServiceBus listener accordingly
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            //based on application state (on/off),
            //we continue listening to the service bus for any queue messages 
            hostApplicationLife.ApplicationStarted.Register(OnStart);
            hostApplicationLife.ApplicationStopping.Register(OnStop);

            return app;
        }

        private static void OnStart()
        {
            ServiceBusConsumer.Start();
        }

        private static void OnStop()
        {
            ServiceBusConsumer.Stop();
        }
    }
}
