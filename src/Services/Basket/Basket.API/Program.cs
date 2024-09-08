using BuildingBlocks.Exceptions.Handler;
using Discount.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add Services to the container
var services = builder.Services;
var configuration = builder.Configuration;

// Application Services
var assembly = typeof(Program).Assembly;
services.AddCarter();
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Data Services
services.AddMarten(opt =>
{
    opt.Connection(configuration.GetConnectionString("BasketDb")!);
    opt.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

services.AddScoped<IBasketRepository, BasketRepository>();
services.Decorate<IBasketRepository, CachedBasketRepository>();

services.AddStackExchangeRedisCache(opts =>
{
    opts.Configuration = configuration.GetConnectionString("Redis");
});

// Grpc Services
services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(opts =>
{
    opts.Address = new Uri(configuration["GrpcSettings:DiscountUrl"]!);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    return handler;
});

// Cross-Cutting Services
services.AddExceptionHandler<CustomExceptionHandler>();

services
    .AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("BasketDb")!)
    .AddRedis(configuration.GetConnectionString("Redis")!);

var app = builder.Build();

// Configure the HTTP Pipeline
app.MapCarter();
app.UseExceptionHandler(opt =>
{
});

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
