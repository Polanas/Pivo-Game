using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;
using Box2DSharp.Common;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;
using System.Data;
using OpenTK.Graphics.ES20;

namespace Game;

public enum PlayerState
{
    Idle,
    Walking,
    Jumping
}

public enum TongueState
{
    Idle,
    Out
}

class PlayerSystem : MySystem
{

    private EcsFilterInject<Inc<Tongue>> _tongueFilter;

    private EcsPoolInject<Transform> _transformsPool;

    private EcsFilterInject<Inc<Player>> _playerFilter;

    private EcsFilterInject<Inc<PlayerInputs>> _playerInputsFilter;

    private EcsCustomInject<LevelsService> _levelsService;

    private StateMachine<PlayerState> _characterStateMachine;

    private StateMachine<TongueState> _tongueStateMachine;

    private Body _playerBody;

    private Sprite _playerSprite;

    private float _speedXMultiplier;

    private const float HORIZONTAL_SPEED = 70;

    private const float JUMP_SPEED = -150;

    private const float MAX_TONGUE_RANGE = 50;

    private readonly Vector2 _tongueOffset = new Vector2(6, -2 + 1 / MyGameWindow.FullToPixelatedRatio);

    private bool _isGrounded;

    private bool _canJump;

    private bool _tongueReachedDestination;

    private string _noTongueAnimation;

    private int _tongueFrame;

    private int _tongueEntity;

    private float _tongueTime;

    private int _jumpCount;

    private float _tongueAngle;

    private CoroutineRunner _coroutineRunner;

    private LerpValue _tongueLerp;

    private PlayerInputs _playerInputs;

    public int PlayerPrefab(Vector2 position)
    {
        sharedData.gameData.camera.position = position;
        float animSpeed = 1f / 8f;

        Sprite playerSprite = new("player", new Vector2i(32), 2, Layer.Middle);
        playerSprite.offset = new Vector2(0, -3 + 1 / MyGameWindow.FullToPixelatedRatio);
        playerSprite
          .AddAnimation("idle", 0, false, new int[1])
          .AddAnimation("idlem1", 0, false, new int[] { 7 })
          .AddAnimation("idlem2", 0, false, new int[] { 14 })
          .AddAnimation("walk", animSpeed, true, 1, 6)
          .AddAnimation("walkm1", animSpeed, true, 8, 13)
          .AddAnimation("walkm2", animSpeed, true, 15, 20)
          .AddAnimation("jump", animSpeed, false, new int[] { 21, 22, 23 })
          .AddAnimation("jumpm1", animSpeed, false, new int[] { 21 + 5, 22 + 5, 23 + 5 })
          .AddAnimation("jumpm2", animSpeed, false, new int[] { 21 + 10, 22 + 10, 23 + 10 })
          .AddAnimation("float", animSpeed, false, new int[] { 23, 22, 25, 24 })
          .AddAnimation("floatm1", animSpeed, false, new int[] { 23 + 5, 22 + 5, 25 + 5, 24 + 5 })
          .AddAnimation("floatm2", animSpeed, false, new int[] { 23 + 10, 22 + 10, 25 + 10, 24 + 10 })
          .AddAnimation("float2", animSpeed, false, new int[] { 25 })
          .AddAnimation("float2m1", animSpeed, false, new int[] { 30 })
          .AddAnimation("float2m2", animSpeed, false, new int[] { 35 })
          .SetAnimation("idle");

        Vector2 playerSize = new Vector2(14 - 1 / MyGameWindow.FullToPixelatedRatio * 2, 10);

        var player = world.NewEntity();
        sharedData.physicsData.physicsFactory.AddDynamicBody(
            player,
            new Transform(position, playerSize),
            new(PhysicsBodyType.Player, world.PackEntity(player)), 1, true, 0, 0.1f);

        world.AddComponent(player, new Renderable(playerSprite));
        world.AddComponent<Player>(player);
        world.AddComponent<PlayerInputs>(player);

        _playerBody = world.GetComponent<DynamicBody>(player).box2DBody;
        _playerSprite = playerSprite;

        ContactListenerEvent onBeginFunc = (Body b1, Body b2) =>
        {
            var userData = b1.GetUserData();
            world.RepackEntity(ref userData.entity, out int e);
            ref var playerC = ref _playerFilter.Pools.Inc1.Get(e);

            playerC.wasGrounded = _isGrounded;
            _isGrounded = true;
        };

        ContactListenerEvent onEndFunc = (Body b1, Body b2) =>
        {
            var userData = b1.GetUserData();
            world.RepackEntity(ref userData.entity, out int e);
            var playerC = _playerFilter.Pools.Inc1.Get(e);

            if (!(_isGrounded && playerC.wasGrounded))
                _isGrounded = false;
        };

        sharedData.physicsData.contcactListener.AddBeginAndEndEvent(onBeginFunc,
                                                                    onEndFunc,
                                                                    PhysicsBodyType.PlayerSensor,
                                                                    PhysicsBodyType.Block, false);

        sharedData.physicsData.physicsFactory.AddSensor(_playerBody, playerSize, new(PhysicsBodyType.PlayerSensor));
        _noTongueAnimation = playerSprite.Current.Value.name;
        var tongueE = Utils.GetSystem<TongueSystem>().TonguePrefab();
        _tongueLerp = new LerpValue(LerpFunctions.EaseOutSine);

        return player;
    }

