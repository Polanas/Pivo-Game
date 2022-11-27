using System;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class ElementsBuffer : IElementsBuffer
{

    public int Handle => _handle;

    private int _handle;

    private DrawElementsType _elementsType;

    private int _indicesPerInstance;

    private static Dictionary<string, DrawElementsType> _drawElementTypes;

    public ElementsBuffer(int indicesPerInstance)
    {
        _indicesPerInstance = indicesPerInstance;

        Init();
    }

    public int GetElementsPerInstance() => _indicesPerInstance;

    public DrawElementsType GetElementsType() => _elementsType;

    public int GetHandle() => _handle;

    public void Use() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);

    public void SetData<T>(T[] data) where T : struct
    {
        var typeName = TypeOf<T>.Name;

        if (!_drawElementTypes.ContainsKey(typeName))
            throw new Exception("Unsupported data type");

        GL.BufferData(BufferTarget.ElementArrayBuffer, Unsafe.SizeOf<T>() * data.Length, ref data[0], BufferUsageHint.StaticDraw);
        _elementsType = _drawElementTypes[TypeOf<T>.Name];
    }

    public static void InitStatic()
    {
        _drawElementTypes = new();

        _drawElementTypes.Add(TypeOf<uint>.Name, DrawElementsType.UnsignedInt);
        _drawElementTypes.Add(TypeOf<ushort>.Name, DrawElementsType.UnsignedShort);
        _drawElementTypes.Add(TypeOf<byte>.Name, DrawElementsType.UnsignedByte);
    }

    private void Init()
    {
        _handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);
    }
}
