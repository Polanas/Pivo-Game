using System.ComponentModel.DataAnnotations;

namespace Game;

static class KeyDataExtensions
{
    public static KeyData SetPressed(this KeyData keyData) => KeyData.GetPressed(keyData.Name);

    public static KeyData SetDown(this KeyData keyData) => KeyData.GetDown(keyData.Name);

    public static KeyData SetDownPressed(this KeyData keyData) => KeyData.GetDownPressed(keyData.Name);

    public static KeyData SetReleased(this KeyData keyData) => KeyData.GetReleased(keyData.Name);

    public static KeyData SetNotPressed(this KeyData keyData) => KeyData.GetNotPressed(keyData.Name);
}

readonly record struct KeyData(string Name, bool Pressed, bool Down, bool Released)
{
    public static KeyData GetPressed(string Name) => new KeyData(Name, true, false, false);

    public static KeyData GetDown(string Name) => new KeyData(Name, false, true, false);

    public static KeyData GetDownPressed(string Name) => new KeyData(Name, true, true, false);

    public static KeyData GetReleased(string Name) => new KeyData(Name, false, false, true);

    public static KeyData GetNotPressed(string Name) => new KeyData(Name, false, false, false);
}

