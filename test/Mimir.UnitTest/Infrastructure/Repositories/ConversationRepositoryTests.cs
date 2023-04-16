using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Microsoft.Extensions.Options;
using Mimir.Domain.Helpers;
using Mimir.Domain.Models;
using Mimir.Infrastructure.Configurations;
using Mimir.Infrastructure.Repositories;
using Mimir.UnitTest.Fixtures;
using Mimir.UnitTest.Helpers;

namespace Mimir.UnitTest.Infrastructure.Repositories;

public class ConversationRepositoryTests : RepositoryTestBase
{
    private readonly ConversationRepository _sut;

    public ConversationRepositoryTests(DynamoDBFixture dynamoDbFixture) : base(dynamoDbFixture)
    {
        _sut = CreateSut();
    }

    [Fact]
    public async Task Create_a_conversation()
    {
        var conversation = new Conversation(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(), DateTime.UtcNow);

        await _sut.Create(conversation);

        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        var savedConversation = await dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbUtils.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue($"CONVERSATION#{conversation.Id}") },
                { "SK", new AttributeValue($"CONVERSATION#{conversation.CreatedAt}") }
            }
        });
        savedConversation.Item.Should().NotBeNull();
    }

    [Fact]
    public async Task List_conversations_by_CreatedAt_desc()
    {
        var fixture = new Fixture();
        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        var username = Guid.NewGuid().ToString();
        var latestConversation = (await dynamoDb.QueryAsync(new QueryRequest
        {
            IndexName = "GSI1",
            KeyConditionExpression = "GSI1PK = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue($"{username}#CONVERSATION") }
            },
            ScanIndexForward = false,
            Limit = 1,
            TableName = DynamoDbUtils.TableName,
            ProjectionExpression = "GSI1SK"
        })).Items.SingleOrDefault();
        var timestampNow = DateTime.UtcNow.ToUnixTimeStamp();
        if (latestConversation != null)
            timestampNow = Convert.ToInt64(latestConversation["GSI1SK"].S) + 1000;
        var conversations = Enumerable.Range(0, 3)
            .Select(_ => new Conversation(Guid.NewGuid().ToString(),
                username,
                fixture.Create<string>(),
                timestampNow += 1000))
            .ToList();
        foreach (var conversation in conversations)
        {
            var item = Document.FromJson(JsonSerializer.Serialize(conversation))
                .ToAttributeMap();
            item["PK"] = new AttributeValue($"CONVERSATION#{conversation.Id}");
            item["SK"] = new AttributeValue($"CONVERSATION#{conversation.CreatedAt}");
            item["GSI1PK"] = new AttributeValue($"{username}#CONVERSATION");
            item["GSI1SK"] = new AttributeValue(conversation.CreatedAt.ToString());
            await dynamoDb.PutItemAsync(new PutItemRequest
            {
                Item = item,
                TableName = DynamoDbUtils.TableName
            });
        }

        var actual = await _sut.ListByUsername(username, 3);

        var sorted = conversations.OrderByDescending(x => x.CreatedAt).ToList();
        actual.Should().BeEquivalentTo(sorted, options => options.WithStrictOrdering());
    }

    private static ConversationRepository CreateSut()
    {
        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        var sut = new ConversationRepository(dynamoDb, new DynamoDBContext(dynamoDb), Options.Create(new DynamoDbOptions
        {
            TableName = DynamoDbUtils.TableName
        }));
        return sut;
    }
}