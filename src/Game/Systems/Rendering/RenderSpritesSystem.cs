using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Leopotam.EcsLite.Di;
using OpenTK.Graphics.OpenGL4;

namespace Game;


class RenderSpritesSystem : RenderSystem
{

    private Transform _cameraTransform;

    private SpriteBatcher _batcher;

    private Texture _rectangleTexture;

    private EcsPoolInject<Renderable> _renderables;

    private EcsPoolInject<DynamicBody> _dynamicBodies;

    private EcsPoolInject<Transform> _transforms;

    public void DrawSprite(Sprite sprite)
    {
        _batcher.DrawSprite(sprite);
    }

    public void DrawLine(Vector2 start, Vector2 end, Vector3i color, float thickness)
    {
        _batcher.DrawTexture(_rectangleTexture,
                             Layer.Middle,
                             null,
                             (start + end) / 2,
                             color,
                             1f,
                             new Vector2((start - end).Length, thickness),
                             -MyMath.AngleBetweenPoints(end, start),
                             5,
                             new Vector2i(1));
    }

    public void DrawRect(Vector2 position, Vector3i color, Vector2 size, float angle)
    {
        _batcher.DrawTexture(_rectangleTexture,
                             Layer.Middle,
                             null,
                             position,
                             color,
                             1f,
                             size,
                             angle,
                             5,
                             new Vector2i(1));
    }

    public void DrawTexture(Texture texture, Layer layer, Material material, Vector2 position, Vector2 size, Vector3i color, float depth = 0, float alpha = 1, float angle = 0, Vector2i? offDir = null)
    {
        _batcher.DrawTexture(texture, layer, material, position, color, alpha, size, angle, depth, offDir ?? new Vector2i(1));
    }

    public override void Run(EcsSystems systems)
    {
        sharedData.renderData.toTextureRenderer.Update(sharedData.physicsData.deltaTime);

        _cameraTransform.position = sharedData.gameData.camera.position;
        _cameraTransform.size = sharedData.gameData.camera.Size;

        var camera = sharedData.gameData.camera;

        foreach (int entity in world.Filter<Renderable>().End())
        {
            ref var renderable = ref _renderables.Value.Get(entity);

            if (renderable.sprite == null
                || renderable.sprite.layer == null
                || !renderable.sprite.visible
                || (renderable.sprite.Texture == null && renderable.sprite.material == null)
                || renderable.sprite.alpha <= 0)
                continue;

            var sprite = renderable.sprite;

            if (_transforms.Value.Has(entity))
            {
                ref var transform = ref _transforms.Value.Get(entity);

                sprite.position = transform.position;
                sprite.angle = transform.angle;

                if (_dynamicBodies.Value.Has(entity))
                    sprite.angle = MathHelper.RadiansToDegrees(_dynamicBodies.Value.Get(entity).box2DBody.GetAngle());
            }

            if (!Collision.Rectangle(camera.position, Utils.FullScreenTextureSize, sprite.position, new Vector2(sprite.FrameWidth, sprite.FrameHeight)) && sprite.layer.cameraPosModifier != 0)
                continue;

            sprite.UpdateFrame(sharedData.physicsData.fixedDeltaTime);
            _batcher.DrawSprite(sprite);
        }

        _batcher.Render(sharedData.physicsData.time);

        sharedData.renderData.toTextureRenderer.OnFrameEnd();
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        var materialsData = sharedData.eventBus.GetEventBodySingleton<MaterialsData>();

        _batcher = new(Content.GetShader("batchShader"),
                       sharedData.renderData.layersList,
                       sharedData.renderData.cameraLayerProjections,
                       materialsData.materialUniformFields,
                       MyGameWindow.FullToPixelatedRatio
                       );
        _rectangleTexture = Content.GetTexture("rectangle");

        ref var pstProcessingGLData = ref sharedData.eventBus.NewEventSingleton<PostProcessingGLData>();

        pstProcessingGLData.renerSpritesFBO = _batcher.FBO.Handle;
        pstProcessingGLData.uiTexture = Content.LoadEmptyTexture(MyGameWindow.ScreenSize, "uiTexture");
        pstProcessingGLData.spritesTexture = Content.LoadEmptyTexture(MyGameWindow.ScreenSize, "spritesTexture");

        sharedData.eventBus.DestroyEventSingleton<MaterialsData>();
    }
}