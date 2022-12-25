#define FAST_LOAD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class Cool3DIntroSystem : MySystem
{

    private CoroutineRunner _runner;

    private Cool3DLIntroMaterial _material;

    private const float CAM_START_POS = -130;

    private const float CAM_END_POS = 0;

    private float _camSpeed = 1;

    public override void Run(EcsSystems systems)
    {
#if FAST_LOAD
        _material.Dispose();

        GroupState.Set("Game", true);
        GroupState.Set("Physics", true);
        GroupState.Set("Intro", false);
        GroupState.Set("Cursor", true);

        SetSkyState(CAM_END_POS, SkyBackgroundSystem.TopColor, SkyBackgroundSystem.BottomColor);
        return;
#else
        _runner.Update(sharedData.physicsData.deltaTime);
#endif
    }

    private void SetSkyState(float camPos, Vector3? col1 = null, Vector3? col2 = null)
    {
        ref var skyState = ref sharedData.eventBus.GetEventBodySingleton<SkyBackgroundState>();
        skyState.camPosY = camPos;
        if (col1 != null) skyState.topColor = col1.Value;
        if (col2 != null) skyState.bottomColor = col2.Value;
    }

    public IEnumerator ShowLogo()
    {
        while (true)
        {
            sharedData.renderData.toTextureRenderer.Render(_material);
            Graphics.DrawTexture(_material.TextureTarget, Layer.Middle, null, Vector2.Zero);

            if (sharedData.physicsData.time > 16 || Keyboard.PressedAny())
                break;

            yield return null;
        }

        bool movingFinished = false;

        while (true)
        {
            var skyState = sharedData.eventBus.GetEventBodySingleton<SkyBackgroundState>();

            if (skyState.camPosY >= CAM_END_POS)
                movingFinished = true;

            if (!movingFinished)
            {
                SetSkyState(skyState.camPosY + 0.7f * _camSpeed,
                    Vector3.Lerp(Vector3.Zero, new Vector3(35, 186, 194), (skyState.camPosY + MathF.Abs(CAM_START_POS)) / (CAM_END_POS + MathF.Abs(CAM_START_POS))),
                    Vector3.Lerp(Vector3.Zero, new Vector3(0, 153, 219), (skyState.camPosY + MathF.Abs(CAM_START_POS)) / (CAM_END_POS + MathF.Abs(CAM_START_POS))));

                if (skyState.camPosY > CAM_END_POS - 70)
                    _camSpeed = MathF.Max(_camSpeed - 0.005f, 0.05f);
            }
            else { break; }

            yield return null;
        }
        _material.Dispose();

        GroupState.Set("Menu", true);
        GroupState.Set("Intro", false);
        GroupState.Set("Cursor", true);

        SetSkyState(CAM_END_POS, SkyBackgroundSystem.TopColor, SkyBackgroundSystem.BottomColor);
    }

    public override void OnGroupDiactivate()
    {
        base.OnGroupDiactivate();

        ref var skyState = ref sharedData.eventBus.GetEventBodySingleton<SkyBackgroundState>();
        skyState.useCamPosition = true;

        Content.DeleteShader(_material.Shader);

        Content.DeleteTexture(_material.texture1);
        Content.DeleteTexture(_material.texture2);

        _material = null;
    }

    public override void Init(EcsSystems systems)
    {
        base.Init(systems);

        _runner = new();
        _runner.Run(ShowLogo());
        _material = Material.Create<Cool3DLIntroMaterial>(Content.LoadEmptyTexture(Utils.FullScreenTextureSize, "cool3DIntroMaterialTexture"));
        ref var skyState = ref sharedData.eventBus.NewEventSingleton<SkyBackgroundState>();
        skyState.camPosY = CAM_START_POS;
    }
}