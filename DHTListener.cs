using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace DHT;

public class DHTListener
{
    public string IP { get; private set; }
    public int Port { get; private set; }

    private readonly Socket _socket;
    private EndPoint? _remotePoint;
    private BlockingCollection<DHTMessage> Messages => TransferCenter.Instance.DHTMessages;
    public DHTListener(int port = 6881)
    {
        IP = Utils.GetLocalIP();
        Port = port;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        var localPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
        _socket.Bind(localPoint);
        _ = Receive();
    }

    public DHTListener(Socket socket)
    {
        IP = (socket.LocalEndPoint as IPEndPoint)?.Address.ToString() ?? Utils.GetLocalIP();
        Port = (socket.LocalEndPoint as IPEndPoint)?.Port ?? 0;
        _socket = socket;
        _ = Receive();
    }

    private Task Receive()
    {
        return Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    _remotePoint = new IPEndPoint(IPAddress.Any, 0);
                    _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                    var bytes = new byte[1024];
                    var length = _socket.ReceiveFrom(bytes, ref _remotePoint);
                    var ip = (_remotePoint as IPEndPoint)?.Address.ToString();
                    var port = (_remotePoint as IPEndPoint)?.Port;
                    var message = new DHTMessage { Data = bytes[..length], IP = ip ?? string.Empty, Port = port ?? 0 };
                    Messages.Add(message);
                }
                catch (Exception e)
                {
                    var disabledIPs = TransferCenter.Instance.DisabledBootstrapIPSet;
                    var ip = (_socket.RemoteEndPoint as IPEndPoint)?.Address.ToString();
                    if (ip is not null && ip != string.Empty)
                    {
                        disabledIPs.Add(ip);
                    }
                }

            }
        });
    }

}
