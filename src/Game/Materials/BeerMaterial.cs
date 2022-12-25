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
        fragPath = MyPath.Join(MyPath.MaterialsDirectory, "beerMaterial.frag");
        vertPath = MyPath.Join(MyPath.ShadersDirectory, @"batchShader\shader.vert");
    }
}
