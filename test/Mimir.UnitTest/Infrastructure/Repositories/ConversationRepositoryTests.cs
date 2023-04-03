using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using Mimir.Domain.Models;
using Mimir.Infrastructure.Configurations;
using Mimir.Infrastructure.Repositories;
using Mimir.UnitTest.Helpers;

namespace Mimir.UnitTest.Infrastructure.Repositories;

public class ConversationRepositoryTests
{
    private readonly ConversationRepository _sut;

    public ConversationRepositoryTests()
    {
        _sut = CreateSut();
    }

    [Fact]
    public async Task Create_a_conversation()
    {
        var conversation = new Conversation(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), DateTime.UtcNow);
        var firstMessage = new Message(conversation.Id, "user", "Hello, world!", DateTime.UtcNow);
        await _sut.Create(conversation, firstMessage);
        
        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        var savedConversation = await dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbUtils.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"PK", new AttributeValue($"CONVERSATION#{conversation.Id}")},
                {"SK", new AttributeValue($"CONVERSATION#{conversation.CreatedAt}")}
            }
        });
        var savedMessage = await dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbUtils.TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"PK", new AttributeValue($"CONVERSATION#{conversation.Id}")},
                {"SK", new AttributeValue($"MESSAGE#{firstMessage.CreatedAt}")}
            }
        });
        savedConversation.Item.Should().NotBeNull();
        savedMessage.Item.Should().NotBeNull();
    }
    
    private static ConversationRepository CreateSut()
    {
        var dynamoDb = DynamoDbUtils.CreateLocalDynamoDbClient();
        var sut = new ConversationRepository(dynamoDb, new DynamoDBContext(dynamoDb), Options.Create<DynamoDbOptions>(new DynamoDbOptions
        {
            TableName = DynamoDbUtils.TableName
        }));
        return sut;
    }
}