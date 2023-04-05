using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Api.Model.Conversations;
using Mimir.Application.ChatGpt;
using Mimir.Application.Features.CreateConversation;
using Mimir.IntegrationTest.Helpers;
using Refit;
using RichardSzalay.MockHttp;

namespace Mimir.IntegrationTest.Endpoints;

public class CreateConversationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    public CreateConversationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
            });
        });
    }

    [Fact]
    public async Task Create_a_conversation()
    {
        var fixture = new Fixture();
        var completion = fixture.Create<Completion>();
        var chatCompletion = fixture.Create<ChatCompletion>();
            
        var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var mockHttpMessageHandler = new MockHttpMessageHandler();
                    mockHttpMessageHandler.When("/v1/completions")
                        .Respond("application/json",
                            JsonSerializer.Serialize(completion));
                    mockHttpMessageHandler.When("/v1/chat/completions")
                        .Respond("application/json",
                            JsonSerializer.Serialize(chatCompletion));
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
        actualResponse!.Choices.Should().HaveCount(chatCompletion.Choices.Count);
    }
}