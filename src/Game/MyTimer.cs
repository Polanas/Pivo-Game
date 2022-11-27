using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class MyTimer
{

    public bool Activated => _activated;

    public float Elapsed { get; private set; }

    private bool _activated;

    public void Update(float deltaTime)
    {
        if (!_activated)
            return;

        Elapsed += deltaTime;
    }

    public void Start() => _activated = true;

    public void Stop() => _activated = false;

    public void ClearStart()
    {
        Clear();
        _activated = true;
    }

    public void ClearStop()
    {
        Clear();
        _activated = false;
    }

    public void Clear() => Elapsed = 0;
}