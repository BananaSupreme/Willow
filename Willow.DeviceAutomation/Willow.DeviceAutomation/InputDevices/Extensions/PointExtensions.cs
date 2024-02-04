using System.Drawing;
using System.Numerics;

namespace Willow.DeviceAutomation.InputDevices.Extensions;

internal static class PointExtensions
{
    public static Vector2 FromPoint(this Point point)
    {
        return new Vector2(point.X, point.Y);
    }

    public static Point FromVector2(this Vector2 vector)
    {
        return new Point((int)vector.X, (int)vector.Y);
    }
}
