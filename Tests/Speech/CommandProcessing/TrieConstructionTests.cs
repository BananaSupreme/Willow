using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System.Reflection;

using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.VoiceCommandCompilation;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

namespace Tests.Speech.CommandProcessing;

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
                new OptionalNodeProcessor(FlagName: "match",
                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
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
                new OptionalNodeProcessor(FlagName: "match",
                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
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
                                        new OptionalNodeProcessor(FlagName: "match",
                                            InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
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
                                        nodeProcessor: new OptionalNodeProcessor(FlagName: "match",
                                            InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
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
                new OptionalNodeProcessor(FlagName: "hit",
                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new OptionalNodeProcessor(FlagName: "hit",
                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
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
                                nodeProcessor: new OptionalNodeProcessor(FlagName: "hit",
                                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
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
                new OptionalNodeProcessor(FlagName: "catch",
                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new OptionalNodeProcessor(FlagName: "catch",
                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
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
                                nodeProcessor: new OptionalNodeProcessor(FlagName: "catch",
                                    InnerNode: new NumberNodeProcessor(CaptureName: new(value: "amount"))),
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
    public void When_Ordering_WordBeforeOneOf()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two]:enter" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new WordNodeProcessor(Value: new(Value: "increase")),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new OneOfNodeProcessor("enter", [new WordToken("one"), new WordToken("two")]),
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
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(
                            "enter",
                            [new WordToken("one"), new WordToken("two")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_NumberBeforeOneOf()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two]:enter" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "#number" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new OneOfNodeProcessor("enter", [new WordToken("one"), new WordToken("two")]),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new NumberNodeProcessor("number"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new NumberNodeProcessor("number"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(
                            "enter",
                            [new WordToken("one"), new WordToken("two")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_EqualsTakePrecedenceByOrderFound()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "word" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "#number" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new WordNodeProcessor(new("word")),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new NumberNodeProcessor("number"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new WordNodeProcessor(new("word")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new NumberNodeProcessor("number"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_LeafIsAlwaysLast()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "word **phrase" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "word" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new WordNodeProcessor(new("word")),
                new RepeatingWildCardNodeProcessor("phrase"),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WordNodeProcessor(new("word")),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new WordNodeProcessor(new("word")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new RepeatingWildCardNodeProcessor("phrase"),
                                children:
                                [
                                    new(tagRequirements: [new(Tags: [])],
                                        nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                        children: [])
                                ]
                            ),
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    ),
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_OneOfByWordCount()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two|three]:enter" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two]:enter" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two|three|four]:enter" },
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new OneOfNodeProcessor(
                    "enter",
                    [new WordToken("one"), new WordToken("two"), new WordToken("three")]),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new OneOfNodeProcessor(
                    "enter",
                    [new WordToken("one"), new WordToken("two")]),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);
        _compiler.Compile(command: commands[2]).Returns(returnThis:
            [
                new OneOfNodeProcessor(
                    "enter",
                    [
                        new WordToken("one"), new WordToken("two"),
                        new WordToken("three"), new WordToken("four")
                    ]),
                new CommandSuccessNodeProcessor(CommandId: commands[2].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(
                            "enter",
                            [new WordToken("one"), new WordToken("two")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(
                            "enter",
                            [new WordToken("one"), new WordToken("two"), new WordToken("three")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(
                            "enter",
                            [
                                new WordToken("one"), new WordToken("two"),
                                new WordToken("three"), new WordToken("four")
                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[2].Id),
                                children: [])
                        ]
                    ),
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_WildCardBeforeRepeatingWildCard()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "**phrase" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "*wildcard" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new RepeatingWildCardNodeProcessor("phrase"),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WildCardNodeProcessor("wildcard"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new WildCardNodeProcessor("wildcard"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new RepeatingWildCardNodeProcessor("phrase"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_OneOfBeforeWildCard()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two]:enter" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "*wildcard" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new OneOfNodeProcessor("enter", [new WordToken("one"), new WordToken("two")]),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new WildCardNodeProcessor("wildcard"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OneOfNodeProcessor(
                            "enter",
                            [new WordToken("one"), new WordToken("two")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new WildCardNodeProcessor("wildcard"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_OptionalTakesInner()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "?[*wildcard]:_" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "?[#n]:_" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new OptionalNodeProcessor("_", new WildCardNodeProcessor("wildcard")),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new OptionalNodeProcessor("_", new NumberNodeProcessor("n")),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OptionalNodeProcessor("_", new NumberNodeProcessor("n")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OptionalNodeProcessor("_", new WildCardNodeProcessor("wildcard")),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_AndSums()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "&[#n|#nn]" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "#n" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "&[#n|[one|two]:enter]" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new AndNodeProcessor([new NumberNodeProcessor("n"), new NumberNodeProcessor("nn")]),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new NumberNodeProcessor("n"),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);
        _compiler.Compile(command: commands[2]).Returns(returnThis:
            [
                new AndNodeProcessor([
                                         new NumberNodeProcessor("n"),
                                         new OneOfNodeProcessor("enter", [new WordToken("one"), new WordToken("two")])
                                     ]),
                new CommandSuccessNodeProcessor(CommandId: commands[2].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new NumberNodeProcessor("n"),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new AndNodeProcessor(
                            [new NumberNodeProcessor("n"), new NumberNodeProcessor("nn")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new AndNodeProcessor([
                                                                new NumberNodeProcessor("n"),
                                                                new OneOfNodeProcessor("enter",
                                                                    [
                                                                        new WordToken("one"),
                                                                        new WordToken("two")
                                                                    ])
                                                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[2].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_OrderingAndSumOverflows_TakeMaxInner()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "&[#n|#nn]" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "&[#n|**p]" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "&[#n|#nn|*w]" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new AndNodeProcessor([new NumberNodeProcessor("n"), new NumberNodeProcessor("nn")]),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new AndNodeProcessor([new NumberNodeProcessor("n"), new RepeatingWildCardNodeProcessor("p")]),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);
        _compiler.Compile(command: commands[2]).Returns(returnThis:
            [
                new AndNodeProcessor([
                                         new NumberNodeProcessor("n"),
                                         new NumberNodeProcessor("nn"),
                                         new WildCardNodeProcessor("w")
                                     ]),
                new CommandSuccessNodeProcessor(CommandId: commands[2].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new AndNodeProcessor([
                                                                new NumberNodeProcessor("n"),
                                                                new NumberNodeProcessor("nn")
                                                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new AndNodeProcessor([
                                                                new NumberNodeProcessor("n"),
                                                                new NumberNodeProcessor("nn"),
                                                                new WildCardNodeProcessor("w")
                                                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[2].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new AndNodeProcessor([
                                                                new NumberNodeProcessor("n"),
                                                                new RepeatingWildCardNodeProcessor("p")
                                                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    )
                ]
            )
        );

        TestInternal(commands, expectedTrie);
    }

    //when_Ordering_OrTakesMinimum
    [Fact]
    public void when_Ordering_OrTakesMinimum()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "~[#n|#nn]:i" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "~[#n|**p]:i" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "~[*w|[one|two]:enter]:i" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "~[#n|#nn|*w]:i" }
        ];

        _compiler.Compile(command: commands[0]).Returns(returnThis:
            [
                new OrNodeProcessor("i", [new NumberNodeProcessor("n"), new NumberNodeProcessor("nn")]),
                new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
            ]);
        _compiler.Compile(command: commands[1]).Returns(returnThis:
            [
                new OrNodeProcessor("i",
                    [
                        new NumberNodeProcessor("n"),
                        new RepeatingWildCardNodeProcessor("p")
                    ]),
                new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
            ]);
        _compiler.Compile(command: commands[2]).Returns(returnThis:
            [
                new OrNodeProcessor("i",
                    [
                        new WildCardNodeProcessor("w"),
                        new OneOfNodeProcessor("enter",
                            [
                                new WordToken("one"),
                                new WordToken("two")
                            ])
                    ]),
                new CommandSuccessNodeProcessor(CommandId: commands[2].Id)
            ]);
        _compiler.Compile(command: commands[3]).Returns(returnThis:
            [
                new OrNodeProcessor("i",
                    [
                        new NumberNodeProcessor("n"),
                        new NumberNodeProcessor("nn"),
                        new WildCardNodeProcessor("w")
                    ]),
                new CommandSuccessNodeProcessor(CommandId: commands[3].Id)
            ]);

        var expectedTrie = new Trie
        (
            root: new(tagRequirements: [new(Tags: [])], nodeProcessor: new EmptyNodeProcessor(), children:
                [
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OrNodeProcessor("i",
                            [new NumberNodeProcessor("n"), new NumberNodeProcessor("nn")]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[0].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OrNodeProcessor("i",
                            [
                                new NumberNodeProcessor("n"),
                                new RepeatingWildCardNodeProcessor("p")
                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[1].Id),
                                children: [])
                        ]
                    ),
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OrNodeProcessor("i",
                            [
                                new NumberNodeProcessor("n"),
                                new NumberNodeProcessor("nn"),
                                new WildCardNodeProcessor("w")
                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[3].Id),
                                children: [])
                        ]
                    ),
                    
                    new(tagRequirements: [new(Tags: [])],
                        nodeProcessor: new OrNodeProcessor("i",
                            [
                                new WildCardNodeProcessor("w"),
                                new OneOfNodeProcessor("enter",
                                    [
                                        new WordToken("one"),
                                        new WordToken("two")
                                    ])
                            ]),
                        children:
                        [
                            new(tagRequirements: [new(Tags: [])],
                                nodeProcessor: new CommandSuccessNodeProcessor(CommandId: commands[2].Id),
                                children: [])
                        ]
                    ),
                ]
            )
        );

        TestInternal(commands, expectedTrie);
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