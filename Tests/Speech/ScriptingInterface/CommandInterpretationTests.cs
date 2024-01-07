using Willow.Core.Environment.Enums;
using Willow.Core.Environment.Models;
using Willow.Speech.ScriptingInterface;
using Willow.Speech.ScriptingInterface.Abstractions;
using Willow.Speech.ScriptingInterface.Attributes;
using Willow.Speech.ScriptingInterface.Models;

namespace Tests.Speech.ScriptingInterface;

public sealed class CommandInterpretationTests : IDisposable
{
    private readonly IVoiceCommandInterpreter _sut;
    private readonly ServiceProvider _provider;

    public CommandInterpretationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IVoiceCommandInterpreter, VoiceCommandInterpreter>();
        _provider = services.BuildServiceProvider();
        _sut = _provider.GetRequiredService<IVoiceCommandInterpreter>();
    }


    [Fact]
    public void When_ProcessingCommand_StoreCommandsInCapturedValues()
    {
        var voiceCommand = new EmptyTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.CapturedValues.Should().ContainKey(IVoiceCommand.CommandFunctionName);
        result1.CapturedValues[IVoiceCommand.CommandFunctionName]
               .Should()
               .BeOfType<Func<IVoiceCommand>>();
    }

    [Fact]
    public void When_VoiceCommandHasNoAliases_InterpretCommandReturnsSingleInvocationPhrase()
    {
        var voiceCommand = new EmptyTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.InvocationPhrases.Should().ContainSingle()
               .And.Contain(_invocationPhrase);
    }

    [Fact]
    public void When_VoiceCommandHasMultipleAliases_InterpretCommandIncludesAllPhrases()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.InvocationPhrases.Should().BeEquivalentTo(_invocationPhrase, _alias, _alias2, _alias3);
    }

    [Fact]
    public void When_MarkedDictationMode_TagsContainDictationTag()
    {
        var voiceCommand = new DictationTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.TagRequirements.Should().BeEquivalentTo(
            new TagRequirement[] { new([new(nameof(ActivationMode.Dictation))]) },
            options => options.ComparingByValue<TagRequirement>());
    }

    [Fact]
    public void When_VoiceCommandHasNoTags_InterpretCommandAsOnlyContainingCommandModeTag()
    {
        var voiceCommand = new EmptyTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.TagRequirements.Should().BeEquivalentTo(
            new TagRequirement[] { new([new(nameof(ActivationMode.Command))]) },
            options => options.ComparingByValue<TagRequirement>());
    }

    [Fact]
    public void When_VoiceCommandHasEmptyTagAttribute_InterpretCommandAsOnlyContainingCommandModeTag()
    {
        var voiceCommand = new EmptyTagTestVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.TagRequirements.Should().BeEquivalentTo(
            new TagRequirement[] { new([new(nameof(ActivationMode.Command))]) },
            options => options.ComparingByValue<TagRequirement>());
    }

    [Fact]
    public void When_VoiceCommandHasMultipleTags_InterpretCommandIncludesAllTagsAndCommandMode()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.TagRequirements.Should()
               .BeEquivalentTo(
                   new TagRequirement[]
                   {
                       new([new(nameof(ActivationMode.Command)), new(_testTag)]),
                       new([new(nameof(ActivationMode.Command)), new(_testTag2), new(_testTag3)])
                   },
                   options => options.ComparingByValue<TagRequirement>());
    }

    [Fact]
    public void When_VoiceCommandHasDescription_InterpretCommandIncludesDescription()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Description.Should().Be(_description);
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

        result1.SupportedOperatingSystems.Should().Be(SupportedOperatingSystems.All);
    }
    
    [Fact]
    public void When_VoiceCommandStatesSupportedOs_UseFromAttribute()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.SupportedOperatingSystems.Should().Be(SupportedOperatingSystems.Windows);
    }
    
    [Fact]
    public void When_VoiceCommandHasName_UseFromAttribute()
    {
        var voiceCommand = new FullyAnnotatedVoiceCommand();

        var result1 = _sut.InterpretCommand(voiceCommand);

        result1.Name.Should().Be(_name);
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

    private class EmptyTestVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => _invocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    private const string _invocationPhrase = "invocationPhrase";
    private const string _testTag = "TestTag";
    private const string _testTag2 = "TestTag2";
    private const string _testTag3 = "TestTag3";
    private const string _alias = "Alias";
    private const string _alias2 = "AliasTwo";
    private const string _alias3 = "AliasThree";
    private const string _description = "Description";
    private const string _name = "name";

    [Tag(_testTag)]
    [Tag(_testTag2, _testTag3)]
    [Alias(_alias, _alias2)]
    [Alias(_alias3)]
    [Name(_name)]
    [Description(_description)]
    [SupportedOperatingSystems(SupportedOperatingSystems.Windows)]
    private class FullyAnnotatedVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => _invocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    private class CapturingVoiceCommand : IVoiceCommand
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
    private class DictationTestVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => _invocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    [Tag]
    private class EmptyTagTestVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => _invocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }
    
    private class NoEnding : IVoiceCommand
    {
        public string InvocationPhrase => _invocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }
    
    private class A : IVoiceCommand
    {
        public string InvocationPhrase => _invocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }
    
    private class _ : IVoiceCommand
    {
        public string InvocationPhrase => _invocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }
    
    private class ThisIsAReallyLongNameABCDAndSomethingSomethingVoiceCommand : IVoiceCommand
    {
        public string InvocationPhrase => _invocationPhrase;

        public Task ExecuteAsync(VoiceCommandContext context)
        {
            throw new NotImplementedException();
        }
    }

    public void Dispose()
    {
        _provider.Dispose();
    }
}