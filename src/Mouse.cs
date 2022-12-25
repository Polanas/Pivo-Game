using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

static class Mouse
{
    public static bool InTheRightSide => _game.MousePosition.X >= MyGameWindow.ScreenSize.X / 2;

    public static bool InTheLeftSize => _game.MousePosition.X <= MyGameWindow.ScreenSize.X / 2;

    public static Vector2 ScreenPositionCentered => (_game.MousePosition - MyGameWindow.ScreenSize / 2) / MyGameWindow.FullToPixelatedRatio;

    public static Vector2 ScreenPosition => _game.MousePosition / MyGameWindow.FullToPixelatedRatio;

    public static Vector2 Velocity => _game.MouseState.Delta;

    public static MouseState State => _game.MouseState;

    private static MyGameWindow _game;

    public static void Init(MyGameWindow game) => _game = game;

    public static bool Down(MouseButton button) => State.IsButtonDown(button);

    public static bool Pressed(MouseButton button) => !State.WasButtonDown(button) && State.IsButtonDown(button);

    public static bool Released(MouseButton button) => State.WasButtonDown(button) && !State.IsButtonDown(button);

    public static bool Any() => State.IsAnyButtonDown;
}