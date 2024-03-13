﻿namespace ZMK.Domain.Shared;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public string Code { get; }
    public string Text { get; }

    public Error(string code, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(text);
        Text = text;
        Code = code;
    }
}