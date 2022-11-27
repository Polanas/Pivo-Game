using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class TransitionMaterial : Material
{

    [Uniform("cover", UniformType.Float)]
    public float coverAmount;

    [Uniform("coverPos", UniformType.Vector2)]
    public Vector2 coverPos;

    public TransitionMaterial()
    {
        fragPath = Paths.Combine(Paths.MaterialsDirectory, @"transition.frag");
        vertPath = Paths.Combine(Paths.ShadersDirectory, @"batchShader\shader.vert");
    }
}
