using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using Microsoft.Extensions.Options;
using Mimir.Domain.Models;
using Mimir.Infrastructure.Configurations;
using Mimir.Infrastructure.Repositories;
using Mimir.UnitTest.Helpers;

namespace Mimir.UnitTest.Infrastructure.Repositories;

public class MessageRepositoryTests
{
    private readonly MessageRepository _sut;

    public MessageRepositoryTests()
    {
        _sut = CreateSut();
    }

    [Fact]
    public async Task Create_a_message()
    {
        var message = new Message(Guid.NewGuid().ToString(), "user", "Hello, world!", DateTime.UtcNow);

        await _sut.Create(message);

        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        var savedMessage = await dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbUtils.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue($"CONVERSATION#{message.ConversationId}") },
                { "SK", new AttributeValue($"MESSAGE#{message.CreatedAt}") }
            }
        });
        savedMessage.Item.Should().NotBeNull();
    }

    [Fact]
    public async Task List_messages_by_ConversationId_sorted_by_CreatedAt_ascending()
    {
        var conversationId = Guid.NewGuid().ToString();
        var fixture = new Fixture();
        var conversation = new Conversation(conversationId, Guid.NewGuid().ToString(), DateTime.UtcNow);
        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        await dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbUtils.TableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "PK", new AttributeValue($"CONVERSATION#{conversation.Id}") },
                { "SK", new AttributeValue($"CONVERSATION#{conversation.CreatedAt}") },
                { "Title", new AttributeValue(conversation.Title) },
                { "CreatedAt", new AttributeValue { N = conversation.CreatedAt.ToString() } }
            }
        });
        var messages = Enumerable.Range(0, 3)
            .Select(index =>
                new Message(conversationId, "user", fixture.Create<string>(), DateTime.UtcNow.AddMinutes(index)))
            .ToList();
        foreach (var message in messages)
        {
            await _sut.Create(message);
        }

        var conversationMessages = await _sut.ListByConversationId(conversationId);

        conversationMessages.Should().HaveCount(3);
        conversationMessages.Select(x => x.CreatedAt).Should().BeInAscendingOrder();
    }

    private MessageRepository CreateSut()
    {
        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        var sut = new MessageRepository(dynamoDb, new DynamoDBContext(dynamoDb), Options.Create(new DynamoDbOptions
        {
            TableName = DynamoDbUtils.TableName
        }));
        return sut;
    }
}