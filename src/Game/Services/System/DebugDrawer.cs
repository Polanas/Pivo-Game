using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class DebugDrawer : Service, ISystemService
{

    private RenderSpritesSystem _renderSpritesSystem;

    public override void Init(SharedData sharedData, EcsWorld world, ServiceState state)
    {
        base.Init(sharedData, world, state);
        _renderSpritesSystem = Utils.GetSystem<RenderSpritesSystem>();
    }

    public void DrawRect(Vector2 position, Vector2 size, Vector3i color, float angle = 0) =>
        _renderSpritesSystem.DrawRect(position, color, size, angle);

    public void DrawLine(Vector2 startPoint, Vector2 endPoint, Vector3i color, float thickness = 1) =>
        _renderSpritesSystem.DrawLine(startPoint, endPoint, color, thickness);

    public void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Vector3i color, float thickness = 1)
    {
        DrawLine(a, b, color, thickness);
        DrawLine(b, c, color, thickness);
        DrawLine(a, c, color, thickness);
    }
}