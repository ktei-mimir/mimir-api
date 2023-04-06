using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using Mimir.Application.Helpers;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Mimir.Infrastructure.Configurations;

namespace Mimir.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly DynamoDbOptions _options;
    private const int MaxBatchSize = 25;

    public MessageRepository(IAmazonDynamoDB dynamoDb, IDynamoDBContext dynamoDbContext,
        IOptions<DynamoDbOptions> optionsAccessor)
    {
        _dynamoDb = dynamoDb;
        _dynamoDbContext = dynamoDbContext;
        _options = optionsAccessor.Value;
    }

    public async Task Create(IEnumerable<Message> messages, CancellationToken cancellationToken = default)
    {
        var requests = messages.Select(message =>
        {
            var messageDocument = _dynamoDbContext.ToDocument(message);
            messageDocument["PK"] = $"CONVERSATION#{message.ConversationId}";
            messageDocument["SK"] = $"MESSAGE#{message.CreatedAt}";
            return new Put
            {
                Item = messageDocument.ToAttributeMap(),
                TableName = _options.TableName
            };
        });

        var chunks = requests.ChunkBy(MaxBatchSize);

        foreach (var chunk in chunks)
        {
            var transactWriteRequest = new TransactWriteItemsRequest
            {
                TransactItems = chunk.Select(request => new TransactWriteItem { Put = request }).ToList()
            };

            await _dynamoDb.TransactWriteItemsAsync(transactWriteRequest, cancellationToken);
        }
    }


    public async Task<List<Message>> ListByConversationId(string conversationId, int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var queryRequest = new QueryRequest
        {
            TableName = _options.TableName,
            KeyConditionExpression = "PK = :pk and begins_with(SK, :sk)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue($"CONVERSATION#{conversationId}") },
                { ":sk", new AttributeValue("MESSAGE#") }
            },
            Limit = limit
        };

        var messages = new List<Message>();
        QueryResponse response;
        do
        {
            response = await _dynamoDb.QueryAsync(queryRequest, cancellationToken);
            messages.AddRange(response.Items.Select(x =>
                _dynamoDbContext.FromDocument<Message>(Document.FromAttributeMap(x))));
            queryRequest.ExclusiveStartKey = response.LastEvaluatedKey;
        } while (response.LastEvaluatedKey.Any() && messages.Count < limit);

        return messages.Take(limit).ToList();
    }
}
