using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

namespace Game;

public enum SourceType
{
    Frag,
    Vert,
    Geom,
}

record ShaderInfo(Shader shader, string vertPath, string fragPath, string geomPath);

static class Content
{

    public static Dictionary<string, ShaderInfo> ShaderInfos => _shaderInfos;

    private static Dictionary<string, Texture> _textures = new();

    private static Dictionary<string, ShaderInfo> _shaderInfos = new();

    private static Dictionary<string, VirtualTexture> _virtualTextures = new();

    private static int _emptyTexturesCounter;

    public static Shader ReloadShader(Shader shader)
    {
        var shaderInfo = _shaderInfos[shader.name];

        string vertSource = File.ReadAllText(shaderInfo.vertPath);
        string fragSource = File.ReadAllText(shaderInfo.fragPath);
        string geometrySource = null;

        if (shaderInfo.geomPath != null)
            geometrySource = File.ReadAllText(shaderInfo.geomPath);

        Shader.Reload(shaderInfo.shader, vertSource, fragSource, geometrySource, out int handle, out var uLocations);
        shaderInfo.shader.ResetData(handle, uLocations);

        return shaderInfo.shader;
    }

    public static Shader RequestShader(string name, string vertPath, string fragPath)
    {
        if (_shaderInfos.ContainsKey(name))
            return _shaderInfos[name].shader;

        Shader shader = LoadShader(name, vertPath, fragPath, null);
        return shader;
    }

    public static Shader LoadShader(string name, string vertPath, string fragPath, string geomPath = null)
    {
        string vertSource = File.ReadAllText(vertPath);
        string fragSource = File.ReadAllText(fragPath);
        string geomSource = null;

        if (geomPath != null)
            geomSource = File.ReadAllText(geomPath);

        Shader shader = Shader.Load(vertSource, fragSource, name, geomSource);
        _shaderInfos.Add(name, new ShaderInfo(shader, vertPath, fragPath, geomPath));

        return shader;
    }

    public static void DeleteShader(Shader shader)
    {
        GL.DeleteProgram(shader.Handle);
        _shaderInfos.Remove(shader.name);
    }

    public static void DeleteTexture(Texture texture)
    {
        GL.DeleteTexture(texture);
        _textures.Remove(texture.name);
    }

    public static void DeleteVirtualTexture(string name) =>
        _virtualTextures.Remove(name);

    public static void DeleteVirtualTexture(VirtualTexture virtualTexture) =>
        _virtualTextures.Remove(virtualTexture.Name);

    public static void DeleteTexture(string name)
    {
        var texture = _textures[name];

        GL.DeleteTexture(texture);
        _textures.Remove(texture.name);
    }

    public static void AddVirtualTexture(VirtualTexture virtualTexture) =>
        _virtualTextures.Add(virtualTexture.Name, virtualTexture);

    public static Texture LoadTexture(string path)
    {
        ImageResult image;

#if DEBUG
        if (!File.Exists(path))
            throw new ArgumentException(@$"Texture with path ""{path}"" doesn't exsist!");


        using (var stream = File.OpenRead(path))
            try
            {
                image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            }
            catch { throw new ArgumentException($"Texture with path {path} couldn't be loaded!"); }
#else
        using (var stream = File.OpenRead(path))
            image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

#endif

        var name = Path.GetFileNameWithoutExtension(path);
        var texture = LoadTextureInternal(image, name);
        _textures.Add(name, texture);
        texture.Path = path;

        return texture;
    }

    public static Texture LoadEmptyTexture(Vector2i size, string name, bool addCounterToName = false, PixelFormat format = PixelFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
    {
        var texture = LoadEmptyTextureInternal(size, name, format, pixelType, addCounterToName, out string actualName);
        _textures.Add(actualName, texture);

        return texture;
    }

    public static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
    {
        using (BinaryReader reader = new(stream))
        {
            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
                throw new NotSupportedException("Specified stream is not a wave file.");

            int riffChunkSize = reader.ReadInt32();

            string format = new string(reader.ReadChars(4));
            if (format != "WAVE")
                throw new NotSupportedException("Specified stream is not a wave file.");

            string formatSignature = new string(reader.ReadChars(4));
            if (formatSignature != "fmt ")
                throw new NotSupportedException("Specified wave file is not supported.");

            int formatChunkSize = reader.ReadInt32();
            int audioFormat = reader.ReadInt16();
            channels = reader.ReadInt16();
            rate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            int blockAlign = reader.ReadInt16();
            bits = reader.ReadInt16();

            string dataSignature = new string(reader.ReadChars(4));
            if (dataSignature != "data" && dataSignature != "LIST")
                throw new NotSupportedException("Specified wave file is not supported.");

            int dataChunkSize = reader.ReadInt32();

            return reader.ReadBytes((int)reader.BaseStream.Length);
        }
    }

    public static Shader GetShader(string name) => _shaderInfos[name].shader;

    public static ShaderInfo GetShaderInfo(string name) => _shaderInfos[name];

    public static Texture GetTexture(string name) => _textures[name];

    public static VirtualTexture GetVirtualTexture(string name)
    {
        if (!_virtualTextures.ContainsKey(name))
            return _virtualTextures["unknownTexture"];

        return _virtualTextures[name];
    }

    public static bool HasTexture(string name) => _textures.ContainsKey(name);

    public static bool HasVirtualTexture(string name) => _virtualTextures.ContainsKey(name);
 
    public static Dictionary<string, Texture> GetTextures() => _textures;

    public static Dictionary<string, VirtualTexture> GetVirtualTextures() => _virtualTextures;

    private static Texture LoadTextureInternal(ImageResult image, string name)
    {
        int handle = GL.GenTexture();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, handle);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            image.Width,
            image.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            image.Data
            );

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TextureParameter(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameter(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return new Texture(handle, image.Width, image.Height, name);
    }

    private static Texture LoadEmptyTextureInternal(Vector2i size, string name, PixelFormat format, PixelType pixelType, bool addCounterToName, out string actualName)
    {
        _emptyTexturesCounter++;
        actualName = !addCounterToName ? name : name + _emptyTexturesCounter.ToString();

        int handle = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, handle);

        GL.TexImage2D(
               TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                size.X,
                size.Y,
                0,
                format,
                pixelType,
                (IntPtr)0
            );

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TextureParameter(handle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameter(handle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        return new Texture(handle, size.X, size.Y, actualName);
    }
}