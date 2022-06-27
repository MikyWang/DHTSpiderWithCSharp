using System.Collections.Concurrent;
using System.Text;
namespace DHT;

public class FindNodeResponseClient : DHTClientBase
{

    public FindNodeResponseClient(DHTListener dhtListener, in BlockingCollection<DHTMessage> messages) : base(dhtListener, in messages) { }
    protected override void HandlerMessage(DHTMessage message)
    {
        var krpc = message.ConvertToKRPC(ref Context);
        if (krpc.Body["id"] is not string { Length: 20 } sID) return;
        if (krpc.Body["target"] is not string { Length: 20 } sTarget) return;
        var id = sID.GetBytes();
        var target = sTarget.GetBytes();
        var neighbor = TransferCenter.Instance.Table.Neighbor(id);
        var nearestNodes = TransferCenter.Instance.Table.NearestNodes();
        krpc.MsgType = "r";
        krpc.Response.SetValue("id",neighbor.GetString());
        krpc.Response.SetValue("nodes",nearestNodes.GetString());
        Send(krpc, message.IP, message.Port);
    }
}