    private IEnumerator CanJump()
    {
        MyTimer timerInput = new(), timerCooyote = new();
        bool canJump = false, canCoyoteJump = false, wasGrounded = false;

        while (true)
        {
            timerInput.Update(sharedData.physicsData.fixedDeltaTime);
            timerCooyote.Update(sharedData.physicsData.fixedDeltaTime);

            if (!wasGrounded && _isGrounded)
            {
                canCoyoteJump = false;
            }
            else if (!_isGrounded && wasGrounded)
            {
                canCoyoteJump = true;
                timerCooyote.ClearStart();
            }

            if (timerCooyote.Elapsed > .15f)
            {
                canCoyoteJump = false;
                timerCooyote.ClearStop();
            }

            if (_playerInputs.up.Pressed)
            {
                canJump = true;

                timerInput.ClearStart();
            }
            else if (_playerInputs.up.Released)
            {
                canJump = false;
            }

            if (timerInput.Elapsed >= .15f)
            {
                timerInput.ClearStop();
                canJump = false;
            }

            _canJump = canJump && _isGrounded;

            if (canCoyoteJump && canJump)
                _canJump = true;

            wasGrounded = _isGrounded;

            yield return null;
        }
    }

    private PlayerState Idle()
    {
        if (_playerInputs.left.Down != _playerInputs.right.Down)
        {
            _playerSprite.SetAnimation("walk");
            return PlayerState.Walking;
        }

        if (MathHelper.ApproximatelyEquivalent(_playerBody.GetLinearVelocity().Y, 0, .01f) && _playerSprite.Current.Value == "float2")
            _playerSprite.SetAnimation("idle");

        if (_canJump)
            return PlayerState.Jumping;

        return PlayerState.Idle;
    }

    private PlayerState Walking()
    {
        if (_canJump)
            return PlayerState.Jumping;

        bool right = _playerInputs.right.Down;
        bool left = _playerInputs.left.Down;
        var velocity = _playerBody.GetLinearVelocityFromWorldPoint(_playerBody.GetPosition());

        if (right == left && _speedXMultiplier == 0)
        {
            _playerBody.SetLinearVelocity(new System.Numerics.Vector2(0, velocity.Y));
            if (_playerSprite.Current.Value != "float2")
                _playerSprite.SetAnimation("idle");
            return PlayerState.Idle;
        }

        if (velocity.Y > 0.01f && _playerSprite.Current.Value != "float2")
            _playerSprite.SetAnimation("float2");
        if (MathHelper.ApproximatelyEquivalent(velocity.Y, 0, .01f) && (_playerSprite.Current.Value == "float2"))
            _playerSprite.SetAnimation("walk");

        float horizontalSpeed = HORIZONTAL_SPEED * _speedXMultiplier;

        if (_playerSprite.flippedHorizontally)
            horizontalSpeed *= -1;

        if (left)
        {
            _playerBody.SetLinearVelocity(new System.Numerics.Vector2(horizontalSpeed * sharedData.physicsData.PTM, velocity.Y));
            return PlayerState.Walking;
        }

        _playerBody.SetLinearVelocity(new System.Numerics.Vector2(horizontalSpeed * sharedData.physicsData.PTM, velocity.Y));
        return PlayerState.Walking;
    }

