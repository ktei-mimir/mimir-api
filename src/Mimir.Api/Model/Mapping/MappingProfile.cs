using AutoMapper;
using Mimir.Api.Model.Conversations;
using Mimir.Application.Features.CreateConversation;

namespace Mimir.Api.Model.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateConversationRequest, CreateConversationCommand>();
    }
}