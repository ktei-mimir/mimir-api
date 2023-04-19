using System.Text.Json;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Microsoft.Extensions.Options;
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
    public async Task Create_a_conversation_with_2_messages()
    {
        var conversation = new Conversation(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(), DateTime.UtcNow);
        var message1 = new Message(conversation.Id, "system", "Hello, world!", DateTime.UtcNow.AddSeconds(1));
        var message2 = new Message(conversation.Id, "system", "Goodbye, world!", DateTime.UtcNow.AddSeconds(1));
        var messages = new List<Message> { message1, message2 };

        await _sut.Create(conversation, messages);

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
        var savedMessages = await dynamoDb.QueryAsync(new QueryRequest
        {
            TableName = DynamoDbUtils.TableName,
            KeyConditionExpression = "PK = :pk and begins_with(SK, :sk)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue($"CONVERSATION#{conversation.Id}") },
                { ":sk", new AttributeValue("MESSAGE#") }
            }
        });
        savedConversation.Item.Should().NotBeNull();
        savedMessages.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task List_conversations_by_CreatedAt_desc()
    {
        var fixture = new Fixture();
        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        var username = Guid.NewGuid().ToString();
        var utcNow = DateTime.UtcNow;
        var conversations = Enumerable.Range(0, 3)
            .Select(_ => new Conversation(Guid.NewGuid().ToString(),
                username,
                fixture.Create<string>(),
                utcNow = utcNow.AddMinutes(1)))
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