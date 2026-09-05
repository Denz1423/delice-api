using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using delice_api.Repositories;
using delice_api.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Stripe: configure API key once at startup (not per-request)
StripeConfiguration.ApiKey = builder.Configuration["StripeSettings:SecretKey"];

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DynamoDB: singleton client per AWS best practices (reuse across threads)
builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
    new AmazonDynamoDBClient(Amazon.RegionEndpoint.APSoutheast2));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<PaymentService>();

builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(opt =>
    opt.AllowAnyHeader()
       .AllowAnyMethod()
       .AllowCredentials()
       .WithOrigins(
           "https://delice.davisdjaja.com",
           "http://localhost:5173"
       )
       .SetIsOriginAllowedToAllowWildcardSubdomains());

app.UseAuthorization();
app.MapControllers();
app.Run();
