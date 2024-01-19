using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Tests.Helpers;

using Willow.Core.Environment.Models;
using Willow.Speech.Tokenization.Tokens;
using Willow.Speech.VoiceCommandCompilation;
using Willow.Speech.VoiceCommandCompilation.Abstractions;
using Willow.Speech.VoiceCommandCompilation.Models;
using Willow.Speech.VoiceCommandParsing;
using Willow.Speech.VoiceCommandParsing.NodeProcessors;

using Xunit.Abstractions;

namespace Tests.Speech.CommandProcessing;

public sealed class TrieConstructionTests : IDisposable
{
    private readonly IVoiceCommandCompiler _compiler;
    private readonly Fixture _fixture;
    private readonly ServiceProvider _provider;
    private readonly ITrieFactory _sut;

    public TrieConstructionTests(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();

        _fixture.Register(static () => new Dictionary<string, object>());
        _fixture.Register(static () => new TagRequirement[] { new([]) });

        _compiler = Substitute.For<IVoiceCommandCompiler>();
        var services = new ServiceCollection();
        services.AddTestLogger(testOutputHelper);
        services.AddSingleton<ITrieFactory, TrieFactory>();
        services.AddSingleton<IVoiceCommandCompiler>(implementationFactory: _ => _compiler);
        _provider = services.BuildServiceProvider();
        _sut = _provider.GetRequiredService<ITrieFactory>();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    [Fact]
    public void When_InsertingParsersWithFullOverlap_SecondLongerChainIsAppended()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase #amount" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase #amount and" }
        ];

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new NumberNodeProcessor(CaptureName: "amount"),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new NumberNodeProcessor(CaptureName: "amount"),
                              new WordNodeProcessor(Value: new WordToken(Value: "and")),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new WordNodeProcessor(
                                                                    Value: new WordToken(Value: "increase")),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new NumberNodeProcessor(
                                                                            CaptureName: new string(value: "amount")),
                                                                        children:
                                                                        [
                                                                            new Node(
                                                                                tagRequirements
                                                                                : [new TagRequirement(Tags: [])],
                                                                                nodeProcessor:
                                                                                new WordNodeProcessor(
                                                                                    Value: new WordToken(Value: "and")),
                                                                                children:
                                                                                [
                                                                                    new Node(
                                                                                        tagRequirements
                                                                                        : [new TagRequirement(Tags: [])],
                                                                                        nodeProcessor:
                                                                                        new CommandSuccessNodeProcessor(
                                                                                            CommandId: commands[1].Id),
                                                                                        children: [])
                                                                                ]),
                                                                            new Node(
                                                                                tagRequirements:
                                                                                [
                                                                                    new TagRequirement(Tags: [])
                                                                                ],
                                                                                nodeProcessor:
                                                                                new CommandSuccessNodeProcessor(
                                                                                    CommandId: commands[0].Id),
                                                                                children: [])
                                                                        ])
                                                                ])
                                                   ]));

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_InsertingParsersWithPartialOverlap_BranchIsCreatedAtSplitPoint()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase for #amount" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase and #amount" }
        ];

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new WordNodeProcessor(Value: new WordToken(Value: "and")),
                              new NumberNodeProcessor(CaptureName: "amount"),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new WordNodeProcessor(Value: new WordToken(Value: "for")),
                              new NumberNodeProcessor(CaptureName: "amount"),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);
        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new WordNodeProcessor(
                                                                    Value: new WordToken(Value: "increase")),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new WordNodeProcessor(
                                                                            Value: new WordToken(Value: "and")),
                                                                        children:
                                                                        [
                                                                            new Node(
                                                                                tagRequirements
                                                                                : [new TagRequirement(Tags: [])],
                                                                                nodeProcessor:
                                                                                new NumberNodeProcessor(
                                                                                    CaptureName:
                                                                                    new string(value: "amount")),
                                                                                children:
                                                                                [
                                                                                    new Node(
                                                                                        tagRequirements
                                                                                        : [new TagRequirement(Tags: [])],
                                                                                        nodeProcessor:
                                                                                        new CommandSuccessNodeProcessor(
                                                                                            CommandId: commands[0].Id),
                                                                                        children: [])
                                                                                ])
                                                                        ]),
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new WordNodeProcessor(
                                                                            Value: new WordToken(Value: "for")),
                                                                        children:
                                                                        [
                                                                            new Node(
                                                                                tagRequirements
                                                                                : [new TagRequirement(Tags: [])],
                                                                                nodeProcessor: new NumberNodeProcessor(
                                                                                    CaptureName:
                                                                                    new string(value: "amount")),
                                                                                children:
                                                                                [
                                                                                    new Node(
                                                                                        tagRequirements:
                                                                                        [
                                                                                            new TagRequirement(Tags: [])
                                                                                        ],
                                                                                        nodeProcessor:
                                                                                        new CommandSuccessNodeProcessor(
                                                                                            CommandId: commands[1].Id),
                                                                                        children: [])
                                                                                ])
                                                                        ])
                                                                ])
                                                   ]));

        TestInternal(commands, expectedTrie);
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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter",
                                                     [
                                                         new WordToken(Value: "five"),
                                                         new WordToken(Value: "six"),
                                                         new WordToken(Value: "seven")
                                                     ]),
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new OptionalNodeProcessor("match",
                                                        new NumberNodeProcessor(
                                                            CaptureName: new string(value: "amount"))),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter",
                                                     [
                                                         new WordToken(Value: "one"),
                                                         new WordToken(Value: "two"),
                                                         new WordToken(Value: "three")
                                                     ]),
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new OptionalNodeProcessor("match",
                                                        new NumberNodeProcessor(
                                                            CaptureName: new string(value: "amount"))),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OneOfNodeProcessor(
                                                                    "enter",
                                                                    [
                                                                        new WordToken(Value: "five"),
                                                                        new WordToken(Value: "six"),
                                                                        new WordToken(Value: "seven")
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new WordNodeProcessor(
                                                                            Value: new WordToken(Value: "increase")),
                                                                        children:
                                                                        [
                                                                            new Node(
                                                                                tagRequirements
                                                                                : [new TagRequirement(Tags: [])],
                                                                                nodeProcessor:
                                                                                new OptionalNodeProcessor("match",
                                                                                    new NumberNodeProcessor(
                                                                                        CaptureName:
                                                                                        new string(value: "amount"))),
                                                                                children
                                                                                :
                                                                                [
                                                                                    new Node(
                                                                                        tagRequirements
                                                                                        : [new TagRequirement(Tags: [])],
                                                                                        nodeProcessor:
                                                                                        new CommandSuccessNodeProcessor(
                                                                                            CommandId: commands[0].Id),
                                                                                        children: [])
                                                                                ])
                                                                        ])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OneOfNodeProcessor(
                                                                    "enter",
                                                                    [
                                                                        new WordToken(Value: "one"),
                                                                        new WordToken(Value: "two"),
                                                                        new WordToken(Value: "three")
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new WordNodeProcessor(
                                                                            Value: new WordToken(Value: "increase")),
                                                                        children:
                                                                        [
                                                                            new Node(
                                                                                tagRequirements
                                                                                : [new TagRequirement(Tags: [])],
                                                                                nodeProcessor:
                                                                                new OptionalNodeProcessor(
                                                                                    "match",
                                                                                    new NumberNodeProcessor(
                                                                                        CaptureName:
                                                                                        new string(value: "amount"))),
                                                                                children:
                                                                                [
                                                                                    new Node(
                                                                                        tagRequirements:
                                                                                        [
                                                                                            new TagRequirement(Tags: [])
                                                                                        ],
                                                                                        nodeProcessor:
                                                                                        new CommandSuccessNodeProcessor(
                                                                                            CommandId: commands[1].Id),
                                                                                        children: [])
                                                                                ])
                                                                        ])
                                                                ])
                                                   ]));

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_InsertingDuplicateParsersWithDifferentRequirements_MergedNodeWithCombinedRequirementsIsCreated()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "increase ?[#amount]:hit",
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "hello")])]
            },
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "increase ?[#amount]:hit",
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "world")])]
            }
        ];

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new OptionalNodeProcessor("hit",
                                                        new NumberNodeProcessor(
                                                            CaptureName: new string(value: "amount"))),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new OptionalNodeProcessor("hit",
                                                        new NumberNodeProcessor(
                                                            CaptureName: new string(value: "amount"))),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(
                                        tagRequirements
                                        :
                                        [
                                            new TagRequirement(Tags: [new Tag(Name: "hello")]),
                                            new TagRequirement(Tags: [new Tag(Name: "world")])
                                        ],
                                        nodeProcessor: new EmptyNodeProcessor(),
                                        children:
                                        [
                                            new Node(
                                                tagRequirements
                                                :
                                                [
                                                    new TagRequirement(Tags: [new Tag(Name: "hello")]),
                                                    new TagRequirement(Tags: [new Tag(Name: "world")])
                                                ],
                                                nodeProcessor:
                                                new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                                                children:
                                                [
                                                    new Node(
                                                        tagRequirements
                                                        :
                                                        [
                                                            new TagRequirement(Tags: [new Tag(Name: "hello")]),
                                                            new TagRequirement(Tags: [new Tag(Name: "world")])
                                                        ],
                                                        nodeProcessor:
                                                        new OptionalNodeProcessor(
                                                            "hit",
                                                            new NumberNodeProcessor(
                                                                CaptureName: new string(value: "amount"))),
                                                        children:
                                                        [
                                                            new Node(
                                                                tagRequirements
                                                                : [new TagRequirement(Tags: [new Tag(Name: "hello")])],
                                                                nodeProcessor:
                                                                new CommandSuccessNodeProcessor(
                                                                    CommandId: commands[0].Id),
                                                                children: []),
                                                            new Node(
                                                                tagRequirements:
                                                                [
                                                                    new TagRequirement(Tags: [new Tag(Name: "world")])
                                                                ],
                                                                nodeProcessor: new CommandSuccessNodeProcessor(
                                                                    CommandId: commands[1].Id),
                                                                children: [])
                                                        ])
                                                ])
                                        ]));

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_InsertingDuplicateRequirements_RequirementsAppearOnce()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "increase ?[#amount]:catch",
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "hello")])]
            },
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "increase ?[#amount]:catch",
                TagRequirements = [new TagRequirement(Tags: [new Tag(Name: "hello")])]
            }
        ];

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new OptionalNodeProcessor("catch",
                                                        new NumberNodeProcessor(
                                                            CaptureName: new string(value: "amount"))),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new OptionalNodeProcessor("catch",
                                                        new NumberNodeProcessor(
                                                            CaptureName: new string(value: "amount"))),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [new Tag(Name: "hello")])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(
                                                           tagRequirements
                                                           : [new TagRequirement(Tags: [new Tag(Name: "hello")])],
                                                           nodeProcessor:
                                                           new WordNodeProcessor(
                                                               Value: new WordToken(Value: "increase")),
                                                           children:
                                                           [
                                                               new Node(
                                                                   tagRequirements
                                                                   :
                                                                   [
                                                                       new TagRequirement(Tags: [new Tag(Name: "hello")])
                                                                   ],
                                                                   nodeProcessor:
                                                                   new OptionalNodeProcessor(
                                                                       "catch",
                                                                       new NumberNodeProcessor(
                                                                           CaptureName: new string(value: "amount"))),
                                                                   children:
                                                                   [
                                                                       new Node(tagRequirements:
                                                                           [
                                                                               new TagRequirement(
                                                                                   Tags: [new Tag(Name: "hello")])
                                                                           ],
                                                                           nodeProcessor:
                                                                           new CommandSuccessNodeProcessor(
                                                                               CommandId: commands[0].Id),
                                                                           children: []),
                                                                       new Node(tagRequirements:
                                                                           [
                                                                               new TagRequirement(
                                                                                   Tags: [new Tag(Name: "hello")])
                                                                           ],
                                                                           nodeProcessor:
                                                                           new CommandSuccessNodeProcessor(
                                                                               CommandId: commands[1].Id),
                                                                           children: [])
                                                                   ])
                                                           ])
                                                   ]));

        TestInternal(commands, expectedTrie);
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
            _fixture.Create<PreCompiledVoiceCommand>() with
            {
                InvocationPhrase = "[one:two]:enter one and one #amount"
            }
        ];

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter",
                                                     [new WordToken(Value: "one"), new WordToken(Value: "two")]),
                              new WordNodeProcessor(Value: new WordToken(Value: "one")),
                              new WordNodeProcessor(Value: new WordToken(Value: "one")),
                              new WordNodeProcessor(Value: new WordToken(Value: "for")),
                              new NumberNodeProcessor(CaptureName: new string(value: "amount")),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter",
                                                     [new WordToken(Value: "one"), new WordToken(Value: "two")]),
                              new WordNodeProcessor(Value: new WordToken(Value: "one")),
                              new WordNodeProcessor(Value: new WordToken(Value: "and")),
                              new WordNodeProcessor(Value: new WordToken(Value: "one")),
                              new NumberNodeProcessor(CaptureName: new string(value: "amount")),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OneOfNodeProcessor(
                                                                    "enter",
                                                                    [
                                                                        new WordToken(Value: "one"),
                                                                        new WordToken(Value: "two")
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new WordNodeProcessor(
                                                                            Value: new WordToken(Value: "one")),
                                                                        children:
                                                                        [
                                                                            new Node(
                                                                                tagRequirements
                                                                                : [new TagRequirement(Tags: [])],
                                                                                nodeProcessor:
                                                                                new WordNodeProcessor(
                                                                                    Value: new WordToken(Value: "one")),
                                                                                children:
                                                                                [
                                                                                    new Node(
                                                                                        tagRequirements
                                                                                        : [new TagRequirement(Tags: [])],
                                                                                        nodeProcessor:
                                                                                        new WordNodeProcessor(
                                                                                            Value:
                                                                                            new WordToken(Value: "for")),
                                                                                        children:
                                                                                        [
                                                                                            new Node(
                                                                                                tagRequirements
                                                                                                :
                                                                                                [
                                                                                                    new TagRequirement(
                                                                                                        Tags: [])
                                                                                                ],
                                                                                                nodeProcessor:
                                                                                                new NumberNodeProcessor(
                                                                                                    CaptureName:
                                                                                                    new string(
                                                                                                        value:
                                                                                                        "amount")),
                                                                                                children:
                                                                                                [
                                                                                                    new Node(
                                                                                                        tagRequirements
                                                                                                        :
                                                                                                        [
                                                                                                            new
                                                                                                                TagRequirement(
                                                                                                                    Tags
                                                                                                                    : [])
                                                                                                        ],
                                                                                                        nodeProcessor:
                                                                                                        new
                                                                                                            CommandSuccessNodeProcessor(
                                                                                                                CommandId
                                                                                                                :
                                                                                                                commands[
                                                                                                                        0]
                                                                                                                    .Id),
                                                                                                        children: [])
                                                                                                ])
                                                                                        ])
                                                                                ]),
                                                                            new Node(
                                                                                tagRequirements
                                                                                : [new TagRequirement(Tags: [])],
                                                                                nodeProcessor:
                                                                                new WordNodeProcessor(
                                                                                    Value: new WordToken(Value: "and")),
                                                                                children:
                                                                                [
                                                                                    new Node(
                                                                                        tagRequirements
                                                                                        : [new TagRequirement(Tags: [])],
                                                                                        nodeProcessor:
                                                                                        new WordNodeProcessor(
                                                                                            Value:
                                                                                            new WordToken(Value: "one")),
                                                                                        children:
                                                                                        [
                                                                                            new Node(
                                                                                                tagRequirements
                                                                                                :
                                                                                                [
                                                                                                    new TagRequirement(
                                                                                                        Tags: [])
                                                                                                ],
                                                                                                nodeProcessor:
                                                                                                new NumberNodeProcessor(
                                                                                                    CaptureName:
                                                                                                    new string(
                                                                                                        value:
                                                                                                        "amount")),
                                                                                                children:
                                                                                                [
                                                                                                    new Node(
                                                                                                        tagRequirements:
                                                                                                        [
                                                                                                            new
                                                                                                                TagRequirement(
                                                                                                                    Tags:
                                                                                                                    [
                                                                                                                    ])
                                                                                                        ],
                                                                                                        nodeProcessor:
                                                                                                        new
                                                                                                            CommandSuccessNodeProcessor(
                                                                                                                CommandId
                                                                                                                : commands
                                                                                                                        [1]
                                                                                                                    .Id),
                                                                                                        children: [])
                                                                                                ])
                                                                                        ])
                                                                                ])
                                                                        ])
                                                                ])
                                                   ]));

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_WordBeforeOneOf()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "increase" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two]:enter" }
        ];

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(Value: new WordToken(Value: "increase")),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter", [new WordToken("one"), new WordToken("two")]),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new WordNodeProcessor(
                                                                    Value: new WordToken(Value: "increase")),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new OneOfNodeProcessor("enter",
                                                                    [new WordToken("one"), new WordToken("two")]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter", [new WordToken("one"), new WordToken("two")]),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new NumberNodeProcessor("number"),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new NumberNodeProcessor("number"),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new OneOfNodeProcessor("enter",
                                                                    [new WordToken("one"), new WordToken("two")]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(new WordToken("word")),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new NumberNodeProcessor("number"),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new WordNodeProcessor(new WordToken("word")),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new NumberNodeProcessor("number"),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(new WordToken("word")),
                              new RepeatingWildCardNodeProcessor("phrase"),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new WordNodeProcessor(new WordToken("word")),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new WordNodeProcessor(new WordToken("word")),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new RepeatingWildCardNodeProcessor("phrase"),
                                                                        children:
                                                                        [
                                                                            new Node(
                                                                                tagRequirements
                                                                                : [new TagRequirement(Tags: [])],
                                                                                nodeProcessor:
                                                                                new CommandSuccessNodeProcessor(
                                                                                    CommandId: commands[0].Id),
                                                                                children: [])
                                                                        ]),
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

        TestInternal(commands, expectedTrie);
    }

    [Fact]
    public void When_Ordering_OneOfByWordCount()
    {
        PreCompiledVoiceCommand[] commands =
        [
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two|three]:enter" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two]:enter" },
            _fixture.Create<PreCompiledVoiceCommand>() with { InvocationPhrase = "[one|two|three|four]:enter" }
        ];

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter",
                                                     [
                                                         new WordToken("one"),
                                                         new WordToken("two"),
                                                         new WordToken("three")
                                                     ]),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter", [new WordToken("one"), new WordToken("two")]),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);
        _compiler.Compile(command: commands[2])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter",
                                                     [
                                                         new WordToken("one"),
                                                         new WordToken("two"),
                                                         new WordToken("three"),
                                                         new WordToken("four")
                                                     ]),
                              new CommandSuccessNodeProcessor(CommandId: commands[2].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OneOfNodeProcessor(
                                                                    "enter",
                                                                    [new WordToken("one"), new WordToken("two")]),
                                                                children
                                                                :
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OneOfNodeProcessor(
                                                                    "enter",
                                                                    [
                                                                        new WordToken("one"),
                                                                        new WordToken("two"),
                                                                        new WordToken("three")
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new OneOfNodeProcessor("enter",
                                                                    [
                                                                        new WordToken("one"),
                                                                        new WordToken("two"),
                                                                        new WordToken("three"),
                                                                        new WordToken("four")
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[2].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new RepeatingWildCardNodeProcessor("phrase"),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new WildCardNodeProcessor("wildcard"),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new WildCardNodeProcessor("wildcard"),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new RepeatingWildCardNodeProcessor("phrase"),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new OneOfNodeProcessor("enter", [new WordToken("one"), new WordToken("two")]),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new WildCardNodeProcessor("wildcard"),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OneOfNodeProcessor(
                                                                    "enter",
                                                                    [new WordToken("one"), new WordToken("two")]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new WildCardNodeProcessor("wildcard"),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new OptionalNodeProcessor("_", new WildCardNodeProcessor("wildcard")),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new OptionalNodeProcessor("_", new NumberNodeProcessor("n")),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OptionalNodeProcessor(
                                                                    "_",
                                                                    new NumberNodeProcessor("n")),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OptionalNodeProcessor(
                                                                    "_",
                                                                    new WildCardNodeProcessor("wildcard")),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new AndNodeProcessor([new NumberNodeProcessor("n"), new NumberNodeProcessor("nn")]),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new NumberNodeProcessor("n"), new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);
        _compiler.Compile(command: commands[2])
                 .Returns(returnThis:
                          [
                              new AndNodeProcessor([
                                                       new NumberNodeProcessor("n"),
                                                       new OneOfNodeProcessor(
                                                           "enter",
                                                           [new WordToken("one"), new WordToken("two")])
                                                   ]),
                              new CommandSuccessNodeProcessor(CommandId: commands[2].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new NumberNodeProcessor("n"),
                                                                children
                                                                :
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new AndNodeProcessor(
                                                                    [
                                                                        new NumberNodeProcessor("n"),
                                                                        new NumberNodeProcessor("nn")
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
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
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[2].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new AndNodeProcessor([new NumberNodeProcessor("n"), new NumberNodeProcessor("nn")]),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new AndNodeProcessor([
                                                       new NumberNodeProcessor("n"),
                                                       new RepeatingWildCardNodeProcessor("p")
                                                   ]),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);
        _compiler.Compile(command: commands[2])
                 .Returns(returnThis:
                          [
                              new AndNodeProcessor([
                                                       new NumberNodeProcessor("n"),
                                                       new NumberNodeProcessor("nn"),
                                                       new WildCardNodeProcessor("w")
                                                   ]),
                              new CommandSuccessNodeProcessor(CommandId: commands[2].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new AndNodeProcessor(
                                                                    [
                                                                        new NumberNodeProcessor("n"),
                                                                        new NumberNodeProcessor("nn")
                                                                    ]),
                                                                children
                                                                :
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new AndNodeProcessor(
                                                                    [
                                                                        new NumberNodeProcessor("n"),
                                                                        new NumberNodeProcessor("nn"),
                                                                        new WildCardNodeProcessor("w")
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[2].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new AndNodeProcessor([
                                                                    new NumberNodeProcessor("n"),
                                                                    new RepeatingWildCardNodeProcessor("p")
                                                                ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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

        _compiler.Compile(command: commands[0])
                 .Returns(returnThis:
                          [
                              new OrNodeProcessor("i", [new NumberNodeProcessor("n"), new NumberNodeProcessor("nn")]),
                              new CommandSuccessNodeProcessor(CommandId: commands[0].Id)
                          ]);
        _compiler.Compile(command: commands[1])
                 .Returns(returnThis:
                          [
                              new OrNodeProcessor("i",
                                                  [
                                                      new NumberNodeProcessor("n"),
                                                      new RepeatingWildCardNodeProcessor("p")
                                                  ]),
                              new CommandSuccessNodeProcessor(CommandId: commands[1].Id)
                          ]);
        _compiler.Compile(command: commands[2])
                 .Returns(returnThis:
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
        _compiler.Compile(command: commands[3])
                 .Returns(returnThis:
                          [
                              new OrNodeProcessor("i",
                                                  [
                                                      new NumberNodeProcessor("n"),
                                                      new NumberNodeProcessor("nn"),
                                                      new WildCardNodeProcessor("w")
                                                  ]),
                              new CommandSuccessNodeProcessor(CommandId: commands[3].Id)
                          ]);

        var expectedTrie = new Trie(root: new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                   nodeProcessor: new EmptyNodeProcessor(),
                                                   children:
                                                   [
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OrNodeProcessor(
                                                                    "i",
                                                                    [
                                                                        new NumberNodeProcessor("n"),
                                                                        new NumberNodeProcessor("nn")
                                                                    ]),
                                                                children
                                                                :
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[0].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OrNodeProcessor(
                                                                    "i",
                                                                    [
                                                                        new NumberNodeProcessor("n"),
                                                                        new RepeatingWildCardNodeProcessor("p")
                                                                    ]),
                                                                children
                                                                :
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[1].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor:
                                                                new OrNodeProcessor(
                                                                    "i",
                                                                    [
                                                                        new NumberNodeProcessor("n"),
                                                                        new NumberNodeProcessor("nn"),
                                                                        new WildCardNodeProcessor("w")
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor:
                                                                        new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[3].Id),
                                                                        children: [])
                                                                ]),
                                                       new Node(tagRequirements: [new TagRequirement(Tags: [])],
                                                                nodeProcessor: new OrNodeProcessor("i",
                                                                    [
                                                                        new WildCardNodeProcessor("w"),
                                                                        new OneOfNodeProcessor("enter",
                                                                            [new WordToken("one"), new WordToken("two")])
                                                                    ]),
                                                                children:
                                                                [
                                                                    new Node(
                                                                        tagRequirements: [new TagRequirement(Tags: [])],
                                                                        nodeProcessor: new CommandSuccessNodeProcessor(
                                                                            CommandId: commands[2].Id),
                                                                        children: [])
                                                                ])
                                                   ]));

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
        var resultString = JsonConvert.SerializeObject(result, settings);
        var expectedTrieString = JsonConvert.SerializeObject(expectedTrie, settings);
        resultString.Should().BeEquivalentTo(expected: expectedTrieString);
    }

    private sealed class MyContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type
                        .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(selector: p => CreateProperty(p, memberSerialization))
                        .Union(second: type
                                       .GetFields(bindingAttr: BindingFlags.Public
                                                               | BindingFlags.NonPublic
                                                               | BindingFlags.Instance)
                                       .Select(selector: f => CreateProperty(f, memberSerialization)))
                        .ToList();
            props.ForEach(action: static p =>
            {
                p.Writable = true;
                p.Readable = true;
            });
            return props;
        }
    }
}
