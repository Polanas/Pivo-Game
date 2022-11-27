using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;

namespace Game;

class SharedData : IGameData
{
 
    public readonly GameData gameData;

    public readonly RenderData renderData;

    public readonly PhysicsData physicsData;

    public readonly NetworkData networkData;

    public readonly EventsBus eventBus;

    public readonly Dictionary<Type, MySystem> systems = new();

    public SharedData(GameData gameData, RenderData renderData, PhysicsData physicsData, NetworkData networkData, EventsBus eventsBus)
    {
        this.gameData = gameData;
        this.renderData = renderData;
        this.physicsData = physicsData;
        this.networkData = networkData;
        this.eventBus = eventsBus;
    }
}
