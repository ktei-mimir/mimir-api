﻿namespace Mimir.Api.Model.Messages;

public class CreateMessageRequest
{
    public string StreamId { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
