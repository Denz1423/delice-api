using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using delice_api.Constants;
using delice_api.Entities;
using delice_api.Entities.Order;

namespace delice_api.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IAmazonDynamoDB _client;
    private readonly string _tableName;

    public OrderRepository(IAmazonDynamoDB client, IConfiguration config)
    {
        _client = client;
        _tableName = config["DynamoDb:OrdersTable"]!;
    }

    public async Task<List<Order>> GetAllAsync()
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
                { ":groupValue", new AttributeValue { S = DynamoDbKeys.OrderGroupValue } }
            }
        };

        var response = await _client.QueryAsync(request);

        return response.Items.Select(MapToOrder).ToList();
    }

    public async Task CreateAsync(Order order)
    {
        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = MapToItem(order)
        };

        await _client.PutItemAsync(request);
    }

    public async Task UpdatePaymentStatusAsync(string orderId, PaymentStatus status)
    {
        var request = new UpdateItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { DynamoDbKeys.GroupKey, new AttributeValue { S = DynamoDbKeys.OrderGroupValue } },
                { "Id", new AttributeValue { S = orderId } }
            },
            UpdateExpression = "SET PaymentStatus = :status",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":status", new AttributeValue { S = status.ToString() } }
            }
        };

        await _client.UpdateItemAsync(request);
    }

    private static Order MapToOrder(Dictionary<string, AttributeValue> item) => new()
    {
        Id = item["Id"].S,
        TableNumber = int.Parse(item["TableNumber"].N),
        OrderDate = DateTime.Parse(item["OrderDate"].S),
        OrderItems = item["OrderItems"].L.Select(attr => new CartProduct
        {
            // Handle both S and N storage for Id (legacy records stored it as N)
            Id = attr.M.TryGetValue("Id", out var idAttr)
                ? (string.IsNullOrEmpty(idAttr.S) ? idAttr.N : idAttr.S)
                : string.Empty,
            Name = attr.M["Name"].S,
            Type = attr.M["Type"].S,
            Price = double.Parse(attr.M["Price"].N),
            Quantity = int.Parse(attr.M["Quantity"].N),
            ImageUrl = attr.M.TryGetValue("ImageUrl", out var imgAttr) ? imgAttr.S : string.Empty
        }).ToList(),
        SubTotal = double.Parse(item["SubTotal"].N),
        OrderStatus = Enum.Parse<OrderStatus>(item["OrderStatus"].S),
        PaymentStatus = Enum.Parse<PaymentStatus>(item["PaymentStatus"].S),
        PaymentIntentId = item.TryGetValue("PaymentIntentId", out var pi) ? pi.S : null
    };

    private static Dictionary<string, AttributeValue> MapToItem(Order order)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { DynamoDbKeys.GroupKey, new AttributeValue { S = DynamoDbKeys.OrderGroupValue } },
            { "Id", new AttributeValue { S = order.Id } },
            { "TableNumber", new AttributeValue { N = order.TableNumber.ToString() } },
            { "SubTotal", new AttributeValue { N = order.SubTotal.ToString() } },
            { "OrderStatus", new AttributeValue { S = order.OrderStatus.ToString() } },
            { "PaymentStatus", new AttributeValue { S = order.PaymentStatus.ToString() } },
            { "OrderDate", new AttributeValue { S = order.OrderDate.ToString("o") } },
            {
                "OrderItems", new AttributeValue
                {
                    L = order.OrderItems.Select(p => new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = p.Id } },
                            { "Name", new AttributeValue { S = p.Name } },
                            { "Price", new AttributeValue { N = p.Price.ToString() } },
                            { "ImageUrl", new AttributeValue { S = p.ImageUrl } },
                            { "Type", new AttributeValue { S = p.Type } },
                            { "Quantity", new AttributeValue { N = p.Quantity.ToString() } }
                        }
                    }).ToList()
                }
            }
        };

        if (!string.IsNullOrEmpty(order.PaymentIntentId))
            item["PaymentIntentId"] = new AttributeValue { S = order.PaymentIntentId };

        return item;
    }
}
