using Willow.Core.Helpers.Extensions;
using Willow.Speech.Tokenization;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;

namespace Tests.Speech.CommandProcessing;

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
        ValidTestData.Select((x, index) => new object[] { index, x.Item1 }).ToArray();

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
    public void TestInputs(int index, string input)
    {
        var output = ValidTestData[index].Item2;
        output = [.. output];
        var result = _sut.Tokenize(input);
        result.Should().BeEquivalentTo(output);
    }
}