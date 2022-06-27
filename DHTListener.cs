using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace DHT;

public class DHTListener : IDisposable
{
    public string IP { get; private set; }
    public int Port { get; private set; }
    public Socket Socket { get; }

    private EndPoint? _remotePoint;
    private readonly BlockingCollection<DHTMessage> _messages;
    public DHTListener(in BlockingCollection<DHTMessage> messages, int port = 6881)
    {
        _messages = messages;
        IP = Utils.GetLocalIP();
        Port = port;
        Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        var localPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
        Socket.Bind(localPoint);
    }
    public Task Listen()
    {
        return Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    _remotePoint = new IPEndPoint(IPAddress.Any, 0);
                    var bytes = new byte[1024];
                    var length = Socket.ReceiveFrom(bytes, ref _remotePoint);
                    var ip = (_remotePoint as IPEndPoint)?.Address.ToString();
                    var port = (_remotePoint as IPEndPoint)?.Port;
                    var message = new DHTMessage { Data = bytes[..length], IP = ip ?? string.Empty, Port = port ?? 0 };
                    _messages.Add(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        });
    }

    public void Dispose()
    {
        Socket.Dispose();
    }
}
