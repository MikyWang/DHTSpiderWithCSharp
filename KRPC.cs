using System.Collections;
using System.Security.Cryptography;
using System.Text;
namespace DHT;

public class KRPC
{
    [KRPCName("t")]
    public string TransactionID { get; private set; } = string.Empty;
    [KRPCName("y")]
    public string MsgType { get; private set; } = string.Empty;
    [KRPCName("q")]
    public string Request { get; private set; } = string.Empty;
    [KRPCName("a")]
    public Dictionary<string, object> Body { get; private set; } = new();
    [KRPCName("r")]
    public Dictionary<string, object> Response { get; private set; } = new();
    [KRPCName("e")]
    public ArrayList Error { get; private set; } = new();

    private readonly Dictionary<string, string> _mapping = new(6);

    public bool IsRequest => MsgType == "q";
    public bool IsResponse => MsgType == "r";
    public bool IsError => MsgType == "e";

    public KRPC()
    {
        var pInfo = GetType().GetProperties();
        foreach (var propertyInfo in pInfo)
        {
            var attrs = propertyInfo.GetCustomAttributes(typeof(KRPCNameAttribute), false);
            if (attrs.Length > 0 && attrs.First(x => x is KRPCNameAttribute) is KRPCNameAttribute attr)
            {
                _mapping.Add(propertyInfo.Name, attr.Name);
            }
        }
    }

    public KRPC SendPing(string id)
    {
        var bytes = new byte[2];
        using (var ctx=RandomNumberGenerator.Create())
        {
            ctx.GetBytes(bytes);
        }
        id = Utils.HexToString(id);
        TransactionID = Encoding.ASCII.GetString(bytes);

        MsgType = "q";
        Request = "ping";
        if (Body.ContainsKey("id"))
        {
            Body["id"] = id;
        }
        else
        {
            Body.Add("id", id);
        }
        return this;
    }

    public KRPC FindNode(string id, string target)
    {
        id = Utils.HexToString(id);
        target = Utils.HexToString(target);
        TransactionID = Encoding.ASCII.GetString(new byte[] { 0, 1 });
        MsgType = "q";
        Request = "find_node";
        if (Body.ContainsKey("id"))
        {
            Body["id"] = id;
        }
        else
        {
            Body.Add("id", id);
        }
        if (Body.ContainsKey("target"))
        {
            Body["target"] = target;
        }
        else
        {
            Body.Add("target", target);
        }

        return this;
    }

    public BEncode ConvertToBEncode()
    {
        var type = GetType();
        var mapping = FilterMapping();
        var dic = new Dictionary<string, object>();
        foreach (var m in mapping)
        {
            var value = type.GetProperty(m.Key)?.GetValue(this);
            dic.Add(m.Value, value ?? string.Empty);
        }
        var be = new BEncode();
        be.Add(dic);
        return be;
    }

    private Dictionary<string, string> FilterMapping()
    {
        return MsgType switch
        {
            "q" => _mapping.Where(m => m.Value != "r" && m.Value != "e").ToDictionary(x => x.Key, x => x.Value),
            "r" => _mapping.Where(m => m.Value != "q" && m.Value != "a" && m.Value != "e").ToDictionary(x => x.Key, x => x.Value),
            "e" => _mapping.Where(m => m.Value != "q" && m.Value != "a" && m.Value != "r").ToDictionary(x => x.Key, x => x.Value),
            _ => throw new Exception("不支持的消息类型!")
        };
    }

}
