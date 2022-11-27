using System;

namespace Game;

struct PlayerInputs
{
    public KeyData left;

    public KeyData right;

    public KeyData up;

    public KeyData down;

    public KeyData useTongue;

    public void SetDirectionKeyData(sbyte direction)
    {
        if (direction == 1)
        {
            right = right.SetDownPressed();
            return;
        }

        left = left.SetDownPressed();
    }

    public void SetDefaultKeyData()
    {
        left = Input.GetKeyData("left");
        right = Input.GetKeyData("right");
        up = Input.GetKeyData("up");
        down = Input.GetKeyData("down");
        useTongue = Input.GetKeyData("useTongue");
    }

    public void SetEmptyKeyData( )
    {
        left = KeyData.GetNotPressed("left");
        right = KeyData.GetNotPressed("right");
        up = KeyData.GetNotPressed("up");
        down = KeyData.GetNotPressed("down");
        useTongue = KeyData.GetNotPressed("useTongue");
    }
}
