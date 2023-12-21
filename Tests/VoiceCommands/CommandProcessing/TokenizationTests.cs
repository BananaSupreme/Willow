using Willow.Core.Helpers.Extensions;
using Willow.Core.SpeechCommands.Tokenization;
using Willow.Core.SpeechCommands.Tokenization.Abstractions;
using Willow.Core.SpeechCommands.Tokenization.Tokens;
using Willow.Core.SpeechCommands.Tokenization.Tokens.Abstractions;

namespace Tests.VoiceCommands.CommandProcessing;

public class TokenizationTests
{
    private readonly ITokenizer _sut;

    public TokenizationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITokenizer, Tokenizer>();
        services.AddAllTypesFromOwnAssembly<ISpecializedTokenProcessor>(ServiceLifetime.Singleton);
        var serviceProvider = services.BuildServiceProvider();
        _sut = serviceProvider.GetRequiredService<ITokenizer>();
    }

    public static object[][] ValidTestDataWrapper =>
        ValidTestData.Select((x, idx) => new object[] { idx, x.Item1 }).ToArray();

    private static (string, Token[])[] ValidTestData =>
    [
        ("Word", [new WordToken("Word")]),
        ("a", [new WordToken("a")]),
        ("Word1 Word2", [new WordToken("Word1"), new WordToken("Word2")]),
        ("w b", [new WordToken("w"), new WordToken("b")]),
        ("   Word   ", [new WordToken("Word")]),
        ("Word1      Word2", [new WordToken("Word1"), new WordToken("Word2")]),
        ("12", [new NumberToken(12)]),
        ("12 13", [new NumberToken(12), new NumberToken(13)]),
        ("12 something 13 new",
         [new NumberToken(12), new WordToken("something"), new NumberToken(13), new WordToken("new")])
    ];

    [Fact]
    public void When_EmptyOrWhiteSpace_ReturnsEmpty()
    {
        var result = _sut.Tokenize(string.Empty);
        result.Should().BeEmpty();
        result = _sut.Tokenize("   ");
        result.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(ValidTestDataWrapper))]
    public void TestInputs(int idx, string input)
    {
        var output = ValidTestData[idx].Item2;
        output = [.. output];
        var result = _sut.Tokenize(input);
        result.Should().BeEquivalentTo(output);
    }
}