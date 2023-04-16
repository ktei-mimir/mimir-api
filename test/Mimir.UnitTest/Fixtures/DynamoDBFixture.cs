using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Mimir.UnitTest.Helpers;

namespace Mimir.UnitTest.Fixtures;

public class DynamoDBFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        var dbClient = DynamoDbUtils.CreateLocalDynamoDbClient();
        // check if table named 'mimir' exists
        var tableNames = await dbClient.ListTablesAsync();
        if (tableNames.TableNames.Contains(DynamoDbUtils.TableName)) return;
        // if not, create it
        await dbClient.CreateTableAsync(new CreateTableRequest
        {
            TableName = DynamoDbUtils.TableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions = new List<AttributeDefinition>
            {
                new()
                {
                    AttributeName = "PK",
                    AttributeType = ScalarAttributeType.S
                },
                new()
                {
                    AttributeName = "SK",
                    AttributeType = ScalarAttributeType.S
                },
                new()
                {
                    AttributeName = "GSI1PK",
                    AttributeType = ScalarAttributeType.S
                },
                new()
                {
                    AttributeName = "GSI1SK",
                    AttributeType = ScalarAttributeType.S
                }
            },
            KeySchema = new List<KeySchemaElement>
            {
                new()
                {
                    AttributeName = "PK",
                    KeyType = KeyType.HASH
                },
                new()
                {
                    AttributeName = "SK",
                    KeyType = KeyType.RANGE
                }
            },
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
            {
                new()
                {
                    IndexName = "GSI1",
                    KeySchema = new List<KeySchemaElement>
                    {
                        new()
                        {
                            AttributeName = "GSI1PK",
                            KeyType = KeyType.HASH
                        },
                        new()
                        {
                            AttributeName = "GSI1SK",
                            KeyType = KeyType.RANGE
                        }
                    },
                    Projection = new Projection
                    {
                        ProjectionType = ProjectionType.ALL
                    }
                }
            }
        });
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public IAmazonDynamoDB GetDynamoDbClient()
    {
        return DynamoDbUtils.CreateLocalDynamoDbClient();
    }

    public DynamoDBContext GetDynamoDbContext()
    {
        return new DynamoDBContext(GetDynamoDbClient());
    }
}