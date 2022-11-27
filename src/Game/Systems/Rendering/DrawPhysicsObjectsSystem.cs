using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;
using Box2DSharp.Common;

namespace Game;

class DrawPhysicsObjectsSystem : MySystem
{

    private EcsPoolInject<Transform> _transforms;

    private EcsPoolInject<StaticBody> _staticBodies;

    private EcsPoolInject<DynamicBody> _dynamicBodies;

    private DebugDrawer _drawer;

    private PhysicsData _physicsData;

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _drawer = sharedData.renderData.debugDrawer;
        _physicsData = sharedData.physicsData;
    }

    public override void Run(EcsSystems systems)
    {
        System.Numerics.Vector2 vec2;
        Vector2 pos;

        foreach (var e in world.Filter<StaticBody>().End())
        {
            ref var staticBody = ref _staticBodies.Value.Get(e);
            ref var transform = ref _transforms.Value.Get(e);

            vec2 = staticBody.body.GetPosition();
            pos.X = vec2.X / _physicsData.PTM;
            pos.Y = vec2.Y / _physicsData.PTM;

            _drawer.DrawRect(pos, transform.size, new Vector3i(255));
        }

        foreach (var e in world.Filter<DynamicBody>().End())
        {
            ref var dynamicBody = ref _dynamicBodies.Value.Get(e);
            ref var transform = ref _transforms.Value.Get(e);

            vec2 = dynamicBody.box2DBody.GetPosition();
            pos.X = vec2.X / _physicsData.PTM + _physicsData.PTM;
            pos.Y = vec2.Y / _physicsData.PTM + _physicsData.PTM;
            float anlge = MathHelper.RadiansToDegrees(dynamicBody.box2DBody.GetAngle());

            _drawer.DrawRect(pos, transform.size, new Vector3i(0, 255, 0), MathHelper.RadiansToDegrees(dynamicBody.box2DBody.GetAngle()));

            if (dynamicBody.additionalObjects != null)
            {
                foreach (var additionalObj in dynamicBody.additionalObjects)
                {
                    _drawer.DrawRect(pos + additionalObj.localPosition.Rotate(anlge, Vector2.Zero), additionalObj.size, new Vector3i(0, 255, 0), anlge);
                }
            }
        }
    }
}