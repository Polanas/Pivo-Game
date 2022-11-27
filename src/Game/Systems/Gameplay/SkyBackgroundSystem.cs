using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;

namespace Game;

class SkyBackgroundSystem : MySystem
{

    public static Vector3 TopColor { get; private set; } = new Vector3(35, 186, 194);

    public static Vector3 BottomColor { get; private set; } = new Vector3(0, 153, 219);

    private SkyBackgroundMaterial _material;

    private Vector2 _size;

    public override void Run(EcsSystems systems)
    {
        base.Run(systems);

        var skyBackgroundState = sharedData.eventBus.GetEventBodySingleton<SkyBackgroundState>();

        _material.camY = skyBackgroundState.useCamPosition ? sharedData.gameData.camera.position.Y / 3 : skyBackgroundState.camPosY;
        _material.color1 = skyBackgroundState.topColor;
        _material.color2 = skyBackgroundState.bottomColor;

        Graphics.DrawMaterial(Layer.Back, _material, _size, sharedData.gameData.camera.renderingPosition, -1);
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _size = Utils.FullScreenTextureSize;
        _material = Material.Create<SkyBackgroundMaterial>();

        _material.color1 = TopColor;
        _material.color2 = BottomColor;

        sharedData.eventBus.NewEventSingleton<SkyBackgroundState>();
    }
}