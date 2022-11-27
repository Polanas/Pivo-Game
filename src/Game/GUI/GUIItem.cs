using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class GUIItem
{

    public event Action OnUpdate { add => _onUpdate += value; remove => _onUpdate -= value; }

    public event Action OnMouseRelease { add => _onMouseRelease += value; remove => _onMouseRelease -= value; }

    public event Action OnClick { add => _onClick += value; remove => _onClick -= value; }

    public event Action OnMouseEnter { add => _onMouseEnter += value; remove => _onMouseEnter -= value; }

    public event Action OnMouseLeave { add => _onMouseLeave += value; remove => _onMouseLeave -= value; }

    public bool Collided { get; set; }

    public bool Clicked { get; set; }

    public bool ClickedDown { get; set; }

    public ref Transform Transform { get => ref _transform; }

    private Transform _transform;

    private event Action _onClick;

    private event Action _onMouseRelease;

    private event Action _onMouseEnter;

    private event Action _onMouseLeave;

    private event Action _onUpdate;

    public virtual void Click() => _onClick?.Invoke();

    public virtual void Update() => _onUpdate?.Invoke();

    public virtual void MouseEntered() => _onMouseEnter?.Invoke();

    public virtual void MouseLeft() => _onMouseLeave?.Invoke();

    public virtual void MouseReleased() => _onMouseRelease?.Invoke();

    public GUIItem(Vector2 position, Vector2 bounds)
    {
        _transform = new(position, bounds);
    }

    public GUIItem() { }
}
