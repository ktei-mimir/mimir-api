using AutoMapper;
using Mimir.Api.Model.Conversations;
using Mimir.Api.Model.Messages;
using Mimir.Application.Features.CreateConversation;
using Mimir.Domain.Models;

namespace Mimir.Api.Model.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateConversationRequest, CreateConversationCommand>();
        CreateMap<Conversation, ConversationDto>();
        CreateMap<Message, MessageDto>();
    }
}