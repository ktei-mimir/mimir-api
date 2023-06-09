﻿using System.Net.Http.Json;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Api.Model.Conversations;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Mimir.IntegrationTest.Helpers;

namespace Mimir.IntegrationTest.Endpoints;

public class ListConversationsTests : EndpointTestBase
{
    public ListConversationsTests(WebApplicationFactory<Program> factory) : base(
        factory)
    {
    }

    [Fact]
    public async Task List_conversations()
    {
        var fixture = new Fixture();
        var username = TestAuthHandler.DefaultUsername;
        var conversations = Enumerable.Range(0, 2)
            .Select(_ =>
                new Conversation(Guid.NewGuid().ToString(),
                    username,
                    fixture.Create<string>(), fixture.Create<DateTime>()))
            .ToArray();
        using (var scope = Factory.Services.CreateScope())
        {
            var conversationRepository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();
            foreach (var conversation in conversations) await conversationRepository.Create(conversation);
        }

        var client = Factory
            .CreateClient();
        var response = await client.GetFromJsonAsync<ListConversationsResponse>("/v1/conversations");

        response.Should().NotBeNull();
        response!.Items.Should().HaveCountGreaterOrEqualTo(conversations.Length);
    }
}