using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Api.Model.Conversations;
using Mimir.Application.ChatGpt;
using Mimir.Application.Features.CreateConversation;
using Refit;
using RichardSzalay.MockHttp;

namespace Mimir.IntegrationTest.Endpoints;

public class CreateConversationTests : EndpointTestBase
{
    public CreateConversationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_a_conversation()
    {
        var fixture = new Fixture();
        var completion = fixture.Create<Completion>();

        var client = Factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var mockHttpMessageHandler = new MockHttpMessageHandler();
                    mockHttpMessageHandler.When("/v1/completions")
                        .Respond("application/json",
                            JsonSerializer.Serialize(completion));
                    services.AddRefitClient<IChatGptApi>()
                        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://test.com"))
                        .ConfigurePrimaryHttpMessageHandler(() => mockHttpMessageHandler);
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