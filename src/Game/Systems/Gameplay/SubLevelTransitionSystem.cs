using ldtk;
using Leopotam.EcsLite.Di;
using OpenTK.Graphics.ES11;
using OpenTK.Graphics.ES20;
using System;

namespace Game;

enum SubLevelTransitionState
{
    Idle,
    Transitioning,
    ChangingScreen
}

class SubLevelTransitionSystem : MySystem
{

    private EcsCustomInject<LevelsService> _levelService;

    private EcsFilterInject<Inc<Transform, Player>> _playerFilter;

    private SubLevelTransition1Material _material;

    private StateMachine<SubLevelTransitionState> _stateMachine;

    private const float TRANSITION_FINISH_TIME = .85f;

    private LerpValue _lerpValue;

    private float _time;

    private int _playerEntity;

    private Random _rand;

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _material = Material.Create<SubLevelTransition1Material>(Utils.FullScreenTextureSize);
        _stateMachine = new();
        _stateMachine.SetCallBacks(SubLevelTransitionState.Idle, Idle);
        _stateMachine.SetCallBacks(SubLevelTransitionState.Transitioning, Transitioning, TransitioningBegin);
        _stateMachine.SetCallBacks(SubLevelTransitionState.ChangingScreen, ChangingScreen, ChangingScreenBegin);

        _lerpValue = new(LerpFunctions.EaseOutQuad);
        _rand = new();
    }

    public override void Run(EcsSystems systems)
    {
        if (sharedData.eventBus.HasEvents<PlayerOutOfSubLevel>())
            StartTransition();

        _stateMachine.Update(sharedData.physicsData.deltaTime);
    }

    private void StartTransition()
    {
        foreach (var e in sharedData.eventBus.GetEventBodies<PlayerOutOfSubLevel>(out var pool))
        {
            ref var playerOutOfSubLevel = ref pool.Get(e);

            if (playerOutOfSubLevel.transitionStarted)
                break;

            playerOutOfSubLevel.transitionStarted = true;
            _stateMachine.ForceState(SubLevelTransitionState.Transitioning);
        }
    }

    private SubLevelTransitionState Idle()
    {
        return SubLevelTransitionState.Idle;
    }

    private SubLevelTransitionState Transitioning()
    {
        UpdateMaterial(sharedData.physicsData.fixedDeltaTime);

        Graphics.DrawMaterial(sharedData.renderData.layers["front"], _material, sharedData.gameData.camera.renderingPosition, -1);

        if (_time > TRANSITION_FINISH_TIME)
            return SubLevelTransitionState.ChangingScreen;

        return SubLevelTransitionState.Transitioning;
    }

    private void TransitioningBegin()
    {
        _material.rand = _rand.NextSingle();
        _time = 0;
        _material.reverse = false;
    }

    private SubLevelTransitionState ChangingScreen()
    {
        UpdateMaterial(sharedData.physicsData.fixedDeltaTime);
        Graphics.DrawMaterial(sharedData.renderData.layers["front"], _material, sharedData.gameData.camera.renderingPosition, -1);

        if (_time >= TRANSITION_FINISH_TIME)
        {
            sharedData.eventBus.DestroyEvents<PlayerOutOfSubLevel>();
            return SubLevelTransitionState.Idle;
        }

        return SubLevelTransitionState.ChangingScreen;
    }

    private void ChangingScreenBegin()
    {
        _playerEntity = world.GetEntitiyWithComponent<Player>();
        ref var body = ref world.GetComponent<DynamicBody>(_playerEntity);

        foreach (var e in sharedData.eventBus.GetEventBodies<PlayerOutOfSubLevel>(out var pool))
        {
            ref var playerOutOfSubLevel = ref pool.Get(e);
            var newSubLevel = FindClosestSubLevel(playerOutOfSubLevel.playerTransform, out Vector2 direction);
            body.box2DBody.SetPosition(playerOutOfSubLevel.playerTransform.position + (playerOutOfSubLevel.playerTransform.size * 1.2f * direction));

            _levelService.Value.SetSublevel(newSubLevel);

            playerOutOfSubLevel.levelsSwitched = true;
        }

        _material.reverse = true;
        _time = _material.tm = -.05f;
    }

    private void UpdateMaterial(float time)
    {
        _time += time * 1.5f;
        _lerpValue.Run(_time);
        _material.tm = _lerpValue.Value;
    }

    private Sublevel FindClosestSubLevel(Transform playerTransform, out Vector2 direction)
    {
        var neighbours = _levelService.Value.NeighourSubLevels;

        if (neighbours.Length == 1)
        {
            var subLevel = _levelService.Value.SubLevelsByIIDs[neighbours[0].LevelIid];
            direction = GetSubLevelDirection(neighbours[0].Dir);
            return subLevel;
        }

        Sublevel closestSubLevel = null;
        float closestDist = float.MaxValue;
        string dir = null;

        foreach (var neighbour in neighbours)
        {
            var sublevel = _levelService.Value.SubLevelsByIIDs[neighbour.LevelIid];

            Vector2 pos = new Vector2(sublevel.WorldX, sublevel.WorldY);
            Vector2 size = new Vector2(sublevel.PxWid, sublevel.PxHei);
            pos += size / 2;

            var levelTransform = new Transform(pos, size);
            if (!Collision.Rectangle(levelTransform, playerTransform))
                continue;

            float left = pos.X - size.X / 2;
            float right = pos.X + size.X / 2;
            float top = pos.Y - size.Y / 2;
            float bottom = pos.Y + size.Y / 2;

            Vector2 topLeft = new Vector2(left, top);
            Vector2 topRight = new Vector2(right, top);
            Vector2 bottomLeft = new Vector2(left, bottom);
            Vector2 bottomRight = new Vector2(right, bottom);

            float d1, d2;

            Vector2 playerPos = playerTransform.position;

            d1 = Vector2.Distance(playerPos, topLeft);
            d2 = Vector2.Distance(playerPos, topRight);
            if (d2 < d1) d1 = d2;

            d2 = Vector2.Distance(playerPos, bottomLeft);
            if (d2 < d1) d1 = d2;

            d2 = Vector2.Distance(playerPos, bottomRight);
            if (d2 < d1) d1 = d2;

            if (d1 < closestDist)
            {
                closestDist = d1;
                closestSubLevel = sublevel;
                dir = neighbour.Dir;
            }
        }

        direction = GetSubLevelDirection(dir);
        return closestSubLevel;
    }

    private Vector2 GetSubLevelDirection(string dir) =>
         dir switch
         {
             "n" => new Vector2(0, -1),
             "w" => new Vector2(-1, 0),
             "s" => new Vector2(0, -1),
             "e" => new Vector2(1, 0),
             _ => Vector2.Zero
         };
}