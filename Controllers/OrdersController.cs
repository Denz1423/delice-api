using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using delice_api.DTOs;
using delice_api.Entities;
using delice_api.Entities.Order;
using delice_api.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly DynamoDB _dynamoDB;
        private readonly IConfiguration _config;

        public OrdersController(DynamoDB dynamoDB, IConfiguration config)
        {
            _dynamoDB = dynamoDB;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            var request = new QueryRequest
            {
                TableName = _config["DynamoDb:OrdersTable"],
                KeyConditionExpression = "#group = :groupValue",
                ExpressionAttributeNames = new Dictionary<string, string> { { "#group", "Group" } },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {
                        ":groupValue",
                        new AttributeValue { S = "ORDER" }
                    }
                }
            };

            try
            {
                var response = await _dynamoDB.Client.QueryAsync(request);
                var items = response.Items;

                var orders = items
                    .Select(item => new OrderDto
                    {
                        Id = item["Id"].S,
                        TableNumber = int.Parse(item["TableNumber"].N),
                        OrderDate = DateTime.Parse(item["OrderDate"].S),
                        OrderItems = item["OrderItems"]
                            .L.Select(attrValue => new OrderItemDto
                            {
                                Id = attrValue.M["Id"].N,
                                Name = attrValue.M["Name"].S,
                                Type = attrValue.M["Type"].S,
                                Price = double.Parse(attrValue.M["Price"].N),
                                Quantity = int.Parse(attrValue.M["Quantity"].N)
                            })
                            .ToList(),
                        SubTotal = double.Parse(item["SubTotal"].N),
                        OrderStatus = Enum.Parse<OrderStatus>(item["OrderStatus"].S),
                        PaymentStatus = Enum.Parse<PaymentStatus>(item["PaymentStatus"].S)
                    })
                    .ToList();

                return Ok(orders);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Cart cart)
        {
            if (cart == null)
                return BadRequest(
                    new ProblemDetails
                    {
                        Title = "Invalid Cart",
                        Detail =
                            "The cart is empty. Please add items to the cart before creating an order."
                    }
                );

            var total = cart.Products.Sum(product => product.Price * product.Quantity);
            var orderId = Guid.NewGuid().ToString();

            var orderItem = new Dictionary<string, AttributeValue>
            {
                {
                    "Group",
                    new AttributeValue { S = "ORDER" }
                },
                {
                    "Id",
                    new AttributeValue { S = orderId }
                },
                {
                    "TableNumber",
                    new AttributeValue { N = cart.TableNumber.ToString() }
                },
                {
                    "SubTotal",
                    new AttributeValue { N = total.ToString() }
                },
                {
                    "OrderItems",
                    new AttributeValue
                    {
                        L = cart
                            .Products.Select(p => new AttributeValue
                            {
                                M = new Dictionary<string, AttributeValue>
                                {
                                    {
                                        "Id",
                                        new AttributeValue { N = p.Id }
                                    },
                                    {
                                        "Name",
                                        new AttributeValue { S = p.Name }
                                    },
                                    {
                                        "Price",
                                        new AttributeValue { N = p.Price.ToString() }
                                    },
                                     {
                                        "Type",
                                        new AttributeValue { S = p.Type.ToString() }
                                    },
                                    {
                                        "Quantity",
                                        new AttributeValue { N = p.Quantity.ToString() }
                                    }
                                }
                            })
                            .ToList()
                    }
                },
                {
                    "OrderStatus",
                    new AttributeValue { S = OrderStatus.Pending.ToString() }
                },
                {
                    "PaymentStatus",
                    new AttributeValue { S = PaymentStatus.Pending.ToString() }
                },
                {
                    "OrderDate",
                    new AttributeValue { S = DateTime.UtcNow.ToString("o") }
                }
            };

            var request = new PutItemRequest
            {
                TableName = _config["DynamoDb:OrdersTable"],
                Item = orderItem
            };

            try
            {
                await _dynamoDB.Client.PutItemAsync(request);
                return Ok(new { OrderId = orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Problem creating order: {ex.Message}");
            }
        }
    }
}
