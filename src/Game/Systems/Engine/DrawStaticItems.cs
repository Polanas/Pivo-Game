namespace Game;

class DrawStaticItems : MySystem
{

    private EcsFilterInject<Inc<StaticRenderable>> _staticRenderableFilter;

    private EcsCustomInject<SpriteBatcher> _batcher;

    public override void Run(EcsSystems systems)
    {
        foreach (var e in _staticRenderableFilter.Value)
        {
            var item = _staticRenderableFilter.Pools.Inc1.Get(e).batchItem;

            _batcher.Value.SubmitDirect(item.layer,
                                  item.material,
                                  item.texture,
                                  item.projectionPart1,
                                  item.projectionPart2,
                                  item.color,
                                  item.frameData,
                                  item.depth);
        }
    }
}
