using System.Collections.Concurrent;
using System.Text;
namespace DHT;

public sealed class TransferCenter
{
    public static TransferCenter Instance { get; } = new();
    public RouteTable Table { get; } = new();
    public BlockingCollection<DHTMessage> DHTMessages { get; } = new();
    public BlockingCollection<KRPC> ResponseMessages { get; } = new();
    public BlockingCollection<KRPC> SendPingRequestMessages { get; } = new();
    public BlockingCollection<KRPC> FindNodeRequestMessages { get; } = new();
    public BlockingCollection<KRPC> GetPeersRequestMessages { get; } = new();
    public BlockingCollection<KRPC> AnnouncePeerRequestMessages { get; } = new();
    public HashSet<string> DisabledBootstrapIPSet { get; } = new();
    private TransferCenter() { }
    public void DealMessage()
    {
        foreach (var message in DHTMessages.GetConsumingEnumerable())
        {
            Console.WriteLine($"{message.IP}:{message.Port}");
            var krpc = new BEncode(message.Data).ConvertToKRPC();
            switch (krpc.MsgType)
            {
                case "r":
                    ResponseMessages.Add(krpc);
                    break;
                case "q":
                    HandlerRequest(krpc);
                    break;
            }
        }
    }
    private void HandlerRequest(KRPC krpc)
    {
        if (krpc.MsgType != "q") return;

        switch (krpc.Request)
        {
            case "ping":
                SendPingRequestMessages.Add(krpc);
                break;
            case "find_node":
                FindNodeRequestMessages.Add(krpc);
                break;
            case "get_peers":
                GetPeersRequestMessages.Add(krpc);
                break;
            case "announce_peer":
                AnnouncePeerRequestMessages.Add(krpc);
                break;
        }
    }
}
