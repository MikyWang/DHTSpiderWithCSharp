using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
namespace DHT;

public class Utils
{
    public static string StringToHex(string source)
    {
        var sb = new StringBuilder();
        var bytes = Encoding.ASCII.GetBytes(source);
        foreach (var ch in bytes)
        {
            var hex = $"{ch:x2}";
            sb.Append(hex);
        }
        return sb.ToString();
    }
    public static string HexToString(string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            hexString += "20"; //添加空格
        }
        var bytes = new byte[hexString.Length / 2];
        var hex = hexString.AsSpan();
        for (var i = 0; i < bytes.Length; i++)
        {
            try
            {
                bytes[i] = byte.Parse(hex.Slice(i * 2, 2), NumberStyles.HexNumber);
            }
            catch
            {
                throw new ArgumentException("参数类型必须为16进制字符串");
            }
        }
        return Encoding.ASCII.GetString(bytes);
    }
}
