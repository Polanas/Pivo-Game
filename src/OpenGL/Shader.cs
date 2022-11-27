using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class Shader
{

    public int Handle { get; private set; }

    public readonly string name;

    private Dictionary<string, int> _uniformLocations = new();

    public static Shader Load(string vertSource, string fragSource, string name, string geometrySource = null)
    {
        int vertShader, fragShader, geomShader = 0;
        int handle = 0;
        Dictionary<string, int> uniformLocations = new();

        vertShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertShader, vertSource);
        CompileShader(vertShader);

        fragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragShader, fragSource);
        CompileShader(fragShader);

        bool hasGShader = geometrySource != null;

        if (hasGShader)
        {
            geomShader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(handle, geometrySource);
            CompileShader(geomShader);
        }

        handle = GL.CreateProgram();

        GL.AttachShader(handle, vertShader);
        GL.AttachShader(handle, fragShader);
        if (hasGShader)
            GL.AttachShader(handle, geomShader);

        LinkProgram(handle, name);

        GL.DetachShader(handle, fragShader);
        GL.DetachShader(handle, vertShader);
        GL.DeleteShader(fragShader);
        GL.DeleteShader(vertShader);

        if (hasGShader)
        {
            GL.DetachShader(handle, geomShader);
            GL.DeleteShader(geomShader);
        }

        GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

        for (int i = 0; i < numberOfUniforms; i++)
        {
            var key = GL.GetActiveUniform(handle, i, out _, out _);
            var location = GL.GetUniformLocation(handle, key);
            uniformLocations.Add(key, location);
        }

        return new Shader(handle, uniformLocations, name);
    }

    public static void Reload(Shader shader, string vertSource, string fragSource, string geometrySource, out int handle, out Dictionary<string, int> uniformLocations)
    {
        int vertShader, fragShader, geomShader = 0;
        int oldHandle = shader.Handle;
        uniformLocations = new();

        handle = GL.CreateProgram();

        vertShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertShader, vertSource);
        GL.CompileShader(vertShader);

        fragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragShader, fragSource);
        GL.CompileShader(fragShader);

        bool hasGShader = geometrySource != null;
        if (hasGShader)
        {
            geomShader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(handle, geometrySource);
            GL.CompileShader(geomShader);
        }

        GL.AttachShader(handle, vertShader);
        GL.AttachShader(handle, fragShader);
        if (hasGShader)
            GL.AttachShader(handle, geomShader);

        LinkProgram(handle, shader.name);

        GL.DetachShader(handle, fragShader);
        GL.DetachShader(handle, vertShader);
        GL.DeleteShader(fragShader);
        GL.DeleteShader(vertShader);

        if (hasGShader)
        {
            GL.DetachShader(handle, geomShader);
            GL.DeleteShader(geomShader);
        }

        GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

        for (int i = 0; i < numberOfUniforms; i++)
        {
            var key = GL.GetActiveUniform(handle, i, out _, out _);
            var location = GL.GetUniformLocation(handle, key);
            uniformLocations.Add(key, location);
        }

        GL.DeleteProgram(oldHandle);
    }

    public Shader(int handle, Dictionary<string, int> uniformLocations, string name)
    {
        Handle = handle;
        this.name = name;
        _uniformLocations = uniformLocations;
    }

    private static void CompileShader(int handle)
    {
        GL.CompileShader(handle);

        GL.GetShader(handle, ShaderParameter.CompileStatus, out var code);
        if (code != (int)All.True)
        {
            var infoLog = GL.GetShaderInfoLog(handle);
            throw new Exception($"Error occurred while compiling Shader({handle}).\n\n{infoLog}");
        }
    }

    private static void LinkProgram(int program, string name)
    {
        GL.LinkProgram(program);

        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);

        if (code != (int)All.True)
            throw new Exception($"Error occurred while linking Program {name} ({program}). Log: {GL.GetProgramInfoLog(program)}");
    }

    public void Use() => GL.UseProgram(Handle);

    public int GetUniformLocation(string name) => GL.GetUniformLocation(Handle, name);

    public void ResetData(int handle, Dictionary<string, int> unifromLocations)
    {
        Handle = handle;
        _uniformLocations = unifromLocations;
    }

    public void UseTexture(string name, Texture texture, TextureUnit unit)
    {
        SetValue(name, (int)unit - (int)TextureUnit.Texture0);
        GL.ActiveTexture(unit);
        texture.Use();
    }

    public void SetValue(string name, Vector2[] value)
    {
        float[] data = new float[value.Length * 2];

        for (int i = 0; i < data.Length; i += 2)
        {
            data[i] = value[i / 2].X;
            data[i + 1] = value[i / 2].Y;
        }

        GL.Uniform2(_uniformLocations[name], data.Length, data);
    }

    public void SetValue(string name, float[] value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.Uniform2(_uniformLocations[name], value.Length, value);
    }

    public void SetValue(string name, int value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.Uniform1(_uniformLocations[name], value);
    }

    public void SetValue(string name, bool value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.Uniform1(_uniformLocations[name], value ? 1 : 0);
    }

    public void SetValue(string name, float value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.Uniform1(_uniformLocations[name], value);
    }

    public void SetValue(string name, Vector2 value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.Uniform2(_uniformLocations[name], value);
    }

    public void SetValue(string name, Vector2i value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.Uniform2(_uniformLocations[name], value);
    }

    public void SetValue(string name, Vector3 value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.Uniform3(_uniformLocations[name], value);
    }

    public void SetValue(string name, Vector4 value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.Uniform4(_uniformLocations[name], value);
    }

    public void SetValue(string name, Matrix4 value)
    {
        if (!_uniformLocations.ContainsKey(name))
            return;

        GL.UniformMatrix4(_uniformLocations[name], true, ref value);
    }
}