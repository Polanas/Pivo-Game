using System;

namespace Game;

class LerpValue
{

    public float Elapsed { get; private set; }

    public float Value { get; private set; } 

    private Func<float, float> _lerpFunction;

    public LerpValue(Func<float, float> lerpFunction)
    {
        _lerpFunction = lerpFunction;
    }

    public void Update(float deltaTime)
    {
        Elapsed = Math.Min(1, Elapsed + deltaTime);

        Value = _lerpFunction(Elapsed);
    }

    public void Run(float time) => Value = _lerpFunction(time);

    public void Reset() => Elapsed = 0;
}
