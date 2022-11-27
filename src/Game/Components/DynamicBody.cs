using Box2DSharp.Dynamics;

namespace Game;

struct AdditionalObject
{
    public Vector2 localPosition;

    public Vector2 size;
}

struct DynamicBody
{
    public Body box2DBody;

    public List<AdditionalObject> additionalObjects;    
}