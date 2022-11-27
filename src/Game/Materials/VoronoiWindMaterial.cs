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
        fragPath = Paths.Combine(Paths.MaterialsDirectory, @"voronoiWind.frag");
        vertPath = Paths.Combine(Paths.ShadersDirectory, @"batchShader\shader.vert");
    }
}
