using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

static class Paths
{
    public static readonly string ContentDirectory = "Content";

    public static readonly string ShadersDirectory = Combine("Content", "Shaders");

    public static readonly string MaterialsDirectory = Combine(ShadersDirectory, "Materials");

    public static readonly string TexturesDirectory = Combine("Content", "Textures");

    public static readonly string FontsDirectory = Combine("Content", "Fonts");

    public static readonly string SavesDirectory = Combine("Content", "Saves");

    public static readonly string SoundsDirectory = Combine("Content", "Sounds");

    public static readonly string LevelsDirectory = Combine("Content", "Levels");

    public static string Combine(string path, string other) =>
        @$"{path}\{other}";
}