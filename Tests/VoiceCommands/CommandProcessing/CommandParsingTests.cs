using Willow.Core.Helpers.Extensions;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Tests.VoiceCommands.CommandProcessing;

public class CommandParsingTests
{
    private static readonly Dictionary<string, object> _capturedValues = new()
    {
        {
            "devices", new Token[] { new WordToken("Microphone"), new WordToken("Kettle"), new WordToken("Bananas") }
        },
        { "genres", new Token[] { new WordToken("Music"), new WordToken("Rock"), new WordToken("Folk") } }
    };

    private readonly PreCompiledVoiceCommand _base;
    private readonly Guid _guid;

    private readonly IVoiceCommandCompiler _sut;

    public CommandParsingTests()
    {
        _guid = Guid.Parse("58b36a5f-de95-4986-ac06-35bf9a35966e");
        _base = new(_guid, string.Empty, [], []);

        _base = _base with { CapturedValues = _capturedValues };

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IVoiceCommandCompiler, VoiceCommandCompiler>();
        services.AddAllTypesFromOwnAssembly<INodeCompiler>(ServiceLifetime.Singleton);
        var serviceProvider = services.BuildServiceProvider();
        _sut = serviceProvider.GetRequiredService<IVoiceCommandCompiler>();
    }

    public static object[][] ValidTestDataWrapper =>
        ValidTestData.Select((x, idx) => new object[] { idx, x.Item1 }).ToArray();

    public static object[][] InvalidData =>
    [
        [string.Empty],
        ["    "],
        ["NoneExistentNode:VariableName"],
        ["Number:"], // missing capture name
        ["OneOf:MissingCrucialParameter>"],
        ["OneOf:InvalidListMember>"],
        ["OneOf:InvalidListMember{[_missingCapturedItem]}>"],
        ["hanging ? symbol"],
        ["* hanging symbol"],
        ["hanging symbol *"],
        ["?[Number:]"], //invalid inner token
        ["!invalid command format"],
        ["process data #number% error"],
        ["?[captureWithoutAFlagName]"],
        ["pro&cess with symbols in word"],
        ["process# with symbols in word"]
    ];