    private PlayerState Jumping()
    {
        var velocity = _playerBody.GetLinearVelocity();

        if (_canJump && _jumpCount == 0)
        {
            _jumpCount++;
            _playerBody.SetLinearVelocity(new(velocity.X, 0));
            _playerBody.ApplyLinearImpulseToCenter(new(0, JUMP_SPEED * sharedData.physicsData.PTM), true);
            _playerSprite.SetAnimation("jump");
        }

        if (MathHelper.ApproximatelyEquivalent(velocity.Y, 0, 0.001f) && _isGrounded)
        {
            if (MathHelper.ApproximatelyEquivalent(velocity.X, 0, 0.001f))
            {
                _playerSprite.SetAnimation("idle");
                return PlayerState.Idle;
            }

            _playerSprite.SetAnimation("walk");
            return PlayerState.Walking;
        }

        velocity = _playerBody.GetLinearVelocity();

        float horizontalSpeed = HORIZONTAL_SPEED * sharedData.physicsData.PTM * _speedXMultiplier * Utils.BoolToFloat(_playerInputs.right.Down);

        _playerBody.SetLinearVelocity(new(horizontalSpeed, velocity.Y));

        return PlayerState.Jumping;
    }

    private IEnumerator SetTongueFrame()
    {
        while (true)
        {
            var tongue = _tongueFilter.Pools.Inc1.Get(_tongueEntity);

            if (_playerInputs.useTongue.Pressed && _tongueFrame == 0)
            {
                _tongueFrame = 1;
                yield return .07f;

                _tongueFrame = 2;
            }
            else if (_tongueReachedDestination && _tongueFrame == 2 && tongue.length <= 0)
            {
                _tongueFrame = 1;
                yield return 0.07f;

                _tongueFrame = 0;
            }

            yield return null;
        }
    }

    private IEnumerator JumpingCoroutine()
    {
        bool releasedJump = false;
        bool startedFalling = false;

        _jumpCount = 0;

        while (true)
        {
            if (!_playerInputs.up.Down && !releasedJump)
                releasedJump = true;

            var velocity = _playerBody.GetLinearVelocity();

            if (releasedJump && !startedFalling)
            {
                velocity.Y += 2;
                _playerBody.SetLinearVelocity(velocity);
            }

            if (velocity.Y > 0)
            {
                _playerSprite.SetAnimation("float");
                break;
            }

            yield return null;
        }
    }

    private void OnBeginWalking() => _speedXMultiplier = 0;

    private IEnumerator SetHorizontalSpeedMultiplier()
    {
        while (true)
        {
            if (_playerInputs.left.Down != _playerInputs.right.Down)
                _speedXMultiplier = MathHelper.Clamp(_speedXMultiplier + 1 * sharedData.physicsData.fixedDeltaTime * 10, 0, 1);
            else _speedXMultiplier = MathHelper.Clamp(_speedXMultiplier - 1 * sharedData.physicsData.fixedDeltaTime * 10, 0, 1);

            yield return null;
        }
    }

