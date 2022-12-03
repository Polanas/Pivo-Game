using Leopotam.EcsLite.Di;
using System;

namespace Game;

class PlayerControllerSystem : MySystem
{

    private EcsFilterInject<Inc<PlayerInputs>> _playerInputsFilter;

    public override void Run(EcsSystems systems)
    {
        foreach (var e in _playerInputsFilter.Value)
        {
            ref var playerInputs = ref _playerInputsFilter.Pools.Inc1.Get(e);

            if (!sharedData.eventBus.HasEvents<PlayerOutOfSubLevel>())
            {
                playerInputs.SetDefaultKeyData();
                return;
            }

            foreach (var e1 in sharedData.eventBus.GetEventBodies<PlayerOutOfSubLevel>(out var pool))
            {
                var playerOutOfSubLevel = pool.Get(e1);

                if (playerOutOfSubLevel.time < .5f)
                {
                    playerInputs.SetEmptyKeyData();
                    playerInputs.SetDirectionKeyData(playerOutOfSubLevel.direction);
                    return;
                }

                playerInputs.SetDefaultKeyData();
            }
        }
    }
}
