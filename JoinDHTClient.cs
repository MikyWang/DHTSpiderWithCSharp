using System.Collections.Concurrent;
namespace DHT;

public class JoinDHTClient : DHTClientBase
{
    private RouteTable Table => TransferCenter.Instance.Table;
    private Dictionary<string, int> Bootstraps => TransferCenter.Instance.ValidBootstraps;
    private readonly ConcurrentQueue<Node> _responseNodes = new();
    public JoinDHTClient(DHTListener dhtListener, in BlockingCollection<DHTMessage> messages) : base(dhtListener, in messages)
    {
        _ = JoinDHT();
    }
    protected override void HandlerMessage(DHTMessage message)
    {
        try
        {
            var krpc = message.ConvertToKRPC(ref Context);
            if (krpc.MsgType != "r") return;
            if (!krpc.Response.ContainsKey("nodes")) return;
            if (krpc.Response["nodes"] is not string content) return;
            if (krpc.Response["id"] is not string { Length: 20 } sID) return;
            if (krpc.Response.ContainsKey("samples"))
            {
                if (krpc.Response["samples"] is string infohashes)
                {
                    HandlerInfoHashes(infohashes.GetBytes());
                }
            }
            var id = sID.GetBytes();
            var knownNode = new Node(id, message.IP, message.Port);
            TransferCenter.Instance.KnownNodes.TryAdd(knownNode);
            if (content.Length % 26 != 0) return;
            var nodes = content.GetBytes();
            for (var i = 0; i < nodes.Length; i += 26)
            {
                var node = new Node(nodes.AsSpan(i, 26));
                if (node.ID == Table.CurrentNode.ID || !Utils.IsValidPort(node.DecodePort)) continue;
                if (_responseNodes.Count < 200)
                    _responseNodes.Enqueue(node);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void HandlerInfoHashes(byte[] infohashes)
    {
        for (var i = 0; i < infohashes.Length; i += 20)
        {
            var infohash = infohashes.AsSpan(i, 20).ToArray();
            Console.WriteLine($"收到sample_infohashes响应,磁力链接[magnet:?xt=urn:btih:{infohash.PrintHex()}]");
        }
    }

    private Task JoinDHT()
    {
        var index = 0;
        var useSample = false;
        return Task.Run(() =>
        {
            var krpc = new KRPC();
            while (true)
            {
                if (_responseNodes.TryDequeue(out var node))
                {
                    var id = Table.Neighbor(node.ID);
                    krpc = useSample ? krpc.SampleInfoHashes(id) : krpc.FindNode(id);
                    useSample = !useSample;
                    Send(krpc, node.DecodeIP, node.DecodePort);
                    Thread.Sleep(1);
                }
                else
                {
                    if (Bootstraps.Count == 0) continue;
                    index = index++ % Bootstraps.Count;
                    var bootstrap = Bootstraps.Skip(index).Take(1).ToArray()[0];
                    krpc = krpc.FindNode(Table.CurrentNode.ID);
                    Send(krpc, bootstrap.Key, bootstrap.Value);
                    Thread.Sleep(1000);
                }
            }
        });
    }
}
