using Mimir.UnitTest.Fixtures;

namespace Mimir.UnitTest.Infrastructure.Repositories;

[CollectionDefinition(CollectionName)]
public class RepositoryTestCollection : ICollectionFixture<DynamoDBFixture>
{
    public const string CollectionName = nameof(RepositoryTestCollection);
}