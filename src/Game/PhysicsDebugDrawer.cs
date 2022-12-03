using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DSharp.Common;

namespace Game;

class PhysicsDebugDrawer : IDrawer
{

    public DrawFlag Flags { get; set; }

    private DebugDrawer _debugDrawer;

    private float _PTM;

    public PhysicsDebugDrawer(DebugDrawer debugDrawer, float PTM)
    {
        _debugDrawer = debugDrawer;
        Flags = DrawFlag.DrawAABB;
        _PTM = PTM;
    }

    public void DrawTransform(in Box2DSharp.Common.Transform xf) { }

    public void DrawPolygon(Span<System.Numerics.Vector2> vertices, int vertexCount, in Color color)
    {
        var topLeft = vertices[0];
        var botomRight = vertices[2];

        var b2Pos = (botomRight + topLeft)/2;
        var pos = Utils.ToOpenTKVector2(ref b2Pos);
        var size = new Vector2(MathF.Abs(botomRight.X - topLeft.X), MathF.Abs(botomRight.Y - topLeft.Y));

        pos /= _PTM;
        size /= _PTM;

        //_debugDrawer.DrawRect(pos, size, new Vector3i(color.R, color.G, color.B));
    }

    public void DrawSolidPolygon(Span<System.Numerics.Vector2> vertices, int vertexCount, in Color color) { }

    public void DrawCircle(in System.Numerics.Vector2 center, float radius, in Color color) { }

    public void DrawSolidCircle(in System.Numerics.Vector2 center, float radius, in System.Numerics.Vector2 axis, in Color color) { }

    public void DrawSegment(in System.Numerics.Vector2 p1, in System.Numerics.Vector2 p2, in Color color) { }

    public void DrawPoint(in System.Numerics.Vector2 p, float size, in Color color) { }
}
