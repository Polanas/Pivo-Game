namespace Game;

class VirtualTexture : ITexture
{
    public Vector2i AtlasPosition { get; private set; }

    public Vector2i Size { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public string Name { get; private set; }

    public Dictionary<int, Vector2i> FramesPositions { get; private set; }

    public VirtualTexture(Vector2i atlasPosition, Vector2i size, string name, Dictionary<int, Vector2i> framesPositions)
    {
        AtlasPosition = atlasPosition;
        Size = size;
        Name = name;
        Width = size.X;
        Height = size.Y;
        FramesPositions = framesPositions;
    }

    public int GetWidth() => Width;

    public int GetHeight() => Height;
}
