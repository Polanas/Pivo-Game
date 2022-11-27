using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class Camera
{
    public readonly List<Matrix4> layerProjections = new();

    public Vector2 Size => MyGameWindow.PixelatedScreenSize;

    public Vector2 position;

    /// <summary>
    /// Use this for only rendering, not for computation. This position is always divisible by 1/FullToPixelatedRatio, which removes rendering artifacts (pixel missalignment).
    /// </summary>
    public Vector2 renderingPosition;

    public Vector2 offset;

    public float zoom;
}