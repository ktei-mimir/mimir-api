﻿using Mimir.Domain.Models;

namespace Mimir.Domain.Repositories;

public interface IMessageRepository
{
    Task Create(Message message, CancellationToken cancellationToken = default);

    Task<List<Message>> GetByConversationId(string conversationId, int limit = 10,
        CancellationToken cancellationToken = default);
}