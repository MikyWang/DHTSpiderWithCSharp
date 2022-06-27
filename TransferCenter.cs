using System.Collections.Concurrent;
using System.Text;
namespace DHT;

public sealed class TransferCenter : IDisposable
{
    public static TransferCenter Instance { get; } = new();
    public RouteTable Table { get; } = new();
    public TokenManager TokenManager { get; } = new();
    public BlockingCollection<DHTMessage> DHTMessages { get; } = new();
    public BlockingCollection<DHTMessage> ResponseMessages { get; } = new();
    public BlockingCollection<DHTMessage> SendPingRequestMessages { get; } = new();
    public BlockingCollection<DHTMessage> FindNodeRequestMessages { get; } = new();
    public BlockingCollection<DHTMessage> GetPeersRequestMessages { get; } = new();
    public BlockingCollection<DHTMessage> AnnouncePeerRequestMessages { get; } = new();
    public Dictionary<string, int> ValidBootstraps { get; } = new();
    private KRPC _context = new KRPC();
    private TransferCenter() { }
    public void DealMessage()
    {
        foreach (var message in DHTMessages.GetConsumingEnumerable())
        {
            try
            {
                var krpc = message.ConvertToKRPC(ref _context);
                switch (krpc.MsgType)
                {
                    case "r":
                        ResponseMessages.Add(message);
                        break;
                    case "q":
                        HandlerRequest(message);
                        break;
                }
            }
            catch (Exception e)
            {
                continue;
            }

        }
    }
    private void HandlerRequest(DHTMessage message)
    {
        var krpc = message.ConvertToKRPC(ref _context);
        if (krpc.MsgType != "q") return;

        switch (krpc.Request)
        {
            case "ping":
                SendPingRequestMessages.Add(message);
                break;
            case "find_node":
                FindNodeRequestMessages.Add(message);
                break;
            case "get_peers":
                GetPeersRequestMessages.Add(message);
                break;
            case "announce_peer":
                AnnouncePeerRequestMessages.Add(message);
                break;
        }
    }
    public void Dispose()
    {
        Table.Dispose();
        DHTMessages.Dispose();
        ResponseMessages.Dispose();
        SendPingRequestMessages.Dispose();
        FindNodeRequestMessages.Dispose();
        GetPeersRequestMessages.Dispose();
        AnnouncePeerRequestMessages.Dispose();
    }
}
