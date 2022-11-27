using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DSharp.Common;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;

namespace Game;

class PhysicsObjectsFactory : Service, IRegularService
{

    private EcsPool<Transform> _transforms;

    private EcsPool<StaticBody> _staticBodies;

    private EcsPool<DynamicBody> _dynamicBodies;

    private World _b2World;

    private float _PTM;

    public override void Init(SharedData sharedData, EcsWorld world, ServiceState state)
    {
        base.Init(sharedData, world, state);

        _transforms = world.GetPool<Transform>();
        _staticBodies = world.GetPool<StaticBody>();
        _dynamicBodies = world.GetPool<DynamicBody>();

        _b2World = sharedData.physicsData.b2World;
        _PTM = sharedData.physicsData.PTM;
    }

    public Body AddSaticBody(int entity, Transform transform, PhysicsUserData userData = null, PhysicsCategory category = PhysicsCategory.Other, ushort maskBits = ushort.MaxValue)
    {
        ref var tr = ref _transforms.Add(entity);
        tr = transform;

        ref var stBody = ref _staticBodies.Add(entity);

        var bodyDef = new BodyDef()
        {
            BodyType = BodyType.StaticBody,
            AllowSleep = true,
        };

        Body body = _b2World.CreateBody(bodyDef);
        body.UserData = userData;

        PolygonShape shape = new();
        shape.SetAsBox(transform.size.X / 2 * _PTM, transform.size.Y / 2 * _PTM);

        var fixtureDef = new FixtureDef();
        fixtureDef.Shape = shape;
        fixtureDef.Density = 1;
        fixtureDef.UserData = userData;

        fixtureDef.Filter.CategoryBits = (ushort)category;
        fixtureDef.Filter.MaskBits = maskBits;

        body.SetTransform(new System.Numerics.Vector2(transform.position.X * _PTM, transform.position.Y * _PTM), 0);
        body.CreateFixture(fixtureDef);

        stBody.body = body;

        return body;
    }

    public Body AddDynamicBody(int entity,
                               Transform transform,
                               PhysicsUserData userData = null,
                               float mass = 1,
                               bool fixedRotation = true,
                               float friction = 0.2f,
                               float density = 0,
                               PhysicsCategory category = PhysicsCategory.Other,
                               ushort maskBits = ushort.MaxValue)
    {
        ref var tr = ref _transforms.Add(entity);
        tr = transform;

        ref var dBody = ref _dynamicBodies.Add(entity);

        var bodyDef = new BodyDef
        {
            BodyType = BodyType.DynamicBody,
            AllowSleep = true,
            FixedRotation = fixedRotation,
        };

        Body body = _b2World.CreateBody(bodyDef);
        body.UserData = userData;

        var shape = new PolygonShape();
        shape.SetAsBox(transform.size.X / 2 * _PTM, transform.size.Y / 2 * _PTM);

        var fixtureDef = new FixtureDef();
        fixtureDef.Shape = shape;
        fixtureDef.Density = density;
        fixtureDef.Friction = friction;
        fixtureDef.UserData = userData;

        fixtureDef.Filter.CategoryBits = (ushort)category;
        fixtureDef.Filter.MaskBits = maskBits;

        body.CreateFixture(fixtureDef);
        body.SetTransform(new System.Numerics.Vector2(transform.position.X * _PTM, transform.position.Y * _PTM), 0);

        MassData massData;
        body.GetMassData(out massData);
        massData.Mass = mass;
        body.SetMassData(massData);

        dBody.box2DBody = body;

        return body;
    }

    /// <summary>
    /// This needs to be reworked
    /// </summary>
    public void AddSensor(Body body, Vector2 size, PhysicsUserData userData = null, PhysicsCategory physicsCategory = PhysicsCategory.Other)
    {
        var fixtureDef = new FixtureDef();
        var shape = new PolygonShape();
        shape.SetAsBox(size.X * sharedData.physicsData.PTM - 1f, 3 * sharedData.physicsData.PTM, new(0, size.Y / 2 * sharedData.physicsData.PTM), 0);
        fixtureDef.Shape = shape;
        fixtureDef.IsSensor = true;
        fixtureDef.UserData = userData;
        body.CreateFixture(fixtureDef);
    }
    
    public Body CreateBoxSensor(Vector2 size, PhysicsUserData userData = null, PhysicsCategory category = PhysicsCategory.Other, ushort maskBits = ushort.MaxValue)
    {
        var bodyDef = new BodyDef
        {
            BodyType = BodyType.DynamicBody,
            AllowSleep = false,
            FixedRotation = true
        };

        var body = _b2World.CreateBody(bodyDef);
        body.UserData = userData;

        var shape = new PolygonShape();
        shape.SetAsBox(size.X / 2 * _PTM, size.Y / 2 * _PTM);

        var fixture = new FixtureDef();
        fixture.Shape = shape;
        fixture.IsSensor = true;
        fixture.UserData = userData;

        fixture.Filter.CategoryBits = (ushort)category;
        fixture.Filter.MaskBits = maskBits;

        body.CreateFixture(fixture);

        return body;
    }

    public void AddAdditionalBody(int e, Vector2 localPos, Vector2 size)
    {
        ref var body = ref _dynamicBodies.Get(e);

        var shape = new PolygonShape();
        shape.SetAsBox(size.X/2 * _PTM, size.Y/2 * _PTM, (localPos*_PTM).ToSystemVector2(), 0);
        body.box2DBody.CreateFixture(shape, 1f);

        body.additionalObjects = new();
        body.additionalObjects.Add(new AdditionalObject { localPosition = localPos, size = size });
    }

    public void RemoveStaticBody(int entity)
    {
        ref var box2dBody = ref _staticBodies.Get(entity).body;
        _b2World.DestroyBody(box2dBody);
    }

    public void RemoveDynamicBody(int entity)
    {
        ref var box2dBody = ref _dynamicBodies.Get(entity).box2DBody;
        _b2World.DestroyBody(box2dBody);
    }
}