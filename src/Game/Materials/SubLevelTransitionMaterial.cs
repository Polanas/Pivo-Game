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
        fragPath = MyPath.Join(MyPath.MaterialsDirectory, "subLevelTransition.frag");
        vertPath = MyPath.Join(MyPath.ShadersDirectory, @"batchShader\shader.vert");
    }
}
