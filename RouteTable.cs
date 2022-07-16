using System.Diagnostics;
namespace DHT;

public class RouteTable
{
    public Node CurrentNode { get; }
    public KBucketNode Root { get; }

    public RouteTable()
    {
        var ip = Utils.GetPublicIP().Result;
        CurrentNode = new Node(ip);
        Root = new KBucketNode(0)
        {
            Bucket = new List<Node> { CurrentNode }
        };
        _ = HandlerKnownNodes();
    }
    public void AddNode(Node node)
    {
        var bucketNode = FindBucket(node.ID);
        if (bucketNode.Bucket is not null && bucketNode.IsFull)
        {
            var hasFound = bucketNode.FindAndReplaceNode(node);
            if (hasFound) return;
            if (!bucketNode.HasNode(CurrentNode)) return;

            foreach (var bn in bucketNode.Bucket.Where(bn => !bn.Distance(CurrentNode).IsZero()))
            {
                TransferCenter.Instance.KnownNodes.TryAdd(bn);
            }
            bucketNode.Split(CurrentNode);
            TransferCenter.Instance.KnownNodes.Add(node);
        }
        else
        {
            if (bucketNode.HasNode(node)) return;
            bucketNode.Bucket?.Add(node);
            bucketNode.Bucket?.Sort();
        }
    }

    public Task HandlerKnownNodes()
    {
        return Task.Run(() =>
        {
            foreach (var node in TransferCenter.Instance.KnownNodes.GetConsumingEnumerable())
            {
                AddNode(node);
            }
        });
    }
    public byte[] Neighbor(byte[] nodeID)
    {
        return nodeID[..6].Concat(CurrentNode.ID[6..20]).ToArray();
    }
    public IEnumerable<byte> NearestNodes(byte[] nodeID)
    {
        var bucketNode = FindBucket(nodeID);
        if (bucketNode.Bucket is null) return CurrentNode.Encode();

        var count = bucketNode.Bucket.Count;
        var bytes = bucketNode.Bucket[0].Encode();
        bytes = bucketNode.Bucket.Skip(1).Take(count - 1).Aggregate(bytes, (current, node) => current.Concat(node.Encode()));
        var left = KBucketNode.K - count;
        for (var i = 0; i < left; i++)
        {
            bytes = bytes.Concat(CurrentNode.Encode());
        }
        return bytes;
    }
    private KBucketNode FindBucket(byte[] nodeID)
    {
        var bucketNode = Root;
        while (bucketNode is not null && bucketNode.Bucket is null)
        {
            var dis = CurrentNode.ID.XOR(nodeID);
            var index = (int)MathF.Ceiling((float)(bucketNode.Bit + 1) / 8);
            var move = bucketNode.Bit % 8;
            var div = (byte)(dis[index - 1] << move & 0b10000000);
            bucketNode = div == 0 ? bucketNode.LeftChild : bucketNode.RightChild;
        }
        return bucketNode ?? Root;
    }

}
