using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ldtk;
using Leopotam.EcsLite.Di;

namespace Game;

class TileData
{
    public Vector2 posStart, posEnd, defPos;

    public Vector2 size;

    public TileData(Vector2 pos, Vector2 size)
    {
        this.posStart = pos;
        this.posEnd = pos;
        this.defPos = pos;
        this.size = size;
    }

    public TileData() { }
}

class LevelsSystem : MySystem
{

    private EcsCustomInject<LevelsService> _levelsService;

    public override void Run(EcsSystems systems)
    {
        _levelsService.Value.Update(sharedData.physicsData.deltaTime);
    }


    public override void OnGroupActivate()
    {
        base.OnGroupActivate();

        _levelsService.Value.SetLevel(MyPath.Join(MyPath.LevelsDirectory, "level1.json"), LoadingMode.Everything);
    }
}
