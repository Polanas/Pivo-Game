using System;
using OpenTK.Graphics.OpenGL4;

namespace Game;

interface IFrameBuffer : IBuffer
{
    void Clear();
}
