using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using System.Threading.Tasks;

namespace Game;

class RenderLayersSystem : MySystem
{

    private Shader _shader;

    private bool _switchedToUI;

    private VertexArray _VAO;

    private FrameBuffer _FBO;

    private ElementsBuffer _EBO;

    public override void Run(EcsSystems systems)
    {
        _switchedToUI = false;

        ref var layersData = ref sharedData.eventBus.NewEventSingleton<PostProcessingGLData>();

        // shader.Use();
        //   shader.SetMatrix4("projection", sharedData.RenderData.pixelatedProjection);

        //if (Keyboard.Pressed(Keys.K))
        //      sharedData.RenderData.rectangleLayer.ScreenTexture.SaveRGBA(@"C:\Users\ivanh\Downloads\test1.png");

        _shader.Use();
        // GL.BindVertexArray(_VAO);

        //GL.BindFramebuffer(FramebufferTarget.Framebuffer, _FBO);
        //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, layersData.spritesTexture, 0);
        //GL.Clear(ClearBufferMask.ColorBufferBit);
        _VAO.RenderBegin();
        _FBO.Use();
        _FBO.UseTexture(layersData.uiTexture, FramebufferAttachment.ColorAttachment0);
        _FBO.Clear();
        _FBO.UseTexture(layersData.spritesTexture, FramebufferAttachment.ColorAttachment0);
        _FBO.Clear();

        //shader.UseTexture("spritesTexture", sharedData.RenderData.background1Layer.ScreenTexture, TextureUnit.Texture0);
        //shader.UseTexture("lightTexture", sharedData.RenderData.lightTexture, TextureUnit.Texture1);
        //shader.UseTexture("lightMaskTexture", sharedData.RenderData.maskTexture, TextureUnit.Texture2);
        //shader.UseTexture("shadowCastersTexture", sharedData.RenderData.shadowCastersTexture, TextureUnit.Texture3);
        //GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (IntPtr)0);

        GL.ActiveTexture(TextureUnit.Texture0);

        for (int i = 0; i < sharedData.renderData.layersList.Count; i++)
        {
            var layer = sharedData.renderData.layersList[i];

            if (layer.isUI && !_switchedToUI)
            {
                _FBO.UseTexture(layersData.uiTexture, FramebufferAttachment.ColorAttachment0);
                _switchedToUI = true;
            }

            Matrix4 projection = sharedData.renderData.layerProjections[layer];

            _shader.SetValue("projection", projection);
            sharedData.renderData.layersList[i].Texture.Use();

            _VAO.RenderElements(1);
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _VAO.RenderEnd();
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _shader = Content.GetShader("postProcessing1");

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

        _VAO = new VertexArray(PrimitiveType.Triangles);

        _EBO = new ElementsBuffer(6);
        _EBO.SetData(indices);
        _VAO.SetElementsBuffer(_EBO);

        var VBO = new VertexBuffer(BufferUsageHint.StaticDraw, 8);
        VBO.SetData(vertices);
        VBO.SetStride<float>(2);
        VBO.SetAttribPointer<float>(2);
        _VAO.SetVertexBuffer(VBO);

        _FBO = new FrameBuffer(DrawBuffersEnum.ColorAttachment0, ClearBufferMask.ColorBufferBit);

        var EBO = new ElementsBuffer(6);
        EBO.SetData(indices);

        GL.BindVertexArray(0);
    }
}