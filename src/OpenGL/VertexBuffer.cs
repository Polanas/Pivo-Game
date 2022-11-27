using System;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class VertexBuffer : IVertexBuffer
{

    public int Handle => _handle;

    private object[] _data;

    private BufferUsageHint _usageHint;

    private int _offset;

    private int _attribIndex;

    private int _handle;

    private int _stride;

    private int _sizePerInstance;

    private VertexAttribPointerType _pointerType;

    private VertexAttribIntegerType _iPointerType;

    private static Dictionary<TypeCode, VertexAttribIntegerType> _vertexAttribIntTypes;

    private static Dictionary<TypeCode, VertexAttribPointerType> _vertexAttribTypes;

    private bool _isDataTypeInteger;

    public VertexBuffer(BufferUsageHint usageHint, int offsetPerInstance)
    {
        _usageHint = usageHint;
        _sizePerInstance = offsetPerInstance;

        Init();
    }

    public void SetData<T>(T[] data) where T : struct
    {
        _data = Unsafe.As<object[]>(data);
        GL.BufferData(BufferTarget.ArrayBuffer, Unsafe.SizeOf<T>() * data.Length, ref data[0], _usageHint);
    }

    public void SetEmptyData<T>(int length) where T : struct
    {
        GL.BufferData(BufferTarget.ArrayBuffer, Unsafe.SizeOf<T>() * length, IntPtr.Zero, _usageHint);
    }

    public void UpdateData<T>(T[] data, int offset = 0) where T : struct
    {
        _data = Unsafe.As<object[]>(data);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);
        GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offset, Unsafe.SizeOf<T>() * data.Length, ref data[0]);
    }

    public void SetAttribPointerType<T>() where T : struct
    {
        _isDataTypeInteger = Utils.IsInteger<T>();
        var typeCode = Type.GetTypeCode(TypeOf<T>.Raw);

        if (_isDataTypeInteger)
            _iPointerType = _vertexAttribIntTypes[typeCode];
        else _pointerType = _vertexAttribTypes[typeCode];
    }

    public void SetStride<T>(int amount) where T : struct
    {
        _stride = Unsafe.SizeOf<T>() * amount;
    }

    public void SetAttribPointer<T>(int size) where T : struct
    {
        if (_stride <= 0)
            throw new InvalidDataException("Stride was invalid");

        _isDataTypeInteger = Utils.IsInteger<T>();
        var typeCode = Type.GetTypeCode(TypeOf<T>.Raw);

        GL.EnableVertexAttribArray(_attribIndex);

        if (!_isDataTypeInteger)
            GL.VertexAttribPointer(_attribIndex, size, _vertexAttribTypes[typeCode], false,  _stride, (IntPtr)(sizeof(float) * _offset));
        else GL.VertexAttribIPointer(_attribIndex, size, _vertexAttribIntTypes[typeCode],  _stride, (IntPtr)(sizeof(float) * _offset));

        _attribIndex++;
        _offset += size;
    }

    public void SetAttribPointers(params int[] size)
    {
        if (_stride <= 0)
            throw new InvalidDataException("Stride was invalid");

        for (int i = 0; i < size.Length; i++)
        {
            GL.EnableVertexAttribArray(_attribIndex);

            if (!_isDataTypeInteger)
                GL.VertexAttribPointer(_attribIndex, size[i], _pointerType, false, _stride, (IntPtr)(sizeof(float) * _offset));
            else GL.VertexAttribIPointer(_attribIndex, size[i], _iPointerType, _stride, (IntPtr)(sizeof(float) * _offset));

            _attribIndex++;
            _offset += size[i];
        }
    }

    public int GetSizePerInstance() => _sizePerInstance;

    public int GetHandle() => _handle;

    public void Use() => GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);

    public static void InitStatic()
    {
        _vertexAttribIntTypes = new();
        _vertexAttribIntTypes[Type.GetTypeCode(TypeOf<int>.Raw)] = VertexAttribIntegerType.Int; 
        _vertexAttribIntTypes[Type.GetTypeCode(TypeOf<uint>.Raw)] = VertexAttribIntegerType.UnsignedInt;
        _vertexAttribIntTypes[Type.GetTypeCode(TypeOf<ushort>.Raw)] = VertexAttribIntegerType.UnsignedShort;
        _vertexAttribIntTypes[Type.GetTypeCode(TypeOf<byte>.Raw)] = VertexAttribIntegerType.Byte;

        _vertexAttribTypes = new();
        _vertexAttribTypes[Type.GetTypeCode(TypeOf<float>.Raw)] = VertexAttribPointerType.Float;
        _vertexAttribTypes[Type.GetTypeCode(TypeOf<ushort>.Raw)] = VertexAttribPointerType.UnsignedShort;
        _vertexAttribTypes[Type.GetTypeCode(TypeOf<byte>.Raw)] = VertexAttribPointerType.UnsignedByte;
        _vertexAttribTypes[Type.GetTypeCode(TypeOf<double>.Raw)] = VertexAttribPointerType.Double;
    }

    private void Init()
    {
        _handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);
    }
}
