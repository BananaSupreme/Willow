﻿using Willow.Environment.Models;
using Willow.Helpers.Extensions;

namespace Willow.Speech.ScriptingInterface.Attributes;

/// <summary>
/// Defines a group of tags that must all be active for the command to activate. That is that all the tags defined in
/// one attribute will be grouped into a singular <see cref="TagRequirement" />.<br/>
/// When multiple groups of tags are relevant, add multiple versions of this attribute.
/// </summary>
/// <remarks>
/// When no attribute exists it is assumed a singular attribute with no tags defined. That is it is relevant in all
/// environments.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class TagAttribute : Attribute, IVoiceCommandDescriptor
{
    public TagAttribute()
    {
        Tags = [];
    }

    public TagAttribute(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        Tags = [new Tag(tag)];
    }

    public TagAttribute(params string[] tags)
    {
        tags.ThrowIfAnyNull();
        Tags = tags.Select(static x => new Tag(x)).ToArray();
    }

    /// <summary>
    /// The group of tags.
    /// </summary>
    public Tag[] Tags { get; }
}
