using Amazon.DynamoDBv2;
using Amazon.Runtime;

namespace Mimir.UnitTest.Helpers;

internal static class DynamoDbUtils
{
    public const string TableName = "mimir";
    public static IAmazonDynamoDB CreateLocalDynamoDbClient() =>
        new AmazonDynamoDBClient(new BasicAWSCredentials("test", "test"),
            new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000"
            });
}