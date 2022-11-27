using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class BeerMaterial : Material
{

    [Uniform("frame", UniformType.Int)]
    public int frame;

    public BeerMaterial()
    {
        fragPath = Paths.Combine(Paths.MaterialsDirectory, "beerMaterial.frag");
        vertPath = Paths.Combine(Paths.ShadersDirectory, @"batchShader\shader.vert");
    }
}
