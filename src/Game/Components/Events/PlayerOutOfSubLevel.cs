namespace Game;

struct PlayerOutOfSubLevel : IEventReplicant
{
    public bool transitionStarted;

    public bool levelsSwitched;

    public float time;

    public Transform playerTransform;

    public sbyte direction;

    public bool oldTilesRemoved;
}
