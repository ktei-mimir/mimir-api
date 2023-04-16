using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Api.Model.Conversations;
using Mimir.Application.Features.CreateConversation;
using Mimir.Application.OpenAI;
using Mimir.UnitTest.Fixtures;

namespace Mimir.IntegrationTest.Endpoints;

public class CreateConversationTests : EndpointTestBase
{
    public CreateConversationTests(WebApplicationFactory<Program> factory,
        DynamoDBFixture dynamoDbFixture) : base(factory)
    {
    }

    [Fact]
    public async Task Create_a_conversation()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        var client = Factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // var mockHttpMessageHandler = new MockHttpMessageHandler();
                    // mockHttpMessageHandler.When("/v1/completions")
                    //     .Respond("application/json",
                    //         JsonSerializer.Serialize(completion));
                    // services.AddRefitClient<IChatGptApi>()
                    //     .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://test.com"))
                    //     .ConfigurePrimaryHttpMessageHandler(() => mockHttpMessageHandler);
                    services.AddScoped<IChatGptApi>(_ => fixture.Create<IChatGptApi>());
                });
            })
            .CreateClient();

        var response = await client.PostAsJsonAsync("/v1/conversations", new CreateConversationRequest
        {
            Message = "Hello"
        });

        response.EnsureSuccessStatusCode();
        var actualResponse = await response.Content.ReadFromJsonAsync<CreateConversationResponse>();
        actualResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task Return_bad_request_given_invalid_request()
    {
        var client = Factory
            .CreateClient();

        var response = await client.PostAsJsonAsync("/v1/conversations", new CreateConversationRequest
        {
            Message = string.Empty
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}