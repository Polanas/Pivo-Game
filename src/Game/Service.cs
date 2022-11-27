namespace Game;

enum ServiceState
{
    Regular,
    System
}

/// <summary>
/// For services that use other systems
/// </summary>
interface ISystemService
{
}

interface IRegularService
{

}

class Service
{
    protected SharedData sharedData;

    protected EcsWorld world;

    public virtual void Init(SharedData sharedData, EcsWorld world, ServiceState state)
    {
        this.sharedData = sharedData;
        this.world = world;
    }
}