    private static (string, INodeProcessor[])[] ValidTestData =>
    [
        ("go", [new WordNodeProcessor(new("go"))]),
        ("decrease Number:value", [new WordNodeProcessor(new("decrease")), new NumberNodeProcessor("value")]),
        ("increase #amount", [new WordNodeProcessor(new("increase")), new NumberNodeProcessor("amount")]),
        ("plot graph for N:dataPoints points",
         [
             new WordNodeProcessor(new("plot")), new WordNodeProcessor(new("graph")), new WordNodeProcessor(new("for")),
             new NumberNodeProcessor("dataPoints"), new WordNodeProcessor(new("points"))
         ]),
        ("set alarm for #hour #minute",
         [
             new WordNodeProcessor(new("set")), new WordNodeProcessor(new("alarm")), new WordNodeProcessor(new("for")),
             new NumberNodeProcessor("hour"), new NumberNodeProcessor("minute")
         ]),
        ("search WildCard:query", [new WordNodeProcessor(new("search")), new WildCardNodeProcessor("query")]),
        ("play *songName", [new WordNodeProcessor(new("play")), new WildCardNodeProcessor("songName")]),
        ("navigate to RepeatingWildCard:destination",
         [
             new WordNodeProcessor(new("navigate")), new WordNodeProcessor(new("to")),
             new RepeatingWildCardNodeProcessor("destination")
         ]),
        ("open website **websiteAddress",
         [
             new WordNodeProcessor(new("open")), new WordNodeProcessor(new("website")),
             new RepeatingWildCardNodeProcessor("websiteAddress")
         ]),
        ("open website **websiteAddress{10}",
         [
             new WordNodeProcessor(new("open")), new WordNodeProcessor(new("website")),
             new RepeatingWildCardNodeProcessor("websiteAddress", 10)
         ]),
        ("open website RepeatingWildCard:websiteAddress{5}",
         [
             new WordNodeProcessor(new("open")), new WordNodeProcessor(new("website")),
             new RepeatingWildCardNodeProcessor("websiteAddress", 5)
         ]),
        ("create event [work|personal|vacation]:eventType",
         [
             new WordNodeProcessor(new("create")), new WordNodeProcessor(new("event")),
             new OneOfNodeProcessor("eventType",
                 [new WordToken("work"), new WordToken("personal"), new WordToken("vacation")])
         ]),
        ("send email to OneOf:recipient{[John|Jane|Sam]}",
         [
             new WordNodeProcessor(new("send")), new WordNodeProcessor(new("email")), new WordNodeProcessor(new("to")),
             new OneOfNodeProcessor("recipient", [new WordToken("John"), new WordToken("Jane"), new WordToken("Sam")])
         ]),
        ("turn [_devices]:device off",
         [
             new WordNodeProcessor(new("turn")), new OneOfNodeProcessor("device", (Token[])_capturedValues["devices"]),
             new WordNodeProcessor(new("off"))
         ]),
        ("show weather ?[*location]:capture",
         [
             new WordNodeProcessor(new("show")), new WordNodeProcessor(new("weather")),
             new OptionalNodeProcessor(new WildCardNodeProcessor("location"), "capture")
         ]),
        ("book a flight to Optional[OneOf:destination{[York|London|Paris]}]:flag",
         [
             new WordNodeProcessor(new("book")), new WordNodeProcessor(new("a")), new WordNodeProcessor(new("flight")),
             new WordNodeProcessor(new("to")),
             new OptionalNodeProcessor(new OneOfNodeProcessor("destination",
                 [new WordToken("York"), new WordToken("London"), new WordToken("Paris")]), "flag")
         ]),
        ("list files in Opt[WildCard:directory]:name",
         [
             new WordNodeProcessor(new("list")), new WordNodeProcessor(new("files")), new WordNodeProcessor(new("in")),
             new OptionalNodeProcessor(new WildCardNodeProcessor("directory"), "name")
         ]),
        ("list files in Optional[WildCard:directory]:hit",
         [
             new WordNodeProcessor(new("list")), new WordNodeProcessor(new("files")), new WordNodeProcessor(new("in")),
             new OptionalNodeProcessor(new WildCardNodeProcessor("directory"), "hit")
         ]),
        ("list files in Optional[system]:flag",
         [
             new WordNodeProcessor(new("list")), new WordNodeProcessor(new("files")), new WordNodeProcessor(new("in")),
             new OptionalNodeProcessor(new WordNodeProcessor(new("system")), "flag")
         ]),
        ("set alarm for #hour #minute ?[quickly]:match",
         [
             new WordNodeProcessor(new("set")), new WordNodeProcessor(new("alarm")), new WordNodeProcessor(new("for")),
             new NumberNodeProcessor("hour"), new NumberNodeProcessor("minute"),
             new OptionalNodeProcessor(new WordNodeProcessor(new("quickly")), "match")
         ]),
        ("remind me to *task at #time",
         [
             new WordNodeProcessor(new("remind")), new WordNodeProcessor(new("me")), new WordNodeProcessor(new("to")),
             new WildCardNodeProcessor("task"), new WordNodeProcessor(new("at")), new NumberNodeProcessor("time")
         ]),
        ("calculate #number1 plus #_number2",
         [
             new WordNodeProcessor(new("calculate")), new NumberNodeProcessor("number1"),
             new WordNodeProcessor(new("plus")), new NumberNodeProcessor("_number2")
         ]),
        ("translate WildCard:word to [Spanish|French|German]:language",
         [
             new WordNodeProcessor(new("translate")), new WildCardNodeProcessor("word"),
             new WordNodeProcessor(new("to")),
             new OneOfNodeProcessor("language",
                 [new WordToken("Spanish"), new WordToken("French"), new WordToken("German")])
         ]),
        ("play OneOf:genre{_genres} music",
         [
             new WordNodeProcessor(new("play")), new OneOfNodeProcessor("genre", (Token[])_capturedValues["genres"]),
             new WordNodeProcessor(new("music"))
         ]),
        ("turn Optional[OneOf:device{_devices}]:hit off",
         [
             new WordNodeProcessor(new("turn")),
             new OptionalNodeProcessor(new OneOfNodeProcessor("device", (Token[])_capturedValues["devices"]), "hit"),
             new WordNodeProcessor(new("off"))
         ]),
        ("turn ?[OneOf:device{_devices}]:hit off",
         [
             new WordNodeProcessor(new("turn")),
             new OptionalNodeProcessor(new OneOfNodeProcessor("device", (Token[])_capturedValues["devices"]), "hit"),
             new WordNodeProcessor(new("off"))
         ]),
        ("turn ?[[_devices]:device]:hit off",
         [
             new WordNodeProcessor(new("turn")),
             new OptionalNodeProcessor(new OneOfNodeProcessor("device", (Token[])_capturedValues["devices"]), "hit"),
             new WordNodeProcessor(new("off"))
         ]),
        ("rename WildCard:oldName to WildCard:newName",
         [
             new WordNodeProcessor(new("rename")), new WildCardNodeProcessor("oldName"),
             new WordNodeProcessor(new("to")), new WildCardNodeProcessor("newName")
         ]),
        ("compare prices between [Amazon|eBay|Walmart]:platforms for RepeatingWildCard:productNames{10}",
         [
             new WordNodeProcessor(new("compare")), new WordNodeProcessor(new("prices")),
             new WordNodeProcessor(new("between")),
             new OneOfNodeProcessor("platforms",
                 [new WordToken("Amazon"), new WordToken("eBay"), new WordToken("Walmart")]),
             new WordNodeProcessor(new("for")), new RepeatingWildCardNodeProcessor("productNames", 10)
         ]),
        ("[work|personal|vacation]:eventType",
         [
             new OneOfNodeProcessor("eventType",
                 [new WordToken("work"), new WordToken("personal"), new WordToken("vacation")])
         ]),
        ("**Phrase", [new RepeatingWildCardNodeProcessor("Phrase")])
    ];

    [Theory]
    [MemberData(nameof(ValidTestDataWrapper))]
    public void When_ValidData_ReturnCorrectNodes(int idx, string input)
    {
        var output = ValidTestData[idx].Item2;
        output = [.. output, new CommandSuccessNodeProcessor(_guid)];
        var result = _sut.Compile(_base with { InvocationPhrase = input });
        result.Should().BeEquivalentTo(output, options => options.ComparingByValue<INodeProcessor>());
    }

    [Theory]
    [MemberData(nameof(InvalidData))]
    public void When_InvalidData_ThrowParsingException(string input)
    {
        _sut.Invoking(x => x.Compile(_base with { InvocationPhrase = input }))
            .Should().Throw<CommandCompilationException>();
    }
}