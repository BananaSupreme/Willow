using DryIoc.Microsoft.DependencyInjection;

using Tests.Helpers;

using Willow.Core.Eventing.Abstractions;
using Willow.Core.Eventing.Registration;
using Willow.Core.Registration;
using Willow.Core.Settings.Abstractions;
using Willow.Core.Settings.Events;
using Willow.Helpers.Extensions;
using Willow.Speech.Tokenization.Abstractions;
using Willow.Speech.Tokenization.Enums;
using Willow.Speech.Tokenization.Registration;
using Willow.Speech.Tokenization.Settings;
using Willow.Speech.Tokenization.Tokenizers;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;

using Xunit.Abstractions;

namespace Tests.Speech.CommandProcessing;

public sealed class TokenizationTests : IDisposable
{
    private readonly IServiceProvider _provider;
    private readonly ITokenizer _sut;
    private readonly ISettings<HomophoneSettings> _homophoneSettings;

    public TokenizationTests(ITestOutputHelper testOutputHelper)
    {
        _homophoneSettings = Substitute.For<ISettings<HomophoneSettings>>();
        var services = new ServiceCollection();
        services.AddTestLogger(testOutputHelper);
        services.AddRegistration();
        services.AddSettings();
        services.AddSingleton(_homophoneSettings);
        new TokenizationRegistrar().RegisterServices(services);
        new EventingRegistrar().RegisterServices(services);
        services.AddAllTypesAsMappingFromOwnAssembly<ITranscriptionTokenizer>();
        services.AddRegistration();
        services.AddSingleton<IHomophonesDictionaryLoader, HomophoneDictionaryLoaderTestDouble>();
        _provider = services.CreateServiceProvider();
        _homophoneSettings.CurrentValue.Returns(new HomophoneSettings(false, HomophoneType.Caverphone, []));
        _sut = _provider.GetRequiredService<ITokenizer>();
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.RegisterHandler<SettingsUpdatedEvent<HomophoneSettings>, HomophonesTranscriptionTokenizer>();
    }

    public static object[][] ValidTestDataWrapper =>
        ValidTestData.Select(static (x, index) => new object[] { index, x.Item1 }).ToArray();

    private static (string, Token[])[] ValidTestData =>
    [
        ("Value", [new WordToken("Value")]),
        ("a", [new WordToken("a")]),
        ("Word1 Word2", [new WordToken("Word1"), new WordToken("Word2")]),
        ("w b", [new WordToken("w"), new WordToken("b")]),
        ("   Value   ", [new WordToken("Value")]),
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
        var result = _sut.Tokenize(input);
        result.Should().BeEquivalentTo(output, static options => options.ComparingByValue<Token>());
    }

