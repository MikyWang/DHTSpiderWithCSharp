namespace DHT;

public class KBucketNode
{
    public const int K = 8;
    public List<Node>? Bucket { get; set; }
    public KBucketNode? LeftChild;
    public KBucketNode? RightChild;
    public int Bit { get; }
    public bool IsFull => Bucket?.Count == 8;
    public bool IsEmpty => Bucket is null && LeftChild is null && RightChild is null;

    public KBucketNode(int bit)
    {
        Bit = bit;
    }
    public bool HasNode(Node node)
    {
        return Bucket?.Find(n => n.Distance(node).IsZero()) is not null;
    }
    public bool FindAndReplaceNode(Node node)
    {
        var found = Bucket?.Find(n => n.Distance(node).IsZero());
        if (found is null) return false;
        Bucket?.Remove(found);
        Bucket?.Add(node);
        return true;
    }
    public void Split(Node self)
    {
        Bucket = null;
        LeftChild = new KBucketNode(Bit + 1)
        {
            Bucket = new List<Node>(8)
            {
                self
            }
        };
        RightChild = new KBucketNode(Bit + 1)
        {
            Bucket = new List<Node>(8)
        };
    }
}
