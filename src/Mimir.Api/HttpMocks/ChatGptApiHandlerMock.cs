using System.Text;
using System.Text.Json;
using AutoFixture;
using Microsoft.Extensions.Options;
using Mimir.Api.Configurations;
using Mimir.Application.ChatGpt;
using RichardSzalay.MockHttp;

namespace Mimir.Api.HttpMocks;

public class ChatGptApiHandlerMock : MockHttpMessageHandler
{
    private static readonly IFixture Fixture = new Fixture();

    public ChatGptApiHandlerMock(IOptions<ChatGptOptions> optionsAccessor)
    {
        var chatGptOptions = optionsAccessor.Value;
        this.When(new Uri(new Uri(chatGptOptions.ApiDomain), "/v1/chat/completions").ToString())
            .Respond(
                _ => new StringContent(JsonSerializer.Serialize(Fixture.Create<ChatCompletion>()), Encoding.UTF8,
                    "application/json"));
        this.When(new Uri(new Uri(chatGptOptions.ApiDomain), "/v1/completions").ToString())
            .Respond(
                _ => new StringContent(JsonSerializer.Serialize(Fixture.Create<Completion>()), Encoding.UTF8,
                    "application/json"));
    }
}