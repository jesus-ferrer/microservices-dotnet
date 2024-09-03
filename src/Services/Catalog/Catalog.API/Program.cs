using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;
var assembly = typeof(Program).Assembly;

// Add services to the container
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
services.AddValidatorsFromAssembly(assembly);

services.AddCarter();

services.AddMarten(opt =>
{
    opt.Connection(configuration.GetConnectionString("catalogDb")!);
}).UseLightweightSessions();

if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<CatalogInitialData>();

services.AddExceptionHandler<CustomExceptionHandler>();

services
    .AddHealthChecks()
    .AddNpgSql(configuration.GetConnectionString("catalogDb")!);

var app = builder.Build();

// Configure the HTTP Request Pipeline
app.MapCarter();

app.UseExceptionHandler(opts =>
{
});


app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

// Inline code ExceptionHandler
//app.UseExceptionHandler(exceptionHandlerapp =>
//{
//    exceptionHandlerapp.Run(async context =>
//    {
//        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
//        if (exception is null)
//            return;

//        var problemDetails = new ProblemDetails
//        {
//            Title = exception.Message,
//            Status = StatusCodes.Status500InternalServerError,
//            Detail = exception.StackTrace
//        };

//        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//        logger.LogError(exception, exception.Message);

//        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//        context.Response.ContentType = "application/problem+json";

//        await context.Response.WriteAsJsonAsync(problemDetails);
//    });
//});

app.Run();
