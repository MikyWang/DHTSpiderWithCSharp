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
    public static IEnumerable<byte> GenerateID(int length)
    {
        var bytes = new byte[length];
        using var ctx = RandomNumberGenerator.Create();
        ctx.GetBytes(bytes);
        return bytes;
    }
    public static async Task<string> GetPublicIP()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("http://pv.sohu.com/cityjson?ie=utf-8");
        var resStream = await response.Content.ReadAsStreamAsync();
        var sr = new StreamReader(resStream, Encoding.Default);
        var html = await sr.ReadToEndAsync();
        var r = new Regex("((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)\\.){3}(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|[1-9])", RegexOptions.None);
        var mc = r.Match(html);
        var ip = mc.Groups[0].Value;
        resStream.Close();
        sr.Close();
        return ip;
    }

    public static void CreateDHT(int port = 6881)
    {
        var dhtListener = new DHTListener(TransferCenter.Instance.DHTMessages, port);
        dhtListener.Listen();
        new JoinDHTClient(dhtListener, TransferCenter.Instance.ResponseMessages).HandlerMessages();
        new PingResponseClient(dhtListener, TransferCenter.Instance.SendPingRequestMessages).HandlerMessages();
        new FindNodeResponseClient(dhtListener, TransferCenter.Instance.FindNodeRequestMessages).HandlerMessages();
        new GetPeersResponseClient(dhtListener, TransferCenter.Instance.GetPeersRequestMessages).HandlerMessages();
        new AnnouncePeerResponseClient(dhtListener, TransferCenter.Instance.AnnouncePeerRequestMessages).HandlerMessages();
    }


}
