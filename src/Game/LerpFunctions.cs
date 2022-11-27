namespace Game;

static class LerpFunctions
{
    public static readonly Func<float, float> Linear = (float t) => t;

    public static readonly Func<float, float> EaseOutQuad = (float t) => 1 - (1 - t) * (1 - t);

    public static readonly Func<float, float> EaseInOutQuad = (float t) => t < .5f ? (2 * t * t) : 1 - MathF.Pow(-2 * t + 2, 2) / 2;

    public static readonly Func<float, float> EaseOutQuint = (float t) => 1f - MathF.Pow(1 - t, 5);

    public static readonly Func<float, float> EaseOutQubic = (float t) => 1f - (1 - t) * (1 - t) * (1 - t);

    public static readonly Func<float, float> EaseOutSine = (float t) => MathF.Sin(t * MathF.PI / 2);
}
