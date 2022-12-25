using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Game;

static class MyPath
{
    public static readonly string ContentDirectory = "Content";

    public static readonly string ShadersDirectory = Join("Content", "Shaders");

    public static readonly string MaterialsDirectory = Join(ShadersDirectory, "Materials");

    public static readonly string TexturesDirectory = Join("Content", "Textures");

    public static readonly string FontsDirectory = Join("Content", "Fonts");

    public static readonly string SavesDirectory = Join("Content", "Saves");

    public static readonly string SoundsDirectory = Join("Content", "Sounds");

    public static readonly string LevelsDirectory = Join("Content", "Levels");

    public static readonly string TexturesMetaDataDirectory = Join("Content", "Textures meta data");

    public static string Join(string path, string other) =>
        Path.Join(path, other);
}