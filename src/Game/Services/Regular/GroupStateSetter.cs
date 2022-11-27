using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.ExtendedSystems;

namespace Game;

class GroupState : Service, IRegularService
{
    private static SharedData _sharedData;

    private static EcsPool<EcsGroupSystemState> _ecsGroupStates;

    private static EcsFilter _ecsGroupStatesFilter;

    public override void Init(SharedData sharedData, EcsWorld world, ServiceState state)
    {
        base.Init(sharedData, world, state);

        _sharedData = sharedData;

        _ecsGroupStates = world.GetPool<EcsGroupSystemState>();
        _ecsGroupStatesFilter = world.Filter<EcsGroupSystemState>().End();
    }

    public static ref EcsGroupSystemState Get(string groupName, out bool found)
    {
        foreach (var e in _ecsGroupStatesFilter)
        {
            ref var groupState = ref _ecsGroupStates.Get(e);

            if (groupState.Name == groupName)
            {
                found = true;
                return ref groupState;
            }
        }

        found = false;
        return ref _ecsGroupStates.GetRawDenseItems()[0];
    }

    public static void Set(string groupName, bool state)
    {
        foreach (var e in _ecsGroupStatesFilter)
        {
            ref var groupState = ref _ecsGroupStates.Get(e);

            if (groupState.Name != groupName)
                continue;

            if (state == groupState.State)
                return;

            var groupSystems = _sharedData.gameData.groupSystems[groupName];
            groupState.State = state;

            if (state)
            {
                for (int k = 0; k < groupSystems.Length; k++)
                    if (groupSystems[k] is MySystem)
                        ((MySystem)groupSystems[k]).OnGroupActivate();
            }
            else
            {

                for (int k = 0; k < groupSystems.Length; k++)
                    if (groupSystems[k] is MySystem)
                        ((MySystem)groupSystems[k]).OnGroupDiactivate();
            }
        }
    }

    public static void Toggle(string groupName)
    {
        foreach (var e in _ecsGroupStatesFilter)
        {
            ref var groupState = ref _ecsGroupStates.Get(e);

            if (groupState.Name != groupName)
                continue;

            var groupSystems = _sharedData.gameData.groupSystems[groupName];
            groupState.State = !groupState.State;

            if (groupState.State)
            {
                for (int k = 0; k < groupSystems.Length; k++)
                    if (groupSystems[k] is MySystem)
                        ((MySystem)groupSystems[k]).OnGroupActivate();
            }
            else
            {

                for (int k = 0; k < groupSystems.Length; k++)
                    if (groupSystems[k] is MySystem)
                        ((MySystem)groupSystems[k]).OnGroupDiactivate();
            }
        }
    }
}
