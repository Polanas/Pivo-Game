using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class TextButton : GUIItem
{

    public string Text { get; set; }

    public Vector3i Color { get; set; }

    private string _fontName;

    private float _scale;

    public TextButton(Vector2 position, string fontName, string text, Vector3i color, float scale) : base(position, TextHelper.GetTextSize(text, fontName, scale))
    {
        _fontName = fontName;
        _scale = scale;
        Text = text;
        Color = color;
    }

    public override void Update()
    {
        base.Update();

        Vector3i color = Color;

        if (Collided)
            color = new Vector3i(255) - Color;

        Graphics.DrawText(Text, _fontName, Transform.position, color, _scale, true);
    }
}
