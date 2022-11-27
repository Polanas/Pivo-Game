using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leopotam.EcsLite.Di;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;

namespace Game;

class TongueSystem : MySystem
{

    private readonly Vector3 TONGUE_COL_TOP = new Vector3(212, 59, 80);

    private readonly Vector3 TONGUE_COL_BOTTOM = new Vector3(240, 139, 151);

    private EcsFilterInject<Inc<Tongue>> _tongueFilter;

    private EcsPoolInject<Eatable> _eatables;

    private EcsPoolInject<Tongue> _tongue;

    public override void Run(EcsSystems systems)
    {
        foreach (var e in _tongueFilter.Value)
        {
            ref var tongue = ref _tongue.Value.Get(e);

            if (!tongue.rendered)
                continue;

            tongue.topMaterial.angle = MathHelper.DegreesToRadians(tongue.angle);
            tongue.bottomMaterial.angle = MathHelper.DegreesToRadians(tongue.angle);

            tongue.topMaterial.length = tongue.bottomMaterial.length = tongue.length;

            Graphics.DrawMaterial(Layer.Middle, tongue.topMaterial, tongue.position, 0, 1, 0, new Vector2i(Utils.BoolToInt(!tongue.flipped), 1));
            Graphics.DrawMaterial(Layer.Middle, tongue.bottomMaterial, tongue.position + GetHalfTongueOffset(tongue.angle), 0, 1, 0, new Vector2i(Utils.BoolToInt(!tongue.flipped), 1));
        }
    }

    public void EatablePrefab(int entity, PhysicsBodyType userdata)
    {
        ref var eatableRef = ref world.AddComponent<Eatable>(entity);

        foreach (var e in _tongueFilter.Value)
        {
            eatableRef.tongue = world.PackEntity(e);

            sharedData.physicsData.contcactListener.AddBeginAndEndEvent(
                (b2, b1) =>
                {                   
                    SetTongueCollisionState((PhysicsUserData)b1.UserData, true);

                    world.RepackEntity(ref ((PhysicsUserData)b1.UserData).entity, out int tongue);
                    ref var tongueC = ref _tongue.Value.Get(tongue);

                    if (!tongueC.rendered)
                        return;

                    var data1 = (PhysicsUserData)b2.UserData;
                    world.RepackEntity(ref data1.entity, out int eatable);
                    ref var eatable1 = ref _eatables.Value.Get(eatable);

                    eatable1.beingConsumed = true;
                },
                (b2, b1) => SetTongueCollisionState((PhysicsUserData)b1.UserData, false),
                PhysicsBodyType.TongueSensor, userdata);
        }
    }

    public int TonguePrefab()
    {
        var tongue = world.NewEntity();
        HalfTonguePrefab(TONGUE_COL_TOP, out TongueMaterial topMaterial);
        HalfTonguePrefab(TONGUE_COL_BOTTOM, out TongueMaterial bottomMaterial);

        world.AddComponent(tongue, new Tongue() { topMaterial = topMaterial, bottomMaterial = bottomMaterial });

        ref var tongueComponent = ref _tongue.Value.Get(tongue);

        tongueComponent.sensor = sharedData.physicsData.physicsFactory.CreateBoxSensor(new Vector2(2), new(PhysicsBodyType.TongueSensor, world.PackEntity(tongue)), PhysicsCategory.TongueSensor);

        sharedData.physicsData.contcactListener.AddBeginAndEndEvent(
            (Body b1, Body b2) => SetTongueCollisionState((PhysicsUserData)b1.UserData, true),
            (Body b1, Body b2) => SetTongueCollisionState((PhysicsUserData)b1.UserData, false),
            PhysicsBodyType.TongueSensor, PhysicsBodyType.Block);

        return tongue;
    }

    private void SetTongueCollisionState(PhysicsUserData tongueSensorData, bool collided)
    {
        world.RepackEntity(ref tongueSensorData.entity, out int tongue);
        ref var tongueC = ref _tongue.Value.Get(tongue);
        tongueC.r_collided = collided;
    }

    private Vector2 GetHalfTongueOffset(float angle)
    {
        angle -= 45;
        angle = angle % 360f;
        if (angle < 0)
            angle += 360;

        if (angle > 0 && angle <= 90)
            return new Vector2(-1, 0);
        else if (angle > 90 && angle <= 180)
            return new Vector2(0, -1);
        else if (angle > 180 && angle <= 270)
            return new Vector2(1, 0);
        else if (angle > 270 && angle <= 360)
            return new Vector2(0, 1);

        return Vector2.Zero;
    }

    private void HalfTonguePrefab(Vector3 color, out TongueMaterial material)
    {
        var tongueMaterial = Material.Create<TongueMaterial>(new Vector2i(80));
        tongueMaterial.color = color;

        material = tongueMaterial;
    }
}
