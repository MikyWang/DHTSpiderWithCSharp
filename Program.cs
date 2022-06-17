using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DHT;

var dhtListener = new DHTListener();
var responseClient = new ResponseClient(TransferCenter.Instance.ResponseMessages);
TransferCenter.Instance.DealMessage();

// var random = new Random();
// var remoteEntry = Dns.GetHostEntry("dht.transmissionbt.com");
// var remoteAddr = remoteEntry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToArray();
// var index = 1;
// EndPoint remoteEndPoint = new IPEndPoint(remoteAddr[index], 6881);
// var krpc = new KRPC().FindNode(new Node().ID);
// var bencode = krpc.ConvertToBEncode().ToString();
// var bytes = Encoding.ASCII.GetBytes(bencode);
// var localIPEndPoint= new IPEndPoint(IPAddress.Parse(Utils.GetLocalIP()), 0);
// var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
// socket.Bind(localIPEndPoint);
// socket.SendTo(bytes, 0, bytes.Length, SocketFlags.None, remoteEndPoint);
// var rcvBytes = new byte[1024];
// EndPoint rcvEndPoint = new IPEndPoint(IPAddress.Any, 0);
// var length = socket.ReceiveFrom(rcvBytes, ref rcvEndPoint);
// Console.WriteLine(length);
