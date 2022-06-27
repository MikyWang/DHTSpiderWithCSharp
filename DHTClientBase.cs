using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace DHT;

public abstract class DHTClientBase
{
    protected BlockingCollection<DHTMessage> Messages { get; }
    protected readonly Socket Socket;
    protected KRPC Context;
    protected abstract void HandlerMessage(DHTMessage message);
    protected DHTClientBase(DHTListener dhtListener, in BlockingCollection<DHTMessage> messages)
    {
        Socket = dhtListener.Socket;
        Messages = messages;
        Context = new KRPC();
    }
    public Task HandlerMessages()
    {
        return Task.Run(() =>
        {
            foreach (var message in Messages.GetConsumingEnumerable())
            {
                try
                {
                    HandlerMessage(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        });
    }
    protected void Send(KRPC krpc, string ip, int port)
    {
        if (ip == string.Empty || !Utils.IsValidPort(port)) return;

        var remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        var content = krpc.ConvertToBEncode().ToString();
        var bytes = content.GetBytes();
        Socket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, remoteEndPoint);
    }
}
