using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System.Reflection;

using Willow.Core.Environment.Models;
using Willow.Core.SpeechCommands.Tokenization.Tokens;
using Willow.Core.SpeechCommands.VoiceCommandCompilation;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Abstractions;
using Willow.Core.SpeechCommands.VoiceCommandCompilation.Models;
using Willow.Core.SpeechCommands.VoiceCommandParsing;
using Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors;

using OptionalNodeProcessor = Willow.Core.SpeechCommands.VoiceCommandParsing.NodeProcessors.OptionalNodeProcessor;

namespace Tests.VoiceCommands.CommandProcessing;

public class TrieConstructionTests
{
    private readonly IVoiceCommandCompiler _compiler;
    private readonly ITrieFactory _sut;
    private readonly Fixture _fixture;

    public TrieConstructionTests()
    {
        _fixture = new();

        _fixture.Register(() => new Dictionary<string, object>());
        _fixture.Register(() => new TagRequirement[] { new([]) });

        _compiler = Substitute.For<IVoiceCommandCompiler>();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITrieFactory, TrieFactory>();
        services.AddSingleton<IVoiceCommandCompiler>(implementationFactory: _ => _compiler);
        var serviceProvider = services.BuildServiceProvider();
        _sut = serviceProvider.GetRequiredService<ITrieFactory>();
    }

