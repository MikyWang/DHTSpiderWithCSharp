using System.Collections;
using System.Reflection;
namespace DHT;

public static class DHTExtentions
{
    public static KRPC ConvertToKRPC(this BEncode bEncode)
    {
        var krpc = new KRPC();
        var type = krpc.GetType();
        if (bEncode.Read()[0] is not Dictionary<string, object> dict)
        {
            throw new ArgumentException("不是正确的KRPC协议B编码报文！");
        }
        foreach (var map in krpc.Mapping)
        {
            if (!dict.ContainsKey(map.Value)) continue;
            var propertyInfo = type.GetProperty(map.Key);
            propertyInfo?.SetValue(krpc, dict[map.Value]);
        }
        return krpc;
    }
}
