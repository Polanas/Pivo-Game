using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL4;

namespace Game;

[StructLayout(LayoutKind.Explicit)]
struct Vertex
{
    [FieldOffset(0)]
    public Vector4 projection1;

    [FieldOffset(16)]
    public Vector2 projection2;

    [FieldOffset(24)]
    public int textureIndex;

    [FieldOffset(28)]
    public float depth;

    [FieldOffset(32)]
    public Vector4 color;

    [FieldOffset(48)]
    public Vector3i frameData;
}

class SpriteBatcher
{

    public FrameBuffer FBO { get; private set; }

    private VertexBuffer _VBO;

    private VertexArray _VAO;

    private Dictionary<Layer, List<SpriteBatchItem>> _spriteBatchItems;

    private Shader _shader;

    private Vertex[] _vertices;

    private List<Layer> _layers;

    private Dictionary<Layer, Matrix4> _cameraProjections;

    private IComparer<SpriteBatchItem> _comparer = Comparer<SpriteBatchItem>.Create((x, y) =>
    {
        if (x.depth != y.depth || (x.texture == null || y.texture == null))
            return x.depth.CompareTo(y.depth);

        return x.texture.Handle.CompareTo(y.texture.Handle);
    });

    private Dictionary<Type, List<MaterialFieldData>> _materialUniformFields;

    private Dictionary<string, List<MaterialFieldData>> _materialUniformFieldsCache;

    private Dictionary<UniformType, Action<Shader, string, object>> _setUniformFunctions;

    private int _materialTextureUnit;

    private int _lastTexture;

    private int _currentTextureUnit;

    private float _time;

    private readonly int _currentMaxTexturesPerBatch;

    private const int MAX_SPRITES_PER_RENDER = 5461;

    private const int MAX_TEXTURES_PER_BATCH = 32;

    private float _pixelRatio;

    private Layer _lastLayer;

    public SpriteBatcher(Shader shader, List<Layer> layers, Dictionary<Layer, Matrix4> cameraProjections, Dictionary<Type, List<MaterialFieldData>> materialUniformFields, float pixelRatio)
    {
        _spriteBatchItems = new();

        _shader = shader;
        _layers = layers;
        _materialUniformFields = materialUniformFields;
        _cameraProjections = cameraProjections;
        _pixelRatio = pixelRatio;

        _vertices = new Vertex[4 * MAX_SPRITES_PER_RENDER];

        _currentMaxTexturesPerBatch = Math.Min(GL.GetInteger(GetPName.MaxTextureImageUnits), MAX_TEXTURES_PER_BATCH);

        Init();
    }

