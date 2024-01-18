namespace Tests.Core.Settings;

public sealed partial class SettingsManagementTests
{
    [Fact]
    public void When_RequestingSettingsForTheFirstTime_RegisteredWithDefaultValues_RecordStruct()
    {
        When_RequestingSettingsForTheFirstTime_RegisteredWithDefaultValues<TestSettingsRecordStruct>();
    }

    [Fact]
    public async Task When_RequestingSettingsWithExistingFile_UseExistingSettings_RecordStruct()
    {
        await When_RequestingSettingsWithExistingFile_UseExistingSettings<TestSettingsRecordStruct>();
    }

    [Fact]
    public void When_UpdateCalled_ValueShouldBeUpdated_RecordStruct()
    {
        When_UpdateCalled_ValueShouldBeUpdated<TestSettingsRecordStruct>();
    }

    [Fact]
    public async Task When_UpdateCalled_FileUpdated_RecordStruct()
    {
        await When_UpdateCalled_FileUpdated<TestSettingsRecordStruct>();
    }

    [Fact]
    public void When_UpdateCalled_SettingsChangedEventShouldBeTriggered_RecordStruct()
    {
        When_UpdateCalled_SettingsChangedEventShouldBeTriggered<TestSettingsRecordStruct>();
    }

    [Fact]
    public void When_UpdateCalledWithSameValue_SettingsChangedEventShouldNotBeTriggered_RecordStruct()
    {
        When_UpdateCalledWithSameValue_SettingsChangedEventShouldNotBeTriggered<TestSettingsRecordStruct>();
    }

    [Fact]
    public void When_FileUpdateAttempt_IOException_RecordStruct()
    {
        When_FileUpdateAttempt_IOException<TestSettingsRecordStruct>();
    }

    [Fact]
    public async Task When_CorruptedFileInRegistration_DefaultValuesShouldBeUsedAndFileCorrected_RecordStruct()
    {
        await When_CorruptedFileInRegistration_DefaultValuesShouldBeUsedAndFileCorrected<TestSettingsRecordStruct>();
    }

    private readonly record struct TestSettingsRecordStruct(Guid Id) : ITestSettings
    {
        public TestSettingsRecordStruct() : this(Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"))
        {
        }
    }
}
