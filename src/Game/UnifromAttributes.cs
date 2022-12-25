using OpenTK.Graphics.OpenGL;

namespace Game;

enum UniformType
{
    Float,
    Vector2,
    Vector3,
    Vector4,
    Vector2i,
    Vector3i,
    Vector4i,
    Int,
    Bool,
    Matrix4,
}

[AttributeUsage(AttributeTargets.Field)]
class UniformAttribute : Attribute
{

    public readonly UniformType uniformType;

    public readonly string uniformName;

    public UniformAttribute(string uniformName, UniformType type)
    {
        uniformType = type;
        this.uniformName = uniformName;
    }
}