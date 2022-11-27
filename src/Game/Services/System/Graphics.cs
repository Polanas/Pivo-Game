using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class Graphics : Service, ISystemService, IRegularService
{
    private static RenderSpritesSystem _renderSpritesSystem;

    private static RenderTextSystem _renderTextSystem;

    private static SharedData s_sharedData;

    private static EcsPool<Renderable> _renerables;

    private Comparer<Layer> _layerComparer = Comparer<Layer>.Create((x, y) => x.depth.CompareTo(y.depth));

    public override void Init(SharedData sharedData, EcsWorld world, ServiceState state)
    {
        if (state == ServiceState.Regular)
        {
            base.Init(sharedData, world, state);
            _renerables = world.GetPool<Renderable>();
            return;
        }

        _renderSpritesSystem = Utils.GetSystem<RenderSpritesSystem>();
        _renderTextSystem = Utils.GetSystem<RenderTextSystem>();
        s_sharedData = sharedData;
    }

    public void AddLayer(Layer layer)
    {
        sharedData.renderData.layers[layer.name] = layer;

        sharedData.renderData.layersList.Add(layer);
        sharedData.renderData.layersList.Sort(_layerComparer);

        sharedData.renderData.cameraLayerProjections.Add(layer, default(Matrix4));
        sharedData.renderData.layerProjections.Add(layer, default(Matrix4));
    }

    public void RemoveLayer(Layer layer)
    {
        var name = layer.name;

        sharedData.renderData.layers.Remove(name);
        sharedData.renderData.layersList.Remove(layer);

        sharedData.renderData.cameraLayerProjections.Remove(layer);
        sharedData.renderData.layerProjections.Remove(layer);
    }

    public static void RemoveRenderable(int e)
    {
        if (!_renerables.Has(e))
            return;

        ref var renderable = ref _renerables.Get(e);
        renderable.sprite?.material.Dispose();

        _renerables.Del(e);
    }

    public static void DrawSprite(Sprite sprite)
    {
        _renderSpritesSystem.DrawSprite(sprite);
        sprite.UpdateFrame(s_sharedData.physicsData.fixedDeltaTime);
    }

    public static void DrawText(string text, string fontName, Vector2 position, Vector3i color, float scale = 1, bool centered = true, float alpha = 1) =>
        _renderTextSystem.DrawText(text, fontName, position, scale, color, centered, alpha);

    public static void DrawTextDebug(string text, Vector2 position) =>
      _renderTextSystem.DrawText(text, "pixuf", position, MyGameWindow.FullToPixelatedRatio / 2, new Vector3i(255), false);

    public static void DrawTexture(Texture texture, Layer layer, Material material, Vector2 position, float depth = 0, float scale = 1, float alpha = 1, float angle = 0, Vector2i? offDir = null)
    {
        _renderSpritesSystem.DrawTexture(texture, layer, material, position, new Vector2(texture.Width, texture.Height)*scale, new Vector3i(255), depth, alpha, angle, offDir);
    }

    public static void DrawMaterial(Layer layer, Material material, Vector2 size, Vector2 position, float depth = 0, float alpha = 1, float angle = 0, Vector2i? offDir = null)
    {
        _renderSpritesSystem.DrawTexture(null, layer, material, position, size, new Vector3i(255), depth, alpha, angle, offDir);
    }

    public static void DrawMaterial(Layer layer, Material material, Vector2 position, float depth = 0, float alpha = 1, float angle = 0, Vector2i? offDir = null)
    {
        _renderSpritesSystem.DrawTexture(null, layer, material, position, material.QuadSize, new Vector3i(255), depth, alpha, angle, offDir);
    }
}