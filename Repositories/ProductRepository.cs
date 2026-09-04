using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using delice_api.Constants;
using delice_api.Entities;

namespace delice_api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IAmazonDynamoDB _client;
    private readonly string _tableName;

    public ProductRepository(IAmazonDynamoDB client, IConfiguration config)
    {
        _client = client;
        _tableName = config["DynamoDb:ProductsTable"]!;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "#grp = :groupValue",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#grp", DynamoDbKeys.GroupKey }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":groupValue", new AttributeValue { S = DynamoDbKeys.ProductsGroupValue } }
            }
        };

        var response = await _client.QueryAsync(request);

        return response.Items.Select(item => new Product
        {
            Id = item["id"].S,
            Name = item["Name"].S,
            Price = double.Parse(item["Price"].N),
            ImageUrl = item["ImageUrl"].S,
            Type = item["Type"].S
        }).ToList();
    }
}
