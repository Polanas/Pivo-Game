using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Game;

static class Collision
{

    public static bool Point(Vector2 point, Vector2 transformPosition, Vector2 transformSize)
    {
        float left = transformPosition.X - transformSize.X / 2;
        float right = transformPosition.X + transformSize.X / 2;
        float top = transformPosition.Y - transformSize.Y / 2;
        float bottom = transformPosition.Y + transformSize.Y / 2;

       return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
    }

    public static bool Rectangle(Vector2 position1, Vector2 size1, Vector2 position2, Vector2 size2)
    {
        if (Math.Abs(position1.X - position2.X) > size1.X / 2 + size2.X / 2) return false;
        if (Math.Abs(position1.Y - position2.Y) > size1.Y / 2 + size2.Y / 2) return false;

        return true;
    }

    public static bool Point(Vector2 point, Transform transform) =>
        point.X >= transform.Left && point.X <= transform.Right && point.Y >= transform.Top && point.Y <= transform.Bottom;

    public static bool Rectangle(Transform t1, Transform t2)
    {
        if (Math.Abs(t1.position.X - t2.position.X) > t1.size.X / 2 + t2.size.X / 2) return false;
        if (Math.Abs(t1.position.Y - t2.position.Y) > t1.size.Y / 2 + t2.size.Y / 2) return false;

        return true;
    }

    public static bool RectangleInside(Transform t1, Transform t2) =>
        Point(new Vector2(t1.Right, t1.Top), t2) && Point(new Vector2(t1.Left, t1.Bottom), t2);
}