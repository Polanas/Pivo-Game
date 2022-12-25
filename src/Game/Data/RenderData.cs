using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Windowing.Common;

namespace Game;

class RenderData : IGameData
{

    // public Texture lightTexture;

    // public Texture maskTexture;

    //public Texture distortedShadowsTexture;

    //public Texture shadowCastersTexture;

    public Texture atlasTexture;

    public FrameEventArgs frameEventArgs;

    public readonly Graphics graphics;

    public readonly Dictionary<Layer, Matrix4> cameraLayerProjections;

    public readonly ToTextureRenderer toTextureRenderer;

    public readonly Dictionary<Layer, Matrix4> layerProjections;

    public readonly DebugDrawer debugDrawer;

    public readonly Dictionary<string, Layer> layers;

    public readonly List<Layer> layersList;

    public RenderData(DebugDrawer debugDrawer, Graphics graphics, ToTextureRenderer toTextureRenderer)
    {
        this.debugDrawer = debugDrawer;
        this.graphics = graphics;
        this.toTextureRenderer = toTextureRenderer;

        layers = new();
        layersList = new();
        cameraLayerProjections = new();
        layerProjections = new();
    }
}
