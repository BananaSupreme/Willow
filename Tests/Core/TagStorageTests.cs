using Tests.Helpers;

using Willow.Environment;
using Willow.Environment.Abstractions;
using Willow.Environment.Models;

using Xunit.Abstractions;

namespace Tests.Core;

public sealed class TagStorageTests : IDisposable
{
    private readonly IServiceProvider _provider;
    private readonly IActiveWindowTagStorage _sut;

    public TagStorageTests(ITestOutputHelper outputHelper)
    {
        var services = new ServiceCollection();
        services.AddSettings();
        services.AddTestLogger(outputHelper);
        services.AddSingleton<IActiveWindowTagStorage, ActiveWindowTagStorage>();
        _provider = services.BuildServiceProvider();
        _sut = _provider.GetRequiredService<IActiveWindowTagStorage>();
    }

    [Fact]
    public void When_AddingEmptyTagDictionary_NoFailure()
    {
        _sut.Invoking(static x => x.Add(new Dictionary<string, Tag[]>())).Should().NotThrow();
    }

    [Fact]
    public void When_AddingTags_TagCanBeFound()
    {
        Tag[] value = [new Tag("tag")];
        Dictionary<string, Tag[]> tags = new() { { "key", value } };
        _sut.Add(tags);
        var result = _sut.GetByProcessName("key");
        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void When_AddingDuplicateKeys_TagCollectionsAreMerged()
    {
        Tag[] value = [new Tag("tag")];
        Tag[] value2 = [new Tag("tag2")];
        Dictionary<string, Tag[]> tags = new() { { "key", value } };
        Dictionary<string, Tag[]> tags2 = new() { { "key", value2 } };
        _sut.Add(tags);
        _sut.Add(tags2);
        var result = _sut.GetByProcessName("key");
        result.Should().BeEquivalentTo(value.Union(value2));
    }

    [Fact]
    public void When_AddingDuplicateTags_OnlyOneCopyIsRetained()
    {
        Tag[] value = [new Tag("tag")];
        Dictionary<string, Tag[]> tags = new() { { "key", value } };
        _sut.Add(tags);
        _sut.Add(tags);
        var result = _sut.GetByProcessName("key");
        result.Should().BeEquivalentTo(value);
    }

    [Fact]
    public void When_RemovingNonExistentKey_FailSilently()
    {
        Tag[] value = [new Tag("tag")];
        Tag[] value2 = [new Tag("tag2")];
        Dictionary<string, Tag[]> tags = new() { { "key", value } };
        Dictionary<string, Tag[]> tags2 = new() { { "key2", value2 } };
        _sut.Add(tags);
        _sut.Invoking(x => x.Remove(tags2)).Should().NotThrow();
    }

    [Fact]
    public void When_RemovingFromEmptyDictionary_NoFailure()
    {
        Tag[] value = [new Tag("tag")];
        Dictionary<string, Tag[]> tags = new() { { "key", value } };
        _sut.Invoking(x => x.Remove(tags)).Should().NotThrow();
    }

    [Fact]
    public void When_RemovingOneKey_OthersStillAccessible()
    {
        Tag[] value = [new Tag("tag")];
        Tag[] value2 = [new Tag("tag2")];
        Dictionary<string, Tag[]> tags = new() { { "key", value } };
        Dictionary<string, Tag[]> tags2 = new() { { "key2", value2 } };
        _sut.Add(tags);
        _sut.Add(tags2);
        _sut.Remove(tags2);
        _sut.GetByProcessName("key").Should().BeEquivalentTo(value);
    }

    [Fact]
    public void When_RemovingTags_NoLongerAccessible()
    {
        Tag[] value = [new Tag("tag"), new Tag("tag2")];
        Tag[] value2 = [new Tag("tag2")];
        Dictionary<string, Tag[]> tags = new() { { "key", value } };
        Dictionary<string, Tag[]> tags2 = new() { { "key", value2 } };
        _sut.Add(tags);
        _sut.Remove(tags2);
        _sut.GetByProcessName("key").Should().BeEquivalentTo([new Tag("tag")]);
    }

    [Fact]
    public void When_RemovingAllTags_KeyNoLongerPresent()
    {
        Tag[] value = [new Tag("tag"), new Tag("tag2")];
        Dictionary<string, Tag[]> tags = new() { { "key", value } };
        _sut.Add(tags);
        _sut.Remove(tags);
        _sut.GetByProcessName("key").Should().BeEmpty();
    }

    public void Dispose()
    {
        (_provider as IDisposable)?.Dispose();
    }
}
