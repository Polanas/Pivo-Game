using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class SubLevelTransitionMaterial : Material
{

    [Uniform("tm", UniformType.Float)]
    public float time;

    public SubLevelTransitionMaterial()
    {
        fragPath = Paths.Combine(Paths.MaterialsDirectory, "subLevelTransition.frag");
        vertPath = Paths.Combine(Paths.ShadersDirectory, @"batchShader\shader.vert");
    }
}
