using System;

namespace Game;

class LerpPosition
{

    private LerpValue _lerp;

    public Vector2 StartPosition { get; set; }

    public Vector2 EndPosition { get; set; }

    public Vector2 ResultPosition { get; private set; }

    public LerpPosition(LerpValue lerp, Vector2 startPosition, Vector2 endPosition)
    {
        _lerp = lerp;
        StartPosition = startPosition;
        EndPosition = endPosition;
    }

    public void Update(float deltaTime)
    {
        _lerp.Update(deltaTime);
        float value = _lerp.Value;

        ResultPosition = Vector2.Lerp(StartPosition, EndPosition, value);
    }

    public void Run(float time)
    {
        _lerp.Run(time);
        float value = _lerp.Value;

        ResultPosition = Vector2.Lerp(StartPosition, EndPosition, value);
    }

    public void Reset() => _lerp.Reset();
}
