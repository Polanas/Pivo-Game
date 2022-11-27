using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

struct LoadFont : IEventReplicant
{
    public string path;

    public string name;

    public uint size;

    public LoadFont(string path, string name, uint size)
    {
        this.path = path;
        this.size = size;
        this.name = name;
    }
}
