using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

    public static string GetLocalIP()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        socket.Connect("8.8.8.8", 65530);
        if (socket.LocalEndPoint is not IPEndPoint endPoint) throw new Exception("无有效的网络适配器！");
        return endPoint.Address.ToString();
    }

    public static bool IsValidPort(int port)
    {
        return port is > 1 and < 1 << 16;
    }

}
