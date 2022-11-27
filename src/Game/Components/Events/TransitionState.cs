using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

struct TransitionState : IEventReplicant
{
    public bool shouldCover;

    public Vector2 coverPos;

    public TransitionState(bool shouldCover, Vector2 coverPos)
    {
        this.shouldCover = shouldCover;
        this.coverPos = coverPos;
    }
}
