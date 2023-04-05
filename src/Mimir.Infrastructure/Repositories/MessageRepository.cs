using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Mimir.Infrastructure.Configurations;

namespace Mimir.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly DynamoDbOptions _options;

    public MessageRepository(IAmazonDynamoDB dynamoDb, IDynamoDBContext dynamoDbContext,
        IOptions<DynamoDbOptions> optionsAccessor)
    {
        _dynamoDb = dynamoDb;
        _dynamoDbContext = dynamoDbContext;
        _options = optionsAccessor.Value;
    }

    public async Task Create(Message message, CancellationToken cancellationToken = default)
    {
        var messageDocument = _dynamoDbContext.ToDocument(message);
        messageDocument["PK"] = $"CONVERSATION#{message.ConversationId}";
        messageDocument["SK"] = $"MESSAGE#{message.CreatedAt}";
        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            Item = messageDocument.ToAttributeMap(),
            TableName = _options.TableName
        }, cancellationToken);
    }

    public async Task<List<Message>> ListByConversationId(string conversationId, int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var queryRequest = new QueryRequest
        {
            TableName = _options.TableName,
            KeyConditionExpression = "PK = :pk and SK begins_with(:sk)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue($"CONVERSATION#{conversationId}") },
                { ":sk", new AttributeValue("MESSAGE#") }
            },
            Limit = limit
        };

        var response = await _dynamoDb.QueryAsync(queryRequest, cancellationToken);

        return response.Items.Select(x => _dynamoDbContext.FromDocument<Message>(Document.FromAttributeMap(x)))
            .ToList();
    }
}