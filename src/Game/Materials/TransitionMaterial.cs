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
        fragPath = MyPath.Join(MyPath.MaterialsDirectory, @"transition.frag");
        vertPath = MyPath.Join(MyPath.ShadersDirectory, @"batchShader\shader.vert");
    }
}
