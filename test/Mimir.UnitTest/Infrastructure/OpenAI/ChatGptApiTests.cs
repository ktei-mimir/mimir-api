using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Mimir.Application.OpenAI;
using Mimir.Domain.Exceptions;
using Mimir.Infrastructure.OpenAI;
using Mimir.UnitTest.Helpers;
using Moq;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using OpenAI.GPT3.ObjectModels.SharedModels;

namespace Mimir.UnitTest.Infrastructure.OpenAI;

public class ChatGptApiTests
{
    [Theory]
    [MoqAutoData]
    public async Task Completion_should_retry_on_OpenAIException(
        [Frozen] IOpenAIService openAIService,
        ChatGptApi sut)
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        var expected = new CompletionCreateResponse
        {
            Choices = fixture.CreateMany<ChoiceResponse>(3).ToList(),
            Usage = fixture.Create<UsageResponse>()
        };
        var invocations = 0;
        Mock.Get(openAIService)
            .Setup(m => m.Completions.CreateCompletion(It.IsAny<CompletionCreateRequest>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Callback((CompletionCreateRequest _, string? _, CancellationToken _) =>
            {
                invocations++;
                if (invocations < 4) throw new OpenAIAPIException("test");
            })
            .ReturnsAsync(expected);

        var actual = await sut.CreateCompletion(new CreateCompletionRequest(), CancellationToken.None);

        actual.Choices.Should().HaveCount(expected.Choices.Count);
        actual.Choices.Select(x => x.Text).Should().BeEquivalentTo(expected.Choices.Select(x => x.Text));
    }
}