    [Fact]
    public void When_UserDefinesHomophones_TheyAreLoaded()
    {
        _homophoneSettings.CurrentValue.Returns(new HomophoneSettings(false,
                                                                      HomophoneType.Caverphone,
                                                                      new Dictionary<string, string[]>
                                                                      {
                                                                          { "word", ["another"] }
                                                                      }));

        var result = _sut.Tokenize("word");
        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new HomophonesToken(new WordToken("word"), ["another"]));
        result[0].Match(new WordToken("word")).Should().BeTrue();
        result[0].Match(new WordToken("another")).Should().BeTrue();
    }

    [Fact]
    public void When_UserDefinesCaverphone_CaverphoneTokenReturned()
    {
        _homophoneSettings.CurrentValue.Returns(new HomophoneSettings(true, HomophoneType.Caverphone, []));

        var result = _sut.Tokenize("word");
        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new EncodingToken("word", WordEncoderType.Caverphone));
        result[0].Match(new WordToken("word")).Should().BeTrue();
    }

    [Fact]
    public void When_UserDefinesMetaphone_MetaphoneTokenReturned()
    {
        _homophoneSettings.CurrentValue.Returns(new HomophoneSettings(true, HomophoneType.Metaphone, []));

        var result = _sut.Tokenize("word");
        result.Should().HaveCount(1);
        result[0].Should().BeEquivalentTo(new EncodingToken("word", WordEncoderType.Metaphone));
        result[0].Match(new WordToken("word")).Should().BeTrue();
    }

    [Fact]
    public void When_UserDefinesCarnegieMelonAndDictionaryNotLoaded_NoFailure()
    {
        _homophoneSettings.CurrentValue.Returns(
            new HomophoneSettings(true, HomophoneType.CarnegieMelonDictionaryEquivalents, []));
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.Dispatch(
            new SettingsUpdatedEvent<HomophoneSettings>(_homophoneSettings.CurrentValue,
                                                        _homophoneSettings.CurrentValue));

        var result = _sut.Tokenize("course");
        result.Should().HaveCount(1);
        result[0].Match(new WordToken("course")).Should().BeTrue();
    }

    [Fact]
    public async Task When_UserDefinesCarnegieMelonAndDictionaryLoaded_HomophonesLoaded()
    {
        _homophoneSettings.CurrentValue.Returns(
            new HomophoneSettings(true, HomophoneType.CarnegieMelonDictionaryEquivalents, []));
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.Dispatch(
            new SettingsUpdatedEvent<HomophoneSettings>(_homophoneSettings.CurrentValue,
                                                        _homophoneSettings.CurrentValue));
        dispatcher.Flush();
        var tokenizer = _provider.GetServices<ITranscriptionTokenizer>()
                                 .OfType<HomophonesTranscriptionTokenizer>()
                                 .First();
        await tokenizer.FlushAsync();

        var result = _sut.Tokenize("course");
        result.Should().HaveCount(1);
        result[0].Match(new WordToken("course")).Should().BeTrue();
        result[0].Match(new WordToken("coarse")).Should().BeTrue();
        result[0].Match(new WordToken("hello")).Should().BeFalse();
    }

    [Fact]
    public async Task When_UserDefinesCarnegieMelonNearAndDictionaryLoaded_HomophonesLoaded()
    {
        _homophoneSettings.CurrentValue.Returns(
            new HomophoneSettings(true, HomophoneType.CarnegieMelonDictionaryNearEquivalents, []));
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.Dispatch(
            new SettingsUpdatedEvent<HomophoneSettings>(_homophoneSettings.CurrentValue,
                                                        _homophoneSettings.CurrentValue));

        var tokenizer = _provider.GetServices<ITranscriptionTokenizer>()
                                 .OfType<HomophonesTranscriptionTokenizer>()
                                 .First();
        await tokenizer.FlushAsync();

        var result = _sut.Tokenize("haste");
        result.Should().HaveCount(1);
        result[0].Match(new WordToken("haste")).Should().BeTrue();
        result[0].Match(new WordToken("paced")).Should().BeTrue();
        result[0].Match(new WordToken("hello")).Should().BeFalse();
    }

    [Fact]
    public async Task When_UserSwitchesDictionaries_HomophonesLoaded()
    {
        _homophoneSettings.CurrentValue.Returns(
            new HomophoneSettings(true, HomophoneType.CarnegieMelonDictionaryEquivalents, []));
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.Dispatch(
            new SettingsUpdatedEvent<HomophoneSettings>(_homophoneSettings.CurrentValue,
                                                        _homophoneSettings.CurrentValue));

        var tokenizer = _provider.GetServices<ITranscriptionTokenizer>()
                                 .OfType<HomophonesTranscriptionTokenizer>()
                                 .First();
        await tokenizer.FlushAsync();

        var result = _sut.Tokenize("haste");
        result.Should().HaveCount(1);
        result[0].Match(new WordToken("haste")).Should().BeTrue();
        result[0].Match(new WordToken("paced")).Should().BeFalse();
        result[0].Match(new WordToken("hello")).Should().BeFalse();

        _homophoneSettings.CurrentValue.Returns(
            new HomophoneSettings(true, HomophoneType.CarnegieMelonDictionaryNearEquivalents, []));
        dispatcher.Dispatch(
            new SettingsUpdatedEvent<HomophoneSettings>(_homophoneSettings.CurrentValue,
                                                        _homophoneSettings.CurrentValue));
        await tokenizer.FlushAsync();

        result = _sut.Tokenize("haste");
        result.Should().HaveCount(1);
        result[0].Match(new WordToken("haste")).Should().BeTrue();
        result[0].Match(new WordToken("paced")).Should().BeTrue();
        result[0].Match(new WordToken("hello")).Should().BeFalse();

        _homophoneSettings.CurrentValue.Returns(new HomophoneSettings(true, HomophoneType.Caverphone, []));
        dispatcher.Dispatch(
            new SettingsUpdatedEvent<HomophoneSettings>(_homophoneSettings.CurrentValue,
                                                        _homophoneSettings.CurrentValue));
        await tokenizer.FlushAsync();

        result = _sut.Tokenize("haste");
        result.Should().HaveCount(1);
        result[0].Should().BeOfType<EncodingToken>();
        result[0].Match(new WordToken("haste")).Should().BeTrue();
        result[0].Match(new WordToken("paced")).Should().BeFalse();
        result[0].Match(new WordToken("hello")).Should().BeFalse();
    }

    [Fact]
    public async Task When_UserDefinesCarnegieMelonNearAndDictionaryLoadedAndSelfDefined_HomophonesLoaded()
    {
        _homophoneSettings.CurrentValue.Returns(new HomophoneSettings(true,
                                                                      HomophoneType
                                                                          .CarnegieMelonDictionaryNearEquivalents,
                                                                      new Dictionary<string, string[]>
                                                                      {
                                                                          { "haste", ["another"] }
                                                                      }));
        var dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        dispatcher.Dispatch(
            new SettingsUpdatedEvent<HomophoneSettings>(_homophoneSettings.CurrentValue,
                                                        _homophoneSettings.CurrentValue));

        var tokenizer = _provider.GetServices<ITranscriptionTokenizer>()
                                 .OfType<HomophonesTranscriptionTokenizer>()
                                 .First();
        await tokenizer.FlushAsync();

        var result = _sut.Tokenize("haste");
        result.Should().HaveCount(1);
        result[0].Match(new WordToken("haste")).Should().BeTrue();
        result[0].Match(new WordToken("paced")).Should().BeTrue();
        result[0].Match(new WordToken("another")).Should().BeTrue();
        result[0].Match(new WordToken("hello")).Should().BeFalse();
    }

    [Fact]
    public void When_UserDefinesHomophonesAndRequiresEncoding_TheyAreLoadedAndAllMatch()
    {
        _homophoneSettings.CurrentValue.Returns(new HomophoneSettings(true,
                                                                      HomophoneType.Caverphone,
                                                                      new Dictionary<string, string[]>
                                                                      {
                                                                          { "word", ["another"] }
                                                                      }));

        var result = _sut.Tokenize("word");
        result.Should().HaveCount(1);
        result[0]
            .Should()
            .BeEquivalentTo(new HomophonesToken(new EncodingToken("word", WordEncoderType.Caverphone), ["another"]));
        result[0].Match(new WordToken("word")).Should().BeTrue();
        result[0].Match(new WordToken("another")).Should().BeTrue();
    }

    public void Dispose()
    {
        (_provider as IDisposable)?.Dispose();
    }
}