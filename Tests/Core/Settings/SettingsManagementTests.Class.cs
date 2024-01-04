namespace Tests.Core.Settings;

public sealed partial class SettingsManagementTests
{
    [Fact]
    public void When_RequestingSettingsForTheFirstTime_RegisteredWithDefaultValues_Class()
    {
        When_RequestingSettingsForTheFirstTime_RegisteredWithDefaultValues<TestSettings>();
    }

    [Fact]
    public async Task When_RequestingSettingsWithExistingFile_UseExistingSettings_Class()
    {
        await When_RequestingSettingsWithExistingFile_UseExistingSettings<TestSettings>();
    }

    [Fact]
    public void When_UpdateCalled_ValueShouldBeUpdated_Class()
    {
        When_UpdateCalled_ValueShouldBeUpdated<TestSettings>();
    }

    [Fact]
    public async Task When_UpdateCalled_FileUpdated_Class()
    {
        await When_UpdateCalled_FileUpdated<TestSettings>();
    }

    [Fact]
    public void When_UpdateCalled_SettingsChangedEventShouldBeTriggered_Class()
    {
        When_UpdateCalled_SettingsChangedEventShouldBeTriggered<TestSettings>();
    }

    [Fact]
    public void When_UpdateCalledWithSameValue_SettingsChangedEventShouldNotBeTriggered_Class()
    {
        When_UpdateCalledWithSameValue_SettingsChangedEventShouldNotBeTriggered<TestSettings>();
    }

    [Fact]
    public void When_FileUpdateAttempt_IOException_Class()
    {
        When_FileUpdateAttempt_IOException<TestSettings>();
    }

    [Fact]
    public async Task When_CorruptedFileInRegistration_DefaultValuesShouldBeUsedAndFileCorrected_Class()
    {
        await When_CorruptedFileInRegistration_DefaultValuesShouldBeUsedAndFileCorrected<TestSettings>();
    }

    private class TestSettings : ITestSettings
    {
        public Guid Id { get; init; } = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
    }
}