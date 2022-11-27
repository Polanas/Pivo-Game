using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Game;

class ToTextureRenderer
{

    private int _VAO;

    private int _textureUnit;

    private int _EAO;

    private int _FBO;

    private Shader _lastShader;

    private float _time;

    private Dictionary<Type, List<MaterialFieldData>> _materialUnifromFields = new();

    public void Update(float deltaTime)
    {
        _time += deltaTime;
    }

    public void Render(Material material)
    {
        if ((material.Shader is var shader) && shader != _lastShader)
            material.Shader.Use();

        GL.BindVertexArray(_VAO);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _FBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, material.TextureTarget, 0);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.Viewport(0, 0, material.TextureTarget.Width, material.TextureTarget.Height);

        shader.SetValue("texSize", material.TextureTarget.Size);
        shader.SetValue("time", _time);

        var materialInfo = _materialUnifromFields[material.GetType()];
        _textureUnit = (int)TextureUnit.Texture1;

        for (int i = 0; i < materialInfo.Count; i++)
        {
            var value = materialInfo[i].field.GetValue(material);

            SetShaderUnifrom(shader, materialInfo[i].uniformAttribute, value);
        }

        foreach (var pair in material.Textures)
        {
            shader.UseTexture(pair.Key, pair.Value, (TextureUnit)_textureUnit);
            _textureUnit++;
        }

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (IntPtr)0);

        for (int i = _textureUnit; i >= (int)TextureUnit.Texture1; i--)
            GLUtils.ClearTextureUnit((TextureUnit)i);

        GL.ActiveTexture(TextureUnit.Texture0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindVertexArray(0);

        _lastShader = material.Shader;
        _textureUnit = 0;
        GL.Viewport(0, 0, MyGameWindow.ScreenSize.X, MyGameWindow.ScreenSize.Y);
    }
    public void SetShaderUnifrom(Shader shader, UniformAttribute uniformAttribute, object value)
    {
        UniformType uniformType = uniformAttribute.uniformType;
        string uniformName = uniformAttribute.uniformName;

        switch (uniformType)
        {
            case UniformType.Float:
                shader.SetValue(uniformName, (float)value);
                break;
            case UniformType.Vector2:
                shader.SetValue(uniformName, (Vector2)value);
                break;
            case UniformType.Vector3:
                shader.SetValue(uniformName, (Vector3)value);
                break;
            case UniformType.Vector4:
                shader.SetValue(uniformName, (Vector4)value);
                break;
            case UniformType.Vector2i:
                shader.SetValue(uniformName, (Vector2i)value);
                break;
            case UniformType.Vector3i:
                shader.SetValue(uniformName, (Vector3i)value);
                break;
            case UniformType.Vector4i:
                shader.SetValue(uniformName, (Vector4i)value);
                break;
            case UniformType.Int:
                shader.SetValue(uniformName, (int)value);
                break;
            case UniformType.Bool:
                shader.SetValue(uniformName, (bool)value);
                break;
        }
    }

    public void OnFrameEnd()
    {
        _lastShader = null;
    }

    public void Init(Dictionary<Type, List<MaterialFieldData>> materialUniformsFields)
    {
        _VAO = GL.GenVertexArray();
        _EAO = GL.GenBuffer();
        _FBO = GL.GenFramebuffer();
        
        GL.BindVertexArray(_VAO);

        uint[] indices =
        {
          0,1,3,
          1,2,3
        };

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EAO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _FBO);
        GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        GL.BindVertexArray(0);

        _materialUnifromFields = materialUniformsFields;
    }
}