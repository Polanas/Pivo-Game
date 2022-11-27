using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class TimeSystem : MySystem
{

    public override void Run(EcsSystems systems)
    {
        var pausedState = sharedData.eventBus.GetEventBodySingleton<PauseState>();

        if (!pausedState.paused)
        {
            sharedData.physicsData.time += sharedData.physicsData.deltaTime;
            sharedData.physicsData.deltaTime = (float)sharedData.renderData.frameEventArgs.Time;
            sharedData.physicsData.fixedDeltaTime = 1f / MyGameWindow.RefreshRate;
        }
        else
        {
            sharedData.physicsData.fixedDeltaTime = 0;
            sharedData.physicsData.deltaTime = 0;
        }
    }
}