    public void Render(float time)
    {
        _time = time;

        if (_spriteBatchItems.Count == 0)
            return;

        _VAO.RenderBegin();
        FBO.Use();

        for (int i = 0; i < _layers.Count; i++)
        {
            var layer = _layers[i];
            if (layer.autoCleared)
            {
                FBO.UseAndClearTexture(layer.Texture, FramebufferAttachment.ColorAttachment0);
            }

            if (!_spriteBatchItems.ContainsKey(layer))
                continue;

            var items = _spriteBatchItems[layer];
            items.Sort(_comparer);

            if (items.Count == 0)
                continue;

          //  if (_lastLayer == null || (_lastLayer.pixelated != layer.pixelated))
            {
                if (layer.pixelated)
                    GL.Viewport(0, 0, 512, 512);
                else GL.Viewport(new System.Drawing.Rectangle(0, 0, MyGameWindow.ScreenSize.X, MyGameWindow.ScreenSize.Y));
            }

            int batchIndex = 0;
            int spritesToProcess;
            int index = 0;
            int processedSprites = 0;
            float depth = 0;

            for (int batchCount = items.Count; batchCount > 0; batchCount -= spritesToProcess)
            {
                spritesToProcess = Math.Min(batchCount, MAX_SPRITES_PER_RENDER);

                int notProcessedSprites = 0;
                SpriteBatchItem? item = null;

                var firstTexture = items[processedSprites].texture;
                GL.ActiveTexture((TextureUnit)(_currentTextureUnit + (int)TextureUnit.Texture0));

                if (firstTexture != null)
                {
                    _lastTexture = firstTexture;
                    firstTexture.Use();
                }

                for (int j = 0; j < spritesToProcess; j++)
                {
                    if (_currentTextureUnit == _currentMaxTexturesPerBatch)
                    {
                        notProcessedSprites = spritesToProcess - j;
                        index = 0;
                        break;
                    }

                    item = items[batchIndex++];

                    if (item.Value.material != null)
                    {
                        notProcessedSprites = spritesToProcess - j;

                        if (j > 0)
                        {
                            _shader.Use();
                            _shader.SetValue("cameraMatrix", _cameraProjections[layer]);
                            _VBO.UpdateData(_vertices);
                            _VAO.RenderElements(spritesToProcess - notProcessedSprites);

                            Array.Clear(_vertices);
                        }

                        index = 0;

                        break;
                    }

                    var texture = item.Value.texture;
                    if (_lastTexture != texture.Handle)
                    {
                        _currentTextureUnit++;
                        GL.ActiveTexture((TextureUnit)(_currentTextureUnit + (int)TextureUnit.Texture0));
                        texture.Use();

                        _lastTexture = texture.Handle;
                    }

                    for (int k = 0; k < 4; k++)
                        FillVertex(ref _vertices[index++], item.Value, depth, _currentTextureUnit);

                    depth += 0.0001f;
                }

                processedSprites = spritesToProcess - notProcessedSprites;

                if (item != null)
                {
                    if (item.Value.material == null)
                    {
                        _shader.Use();
                        _shader.SetValue("cameraMatrix", _cameraProjections[layer]);
                        _VBO.UpdateData(_vertices);
                        _VAO.RenderElements(processedSprites);

                        Array.Clear(_vertices);
                    }
                    else
                    {
                        batchCount--;

                        var material = item.Value.material;

                        if (material.TextureTarget != null)
                            throw new InvalidDataException($"Attempt to render using a {material.GetType().Name} material with initialized TextureTarget");

                        material.Shader.Use();
                        material.Shader.SetValue("cameraMatrix", _cameraProjections[layer]);
                        material.Shader.SetValue("time", _time);
                        if (item.Value.texture != null)
                        {
                            GL.ActiveTexture(TextureUnit.Texture0);
                            item.Value.texture.Use();
                        }
                        material.Shader.SetValue("quadSize", material.QuadSize);
                        material.Shader.SetValue("pixelRatio", _pixelRatio);

                        if (!_materialUniformFieldsCache.ContainsKey(material.Name))
                            _materialUniformFieldsCache.Add(material.Name, _materialUniformFields[material.GetType()]);

                        var materialDataList = _materialUniformFieldsCache[material.Name];

                        for (int k = 0; k < materialDataList.Count; k++)
                        {
                            var materialData = materialDataList[k];
                            var uniformType = materialData.uniformAttribute.uniformType;

                            _setUniformFunctions[uniformType](material.Shader, materialData.uniformAttribute.uniformName, materialData.field.GetValue(material));
                        }

                        foreach (var pair in material.Textures)
                        {
                            material.Shader.UseTexture(pair.Key, pair.Value, (TextureUnit)_materialTextureUnit);
                            _materialTextureUnit++;
                        }

                        for (int k = 0; k < 4; k++)
                            FillVertex(ref _vertices[k], item.Value, depth, 0);
                        depth += 0.001f;

                        _VBO.UpdateData(_vertices);
                        _VAO.RenderElements(1);

                        Array.Clear(_vertices);

                        processedSprites++;
                        _materialTextureUnit = (int)TextureUnit.Texture1;
                    }
                }

                _currentTextureUnit = 0;
                _lastTexture = 0;

                batchCount += notProcessedSprites;
            }

            items.Clear();
            _lastLayer = layer;
        }

        _VAO.RenderEnd();
        FBO.UseDefault();
    }

    public void DrawTexture(Texture texture, Layer layer, Material material, Vector2 position, Vector3 color, float alpha, Vector2 size, float angle, float depth, Vector2i offDir)
    {
        var frameInfo = new Vector3i(1, 1, 0);
        Vector4 color4 = new Vector4(color.X, color.Y, color.Z, alpha);

        DrawInternal(layer,
                    material,
                    texture,
                    position,
                    color4,
                    frameInfo,
                    size * offDir,
                    angle,
                    depth);
    }

