using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class Sprite
{

    public Animation? Current => _current;

    public bool Finished => _finished;

    public int FrameWidth { get; private set; }

    public int FrameHeight { get; private set; }

    public int TexWidth => _virtualTexture.Width;

    public int TexHeight => _virtualTexture.Height;

    public int Frame
    {
        get => _frameCount;
        set
        {
            _frameCount = value;
            _frameInc = 0;
        }
    }

    public int FramesAmount => _virtualTexture.Size.X / FrameWidth * (_virtualTexture.Size.Y / FrameHeight);

    public VirtualTexture VirtualTexture { get => _virtualTexture; set => _virtualTexture = value; }

    public float X { get => position.X; set => position.X = value; }

    public float Y { get => position.Y; set => position.Y = value; }

    public Vector2 size;

    public float depth;

    public float angle;

    public bool flippedVertically;

    public bool flippedHorizontally;

    public bool visible = true;

    public float scale = 1;

    public float alpha = 1;

    public Vector2 position;

    public Vector2 offset;

    public Layer layer;

    public Vector3i color = new Vector3i(255);

    public Material material;

    private bool _finished;

    private Dictionary<string, Animation> _animations = new();

    private Animation? _current;

    private VirtualTexture _virtualTexture;

    private float _frameInc;

    private int _lastFrame;

    private int _frameCount;

    public Sprite(string virtualTextureName, Vector2i? frameSize = null, int depth = 0, Layer layer = null)
    {
        _virtualTexture = Content.GetVirtualTexture(virtualTextureName);

        if (frameSize != null)
        {
            FrameWidth = frameSize.Value.X;
            FrameHeight = frameSize.Value.Y;
        }
        else
        {
            FrameWidth = _virtualTexture.Width;
            FrameHeight = _virtualTexture.Height;
        }

        size = new Vector2(FrameWidth, FrameHeight);

        this.depth = depth;
        this.layer = layer ?? Layer.Middle;
    }

    public Sprite(VirtualTexture virtualTexture, Vector2i? frameSize = null, int depth = 0, Layer layer = null)
    {
        _virtualTexture = virtualTexture;

        if (frameSize != null)
        {
            FrameWidth = frameSize.Value.X;
            FrameHeight = frameSize.Value.Y;
        }
        else
        {
            FrameWidth = _virtualTexture.Width;
            FrameHeight = _virtualTexture.Height;
        }

        size = new Vector2(FrameWidth, FrameHeight);

        this.depth = depth;
        this.layer = layer ?? Layer.Middle;
    }

    /// <summary>
    /// For materials, which don't use original texture
    /// </summary>
    public Sprite(Vector2i size, Layer layer = null)
    {
        this.size = (Vector2)size;

        FrameWidth = size.X;
        FrameHeight = size.Y;
        this.layer = layer ?? Layer.Middle;
    }

    public void UpdateFrame(float deltaTime)
    {
        if (_current == null || _finished)
            return;

        _frameInc += deltaTime;

        if (_frameInc >= _current.Value.delay)
        {
            _frameInc -= Current.Value.delay;
            _frameCount++;
        }

        if (_frameCount != _lastFrame)
        {

            if (_frameCount == _current.Value.frames.Length)
                if (_current.Value.loop)
                {
                    _frameCount = 0;
                }
                else
                {
                    _frameCount = _current.Value.frames.Length - 1;
                    _finished = true;
                }
        }

        _lastFrame = _frameCount;
    }

    public Sprite AddAnimation(string name, float delay, bool loop, int[] frames)
    {
        _animations.Add(name, new Animation(name, delay, loop, frames));
        return this;
    }

    public Sprite AddAnimation(string name, float delay, bool loop, int startIndex, int endIndex)
    {
        int framesCount = Math.Abs(endIndex - startIndex) + 1;
        int[] frames = new int[framesCount];

        for (int i = 0; i < framesCount; i++)
        {
            if (startIndex < endIndex)
                frames[i] = startIndex + i;
            else frames[i] = startIndex - i;
        }

        _animations.Add(name, new Animation(name, delay, loop, frames));
        return this;
    }

    public void SetAnimation(string name, bool saveFrame = false)
    {
        if ((_current != null && _current.Value == name) || !_animations.ContainsKey(name))
            return;

        int savedFrame;
        float savedFrameInc;

        var anim = _animations[name];

        savedFrame = _frameCount;
        savedFrameInc = _frameInc;
        ResetCurrentAnimation();
        _current = new Animation?(anim);

        if (saveFrame)
        {
            _frameCount = savedFrame;
            _frameInc = savedFrameInc;
        }

        return;
    }

    public void Draw() => Graphics.DrawSprite(this);

    public void ResetCurrentAnimation()
    {
        _frameCount = 0;
        _frameInc = 0;
        _lastFrame = 0;
        _finished = false;
    }
}