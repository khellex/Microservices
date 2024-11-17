using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

//we moved the route config to a different json file, we need to add that json to the container
//optional: false means this file is required, reloadOnChange:true means if any change is made
//in the file then the file will be reloaded
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange:true);

//adding ocelot to our container
builder.Services.AddOcelot(builder.Configuration);

//adding authentication support for ocelot, so that the gateway
//can forward this token to the other endpoints and get back the response
builder.AddAppAuthentication();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

//adding ocelot to the DI pipeline
await app.UseOcelot();

await app.RunAsync();
