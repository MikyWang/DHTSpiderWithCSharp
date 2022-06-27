using System.Collections.Concurrent;
using System.Text;
namespace DHT;

public class GetPeersResponseClient : DHTClientBase
{
    public GetPeersResponseClient(DHTListener dhtListener, in BlockingCollection<DHTMessage> messages) : base(dhtListener, in messages) { }
    protected override void HandlerMessage(DHTMessage message)
    {
        var krpc = message.ConvertToKRPC(ref Context);
        if (!krpc.Body.ContainsKey("id")) return;
        if (!krpc.Body.ContainsKey("info_hash")) return;
        if (krpc.Body["id"] is not string { Length: 20 } id) return;
        if (krpc.Body["info_hash"] is not string { Length: 20 } infoHash) return;
        DealTorrent(infoHash.GetBytes());
        var neighbor = TransferCenter.Instance.Table.Neighbor(id.GetBytes());
        var nearestNodes = TransferCenter.Instance.Table.NearestNodes();
        var token = TransferCenter.Instance.TokenManager.Token;
        krpc.MsgType = "r";
        krpc.Response.SetValue("id", neighbor.GetString());
        krpc.Response.SetValue("token", token.GetString());
        krpc.Response.SetValue("nodes", nearestNodes.GetString());
        Send(krpc, message.IP, message.Port);
    }
    private static void DealTorrent(byte[] torrent)
    {
        Console.WriteLine($"收到get_peers请求,torrent[{torrent.PrintHex()}]");
    }
}
