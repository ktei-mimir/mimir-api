﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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

    public async Task<List<Conversation>> ListByUsername(string username, int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var request = new QueryRequest
        {
            TableName = _options.TableName,
            IndexName = "GSI1",
            KeyConditionExpression = "GSI1PK = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue($"{username}#CONVERSATION") }
            },
            Limit = limit,
            ScanIndexForward = false // sort in descending order
        };

        var response = await _dynamoDb.QueryAsync(request, cancellationToken);

        return response.Items
            .Select(item => _dynamoDbContext.FromDocument<Conversation>(Document.FromAttributeMap(item))).ToList();
    }

    public async Task Create(Conversation conversation, IEnumerable<Message>? messages = null,
        CancellationToken cancellationToken = default)
    {
        var conversationDocument = _dynamoDbContext.ToDocument(conversation);
        conversationDocument["PK"] = $"CONVERSATION#{conversation.Id}";
        conversationDocument["SK"] = $"CONVERSATION#{conversation.CreatedAt}";
        conversationDocument["GSI1PK"] = $"{conversation.Username}#CONVERSATION";
        conversationDocument["GSI1SK"] = conversation.CreatedAt.ToString();
        var transactItems = new List<TransactWriteItem>
        {
            new()
            {
                Put = new Put
                {
                    Item = conversationDocument.ToAttributeMap(),
                    TableName = _options.TableName
                }
            }
        };
        if (messages != null)
            transactItems.AddRange(messages.Select(m =>
            {
                var messageDocument = _dynamoDbContext.ToDocument(m);
                messageDocument["PK"] = $"CONVERSATION#{conversation.Id}";
                messageDocument["SK"] = $"MESSAGE#{m.CreatedAt}#{m.Role}";
                return messageDocument;
            }).Select(messageDocument => new TransactWriteItem
            {
                Put = new Put
                {
                    Item = messageDocument.ToAttributeMap(),
                    TableName = _options.TableName
                }
            }));

        await _dynamoDb.TransactWriteItemsAsync(new TransactWriteItemsRequest
        {
            TransactItems = transactItems
        }, cancellationToken);
    }
}