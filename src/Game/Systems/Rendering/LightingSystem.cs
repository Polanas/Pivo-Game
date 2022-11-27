using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;

namespace Game;

class LightingSystem : RenderSystem
{

    private int _FBO;

    private Matrix4 _projection;

    private Shader _finalShadowShader;

    private Texture _compressedShadowsInfoTexture;

    public override void Run(EcsSystems systems)
    {
        GL.ActiveTexture(TextureUnit.Texture0);

        GL.BindVertexArray(VAO);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _FBO);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _compressedShadowsInfoTexture, 0);
        GL.Viewport(new System.Drawing.Rectangle(0, 0, 512, 1));
        shader.Use();
        shader.SetValue("projection", _projection);
     //   sharedData.renderData.shadowCastersTexture.Use();
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (IntPtr)null);

      //  GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, sharedData.renderData.lightTexture, 0);
      //  GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, sharedData.renderData.maskTexture, 0);
        GL.Viewport(new System.Drawing.Rectangle(0, 0, 512, 512));
        _finalShadowShader.Use();
        _finalShadowShader.SetValue("projection", _projection);
        _compressedShadowsInfoTexture.Use();
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (IntPtr)null);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindVertexArray(0);
        GL.Viewport(new System.Drawing.Rectangle(0, 0, MyGameWindow.ScreenSize.X, MyGameWindow.ScreenSize.Y));
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        shader = Content.GetShader("genShadows");
        _finalShadowShader = Content.GetShader("light");

        VAO = GL.GenVertexArray();
        EAO = GL.GenBuffer();
        VBO = GL.GenBuffer();
        _FBO = GL.GenFramebuffer();

        GL.BindVertexArray(VAO);

        uint[] indices =
        {
          0,1,3,
          1,2,3
        };

        float[] vertices =
        {
            0,0,
            1,0,
            1,1,
            0,1
        };

        //GL.BindBuffer(BufferTarget.ElementArrayBuffer, EAO);
        //GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        //GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        //GL.EnableVertexAttribArray(0);
        //GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        //GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        //Texture texture = Texture.LoadEmpty(new Vector2i(512), PixelType.Float);
        //sharedData.renderData.distortedShadowsTexture = texture;
        //sharedData.renderData.lightTexture = Texture.LoadEmpty(new Vector2i(512), PixelType.Float);
        //sharedData.renderData.maskTexture = Texture.LoadEmpty(new Vector2i(512));
        //_compressedShadowsInfoTexture = Texture.LoadEmpty(new Vector2i(512,1), PixelType.Float);

        //GL.BindFramebuffer(FramebufferTarget.Framebuffer, _FBO);
        //GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
        //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        //GL.BindVertexArray(0);

        Matrix4 model = MyMath.CreateTransformMatrix(MyGameWindow.ScreenSize / 2, MyGameWindow.ScreenSize);
        _projection = model * Matrix4.CreateOrthographicOffCenter(0, MyGameWindow.ScreenSize.X, MyGameWindow.ScreenSize.Y, 0, -1, 1);
    }
}