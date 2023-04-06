using AutoFixture.Xunit2;
using Mimir.Application.Configurations;
using Mimir.Application.Features.ListConversations;
using Mimir.Domain.Models;
using Mimir.Domain.Repositories;
using Mimir.UnitTest.Helpers;
using Moq;

namespace Mimir.UnitTest.Application.Features;

public class ListConversationsQueryHandlerTests
{
    [Theory, MoqAutoData]
    public async Task List_conversations(
        ListConversationsQuery query,
        List<Conversation> conversations,
        [Frozen] Mock<IConversationRepository> conversationRepositoryMock,
        ListConversationsQueryHandler sut)
    {
        // Arrange
        conversationRepositoryMock.Setup(x => x.ListAll(Limits.MaxConversationsPerRequest, default))
            .ReturnsAsync(conversations);

        // Act
        var result = await sut.Handle(query, default);

        // Assert
        result.Should().BeEquivalentTo(conversations);
        conversationRepositoryMock.Verify(x => x.ListAll(Limits.MaxConversationsPerRequest, default), Times.Once);
    }
}