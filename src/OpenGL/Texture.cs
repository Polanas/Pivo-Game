using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using StbImageWriteSharp;

namespace Game;

class Texture : ITexture
{
    public int Handle { get; private set; }

    public readonly string name;

    public int Height { get; private set; }

    public string Path { get; set; }

    public int Width { get; private set; }

    public Vector2i Size { get; private set; }

    public Texture(int handle, int width, int height, string name)
    {
        Handle = handle;
        this.name = name;

        Width = width;
        Height = height;
        Size = new Vector2i(width, height);
    }

    public static implicit operator int(Texture texture) => texture.Handle;

    public void Use() => GL.BindTexture(TextureTarget.Texture2D, Handle);

    public void Save(string path, PixelFormat pixelFormat, PixelType pixelType, StbImageWriteSharp.ColorComponents colorComponents)
    {
        int dataLength = Width * Height * 4;
        byte[] data = new byte[dataLength];
        GL.GetTextureImage(Handle, 0, pixelFormat, pixelType, dataLength, data);

        using (FileStream fs = new(path, FileMode.OpenOrCreate))
        {
            ImageWriter writer = new ImageWriter();
            writer.WritePng(data, Width, Height, colorComponents, fs);
        }
    }

    public int GetWidth() => Width;

    public int GetHeight() => Height;
}
