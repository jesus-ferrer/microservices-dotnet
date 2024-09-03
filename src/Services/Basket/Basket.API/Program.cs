using BuildingBlocks.Exceptions.Handler;

var builder = WebApplication.CreateBuilder(args);

// Add Services to the container
var services = builder.Services;
var configuration = builder.Configuration;
var assembly = typeof(Program).Assembly;

services.AddScoped<IBasketRepository, BasketRepository>();

services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

services.AddCarter();

services.AddMarten(opt =>
{
    opt.Connection(configuration.GetConnectionString("BasketDb")!);
    opt.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Configure the HTTP Pipeline
app.MapCarter();
app.UseExceptionHandler(opt =>
{
});

app.Run();
