﻿using System.Net.Http.Json;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Api.Model.Messages;
using Mimir.Application.ChatGpt;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Message = Mimir.Domain.Models.Message;

namespace Mimir.IntegrationTest.Endpoints;

public class ListMessagesTests : EndpointTestBase
{
    public ListMessagesTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task List_messages()
    {
        var conversationId = Guid.NewGuid().ToString();
        var fixture = new Fixture();
        using (var scope = Factory.Services.CreateScope())
        {
            var conversationRepository = scope.ServiceProvider.GetRequiredService<IConversationRepository>();
            var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
            await conversationRepository.Create(new Conversation(conversationId, fixture.Create<string>(),
                DateTime.UtcNow));
            var utcNow = DateTime.UtcNow;
            foreach (var _ in Enumerable.Range(0, 10))
            {
                await messageRepository.Create(new Message(conversationId, Roles.User, fixture.Create<string>(),
                    utcNow = utcNow.AddMinutes(1)));
            }
        }

        var client = Factory
            .CreateClient();
        var response = await client.GetFromJsonAsync<MessageDto[]>($"/v1/conversations/{conversationId}/messages");

        response.Should().NotBeNull();
        response!.Should().HaveCount(10);
    }
}