namespace Tests.Core.Settings;

public sealed partial class SettingsManagementTests
{
    [Fact]
    public void When_RequestingSettingsForTheFirstTime_RegisteredWithDefaultValues_Record()
    {
        When_RequestingSettingsForTheFirstTime_RegisteredWithDefaultValues<TestSettingsRecord>();
    }

    [Fact]
    public async Task When_RequestingSettingsWithExistingFile_UseExistingSettings_Record()
    {
        await When_RequestingSettingsWithExistingFile_UseExistingSettings<TestSettingsRecord>();
    }

    [Fact]
    public void When_UpdateCalled_ValueShouldBeUpdated_Record()
    {
        When_UpdateCalled_ValueShouldBeUpdated<TestSettingsRecord>();
    }

    [Fact]
    public async Task When_UpdateCalled_FileUpdated_Record()
    {
        await When_UpdateCalled_FileUpdated<TestSettingsRecord>();
    }

    [Fact]
    public void When_UpdateCalled_SettingsChangedEventShouldBeTriggered_Record()
    {
        When_UpdateCalled_SettingsChangedEventShouldBeTriggered<TestSettingsRecord>();
    }

    [Fact]
    public void When_UpdateCalledWithSameValue_SettingsChangedEventShouldNotBeTriggered_Record()
    {
        When_UpdateCalledWithSameValue_SettingsChangedEventShouldNotBeTriggered<TestSettingsRecord>();
    }

    [Fact]
    public void When_FileUpdateAttempt_IOException_Record()
    {
        When_FileUpdateAttempt_IOException<TestSettingsRecord>();
    }

    [Fact]
    public async Task When_CorruptedFileInRegistration_DefaultValuesShouldBeUsedAndFileCorrected_Record()
    {
        await When_CorruptedFileInRegistration_DefaultValuesShouldBeUsedAndFileCorrected<TestSettingsRecord>();
    }

    private sealed record TestSettingsRecord(Guid Id) : ITestSettings
    {
        public TestSettingsRecord() : this(Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"))
        {
        }
    }
}
