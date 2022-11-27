namespace Game;

abstract class MySystem : IEcsRunSystem, IEcsInitSystem, IEcsDestroySystem, IEcsSystem
{

    public bool CurrentGroupState { set => groupActive = value; }

    protected SharedData sharedData;

    protected EcsWorld world;

    protected bool groupActive;

    public virtual void Init(EcsSystems systems)
    {
        world = systems.GetWorld();
        sharedData = systems.GetShared<SharedData>();
    }

    public virtual void Run(EcsSystems systems) { }

    public virtual void Destroy(EcsSystems systems) { }

    public virtual void OnGroupDiactivate() { groupActive = false; }

    public virtual void OnGroupActivate() { groupActive = true; }
}