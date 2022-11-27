using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class BenSystem : MySystem
{

    private Sprite _benSprite;

    public override void Run(EcsSystems systems)
    {
        Graphics.DrawSprite(_benSprite);
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _benSprite = new("ben", new Vector2i(489, 896));
        _benSprite.depth = 5;
        _benSprite.scale = .1f;
        _benSprite.position = new Vector2(-72, -65);
    }
}
