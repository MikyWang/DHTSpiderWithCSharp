using System.Collections;
using System.Security.Cryptography;
using System.Text;
namespace DHT;

public class KRPC
{
    [KRPCName("t")]
    public string TransactionID { get; set; } = string.Empty;
    [KRPCName("y")]
    public string MsgType { get; set; } = string.Empty;
    [KRPCName("q")]
    public string Request { get; set; } = string.Empty;
    [KRPCName("a")]
    public Dictionary<string, object> Body { get; set; } = new();
    [KRPCName("r")]
    public Dictionary<string, object> Response { get; set; } = new();
    [KRPCName("e")]
    public ArrayList Error { get; set; } = new();
    public Dictionary<string, string> Mapping { get; private set; } = new(6);

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
                Mapping.Add(propertyInfo.Name, attr.Name);
            }
        }
    }

    public KRPC SendPing(string id)
    {
        var bytes = new byte[2];
        using (var ctx = RandomNumberGenerator.Create())
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

    public KRPC FindNode(byte[] id)
    {

        var bytes = new byte[2];
        using (var ctx = RandomNumberGenerator.Create())
        {
            ctx.GetBytes(bytes);
        }
        TransactionID = Encoding.ASCII.GetString(bytes);
        MsgType = "q";
        Request = "find_node";
        var sID = Encoding.ASCII.GetString(id);
        if (Body.ContainsKey("id"))
        {
            Body["id"] = sID;
        }
        else
        {
            Body.Add("id", sID);
        }
        if (Body.ContainsKey("target"))
        {
            Body["target"] = Encoding.ASCII.GetString(new Node().Encode().ToArray());
        }
        else
        {
            Body.Add("target", Encoding.ASCII.GetString(new Node().Encode().ToArray()));
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
            "q" => Mapping.Where(m => m.Value != "r" && m.Value != "e").ToDictionary(x => x.Key, x => x.Value),
            "r" => Mapping.Where(m => m.Value != "q" && m.Value != "a" && m.Value != "e").ToDictionary(x => x.Key, x => x.Value),
            "e" => Mapping.Where(m => m.Value != "q" && m.Value != "a" && m.Value != "r").ToDictionary(x => x.Key, x => x.Value),
            _ => throw new Exception("不支持的消息类型!")
        };
    }

}
