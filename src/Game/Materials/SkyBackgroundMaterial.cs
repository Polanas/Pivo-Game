using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class SkyBackgroundMaterial : Material
{
    [Uniform("camY", UniformType.Float)]
    public float camY;

    [Uniform("col1", UniformType.Vector3)]
    public Vector3 color1;

    [Uniform("col2", UniformType.Vector3)]
    public Vector3 color2;

    public SkyBackgroundMaterial()
    {
        fragPath = MyPath.Join(MyPath.MaterialsDirectory, @"skyBackground.frag");
        vertPath = MyPath.Join(MyPath.ShadersDirectory, @"batchShader\shader.vert");
    }
}
