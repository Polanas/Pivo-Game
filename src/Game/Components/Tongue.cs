namespace Game;

struct Tongue
{
    public TongueMaterial topMaterial;

    public TongueMaterial bottomMaterial;

    public Vector2 position;

    public float angle;

    public float length;

    public bool rendered;

    public bool flipped;

    public Box2DSharp.Dynamics.Body sensor;

    public bool r_collided;
}
