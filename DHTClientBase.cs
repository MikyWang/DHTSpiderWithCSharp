using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
namespace DHT;

public abstract class DHTClientBase : IDisposable
{
    protected static readonly Dictionary<string, int> Bootstraps = new()
    {
        { "router.utorrent.com", 6881 },
        { "router.bittorrent.com", 6881 },
        { "dht.transmissionbt.com", 6881 }
    };
    protected BlockingCollection<KRPC> Messages { get; }
    protected readonly Socket _socket;
    protected abstract void HandlerMessage(KRPC krpc);
    protected IPEndPoint LocalIPEndPoint { get; }
    protected DHTClientBase(BlockingCollection<KRPC> messages)
    {
        Messages = messages;
        LocalIPEndPoint = new IPEndPoint(IPAddress.Parse(Utils.GetLocalIP()), 0);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(LocalIPEndPoint);
        _ = HandlerMessages();
    }
    private Task HandlerMessages()
    {
        return Task.Run(() =>
        {
            foreach (var krpc in Messages.GetConsumingEnumerable())
            {
                HandlerMessage(krpc);
            }
        });
    }

    public void Dispose()
    {
        _socket.Close();
    }
}
