using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
namespace DHT;

public class Utils
{
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
    public static byte[] GenerateTransactionID()
    {
        var bytes = new byte[2];
        using var ctx = RandomNumberGenerator.Create();
        ctx.GetBytes(bytes);
        return bytes;
    }
  
    public static string GetPublicIP()
    {
        string tempip = "";
        WebRequest request = WebRequest.Create("http://pv.sohu.com/cityjson?ie=utf-8");
        request.Timeout = 10000;
        WebResponse response = request.GetResponse();
        Stream resStream = response.GetResponseStream();
        StreamReader sr = new StreamReader(resStream, System.Text.Encoding.Default);
        string htmlinfo = sr.ReadToEnd();
        Regex r = new Regex("((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)\\.){3}(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|[1-9])", RegexOptions.None);
        Match mc = r.Match(htmlinfo);
        tempip = mc.Groups[0].Value;
        resStream.Close();
        sr.Close();
        return tempip;
    }

}
