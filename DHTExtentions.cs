using System.Collections;
using System.Reflection;
using System.Text;
namespace DHT;

public static class DHTExtentions
{
    public static string PrintHex(this byte[] src)
    {
        var dst = new StringBuilder();
        dst = src.Aggregate(dst, (s, b) => s.Append($"{b:X2}"));
        return dst.ToString();
    }
    public static byte[] GetBytes(this string src)
    {
        IEnumerable<byte> bytes = new[] { Convert.ToByte(src[0]) };
        bytes = src.Skip(1).Aggregate(bytes, (b, ch) => b.Concat(new[] { Convert.ToByte(ch) }));
        return bytes.ToArray();
    }
    public static string GetString(this IEnumerable<byte> src)
    {
        var dst = new StringBuilder();
        dst = src.Aggregate(dst, (s, b) => s.Append(Convert.ToChar(b)));
        return dst.ToString();
    }
    public static ref KRPC ConvertToKRPC(this BEncode bEncode, ref KRPC krpc)
    {
        var type = krpc.GetType();
        if (bEncode.Read()[0] is not Dictionary<string, object> dict)
        {
            throw new ArgumentException("不是正确的KRPC协议B编码报文！");
        }
        foreach (var map in KRPC.Mapping)
        {
            if (!dict.ContainsKey(map.Value)) continue;
            var propertyInfo = type.GetProperty(map.Key);
            propertyInfo?.SetValue(krpc, dict[map.Value]);
        }
        return ref krpc;
    }
    public static ref KRPC ConvertToKRPC(this DHTMessage message, ref KRPC krpc)
    {
        return ref new BEncode(message.Data).ConvertToKRPC(ref krpc);
    }

    public static void SetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }

}
