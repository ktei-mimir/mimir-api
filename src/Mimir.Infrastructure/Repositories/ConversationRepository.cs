using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.Infrastructure.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly IDynamoDBContext _dynamoDbContext;

    public ConversationRepository(IAmazonDynamoDB dynamo, IDynamoDBContext dynamoDbContext)
    {
        _dynamoDbContext = dynamoDbContext;
    }

    public async Task Create(Conversation conversation, Message message)
    {
        var table= _dynamoDbContext.GetTargetTable<Conversation>(new DynamoDBOperationConfig
        {
            OverrideTableName = "mimir",
        });
        var document = _dynamoDbContext.ToDocument(conversation);
        document["PK"] = $"CONVERSATION#{conversation.Id}";
        document["SK"] = $"CONVERSATION#{conversation.CreatedAt}";
        await table.PutItemAsync(document);
    }

    public Task<Conversation?> GetById(string id)
    {
        throw new NotImplementedException();
    }
}