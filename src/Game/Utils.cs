using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Game;

static class Utils
{

    public static Vector2i FullScreenTextureSize { get; private set; }

    public static Vector3i DefaultFrameData { get; private set; }

    private static MyGameWindow _game;

    public static T GetSystem<T>() where T : MySystem
    {
        Type type = typeof(T);

        if (!_game.SharedData.systems.ContainsKey(type))
            throw new ArgumentException($"Oops. Looks like you need to add {type.Name} using AddExt method");

        return (T)_game.SharedData.systems[type];
    }

    public static Vector2 ToIngameSpace(Vector2 position)
    {
        Camera camera = _game.SharedData.gameData.camera;
        return (position + camera.renderingPosition - camera.offset - (Vector2)MyGameWindow.ScreenSize / MyGameWindow.FullToPixelatedRatio / 2) * camera.zoom;
    }

    public static Vector2 ToScreenSpace(Vector2 position)
    {
        Camera camera = _game.SharedData.gameData.camera;
        return position / camera.zoom - camera.renderingPosition + camera.offset + ((Vector2)MyGameWindow.ScreenSize / (MyGameWindow.FullToPixelatedRatio) / 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    public static ref Vector2 ToOpenTKVector2(ref System.Numerics.Vector2 vec2)
    {
        return ref Unsafe.As<System.Numerics.Vector2, Vector2>(ref vec2);
    }

    public static System.Numerics.Vector2 ToSystemVector2(ref Vector2 vector2)
    {
        return Unsafe.As<Vector2, System.Numerics.Vector2>(ref vector2);
    }

    public static float BoolToFloat(bool value) => value ? 1f : -1;

    public static int BoolToInt(bool value) => value ? 1 : -1;

    public static void Init(MyGameWindow game)
    {
        _game = game;

        Vector2 texSize = (Vector2)MyGameWindow.ScreenSize / MyGameWindow.FullToPixelatedRatio;
        FullScreenTextureSize = new Vector2i((int)MathF.Round(texSize.X), (int)MathF.Round(texSize.Y));

        DefaultFrameData = new Vector3i(1, 1, 0);
    }

    public static bool IsInteger<T>() where T : struct
    {
        var typeCode = Type.GetTypeCode(TypeOf<T>.Raw);

        switch (typeCode)
        {
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Byte:
            case TypeCode.UInt16:
            case TypeCode.UInt64:
            case TypeCode.SByte:
                return true;
            case TypeCode.Double:
            case TypeCode.Single:
            case TypeCode.Decimal:
                return false;
            default:
                return false;

        }
    }

    public static Vector3i GetFrameData(VirtualTexture texture, Vector2i frameSize, int frame) =>
          texture == null ? new Vector3i(1, 1, 0) : new(texture.Width / frameSize.X, texture.Height / frameSize.Y, frame);
}