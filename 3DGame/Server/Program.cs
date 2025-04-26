using System.Net.Sockets;
using System.Net;

namespace GameServerCS
{
    public class Program : Socket
    {
        public Program() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            base.Bind(new IPEndPoint(IPAddress.Any, 8080));
            base.Listen(100);
            base.AcceptAsync(new Server(this));
            base.ReceiveBufferSize = 256 * 1024;
            base.SendBufferSize = 256 * 1024;
        }
    }
}