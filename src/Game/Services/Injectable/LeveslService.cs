using ldtk;
using Leopotam.EcsLite.Di;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game;

enum LoadingMode
{
    Tiles,
    Entities,
    Everything
}

class LevelsService : MyInjectableService
{

    public Vector2 WorldSize { get; private set; }

    public Vector2 SubLevelPos => SubLevelsTransforms[CurrentSubLevel].position;
    public Vector2 SubLevelSize => SubLevelsTransforms[CurrentSubLevel].size;

    public Level CurrentLevel { get; private set; }

    public Transform CurrentSubLevelTransform => SubLevelsTransforms[CurrentSubLevel];

    public NeighbourLevel[] NeighourSubLevels => CurrentSubLevel.Neighbours;

    public Dictionary<string, Sublevel> SubLevelsByIIDs { get; set; } = new();

    public Dictionary<Sublevel, Transform> SubLevelsTransforms { get; private set; }

    public Sublevel CurrentSubLevel { get; private set; }

    private List<TileData> _tileData = new();

    private List<TileData> _optimizedTileData = new();

    private const float TILE_SIZE = 8;

    private const int MAX_LOADING_TIME = 10;

    private const string START_LEVEL_NAME = "Start";

    private EcsPool<Renderable> _renderables;

    private EcsPool<Transform> _transforms;

    private EcsPool<LevelObject> _levelObjectsPool;

    private EcsFilter _staticBodiesFilter;

    private EcsFilter _dynamicBodiesFilter;

    private EcsFilter _levelObjectsFilter;

    private EcsFilter _oldLevelObjectsFilter;

    private Stopwatch _clearingOldLevelStopWatch;

    private CoroutineRunner _runner;

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _renderables = world.GetPool<Renderable>();
        _transforms = world.GetPool<Transform>();
        _levelObjectsPool = world.GetPool<LevelObject>();

        _staticBodiesFilter = world.Filter<StaticBody>().End();
        _dynamicBodiesFilter = world.Filter<DynamicBody>().End();
        _levelObjectsFilter = world.Filter<LevelObject>().End();
        _oldLevelObjectsFilter = world.Filter<OldLevelObject>().End();

