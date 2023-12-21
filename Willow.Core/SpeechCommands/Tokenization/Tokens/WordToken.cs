﻿using Willow.Core.SpeechCommands.Tokenization.Enums;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Willow.Core.SpeechCommands.Tokenization.Tokens;

public sealed record WordToken(string Value) : Token
{
    public override TokenType Type => TokenType.Word;

    public override string GetString()
    {
        return Value;
    }

    public override string ToString()
    {
        return GetString();
    }

    public bool Equals(WordToken? other)
    {
        return other is not null
               && other.Value.Equals(Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }
}