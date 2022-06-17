using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace DHT;

public class ResponseClient : DHTClientBase
{
    public ResponseClient(BlockingCollection<KRPC> messages) : base(messages)
    {
        _ = JoinDHT();
        _responseListener = new DHTListener(_socket);
    }

    private RouteTable Table => TransferCenter.Instance.Table;
    private DHTListener _responseListener;
    protected override void HandlerMessage(KRPC krpc)
    {
        if (krpc.MsgType != "r") return;

        if (krpc.Response.ContainsKey("nodes"))
        {
            var nodes = (krpc.Response["nodes"] as byte[]).AsSpan();
            for (var i = 0; i < nodes.Length; i += 26)
            {
                var node = new Node(nodes[i..(i + 26)]);
                Console.WriteLine(node.ToString());
                if (node.ID != Table.CurrentNode.ID)
                {
                    Table.TryAdd(node);
                }
            }
        }
    }

    private Task JoinDHT()
    {
        var i = 0;
        var index = 0;
        return Task.Run(() =>
        {
            while (true)
            {
                var node = Table.Shift();
                var krpc = new KRPC();
                IPEndPoint remoteEndPoint;

                if (node is null)
                {
                    i = ++i % Bootstraps.Count;
                    var bootstrap = Bootstraps.Skip(i).Take(1).ToArray()[0];
                    var remoteEntry = Dns.GetHostEntry(bootstrap.Key);
                    var remoteAddr = remoteEntry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToArray();
                    index = (++index) % remoteAddr.Length;
                    Console.WriteLine($"{i}:{index}");
                    Console.WriteLine(TransferCenter.Instance.DisabledBootstrapIPSet.Count);
                    Console.WriteLine(remoteAddr[index].ToString());
                    if (TransferCenter.Instance.DisabledBootstrapIPSet.Contains(remoteAddr[index].ToString()))
                    {
                        continue;
                    }
                    remoteEndPoint = new IPEndPoint(remoteAddr[index], bootstrap.Value);
                    krpc = krpc.FindNode(Table.CurrentNode.ID);
                }
                else
                {
                    remoteEndPoint = new IPEndPoint(IPAddress.Parse(node.DecodeIP), node.DecodePort);
                    krpc = krpc.FindNode(Table.Neighbor(node.ID));
                }
                var content = krpc.ConvertToBEncode().ToString();
                var bytes = Encoding.ASCII.GetBytes(content);
                _socket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, remoteEndPoint);
                Thread.Sleep(3000);
            }
        });
    }
}