    public void DrawSprite(Sprite sprite)
    {
        var frameInfo = sprite.Texture == null ? new Vector3i(1, 1, 0) : new(sprite.TexWidth / sprite.FrameWidth,
                                                                            sprite.TexHeight / sprite.FrameHeight,
                                                                            sprite.Current == null ? sprite.Frame : sprite.Current.Value.frames[sprite.Frame]);
        var offDir = new Vector2i(Utils.BoolToInt(!sprite.flippedHorizontally), Utils.BoolToInt(!sprite.flippedVertically));
        Vector4 color = new Vector4(sprite.color.X, sprite.color.Y, sprite.color.Z, sprite.alpha);

        DrawInternal(sprite.layer,
                    sprite.material == null || !sprite.material.IsApplying ? null : sprite.material,
                    sprite.Texture,
                    sprite.position + sprite.offset,
                    color,
                    frameInfo,
                    sprite.size * sprite.scale * offDir,
                    sprite.angle,
                    sprite.depth);
    }

    private void DrawInternal(Layer layer, Material material, Texture texture, Vector2 position, Vector4 color, Vector3i frameInfo, Vector2 size, float angle, float depth)
    {
        if (layer == Layer.MiddlePixelized)
        {

        }

        Matrix4 model = MyMath.CreateTransformMatrix(position, size, angle);

        if (material != null)
            material.QuadSize = new Vector2(MathF.Abs(size.X), MathF.Abs(size.Y));

        if (!_spriteBatchItems.ContainsKey(layer))
            _spriteBatchItems[layer] = new List<SpriteBatchItem>();

        var list = _spriteBatchItems[layer];
        list.Add(new SpriteBatchItem(new Vector4(model.Column0.X, model.Column0.Y, model.Column0.W, model.Column1.X),
                                     new Vector2(model.Column1.Y, model.Column1.W),
                                     depth, color, frameInfo, texture, layer, material));
    }

    private void FillVertex(ref Vertex vertex, SpriteBatchItem item, float depth, int textureUnit)
    {
        vertex.projection1 = item.projectionPart1;
        vertex.projection2 = item.projectionPart2;
        vertex.textureIndex = textureUnit;
        vertex.depth = depth;
        vertex.color = item.color;
        vertex.frameData = item.frameData;
    }

    public void Init()
    {
        uint[] indices = new uint[6 * MAX_SPRITES_PER_RENDER];

        uint index = 0;
        for (uint i = 0; i < indices.Length; i += 6)
        {
            indices[i] = index;
            indices[i + 1] = index + 1;
            indices[i + 2] = index + 3;

            indices[i + 3] = index + 1;
            indices[i + 4] = index + 2;
            indices[i + 5] = index + 3;

            index += 4;
        }

        _VAO = new VertexArray(PrimitiveType.Triangles);

        var EBO = new ElementsBuffer(6);
        EBO.SetData(indices);
        _VAO.SetElementsBuffer(EBO);

        _VBO = new VertexBuffer(BufferUsageHint.DynamicDraw, 1);
        _VBO.SetAttribPointerType<float>();
        _VBO.SetStride<Vertex>(1);
        _VBO.SetAttribPointers(4, 2, 1, 1, 4);
        _VBO.SetAttribPointer<int>(3);
        _VBO.SetEmptyData<Vertex>(_vertices.Length);
        _VAO.SetVertexBuffer(_VBO);

        FBO = new FrameBuffer(DrawBuffersEnum.ColorAttachment0, ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        var location = _shader.GetUniformLocation("textures");
        var sameplers = Enumerable.Range(0, _currentMaxTexturesPerBatch).ToArray();
        GL.Uniform1(location, sameplers.Length, ref sameplers[0]);

        _setUniformFunctions = new()
        {
            { UniformType.Float, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (float)value) },
            { UniformType.Vector2, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (Vector2)value) },
            { UniformType.Vector2i, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (Vector2i)value) },
            { UniformType.Vector3, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (Vector3)value) },
            { UniformType.Vector3i, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (Vector3i)value) },
            { UniformType.Vector4, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (Vector4)value) },
            { UniformType.Vector4i, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (Vector4)value) },
            { UniformType.Int, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (int)value) },
            { UniformType.Bool, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (bool)value) },
        };

        _materialUniformFieldsCache = new();
        _materialTextureUnit = (int)TextureUnit.Texture1;
    }
}