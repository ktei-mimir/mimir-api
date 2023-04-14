using AutoFixture.Xunit2;
using Mimir.Application.Configurations;
using Mimir.Application.Features.ListMessages;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Mimir.UnitTest.Helpers;
using Moq;

namespace Mimir.UnitTest.Application.Features;

public class ListMessagesQueryHandlerTests
{
    [Theory, MoqAutoData]
    public async Task List_conversation_messages(
        string conversationId,
        [Frozen] Mock<IMessageRepository> mockRepository,
        ListMessagesQueryHandler handler)
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var username = Guid.NewGuid().ToString();
        var messages = Enumerable.Range(0, 10)
            .Select(x => new Message(Guid.NewGuid().ToString(), "user", Guid.NewGuid().ToString(),
                utcNow = utcNow.AddMinutes(1)))
            .ToList();
        mockRepository
            .Setup(x => x.ListByConversationId(conversationId, Limits.MaxMessagesPerRequest, CancellationToken.None))
            .ReturnsAsync(messages);

        // Act
        var result = await handler.Handle(new ListMessagesQuery(username, conversationId),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(messages);
    }
}