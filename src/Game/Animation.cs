using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

struct Animation
{
    public string name;

    public float delay;

    public bool loop;

    public int[] frames;

    public Animation(string name, float delay, bool loop, int[] frames)
    {
        this.name = name;
        this.delay = delay;
        this.loop = loop;
        this.frames = frames;
    }

    public static bool operator ==(Animation a1, Animation a2) => a1.Equals(a2);

    public static bool operator !=(Animation a1, Animation a2) => !a1.Equals(a2);

    public static bool operator ==(Animation a, string str) => a.name == str;

    public static bool operator !=(Animation a, string str) => a.name != str;

    public override bool Equals(object obj)
    {
        if (obj is not Animation)
            return false;

        Animation a = (Animation)obj;
        return a.name == name && a.delay == delay && a.loop == loop && Enumerable.SequenceEqual(a.frames, frames);
    }

    public override int GetHashCode() => GetHashCode();
}
