using System.Collections;
using System.Collections.Concurrent;
namespace DHT;

public class RouteTable : BlockingCollection<Node>
{
    public Node CurrentNode { get; private set; }

    public RouteTable()
    {
        CurrentNode = new Node();
    }

    public Node? Shift()
    {
        TryTake(out var first);
        return first;
    }

    public byte[] Neighbor(byte[] nodeID)
    {
        return CurrentNode.ID[..6].Concat(nodeID[6..20]).ToArray();
    }
    
    public ReadOnlySpan<byte> NearestNodes()
    {
        var bytes = this.First().Encode();
        bytes = this.Skip(1).Take(7).Aggregate(bytes, (current, node) => current.Concat(node.Encode()));
        return bytes.ToArray();
    }

}
