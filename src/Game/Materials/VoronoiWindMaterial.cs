using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class VoronoiWindMaterial : Material
{

    [Uniform("seed", UniformType.Float)]
    public float seed;

    public VoronoiWindMaterial()
    {
        fragPath = MyPath.Join(MyPath.MaterialsDirectory, @"voronoiWind.frag");
        vertPath = MyPath.Join(MyPath.ShadersDirectory, @"batchShader\shader.vert");
    }
}
