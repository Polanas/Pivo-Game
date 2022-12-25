using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class GameData : IGameData
{

    public Vector2 ingameMousePosition;

    public Input Input { get; set; }

    public readonly SFX SFX;

    public readonly MyGameWindow gameWindow;

    public readonly Camera camera;

    public readonly Dictionary<string, IEcsSystem[]> groupSystems;

    public readonly GroupState groupStateSetter;

    public readonly TextHelper textHelper;

    public GameData(Camera camera, MyGameWindow game, SFX sfx, GroupState groupStateSetter, TextHelper textHelper)
    {
        this.camera = camera;
        SFX = sfx;
        this.gameWindow = game;
        this.textHelper = textHelper;
        this.groupStateSetter = groupStateSetter;

        groupSystems = new();
    }
}