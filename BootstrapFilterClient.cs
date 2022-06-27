using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace DHT;

public class BootstrapFilter : IDisposable
{
    private static readonly Dictionary<string, int> AllBootstraps = new()
    {
        { "router.utorrent.com", 6881 },
        { "router.bittorrent.com", 6881 },
        { "dht.transmissionbt.com", 6881 }
    };
    private readonly Socket _socket;
    private readonly Dictionary<string, int> _bootstrapStation;

    public BootstrapFilter(in Dictionary<string, int> bootstrapStation)
    {
        _bootstrapStation = bootstrapStation;
        var localIPEndPoint = new IPEndPoint(IPAddress.Parse(Utils.GetLocalIP()), 0);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(localIPEndPoint);
        Filter();
    }

    private void Filter()
    {
        foreach (var bootstrap in AllBootstraps)
        {
            var remoteEntry = Dns.GetHostEntry(bootstrap.Key);
            var remoteAddr = remoteEntry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToArray();
            foreach (var ipAddress in remoteAddr)
            {
                if (!isValidBootstrap(ipAddress, bootstrap.Value)) continue;
                Console.WriteLine($"启动节点IP为{ipAddress}:{bootstrap.Value}");
                _bootstrapStation.Add(ipAddress.ToString(), bootstrap.Value);
            }
        }
    }

    private bool isValidBootstrap(IPAddress ipAddress, int port)
    {
        try
        {
            EndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);
            var krpc = new KRPC().SendPing(new Node().ID);
            var content = krpc.ConvertToBEncode().ToString();
            var bytes = content.GetBytes();
            _socket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, remoteEndPoint);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

            var rcvBytes = new byte[1024];
            var length = _socket.ReceiveFrom(rcvBytes, ref remoteEndPoint);
            if (length > 0)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            return false;
        }
        return false;
    }

    public void Dispose()
    {
        _socket.Dispose();
    }
}
