﻿namespace Willow.Speech.ScriptingInterface.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string tag)
    {
        Description = tag;
    }
}