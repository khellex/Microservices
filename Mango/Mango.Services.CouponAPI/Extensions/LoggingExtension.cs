using Serilog;

namespace Mango.Services.CouponAPI.Extensions
{
    public static class LoggingExtension
    {
        public static WebApplicationBuilder SerilogLogging(this WebApplicationBuilder builder)
        {

            // Build configuration from appsettings.json
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // Set up Serilog using configuration from appsettings.json
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                //.WriteTo.Console() // Optional: Log to console
                .CreateLogger();

            // Replace default .NET logging with Serilog
            builder.Host.UseSerilog();

            return builder;
        }
    }
}
