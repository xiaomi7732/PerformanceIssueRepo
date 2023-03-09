using System;

namespace OPI.WebAPI.Contracts;

public record Error
{
    public string Message { get; init; } = default!;
    public Uri HelpLink { get; init; } = default!;
}