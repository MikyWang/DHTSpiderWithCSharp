using System.Collections.Concurrent;
using System.Text;
namespace DHT;

public class PingResponseClient : DHTClientBase
{
    public PingResponseClient(DHTListener dhtListener, in BlockingCollection<DHTMessage> messages) : base(dhtListener, in messages) { }
    protected override void HandlerMessage(DHTMessage message)
    {
        try
        {
            var krpc = message.ConvertToKRPC(ref Context);
            if (!krpc.Body.ContainsKey("id")) return;
            if (krpc.Body["id"] is not string { Length: 20 } sID) return;
            var nID = sID.GetBytes();
            var neighbor = TransferCenter.Instance.Table.Neighbor(nID);
            krpc.Response.SetValue("id",neighbor.GetString());
            krpc.MsgType = "r";
            Send(krpc, message.IP, message.Port);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
    }
}
