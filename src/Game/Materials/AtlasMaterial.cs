using System;

namespace Game;

class AtlasMaterial : Material
{

    [Uniform("frameData", UniformType.Vector3i)]
    public Vector3i frameData;

    [Uniform("cameraMatrix", UniformType.Matrix4)]
    public Matrix4 cameraMatrix;

    [Uniform("modelMatrix", UniformType.Matrix4)]
    public Matrix4 modelMatix;


    public AtlasMaterial()
    {
        fragPath = MyPath.Join(MyPath.MaterialsDirectory, "atlas.frag");
        vertPath = MyPath.Join(MyPath.MaterialsDirectory, "atlas.vert");

        textures.Add("spriteTexture", null);
    }
}
