using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

static class MyMath
{

    public static float Clamp(float num, float edge1, float edge2) => MathF.Min(MathF.Max(num, edge1), edge2);

    public static int GetIndex(Vector2 position, int width)
    {
        int posX = (int)position.X;
        int posY = (int)position.Y;

        return (posY * width + posX);
    }

    public static Vector2 GetPosition(int index, int width)
    {
        int x = index % width;
        int y = (index - x) / width;

        return new Vector2(x, y);
    }

    public static Vector2 Max(Vector2 a, Vector2 b) => new Vector2(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y));

    public static Vector2 Min(Vector2 a, Vector2 b) => new Vector2(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y));

    public static Vector2 GetRound(this Vector2 vector2) => new Vector2(MathF.Round(vector2.X), MathF.Round(vector2.Y));

    public static Vector2 GetActionX(this Vector2 vector2, Func<float, float> action)
    {
        vector2.X = action.Invoke(vector2.X);

        return vector2;
    }

    public static Vector2 GetActionY(this Vector2 vector2, Func<float, float> action)
    {
        vector2.Y = action.Invoke(vector2.Y);

        return vector2;
    }

    public static Vector2 GetActionXY(this Vector2 vector2, Func<float, float> action)
    {
        vector2.X = action.Invoke(vector2.X);
        vector2.Y = action.Invoke(vector2.Y);

        return vector2;
    }

    public static Vector2 GetActionsXY(this Vector2 vector2, Func<float, float> action1, Func<float, float> action2)
    {
        vector2.X = action1.Invoke(vector2.X);
        vector2.Y = action2.Invoke(vector2.Y);

        return vector2;
    }

    public static float AngleBetweenPoints(Vector2 point1, Vector2 point2) =>
        MathHelper.RadiansToDegrees(MathF.Atan2(point1.Y - point2.Y, point2.X - point1.X));


    public static Vector2 Rotate(this Vector2 vector2, float angle, Vector2 pivot)
    {
        float radians = MathHelper.DegreesToRadians(angle);
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        Vector2 translatedPont = new Vector2(vector2.X - pivot.X, vector2.Y - pivot.Y);

        return new Vector2
        {
            X = translatedPont.X * cos - translatedPont.Y * sin + pivot.X,
            Y = translatedPont.X * sin + translatedPont.Y * cos + pivot.Y,
        };
    }

    public static Matrix4 CreateTransformMatrix(Vector2 position, Vector2 size, float angle = 0)
    {
        Matrix4 result = Matrix4.Identity;

        result *= Matrix4.CreateScale(new Vector3(size.X, size.Y, 1));

        if (angle != 0)
        {
            result *= Matrix4.CreateTranslation(new Vector3(size.X * -0.5f, size.Y * -0.5f, 0));
            result *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(angle));
            result *= Matrix4.CreateTranslation(new Vector3(size.X * 0.5f, size.Y * 0.5f, 0));
        }

        result *= Matrix4.CreateTranslation(new Vector3(position.X - size.X / 2, position.Y - size.Y / 2, 0));

        return result;
    }

    public static Matrix4 CreateCameraMatrix(Vector2 position, Vector2 size)
    {
        Matrix4 result = Matrix4.Identity;
        Matrix4 ortho;

        result *= Matrix4.CreateTranslation(new Vector3(-position.X, -position.Y, 0));
        Matrix4.CreateOrthographic(size.X, size.Y, -1, 1, out ortho);
        result *= ortho;

        return result;
    }
}