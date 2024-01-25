using Tests.Helpers;

using Willow.Core.Registration;
using Willow.Helpers.Extensions;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.Tokenization.Tokens.Abstractions;
using Willow.Speech.VoiceCommandCompilation;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Exceptions;
using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing.Abstractions;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

using Xunit.Abstractions;

namespace Tests.Speech.CommandProcessing;

public sealed class CommandCompilationTests : IDisposable
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
    private readonly ServiceProvider _provider;

    private readonly IVoiceCommandCompiler _sut;

    public CommandCompilationTests(ITestOutputHelper testOutputHelper)
    {
        _guid = Guid.Parse("58b36a5f-de95-4986-ac06-35bf9a35966e");
        _base = new PreCompiledVoiceCommand(_guid, string.Empty, [], []);

        _base = _base with { CapturedValues = _capturedValues };

        var services = new ServiceCollection();
        services.AddRegistration();
        services.AddTestLogger(testOutputHelper);
        services.AddSingleton<IVoiceCommandCompiler, VoiceCommandCompiler>();
        services.AddAllTypesFromOwnAssembly<INodeCompiler>();
        _provider = services.BuildServiceProvider();
        _sut = _provider.GetRequiredService<IVoiceCommandCompiler>();
    }

    public static object[][] ValidTestDataWrapper =>
        ValidTestData.Select(static (x, index) => new object[] { index, x.Item1 }).ToArray();

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
        ["&[Not|~[Enough|Right|Squares]"],
        ["&[No|Space]~[Between|Nodes]"],
        ["&[Not|~[Enough|Left|Squares]]]"],
        ["~[NO|FLAG]"],
        ["~[Wrong,Separator]"],
        ["?[captureWithoutAFlagName]"],
        ["pro&cess with symbols in word"],
        ["process# with symbols in word"]
    ];

    private static (string, INodeProcessor[])[] ValidTestData =>
    [
        ("go", [new WordNodeProcessor(new WordToken("go"))]),
        ("decrease Number:value",
         [new WordNodeProcessor(new WordToken("decrease")), new NumberNodeProcessor("value")]),
        ("increase #amount", [new WordNodeProcessor(new WordToken("increase")), new NumberNodeProcessor("amount")]),
        ("plot graph for N:dataPoints points",
         [
             new WordNodeProcessor(new WordToken("plot")),
             new WordNodeProcessor(new WordToken("graph")),
             new WordNodeProcessor(new WordToken("for")),
             new NumberNodeProcessor("dataPoints"),
             new WordNodeProcessor(new WordToken("points"))
         ]),
        ("set alarm for #hour #minute",
         [
             new WordNodeProcessor(new WordToken("set")),
             new WordNodeProcessor(new WordToken("alarm")),
             new WordNodeProcessor(new WordToken("for")),
             new NumberNodeProcessor("hour"),
             new NumberNodeProcessor("minute")
         ]),
        ("search WildCard:query",
         [new WordNodeProcessor(new WordToken("search")), new WildCardNodeProcessor("query")]),
        ("play *songName", [new WordNodeProcessor(new WordToken("play")), new WildCardNodeProcessor("songName")]),
        ("navigate to RepeatingWildCard:destination",
         [
             new WordNodeProcessor(new WordToken("navigate")),
             new WordNodeProcessor(new WordToken("to")),
             new RepeatingWildCardNodeProcessor("destination")
         ]),
        ("open website **websiteAddress",
         [
             new WordNodeProcessor(new WordToken("open")),
             new WordNodeProcessor(new WordToken("website")),
             new RepeatingWildCardNodeProcessor("websiteAddress")
         ]),
        ("open website **websiteAddress{10}",
         [
             new WordNodeProcessor(new WordToken("open")),
             new WordNodeProcessor(new WordToken("website")),
             new RepeatingWildCardNodeProcessor("websiteAddress", 10)
         ]),
        ("open website RepeatingWildCard:websiteAddress{5}",
         [
             new WordNodeProcessor(new WordToken("open")),
             new WordNodeProcessor(new WordToken("website")),
             new RepeatingWildCardNodeProcessor("websiteAddress", 5)
         ]),
        ("create event [work|personal|vacation]:eventType",
         [
             new WordNodeProcessor(new WordToken("create")),
             new WordNodeProcessor(new WordToken("event")),
             new OneOfNodeProcessor("eventType",
                                    [new WordToken("work"), new WordToken("personal"), new WordToken("vacation")])
         ]),
        ("send email to OneOf:recipient{[John|Jane|Sam]}",
         [
             new WordNodeProcessor(new WordToken("send")),
             new WordNodeProcessor(new WordToken("email")),
             new WordNodeProcessor(new WordToken("to")),
             new OneOfNodeProcessor("recipient",
                                    [new WordToken("John"), new WordToken("Jane"), new WordToken("Sam")])
         ]),
        ("turn [_devices]:device off",
         [
             new WordNodeProcessor(new WordToken("turn")),
             new OneOfNodeProcessor("device", (Token[])_capturedValues["devices"]),
             new WordNodeProcessor(new WordToken("off"))
         ]),
        ("show weather ?[*location]:capture",
         [
             new WordNodeProcessor(new WordToken("show")),
             new WordNodeProcessor(new WordToken("weather")),
             new OptionalNodeProcessor("capture", new WildCardNodeProcessor("location"))
         ]),
        ("book a flight to Optional[OneOf:destination{[York|London|Paris]}]:flag",
         [
             new WordNodeProcessor(new WordToken("book")),
             new WordNodeProcessor(new WordToken("a")),
             new WordNodeProcessor(new WordToken("flight")),
             new WordNodeProcessor(new WordToken("to")),
             new OptionalNodeProcessor("flag",
                                       new OneOfNodeProcessor("destination",
                                                              [
                                                                  new WordToken("York"),
                                                                  new WordToken("London"),
                                                                  new WordToken("Paris")
                                                              ]))
         ]),
        ("list files in Opt[WildCard:directory]:name",
         [
             new WordNodeProcessor(new WordToken("list")),
             new WordNodeProcessor(new WordToken("files")),
             new WordNodeProcessor(new WordToken("in")),
             new OptionalNodeProcessor("name", new WildCardNodeProcessor("directory"))
         ]),
        ("list files in Optional[WildCard:directory]:hit",
         [
             new WordNodeProcessor(new WordToken("list")),
             new WordNodeProcessor(new WordToken("files")),
             new WordNodeProcessor(new WordToken("in")),
             new OptionalNodeProcessor("hit", new WildCardNodeProcessor("directory"))
         ]),
        ("list files in Optional[system]:flag",
         [
             new WordNodeProcessor(new WordToken("list")),
             new WordNodeProcessor(new WordToken("files")),
             new WordNodeProcessor(new WordToken("in")),
             new OptionalNodeProcessor("flag", new WordNodeProcessor(new WordToken("system")))
         ]),
        ("set alarm for #hour #minute ?[quickly]:match",
         [
             new WordNodeProcessor(new WordToken("set")),
             new WordNodeProcessor(new WordToken("alarm")),
             new WordNodeProcessor(new WordToken("for")),
             new NumberNodeProcessor("hour"),
             new NumberNodeProcessor("minute"),
             new OptionalNodeProcessor("match", new WordNodeProcessor(new WordToken("quickly")))
         ]),
        ("remind me to *task at #time",
         [
             new WordNodeProcessor(new WordToken("remind")),
             new WordNodeProcessor(new WordToken("me")),
             new WordNodeProcessor(new WordToken("to")),
             new WildCardNodeProcessor("task"),
             new WordNodeProcessor(new WordToken("at")),
             new NumberNodeProcessor("time")
         ]),
        ("calculate #number1 plus #_number2",
         [
             new WordNodeProcessor(new WordToken("calculate")),
             new NumberNodeProcessor("number1"),
             new WordNodeProcessor(new WordToken("plus")),
             new NumberNodeProcessor("_number2")
         ]),
        ("translate WildCard:word to [Spanish|French|German]:language",
         [
             new WordNodeProcessor(new WordToken("translate")),
             new WildCardNodeProcessor("word"),
             new WordNodeProcessor(new WordToken("to")),
             new OneOfNodeProcessor("language",
                                    [new WordToken("Spanish"), new WordToken("French"), new WordToken("German")])
         ]),
        ("play OneOf:genre{_genres} music",
         [
             new WordNodeProcessor(new WordToken("play")),
             new OneOfNodeProcessor("genre", (Token[])_capturedValues["genres"]),
             new WordNodeProcessor(new WordToken("music"))
         ]),
        ("turn Optional[OneOf:device{_devices}]:hit off",
         [
             new WordNodeProcessor(new WordToken("turn")),
             new OptionalNodeProcessor("hit", new OneOfNodeProcessor("device", (Token[])_capturedValues["devices"])),
             new WordNodeProcessor(new WordToken("off"))
         ]),
        ("turn ?[OneOf:device{_devices}]:hit off",
         [
             new WordNodeProcessor(new WordToken("turn")),
             new OptionalNodeProcessor("hit", new OneOfNodeProcessor("device", (Token[])_capturedValues["devices"])),
             new WordNodeProcessor(new WordToken("off"))
         ]),
        ("turn ?[[_devices]:device]:hit off",
         [
             new WordNodeProcessor(new WordToken("turn")),
             new OptionalNodeProcessor("hit", new OneOfNodeProcessor("device", (Token[])_capturedValues["devices"])),
             new WordNodeProcessor(new WordToken("off"))
         ]),
        ("rename WildCard:oldName to WildCard:newName",
         [
             new WordNodeProcessor(new WordToken("rename")),
             new WildCardNodeProcessor("oldName"),
             new WordNodeProcessor(new WordToken("to")),
             new WildCardNodeProcessor("newName")
         ]),
        ("compare prices between [Amazon|eBay|Walmart]:platforms for RepeatingWildCard:productNames{10}",
         [
             new WordNodeProcessor(new WordToken("compare")),
             new WordNodeProcessor(new WordToken("prices")),
             new WordNodeProcessor(new WordToken("between")),
             new OneOfNodeProcessor("platforms",
                                    [new WordToken("Amazon"), new WordToken("eBay"), new WordToken("Walmart")]),
             new WordNodeProcessor(new WordToken("for")),
             new RepeatingWildCardNodeProcessor("productNames", 10)
         ]),
        ("[work|personal|vacation]:eventType",
         [
             new OneOfNodeProcessor("eventType",
                                    [new WordToken("work"), new WordToken("personal"), new WordToken("vacation")])
         ]),
        ("**Phrase", [new RepeatingWildCardNodeProcessor("Phrase")]),
        ("hello &[world|else]",
         [
             new WordNodeProcessor(new WordToken("hello")),
             new AndNodeProcessor([
                                      new WordNodeProcessor(new WordToken("world")),
                                      new WordNodeProcessor(new WordToken("else"))
                                  ])
         ]),
        ("hello ~[world|else]:i &[world|else] something",
         [
             new WordNodeProcessor(new WordToken("hello")),
             new OrNodeProcessor("i",
                                 [
                                     new WordNodeProcessor(new WordToken("world")),
                                     new WordNodeProcessor(new WordToken("else"))
                                 ]),
             new AndNodeProcessor([
                                      new WordNodeProcessor(new WordToken("world")),
                                      new WordNodeProcessor(new WordToken("else"))
                                  ]),
             new WordNodeProcessor(new WordToken("something"))
         ]),
        ("?[~[[hello|world]:captured|something|&[group|?[[lot|anti]:anotherCapture]:hitLot]]:index]:hit",
         [
             new OptionalNodeProcessor("hit",
                                       new OrNodeProcessor("index",
                                                           [
                                                               new OneOfNodeProcessor("captured",
                                                                   [new WordToken("hello"), new WordToken("world")]),
                                                               new WordNodeProcessor(new WordToken("something")),
                                                               new AndNodeProcessor([
                                                                   new WordNodeProcessor(
                                                                       new WordToken("group")),
                                                                   new OptionalNodeProcessor("hitLot",
                                                                       new OneOfNodeProcessor(
                                                                           "anotherCapture",
                                                                           [
                                                                               new WordToken("lot"),
                                                                               new WordToken("anti")
                                                                           ]))
                                                               ])
                                                           ]))
         ]),
        ("**Phrase", [new RepeatingWildCardNodeProcessor("Phrase")])
    ];

    public void Dispose()
    {
        _provider.Dispose();
    }

    [Theory]
    [MemberData(nameof(ValidTestDataWrapper))]
    public void When_ValidData_ReturnCorrectNodes(int index, string input)
    {
        var output = ValidTestData[index].Item2;
        output = [.. output, new CommandSuccessNodeProcessor(_guid)];
        var result = _sut.Compile(_base with { InvocationPhrase = input });
        result.Should().BeEquivalentTo(output, static options => options.ComparingByValue<INodeProcessor>());
    }

    [Theory]
    [MemberData(nameof(InvalidData))]
    public void When_InvalidData_ThrowCompilationException(string input)
    {
        _sut.Invoking(x => x.Compile(_base with { InvocationPhrase = input }))
            .Should()
            .Throw<CommandCompilationException>();
    }
}
