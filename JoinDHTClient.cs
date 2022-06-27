using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace DHT;

public class JoinDHTClient : DHTClientBase
{
    private RouteTable Table => TransferCenter.Instance.Table;
    private Dictionary<string, int> Bootstraps => TransferCenter.Instance.ValidBootstraps;
    public JoinDHTClient(DHTListener dhtListener, in BlockingCollection<DHTMessage> messages) : base(dhtListener, in messages)
    {
        _ = JoinDHT();
    }
    protected override void HandlerMessage(DHTMessage message)
    {
        var krpc = message.ConvertToKRPC(ref Context);
        if (krpc.MsgType != "r") return;
        if (!krpc.Response.ContainsKey("nodes")) return;
        if (krpc.Response["nodes"] is not string content) return;

        var nodes = content.GetBytes().AsSpan();
        for (var i = 0; i < nodes.Length; i += 26)
        {
            var node = new Node(nodes[i..(i + 26)]);
            if (node.ID == Table.CurrentNode.ID || !Utils.IsValidPort(node.DecodePort)) continue;
            Table.TryAdd(node);
        }
    }

    private Task JoinDHT()
    {
        var index = 0;
        return Task.Run(() =>
        {
            var krpc = new KRPC();
            while (true)
            {
                var node = Table.Shift();
                if (node is null)
                {
                    if (Bootstraps.Count == 0) continue;
                    index = index++ % Bootstraps.Count;
                    var bootstrap = Bootstraps.Skip(index).Take(1).ToArray()[0];
                    krpc = krpc.FindNode(Table.CurrentNode.ID);
                    Send(krpc, bootstrap.Key, bootstrap.Value);
                    Thread.Sleep(3000);
                }
                else
                {
                    krpc = krpc.FindNode(Table.Neighbor(node.ID));
                    Send(krpc, node.DecodeIP, node.DecodePort);
                    Thread.Sleep(1);
                }
            }
        });
    }
}
