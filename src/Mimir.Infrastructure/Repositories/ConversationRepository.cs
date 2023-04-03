using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Mimir.Infrastructure.Configurations;

namespace Mimir.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly DynamoDbOptions _options;

    public ConversationRepository(IAmazonDynamoDB dynamoDb, IDynamoDBContext dynamoDbContext,
        IOptions<DynamoDbOptions> optionsAccessor)
    {
        _dynamoDb = dynamoDb;
        _dynamoDbContext = dynamoDbContext;
        _options = optionsAccessor.Value;
    }

    public async Task Create(Conversation conversation, Message firstMessage, CancellationToken cancellationToken = default)
    {
        var conversationDocument = _dynamoDbContext.ToDocument(conversation);
        conversationDocument["PK"] = $"CONVERSATION#{conversation.Id}";
        conversationDocument["SK"] = $"CONVERSATION#{conversation.CreatedAt}";
        var messageDocument = _dynamoDbContext.ToDocument(firstMessage);
        messageDocument["PK"] = $"CONVERSATION#{conversation.Id}";
        messageDocument["SK"] = $"MESSAGE#{firstMessage.CreatedAt}";
        await _dynamoDb.TransactWriteItemsAsync(new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
            {
                new()
                {
                    Put = new Put
                    {
                        Item = conversationDocument.ToAttributeMap(),
                        TableName = _options.TableName,
                    }
                },
                new()
                {
                    Put = new Put
                    {
                        Item = messageDocument.ToAttributeMap(),
                        TableName = _options.TableName
                    }
                }
            }
        }, cancellationToken);
    }

    public Task<Conversation?> GetById(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}