    [Fact]
    public void When_InsertingParsersWithFullOverlap_SecondLongerChainIsAppended()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase #amount" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase #amount and" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new NumberNodeProcessor(CaptureName: "amount"),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new NumberNodeProcessor(CaptureName: "amount"),
                new WordNodeProcessor(Value: new(Value: "and")),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new WordNodeProcessor(Value: new(Value: "increase")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new WordNodeProcessor(Value: new(Value: "and")),
                                        children:
                                        [
                                            new(tagRequirements: [new(Tags: [])],
                                                nodeProcessor: new CommandSuccessNodeProcessor(
                                                    CommandId: commands[1].Id),
                                                children: [])
                                        ]
                                    ),
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                        children: [])
                                ]
                            )
                        ]
                    )
                ]
            )
        );

        TestInternal(commands: commands, expectedTrie: expectedTrie);
    }

    [Fact]
    public void When_InsertingParsersWithPartialOverlap_BranchIsCreatedAtSplitPoint()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase for #amount" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase and #amount" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new WordNodeProcessor(Value: new(Value: "and")),
                new NumberNodeProcessor(CaptureName: "amount"),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new WordNodeProcessor(Value: new(Value: "for")),
                new NumberNodeProcessor(CaptureName: "amount"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);


        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new WordNodeProcessor(Value: new(Value: "increase")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new WordNodeProcessor(Value: new(Value: "and")),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                                        children:
                                        [
                                            new(tagRequirements: [new(Tags: [])],
                                                nodeProcessor: new CommandSuccessNodeProcessor(
                                                    CommandId: commands[0].Id),
                                                children: [])
                                        ]
                                    )
                                ]
                            ),
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new WordNodeProcessor(Value: new(Value: "for")),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                                        children:
                                        [
                                            new(tagRequirements: [new(Tags: [])],
                                                nodeProcessor: new CommandSuccessNodeProcessor(
                                                    CommandId: commands[1].Id),
                                                children: [])
                                        ]
                                    )
                                ]
                            )
                        ]
                    )
                ]
            )
        );

        TestInternal(commands: commands, expectedTrie: expectedTrie);
    }

    [Fact]
    public void When_InsertingDistinctParsers_SeparateChainsAreFormed()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "[five:six:seven]:enter increase ?[#amount]:match"
            },
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "[one:two:three]:enter increase ?[#amount]:match"
            }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new OneOfNodeProcessor(CaptureName: "enter",
                    ValidWords:
                    [
                        new WordToken(Value: "five"), new WordToken(Value: "six"),
                        new WordToken(Value: "seven")
                    ]),
                new WordNodeProcessor(Value: new(Value: "increase")),
                new OptionalNodeProcessor(InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                    FlagName: "match"),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new OneOfNodeProcessor(CaptureName: "enter",
                    ValidWords:
                    [
                        new WordToken(Value: "one"), new WordToken(Value: "two"),
                        new WordToken(Value: "three")
                    ]),
                new WordNodeProcessor(Value: new(Value: "increase")),
                new OptionalNodeProcessor(InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                    FlagName: "match"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(CaptureName: "enter",
                            ValidWords:
                            [
                                new WordToken(Value: "five"), new WordToken(Value: "six"),
                                new WordToken(Value: "seven")
                            ]), children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new WordNodeProcessor(Value: new(Value: "increase")),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor:
                                        new OptionalNodeProcessor(
                                            InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                                            FlagName: "match"),
                                        children:
                                        [
                                            new(tagRequirements: [new(Tags: [])],
                                                nodeProcessor: new CommandSuccessNodeProcessor(
                                                    CommandId: commands[0].Id),
                                                children: []
                                            )
                                        ]
                                    )
                                ]
                            )
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(CaptureName: "enter",
                            ValidWords:
                            [
                                new WordToken(Value: "one"), new WordToken(Value: "two"),
                                new WordToken(Value: "three")
                            ]), children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new WordNodeProcessor(Value: new(Value: "increase")),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new OptionalNodeProcessor(
                                            InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                                            FlagName: "match"),
                                        children:
                                        [
                                            new(tagRequirements: [new(Tags: [])],
                                                nodeProcessor: new CommandSuccessNodeProcessor(
                                                    CommandId: commands[1].Id),
                                                children: [])
                                        ]
                                    )
                                ]
                            )
                        ]
                    )
                ]
            )
        );

        TestInternal(commands: commands, expectedTrie: expectedTrie);
    }

    [Fact]
    public void When_InsertingDuplicateParsersWithDifferentRequirements_MergedNodeWithCombinedRequirementsIsCreated()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "increase ?[#amount]:hit", TagRequirements = [new(Tags: [new(Name: "hello")])]
            },
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "increase ?[#amount]:hit", TagRequirements = [new(Tags: [new(Name: "world")])]
            }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new OptionalNodeProcessor(InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                    FlagName: "hit"),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new OptionalNodeProcessor(InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                    FlagName: "hit"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [new(Name: "hello")]), new(Tags: [new(Name: "world")])],
                nodeProcessor: new EmptyNodeProcessor(),
                children:
                [
                    new(
                        tagRequirements:
                        [new(Tags: [new(Name: "hello")]), new(Tags: [new(Name: "world")])],
                        nodeProcessor: new WordNodeProcessor(Value: new(Value: "increase")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [new(Name: "hello")]), new(Tags: [new(Name: "world")])],
                                nodeProcessor: new OptionalNodeProcessor(
                                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                                    FlagName: "hit"),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [new(Name: "hello")])],
                                        nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                        children: []
                                    ),
                                    new(tagRequirements: [new(Tags: [new(Name: "world")])],
                                        nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                        children: []
                                    )
                                ]
                            )
                        ]
                    )
                ]
            )
        );

        TestInternal(commands: commands, expectedTrie: expectedTrie);
    }

    [Fact]
    public void When_InsertingDuplicateRequirements_RequirementsAppearOnce()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "increase ?[#amount]:catch", TagRequirements = [new(Tags: [new(Name: "hello")])]
            },
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "increase ?[#amount]:catch", TagRequirements = [new(Tags: [new(Name: "hello")])]
            }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new OptionalNodeProcessor(InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                    FlagName: "catch"),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new OptionalNodeProcessor(InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                    FlagName: "catch"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [new(Name: "hello")])], nodeProcessor: new EmptyNodeProcessor(),
                children:
                [
                    new(
                        tagRequirements:
                        [new(Tags: [new(Name: "hello")])],
                        nodeProcessor: new WordNodeProcessor(Value: new(Value: "increase")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [new(Name: "hello")])],
                                nodeProcessor: new OptionalNodeProcessor(
                                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount")),
                                    FlagName: "catch"),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [new(Name: "hello")])],
                                        nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                        children: []
                                    ),
                                    new(tagRequirements: [new(Tags: [new(Name: "hello")])],
                                        nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                        children: []
                                    )
                                ]
                            )
                        ]
                    )
                ]
            )
        );

        TestInternal(commands: commands, expectedTrie: expectedTrie);
    }

    [Fact]
    public void When_InsertingParsersWithRepeatingPatterns_CorrectStructureIsMaintained()
    {
        //one one one for #amount - one one and one #amount 
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "[one:two]:enter one one for #amount"
            },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one:two]:enter one and one #amount" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new OneOfNodeProcessor(CaptureName: "enter",
                    ValidWords: [new WordToken(Value: "one"), new WordToken(Value: "two")]),
                new WordNodeProcessor(Value: new(Value: "one")),
                new WordNodeProcessor(Value: new(Value: "one")),
                new WordNodeProcessor(Value: new(Value: "for")),
                new NumberNodeProcessor(CaptureName: new(value: "amount")),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new OneOfNodeProcessor(CaptureName: "enter",
                    ValidWords: [new WordToken(Value: "one"), new WordToken(Value: "two")]),
                new WordNodeProcessor(Value: new(Value: "one")),
                new WordNodeProcessor(Value: new(Value: "and")),
                new WordNodeProcessor(Value: new(Value: "one")),
                new NumberNodeProcessor(CaptureName: new(value: "amount")),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(CaptureName: "enter",
                            ValidWords: [new WordToken(Value: "one"), new WordToken(Value: "two")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new WordNodeProcessor(Value: new(Value: "one")),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new WordNodeProcessor(Value: new(Value: "one")),
                                        children:
                                        [
                                            new(tagRequirements: [new(Tags: [])],
                                                nodeProcessor: new WordNodeProcessor(Value: new(Value: "for")),
                                                children:
                                                [
                                                    new(tagRequirements: [new(Tags: [])],
                                                        nodeProcessor: new NumberNodeProcessor(
                                                            CaptureName: new(value: "amount")),
                                                        children:
                                                        [
                                                            new(tagRequirements: [new(Tags: [])],
                                                                nodeProcessor: new CommandSuccessNodeProcessor(
                                                                    CommandId: commands[0].Id),
                                                                children: [])
                                                        ]
                                                    )
                                                ]
                                            )
                                        ]
                                    ),
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new WordNodeProcessor(Value: new(Value: "and")),
                                        children:
                                        [
                                            new(tagRequirements: [new(Tags: [])],
                                                nodeProcessor: new WordNodeProcessor(Value: new(Value: "one")),
                                                children:
                                                [
                                                    new(tagRequirements: [new(Tags: [])],
                                                        nodeProcessor: new NumberNodeProcessor(
                                                            CaptureName: new(value: "amount")),
                                                        children:
                                                        [
                                                            new(tagRequirements: [new(Tags: [])],
                                                                nodeProcessor: new CommandSuccessNodeProcessor(
                                                                    CommandId: commands[1].Id),
                                                                children: [])
                                                        ]
                                                    )
                                                ]
                                            )
                                        ]
                                    )
                                ]
                            )
                        ]
                    )
                ]
            )
        );

        TestInternal(commands: commands, expectedTrie: expectedTrie);
    }

    [Fact]
    public void When_InsertingCommands_TheyAreOrderedCorrectly()
    {
        //ordering should be - word/number > OneOf(also ordered by choice size) > WildCard > RepeatingWildCar > LeafNode- Optional takes the value inside
        //Equivalent should be by order of input
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "*wildCard" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one:two:three]:enter" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one:two]:enter" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "#number" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "**phrase" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "?[*wildcard]:hit" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "?[#number]:hit" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "one" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "one **phrase" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new WildCardNodeProcessor(CaptureName: "wildCard"),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new OneOfNodeProcessor(CaptureName: "enter",
                    ValidWords:
                    [
                        new WordToken(Value: "one"), new WordToken(Value: "two"),
                        new WordToken(Value: "three")
                    ]),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);
        _compiler.Compile(command: commands[2]).Returns(returnThis:
            [
                new OneOfNodeProcessor(CaptureName: "enter",
                    ValidWords:
                    [
                        new WordToken(Value: "one"), new WordToken(Value: "two")
                    ]),
                new CommandSuccessNodeProcessor(CommandId: commands[2].Id)
            ]);
        _compiler.Compile(command: commands[3]).Returns(returnThis:
            [
                new NumberNodeProcessor(CaptureName: "number"),
                new CommandSuccessNodeProcessor(CommandId: commands[3].Id)
            ]);
        _compiler.Compile(command: commands[4]).Returns(returnThis:
            [
                new RepeatingWildCardNodeProcessor(CaptureName: "phrase"),
                new CommandSuccessNodeProcessor(CommandId: commands[4].Id)
            ]);
        _compiler.Compile(command: commands[5]).Returns(returnThis:
            [
                new OptionalNodeProcessor(InnerNode: new WildCardNodeProcessor(CaptureName: "wildCard"),
                    FlagName: "hit"),
                new CommandSuccessNodeProcessor(CommandId: commands[5].Id)
            ]);
        _compiler.Compile(command: commands[6]).Returns(returnThis:
            [
                new OptionalNodeProcessor(InnerNode: new NumberNodeProcessor(CaptureName: "number"), FlagName: "hit"),
                new CommandSuccessNodeProcessor(CommandId: commands[6].Id)
            ]);
        _compiler.Compile(command: commands[7]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "one")),
                new CommandSuccessNodeProcessor(CommandId: commands[7].Id)
            ]);
        _compiler.Compile(command: commands[8]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "one")),
                new RepeatingWildCardNodeProcessor(CaptureName: "phrase"),
                new CommandSuccessNodeProcessor(CommandId: commands[8].Id)
            ]);
        //ordering should be - word/number > OneOf(also ordered by choice size) > WildCard > RepeatingWildCar > LeafNode- Optional takes the value inside
        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new NumberNodeProcessor(CaptureName: "number"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[3].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OptionalNodeProcessor(
                            InnerNode: new NumberNodeProcessor(CaptureName: "number"), FlagName: "hit"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[6].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new WordNodeProcessor(Value: new(Value: "one")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new RepeatingWildCardNodeProcessor(CaptureName: "phrase"),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[8].Id),
                                        children: [])
                                ]
                            ),
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[7].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(CaptureName: "enter",
                            ValidWords: [new WordToken(Value: "one"), new WordToken(Value: "two")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[2].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(CaptureName: "enter",
                            ValidWords:
                            [
                                new WordToken(Value: "one"), new WordToken(Value: "two"),
                                new WordToken(Value: "three")
                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new WildCardNodeProcessor(CaptureName: "wildCard"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OptionalNodeProcessor(
                            InnerNode: new WildCardNodeProcessor(CaptureName: "wildCard"), FlagName: "hit"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[5].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new RepeatingWildCardNodeProcessor(CaptureName: "phrase"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[4].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands: commands, expectedTrie: expectedTrie);
    }

    private void TestInternal(PreCompiledVoiceCommand[] commands, Trie expectedTrie)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new MyContractResolver(), Formatting = Formatting.Indented
        };

        _sut.Set(commands: commands);
        var result = _sut.Get();
        var resultString = JsonConvert.SerializeObject(value: result, settings: settings);
        var expectedTrieString = JsonConvert.SerializeObject(value: expectedTrie, settings: settings);
        resultString.Should().BeEquivalentTo(expected: expectedTrieString);
    }

    private class MyContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type
                        .GetProperties(
                            bindingAttr: BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(selector: p => base.CreateProperty(member: p, memberSerialization: memberSerialization))
                        .Union(second: type
                                       .GetFields(bindingAttr: BindingFlags.Public | BindingFlags.NonPublic |
                                                               BindingFlags.Instance)
                                       .Select(selector: f =>
                                           base.CreateProperty(member: f, memberSerialization: memberSerialization)))
                        .ToList();
            props.ForEach(action: p =>
            {
                p.Writable = true;
                p.Readable = true;
            });
            return props;
        }
    }
}