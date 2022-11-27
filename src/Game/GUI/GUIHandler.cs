using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class GUIHandler
{
    private List<GUIItem> _items;

    public GUIHandler()
    {
        _items = new();
    }

    public void AddItem(GUIItem item)
    {
        _items.Add(item);
    }

    public void Remove(GUIItem item)
    {
        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public void Update()
    {
        Vector2 mousePos = Mouse.ScreenPosition - (Vector2)MyGameWindow.ScreenSize / (2 * MyGameWindow.FullToPixelatedRatio);

        for (int i = 0; i < _items.Count; i++)
        {
            GUIItem item = _items[i];

            if (Collision.Point(mousePos, item.Transform))
            {
                item.ClickedDown = Input.Down("menuLeft");

                if (!item.Collided)
                {
                    item.MouseEntered();
                    item.Collided = true;
                }

                if (Input.Pressed("menuLeft"))
                {
                    item.Clicked = true;
                    item.Click();
                }
                if (!item.ClickedDown && item.Clicked)
                {
                    item.Clicked = false;
                    item.MouseReleased();
                }
            }
            else
            {
                item.ClickedDown = false;

                if (item.Collided)
                {
                    item.Collided = false;
                    item.MouseLeft();
                }
            }

            item.Update();
        }
    }
}