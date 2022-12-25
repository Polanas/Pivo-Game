using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Game;

abstract class Material
{

    public Shader Shader { get; private set; }

    public string Name { get; private set; }

    public Texture TextureTarget { get; private set; }

    public bool IsApplying { get; set; } = true;

    public Vector2 QuadSize { get; set; }

    public Dictionary<string, ITexture> Textures => textures;

    protected Dictionary<string, ITexture> textures = new();

    protected string fragPath;

    protected string vertPath;

    private bool _disposed;

    public Material() { }

    public static T Create<T>(string fragPath = null, string vertPath = null) where T : Material, new()
    {
        var material = new T();
        var ActualLocalFragPath = fragPath ?? material.fragPath;
        var ActualLocalVertPath = vertPath ?? material.vertPath;

        string shaderName = Path.GetFileNameWithoutExtension(ActualLocalFragPath);
        material.Shader = Content.RequestShader(shaderName, ActualLocalVertPath, ActualLocalFragPath); 
        material.Name = TypeOf<T>.Name;

        return material;
    }

    public static T Create<T>(Vector2 quadSize, string localFragPath = null, string localVertPath = null) where T : Material, new()
    {
        var material = new T();
        var ActualLocalFragPath = localFragPath ?? material.fragPath;
        var ActualLocalVertPath = localVertPath ?? material.vertPath;

        string shaderName = Path.GetFileNameWithoutExtension(ActualLocalFragPath);
        material.Shader = Content.RequestShader(shaderName, ActualLocalVertPath, ActualLocalFragPath);
        material.Name = TypeOf<T>.Name;
        material.QuadSize = quadSize;

        return material;
    }

    public static T Create<T>(Texture textureTarget, string localFragPath = null, string localVertPath = null) where T : Material, new()
    {
        var material = new T();
        var ActualLocalFragPath = localFragPath ?? material.fragPath;
        var ActualLocalVertPath = localVertPath ?? material.vertPath;

        string shaderName = Path.GetFileNameWithoutExtension(ActualLocalFragPath);
        material.Shader = Content.RequestShader(shaderName, ActualLocalVertPath, ActualLocalFragPath);
        material.Name = TypeOf<T>.Name;
        material.QuadSize = textureTarget.Size;
        material.TextureTarget = textureTarget;

        return material;
    }

    public void Dispose()
    {
        if (TextureTarget == null || _disposed)
            return;

        Content.DeleteTexture(TextureTarget);
        _disposed = true;
    }
}