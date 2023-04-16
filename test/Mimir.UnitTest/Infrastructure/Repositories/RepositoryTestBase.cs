using Mimir.UnitTest.Fixtures;

namespace Mimir.UnitTest.Infrastructure.Repositories;

[Collection(RepositoryTestCollection.CollectionName)]
public abstract class RepositoryTestBase
{
    protected RepositoryTestBase(DynamoDBFixture dynamoDbFixture)
    {
        DynamoDbFixture = dynamoDbFixture;
    }

    protected DynamoDBFixture DynamoDbFixture { get; }
}