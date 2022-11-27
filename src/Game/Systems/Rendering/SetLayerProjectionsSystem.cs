using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class SetLayerProjectionsSystem : MySystem
{

    private Camera _camera;

    private Matrix4 _projection;

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _camera = sharedData.gameData.camera;
        _projection = MyMath.CreateCameraMatrix(MyGameWindow.ScreenSize / 2, MyGameWindow.ScreenSize);
    }

    public override void Run(EcsSystems systems)
    {
        Layer layer;
        Matrix4 model;

        for (int i = 0; i < sharedData.renderData.layersList.Count; i++)
        {
            layer = sharedData.renderData.layersList[i];

            if (layer.pixelated)
            {
                Vector2 camPos = (_camera.position + _camera.offset) * layer.cameraPosModifier;

                float ratio = MyGameWindow.FullToPixelatedRatio;

                camPos.X = MathF.Floor(camPos.X * ratio) / ratio;
                camPos.Y = MathF.Floor(camPos.Y * ratio) / ratio;

                Vector2 fracCamPos = new Vector2
                {
                    X = camPos.X - MathF.Floor(camPos.X),
                    Y = camPos.Y - MathF.Floor(camPos.Y),
                };

                if (layer.angle != 0.0f)
                    fracCamPos = fracCamPos.Rotate(layer.angle, Vector2.Zero);

                fracCamPos.Y *= -1;
                fracCamPos *= MyGameWindow.FullToPixelatedRatio;

                model = MyMath.CreateTransformMatrix(MyGameWindow.ScreenSize / 2 - fracCamPos, new Vector2(512) * MyGameWindow.FullToPixelatedRatio, layer.angle);
            }
            else model = MyMath.CreateTransformMatrix(MyGameWindow.ScreenSize / 2, MyGameWindow.ScreenSize, layer.angle);

            sharedData.renderData.layerProjections[layer] = model * _projection;
        }
    }
}