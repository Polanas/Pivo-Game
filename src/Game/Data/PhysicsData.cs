using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DSharp.Dynamics;

namespace Game;

class PhysicsData : IGameData
{

    public float deltaTime;

    public float time;

    public float fixedDeltaTime;

    public ContactListener contcactListener;

    public readonly float PTM;

    public readonly PhysicsObjectsFactory physicsFactory;

    public readonly World b2World;

    public readonly Vector2 gravity;

    public readonly int velocityIterations;

    public readonly int positionIterations;

    public PhysicsData(float fixedDeltaTime, float PTM, PhysicsObjectsFactory physicsFactory, World b2World, int velocityIterations, int positionIterations)
    {
        this.fixedDeltaTime = fixedDeltaTime;
        this.PTM = PTM;
        this.physicsFactory = physicsFactory;
        this.b2World = b2World;
        this.velocityIterations = velocityIterations;
        this.positionIterations = positionIterations;

        gravity = b2World.Gravity.ToOpenTKVector2();
    }
}
