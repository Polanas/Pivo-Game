#define LOAD_SHARP_FONT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpFont;
using OpenTK.Graphics.OpenGL4;
using Leopotam.EcsLite.Di;
using System.Reflection.Emit;

namespace Game;

readonly record struct DrawCall(string Text, string FontName, float[] Vertices, int VerticesCount, Vector3i Color, float Alpha);

class RenderTextSystem : RenderSystem
{
    private Library _library;

    private Dictionary<string, Font> _fonts = new();

    private Matrix4 _projection;

    private float[] _vertices = new float[12];

    public Vector2 GetTextSize(string text, string fontName, float scale)
    {
        Font font = _fonts[fontName];

        Vector2 size = Vector2.Zero;

        float top, bottom, left, right;

        left = bottom = right = 0;
        top = font.size * scale;

        for (int i = 0; i < text.Length; i++)
            right += (font.Characters[text[i]].advance >> 6) * scale;

        size.X = MathF.Abs(right - left);
        size.Y = MathF.Abs(bottom - top);

        return size;
    }

    public void DrawText(string text, string fontName, Vector2 position, float scale, Vector3i color, bool centered = true, float alpha = 1)
    {
        GLUtils.ClearTextureUnit(TextureUnit.Texture0);

        GL.BindVertexArray(VAO);

        shader.Use();
        shader.SetValue("projection", _projection);

        Font font = _fonts[fontName];

        position /= MyGameWindow.FullToPixelatedRatio;
        position -= (Vector2)MyGameWindow.ScreenSize / (MyGameWindow.FullToPixelatedRatio * 2f);
        scale /= MyGameWindow.FullToPixelatedRatio;

        color /= 255;
        scale /= font.size / 8;

        Vector2 size = Vector2.Zero;

        if (centered)
            size = GetTextSize(text, fontName, scale);

        shader.SetValue("textColor", color);
        shader.SetValue("alpha", alpha);

        for (int i = 0; i < text.Length; i++)
        {
            char ind = text[i];
            var character = font.Characters[ind];

            float xPos = position.X + character.bearing.X * scale;
            float yPos = position.Y + (character.size.Y - character.bearing.Y) * scale;

            float w = character.size.X * scale;
            float h = character.size.Y * scale;

            h *= -1;
            xPos -= size.X / 2;
            yPos += size.Y / 2;

            _vertices[0] = xPos;
            _vertices[1] = yPos + h;
            _vertices[2] = xPos;
            _vertices[3] = yPos;
            _vertices[4] = xPos + w;
            _vertices[5] = yPos;

            _vertices[6] = xPos;
            _vertices[7] = yPos + h;
            _vertices[8] = xPos + w;
            _vertices[9] = yPos;
            _vertices[10] = xPos + w;
            _vertices[11] = yPos + h;

            GL.BindTexture(TextureTarget.Texture2D, character.textureID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.DynamicDraw);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            position.X += (character.advance >> 6) * scale;
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindVertexArray(0);
    }

    public override void Run(EcsSystems systems)
    {
        foreach (var e in sharedData.eventBus.GetEventBodies<LoadFont>(out var pool))
        {
            var loadFontEvent = pool.Get(e);

            _fonts.Add(loadFontEvent.name, new Font(_library, loadFontEvent.path, loadFontEvent.size));
            pool.Del(e);
        }
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _library = new Library();

        shader = Content.GetShader("text");

        VBO = GL.GenBuffer();
        VAO = GL.GenVertexArray();

        GL.BindVertexArray(VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);

        Vector2 size = (Vector2)MyGameWindow.ScreenSize / MyGameWindow.FullToPixelatedRatio;
       size.Y *= -1;
        _projection = MyMath.CreateCameraMatrix(Vector2.Zero, size);

        _fonts.Add("verdana", new Font(_library, @"Content\Fonts\PixelFJVerdana12pt.ttf", 32));
        _fonts.Add("pixuf", new Font(_library, @"Content\Fonts\Pixuf.ttf", 32));
    }
}