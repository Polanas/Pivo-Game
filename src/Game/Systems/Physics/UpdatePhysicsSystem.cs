using Leopotam.EcsLite.Di;

namespace Game;

class UpdatePhysicsSystem : MySystem
{

    private PhysicsData _physicsData;

    private EcsPoolInject<StaticBody> _staticBodies;

    private EcsPoolInject<DynamicBody> _dynamicBodies;

    private EcsPoolInject<Transform> _transforms;

    private EcsFilterInject<Inc<StaticBody>> _staticBodiesFilter;

    private EcsFilterInject<Inc<DynamicBody>> _dynamicBodiesFilter;

    public override void Run(EcsSystems systems)
    {
        sharedData.physicsData.b2World.DebugDraw();

        var pauseState = sharedData.eventBus.GetEventBodySingleton<PauseState>();
        if (pauseState.paused)
            return;

        _physicsData.b2World.Step(_physicsData.fixedDeltaTime, _physicsData.velocityIterations, _physicsData.positionIterations);

        foreach (var e in _staticBodiesFilter.Value)
        {
            ref var staticBody = ref _staticBodies.Value.Get(e);
            ref var transform = ref _transforms.Value.Get(e);

            var box2DPos = staticBody.body.GetPosition();
            transform.position.X = box2DPos.X / _physicsData.PTM;
            transform.position.Y = box2DPos.Y / _physicsData.PTM;
        }

        foreach (var e in _dynamicBodiesFilter.Value)
        {
            ref var dynamicBody = ref _dynamicBodies.Value.Get(e);
            ref var transform = ref _transforms.Value.Get(e);

            var box2DPos = dynamicBody.box2DBody.GetPosition();

            transform.position.X = box2DPos.X / _physicsData.PTM;
            transform.position.Y = box2DPos.Y / _physicsData.PTM;
        }
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _physicsData = sharedData.physicsData;
    }
}
