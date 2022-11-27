using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class SetMouseIngamePosSystem : MySystem
{
    public override void Run(EcsSystems systems)
    {
        sharedData.gameData.inGameMousePosition = Utils.ToIngameSpace(Mouse.ScreenPosition);
    }
}
