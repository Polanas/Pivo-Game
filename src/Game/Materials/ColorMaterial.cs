using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class ColorMaterial : Material
{
    [Uniform("color", UniformType.Vector3)]
    public Vector3 color;

    [Uniform("mixAmount", UniformType.Float)]
    public float mixAmount;

    public ColorMaterial()
    {
        fragPath = Paths.Combine(Paths.MaterialsDirectory, "colorM.frag");
    }
}