        SubLevelsTransforms = new();
        _clearingOldLevelStopWatch = new();
        _runner = new();
    }

    public void Update(float deltaTime)
    {
        _runner.Update(deltaTime);
    }

    public void SetLevel(string filename, LoadingMode mode)
    {
        var ldtkJson = LdtkJson.FromJson(File.ReadAllText(filename));
        CurrentLevel = new Level(ldtkJson);

        SubLevelsTransforms.Clear();
        SubLevelsByIIDs.Clear();

        bool subLevelWasSet = false;

        foreach (var subLevel in ldtkJson.Sublevels)
        {
            if (subLevel.Identifier == START_LEVEL_NAME && !subLevelWasSet)
            {
                subLevelWasSet = true;

                WorldSize = new Vector2(ldtkJson.WorldGridWidth.Value, ldtkJson.WorldGridHeight.Value);
                SetSublevel(subLevel, mode, true);
            }

            float halfSizeX = subLevel.PxWid / 2;
            float halfSizeY = subLevel.PxHei / 2;

            var transform = new Transform(subLevel.WorldX + halfSizeX, subLevel.WorldY + halfSizeY, subLevel.PxWid, subLevel.PxHei);
            SubLevelsTransforms[subLevel] = transform;

            SubLevelsByIIDs[subLevel.Iid] = subLevel;
        }
    }

    public void SetSublevel(Sublevel subLevel, LoadingMode mode, bool resetSubLevel)
    {
        if (resetSubLevel)
            CurrentSubLevel = subLevel;

        foreach (var layer in subLevel.LayerInstances)
        {
            switch (layer.Type)
            {
                case "IntGrid":

                    if (mode != LoadingMode.Everything && mode != LoadingMode.Tiles)
                        continue;

                    foreach (var tile in layer.AutoLayerTiles)
                    {
                        var renderable = new Renderable();
                        var sprite = new Sprite(Path.GetFileNameWithoutExtension(layer.TilesetRelPath), new Vector2i(8));
                        sprite.Frame = (int)tile.T;

                        renderable.sprite = sprite;
                        sprite.layer = Layer.MiddlePixelized;
                        sprite.position = new Vector2(tile.Px[0] + layer.GridSize / 2, tile.Px[1] + layer.GridSize / 2);
                        sprite.position += new Vector2(subLevel.WorldX, subLevel.WorldY);
                        sprite.depth = 1;
                        sprite.flippedHorizontally = tile.F == 1 || tile.F == 3;
                        sprite.flippedVertically = tile.F == 2 || tile.F == 3;

                        Transform tr = new Transform(sprite.position, 0, sprite.size);

                        int entity = world.NewEntity();

                        _levelObjectsPool.Add(entity);

                        ref var rend = ref _renderables.Add(entity);
                        rend = renderable;

                        _tileData.Add(new TileData(tr.position, tr.size));
                    }
                    break;
                case "Entities":

                    if (mode != LoadingMode.Everything && mode != LoadingMode.Entities)
                        continue;

                    foreach (var ldtkEntity in layer.EntityInstances)
                    {
                        if (ldtkEntity.Tags.Length == 0)
                        {
                            switch (ldtkEntity.Identifier)
                            {
                                case "SpawnPoint":
                                    var spawnPointE = world.NewEntity();
                                    ref var spawnPoint = ref sharedData.eventBus.NewEventSingleton<SpawnPoint>();
                                    spawnPoint.position = new Vector2(ldtkEntity.Px[0] + ldtkEntity.Width / 2, ldtkEntity.Px[1] + ldtkEntity.Height / 2);
                                    spawnPoint.position += new Vector2(subLevel.WorldX, subLevel.WorldY);

                                    break;
                                default:
                                    break;
                            }

                            continue;
                        }
                        var sprite = new Sprite(Path.GetFileNameWithoutExtension(ldtkEntity.Tags[0]));
                        var renderable = new Renderable();

                        renderable.sprite = sprite;
                        sprite.position = new Vector2(ldtkEntity.Px[0] + sprite.TexWidth / 2, ldtkEntity.Px[1] + sprite.TexHeight / 2);
                        sprite.position += new Vector2(subLevel.WorldX, subLevel.WorldY);

                        sprite.layer = Layer.BackPixelized;
                        sprite.depth = -1;

                        var entity = world.NewEntity();

                        switch (ldtkEntity.Tags[0])
                        {
                            case "Tree":
                            case "sakura":
                            case "tree1":
                                Utils.GetSystem<VoronoiWindSystem>().Add(entity, out var windMaterial);
                                sprite.material = windMaterial;
                                break;
                        }

                        ref var tr = ref _transforms.Add(entity);
                        ref var rend = ref _renderables.Add(entity);

                        _levelObjectsPool.Add(entity);

                        tr = new Transform(sprite.position, 0, sprite.size);
                        rend = renderable;
                    }
                    break;
                case "Tiles":

                    break;

                default: //TODO: Add other layers support

                    continue;
            }
        }

        if (mode != LoadingMode.Tiles && mode != LoadingMode.Everything)
            return;

        _tileData = _tileData.OrderBy(td => -td.posStart.Y).ThenBy(td => td.posStart.X).ToList();

        Vector2 startPos, endPos, lastPos;
        startPos = endPos = lastPos = Vector2.Zero;
        bool setStartPos = true;

        for (int i = 0; i < _tileData.Count; i++)
        {
            var tData = _tileData[i];
            TileData nextData = default;

            if (i < _tileData.Count - 1)
                nextData = _tileData[i + 1];

            if (setStartPos)
            {
                startPos = tData.defPos;
                setStartPos = false;
            }

            if (i == _tileData.Count - 1 || MathF.Abs(nextData.defPos.X - tData.defPos.X) > TILE_SIZE || (nextData.defPos.Y < tData.defPos.Y))
            {
                endPos = tData.defPos;

                setStartPos = true;
                _optimizedTileData.Add(new TileData() { posStart = startPos, posEnd = endPos });
            }

            lastPos = tData.defPos;
        }

        for (int i = 0; i < _optimizedTileData.Count; i++)
        {
            var tileData = _optimizedTileData[i];
            float x = MathHelper.Lerp(tileData.posStart.X, tileData.posEnd.X, .5f);
            Vector2 size = new Vector2((tileData.posEnd.X - tileData.posStart.X) + TILE_SIZE, TILE_SIZE);

            int e = world.NewEntity();
            sharedData.physicsData.physicsFactory.AddSaticBody(e, new Transform(new Vector2(x, tileData.posStart.Y), size), new(PhysicsBodyType.Block));
        }

        _optimizedTileData.Clear();
        _tileData.Clear();
    }

    public void MarkOldLevelObjects()
    {
        var oldObjectsPool = world.GetPool<OldLevelObject>();

        foreach (var entity in _levelObjectsFilter)
        {
            oldObjectsPool.Add(entity);
        }

        foreach (var entity in _staticBodiesFilter)
        {
            oldObjectsPool.Add(entity);
        }

        var dynamicBodes = world.GetPool<DynamicBody>();
        foreach (var entity in _dynamicBodiesFilter)
        {
            var body = dynamicBodes.Get(entity);
            if (body.box2DBody.GetUserData().bodyType == PhysicsBodyType.Player)
                continue;

            oldObjectsPool.Add(entity);
        }
    }

    public void ClearOldLevelObjects() => _runner.Run(ClearOldLevelObjectsCoroutine());

    private IEnumerator ClearOldLevelObjectsCoroutine()
    {
        _clearingOldLevelStopWatch.Start();

        var staticBodiesPool = world.GetPool<StaticBody>();
        var dynamicBodesPool = world.GetPool<DynamicBody>();

        foreach (var entity in _oldLevelObjectsFilter)
        {
            if (IsLoadingTooLong(_clearingOldLevelStopWatch))
            {
                _clearingOldLevelStopWatch.Reset();
                yield return null;
            }

            if (staticBodiesPool.Has(entity))
            {
                sharedData.physicsData.physicsFactory.RemoveStaticBody(entity);
                world.DelEntity(entity);
                continue;
            }

            if (dynamicBodesPool.Has(entity))
            {
                var bodyType = dynamicBodesPool.Get(entity).box2DBody.GetUserData().bodyType;

                if (bodyType == PhysicsBodyType.Player || bodyType == PhysicsBodyType.TongueSensor)
                    continue;

                sharedData.physicsData.physicsFactory.RemoveDynamicBody(entity);
                world.DelEntity(entity);

                continue;
            }

            world.DelEntity(entity);
        }
    }

    private bool IsLoadingTooLong(Stopwatch watch) => watch.ElapsedMilliseconds > MAX_LOADING_TIME;
}
