using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class Cool3DLIntroMaterial : Material
{

    public Texture texture1;

    public Texture texture2;

    public Cool3DLIntroMaterial()
    {
        fragPath = Paths.Combine(Paths.MaterialsDirectory, "cool3DIntro.frag");
        vertPath = Paths.Combine(Paths.MaterialsDirectory, "materials.vert");

        texture1 = Content.LoadTexture(Paths.Combine(Paths.TexturesDirectory, @"Intro\sdfText1.png"));
        texture2 = Content.LoadTexture(Paths.Combine(Paths.TexturesDirectory, @"Intro\sdfText2.png"));

        textures["sdfText1"] = texture1;
        textures["sdfText2"] = texture2;

        GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, texture1);

        GL.TextureParameter(texture1, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TextureParameter(texture1, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, texture2);

        GL.TextureParameter(texture2, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TextureParameter(texture2, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.BindTexture(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D, 0);
    }
}
