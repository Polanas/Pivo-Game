using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

struct SkyBackgroundState : IEventSingleton
{
    public Vector3 topColor;

    public Vector3 bottomColor;

    public float camPosY;

    public bool useCamPosition;
}
