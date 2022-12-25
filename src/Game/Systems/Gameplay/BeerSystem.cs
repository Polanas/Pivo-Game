using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;
using Box2DSharp.Dynamics;

namespace Game;

class BeerSystem : MySystem
{

    private EcsFilterInject<Inc<Beer, Transform>> _beerFilter;

    private EcsPoolInject<Beer> _beerPool;

    private EcsPoolInject<StaticBody> _staticBodies;

    private EcsPoolInject<Renderable> _renderables;

    private EcsPoolInject<Transform> _transforms;

    private CoroutineRunner _runner;

    public override void Run(EcsSystems systems)
    {
        foreach (var e in _beerFilter.Value)
        {
            ref var renderable = ref _renderables.Value.Get(e);
            ref var beer = ref _beerPool.Value.Get(e);
            ref var beerTransform = ref _transforms.Value.Get(e);

            beerTransform.angle = MathF.Sin(sharedData.physicsData.time * 2) * 10;
            beer.material.frame = _renderables.Value.Get(e).sprite.Frame;

            var eatable = world.GetComponent<Eatable>(e);

            if (eatable.beingConsumed && !beer.startedBeingConsumed)
            {
                beer.startedBeingConsumed = true;
                _runner.Run(BeerConsumption(e));
            }
        }

        _runner.Update(sharedData.physicsData.fixedDeltaTime);
    }

    private IEnumerator BeerConsumption(int beer)
    {
        Body tongueSensor;
        Tongue tongue = world.GetComponent<Tongue>(world.GetEntitiyWithComponent<Tongue>());

        tongueSensor = tongue.sensor;
        var beerBody = _staticBodies.Value.Get(beer);
        var beerSprite = _renderables.Value.Get(beer).sprite;

        while (true)
        {
            beerBody.body.SetPosition(tongueSensor.GetPixelatedPosition());
            beerSprite.scale = MathF.Max(beerSprite.scale - sharedData.physicsData.fixedDeltaTime * 5, 0);

            if (beerSprite.scale <= 0)
            {
                sharedData.physicsData.physicsFactory.RemoveStaticBody(beer);
                Graphics.RemoveRenderable(beer);
                world.DelEntity(beer);
                break;
            }

            yield return null;
        }
    }

    public void BeerPrefab(int entity, Vector2 position)
    {
        var sprite = new Sprite("beerCan", new Vector2i(32), 2, Layer.Middle);
        sprite
            .AddAnimation("idle", 0, false, new int[] { sprite.FramesAmount - 1 })
            .AddAnimation("beering", 1f/6f, true, new int[] { 0, 1, 2, 3, 4, 5, 5 })
            .SetAnimation("beering");

        sprite.material = Material.Create<BeerMaterial>();

        sharedData.physicsData.physicsFactory.AddSaticBody(entity,
                                                           new Transform(position, new Vector2(9, 13)),
                                                           new(PhysicsBodyType.Eatable, world.PackEntity(entity)),
                                                           PhysicsCategory.Beer,
                                                           (ushort)PhysicsCategory.TongueSensor);

        world.AddComponent(entity, new Renderable(sprite));
        world.AddComponent(entity, new Beer() { material = (BeerMaterial)sprite.material });
        Utils.GetSystem<TongueSystem>().EatablePrefab(entity, PhysicsBodyType.Eatable);
    }

    public override void OnGroupActivate()
    {
        base.OnGroupActivate();

        BeerPrefab(world.NewEntity(), new Vector2(-9, -27));

        _runner = new();
    }
}