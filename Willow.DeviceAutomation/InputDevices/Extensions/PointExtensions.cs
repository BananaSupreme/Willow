using System.Drawing;
using System.Numerics;

namespace Willow.DeviceAutomation.InputDevices.Extensions;

internal static class PointExtensions
{
    public static Vector2 FromPoint(this Point point)
    {
        return new(point.X, point.Y);
    }
    
    public static Point FromVector2(this Vector2 vector)
    {
        return new((int)vector.X, (int)vector.Y);
    }
}