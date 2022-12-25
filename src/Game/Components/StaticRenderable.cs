using System.Runtime.CompilerServices;

namespace Game;

struct StaticRenderable
{
    public SpriteBatchItem batchItem;

	public StaticRenderable(SpriteBatchItem batchItem)
	{
		this.batchItem = batchItem;
	}
}
