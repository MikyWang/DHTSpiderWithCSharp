using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using DHT;

using var filter = new BootstrapFilter(TransferCenter.Instance.ValidBootstraps);
var dhtListener = new DHTListener(TransferCenter.Instance.DHTMessages, 6881);
var listen = dhtListener.Listen();
var joinDHT = new JoinDHTClient(dhtListener, TransferCenter.Instance.ResponseMessages).HandlerMessages();
var pingResponse = new PingResponseClient(dhtListener, TransferCenter.Instance.SendPingRequestMessages).HandlerMessages();
var findNodeResponse = new FindNodeResponseClient(dhtListener, TransferCenter.Instance.FindNodeRequestMessages).HandlerMessages();
var getPeersResponse = new GetPeersResponseClient(dhtListener, TransferCenter.Instance.GetPeersRequestMessages).HandlerMessages();
var announcePeerResponse = new AnnouncePeerResponseClient(dhtListener, TransferCenter.Instance.AnnouncePeerRequestMessages).HandlerMessages();
TransferCenter.Instance.DealMessage();

