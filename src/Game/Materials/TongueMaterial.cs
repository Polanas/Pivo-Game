using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class TongueMaterial : Material
{

    [Uniform("angle", UniformType.Float)]
    public float angle;

    [Uniform("len", UniformType.Float)]
    public float length;

    [Uniform("color", UniformType.Vector3)]
    public Vector3 color;

    public TongueMaterial()
    {
        fragPath = Paths.Combine(Paths.MaterialsDirectory, @"tongue.frag");
        vertPath = Paths.Combine(Paths.ShadersDirectory, @"batchShader\shader.vert");
    }
}
