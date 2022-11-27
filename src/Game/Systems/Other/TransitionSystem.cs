using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

enum CoveringState
{
    On,
    Off,
    Idle
}

class TransitionSystem : MySystem
{

    private TransitionMaterial _transitionMaterial;

    private const float MIN_COVER = 0.75f;

    private const float MAX_COVER = 0;

    private StateMachine<CoveringState> _stateMachine;

    public override void Run(EcsSystems systems)
    {
        ref var transitionState = ref sharedData.eventBus.GetEventBodySingleton<StaticTransitionState>();
        transitionState.r_covered = _transitionMaterial.coverAmount <= MAX_COVER;
        transitionState.r_uncovered = _transitionMaterial.coverAmount >= MIN_COVER;

        transitionState.r_coverLevel = MyMath.Clamp(_transitionMaterial.coverAmount, 0, MIN_COVER);

        TransitionState setTransitionState = default(TransitionState);
        if (sharedData.eventBus.HasEvents<TransitionState>())
        {
            var transitionStateFilter = sharedData.eventBus.GetEventBodies<TransitionState>(out var pool);
            foreach (var e in transitionStateFilter)
            {
                setTransitionState = pool.Get(e);
                pool.Del(e);
                break;
            }

            if (setTransitionState.shouldCover)
                _stateMachine.ForceState(CoveringState.On);
            else _stateMachine.ForceState(CoveringState.Off);

            _transitionMaterial.coverPos = setTransitionState.coverPos;
        }

        _stateMachine.Update(sharedData.physicsData.deltaTime);


        if (!transitionState.r_uncovered)
            Graphics.DrawMaterial(Layer.UI, _transitionMaterial, Vector2.Zero, 2.5f);
    }

    public CoveringState Covering()
    {
        _transitionMaterial.coverAmount = MathF.Max(_transitionMaterial.coverAmount - 0.03f, MAX_COVER);

        if (_transitionMaterial.coverAmount > MAX_COVER)
            return CoveringState.On;

        return CoveringState.Idle;
    }

    public CoveringState Uncovering()
    {
        _transitionMaterial.coverAmount = Math.Min(_transitionMaterial.coverAmount + 0.03f, MIN_COVER);

        if (_transitionMaterial.coverAmount < MIN_COVER)
            return CoveringState.Off;

        return CoveringState.Idle;
    }

    public CoveringState Idle()
    {
        _transitionMaterial.coverAmount = sharedData.eventBus.GetEventBodySingleton<StaticTransitionState>().r_covered ? 0 : 10;
        return CoveringState.Idle;
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        sharedData.eventBus.NewEventSingleton<StaticTransitionState>();
        _transitionMaterial = Material.Create<TransitionMaterial>(Utils.FullScreenTextureSize);

        _transitionMaterial.coverAmount = MIN_COVER;

        _stateMachine = new();

        _stateMachine.SetCallBacks(CoveringState.On, Covering,
            () => _transitionMaterial.coverAmount = MIN_COVER);
        _stateMachine.SetCallBacks(CoveringState.Off, Uncovering,
            () => _transitionMaterial.coverAmount = MAX_COVER);
        _stateMachine.SetCallBacks(CoveringState.Idle, Idle);

        _stateMachine.ForceState(CoveringState.Idle);
    }
}
