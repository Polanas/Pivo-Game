using ldtk;

namespace Game;

class Level
{

    public LdtkJson LdtkLevel => _ldtkLevel;

    public Dictionary<string, TilesetDefinition> TileSets => _tileSets;

    private LdtkJson _ldtkLevel;

    private Dictionary<string, TilesetDefinition> _tileSets = new();

    public Level(LdtkJson ldtkLevel)
    {
        _ldtkLevel = ldtkLevel;

        for (int i = 0; i < _ldtkLevel.Defs.Tilesets.Length; i++)
        {
            var tileset = _ldtkLevel.Defs.Tilesets[i];

            if (tileset.RelPath == null)
                continue;

            _tileSets.Add(Path.GetFileNameWithoutExtension(tileset.RelPath), tileset);
        }
    }
}