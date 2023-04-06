using AutoFixture;
using AutoFixture.Xunit2;
using Mimir.Application.ChatGpt;
using Mimir.Application.Configurations;
using Mimir.Application.Features.CreateMessage;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Mimir.UnitTest.Helpers;
using Moq;

namespace Mimir.UnitTest.Application.Features;

public class CreateMessageCommandHandlerTests
{
    [Theory, MoqAutoData]
    public async Task Handle_ShouldCreateMessage(
        [Frozen] Mock<IMessageRepository> messageRepositoryMock,
        [Frozen] Mock<IChatGptApi> chatGptApiMock,
        CreateMessageCommandHandler sut)
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<CreateMessageCommand>();
        var userMessage = new Message(command.ConversationId, command.Role, command.Content, DateTime.UtcNow);
        var utcNow = DateTime.UtcNow;
        var historyMessages = Enumerable.Range(0, 2).Select(x =>
                new Message(command.ConversationId, fixture.Create<string>(), fixture.Create<string>(), utcNow = utcNow.AddMinutes(2)))
            .ToList();
        var gptMessages = historyMessages.Concat(new[] { userMessage }).Select(x => new GptMessage
        {
            Role = x.Role,
            Content = x.Content
        }).ToList();
        var chatCompletion = new Fixture().Create<ChatCompletion>();
        var assistantMessage = new Message(command.ConversationId, chatCompletion.Choices.First().Message.Role,
            chatCompletion.Choices.First().Message.Content, chatCompletion.Created);

        messageRepositoryMock
            .Setup(x => x.ListByConversationId(command.ConversationId, Limits.MaxMessagesPerRequest, default))
            .ReturnsAsync(historyMessages);

        chatGptApiMock
            .Setup(x => x.CreateChatCompletion(It.IsAny<CreateChatCompletionRequest>(), default))
            .ReturnsAsync(chatCompletion);

        // Act
        var result = await sut.Handle(command, default);

        // Assert
        result.Should().BeEquivalentTo(assistantMessage);

        chatGptApiMock.Verify(
            x => x.CreateChatCompletion(
                It.Is<CreateChatCompletionRequest>(r =>
                    r.Messages.SequenceEqual(gptMessages, new GptMessageEqualityComparer())), default),
            Times.Once);
    }

    private class GptMessageEqualityComparer : IEqualityComparer<GptMessage>
    {
        public bool Equals(GptMessage? x, GptMessage? y)
        {
            if (x == null || y == null)
            {
                return false;
            }
        
            return x.Role == y.Role && x.Content == y.Content;
        }

        public int GetHashCode(GptMessage obj)
        {
            return obj.Role.GetHashCode() ^ obj.Content.GetHashCode();
        }
    }
}