using MediatR;
using Mimir.Domain.Models;

namespace Mimir.Application.Features.ListConversations;

public class ListConversationsQuery : IRequest<List<Conversation>>
{
    public string Username { get; set; }

    public ListConversationsQuery(string username)
    {
        Username = username;
    }
}