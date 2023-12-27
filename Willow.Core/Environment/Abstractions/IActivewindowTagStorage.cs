using Willow.Core.Environment.Models;

namespace Willow.Core.Environment.Abstractions;

public interface IActiveWindowTagStorage
{
    public Tag[] GetByProcessName(string processName);
    public void Set(IDictionary<string, Tag[]> tags);
}