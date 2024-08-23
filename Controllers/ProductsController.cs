using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using delice_api.Entities;
using delice_api.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DynamoDB _dynamoDB;
        private readonly IConfiguration _config;

        // Constructor for dependency injection
        public ProductsController(DynamoDB dynamoDB, IConfiguration config)
        {
            _dynamoDB = dynamoDB;
            _config = config;
        }

        [HttpGet]
        [ResponseCache(Duration = 1800)]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            var request = new QueryRequest
            {
                TableName = _config["DynamoDb:ProductsTable"],
                KeyConditionExpression = "#group = :groupValue",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#group", "Group" } // Placeholder for the reserved keyword
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {
                        ":groupValue",
                        new AttributeValue { S = "PRODUCTS" }
                    }
                }
            };

            try
            {
                var response = await _dynamoDB.Client.QueryAsync(request);
                var items = response.Items;

                //Convert DynamoDB items to Product objects
                var products = items
                    .Select(item => new Product
                    {
                        Id = item["id"].S,
                        Name = item["Name"].S,
                        Price = double.Parse(item["Price"].N),
                        ImageUrl = item["ImageUrl"].S,
                        Type = item["Type"].S
                    })
                    .ToList();

                return Ok(products);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }
    }
}
