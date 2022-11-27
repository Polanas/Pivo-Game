using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;

namespace Game;

class CameraFollowPlayerSystem : MySystem
{

    private EcsFilterInject<Inc<Player>> _playersFilter;

    private EcsPoolInject<Transform> _transforms;

    private EcsPoolInject<DynamicBody> _dynamicBodies;

    private Camera _camera;

    private EcsCustomInject<LevelsService> _levelsService;

    public override void Run(EcsSystems systems)
    {
        if (sharedData.eventBus.GetEventBodySingleton<PauseState>().paused || _levelsService.Value.CurrentSubLevel == null)
            return;

        var size = _levelsService.Value.SubLevelSize;
        var pos = _levelsService.Value.SubLevelPos;

        Vector2 minPos = pos - size / 2 + _camera.Size / 2;
        Vector2 maxPos = pos + size / 2 - _camera.Size / 2;

        foreach (var e in _playersFilter.Value)
        {
            var transform = _transforms.Value.Get(e);
            var body = _dynamicBodies.Value.Get(e).box2DBody;

            Vector2 followPos = transform.position;
            CheckIfPlayerOutOfSubLevel(ref followPos);

            followPos += (Mouse.ScreenPosition / 16f) - transform.size / 2;

            float additionalAmount = body.GetLinearVelocity().Y;

            _camera.position = _camera.position.LerpDist(followPos, Vector2.Distance(_camera.position, followPos) / (5 - (additionalAmount / 80f)));

            _camera.position.X = Math.Clamp(_camera.position.X, minPos.X, maxPos.X);
            _camera.position.Y = Math.Clamp(_camera.position.Y, minPos.Y, maxPos.Y);
        }
    }

    private void CheckIfPlayerOutOfSubLevel(ref Vector2 position)
    {
        if (sharedData.eventBus.HasEvents<PlayerOutOfSubLevel>())
            foreach (var e in sharedData.eventBus.GetEventBodies<PlayerOutOfSubLevel>(out var pool))
            {
                var playerOutOfSubLevel = pool.Get(e);

                if (!playerOutOfSubLevel.levelsSwitched)
                    position = playerOutOfSubLevel.playerTransform.position;
            }
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _camera = sharedData.gameData.camera;
    }
}