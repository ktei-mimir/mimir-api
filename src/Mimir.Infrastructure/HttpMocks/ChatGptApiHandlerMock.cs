using System.Text.Json;
using AutoFixture;
using Mimir.Application.ChatGpt;
using RichardSzalay.MockHttp;

namespace Mimir.Infrastructure.HttpMocks;

public class ChatGptApiHandlerMock : MockHttpMessageHandler
{
    private static readonly IFixture Fixture = new Fixture();

    public ChatGptApiHandlerMock()
    {
        this.When("/v1/chat/completions")
            .Respond("application/json",
                JsonSerializer.Serialize(Fixture.Create<ChatCompletion>()));
        this.When("/v1/completions")
            .Respond("application/json",
                JsonSerializer.Serialize(Fixture.Create<Completion>()));
    }
}