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
    public static Dictionary<string, string> Mapping { get; } = new(6);

    public bool IsRequest => MsgType == "q";
    public bool IsResponse => MsgType == "r";
    public bool IsError => MsgType == "e";

    static KRPC()
    {
        var pInfo = typeof(KRPC).GetProperties();
        foreach (var propertyInfo in pInfo)
        {
            var attrs = propertyInfo.GetCustomAttributes(typeof(KRPCNameAttribute), false);
            if (attrs.Length > 0 && attrs.First(x => x is KRPCNameAttribute) is KRPCNameAttribute attr)
            {
                Mapping.Add(propertyInfo.Name, attr.Name);
            }
        }
    }

    public KRPC SendPing(IEnumerable<byte> id)
    {
        TransactionID = Utils.GenerateID(2).GetString();
        MsgType = "q";
        Request = "ping";
        var sID = id.GetString();
        Body.SetValue("id", sID);
        return this;
    }

    public KRPC FindNode(IEnumerable<byte> id)
    {
        TransactionID = Utils.GenerateID(2).GetString();
        MsgType = "q";
        Request = "find_node";
        var sID = id.GetString();
        Body.SetValue("id", sID);
        Body.SetValue("target", new Node().ID.GetString());
        return this;
    }

    public KRPC SampleInfoHashes(IEnumerable<byte> id)
    {
        TransactionID = Utils.GenerateID(2).GetString();
        MsgType = "q";
        Request = "sample_infohashes";
        var sID = id.GetString();
        Body.SetValue("id", sID);
        Body.SetValue("target", new Node().ID.GetString());
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
