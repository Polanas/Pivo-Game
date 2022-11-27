using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class NetworkData : IGameData
{

    public bool isServer;

    public bool isClient;

    public bool isActive;

    public int port;

    public string host;

    public string gameName;

    public EcsSystems networkSystems;

    public NetworkData(string gameName)
    {
        this.gameName = gameName;
    }
}
