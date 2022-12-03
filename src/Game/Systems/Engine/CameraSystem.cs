using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class CameraSystem : MySystem
{

    private SharedData _sharedData;

    private Camera _camera;

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _sharedData = systems.GetShared<SharedData>();
        _camera = _sharedData.gameData.camera;

        _camera.zoom = 1f;

        UpdateProjections();
    }

    public void UpdateProjections()
    {
        Layer layer;

        for (int i = 0; i < sharedData.renderData.layersList.Count; i++)
        {
            layer = sharedData.renderData.layersList[i];

            Vector2 camPos = _camera.position;

            if (!layer.pixelated)
            {
                float ratio = MyGameWindow.FullToPixelatedRatio;
                camPos.X = MathF.Floor(camPos.X * ratio) / ratio;
                camPos.Y = MathF.Floor(camPos.Y * ratio) / ratio;
            }

            if (layer.pixelated)
                sharedData.renderData.cameraLayerProjections[layer] = MyMath.CreateCameraMatrix(new Vector2(MathF.Floor(camPos.X * layer.cameraPosModifier + _camera.offset.X),
                                                                                               MathF.Floor(camPos.Y * layer.cameraPosModifier + _camera.offset.Y)),
                                                                                               new Vector2(512));
            else sharedData.renderData.cameraLayerProjections[layer] = MyMath.CreateCameraMatrix(camPos * layer.cameraPosModifier + _camera.offset, (Vector2)MyGameWindow.ScreenSize / MyGameWindow.FullToPixelatedRatio);
        }
    }

    public override void Run(EcsSystems systems)
    {
        UpdateProjections();


        _camera.renderingPosition = new Vector2
        {
            X = MathF.Floor(_camera.position.X * MyGameWindow.FullToPixelatedRatio) / MyGameWindow.FullToPixelatedRatio,
            Y = MathF.Floor(_camera.position.Y * MyGameWindow.FullToPixelatedRatio) / MyGameWindow.FullToPixelatedRatio,
        };
    }
}