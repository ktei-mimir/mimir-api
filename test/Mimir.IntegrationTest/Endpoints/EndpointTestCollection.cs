using Mimir.UnitTest.Fixtures;

namespace Mimir.IntegrationTest.Endpoints;

[CollectionDefinition(CollectionName)]
public class EndpointTestCollection : ICollectionFixture<DynamoDBFixture>
{
    public const string CollectionName = nameof(EndpointTestCollection);
}