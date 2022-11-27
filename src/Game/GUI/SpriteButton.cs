using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class SpriteButton : GUIItem
{

    private Sprite _sprite;

    private string _clickSound;

    public SpriteButton(Sprite sprite, Vector2 position, string clickSound = null) : base(position, new Vector2(sprite.FrameWidth, sprite.FrameHeight))
    {
        _sprite = sprite;
        _sprite.position = position;
        _clickSound = clickSound;

        _sprite.SetAnimation("idle");
    }

    public override void Update()
    {
        Graphics.DrawSprite(_sprite);

        base.Update();
    }

    public override void Click()
    {
        _sprite.SetAnimation("clicked");
        SFX.Play(_clickSound);

        base.Click();
    }

    public override void MouseEntered()
    {
        _sprite.SetAnimation("collided");

        base.MouseEntered();
    }

    public override void MouseLeft()
    {
        _sprite.SetAnimation("idle");

        base.MouseLeft();
    }

    public override void MouseReleased()
    {
        _sprite.SetAnimation("collided");

        base.MouseReleased();
    }
}
