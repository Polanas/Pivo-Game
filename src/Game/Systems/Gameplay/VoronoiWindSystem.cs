using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;

namespace Game;

class VoronoiWindSystem : MySystem
{

    private EcsPoolInject<HasWindMaterial> _hasWindMaterial;

    private Random _random;

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _random = new();
    }

    public int Add(int entity, out VoronoiWindMaterial outWindMaterial)
    {
        VoronoiWindMaterial windMaterial = Material.Create<VoronoiWindMaterial>();
        windMaterial.seed = _random.Next(10000);
        world.AddComponent(entity, new HasWindMaterial() { windMaterial = windMaterial });

        outWindMaterial = windMaterial;

        return entity;
    }
}