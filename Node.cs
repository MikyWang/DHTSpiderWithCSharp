using System.Security.Cryptography;
using System.Text;
namespace DHT;

public class Node
{
    public byte[] ID { get; private set; }
    public byte[] IP { get; private set; }
    public byte[] Port { get; private set; }

    public string DecodeID => string.Join("", ID.Select(x => $"{x:x2}"));
    public string DecodeIP => string.Join(".", IP);
    public int DecodePort => BitConverter.ToInt16(Port);

    public Node()
    {
        var random = new byte[20];
        using (var ctx = RandomNumberGenerator.Create())
        {
            ctx.GetBytes(random);
        }
        ID = SHA1.Create().ComputeHash(random);
        var localIP = Utils.GetLocalIP();
        IP = localIP.Split(".").Select(x => (byte)int.Parse(x)).ToArray();
        Port = BitConverter.GetBytes(6881)[..2];
    }

    public Node(ReadOnlySpan<byte> nodeInfo)
    {
        ID = nodeInfo[..20].ToArray();
        IP = nodeInfo[20..24].ToArray();
        Port = nodeInfo[24..26].ToArray();
    }

    public IEnumerable<byte> Encode()
    {
        return ID.Concat(IP).Concat(Port);
    }

    public override string ToString()
    {
        return $"{DecodeID}{DecodeIP}/{DecodePort}";
    }

}
