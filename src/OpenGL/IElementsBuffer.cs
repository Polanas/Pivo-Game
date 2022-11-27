using System;
using OpenTK.Graphics.OpenGL4;

namespace Game;

interface IElementsBuffer : IBuffer
{
    int GetElementsPerInstance();

    DrawElementsType GetElementsType();
}
