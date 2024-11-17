using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

//adding ocelot to our container
builder.Services.AddOcelot();

//adding authentication support for ocelot, so that the gateway
//can forward this token to the other endpoints and get back the response
builder.AddAppAuthentication();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//adding ocelot to the DI pipeline
await app.UseOcelot();

await app.RunAsync();
