
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class InitLayersSystem : IEcsInitSystem
{
    public void Init(EcsSystems systems)
    {
        var sharedData = systems.GetShared<SharedData>();

        sharedData.renderData.graphics.AddLayer(new("ui", MyGameWindow.ScreenSize, false, 2, 0, true));
        sharedData.renderData.graphics.AddLayer(new("front", MyGameWindow.ScreenSize, false, 1));
        sharedData.renderData.graphics.AddLayer(new("middlePixelized", new Vector2i(512), true, 0));
        sharedData.renderData.graphics.AddLayer(new("middle", MyGameWindow.ScreenSize, false, -1));
        sharedData.renderData.graphics.AddLayer(new("backPixelized", new Vector2i(512), true, -2));
        sharedData.renderData.graphics.AddLayer(new("back", MyGameWindow.ScreenSize, false, -3));

        Layer.InitLayers(
            sharedData.renderData.layers["back"],
            sharedData.renderData.layers["middle"],
            sharedData.renderData.layers["backPixelized"],
            sharedData.renderData.layers["middlePixelized"],
            sharedData.renderData.layers["ui"]);
    }
}