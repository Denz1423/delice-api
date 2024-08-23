using Amazon.DynamoDBv2;

namespace delice_api.Services
{
    public class DynamoDB
    {
        private readonly IAmazonDynamoDB _client;

        public DynamoDB(IAmazonDynamoDB client)
        {
            _client = client;
        }

        public IAmazonDynamoDB Client => _client;
    }
}
