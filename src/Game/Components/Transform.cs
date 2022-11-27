namespace Game;

struct Transform
{

    public float Left => position.X - size.X / 2;

    public float Right => position.X + size.X / 2;

    public float Top => position.Y - size.Y / 2;

    public float Bottom => position.Y + size.Y / 2;

    public Vector2 position;

    public float angle;

    public Vector2 size;

    public Transform(float posX, float posY, float sizeX, float sizeY)
    {
        angle = 0;
        position = new Vector2(posX, posY);
        size = new Vector2(sizeX, sizeY);
    }

    public Transform(Vector2 position, Vector2 size)
    {
        angle = 0;
        this.position = position;
        this.size = size;
    }

    public Transform(Vector2 position, float angle, Vector2 size)
    {
        this.angle = angle;
        this.position = position;
        this.size = size;
    }
}
