using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class PauseSystem : MySystem
{

    private Sprite _pauseSprite;

    private const float MAX_ALPHA = .95f;

    private const float MIN_ALPHA = 0;

    public override void Run(EcsSystems systems)
    {
        ref var pauseState = ref sharedData.eventBus.GetEventBodySingleton<PauseState>();

        if (Keyboard.Pressed(Keys.Escape)) //TODO: save this in inputs
        {
            pauseState.paused = !pauseState.paused;
            sharedData.eventBus.NewEventExt(new PauseStateChanged() { r_paused = pauseState.paused });
        }

        if (pauseState.paused || (!pauseState.paused && _pauseSprite.alpha > MIN_ALPHA))
            Graphics.DrawSprite(_pauseSprite);

        if (pauseState.paused && _pauseSprite.alpha < MAX_ALPHA)
            _pauseSprite.alpha = MathF.Min(_pauseSprite.alpha + 0.1f, MAX_ALPHA);
        else if (!pauseState.paused && _pauseSprite.alpha > MIN_ALPHA)
            _pauseSprite.alpha = MathF.Max(_pauseSprite.alpha - 0.1f, MIN_ALPHA);
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        Vector2 texSize = (Vector2)MyGameWindow.ScreenSize / MyGameWindow.FullToPixelatedRatio;
        Vector2i itexSize = new Vector2i((int)MathF.Round(texSize.X), (int)MathF.Round(texSize.Y));

        _pauseSprite = new("pause", itexSize, 1, Layer.UI);
        _pauseSprite.size = itexSize;
        _pauseSprite.alpha = MIN_ALPHA;

        sharedData.eventBus.NewEventSingleton<PauseState>();
    }
}
