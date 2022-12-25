using OpenTK.Graphics.OpenGL4;

namespace Game;

static class OpenGL
{
    struct Viewport
    {
        public Vector2i position;
        public Vector2i size;

        public Viewport(Vector2i position, Vector2i size)
        {
            this.position = position;
            this.size = size;
        }
    }

    private static Viewport _lastViewport;

    public static void SetViewport(Vector2i position, Vector2i size)
    {
        var viewport = new Viewport(position, size);

        if (!_lastViewport.Equals(viewport))
        {
            _lastViewport = viewport;
            GL.Viewport(viewport.position.X, viewport.position.Y, viewport.size.X, viewport.size.Y);
        }
    }
}
