using System;

namespace Game;

class SubLevelTransition1Material : Material
{
    [Uniform("tm", UniformType.Float)]
    public float tm;

    [Uniform("rand", UniformType.Float)]
    public float rand;

    [Uniform("reverse", UniformType.Bool)]
    public bool reverse;

    public SubLevelTransition1Material()
    {
        fragPath = Paths.Combine(Paths.MaterialsDirectory, "subLevelTransition1.frag");
        vertPath = Paths.Combine(Paths.ShadersDirectory, @"batchShader\shader.vert");
    }
}
