using System.Collections.Concurrent;
using System.Text;
namespace DHT;

public class AnnouncePeerResponseClient : DHTClientBase
{
    public AnnouncePeerResponseClient(DHTListener dhtListener, in BlockingCollection<DHTMessage> messages) : base(dhtListener, in messages) { }
    protected override void HandlerMessage(DHTMessage message)
    {
        var krpc = message.ConvertToKRPC(ref Context);
        if (!krpc.Body.ContainsKey("id")) return;
        if (!krpc.Body.ContainsKey("info_hash")) return;
        if (!krpc.Body.ContainsKey("port")) return;
        if (!krpc.Body.ContainsKey("token")) return;
        if (krpc.Body["id"] is not string { Length: 20 } sID) return;
        if (krpc.Body["info_hash"] is not string { Length: 20 } infoHash) return;
        if (krpc.Body["port"] is not int imPort) return;
        if (krpc.Body["token"] is not string token) return;
        var validToken = TransferCenter.Instance.TokenManager.Token;
        // if (token.GetBytes() != validToken) return;
        var port = message.Port;
        if (krpc.Body.ContainsKey("implied_port") && krpc.Body["implied_port"] is int impliedPort && impliedPort != 0)
        {
            port = imPort;
        }
        DealTorrent(infoHash.GetBytes(), message.IP, port);
        var id = sID.GetBytes();
        var knownNode = new Node(id, message.IP, message.Port);
        TransferCenter.Instance.KnownNodes.TryAdd(knownNode);
        var neighbor = TransferCenter.Instance.Table.Neighbor(infoHash.GetBytes());
        krpc.MsgType = "r";
        krpc.Response.SetValue("id", neighbor.GetString());
        Send(krpc, message.IP, message.Port);
    }
    private static void DealTorrent(byte[] torrent, string ip, int port)
    {
        Console.WriteLine($"收到announce_peer请求,磁力链接[magnet:?xt=urn:btih:{torrent.PrintHex()},地址为{ip}:{port}]");
    }
}
