using System.Collections;
using System.Collections.Concurrent;
namespace DHT;

public class RouteTable : BlockingCollection<Node>
{
    public Node CurrentNode { get; private set; }

    public RouteTable()
    {
        var ip = Utils.GetPublicIP();
        CurrentNode = new Node(ip);
    }

    public Node? Shift()
    {
        TryTake(out var first);
        return first;
    }

    public byte[] Neighbor(byte[] nodeID)
    {
        return nodeID[..6].Concat(CurrentNode.ID[6..20]).ToArray();
    }

    public byte[] NearestNodes()
    {
        IEnumerable<byte> bytes;
        if (Count < 8)
        {
            bytes = CurrentNode.Encode();
            for (var i = 0; i < 7; i++)
            {
                bytes = bytes.Concat(CurrentNode.Encode());
            }
        }
        else
        {
            bytes = this.First().Encode();
            bytes = this.Skip(1).Take(7).Aggregate(bytes, (current, node) => current.Concat(node.Encode()));
        }
        return bytes.ToArray();
    }

}
