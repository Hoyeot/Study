using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO.Compression;
using System.Collections;
using System.IO;

namespace CsServerAsync
{
    class Client : SocketAsyncEventArgs
    {
        public static ArrayList clientList = new ArrayList();
        private Socket socket;
        private StringBuilder sb = new StringBuilder();
        private IPEndPoint ipep;

        public Client(Socket socket)
        {
            this.socket = socket;
            base.SetBuffer(new byte[1024], 0, 1024); // 메모리 버퍼 초기화
            base.UserToken = socket;
            // 메시지 오면 IOCP에서 꺼냄
            base.Completed += Client_Completed;
            // 메시지 오면 IOCP에 넣음
            this.socket.ReceiveAsync(this);

            ipep = (IPEndPoint)socket.RemoteEndPoint;
            Console.WriteLine($"Client : {ipep.Address.ToString()} : {ipep.Port} Accept!");
            clientList.Add(socket);
        }
        private void Client_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (socket.Connected && base.BytesTransferred > 0) // 접속체크
            {
                byte[] data = e.Buffer; // e.Buffer -> 수신 데이터
                string msg = Encoding.ASCII.GetString(data); // data string형 변환
                base.SetBuffer(new byte[1024], 0, 1024); // 메모리 버퍼 초기화
                sb.Append(msg.Trim('\0')); // 공백 제거

                Console.WriteLine($"[{ipep.Port}] : {msg}");

                foreach (Socket socket_async in clientList)
                {
                    if (sb.Length > 0)
                    {
                        msg = sb.ToString();
                        Send($"[{ipep.Port}] {msg}\r\n", socket_async);
                    }
                }

                if ("exit".Equals(msg, StringComparison.OrdinalIgnoreCase))
                {
                    clientList.Remove(socket);
                    Console.WriteLine($"Client Disconnected [{ipep.Port}]");
                    socket.DisconnectAsync(this);
                    return;
                }
                sb.Clear();
                this.socket.ReceiveAsync(this); // 메시지가 오면 이벤트 발생 (IOCP에 넣음)
            }
            else
            {
                Console.WriteLine($"Client Disconnected [{ipep.Port}]");
                clientList.Remove(socket);
            }
        }
        private SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        private void Send(String msg, Socket socket) // client로 msg를 전송함
        {
            byte[] sendData = Encoding.ASCII.GetBytes(msg);
            sendArgs.SetBuffer(sendData, 0, sendData.Length);
            socket.SendAsync(sendArgs); // 비동기 전송
        }
    }
    class Server : SocketAsyncEventArgs
    {
        private Socket socket;
        public Server(Socket socket)
        {
            this.socket = socket;
            base.UserToken = socket;
            base.Completed += Server_Completed;
        }
        private void Server_Completed(object sender, SocketAsyncEventArgs e)
        {
            var client = new Client(e.AcceptSocket);
            e.AcceptSocket = null;
            this.socket.AcceptAsync(e);
        }
    }

    class Program : Socket
    {
        public Program() : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            base.Bind(new IPEndPoint(IPAddress.Any, 8000));
            base.Listen(20);
            base.AcceptAsync(new Server(this));
        }
    }

    class CsServerAsync
    {
        static void Main(string[] args)
        {
            new Program();

            while (true)
            {

            }
        }
    }
}
