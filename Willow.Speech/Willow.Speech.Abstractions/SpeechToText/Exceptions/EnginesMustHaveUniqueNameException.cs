namespace Willow.Speech.SpeechToText.Exceptions;

public sealed class EnginesMustHaveUniqueNameException : Exception
{
    public EnginesMustHaveUniqueNameException(string nameFound) : base(
        $"Engines must have a unique name, the offending name was {nameFound}")
    {
    }
}
