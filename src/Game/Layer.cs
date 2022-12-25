using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Game;

class Layer
{

    public static Layer Front { get; private set; }

    public static Layer Back { get; private set; }

    public static Layer Middle { get; private set; }

    public static Layer MiddlePixelized { get; private set; }

    public static Layer BackPixelized { get; private set; }

    public static Layer UI { get; private set; }

    public Texture Texture => _screenTexture;


    public readonly bool pixelated;

    public readonly string name;

    public readonly bool isUI;

    public float cameraPosModifier;

    public int depth;

    public float angle;

    private Texture _screenTexture;

    public Layer(string name, Vector2i size, bool pixelated, int depth = 0, float cameraPosModifier = 1, bool isUI = false)
    {
        this.pixelated = pixelated;
        this.depth = depth;
        this.cameraPosModifier = cameraPosModifier;
        this.name = name;
        this.isUI = isUI;

        int texture = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);

        GL.TexImage2D(
               TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                size.X,
                size.Y,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                (IntPtr)0
            );

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TextureParameter(texture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameter(texture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        _screenTexture = new Texture(texture, size.X, size.Y, "layer");
    }

    public static void InitLayers(Layer back, Layer middle, Layer backPixelized, Layer middlePixelized, Layer UILayer, Layer frontLayer)
    {
        Middle = middle;
        Back = back;
        BackPixelized = backPixelized;
        MiddlePixelized = middlePixelized;
        UI = UILayer;
        Front = frontLayer;
    }
}