using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class FinalRenderSystem : MySystem
{

    private Shader _shader;

    private VertexArray _vertexArray;

    public override void Run(EcsSystems systems)
    {
        ref var postData = ref sharedData.eventBus.NewEventSingleton<PostProcessingGLData>();

        _shader.Use();
        _shader.SetValue("scale", sharedData.gameData.camera.zoom);
        postData.spritesTexture.Use();

        _vertexArray.RenderBegin();
            _vertexArray.RenderElements(1);
            postData.uiTexture.Use();
            _shader.SetValue("scale", 1f);
            _vertexArray.RenderElements(1);
        _vertexArray.RenderEnd();
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

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

        _shader = Content.GetShader("finalTexture");

        _vertexArray = new(PrimitiveType.Triangles);

        ElementsBuffer elementsBuffer = new(6);
        elementsBuffer.SetData(indices);
        _vertexArray.SetElementsBuffer(elementsBuffer);

        VertexBuffer vertexBuffer = new(BufferUsageHint.StaticDraw, 8);
        vertexBuffer.SetData(vertices);
        vertexBuffer.SetStride<float>(2);
        vertexBuffer.SetAttribPointer<float>(2);
        _vertexArray.SetVertexBuffer(vertexBuffer);
    }
}
