namespace Willow.Speech.VoiceCommandCompilation.Exceptions;

internal sealed class NodeMissingNodeProcessorException : InvalidOperationException
{
    public NodeMissingNodeProcessorException() 
        : base("A node attempted to build without having the processor assigned to it, this should not happen.")
    {
    }

    public NodeMissingNodeProcessorException(string? message) : base(message)
    {
    }

    public NodeMissingNodeProcessorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}