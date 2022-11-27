using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;

namespace Game;

class CursorSystem : MySystem
{

    private Sprite _cursorSprite;

    private EcsFilterInject<Inc<Cursor>> _cursor;

    private CoroutineRunner _runner = new();

    public override void Run(EcsSystems systems)
    {
        _cursorSprite.position.X += 1;
        _cursorSprite.position = Mouse.ScreenPositionCentered;
        _runner?.Update(sharedData.physicsData.deltaTime);
    }

    public IEnumerator FadeIn()
    {
        while (true)
        {
            if (_cursorSprite.alpha >= 1)
            {
                _cursorSprite.alpha = 1;
                _runner.StopAll();
                _runner = null;
            }

            _cursorSprite.alpha += 0.06f;
            yield return null;
        }
    }

    public override void OnGroupActivate()
    {
        _cursorSprite = new Sprite("cursor");
        _cursorSprite.layer = Layer.UI;
        _cursorSprite.alpha = 0;
        _cursorSprite.depth = 2;

        int e = world.NewEntity();
        world.AddComponent(e, new Renderable(_cursorSprite));
        world.AddComponent(e, new Cursor());

        Run(null);
        _runner.Run(FadeIn());
    }

    public override void OnGroupDiactivate()
    {
        foreach (var e in _cursor.Value)
        {
            world.DelEntity(e);
            return;
        }
    }
}