    public override void Run(EcsSystems systems)
    {
        var pauseState = sharedData.eventBus.GetEventBodySingleton<PauseState>();

        if (pauseState.paused)
            return;

        if (_playerInputs.left.Pressed)
            _playerSprite.flippedHorizontally = true;
        else if (_playerInputs.right.Pressed)
            _playerSprite.flippedHorizontally = false;

        if (_playerInputs.left.Down != _playerInputs.right.Down)
            if (_playerInputs.left.Down && !_playerSprite.flippedHorizontally)
                _playerSprite.flippedHorizontally = true;
            else if (_playerInputs.right.Down && _playerSprite.flippedHorizontally)
                _playerSprite.flippedHorizontally = false;

        foreach (var e in _tongueFilter.Value)
        {
            _tongueEntity = e;
            break;
        }

        foreach (var e in _playerInputsFilter.Value)
        {
            _playerInputs = _playerInputsFilter.Pools.Inc1.Get(e);
            break;
        }

        foreach (var e in _playerFilter.Value)
        {
            ref var player = ref _playerFilter.Pools.Inc1.Get(e);
            player.isGrounded = _isGrounded;

            var transform = _transformsPool.Value.Get(e);

            if (IsPlayerOutOfSubLevel(transform) && !sharedData.eventBus.HasEvents<PlayerOutOfSubLevel>())
            {
                ref var playerOutOfSubLevel = ref sharedData.eventBus.NewEvent<PlayerOutOfSubLevel>();
                playerOutOfSubLevel.playerTransform = transform;
                playerOutOfSubLevel.direction = (sbyte)(_playerBody.GetLinearVelocity().X < 0 ? -1 : 1);

                _playerInputs.SetEmptyKeyData();
                _playerInputs.SetDirectionKeyData(playerOutOfSubLevel.direction);
                _coroutineRunner.Run(TransitionCoroutine());
            }

            break;
        }

        _playerSprite.SetAnimation(_noTongueAnimation, true);

        _characterStateMachine.Update(sharedData.physicsData.fixedDeltaTime);


        _coroutineRunner.Update(sharedData.physicsData.fixedDeltaTime);

        var current = _playerSprite.Current.Value.name;

        _noTongueAnimation = current;

        if (_tongueFrame > 0)
            _playerSprite.SetAnimation($"{current}m{_tongueFrame}", true);

        _tongueStateMachine.Update(sharedData.physicsData.fixedDeltaTime);
    }

    private bool IsPlayerOutOfSubLevel(Transform playerTransform)
    {
        if (!Collision.RectangleInside(playerTransform, _levelsService.Value.CurrentSubLevelTransform))
            return true;

        return false;
    }

    private TongueState TongueIdle()
    {
        if (_tongueFrame == 2)
            return TongueState.Out;

        ref Tongue tongue = ref _tongueFilter.Pools.Inc1.Get(_tongueEntity);
        tongue.sensor.SetPosition(_playerBody.GetPixelatedPosition());

        return TongueState.Idle;
    }

    private IEnumerator TransitionCoroutine()
    {

        PlayerOutOfSubLevel playerOutOfSubLevel = default;
        int playerOutOfSubLevelE = 0;
        EcsPool<PlayerOutOfSubLevel> playerOutOfSubLevelPool = null;
        bool wasGrounded = false;

        foreach (var e in sharedData.eventBus.GetEventBodies<PlayerOutOfSubLevel>(out var pool))
        {
            playerOutOfSubLevel = pool.Get(e);
            playerOutOfSubLevelE = e;
            playerOutOfSubLevelPool = pool;
        }

        while (true)
        {
            if (!sharedData.eventBus.HasEvents<PlayerOutOfSubLevel>())
                yield break;

            foreach (var e in sharedData.eventBus.GetEventBodies<PlayerOutOfSubLevel>(out var pool))
                playerOutOfSubLevel = pool.Get(e);

            if (playerOutOfSubLevel.oldTilesRemoved)
            {
                bool a = _isGrounded;
            }

            wasGrounded = _isGrounded;

            yield return null;
        }
    }

    //  private void SetPlayerOutOfSubLevelGroundedState(bool isGrounded, int entity, EcsPool<PlayerOutOfSubLevel> playerPool) =>
    //    playerPool.Get(entity).isGrounded = isGrounded;


