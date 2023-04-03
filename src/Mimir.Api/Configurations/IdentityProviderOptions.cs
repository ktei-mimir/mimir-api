﻿namespace Mimir.Api.Configurations;

public class IdentityProviderOptions
{
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}