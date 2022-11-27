using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.ExtendedSystems;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace Game;

static class Extensions
{

    private static MyGameWindow _game;

    private static float _PTM;

    private static Dictionary<Type, EcsFilter> _cachedFilters;

    public static void Init(MyGameWindow game)
    {
        _game = game;
        _PTM = _game.SharedData.physicsData.PTM;
        _cachedFilters = new();
    }

    public static ref T AddComponent<T>(this EcsWorld world, int entity) where T : struct =>
        ref world.GetPool<T>().Add(entity);

    public static void AddComponent<T>(this EcsWorld world, int entity, T component) where T : struct
    {
        ref var component1 = ref world.GetPool<T>().Add(entity);
        component1 = component;
    }

    public static void AddComponent<T>(this EcsWorld world, T component) where T : struct
    {
        ref var component1 = ref world.GetPool<T>().Add(world.NewEntity());
        component1 = component;
    }

    public static void AddComponent<T>(this EcsWorld world, int entity, EcsPool<T> pool,  T component) where T : struct
    {
        ref var component1 = ref pool.Add(entity);
        component1 = component;
    }

    public static ref T GetComponent<T>(this EcsWorld world, int entity) where T : struct =>
       ref world.GetPool<T>().Get(entity);

    public static bool HasComponent<T>(this EcsWorld world, int entity) where T : struct =>
        world.GetPool<T>().Has(entity);

    public static void RemoveComponent<T>(this EcsWorld world, int entity) where T : struct =>
       world.GetPool<T>().Del(entity);

    public static List<int> GetAllEntitiesWithTag(this EcsWorld world, string tag)
    {
        var pool = world.GetPool<Tag>();
        Tag tagS;
        List<int> entities = new();

        foreach (var e in world.Filter<Tag>().End())
        {
            tagS = pool.Get(e);

            if (tagS.value == tag)
                entities.Add(e);
        }

        return entities;
    }

    public static int GetEntityWithTag(this EcsWorld world, string tag)
    {
        var pool = world.GetPool<Tag>();
        Tag tagS;

        if (!_cachedFilters.TryGetValue(TypeOf<Tag>.Raw, out var filter))
        {
            filter = world.Filter<Tag>().End();
            _cachedFilters.Add(TypeOf<Tag>.Raw, filter);
        }

        foreach (var e in filter)
        {
            tagS = pool.Get(e);

            if (tagS.value == tag)
                return e;
        }

        return -1;
    }

    public static int GetEntitiyWithComponent<T>(this EcsWorld world) where T : struct
    {
        if (!_cachedFilters.TryGetValue(TypeOf<T>.Raw, out var filter))
        {
            filter = world.Filter<T>().End();
            _cachedFilters.Add(TypeOf<T>.Raw, filter);
        }

        foreach (var e in filter)
            return e;

        return -1;
    }

    /// <summary>
    /// Call this instead of regular add, if access to the system is needed
    /// </summary>
    public static EcsSystems AddExt(this EcsSystems systems, MySystem system)
    {
        AddSystemToShared(system);
        return systems.Add(system);
    }

    private static void AddSystemToShared(MySystem system)
    {
        _game.SharedData.systems.Add(system.GetType(), system);
    }

    public static EcsSystems AddGroupExt(this EcsSystems systems, string groupName, bool defaultState, params MySystem[] nestedSystems)
    {
        _game.AddGroupSystems(groupName, nestedSystems);

        for (int i = 0; i < nestedSystems.Length; i++)
        {
            AddSystemToShared(nestedSystems[i]);
            nestedSystems[i].CurrentGroupState = defaultState;
        }

        return systems.Add(new EcsGroupSystem(groupName, defaultState, null, nestedSystems));
    }

    public static EcsSystems AddGroupExt(this EcsSystems systems, string groupName, bool defaultState, string eventWorldName, params MySystem[] nestedSystems)
    {
        _game.AddGroupSystems(groupName, nestedSystems);

        for (int i = 0; i < nestedSystems.Length; i++)
            nestedSystems[i].CurrentGroupState = defaultState;

        return systems.Add(new EcsGroupSystem(groupName, defaultState, eventWorldName, nestedSystems));
    }

    public static void NewEventExt<T>(this EventsBus eventsBus, T newEvent) where T : struct, IEventReplicant
    {
        ref var newEventRef = ref eventsBus.NewEvent<T>();
        newEventRef = newEvent;
    }

    /// <summary>
    /// Do NOT use this before all systems are initialized
    /// </summary
    public static T Find<T>(this EcsSystems systems) where T : IEcsSystem
    {
        IEcsSystem[] systemsArray = null;
        systems.GetAllSystems(ref systemsArray);

        for (int i = 0; i < systemsArray.Length; i++)
        {
            if (systemsArray[i] is T)
                return (T)systemsArray[i];  //a bit cheasy but WHO CARES
        }

        return default(T);
    }

    public static Vector2 Mirrored(this Vector2 v1, Vector2 point)
    {
        return v1 - (v1 - point) * 2;
    }

    public static void SetPosition(this Body body, Vector2 position) => body.SetTransform(new System.Numerics.Vector2(position.X * _PTM, position.Y * _PTM), 0);

    public static Vector2 GetPixelatedPosition(this Body body)
    {
        System.Numerics.Vector2 pos = body.GetPosition();
        return new Vector2(pos.X / _PTM, pos.Y / _PTM);
    }


    public static Vector2 LerpDist(this Vector2 a, Vector2 b, float amount)
    {
        float dist = Vector2.Distance(a, b);
        float test = dist == 0 ? 1 : amount / dist;
        return Vector2.Lerp(a, b, test);
    }

    public static bool RepackEntity(this EcsWorld world, ref EcsPackedEntity packed, out int entity)
    {
        bool alive = packed.Unpack(world, out int e);

        if (!alive)
        {
            entity = -1;
            return alive;
        }

        packed.Id = entity = e;
        packed.Gen = world.GetEntityGen(packed.Id);

        return alive;
    }

    public static Vector2 ToOpenTKVector2(this System.Numerics.Vector2 vec2) => new Vector2(vec2.X, vec2.Y);

    public static System.Numerics.Vector2 ToSystemVector2(this Vector2 vector2) => new System.Numerics.Vector2(vector2.X, vector2.Y);

    public static System.Numerics.Vector2 GetLinearVelocity(this Body body) =>
        body.GetLinearVelocityFromWorldPoint(body.GetPosition());

    public static PhysicsUserData GetUserData(this Body body) => body.UserData as PhysicsUserData;
}