namespace Game;

struct SpriteBatchItem
{
    public Vector4 projectionPart1;

    public Vector2 projectionPart2;

    public float depth;

    public Vector4 color;

    public Vector3i frameData;

    public ITexture texture;

    public Layer layer;

    public Material material;

    public SpriteBatchItem(Vector4 projectionPart1, Vector2 projectionPart2, float depth, Vector4 color, Vector3i frameData, ITexture texture, Layer layer, Material material)
    {
        this.projectionPart1 = projectionPart1;
        this.projectionPart2 = projectionPart2;
        this.depth = depth;
        this.frameData = frameData;
        this.color = color;
        this.texture = texture;
        this.layer = layer;
        this.material = material;
    }
}