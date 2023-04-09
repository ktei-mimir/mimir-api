using Mimir.Application.Interfaces;

namespace Mimir.Infrastructure.Impl;

public class DateTimeMachine : IDateTime
{
    public DateTime UtcNow() => DateTime.UtcNow;
}