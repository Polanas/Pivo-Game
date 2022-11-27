using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

static class Keyboard
{
    public static KeyboardState State => _game.KeyboardState;

    public static void Init(MyGameWindow game) => _game = game;

    public static bool Pressed(Keys key) => State.IsKeyPressed(key);

    public static bool Released(Keys key) => State.IsKeyReleased(key);

    public static bool PressedAny() => State.IsAnyKeyDown;
    
    public static bool Down(Keys key) => State.IsKeyDown(key);

    public static bool Down(params Keys[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (State.IsKeyDown(keys[i]))
                return true;
        }

        return false;
    }

    private static MyGameWindow _game;
}

