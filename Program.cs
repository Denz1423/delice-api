using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using delice_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DynamoDB client as a singleton service
builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    return new AmazonDynamoDBClient(Amazon.RegionEndpoint.APSoutheast2);
});

builder.Services.AddScoped<DynamoDB>();
builder.Services.AddCors();

builder.Services.AddScoped<PaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:5173").SetIsOriginAllowedToAllowWildcardSubdomains();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
