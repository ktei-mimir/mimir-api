using System.Net.Http.Json;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Api.Model.Conversations;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;

namespace Mimir.IntegrationTest.Endpoints;

public class ListConversationsTests : EndpointTestBase
{
    public ListConversationsTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task List_conversations()
    {
        var fixture = new Fixture();
        var conversations = Enumerable.Range(0, 2)
            .Select(_ =>
                new Conversation(Guid.NewGuid().ToString(), fixture.Create<string>(), fixture.Create<DateTime>()))
            .ToArray();
        using (var scope = Factory.Services.CreateScope())
        {
            var conversationRepository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();
            foreach (var conversation in conversations)
            {
                await conversationRepository.Create(conversation);
            }
        }

        var client = Factory
            .CreateClient();
        var response = await client.GetFromJsonAsync<ConversationDto[]>("/v1/conversations");

        response.Should().NotBeNull();
        response.Should().HaveCountGreaterOrEqualTo(conversations.Length);
    }
}