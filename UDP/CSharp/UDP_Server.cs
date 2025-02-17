using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDP_Server
{
    internal class UDP_Server
    {
        const int PortNumber = 8000;

        static void Main(string[] args)
        {
            try
            {
                Socket ServerSocket = new Socket(
                    AddressFamily.InterNetwork, // Ipv4
                    SocketType.Dgram, // 데이터그램 : 패킷 교환 네트워크와 관련된 기본 전송 단위
                    ProtocolType.Udp // UDP 방식
                    );

                EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PortNumber); // IP종단점
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.None, PortNumber); // 원격 IP종단점
                
                ServerSocket.Bind(localEndPoint); // 소켓을 IP종단점과 연결

                byte[] receiveBuffer = new byte[1024]; // 데이터를 받아올 Buffer 선언

                try
                {
                    Console.WriteLine("Server Start!");

                    while (true)
                    {
                        // 바이트 수
                        int receivedSize = ServerSocket.ReceiveFrom(
                            receiveBuffer, // 송신 데이터 저장소
                            ref remoteEndPoint); // 송신 받을 종단점

                        Console.WriteLine($"Message : {Encoding.UTF8.GetString(receiveBuffer, 0, receivedSize)}");
                        ServerSocket.SendTo(receiveBuffer, receivedSize, SocketFlags.None, remoteEndPoint); // 송신데이터를 다시 보냄
                    }
                }
                catch (SocketException se)
                {
                    Console.WriteLine($"ERROR : {se.Message}");
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine($"ERROR : {se.Message}");
            }
        }
    }
}
