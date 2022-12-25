using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Box2DSharp.Dynamics;
using Leopotam.EcsLite.Di;
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
    public Vector2 atlasPos;

    [FieldOffset(32)]
    public float depth;

    [FieldOffset(36)]
    public Vector4 color;

    [FieldOffset(52)]
    public Vector3i frameData;
}

class SpriteBatcher : InjectableService
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
        if (x.depth != y.depth || x.texture == null || y.texture == null)
            return x.depth.CompareTo(y.depth);

        if (x.texture is not Texture xTexture || y.texture is not Texture yTexture)
            return 1;

        return xTexture.Handle.CompareTo(yTexture.Handle);
    });

    private Dictionary<Type, List<MaterialFieldData>> _materialUniformFields;

    private Dictionary<string, List<MaterialFieldData>> _materialUniformFieldsCache;

    private Dictionary<UniformType, Action<Shader, string, object>> _setUniformFunctions;

    private int _materialTextureUnit = (int)TextureUnit.Texture2;

    private int _currentTextureUnit = 1;

    private int _currentMaxTexturesPerBatch;

    private const int MAX_SPRITES_PER_RENDER = 5461;

    private const int MAX_TEXTURES_PER_BATCH = 31;

    private const float DEPTH_INCREMENT = 0.001f;

    private float _pixelRatio;

    private Texture _atlasTexture;

    private Material _lastMaterial;

    private int _veticesCount;

    private int _itemsCount;

    private float _time;

    private List<SpriteBatchItem> _currentItems;

    public void Render(float time)
    {
        if (_spriteBatchItems.Count == 0)
            return;

        _time = time;

        _VAO.RenderBegin();
        FBO.Use();

        for (int i = 0; i < _layers.Count; i++)
        {
            var currentLayer = _layers[i];

            FBO.UseAndClearTexture(currentLayer.Texture, FramebufferAttachment.ColorAttachment0);

            if (!_spriteBatchItems.ContainsKey(currentLayer))
                continue;

            _currentItems = _spriteBatchItems[currentLayer];
            if (_currentItems.Count == 0)
                continue;

            _currentItems.Sort(_comparer);

            if (currentLayer.pixelated)
                OpenGL.SetViewport(Vector2i.Zero, currentLayer.Texture.Size);
            else OpenGL.SetViewport(Vector2i.Zero, MyGameWindow.ScreenSize);

            _veticesCount = 0;
            _itemsCount = 0;

            float depth = 0;

            for (int j = 0; j < _currentItems.Count; j++)
            {
                var item = _currentItems[j];

                var currentMaterial = item.material;

                if (currentMaterial != null && currentMaterial != _lastMaterial)
                    Flush(currentLayer, null);

                depth += DEPTH_INCREMENT;
                _itemsCount++;

                FillVerices(item, depth);
                if (item.texture is Texture texture)
                    BindTexture(texture);

                if (_itemsCount > MAX_SPRITES_PER_RENDER
                    || j == _currentItems.Count - 1 
                    || (currentMaterial != null && currentMaterial != _lastMaterial)
                    || _currentTextureUnit == 32)
                    Flush(currentLayer, currentMaterial);

                _lastMaterial = currentMaterial;
            }

            _currentItems.Clear();
        }

        RenderEnd();
    }

    public void DrawTexture(ITexture texture, Layer layer, Material material, Vector2 position, Vector3 color, float alpha, Vector2 size, float angle, float depth, Vector2i offDir)
    {
        var frameInfo = new Vector3i(1, 1, 0);
        Vector4 color4 = new Vector4(color.X, color.Y, color.Z, alpha);

        Submit(layer,
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
        var frameInfo = Utils.GetFrameData(sprite.VirtualTexture,
                                           new Vector2i(sprite.FrameWidth, sprite.FrameHeight),
                                           sprite.Current == null ? sprite.Frame : sprite.Current.Value.frames[sprite.Frame]);

        var offDir = new Vector2i(Utils.BoolToInt(!sprite.flippedHorizontally), Utils.BoolToInt(!sprite.flippedVertically));
        Vector4 color = new Vector4(sprite.color.X, sprite.color.Y, sprite.color.Z, sprite.alpha);

        Submit(sprite.layer,
                    sprite.material == null || !sprite.material.IsApplying ? null : sprite.material,
                    sprite.VirtualTexture,
                    sprite.position + sprite.offset,
                    color,
                    frameInfo,
                    sprite.size * sprite.scale * offDir,
                    sprite.angle,
                    sprite.depth);
    }

    private void BindTexture(Texture texture)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + _currentTextureUnit);
        texture.Use();
        _currentTextureUnit++;
    }

    private void FillVerices(SpriteBatchItem item, float depth)
    {
        var texture = item.texture;
        bool isTexture = texture is Texture;
        Vector2 atlasPos = Vector2.Zero;

        if (texture != null)
            atlasPos = isTexture ? new Vector2(_currentTextureUnit, -1) : (texture as VirtualTexture).FramesPositions[item.frameData.Z];
        for (int k = 0; k < 4; k++)
            FillVertex(ref _vertices[_veticesCount++],
                       item,
                       depth,
                       atlasPos);
    }

    private void Flush(Layer layer, Material material)
    {
        var shader = material == null ? _shader : material.Shader;

        shader.Use();
        shader.UseTexture("atlasTexture", _atlasTexture, TextureUnit.Texture0);
        shader.SetValue("cameraMatrix", _cameraProjections[layer]);
        _VBO.UpdateData(_vertices);

        if (material != null)
            ApplyMaterial(material);

        _VAO.RenderElements(_itemsCount);

        _itemsCount = 0;
        _veticesCount = 0;
        _currentTextureUnit = 1;

        Array.Clear(_vertices);
    }

    private void ApplyMaterial(Material material)
    {
        var shader = material.Shader;

        shader.SetValue("time", _time);
        shader.SetValue("quadSize", material.QuadSize);
        shader.SetValue("pixelRatio", _pixelRatio);

        if (!_materialUniformFieldsCache.ContainsKey(material.Name))
            _materialUniformFieldsCache.Add(material.Name, _materialUniformFields[material.GetType()]);

        var lastItem = _currentItems[_itemsCount - 1];

        var materialDataList = _materialUniformFieldsCache[material.Name];

        for (int k = 0; k < materialDataList.Count; k++)
        {
            var materialData = materialDataList[k];
            var uniformType = materialData.uniformAttribute.uniformType;

            _setUniformFunctions[uniformType](material.Shader, materialData.uniformAttribute.uniformName, materialData.field.GetValue(material));
        }

        _materialTextureUnit = 1;
        foreach (var pair in material.Textures)
        {
            if (pair.Value == null)
                continue;

            if (pair.Value is Texture texture)
            {
                shader.UseTexture(pair.Key, texture, (TextureUnit)_materialTextureUnit);
                _materialTextureUnit++;
                continue;
            }

            var virtualTexture = pair.Value as VirtualTexture;

            shader.SetValue($"{pair.Key}.atlasPos", (Vector2)virtualTexture.AtlasPosition);
            shader.SetValue($"{pair.Key}.size", (Vector2)virtualTexture.Size);
        }
    }

    public void Submit(Layer layer, Material material, ITexture texture, Vector2 position, Vector4 color, Vector3i frameInfo, Vector2 size, float angle, float depth)
    {
        if (texture != null)
            frameInfo = texture is Texture ? frameInfo : new Vector3i(((VirtualTexture)texture).Size, frameInfo.Z);

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

    public void SubmitDirect(Layer layer, Material material, ITexture texture, Vector4 projection1, Vector2 projection2, Vector4 color, Vector3i frameInfo, float depth)
    {
        if (texture != null)
            frameInfo = texture is Texture ? frameInfo : new Vector3i(((VirtualTexture)texture).Size, frameInfo.Z);

        if (!_spriteBatchItems.ContainsKey(layer))
            _spriteBatchItems[layer] = new List<SpriteBatchItem>();

        var list = _spriteBatchItems[layer];
        list.Add(new SpriteBatchItem(projection1,projection2,
                                     depth, color, frameInfo, texture, layer, material));
    }

    private void FillVertex(ref Vertex vertex, SpriteBatchItem item, float depth, Vector2 atlasPos)
    {
        vertex.projection1 = item.projectionPart1;
        vertex.projection2 = item.projectionPart2;
        vertex.atlasPos = atlasPos;
        vertex.depth = depth;
        vertex.color = item.color;
        vertex.frameData = item.frameData;
    }

    private void RenderEnd()
    {
        _itemsCount = 0;
        _veticesCount = 0;

        _VAO.RenderEnd();
        FBO.UseDefault();
    }

    public void Init(Shader shader, List<Layer> layers, Dictionary<Layer, Matrix4> cameraProjections, Dictionary<Type, List<MaterialFieldData>> materialUniformFields, float pixelRatio, Texture atlasTexture)
    {
        _spriteBatchItems = new();

        _shader = shader;
        _layers = layers;
        _materialUniformFields = materialUniformFields;
        _cameraProjections = cameraProjections;
        _pixelRatio = pixelRatio;
        _atlasTexture = atlasTexture;

        _vertices = new Vertex[4 * MAX_SPRITES_PER_RENDER];

        _currentMaxTexturesPerBatch = Math.Min(GL.GetInteger(GetPName.MaxTextureImageUnits), MAX_TEXTURES_PER_BATCH);

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
        _VBO.SetAttribPointers(4, 2, 2, 1, 4);
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
            { UniformType.Matrix4, (Shader shader, string unfiromName, object value) => shader.SetValue(unfiromName, (Matrix4)value) },
        };

        _materialUniformFieldsCache = new();
        _materialTextureUnit = (int)TextureUnit.Texture1;
    }
}