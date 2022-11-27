using System;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class VertexArray
{

    public int Handle => _handle;

    private PrimitiveType PrimitiveType { get; set; }

    private IVertexBuffer _VBO;

    private IElementsBuffer _EBO;

    private IFrameBuffer _FBO;

    private int _handle;

    public VertexArray(PrimitiveType primitiveType)
    {
        _handle = GL.GenVertexArray();
        GL.BindVertexArray(_handle);

        PrimitiveType = primitiveType;
    }

    public void RenderArrays(int first, int count)
    {
        GL.DrawArrays(PrimitiveType, first * _VBO.GetSizePerInstance(), count * _VBO.GetSizePerInstance());
    }

    public void RenderElements(int count)
    {
        GL.DrawElements(PrimitiveType, count * _EBO.GetElementsPerInstance(), _EBO.GetElementsType(), IntPtr.Zero);
    }

    public void Delete() => GL.DeleteVertexArray(_handle);

    public void RenderBegin()
    {
        GL.BindVertexArray(_handle);
        _FBO?.Use();
        _FBO?.Clear();
        _VBO.Use();
    }

    public void RenderEnd()
    {
        if (_FBO != null)
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void DeleteAll()
    {
        DeleteFrameBuffer();
        DeleteElementBuffer();
        DeleteVertexBuffer();
        Delete();
    }

    public void SetVertexBuffer(IVertexBuffer vertexBuffer) => _VBO = vertexBuffer;

    public void DeleteVertexBuffer()
    {
        var handle = _VBO.GetHandle();

        if (GL.IsBuffer(handle))
            GL.DeleteBuffer(_VBO.GetHandle());

        _VBO = null;
    }

    public void SetElementsBuffer(IElementsBuffer elementsBuffer) => _EBO = elementsBuffer;

    public void DeleteElementBuffer()
    {
        var handle = _EBO.GetHandle();

        if (GL.IsBuffer(handle))
            GL.DeleteBuffer(_EBO.GetHandle());

        _EBO = null;
    }

    public void SetFrameBuffer(IFrameBuffer frameBuffer) => _FBO = frameBuffer;

    public void DeleteFrameBuffer()
    {
        var handle = _FBO.GetHandle();

        if (GL.IsFramebuffer(handle))
            GL.DeleteFramebuffer(handle);

        _FBO = null;
    }
}
