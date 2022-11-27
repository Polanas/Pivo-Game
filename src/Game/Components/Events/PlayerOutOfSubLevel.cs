namespace Game;

struct PlayerOutOfSubLevel : IEventReplicant
{
    public bool transitionStarted;

    public bool levelsSwitched;

    public Transform playerTransform;

    public sbyte direction;
}