    private TongueState TongueOut()
    {
        ref Tongue tongue = ref _tongueFilter.Pools.Inc1.Get(_tongueEntity);
        tongue.rendered = true;

        var playerPos = _playerBody.GetPixelatedPosition();
        tongue.angle = _tongueAngle;

        if (tongue.angle > 0)
            tongue.angle = MathHelper.Clamp(tongue.angle, 135.001f, 180);
        else tongue.angle = MathHelper.Clamp(tongue.angle, -180, -135);

        float flippedHorizontally = Utils.BoolToFloat(!_playerSprite.flippedHorizontally);

        Vector2 sensorPos, tonguePos = playerPos;
        Vector2 tongueOffset = _tongueOffset;
        tongueOffset.X *= flippedHorizontally;

        if (tongue.angle < 0)
        {
            tonguePos.Y -= 1;
            tonguePos.Y -= MathHelper.Lerp(0.5f, 1, (tongue.angle + 180) / 45);
        }
        else
        {
            tonguePos.Y -= MathHelper.Lerp(0, .5f, (tongue.angle - 135.001f) / 45);
        }

        tongue.flipped = _playerSprite.flippedHorizontally;

        tonguePos.X += tongue.bottomMaterial.QuadSize.X / 2 * flippedHorizontally;
        tonguePos += tongueOffset;

        tongue.position = tonguePos;

        if (tongue.r_collided || tongue.length >= MAX_TONGUE_RANGE)
            _tongueReachedDestination = true;

        tongue.length = _tongueLerp.Value * MAX_TONGUE_RANGE;

        sensorPos = tonguePos;
        sensorPos.X += tongue.bottomMaterial.QuadSize.X / 2 * -flippedHorizontally;

        float sin = MathF.Sin(MathHelper.DegreesToRadians(tongue.angle));
        float cos = MathF.Cos(MathHelper.DegreesToRadians(tongue.angle));

        sensorPos.X += tongue.length * -cos * flippedHorizontally;
        sensorPos.Y += tongue.length * -sin;

        tongue.sensor.SetPosition(sensorPos);

        if (_tongueFrame == 1)
        {
            tongue.length = 0;
            tongue.rendered = false;
            _tongueReachedDestination = false;

            return TongueState.Idle;
        }

        return TongueState.Out;
    }

    private IEnumerator TongueOutCoroutine()
    {
        _tongueTime = 0;
        bool timeInverted = false;

        while (true)
        {
            _tongueTime += sharedData.physicsData.fixedDeltaTime * Utils.BoolToFloat(!timeInverted) * 3f;

            if (!timeInverted && (_tongueTime > 1f || _tongueReachedDestination))
            {
                timeInverted = true;
            }

            _tongueLerp.Run(_tongueTime);

            yield return null;
        }
    }

    private void TongueOutBegin()
    {
        var playerPos = _playerBody.GetPixelatedPosition();
        var mousePos = sharedData.gameData.ingameMousePosition;
        if (_playerSprite.flippedHorizontally)
            mousePos = mousePos.Mirrored(new Vector2(playerPos.X, mousePos.Y));


        _tongueAngle = -MyMath.AngleBetweenPoints(mousePos, playerPos);
        _tongueLerp.Reset();
    }

    public override void OnGroupActivate()
    {
        Vector2 position = sharedData.eventBus.GetEventBodySingleton<SpawnPoint>().position;
        PlayerPrefab(position);

        sharedData.eventBus.DestroyEventSingleton<SpawnPoint>();

        _characterStateMachine = new(true);

        _characterStateMachine.SetCallBacks(PlayerState.Idle, Idle);
        _characterStateMachine.SetCallBacks(PlayerState.Walking, Walking, OnBeginWalking, null, null);
        _characterStateMachine.SetCallBacks(PlayerState.Jumping, Jumping, null, null, JumpingCoroutine);
        _characterStateMachine.RunCoroutine(CanJump);
        _characterStateMachine.RunCoroutine(SetHorizontalSpeedMultiplier);

        _tongueStateMachine = new(true);
        _tongueStateMachine.SetCallBacks(TongueState.Idle, TongueIdle);
        _tongueStateMachine.SetCallBacks(TongueState.Out, TongueOut, TongueOutBegin, null, TongueOutCoroutine);

        GroupState.Set("PlayerCamera", true);

        _coroutineRunner = new();
        _coroutineRunner.Run(SetTongueFrame());

        base.OnGroupActivate();
    }

    public override void OnGroupDiactivate()
    {
        world.DelEntity(world.GetEntitiyWithComponent<Player>());

        GroupState.Set("PlayerCamera", false);

        base.OnGroupDiactivate();
    }
}