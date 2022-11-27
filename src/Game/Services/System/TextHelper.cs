using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class TextHelper : Service, ISystemService
{
    private static TextHelper _instance;

    private RenderTextSystem _textSystem;

    public override void Init(SharedData sharedData, EcsWorld world, ServiceState state)
    {
        base.Init(sharedData, world, state);

        _textSystem = Utils.GetSystem<RenderTextSystem>();
    }

    public static Vector2 GetTextSize(string text, string fontName, float scale)
        => _instance._textSystem.GetTextSize(text, fontName, scale);
    
}
