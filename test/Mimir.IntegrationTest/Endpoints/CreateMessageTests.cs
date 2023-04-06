using Microsoft.AspNetCore.Mvc.Testing;

namespace Mimir.IntegrationTest.Endpoints;

public class CreateMessageTests : EndpointTestBase
{
    public CreateMessageTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }
}