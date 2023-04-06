using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Api.Model.Messages;
using Mimir.Application.ChatGpt;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Refit;
using RichardSzalay.MockHttp;

namespace Mimir.IntegrationTest.Endpoints;

public class CreateMessageTests : EndpointTestBase
{
    public CreateMessageTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task Create_a_message()
    {
        var fixture = new Fixture();
        var chatCompletion = fixture.Create<ChatCompletion>();
        var conversationId = Guid.NewGuid().ToString();
        using (var scope = Factory.Services.CreateScope())
        {
            var conversationRepository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();
            await conversationRepository.Create(new Conversation(conversationId, Guid.NewGuid().ToString(),
                DateTime.UtcNow));
        }
        var client = Factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var mockHttpMessageHandler = new MockHttpMessageHandler();
                    mockHttpMessageHandler.When("/v1/chat/completions")
                        .Respond("application/json",
                            JsonSerializer.Serialize(chatCompletion));
                    services.AddRefitClient<IChatGptApi>()
                        .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://test.com"))
                        .ConfigurePrimaryHttpMessageHandler(() => mockHttpMessageHandler);
                });
            })
            .CreateClient();
        var userMessage = fixture.Create<string>();
        var response = await client.PostAsJsonAsync($"/v1/conversations/{conversationId}/messages", new CreateMessageRequest
        {
            ConversationId = conversationId,
            Content = userMessage
        });

        response.EnsureSuccessStatusCode();
        var actualResponse = await response.Content.ReadFromJsonAsync<MessageDto>();
        actualResponse.Should().NotBeNull();
        actualResponse!.Content.Should().Be(chatCompletion.Choices.First().GptMessage.Content);
        using (var scope = Factory.Services.CreateScope())
        {
            var messageRepository= scope.ServiceProvider.GetRequiredService<IMessageRepository>();
            var historyMessages = await messageRepository.ListByConversationId(conversationId);
            historyMessages.Should().HaveCount(2);
            historyMessages.ElementAt(0).Content.Should().Be(userMessage);
            historyMessages.ElementAt(1).Content.Should().Be(actualResponse.Content);
        }
    }
}