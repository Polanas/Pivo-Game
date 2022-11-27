using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

enum PauseStateEnum
{
    Paused,
    Resumed
}


class PauseMenuSystem : MySystem
{

    private StateMachine<PauseStateEnum> _stateMachine;
 
    private PauseState _pauseState;

    public override void Run(EcsSystems systems)
    {
        _pauseState = sharedData.eventBus.GetEventBodySingleton<PauseState>();

        _stateMachine.Update(sharedData.physicsData.deltaTime);
    }

    public PauseStateEnum OnPause()
    {
        if (!_pauseState.paused)
            return PauseStateEnum.Resumed;

        return PauseStateEnum.Paused;
    }

    public PauseStateEnum OnResume()
    {
        if (_pauseState.paused)
            return PauseStateEnum.Paused;

        return PauseStateEnum.Resumed;
    }

    public override void OnGroupActivate()
    {
        base.OnGroupActivate();

        _stateMachine = new(true);

        _stateMachine.SetCallBacks(PauseStateEnum.Paused, OnPause);
        _stateMachine.SetCallBacks(PauseStateEnum.Resumed, OnResume);

     //   _guiHandler = new();
    }
}
