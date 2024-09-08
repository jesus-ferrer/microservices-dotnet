using Discount.Grpc.Data;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.
services.AddGrpc();
services.AddGrpcReflection();

services.AddDbContext<DiscountContext>(opts => 
    opts.UseSqlite(configuration.GetConnectionString("DB")));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountService>();
app.UseMigration();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
if(app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}
app.Run();
