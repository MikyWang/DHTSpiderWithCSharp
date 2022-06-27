using System.Security.Cryptography;
using System.Text;
namespace DHT;

public class Node : IEquatable<Node>
{
    public byte[] ID { get; private set; }
    public byte[] IP { get; private set; }
    public byte[] Port { get; private set; }

    public string DecodeID => string.Join("", ID.Select(x => $"{x:x2}"));
    public string DecodeIP => string.Join(".", IP);
    public int DecodePort => BitConverter.ToInt32(Port);


    public Node(int port = 6881)
    {
        var random = new byte[20];
        using (var ctx = RandomNumberGenerator.Create())
        {
            ctx.GetBytes(random);
        }
        ID = SHA1.Create().ComputeHash(random);
        var localIP = Utils.GetLocalIP();
        IP = localIP.Split(".").Select(x => (byte)int.Parse(x)).ToArray();
        Port = BitConverter.GetBytes(port);
    }
    public Node( string ip, int port = 6881)
    {
        var random = new byte[20];
        using (var ctx = RandomNumberGenerator.Create())
        {
            ctx.GetBytes(random);
        }
        ID = SHA1.Create().ComputeHash(random);
        IP = ip.Split(".").Select(x => (byte)int.Parse(x)).ToArray();
        Port = BitConverter.GetBytes(port);
    }
    public Node(ReadOnlySpan<byte> nodeInfo)
    {
        ID = nodeInfo[..20].ToArray();
        IP = nodeInfo[20..24].ToArray();
        Port = new byte[] { 0x00, 0x00 };
        Port = Port.Concat(nodeInfo[24..26].ToArray()).ToArray();
        Array.Reverse(Port);
    }

    public IEnumerable<byte> Encode()
    {
        if (Port.Clone() is not byte[] port)
        {
            throw new Exception("端口格式不对！");
        }
        Array.Reverse(port);
        return ID.Concat(IP).Concat(port[2..]);
    }

    public override string ToString()
    {
        return $"{DecodeID}{DecodeIP}/{DecodePort}";
    }

    public bool Equals(Node? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return ID.Equals(other.ID);
    }
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((Node)obj);
    }
    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}
