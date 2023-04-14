using AutoFixture.Xunit2;
using Mimir.Application.Features.CreateConversation;
using Mimir.Application.OpenAI;
using Mimir.Domain.Models;
using Mimir.UnitTest.Helpers;
using Moq;

namespace Mimir.UnitTest.Application.Features;

public class CreateConversationCommandHandlerTests
{
    [Theory, MoqAutoData]
    public async Task Create_a_new_conversation(
        CreateConversationCommand command,
        [Frozen] Mock<IChatGptApi> chatGptApiMock,
        CreateConversationCommandHandler sut)
    {
        // Arrange
        var conversation = new Conversation(Guid.NewGuid().ToString(), 
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(), DateTime.UtcNow);
        chatGptApiMock.Setup(x => x.CreateCompletion(It.IsAny<CreateCompletionRequest>(), default))
            .ReturnsAsync(new Completion
            {
                Choices = new List<CompletionChoice> { new() { Text = conversation.Title } }
            });

        // Act
        var result = await sut.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Title.Should().Be(conversation.Title);
    }
}