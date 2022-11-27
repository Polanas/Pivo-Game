using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SharpFont;

namespace Game;

readonly record struct Character(int textureID, Vector2 size, Vector2 bearing, int advance);

class Font
{
    public Dictionary<char, Character> Characters => _characters;

    public readonly uint size;

    private Dictionary<char, Character> _characters = new();

    public Font(Library library, string fontPath, uint size)
    {
        this.size = size;

        Face face = new(library, fontPath);

        face.SetPixelSizes(0, size);

        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

        for (uint c = 0; c < 128; c++)
        {
            face.LoadChar(c, LoadFlags.Render, LoadTarget.Normal);

            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.CompressedRed,
                face.Glyph.Bitmap.Width,
                face.Glyph.Bitmap.Rows,
                0,
                PixelFormat.Red,
                PixelType.UnsignedByte,
                face.Glyph.Bitmap.Buffer
                );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TextureParameter(texture, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TextureParameter(texture, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            Character character = new Character(
                texture,
                new Vector2i(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                new Vector2i(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                face.Glyph.Advance.X.Value
                );

            _characters[(char)c] = character;
        }

        face.Dispose();
    }
}
