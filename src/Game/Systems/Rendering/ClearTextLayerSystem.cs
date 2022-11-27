using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class ClearTextLayerSystem : MySystem
{
    public override void Run(EcsSystems systems)
    {
        var textTexture = sharedData.renderData.layers["text"].Texture;
        var postProcessingGLData = sharedData.eventBus.GetEventBodySingleton<PostProcessingGLData>();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, postProcessingGLData.renerSpritesFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textTexture, 0);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}
