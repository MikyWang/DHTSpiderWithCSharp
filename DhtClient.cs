using System.Net.Sockets;
namespace DHT;

public class DhtClient
{
    private static readonly Dictionary<string, int> IPAddress = new()
    {
        { "router.utorrent.com", 6881 },
        { "router.bittorrent.com", 6881 },
        { "dht.transmissionbt.com", 6881 }
    };

}
