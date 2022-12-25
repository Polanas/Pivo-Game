using System;
using OpenTK.Graphics.OpenGL4;

namespace Game;

class CreateAtlasSystem : IEcsInitSystem
{

    struct VirtualTextureData
    {
        public Dictionary<int, Vector2i> positions;
        public Vector2i atlasPos;
        public Vector2i size;

        public VirtualTextureData(Vector2i atlasPos, Vector2i size)
        {
            positions = new();
            this.atlasPos = atlasPos;
            this.size = size;
        }
    }

    private Atlas _atlas;

    private AtlasMaterial _atlasMaterial;

    public void Init(EcsSystems systems)
    {
        var sharedData = systems.GetShared<SharedData>();

        _atlasMaterial = Material.Create<AtlasMaterial>(Content.LoadEmptyTexture(new Vector2i(4096), "atlasTexture"));
        _atlas = Atlas.FromDirectories(_atlasMaterial.TextureTarget.Size, MyPath.TexturesMetaDataDirectory,
                                       new string[] {  MyPath.TexturesDirectory,
                                       MyPath.Join(MyPath.TexturesDirectory, "Tilesets") });

        _atlasMaterial.cameraMatrix = MyMath.CreateCameraMatrix((Vector2)_atlasMaterial.TextureTarget.Size / 2f, _atlasMaterial.TextureTarget.Size);

        string textureString = "spriteTexture";
        Dictionary<Texture, VirtualTextureData> textureFramePoses = new();

        for (int i = 0; i < _atlas.Rects.Count; i++)
        {
            var rect = _atlas.Rects[i];

            _atlasMaterial.modelMatix = MyMath.CreateTransformMatrix(rect.pos + (Vector2)rect.size / 2f, rect.size);
            _atlasMaterial.Textures[textureString] = rect.texture;
            _atlasMaterial.frameData = new Vector3i(rect.texture.Width / rect.size.X, rect.texture.Height / rect.size.Y, rect.frame);

            sharedData.renderData.toTextureRenderer.Render(_atlasMaterial, false);

            if (!textureFramePoses.ContainsKey(rect.texture))
                textureFramePoses.Add(rect.texture, new VirtualTextureData(rect.pos, rect.size));

            textureFramePoses[rect.texture].positions.Add(rect.frame, rect.pos);
        }

        foreach (var pair in textureFramePoses)
        {
            var tex = pair.Key;
            var data = pair.Value;

            Content.AddVirtualTexture(new VirtualTexture(data.atlasPos, data.size, tex.name, data.positions));
        }

        for (int i = 0; i < _atlas.Rects.Count; i++)
        {
            var rect = _atlas.Rects[i];

            if (Content.HasTexture(rect.texture.name))
                Content.DeleteTexture(rect.texture);
        }

        sharedData.renderData.atlasTexture = _atlasMaterial.TextureTarget;
        _atlas = null;
        _atlasMaterial = null;
    }
}