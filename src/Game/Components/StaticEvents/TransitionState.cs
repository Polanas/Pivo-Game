using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

struct StaticTransitionState : IEventSingleton
{
    public bool r_covered;

    public bool r_uncovered;

    public float r_coverLevel;
}
