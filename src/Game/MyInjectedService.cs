using Box2DSharp.Dynamics;
using Leopotam.EcsLite.Di;

namespace Game;

class MyInjectableService : IEcsService
{

    private bool _wasInited;

    protected EcsWorld world;

    protected SharedData sharedData;

    public virtual void Init(EcsSystems systems)
    {
        _wasInited = true;

        world = systems.GetWorld();
        sharedData = systems.GetShared<SharedData>();
    }

    public bool WasInited() => _wasInited;
}
