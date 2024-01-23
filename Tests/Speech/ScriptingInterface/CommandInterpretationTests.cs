using Tests.Helpers;

using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

using Xunit.Abstractions;

namespace Tests.Speech.ScriptingInterface;

public sealed class CommandInterpretationTests : IDisposable
{
    private const string TestInvocationPhrase = "invocationPhrase";
    private const string TestTag = "TestTag";
    private const string TestTag2 = "TestTag2";
    private const string TestTag3 = "TestTag3";
    private const string Alias = "Alias";
    private const string Alias2 = "AliasTwo";
    private const string Alias3 = "AliasThree";
    private const string Description = "Description";
    private const string Name = "name";
    private readonly ServiceProvider _provider;
    private readonly IVoiceCommandInterpreter _sut;

    public CommandInterpretationTests(ITestOutputHelper testOutputHelper)
    {
        var services = new ServiceCollection();
        services.AddTestLogger(testOutputHelper);
        services.AddSingleton<IVoiceCommandInterpreter, VoiceCommandInterpreter>();
        _provider = services.BuildServiceProvider();
        _sut = _provider.GetRequiredService<IVoiceCommandInterpreter>();
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    [Fact]
    public void When_ProcessingCommand_StoreCommandsInCapturedValues()
    {
        var voiceCommand = new EmptyTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.CapturedValues.Should().ContainKey(IVoiceCommand.CommandFunctionName);
        result1.CapturedValues[IVoiceCommand.CommandFunctionName].Should().BeOfType<Func<IVoiceCommand>>();
    }

    [Fact]
    public void When_VoiceCommandHasNoAliases_InterpretCommandReturnsSingleInvocationPhrase()
    {
        var voiceCommand = new EmptyTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.InvocationPhrases.Should().ContainSingle().And.Contain(TestInvocationPhrase);
    }

    [Fact]
    public void When_VoiceCommandHasMultipleAliases_InterpretCommandIncludesAllPhrases()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.InvocationPhrases.Should().BeEquivalentTo(TestInvocationPhrase, Alias, Alias2, Alias3);
    }

    [Fact]
    public void When_MarkedDictationMode_TagsContainDictationTag()
    {
        var voiceCommand = new DictationTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.TagRequirements.Should()
               .BeEquivalentTo(new TagRequirement[] { new([new Tag(nameof(ActivationMode.Dictation))]) },
                               static options => options.ComparingByValue<TagRequirement>());
    }

    [Fact]
    public void When_VoiceCommandHasNoTags_InterpretCommandAsOnlyContainingCommandModeTag()
    {
        var voiceCommand = new EmptyTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.TagRequirements.Should()
               .BeEquivalentTo(new TagRequirement[] { new([new Tag(nameof(ActivationMode.Command))]) },
                               static options => options.ComparingByValue<TagRequirement>());
    }

    [Fact]
    public void When_VoiceCommandHasEmptyTagAttribute_InterpretCommandAsOnlyContainingCommandModeTag()
    {
        var voiceCommand = new EmptyTagTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.TagRequirements.Should()
               .BeEquivalentTo(new TagRequirement[] { new([new Tag(nameof(ActivationMode.Command))]) },
                               static options => options.ComparingByValue<TagRequirement>());
    }

    [Fact]
    public void When_VoiceCommandHasMultipleTags_InterpretCommandIncludesAllTagsAndCommandMode()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.TagRequirements.Should()
               .BeEquivalentTo(new TagRequirement[]
                               {
                                   new([new Tag(nameof(ActivationMode.Command)), new Tag(TestTag)]),
                                   new([
                                           new Tag(nameof(ActivationMode.Command)),
                                           new Tag(TestTag2),
                                           new Tag(TestTag3)
                                       ])
                               },
                               static options => options.ComparingByValue<TagRequirement>());
    }

    [Fact]
    public void When_VoiceCommandHasDescription_InterpretCommandIncludesDescription()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Description.Should().Be(Description);
    }

    [Fact]
    public void When_VoiceCommandHasNoDescription_InterpretCommandHasEmptyDescription()
    {
        var voiceCommand = new EmptyTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Description.Should().BeEmpty();
    }

    [Fact]
    public void When_VoiceCommandRequiresCapturing_CapturedValuesAreIncluded()
    {
        var voiceCommand = new CapturingVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.CapturedValues.Should().ContainKey(CapturingVoiceCommand.CapturedName);
        result1.CapturedValues[CapturingVoiceCommand.CapturedName].Should().BeEquivalentTo(new[] { "1", "2" });
    }

    [Fact]
    public void When_VoiceCommandDoesntStateSupportedOs_AllIsAssumed()
    {
        var voiceCommand = new CapturingVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.SupportedOss.Should().Be(SupportedOss.All);
    }

    [Fact]
    public void When_VoiceCommandStatesSupportedOs_UseFromAttribute()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.SupportedOss.Should().Be(SupportedOss.Windows);
    }

    [Fact]
    public void When_VoiceCommandHasName_UseFromAttribute()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Name.Should().Be(Name);
    }

    [Fact]
    public void When_VoiceCommandDoesntHaveName_NameAfterCommandWithoutVoiceCommand()
    {
        var voiceCommand = new DictationTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Name.Should().Be("Dictation Test");
    }

    [Fact]
    public void When_VoiceCommandDoesntHaveNameAndDoesntHaveVoiceCommand_NameAfterCommand()
    {
        var voiceCommand = new NoEnding();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Name.Should().Be("No Ending");
    }

    [Fact]
    public void When_SingleLetterVoiceCommandName_NameDoesntFail()
    {
        var voiceCommand = new A();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Name.Should().Be("A");
    }

    [Fact]
    public void When_NoLetterVoiceCommand_NameDoesntFail()
    {
        var voiceCommand = new _();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Name.Should().Be("_");
    }

    [Fact]
    public void When_VoiceCommandWithReallyLongName_NameDoesntFail()
    {
        var voiceCommand = new ThisIsAReallyLongNameABCDAndSomethingSomethingVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Name.Should().Be("This Is A Really Long Name A B C D And Something Something");
    }

    private sealed class EmptyTestVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => TestInvocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    [Tag(TestTag)]
    [Tag(TestTag2, TestTag3)]
    [Alias(Alias, Alias2)]
    [Alias(Alias3)]
    [Name(Name)]
    [Description(Description)]
    [SupportedOss(SupportedOss.Windows)]
    private sealed class FullyAnnotatedVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => TestInvocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class CapturingVoiceCommand : IVoiceCommand
    {
        public const string CapturedName = nameof(_captured);
        private static readonly string[] _captured = ["1", "2"];
        public string InvocationPhrase => $"[_{nameof(_captured)}]:captured";

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    [ActivationMode(ActivationMode.Dictation)]
    private sealed class DictationTestVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => TestInvocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    [Tag]
    private sealed class EmptyTagTestVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => TestInvocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class NoEnding : IVoiceCommand
    {
        public string InvocationPhrase => TestInvocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class A : IVoiceCommand
    {
        public string InvocationPhrase => TestInvocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class _ : IVoiceCommand
    {
        public string InvocationPhrase => TestInvocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    // ReSharper disable once IdentifierTypo
    // ReSharper disable once InconsistentNaming
    private sealed class ThisIsAReallyLongNameABCDAndSomethingSomethingVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => TestInvocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}
