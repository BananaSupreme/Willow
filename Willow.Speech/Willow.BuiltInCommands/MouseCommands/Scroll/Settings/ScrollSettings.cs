namespace Willow.BuiltInCommands.MouseCommands.Scroll.Settings;

public readonly record struct ScrollSettings(int Speed)
{
    public ScrollSettings() : this(100)
    {
    }
}
