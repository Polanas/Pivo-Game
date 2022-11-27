using System;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class FrameBuffer : IFrameBuffer
{

    public int Handle => _handle;

    private int _handle;

    private ClearBufferMask _clearBufferMask;

    public FrameBuffer(DrawBuffersEnum[] drawBuffersEnums, ClearBufferMask clearBufferMask)
    {
        _clearBufferMask = clearBufferMask;

        Init(drawBuffersEnums);
    }

    public FrameBuffer(DrawBuffersEnum drawBuffersEnum, ClearBufferMask clearBufferMask)
    {
        _clearBufferMask = clearBufferMask;

        Init(new[] { drawBuffersEnum });
    }

    public void UseAndClearTexture(Texture texture, FramebufferAttachment framebufferAttachment)
    {
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, framebufferAttachment, TextureTarget.Texture2D, texture, 0);
        GL.Clear(_clearBufferMask);
    }

    public void UseTexture(Texture texture, FramebufferAttachment framebufferAttachment)
    {
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, framebufferAttachment, TextureTarget.Texture2D, texture, 0);
    }

    public void Use() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);

    public void UseDefault() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

    public void Clear() => GL.Clear(_clearBufferMask);

    public int GetHandle() => _handle;

    private void Init(DrawBuffersEnum[] drawBuffersEnums)
    {
        _handle = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
        GL.DrawBuffers(drawBuffersEnums.Length, drawBuffersEnums);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}
